<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Search.aspx.cs" Inherits="LogSearchTool.Search"
    ValidateRequest="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Log Search Portal</title>
    <link href="Resources/Css/styles.css" rel="stylesheet" type="text/css" />
    <script src="Resources/Scripts/jquery-1.6.2.js" type="text/javascript"></script>
    <script src="Resources/Scripts/jquery-1.6.2.min.js" type="text/javascript"></script>
    <script src="Resources/Scripts/scripts.js" type="text/javascript"></script>
</head>
<body>
    <form id="form1" runat="server">
    <div class="header">
        <div class="avivaLogo">
        </div>
        <div class="tcsLogo">
        </div>
    </div>
    <asp:ScriptManager runat="server" />
    <h2>
        Log Search Portal</h2>
    <table class="defined">
        <tr class="left fullBlock">
            <td class="largeBlock left">
                <div class="block">
                    <label>
                        Select the log</label>
                    <div class="Container">
                        <asp:DropDownList ID="appName" runat="server">
                        </asp:DropDownList>
                        <div class="error">
                            <asp:RequiredFieldValidator ID="forappName" runat="server" ControlToValidate="appName"
                                CssClass="">Please select a log</asp:RequiredFieldValidator>
                        </div>
                    </div>
                </div>
                <div class="block borderTop">
                    <label>
                        Select the date</label>
                    <div class="Container">
                        <asp:DropDownList ID="logDate" runat="server">
                        </asp:DropDownList>
                        <div class="error">
                            <asp:RequiredFieldValidator ID="fordate" runat="server" ControlToValidate="logDate">Please select a date</asp:RequiredFieldValidator>
                        </div>
                    </div>
                </div>
                <div class="block borderTop">
                    <label>
                        Type a keyword to search</label>
                    <div class="Container">
                        <asp:TextBox ID="searchText" runat="server" class="text" />
                        <div class="error">
                            <asp:RequiredFieldValidator ID="forKeyword" runat="server" ControlToValidate="searchText">Please specify a keyword</asp:RequiredFieldValidator>
                        </div>
                    </div>
                </div>
                <div id="clearMessage" runat="server" class="block borderTop" visible="false">
                    <div class="error">
                        The local archive has been cleared now, you can try your search again!
                    </div>
                </div>
                <div class="endBlock">
                    <asp:Button ID="search" runat="server" CausesValidation="true" CssClass="mediumButton"
                        CommandName="Search" Text="Search" />
                    <input type="button" id="reset" runat="server" class="mediumButton" value="Reset"
                        causesvalidation="false" />
                    <asp:Button ID="clearArchive" runat="server" CssClass="mediumButton" CommandName="ClearArchive"
                        Text="Clear" CausesValidation="false" />
                </div>
            </td>
            <td class="mediumBlock right" style="vertical-align: top;">
                <div id="indServerSection" runat="server">
                    <div class="block borderBottom" style="width: 440px">
                        <label>
                            Do you want to search from a specific server?</label>
                        <div class="smallContainer">
                            <span class="radio">
                                <label for="specificServer_0">
                                    No</label>
                                <input id="specificServer_0" runat="server" type="radio" checked="true" class="left"
                                    name="specificServer" />
                                <label for="specificServer_1">
                                    Yes</label>
                                <input id="specificServer_1" runat="server" type="radio" name="specificServer" />
                            </span>
                        </div>
                    </div>
                    <div class="block borderBottom conditional" id="individualServer" runat="server"
                        style="width: 440px">
                        <label>
                            Select the server</label>
                        <div class="smallContainer">
                            <asp:DropDownList ID="indServers" runat="server">
                            </asp:DropDownList>
                            <div class="error">
                                <asp:CustomValidator ID="forindServer" runat="server" ControlToValidate="indServers">Please specify the server</asp:CustomValidator>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="block" style="width: 440px">
                    <label>
                        Specify the minimum file size for 'bisect and search'</label>
                    <div class="smallContainer">
                        <asp:CheckBoxList ID="breakDownSize" runat="server" RepeatDirection="Vertical" RepeatLayout="Flow"
                            CssClass="longRepeat">
                        </asp:CheckBoxList>
                    </div>
                </div>
            </td>
        </tr>
        <tr id="matchedFilesSection" class="left fullBlock conditional" runat="server">
            <td>
                <asp:GridView ID="matchedFilesView" runat="server" AutoGenerateColumns="False" DataKeyNames="File"
                    RowStyle-Font-Size="90%" AlternatingRowStyle-BackColor="#AAAEFF" BackColor="#ffffff">
                    <Columns>
                        <asp:TemplateField HeaderText="Matched Files">
                            <ItemTemplate>
                                <asp:LinkButton ID="fetchMatchedDetails" runat="server" Text='<%# Bind("File") %>'
                                    CommandName="Select" CausesValidation="False"></asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </td>
        </tr>
    </table>
    <div>
        <h3>
            Matched logs
            <label id="selectedFile" runat="server">
            </label>
        </h3>
        <div>
            <asp:TextBox TextMode="MultiLine" ID="matchedContent" runat="server" Width="1000px"
                Height="450px" />
        </div>
    </div>
    </form>
</body>
</html>
