<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AssessmentList.ascx.cs" Inherits="Rockweb.Blocks.Crm.AssessmentList" ViewStateMode="Enabled" EnableViewState="true" %>

<asp:Panel ID="pnlAssessments" runat="server">
    <asp:Literal ID="lAssessments" runat="server"></asp:Literal>
<Rock:NotificationBox ID="nbAssessmentWarning" runat="server" Title="Warning" Text="This is a warning." NotificationBoxType="Warning" />
</asp:Panel>
