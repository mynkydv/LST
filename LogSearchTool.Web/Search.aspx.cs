// -----------------------------------------------------------------------
// <copyright file="Search.aspx.cs" company="Aviva">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace LogSearchTool
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Xml;
    using LogSearchTool.Utilities;

    /// <summary>
    /// Handles the page level asp.net events for the page Search.aspx
    /// </summary>
    public partial class Search : Page
    {
        #region Private fields

        /// <summary>
        /// List to store the server names
        /// </summary>
        private List<string> servers = new List<string>();

        /// <summary>
        /// The location/path to copy the log files
        /// </summary>
        private string copyTo = Path.Combine(HttpContext.Current.Server.MapPath("~"));

        /// <summary>
        /// The log selected to look up the specified keyword
        /// </summary>
        private string logSelected = string.Empty;

        /// <summary>
        /// The date selected to fetch the logs
        /// </summary>
        private string dateSelected = string.Empty;

        /// <summary>
        /// The server selected on which logs needs to be looked for
        /// </summary>
        private string serverSelected = string.Empty;

        private XmlDocument configXml;

        #endregion

        #region Constrcutors

        /// <summary>
        /// Instantiates an instance of Search class
        /// </summary>
        public Search()
        {
            // Load the config document to for performing the search operations
            this.configXml = new XmlDocument();
            this.configXml.Load(this.copyTo + @"\Config.xml");
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Handles the Page load ASP.NET event
        /// </summary>
        /// <param name="sender">sender as object</param>
        /// <param name="e">event arguments</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserName"] != null && Session["Password"] != null)
            {
                if (!Page.IsPostBack)
                {
                    this.PopulateDefaultValues();
                }

                // Assign the event handlers
                this.search.Click += new EventHandler(this.Search_Click);
                this.clearArchive.Click += new EventHandler(this.ClearArchive_Click);
                this.matchedFilesView.SelectedIndexChanged += new EventHandler(this.MatchedFilesView_SelectedIndexChanged);
                this.appName.AutoPostBack = true;
                this.appName.SelectedIndexChanged += new EventHandler(this.AppName_SelectedIndexChanged);
                this.forindServer.ServerValidate += new ServerValidateEventHandler(this.IndividualServer_ServerValidate);
            }
            else
            {
                Response.Redirect("Login.aspx");
            }
        }

        /// <summary>
        /// Handles the search button click event
        /// </summary>
        /// <param name="sender">sender as object</param>
        /// <param name="e">event arguments</param>
        protected void Search_Click(object sender, EventArgs e)
        {
            // Check whether the user input values are valid, if not return
            if (Page.IsValid && this.IsIndServerValid())
            {
                // Get the user selected values
                this.logSelected = this.GetLogFileName(this.appName.SelectedValue);
                this.dateSelected = Convert.ToDateTime(logDate.SelectedValue).Date.ToString("yyyyMMdd");
                this.serverSelected = this.specificServer_1.Checked ? this.indServers.SelectedValue : string.Empty;

                // If user has specified a specific server, then include only that server in the search list
                if (string.IsNullOrEmpty(this.serverSelected))
                {
                    string serverId = this.GetServerId(this.appName.SelectedValue);

                    string serversNames = this.configXml.SelectSingleNode("//Servers/" + serverId).InnerXml;
                    foreach (string server in serversNames.Split(';'))
                    {
                        this.servers.Add(server.Trim().ToUpper());
                    }
                }
                else
                {
                    this.servers.Add(this.serverSelected.Trim().ToUpper());
                }

                // Reset the content, before a fresh search
                matchedContent.Text = string.Empty;

                // Get the list of files for the specified criteria and the keyword
                this.FetchFiles();
            }
        }

        /// <summary>
        /// Handles the clear archive button click event
        /// </summary>
        /// <param name="sender">sender as object</param>
        /// <param name="e">event arguments</param>
        protected void ClearArchive_Click(object sender, EventArgs e)
        {
            DirectoryInfo diInfo = new DirectoryInfo(this.copyTo + @"\Temp");

            if (diInfo.GetFiles().ToList().Count > 0)
            {
                diInfo.GetFiles().ToList().ForEach(fileInfo => fileInfo.Delete());
            }

            this.clearMessage.Visible = true;
        }

        /// <summary>
        /// Handles the grid view selected index change event
        /// </summary>
        /// <param name="sender">sender as object</param>
        /// <param name="e">event arguments</param>
        protected void MatchedFilesView_SelectedIndexChanged(object sender, EventArgs e)
        {
            string fileSelected = this.matchedFilesView.SelectedDataKey.Value.ToString();

            // Reset the content, before a fresh search
            matchedContent.Text = string.Empty;

            // Fetch the content from the selected file where the match is found
            this.MatchAndFetchContent(fileSelected);
        }

        /// <summary>
        /// handles the change event of the log dropdown for populating the servers applicable
        /// </summary>
        /// <param name="sender">sender as object</param>
        /// <param name="e">event arguments</param>
        protected void AppName_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedValue = this.appName.SelectedValue;
            if (!string.IsNullOrEmpty(selectedValue))
            {
                this.PopulateServers(selectedValue);
                this.indServerSection.Visible = true;
            }
            else
            {
                this.indServerSection.Visible = false;
            }
        }

        /// <summary>
        /// Handles the validation for the individual server selection
        /// </summary>
        /// <param name="sender">sender as object</param>
        /// <param name="e">event arguments</param>
        protected void IndividualServer_ServerValidate(object sender, EventArgs e)
        {
            this.IsIndServerValid();
        }

        #endregion

        #region Private functions / methods

        /// <summary>
        /// Get the actual log file name from the selected log file name
        /// </summary>
        /// <param name="selectedValue">The selected log file name</param>
        /// <returns>The actual log file name</returns>
        private string GetLogFileName(string selectedValue)
        {
            // Config file has a delimiter [:] to specify whether any other application has the same log file name
            // The selected log file belongs to one and only application, then return the selected log file name
            if (selectedValue.IndexOf(":") < 0)
            {
                return selectedValue;
            }

            XmlNode application = configXml.SelectSingleNode("//Application/Value[text()='" + selectedValue + "']").ParentNode;

            if (!Object.Equals(application, null))
            {
                return application.SelectSingleNode("LogName").InnerXml;
            }

            return selectedValue;
        }

        /// <summary>
        /// Get the server id which hosts the selected application
        /// </summary>
        /// <param name="selectedValue">The selected log file name</param>
        /// <returns>The server id of the application</returns>
        private string GetServerId(string selectedValue)
        {
            // Get the application details for the selected log file name
            XmlNode application = configXml.SelectSingleNode("//Application/Value[text()='" + selectedValue + "']").ParentNode;

            if (!Object.Equals(application, null))
            {
                return application.SelectSingleNode("ServerId").InnerXml;
            }

            return selectedValue;
        }

        /// <summary>
        /// Checks whether the inddividual server dropdown needs to be validated
        /// </summary>
        /// <returns>a booleamn indicating whether a selection of individual server has been made or not</returns>
        private bool IsIndServerValid()
        {
            if (this.specificServer_1.Checked)
            {
                this.forindServer.Enabled = true;
                if (this.indServers.SelectedIndex <= 0)
                {
                    this.forindServer.IsValid = false;
                    return false;
                }
                else
                {
                    this.forindServer.IsValid = true;
                    return true;
                }
            }
            else
            {
                this.forindServer.Enabled = false;
                return true;
            }
        }

        /// <summary>
        /// To populate the required control lists
        /// </summary>
        private void PopulateDefaultValues()
        {
            // Prepare date dropdown list
            IDictionary<string, string> dateTobeSelected = new Dictionary<string, string>();
            dateTobeSelected.Add(new KeyValuePair<string, string>(string.Empty, "Please select..."));

            int limitOnDays = Convert.ToInt32(ConfigurationManager.AppSettings["DaysLimit"]);
            int dateInterval = 0;

            do
            {
                string date = DateTime.Today.AddDays(-1 * dateInterval).ToShortDateString();
                dateTobeSelected.Add(new KeyValuePair<string, string>(date, date));

                dateInterval++;
            }
            while (dateInterval <= limitOnDays);

            // Bind date dropdown
            this.logDate.DataSource = dateTobeSelected;
            this.logDate.DataValueField = "Key";
            this.logDate.DataTextField = "Value";
            this.logDate.DataBind();

            // Populate Log dropdown list
            DataSet dataSet = new DataSet();
            dataSet.ReadXml(this.copyTo + @"\Config.xml");

            ////string selectedValue = this.Request.QueryString["log"];
            ////if (!string.IsNullOrEmpty(selectedValue))
            ////{
            ////    this.PopulateServers(selectedValue);
            ////}

            // Bind log dropdown
            this.appName.DataSource = dataSet;
            this.appName.DataTextField = "Name";
            this.appName.DataValueField = "Value";
            this.appName.DataBind();
            this.appName.Items.Insert(0, new ListItem("Please select...", string.Empty));

            this.breakDownSize.Items.Clear();
            string[] fileSizes = ConfigurationManager.AppSettings["AllowedMinFileSizesinKB"].Split(';');

            foreach (string fileSize in fileSizes)
            {
                this.breakDownSize.Items.Add(new ListItem(fileSize + " KB", fileSize));
            }
        }

        /// <summary>
        /// Populate the individual servers for the selected log file
        /// </summary>
        /// <param name="selectedValue">The selected log file</param>
        private void PopulateServers(string selectedValue)
        {
            // Fetch the server details for the selected application
            string serverId = this.GetServerId(selectedValue);

            string serversNames = configXml.SelectSingleNode("//Servers/" + serverId).InnerXml;
            IList<string> serverList = new List<string>();

            foreach (string server in serversNames.Split(';'))
            {
                serverList.Add(server.Trim().ToUpper());
            }

            // Bind the list of servers as per the log selected by the user
            if (serverList.Count > 0)
            {
                this.indServers.Items.Clear();
                this.indServers.Items.Add(new ListItem("Please select...", string.Empty));

                foreach (string server in serverList)
                {
                    this.indServers.Items.Add(new ListItem(server, server));
                }
            }
        }

        /// <summary>
        /// Gets the files which matches the specified criteria and binds the list to a grid view
        /// </summary>
        private void FetchFiles()
        {
            FileOperations fileOps = this.CreateFileUtilityInstance();

            // Get the list of files
            DataTable matchFoundIn = fileOps.FetchFilesForTheSpecifiedCriteria();

            // If no files found, then hide the grid view section
            if (matchFoundIn.Rows.Count > 0)
            {
                this.matchedFilesView.DataSource = matchFoundIn;
                this.matchedFilesView.DataBind();
                this.matchedFilesSection.Visible = true;
            }
            else
            {
                this.matchedFilesSection.Visible = false;
                this.matchedContent.Text = @"Specified keyword not found in any of the logs collected for the specified date! Please review your search criteria (keyword, date, logs etc.)" + Environment.NewLine + @"OR" + Environment.NewLine + @"Clear the local archive by clicking the 'Clear' button in the above section and try again.";
                this.matchedContent.CssClass = "error";
            }
        }

        /// <summary>
        /// Fetch the content from the selected file and populate the matched chunks into the text area
        /// </summary>
        /// <param name="fileName">the name of the selected file</param>
        private void MatchAndFetchContent(string fileName)
        {
            FileOperations fileOps = this.CreateFileUtilityInstance();

            string matchString = this.searchText.Text.Trim().ToUpper();
            int selectedFileSize = 0;
            int.TryParse(this.breakDownSize.SelectedValue.Trim(), out selectedFileSize);

            // Get the full info of the selected file
            FileInfo fileInfo = new FileInfo(fileName);

            // Optimize the search by breaking larger files into smaller chunks
            int chunkSize = selectedFileSize != 0 ? selectedFileSize * 1024 : 1024 * 1024 * 15;
            decimal chunkCount = fileOps.OptmizeSearch(fileInfo, chunkSize);

            // boolean indicating whether the transaction/error log
            // containing the specified keyword is distributed in two files
            bool isSplit = false;

            // Run through every smaller file created as per the optimization setting
            for (int indChunk = 1; indChunk <= chunkCount; indChunk++)
            {
                StringBuilder tempBuilder = new StringBuilder();
                StringBuilder resultBuilder = new StringBuilder();
                StreamReader indReader = null;

                List<int> startPositions = new List<int>();
                List<int> endPositions = new List<int>();

                FileInfo indFileInfo = null;

                // If the file was not broken down into smaller chunks, then proceed with the actual file
                // otherwise, get the full file info of the smaller file
                if (chunkCount == 1)
                {
                    indFileInfo = new FileInfo(fileInfo.FullName);
                }
                else
                {
                    indFileInfo = new FileInfo(fileInfo.FullName.Replace(".txt", "_" + indChunk + ".txt"));
                }

                try
                {
                    indReader = new StreamReader(indFileInfo.Open(FileMode.Open));

                    do
                    {
                        string singleLine = indReader.ReadLine();
                        tempBuilder.Append(singleLine + "\r\n");

                        int startIndex, endIndex;

                        // Check each line whether it contains the specified string/keyword
                        if (singleLine.ToUpper().Contains(matchString))
                        {
                            // If the specified string/keyword is found the find the beginning of the transaction/log
                            startIndex = tempBuilder.ToString().LastIndexOf("-----Begin", StringComparison.InvariantCultureIgnoreCase);
                            startIndex = startIndex < 0 ? 0 : startIndex;

                            if (!startPositions.Any(index => index == startIndex))
                            {
                                startPositions.Add(startIndex);
                            }
                        }
                        else if (singleLine.ToUpper().Contains("-----END"))
                        {
                            // Find the end of the transaction/log
                            endIndex = tempBuilder.ToString().LastIndexOf("-----End", StringComparison.InvariantCultureIgnoreCase);
                            endPositions.Add(endIndex);
                        }
                        else if (singleLine.ToUpper().Contains("----- END"))
                        {
                            // Find the end of the transaction/log
                            endIndex = tempBuilder.ToString().LastIndexOf("----- End", StringComparison.InvariantCultureIgnoreCase);
                            endPositions.Add(endIndex);
                        }
                    }
                    while (!indReader.EndOfStream);

                    // If the transaction/log for the specified string/keyword was distributed with first half in the previous file
                    // Then read the content from the beginning of the file till the first occurrence of the end of the transaction
                    if (isSplit)
                    {
                        resultBuilder.Append(tempBuilder.ToString().Substring(0, endPositions.First()) + "\r\n");
                        resultBuilder.Append("************************************************  END  ************************************************" + "\r\n");

                        isSplit = false;
                    }

                    // Run through the start indices of the beginning tags, 
                    // found after running the searh for the specified string/keyword
                    foreach (int startPos in startPositions)
                    {
                        // Boolean to indicate whether a matching end tag is available for the given begin tag
                        bool endPosPresent = false;
                        foreach (int endPos in endPositions)
                        {
                            int length = endPos - startPos;
                            if (length > 0)
                            {
                                endPosPresent = true;

                                resultBuilder.Append("***********************************************  START  ***********************************************" + "\r\n");
                                resultBuilder.Append(tempBuilder.ToString().Substring(startPos, length) + "\r\n");
                                resultBuilder.Append("************************************************  END  ************************************************" + "\r\n");
                                break;
                            }
                        }

                        // If there are is no matching end tag present for the given begin tag,
                        // then copy only the part of the transaction/log from the beginning of the begin tag
                        // Set the indicator that the transaction/log is split in two files, 
                        // and when the next file is picked up the remaining part of the transaction/log is copied
                        if (!endPosPresent)
                        {
                            isSplit = true;

                            resultBuilder.Append("***********************************************  START  ***********************************************" + "\r\n");
                            resultBuilder.Append(tempBuilder.ToString().Substring(startPos) + "\r\n");
                        }
                    }

                    // Keep appending the transaction/log fetched from each of the files to the text area
                    this.matchedContent.Text += resultBuilder.ToString();
                }
                catch (Exception ex)
                {
                    fileOps.WriteToFile(ConfigurationManager.AppSettings["LSTLogFile"], ex.Message + string.Empty + ex.InnerException);
                }
                finally
                {
                    if (indReader != null)
                    {
                        indReader.Close();
                    }
                }
            }

            // Check if there has been any match found at all
            if (string.IsNullOrEmpty(this.matchedContent.Text))
            {
                this.matchedContent.Text = "Specified keyword not found in the selected file, Please check the keyword or try searching in a different file!";
                this.matchedContent.CssClass = "error";
            }
            else
            {
                this.matchedContent.CssClass = string.Empty;
                this.selectedFile.InnerText = string.Format("-- File selected : {0}", fileInfo.Name);
            }
        }

        /// <summary>
        /// Creates an instance of the FileOperations class
        /// </summary>
        /// <returns>an instance of FileOperations class</returns>
        private FileOperations CreateFileUtilityInstance()
        {
            // Prepare the set of attributes/data required for the file operations
            FileSpecifications fileSpecs = new FileSpecifications()
            {
                DateExtension = this.dateSelected,
                File = this.logSelected,
                ToMatch = searchText.Text.Trim(),
                CopyTo = this.copyTo + @"\Temp",
                Servers = this.servers,
                Date = Convert.ToDateTime(logDate.SelectedValue).Date
            };

            return new FileOperations(fileSpecs);
        }

        #endregion
    }
}