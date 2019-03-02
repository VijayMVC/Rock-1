<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AssessmentList.ascx.cs" Inherits="Rockweb.Blocks.Crm.AssessmentList" ViewStateMode="Enabled" EnableViewState="true" %>


<asp:Panel ID="pnlInstructions" runat="server">
    <asp:Literal ID="lInstructions" runat="server"></asp:Literal>

    <div class="actions">
        <asp:LinkButton ID="btnStart" runat="server" CssClass="btn btn-primary pull-right">Start <i class="fa fa-chevron-right"></i></asp:LinkButton>
    </div>
</asp:Panel>
