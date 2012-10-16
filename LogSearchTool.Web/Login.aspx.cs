// -----------------------------------------------------------------------
// <copyright file="Login.aspx.cs" company="Aviva">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace LogSearchTool.Web
{
    using System;
    using System.Web.UI;

    /// <summary>
    /// Handles the page level asp.net events for the page Login.aspx
    /// </summary>
    public partial class Login : Page
    {
        /// <summary>
        /// Handles the asp.net page load event
        /// </summary>
        /// <param name="sender">sender as object</param>
        /// <param name="e">event arguments</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            // Assign the event handlers
            this.loginButton.Click += new EventHandler(this.LoginButton_Click);
        }

        /// <summary>
        /// Handles the login button click event
        /// </summary>
        /// <param name="sender">sender as object</param>
        /// <param name="e">event arguments</param>
        private void LoginButton_Click(object sender, EventArgs e)
        {
            // Clear all sessions
            Session.RemoveAll();

            // Add the domain name, user name and password input by the user in the session
            Session["Domain"] = userName.Text.Split('\\')[0];
            Session["UserName"] = userName.Text.Split('\\')[1];
            Session["Password"] = password.Text;

            // If the user provided credentials is a valid pair, then allow the user to proceed to the search screen
            //if (Authenticate.Impersonate(Session["Domain"].ToString(), Session["UserName"].ToString(), Session["Password"].ToString()))
            //{
            //Authenticate.UndoImpersonation();
            Response.Redirect("Search.aspx");
            //}
            //else
            //{
            //    this.loginFailed.Style.Add("display", "block");
            //}
        }
    }
}