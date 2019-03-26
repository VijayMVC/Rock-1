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

                        <h2>Migrate FinancialPersonSavedAccount from NMI to Pi</h2>
                        <Rock:FileUploader ID="fuCustomerVaultImportFile" runat="server" Label="Select Customer Vault Import File" IsBinaryFile="true" UploadAsTemporary="true"  DisplayMode="DropZone" OnFileUploaded="fuCustomerVaultImportFile_FileUploaded" />
                        <asp:LinkButton ID="btnImportCustomerVault" runat="server" CssClass="btn btn-primary" OnClick="btnImportCustomerVault_Click" Enabled="false">btnImportCustomerVault</asp:LinkButton>
                        <Rock:NotificationBox ID="nbImportCustomerVaultResult" runat="server" NotificationBoxType="Success" />

                        <Rock:CodeEditor ID="ceImportCustomerVaultResults" runat="server" EditorMode="JavaScript" Label="Import CustomerVault Results" />

                        <h2>#TODO# Migration FinancialScheduledTransaction from NMI to Pi#</h2>

                        <Rock:FileUploader ID="fuScheduleImportFile" runat="server" Label="Select Schedule Import File" IsBinaryFile="true" UploadAsTemporary="true"  DisplayMode="DropZone" OnFileUploaded="fuScheduleImportFile_FileUploaded" />
                        <asp:LinkButton ID="btnImportScheduleImport" runat="server" CssClass="btn btn-primary" OnClick="btnImportScheduleImport_Click" Enabled="false">btnImportScheduleImport</asp:LinkButton>
                        <Rock:NotificationBox ID="nbImportScheduleImportResult" runat="server" NotificationBoxType="Success" />

                        <Rock:CodeEditor ID="ceImportScheduleImportResults" runat="server" EditorMode="JavaScript" Label="Import Schedule Import Results" />
                    </div>
                </div>
                
                
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
