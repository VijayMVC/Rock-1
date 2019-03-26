// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using CsvHelper;
using Rock;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Financial Gateway Migration Utility
    /// </summary>
    [DisplayName( "Financial Gateway Migration Utility" )]
    [Category( "Utility" )]
    [Description( "Tool to assist in migrating records to a Pi Gateway from a data export from an NMI gateway. This is limited to gateways that support data migrations." )]

    #region Block Attributes
    #endregion Block Attributes
    public partial class GatewayMigrationUtility : RockBlock
    {

        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        protected static class AttributeKey
        {
        }

        #endregion Attribute Keys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );


            if ( !Page.IsPostBack )
            {
                ShowDetails();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the FileUploaded event of the fuCustomerVaultImportFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.FileUploaderEventArgs"/> instance containing the event data.</param>
        protected void fuCustomerVaultImportFile_FileUploaded( object sender, Rock.Web.UI.Controls.FileUploaderEventArgs e )
        {
            btnImport.Enabled = true;
            
        }

        private class ImportRecord
        {
            public string NMICustomerId { get; set; }
            public string PiCustomerId { get; set; }
        }

        /// <summary>
        /// Handles the Click event of the btnImport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnImport_Click( object sender, EventArgs e )
        {
            BinaryFile binaryFile = null;
            var rockContext = new RockContext();
            var binaryFileService = new BinaryFileService( rockContext );
            var binaryFileId = fuCustomerVaultImportFile.BinaryFileId;
            if ( binaryFileId.HasValue )
            {
                binaryFile = binaryFileService.Get( binaryFileId.Value );
            }

            Dictionary<string, string> nmiToPiCustomerIdLookup = null;

            if ( binaryFile != null )
            {
                var importData = binaryFile.ContentsToString();

                StringReader stringReader = new StringReader( importData );
                CsvReader csvReader = new CsvReader( stringReader );
                csvReader.Configuration.HasHeaderRecord = false;

                nmiToPiCustomerIdLookup = csvReader.GetRecords<ImportRecord>().ToDictionary( k => k.NMICustomerId, v=> v.PiCustomerId );

                // TODO
                //binaryFileService.Delete( binaryFile );
            }
            else
            {
                // TODO
            }

            var financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );
            var financialGatewayService = new FinancialGatewayService( rockContext );
            var nmiGatewayID = ddlNMIGateway.SelectedValue.AsInteger();
            var nmiGateway = financialGatewayService.Get( nmiGatewayID );
            var nmiGatewayComponent = nmiGateway.GetGatewayComponent();
            var piGatewayId = ddlPiGateway.SelectedValue.AsInteger();
            var piGateway = financialGatewayService.Get( piGatewayId );
            var piGatewayComponent = piGateway.GetGatewayComponent() as IHostedGatewayComponent;
            var earliestPiStartDate = piGatewayComponent.GetEarliestScheduledStartDate( piGateway );
            var oneTimeFrequencyId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME.AsGuid() );
            string errorMessage;

            var scheduledTransactions = financialScheduledTransactionService.Queryable().Where( a => a.FinancialGatewayId == nmiGatewayID ).ToList();
            foreach ( var scheduledTransaction in scheduledTransactions.Where( a => a.IsActive ) )
            {
                // get the latest status to get an updated NextStartDate
                financialScheduledTransactionService.GetStatus( scheduledTransaction, out errorMessage );


                var nmiSubscriptionId = scheduledTransaction.GatewayScheduleId;
                var nmiCustomerId = scheduledTransaction.ForeignKey;

                // if there is a NextPaymentDate (onetime or canceled schedules might not have a NextPaymentDate), create a subscription in the Pi System, then cancel the one on the NMI system
                if ( scheduledTransaction.NextPaymentDate.HasValue )
                {
                    PaymentSchedule paymentSchedule = new PaymentSchedule
                    {
                        TransactionFrequencyValue = DefinedValueCache.Get( scheduledTransaction.TransactionFrequencyValueId ),
                        StartDate = scheduledTransaction.NextPaymentDate.Value,
                        PersonId = scheduledTransaction.AuthorizedPersonAlias.PersonId
                    };

                    ReferencePaymentInfo referencePaymentInfo = new ReferencePaymentInfo
                    {
                        // TODO: Get mapped Pi customer_id  from NMI customer_id
                        GatewayPersonIdentifier = nmiToPiCustomerIdLookup[scheduledTransaction.Ref]
                    };

                    var tempFinancialScheduledTransaction = piGatewayComponent.AddScheduledPayment( piGateway, paymentSchedule, referencePaymentInfo, out errorMessage );
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the details.
        /// </summary>
        protected void ShowDetails()
        {
            var rockContext = new RockContext();
            var financialGatewayService = new FinancialGatewayService( rockContext );
            var activeGatewayList = financialGatewayService.Queryable().Where( a => a.IsActive == true ).AsNoTracking().ToList();
            var piGateways = activeGatewayList.Where( a => a.GetGatewayComponent() is Rock.TransNational.Pi.PiGateway ).ToList();
            ddlPiGateway.Items.Clear();
            foreach ( var piGateway in piGateways )
            {
                ddlPiGateway.Items.Add( new ListItem( piGateway.Name, piGateway.Id.ToString() ) );
            }

            var nmiGateways = activeGatewayList.Where( a => a.GetGatewayComponent() is Rock.NMI.Gateway ).ToList();
            ddlNMIGateway.Items.Clear();
            foreach ( var nmiGateway in nmiGateways )
            {
                ddlNMIGateway.Items.Add( new ListItem( nmiGateway.Name, nmiGateway.Id.ToString() ) );
            }
        }

        #endregion
        
    }
}