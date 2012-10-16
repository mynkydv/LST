// -----------------------------------------------------------------------
// <copyright file="Authenticate.cs" company="Aviva">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace LogSearchTool.Web
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Principal;

    /// <summary>
    /// Authenticates the user with the login credentials
    /// </summary>
    public static class Authenticate
    {
        /// <summary>
        /// Not sure what it does
        /// </summary>
        /// <param name="lpszUsername">The login user name</param>
        /// <param name="lpszDomain">The domain name</param>
        /// <param name="lpszPassword">The login password</param>
        /// <param name="dwLogonType">The logon type</param>
        /// <param name="dwLogonProvider">The logon provider</param>
        /// <param name="phToken">The token</param>
        /// <returns>a boolean</returns>
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LogonUser(
        string lpszUsername,
        string lpszDomain,
        string lpszPassword,
        int dwLogonType,
        int dwLogonProvider,
        ref IntPtr phToken);

        /// <summary>
        /// Not sure what it does
        /// </summary>
        /// <param name="handle">Not sure what it is</param>
        /// <returns>a boolean</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool CloseHandle(IntPtr handle);

        /// <summary>
        /// Not sure what it is
        /// </summary>
        private static IntPtr tokenHandle = new IntPtr(0);

        /// <summary>
        /// Windows impersonated context
        /// </summary>
        private static WindowsImpersonationContext impersonatedUser;

        /// <summary>
        /// Create impersonation
        /// If you incorporate this code into a DLL, be sure to demand that it runs with FullTrust.
        /// </summary>
        /// <param name="domainName">The domain name</param>
        /// <param name="userName">The login user name</param>
        /// <param name="password">The login password</param>
        /// <returns>A boolean indicating whether the user is an authenticated user or not</returns>
        public static bool Impersonate(string domainName, string userName, string password)
        {
            // Use the unmanaged LogonUser function to get the user token for
            // the specified user, domain, and password.
            const int LOGON32_PROVIDER_DEFAULT = 0;

            // Passing this parameter causes LogonUser to create a primary token.
            const int LOGON32_LOGON_INTERACTIVE = 2;
            tokenHandle = IntPtr.Zero;

            // ---- Step - 1
            // Call LogonUser to obtain a handle to an access token.
            bool returnValue = LogonUser(
            userName,
            domainName,
            password,
            LOGON32_LOGON_INTERACTIVE,
            LOGON32_PROVIDER_DEFAULT,
            ref tokenHandle); // tokenHandle - new security token

            if (returnValue)
            {
                // ---- Step - 2
                WindowsIdentity newId = new WindowsIdentity(tokenHandle);

                // ---- Step - 3
                impersonatedUser = newId.Impersonate();

                if (impersonatedUser != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Stop impersonation
        /// </summary>
        public static void UndoImpersonation()
        {
            impersonatedUser.Undo();

            // Free the tokens.
            if (tokenHandle != IntPtr.Zero)
            {
                CloseHandle(tokenHandle);
            }
        }
    }
}