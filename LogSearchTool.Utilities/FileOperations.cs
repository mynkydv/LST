// -----------------------------------------------------------------------
// <copyright file="FileOperations.cs" company="Aviva">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace LogSearchTool.Utilities
{
    using System;
    using System.Configuration;
    using System.Data;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Web;

    /// <summary>
    /// The operations / manipulations of the file(s) obtained based on the the specifications provided would be handled in here
    /// </summary>
    public class FileOperations
    {
        #region private fields

        /// <summary>
        /// The specifications pertaining to the search
        /// </summary>
        private FileSpecifications fileSpecs;

        /// <summary>
        /// Datatable to hold the names of the files where match has been found
        /// </summary>
        private DataTable matchedFiles;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="FileOperations"/> class
        /// </summary>
        /// <param name="fileSpecs">The specifications pertaining to a particular search</param>
        public FileOperations(FileSpecifications fileSpecs)
        {
            // The specification must be supplied
            if (fileSpecs == null)
            {
                throw new Exception("File specifications pertaining to the search not specified");
            }

            this.fileSpecs = fileSpecs;

            // Whether today's logs need to be searched?
            if (this.IsToday)
            {
                // Yes, then set the copy from location as the local log file location in the servers
                this.fileSpecs.CopyFrom = ConfigurationManager.AppSettings["LogLocation"];
            }
            else
            {
                // No, then set the copy from location as remote log archive server
                this.fileSpecs.CopyFrom = ConfigurationManager.AppSettings["RemoteServer"];
            }
        }

        #endregion

        #region private properties

        /// <summary>
        /// Gets a value indicating whether the date selected is today's date
        /// </summary>
        private bool IsToday
        {
            get
            {
                return DateTime.Today.Equals(this.fileSpecs.Date);
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Copies and unzips the contents from the archive
        /// Searches for the specified pattern
        /// </summary>
        /// <returns>Collection of full file names, which have been found to have a match</returns>
        public DataTable FetchFilesForTheSpecifiedCriteria()
        {
            if (!this.IsToday)
            {
                // Copy files from remote archive
                this.ImportFiles();
            }
            else
            {
                // Copy files from individual servers
                this.ImportCurrentFiles();
            }

            // Get the list of files which have a match
            this.FetchMatchedFileList();

            return this.matchedFiles;
        }

        /// <summary>
        /// Breaks larger files based on the breakdown size specified to optimize the search
        /// </summary>
        /// <param name="fileInfo">The file information of the provided file</param>
        /// <param name="chunkSize">The minimum size to which larger file needs to be broken down to</param>
        /// <returns>The number of smaller files</returns>
        public decimal OptmizeSearch(FileInfo fileInfo, int chunkSize)
        {
            // Get the number of smaller files based on the total size of the actual file
            decimal chunkCount = this.GetFileBreakdownSize(fileInfo, chunkSize);

            // Split the large file into smaller files
            this.BreakFileIntoSmallerFiles(chunkCount, chunkSize, fileInfo);

            return chunkCount;
        }

        /// <summary>
        /// Writes the given text into the specified file
        /// </summary>
        /// <param name="fileName">The file into which the given text needs to be written</param>
        /// <param name="text">The text which needs to be written</param>
        public void WriteToFile(string fileName, string text)
        {
            Stream writer = null;
            StringBuilder logBuilder = new StringBuilder();

            string sessionId = string.Empty;
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                sessionId = HttpContext.Current.Session.SessionID;
            }

            logBuilder.Append("-------- Begin Log : @ " + DateTime.Now.ToString() + " ---- SessionId : " + sessionId + " --------" + "\r\n");
            logBuilder.Append(text + "\r\n");
            logBuilder.Append("-------- End Log ---------" + "\r\n");

            try
            {
                byte[] bytesToWrite = new byte[1024 * 1024];
                bytesToWrite = Encoding.ASCII.GetBytes(logBuilder.ToString());

                // Write the content into the file
                if (bytesToWrite.Length > 0)
                {
                    writer = new FileStream(fileName, FileMode.Append, FileAccess.Write);
                    writer.Write(bytesToWrite, 0, bytesToWrite.Length);
                }
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
            }
        }

        #endregion

        #region File transport

        /// <summary>
        /// Get the files from the remote archive and unzip the contents
        /// </summary>
        private void ImportFiles()
        {
            string archiveFileExtn = this.fileSpecs.DateExtension + ".txt.gz";

            // Check whether the folder is present in the local drive
            if (!Directory.Exists(this.fileSpecs.CopyTo))
            {
                Directory.CreateDirectory(this.fileSpecs.CopyTo);
            }

            // Run through all the servers obtained for the specified search criteria
            foreach (string name in this.fileSpecs.Servers)
            {
                string remoteArchive = this.fileSpecs.CopyFrom + name + @"\" + this.fileSpecs.File + "_" + archiveFileExtn;
                string localArchive = this.fileSpecs.CopyTo + @"\" + name + "_" + this.fileSpecs.File + archiveFileExtn;

                try
                {
                    // Copy the file only when the file is present in the remote archive and not in the local drive
                    if (!File.Exists(localArchive))
                    {
                        File.Copy(remoteArchive, localArchive);
                    }
                }
                catch (Exception ex)
                {
                    this.WriteToFile(ConfigurationManager.AppSettings["LSTLogFile"], "Error: " + ex.Message + string.Empty + ex.InnerException);
                }
            }

            // Unzip the archive files
            this.DecompressFiles();
        }

        /// <summary>
        /// Get the files from the live servers for the current day
        /// </summary>
        private void ImportCurrentFiles()
        {
            string archiveFileExtn = this.fileSpecs.DateExtension + ".txt";

            // Check whether the folder is present in the local drive
            if (!Directory.Exists(this.fileSpecs.CopyTo))
            {
                Directory.CreateDirectory(this.fileSpecs.CopyTo);
            }

            // Run through all the servers obtained for the specified search criteria
            foreach (string name in this.fileSpecs.Servers)
            {
                string remoteFilePath = @"\\" + name + this.fileSpecs.CopyFrom + this.fileSpecs.File + "_" + archiveFileExtn;
                string localFilePath = this.fileSpecs.CopyTo + @"\" + name + "_" + this.fileSpecs.File + archiveFileExtn;

                try
                {
                    // Copy the file only when the file is present in the remote archive and not in the local drive
                    if (File.Exists(remoteFilePath) && !File.Exists(localFilePath))
                    {
                        File.Copy(remoteFilePath, localFilePath);
                    }
                }
                catch (Exception ex)
                {
                    this.WriteToFile(ConfigurationManager.AppSettings["LSTLogFile"], ex.Message + string.Empty + ex.InnerException);
                }
            }
        }
        #endregion

        #region File read / write

        /// <summary>
        /// Get the list of files, which have a match
        /// </summary>
        private void FetchMatchedFileList()
        {
            this.matchedFiles = new DataTable();
            this.matchedFiles.Columns.Add(new DataColumn() { ColumnName = "File" });

            DirectoryInfo di = new DirectoryInfo(this.fileSpecs.CopyTo);

            // Decompress all zipped/compressed files in the directory.
            foreach (FileInfo fi in di.GetFiles("*" + this.fileSpecs.File + this.fileSpecs.DateExtension + ".txt"))
            {
                string file = fi.Name.ToUpper();

                // Files pertaining to the match criteria alone should be searched
                if (this.fileSpecs.Servers.Any(machine => file.IndexOf(machine.ToUpper()) >= 0))
                {
                    // Check whether the specified pattern/keyword is present in the given file
                    this.FindPatternMatch(fi);
                }
            }
        }

        /// <summary>
        /// Check whether the given file has a match for the specified pattern/keyword
        /// </summary>
        /// <param name="fi">File information of the provided file</param>
        private void FindPatternMatch(FileInfo fi)
        {
            StreamReader reader = new StreamReader(fi.OpenRead());

            try
            {
                // Check line by line whether the match is present
                // If present, add the file name into the datatable
                do
                {
                    string singleLine = reader.ReadLine();

                    if (singleLine.Trim().ToUpper().Contains(this.fileSpecs.ToMatch.Trim().ToUpper()))
                    {
                        object[] rowItems = { fi.FullName };

                        this.matchedFiles.Rows.Add(rowItems);

                        break;
                    }
                }
                while (!reader.EndOfStream);
            }
            catch (Exception ex)
            {
                this.WriteToFile(ConfigurationManager.AppSettings["LSTLogFile"], ex.Message + string.Empty + ex.InnerException);
            }
            finally
            {
                reader.Dispose();
                reader.Close();
            }
        }

        #endregion

        #region File operations

        /// <summary>
        /// Gets the files with the specified pattern for unzipping/decompression
        /// </summary>
        private void DecompressFiles()
        {
            DirectoryInfo di = new DirectoryInfo(this.fileSpecs.CopyTo);

            // Decompress the zip files for the selected log and date
            foreach (FileInfo fi in di.GetFiles("*" + this.fileSpecs.File + this.fileSpecs.DateExtension + ".txt.gz"))
            {
                string file = fi.Name.ToUpper();

                // Files pertaining to the match criteria alone should be decompressed
                if (this.fileSpecs.Servers.Any(machine => file.IndexOf(machine.ToUpper()) >= 0))
                {
                    this.Decompress(fi);
                }
            }
        }

        /// <summary>
        /// Unzips or decompressed the zipped/compressed files
        /// </summary>
        /// <param name="fi">The file information of the specified file</param>
        private void Decompress(FileInfo fi)
        {
            // Get original file extension, for example
            // "doc" from report.doc.gz.
            string curFile = fi.FullName;
            string origName = curFile.Remove(curFile.Length - fi.Extension.Length);

            if (!File.Exists(origName))
            {
                // Get the stream of the source file.
                FileStream inFile = new FileStream(fi.FullName, FileMode.Open);

                // Create the decompressed file.
                FileStream outFile = new FileStream(origName, FileMode.Create);

                try
                {
                    using (GZipStream decomp = new GZipStream(inFile, CompressionMode.Decompress))
                    {
                        // Copy the decompression stream 
                        // into the output file.
                        decomp.CopyTo(outFile);
                    }
                }
                catch (Exception ex)
                {
                    this.WriteToFile(ConfigurationManager.AppSettings["LSTLogFile"], ex.Message + string.Empty + ex.InnerException);
                }
                finally
                {
                    inFile.Close();
                    outFile.Close();
                }
            }
        }

        /// <summary>
        /// Gets the count of smaller files that can be obtained from the large file
        /// </summary>
        /// <param name="fi">The file information of the provided file</param>
        /// <param name="chunkSize">The minimum size of the smaller file</param>
        /// <returns>The number of smaller files</returns>
        private decimal GetFileBreakdownSize(FileInfo fi, int chunkSize)
        {
            long totalSize = fi.Length;
            decimal chunkCount;

            if (totalSize > chunkSize)
            {
                chunkCount = (decimal)totalSize / chunkSize;
                decimal roundedCount = Math.Round(chunkCount, MidpointRounding.AwayFromZero);
                return chunkCount > roundedCount ? roundedCount + 1 : roundedCount;
            }
            else
            {
                return (decimal)1;
            }
        }

        /// <summary>
        /// Split the file into smaller files
        /// </summary>
        /// <param name="chunkCount">The number of smaller files that can be created</param>
        /// <param name="chunkSize">The minimum size of the smaller files</param>
        /// <param name="fileInfo">The file information the large file</param>
        private void BreakFileIntoSmallerFiles(decimal chunkCount, int chunkSize, FileInfo fileInfo)
        {
            // If chunkCount is 1, then there is no need to breakdown into smaller files
            if (chunkCount == 1)
            {
                return;
            }

            StreamReader reader = null;
            StreamWriter writer = null;

            try
            {
                reader = new StreamReader(fileInfo.Open(FileMode.Open));

                for (int indChunk = 1; indChunk <= chunkCount; indChunk++)
                {
                    // Prepare the file name for the smaller file
                    // increments during every iteration
                    string partFileName = fileInfo.FullName.Replace(".txt", "_" + indChunk + ".txt");

                    // Proceed only if the smaller file does not already exist
                    if (!File.Exists(partFileName))
                    {
                        ////// Write the content into the smaller file
                        ////this.Write(partFileName, chunkSize);

                        int chunkBytesRead = 0;
                        writer = new StreamWriter(partFileName, true);

                        // Get the content from the large file until the minimum size
                        do
                        {
                            // Read a line from the source file
                            string readLine = reader.ReadLine();

                            // Write the content into the smaller file
                            writer.Write(readLine + "\r\n");

                            chunkBytesRead += Encoding.ASCII.GetByteCount(readLine);
                        }
                        while (chunkBytesRead < chunkSize && !reader.EndOfStream);

                        writer.Close();
                    }
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                this.WriteToFile(ConfigurationManager.AppSettings["LSTLogFile"], ex.Message + string.Empty + ex.InnerException);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (writer != null)
                {
                    writer.Close();
                }
            }
        }

        /// <summary>
        /// Write content from the provided stream into the specified file
        /// </summary>
        /// <param name="fileName">The name of the file to be created</param>
        /// <param name="bufferSize">The size of the buffer</param>
        private void Write(string fileName, int bufferSize)
        {
            try
            {



            }
            catch (Exception ex)
            {
                this.WriteToFile(ConfigurationManager.AppSettings["LSTLogFile"], ex.Message + string.Empty + ex.InnerException);
            }
            finally
            {

            }
        }

        #endregion
    }
}
