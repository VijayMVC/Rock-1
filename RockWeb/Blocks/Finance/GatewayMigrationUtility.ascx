<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GatewayMigrationUtility.ascx.cs" Inherits="RockWeb.Blocks.Finance.GatewayMigrationUtility" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-credit-card"></i>
                    Gateway Migration Utility
                </h1>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">
                <div class="row">
                    <div class="col-md-12">
                        <Rock:RockDropDownList ID="ddlNMIGateway" Label="NMI Gateway" runat="server" />
                        <Rock:RockDropDownList ID="ddlPiGateway" Label="Pi Gateway" runat="server" />
                        <h2>NMI ScheduledTransactions to PI Scheduled Transactions Test</h2>
                        <Rock:FileUploader ID="fuCustomerVaultImportFile" runat="server" Label="Select Customer Vault Import File" IsBinaryFile="true" UploadAsTemporary="true"  DisplayMode="DropZone" OnFileUploaded="fuCustomerVaultImportFile_FileUploaded" />
                        <asp:LinkButton ID="btnImport" runat="server" CssClass="btn btn-primary" OnClick="btnImport_Click" Enabled="false">btnImport</asp:LinkButton>
                        <Rock:NotificationBox ID="nbImportResult" runat="server" NotificationBoxType="Success" />

                        <Rock:CodeEditor ID="ceImportResults" runat="server" EditorMode="JavaScript" Label="Import Results" />
                    </div>
                </div>
                
                
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
