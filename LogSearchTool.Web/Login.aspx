<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="LogSearchTool.Web.Login" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Log Search Portal</title>
    <link href="Resources/Css/styles.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="login" runat="server">
    <div class="header">
        <div class="avivaLogo">
        </div>
        <div class="tcsLogo">
        </div>
    </div>
    <h2>
        Log Search Portal - Login</h2>
    <div>
        <table class="defined">
            <tr class="left largerBlock">
                <td>
                    <div id="loginFailed" style="display: none;" runat="server">
                        <div class="largerBlock genericError" style="padding:5px 0;">
                            The specified user name and/or password do not seem to be valid/matching
                        </div>
                    </div>
                    <div class="block">
                        <label>
                            User name</label>
                        <div class="Container">
                            <asp:TextBox ID="userName" runat="server" class="medium" />
                            <div class="error">
                                <asp:RequiredFieldValidator ID="foruserName" runat="server" ControlToValidate="userName"
                                    CssClass="">Please specify the user name, Ex. via\RACFID</asp:RequiredFieldValidator>
                            </div>
                        </div>
                    </div>
                    <div class="block">
                        <label>
                            Password</label>
                        <div class="Container">
                            <asp:TextBox ID="password" runat="server" TextMode="Password" class="medium" />
                            <div class="error">
                                <asp:RequiredFieldValidator ID="forPassword" runat="server" ControlToValidate="password"
                                    CssClass="">Please specify the password</asp:RequiredFieldValidator>
                            </div>
                        </div>
                    </div>
                    <div class="endBlock">
                        <asp:Button ID="loginButton" runat="server" CausesValidation="true" CssClass="mediumButton"
                            CommandName="Login" Text="Login" />
                    </div>
                </td>
            </tr>
        </table>
    </div>
    </form>
</body>
</html>
