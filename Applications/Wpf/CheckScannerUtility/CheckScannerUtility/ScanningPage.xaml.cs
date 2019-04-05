﻿// <copyright>
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
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ImageSafeInterop;
using Rock.Wpf.Controls;

namespace Rock.Apps.CheckScannerUtility
{
    /// <summary>
    /// Interaction logic for ScanningPage.xaml
    /// </summary>
    public partial class ScanningPage : System.Windows.Controls.Page
    {
        private RockConfig.InterfaceType _interfaceType;
        private ScannedDocInfo _currentMagtekScannedDoc { get; set; }
        private bool _isBackScan;

        public BatchPage batchPage => ScanningPageUtility.batchPage;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScanningPage"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ScanningPage( BatchPage value )
        {
            InitializeComponent();
            ScanningPageUtility.batchPage = value;

            try
            {
                var config = System.Configuration.ConfigurationManager.OpenExeConfiguration( System.Configuration.ConfigurationUserLevel.None );
                ScanningPageUtility.DebugLogFilePath = config.AppSettings.Settings["DebugLogFilePath"].Value;
                bool isDirectory = !string.IsNullOrWhiteSpace( ScanningPageUtility.DebugLogFilePath ) && Directory.Exists( ScanningPageUtility.DebugLogFilePath );
                if ( isDirectory )
                {
                    ScanningPageUtility.DebugLogFilePath = Path.Combine( ScanningPageUtility.DebugLogFilePath, "CheckScanner.log" );
                }
            }
            catch ( Exception ex )
            {
                System.Diagnostics.Debug.WriteLine( $"ScanningPage: {ex}" );
            }
        }

        /// <summary>
        /// Adds the scanned doc to a history of scanned docs, and shows info and status.
        /// </summary>
        /// <param name="scannedDocInfo">The scanned check info.</param>
        public void ShowScannedDocStatusAndUpload( ScannedDocInfo scannedDocInfo )
        {
            this.HideAlertMessage();
            this.HideUploadSuccess();

            DisplayScannedDocInfo( scannedDocInfo );

            var rockConfig = RockConfig.Load();

            bool scanningChecks = rockConfig.TenderTypeValueGuid.AsGuid() == Rock.Client.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid();
            

            // f they don't enable smart scan, don't warn about bad MICRs, they might be scanning a mixture of checks and envelopes.
            // However, if there was a failure getting an image, show a warning
            if ( ( scannedDocInfo.BadMicr && rockConfig.EnableSmartScan ) || scannedDocInfo.ImageFailure )
            {
                var scannedItemName = scanningChecks ? "check" : "item";
                StringBuilder alertMessageBuilder = new StringBuilder();
                if ( scannedDocInfo.BadMicr && scannedDocInfo.ImageFailure )
                {
                    alertMessageBuilder.AppendLine( $"Unable to read {scannedItemName} information or {scannedItemName} image." );
                }
                else if ( scannedDocInfo.BadMicr )
                {
                    alertMessageBuilder.AppendLine( $"Unable to read {scannedItemName} information." );
                }
                else if ( scannedDocInfo.ImageFailure )
                {
                    alertMessageBuilder.AppendLine( $"Unable to read {scannedItemName} image." );
                }

                alertMessageBuilder.AppendLine( $"Click 'Skip' to reject this {scannedItemName} and continue scanning." );
                alertMessageBuilder.AppendLine( $"To retry this check, put the {scannedItemName} back into the feed tray." );
                alertMessageBuilder.AppendLine( "" );
                alertMessageBuilder.AppendLine( $"Click 'Upload' to upload the {scannedItemName} as-is." );

                this.DisplayAlertMessage( AlertMessageType.Warning, alertMessageBuilder.ToString() );
                ShowUploadWarnings( scannedDocInfo );
                return;
            }
            else
            {

                scannedDocInfo.Upload = true;

            }

            if ( scannedDocInfo.Upload && ScanningPageUtility.IsDuplicateScan( scannedDocInfo ) )
            {
                scannedDocInfo.Duplicate = true;
                scannedDocInfo.Upload = false;
                var message = @"A check with the same account information and check number has already been scanned."
                + Environment.NewLine
                + "Click 'Skip' to reject this check."
                + Environment.NewLine
                + "Click 'Upload' to upload the check as-is.";
                this.DisplayAlertMessage( AlertMessageType.Warning, message );

                ShowUploadWarnings( scannedDocInfo );
            }

            if ( scannedDocInfo.Upload )
            {
                var uploaded = ScanningPageUtility.UploadScannedItem( scannedDocInfo, ( x ) => { lblScanItemUploadSuccess.Visibility = Visibility.Visible; } );
                if ( uploaded )
                {
                    this.ShowUploadSuccess();
                }
                if ( ScanningPageUtility.KeepScanning )
                {

                    ScanningPageUtility.ResumeScanning();
                }
            }

        }

        /// <summary>
        /// Handles the Click event of the btnIgnoreAndUpload control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnIgnoreAndUpload_Click( object sender, RoutedEventArgs e )
        {
            HideUploadWarningPrompts();
            HideAlertMessage();
            var scannedDocInfo = ScanningPageUtility.ConfirmUploadBadScannedDoc;
            scannedDocInfo.Upload = true;
            var uploaded = ScanningPageUtility.UploadScannedItem( scannedDocInfo );
            if ( uploaded )
            {
                this.ShowStartScanningPrompts();
                this.ShowUploadSuccess();
            }
            if ( this._interfaceType == RockConfig.InterfaceType.RangerApi )
            {
                ScanningPageUtility.ResumeScanning();
            }
            else
            {
                btnStart.IsEnabled = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSkipAndContinue control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSkipAndContinue_Click( object sender, RoutedEventArgs e )
        {
            HideUploadWarningPrompts();
            this.HideAlertMessage();
            ShowUploadSkipped();
            ScanningPageUtility.ConfirmUploadBadScannedDoc = null;
            ScanningPageUtility.ItemsSkipped++;
            ShowUploadStats();
            if ( _interfaceType == RockConfig.InterfaceType.RangerApi )
            {
                ScanningPageUtility.ResumeScanning();
            }
            else
            {
                btnStart.IsEnabled = true;
            }
        }

        /// <summary>
        /// Shows the upload warnings.
        /// </summary>
        private void ShowUploadWarnings( ScannedDocInfo scannedDocInfo )
        {
            ScanningPageUtility.ConfirmUploadBadScannedDoc = scannedDocInfo;
            lblScanItemUploadSuccess.Visibility = Visibility.Collapsed;
            lblScanItemUploadSkipped.Visibility = Visibility.Collapsed;
            pnlPromptForUpload.Visibility = (scannedDocInfo.Duplicate || scannedDocInfo.BadMicr || scannedDocInfo.ImageFailure)  ? Visibility.Visible : Visibility.Collapsed;

            btnStart.IsEnabled = false;
            btnStopScanning.IsEnabled = true;
        }

        /// <summary>
        /// Hides the warnings messages and prompts
        /// </summary>
        private void HideUploadWarningPrompts( bool hideWarningMessages = true )
        {
            if ( !hideWarningMessages )
            {
                btnStart.IsEnabled = false;
            }

            pnlPromptForUpload.Visibility = Visibility.Collapsed;
        }



        /// <summary>
        /// Shows the upload success.
        /// </summary>
        private void ShowUploadSuccess()
        {
            lblScanItemUploadSuccess.Visibility = Visibility.Visible;
            lblScanItemUploadSkipped.Visibility = Visibility.Collapsed;
            pnlPromptForUpload.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Shows the upload skipped.
        /// </summary>
        private void ShowUploadSkipped()
        {
            lblScanItemUploadSuccess.Visibility = Visibility.Collapsed;
            lblScanItemUploadSkipped.Visibility = Visibility.Visible;
            pnlPromptForUpload.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Hides the upload success.
        /// </summary>
        private void HideUploadSuccess()
        {
            lblScanItemUploadSkipped.Visibility = Visibility.Collapsed;
            lblScanItemUploadSuccess.Visibility = Visibility.Collapsed;
            pnlPromptForUpload.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Displays the scanned document information.
        /// </summary>
        /// <param name="scannedDocInfo">The scanned document information.</param>
        private void DisplayScannedDocInfo( ScannedDocInfo scannedDocInfo )
        {
            if ( scannedDocInfo.FrontImageData != null )
            {
                this.spScannedItemDisplay.Visibility = Visibility.Visible;
                BitmapImage bitmapImageFront = new BitmapImage();
                bitmapImageFront.BeginInit();
                bitmapImageFront.StreamSource = new MemoryStream( scannedDocInfo.FrontImageData );
                bitmapImageFront.EndInit();
                imgScannedItemFront.Source = bitmapImageFront;
                imgFrontThumb.Source = bitmapImageFront;
                Rock.Wpf.WpfHelper.FadeIn( imgScannedItemFront, 100 );
            }
            else
            {
                imgScannedItemFront.Source = null;
            }

            if ( scannedDocInfo.BackImageData != null )
            {
                grdImageThumbnailsButtons.Visibility = Visibility.Visible;
                BitmapImage bitmapImageBack = new BitmapImage();
                bitmapImageBack.BeginInit();
                bitmapImageBack.StreamSource = new MemoryStream( scannedDocInfo.BackImageData );
                bitmapImageBack.EndInit();
                imgScannedItemBack.Source = bitmapImageBack;
                imgBackThumb.Source = bitmapImageBack;
            }
            else
            {
                grdImageThumbnailsButtons.Visibility = Visibility.Collapsed;
            }

            spScannedItemDisplay.Visibility = Visibility.Visible;

            if ( scannedDocInfo.IsCheck )
            {
                gCheckMICRInfo.Visibility = Visibility.Visible;
                lblMicrRoutingAccountValue.Content = string.Format( "{0} / {1}", scannedDocInfo.RoutingNumber, scannedDocInfo.AccountNumber );
                lblMicrCheckNumber.Content = scannedDocInfo.CheckNumber;
                lblMicrOtherData.Content = scannedDocInfo.OtherData;
                spMicrOtherData.Visibility = !string.IsNullOrWhiteSpace( scannedDocInfo.OtherData ) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Handles the Loaded event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Page_Loaded( object sender, RoutedEventArgs e )
        {
            // Image safe is a manual feed so Stop button not Required
            if ( _interfaceType == RockConfig.InterfaceType.MagTekImageSafe )
            {
                this.btnStopScanning.Visibility = Visibility.Collapsed;
            }

            var rockConfig = RockConfig.Load();
            this._interfaceType = rockConfig.ScannerInterfaceType;
            this.gCheckMICRInfo.Visibility = Visibility.Collapsed;
            this.spScannedItemDisplay.Visibility = Visibility.Collapsed;

            // set the uploadScannedItemClient to null and reconnect to ensure we have a fresh connection (just in case they changed the url, or if the connection died for some other reason)
            ScanningPageUtility.UploadScannedItemClient = null;
            ScanningPageUtility.EnsureUploadScanRestClient();
            ScanningPageUtility.Initialize();
            ShowStartScanningPrompts();
            ShowUploadStats();
            StartScanning();
            lblScanItemCountInfo.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Hides the start scanning prompts.
        /// </summary>
        private void HideStartScanningPrompts()
        {
            this.lblStartInfo.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Shows the start scanning prompts.
        /// </summary>
        private void ShowStartScanningPrompts()
        {
            var rockConfig = RockConfig.Load();
            HideUploadWarningPrompts();
            this.HideUploadSuccess();
            lblAlert.Visibility = Visibility.Collapsed;
            lblExceptions.Visibility = Visibility.Collapsed;
            lblStartInfo.Visibility = Visibility.Visible;

            bool scanningChecks = rockConfig.TenderTypeValueGuid.AsGuid() == Rock.Client.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid();

            if ( rockConfig.ScannerInterfaceType == RockConfig.InterfaceType.MICRImageRS232 )
            {
                lblStartInfo.Content = string.Format( "Ready to scan next {0}.", scanningChecks ? "check" : "item" );
                lblStartInfo.Visibility = Visibility.Visible;

                grdImageThumbnailsButtons.Visibility = Visibility.Collapsed;
                btnStopScanning.Visibility = Visibility.Collapsed;
                btnStart.Visibility = Visibility.Collapsed;
                return;
            }
            else
            {
                lblStartInfo.Content = "Click Start to begin";
            }

            if ( rockConfig.ScannerInterfaceType == RockConfig.InterfaceType.MagTekImageSafe )
            {
                grdImageThumbnailsButtons.Visibility = Visibility.Collapsed;
                btnStopScanning.Visibility = Visibility.Collapsed;
                btnStart.IsEnabled = true;
                return;
            }

            ScannedDocInfo sampleDocInfo = new ScannedDocInfo();
            sampleDocInfo.CurrencyTypeValue = ScanningPageUtility.batchPage.CurrencyValueList.FirstOrDefault( a => a.Guid == RockConfig.Load().SourceTypeValueGuid.AsGuid() );
            DisplayScannedDocInfo( sampleDocInfo );
        }

        /// <summary>
        /// Shows the scanner status.
        /// </summary>
        /// <param name="xportStates">The xport states.</param>
        /// <param name="statusColor">Color of the status.</param>
        /// <param name="statusText">The status text.</param>
        internal void ShowScannerStatus( RangerTransportStates xportStates, System.Windows.Media.Color statusColor, string statusText )
        {
            ScanningPageUtility.ShowScannerStatus( xportStates, statusColor, statusText, ref shapeStatus );
        }

        #region Ranger (Canon CR50/80) Scanner Events

        /// <summary>
        /// Rangers the scanner_ transport feeding stopped.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        public void rangerScanner_TransportFeedingStopped( object sender, AxRANGERLib._DRangerEvents_TransportFeedingStoppedEvent e )
        {
            RangerFeedingStoppedReasons rangerFeedingStoppedReason = ( RangerFeedingStoppedReasons ) e.reason;

            System.Diagnostics.Debug.WriteLine( $"{DateTime.Now.ToString( "o" )} : rangerScanner_TransportFeedingStopped, reason:{rangerFeedingStoppedReason.ConvertToString()}" );

            if ( pnlPromptForUpload.Visibility != Visibility.Visible )
            {
                btnStart.IsEnabled = true;
            }

            btnClose.IsEnabled = true;
            if ( pnlPromptForUpload.Visibility != Visibility.Visible )
            {
                btnStopScanning.IsEnabled = false;
            }

            if ( ScanningPageUtility.ItemsScanned == 0 )
            {
                // show the Startup Info "Welcome" message if no check images are shown yet
                if ( spScannedItemDisplay.Visibility != Visibility.Visible )
                {
                    lblStartInfo.Visibility = Visibility.Visible;
                }

                // show a "No Items" warning if they clicked Start but it stopped because of MainHopperEmpty
                if ( rangerFeedingStoppedReason == RangerFeedingStoppedReasons.MainHopperEmpty )
                {
                    bool scanningChecks = RockConfig.Load().TenderTypeValueGuid.AsGuid() == Rock.Client.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid();
                    var noItemfound = string.Format( "No {0} detected in scanner. Make sure {0} are properly in the feed tray.", scanningChecks ? "checks" : "items" );
                    DisplayAlertMessage( AlertMessageType.Warning, noItemfound );
                }
            }
        }

        /// <summary>
        /// Handles the TransportNewItem event of the rangerScanner control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void rangerScanner_TransportNewItem( object sender, EventArgs e )
        {
            System.Diagnostics.Debug.WriteLine( string.Format( "{0} : rangerScanner_TransportNewItem", DateTime.Now.ToString( "o" ) ) );
            ScanningPageUtility.ItemsScanned++;
        }

        /// <summary>
        /// Handles the TransportFeedingState event of the rangerScanner control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void rangerScanner_TransportFeedingState( object sender, EventArgs e )
        {
            lblStartInfo.Visibility = Visibility.Collapsed;
            System.Diagnostics.Debug.WriteLine( string.Format( "{0} : rangerScanner_TransportFeedingState", DateTime.Now.ToString( "o" ) ) );
            HideAlertMessage();
            btnStart.IsEnabled = false;
            btnClose.IsEnabled = false;
            btnStopScanning.IsEnabled = true;
        }

        /// <summary>
        /// Rangers the scanner_ transport item in pocket.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        public void rangerScanner_TransportSetItemOutput( object sender, AxRANGERLib._DRangerEvents_TransportSetItemOutputEvent e )
        {
            System.Diagnostics.Debug.WriteLine( string.Format( "{0} : rangerScanner_TransportSetItemOutput", DateTime.Now.ToString( "o" ) ) );
            var currentPage = Application.Current.MainWindow.Content;

            if ( currentPage != this )
            {
                // only accept scans when the scanning page is showing
                ScanningPageUtility.batchPage.micrImage.ClearBuffer();
                return;
            }

            try
            {
                HideStartScanningPrompts();

                this.lblAlert.Visibility = Visibility.Collapsed;

                RockConfig rockConfig = RockConfig.Load();

                ScannedDocInfo scannedDoc = new ScannedDocInfo();

                // mark it as Upload, but we'll set it to false if anything bad happens before we actually upload
                scannedDoc.Upload = true;
                scannedDoc.CurrencyTypeValue = ScanningPageUtility.batchPage.SelectedCurrencyValue;
                scannedDoc.SourceTypeValue = ScanningPageUtility.batchPage.SelectedSourceTypeValue;

                scannedDoc.FrontImageData = ScanningPageUtility.GetImageBytesFromRanger( RangerSides.TransportFront );

                if ( rockConfig.EnableRearImage )
                {
                    scannedDoc.BackImageData = ScanningPageUtility.GetImageBytesFromRanger( RangerSides.TransportRear );
                }

                if ( scannedDoc.IsCheck )
                {
                    string checkMicr = ScanningPageUtility.batchPage.rangerScanner.GetMicrText( 1 );
                    ScanningPageUtility.WriteToDebugLog( string.Format( "[{0}] - '{1}'", DateTime.Now.ToString( "o" ), checkMicr ) );
                    string remainingMicr = checkMicr;
                    string accountNumber = string.Empty;
                    string routingNumber = string.Empty;
                    string checkNumber = string.Empty;

                    // there should always be two transit symbols ('d').  The transit number is between them
                    int transitSymbol1 = remainingMicr.IndexOf( ( char ) RangerE13BMicrSymbols.E13B_TransitSymbol );
                    int transitSymbol2 = remainingMicr.LastIndexOf( ( char ) RangerE13BMicrSymbols.E13B_TransitSymbol );
                    int transitStart = transitSymbol1 + 1;
                    int transitLength = transitSymbol2 - transitSymbol1 - 1;
                    if ( transitLength > 0 )
                    {
                        routingNumber = remainingMicr.Substring( transitStart, transitLength );
                        remainingMicr = remainingMicr.Remove( transitStart - 1, transitLength + 2 );
                    }

                    char[] separatorSymbols = new char[] { ( char ) RangerE13BMicrSymbols.E13B_TransitSymbol, ( char ) RangerE13BMicrSymbols.E13B_OnUsSymbol, ( char ) RangerE13BMicrSymbols.E13B_AmountSymbol };

                    // the last 'On-Us' symbol ('c') signifies the end of the account number
                    int lastOnUsPosition = remainingMicr.LastIndexOf( ( char ) RangerE13BMicrSymbols.E13B_OnUsSymbol );
                    if ( lastOnUsPosition > 0 )
                    {
                        int accountNumberDigitPosition = lastOnUsPosition - 1;

                        // read all digits to the left of the last 'OnUs' until you run into another seperator symbol
                        while ( accountNumberDigitPosition >= 0 )
                        {
                            char accountNumberDigit = remainingMicr[accountNumberDigitPosition];
                            if ( separatorSymbols.Contains( accountNumberDigit ) )
                            {
                                break;
                            }
                            else
                            {
                                accountNumber = accountNumberDigit + accountNumber;
                                accountNumber = accountNumber.Trim();
                            }

                            accountNumberDigitPosition--;
                        }

                        remainingMicr = remainingMicr.Remove( accountNumberDigitPosition + 1, lastOnUsPosition - accountNumberDigitPosition );
                    }

                    // any remaining digits that aren't the account number and transit number are probably the check number
                    string[] remainingMicrParts = remainingMicr.Split( new char[] { ( char ) RangerE13BMicrSymbols.E13B_OnUsSymbol, ' ' }, StringSplitOptions.RemoveEmptyEntries );
                    string otherData = null;
                    if ( remainingMicrParts.Any() )
                    {
                        // Now that we've indentified Routing and AccountNumber, the remaining MICR part is probably the CheckNumber. However, there might be multiple Parts left. We'll have to make a best guess on which chunk is the CheckNumber.
                        // In those cases, assume the 'longest' chunk to the CheckNumber. (Other chunks tend to be short 1 or 2 digit numbers that mean something special to the bank)
                        checkNumber = remainingMicrParts.OrderBy( p => p.Length ).Last();

                        // throw any remaining data into 'otherData' (a reject symbol could be in the other data)
                        remainingMicr = remainingMicr.Replace( ( char ) RangerE13BMicrSymbols.E13B_OnUsSymbol, ' ' );
                        remainingMicr = remainingMicr.Replace( checkNumber, string.Empty );
                        otherData = remainingMicr;
                    }

                    scannedDoc.RoutingNumber = routingNumber;
                    scannedDoc.AccountNumber = accountNumber;
                    scannedDoc.CheckNumber = checkNumber;
                    scannedDoc.OtherData = otherData;

                    scannedDoc.ScannedCheckMicrData = checkMicr;

                    // look for the "can't read" symbol (or completely blank read ) to detect if the check micr couldn't be read
                    // from http://www.sbulletsupport.com/forum/index.php?topic=172.0
                    if ( checkMicr.Contains( ( char ) RangerCommonSymbols.RangerRejectSymbol ) || string.IsNullOrWhiteSpace( checkMicr ) )
                    {
                        scannedDoc.BadMicr = true;
                        scannedDoc.Upload = false;
                    }
                }

                ShowScannedDocStatusAndUpload( scannedDoc );
            }
            catch ( Exception ex )
            {
                if ( ex is AggregateException )
                {
                    ScanningPageUtility.ShowException( ( ex as AggregateException ).Flatten(), this.lblExceptions );
                }
                else
                {
                    ScanningPageUtility.ShowException( ex, this.lblExceptions );
                }
            }
        }

        /// <summary>
        /// Handles the TransportIsDead event of the rangerScanner control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void rangerScanner_TransportIsDead( object sender, EventArgs e )
        {
            ScanningPageUtility.rangerScanner_TransportIsDead( sender, e, () =>
            {
                DisplayAlertMessage( AlertMessageType.Warning, "Scanner is not ready. Verify that the scanner is powered on and connected." );
            } );
        }

        #endregion

        #region Scanner (MagTek MICRImage RS232) Events

        /// <summary>
        /// Handles the MicrDataReceived event of the micrImage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public void micrImage_MicrDataReceived( object sender, System.EventArgs e )
        {
            HideStartScanningPrompts();

            var currentPage = Application.Current.MainWindow.Content;

            if ( currentPage != this )
            {
                // only accept scans when the scanning page is showing
                ScanningPageUtility.batchPage.micrImage.ClearBuffer();
                return;
            }


            // from MagTek Sample Code
            object dummy = null;
            string routingNumber = ScanningPageUtility.batchPage.micrImage.FindElement( 0, "T", 0, "TT", ref dummy );
            string accountNumber = ScanningPageUtility.batchPage.micrImage.FindElement( 0, "TT", 0, "A", ref dummy );
            string checkNumber = ScanningPageUtility.batchPage.micrImage.FindElement( 0, "A", 0, "12", ref dummy );
            short trackNumber = 0;
            var rawMICR = ScanningPageUtility.batchPage.micrImage.GetTrack( ref trackNumber );

            ScannedDocInfo scannedDoc = null;
            var rockConfig = RockConfig.Load();
            bool scanningMagTekBackImage = false;

            if ( _currentMagtekScannedDoc != null && _currentMagtekScannedDoc.BackImageData == null && rockConfig.PromptToScanRearImage )
            {
                //// if we didn't get a routingnumber, and we are expecting a back scan, use the scan as the back image
                //// However, if we got a routing number, assuming we are scanning a new check regardless

                if ( string.IsNullOrWhiteSpace( routingNumber ) )
                {
                    scanningMagTekBackImage = true;
                }
                else
                {
                    scanningMagTekBackImage = false;
                }
            }

            if ( scanningMagTekBackImage )
            {
                scannedDoc = _currentMagtekScannedDoc;
            }
            else
            {
                scannedDoc = new ScannedDocInfo();
                scannedDoc.CurrencyTypeValue = ScanningPageUtility.batchPage.SelectedCurrencyValue;
                scannedDoc.SourceTypeValue = ScanningPageUtility.batchPage.SelectedSourceTypeValue;

                if ( scannedDoc.IsCheck )
                {
                    scannedDoc.ScannedCheckMicrData = rawMICR;
                    scannedDoc.RoutingNumber = routingNumber;
                    scannedDoc.AccountNumber = accountNumber;
                    scannedDoc.CheckNumber = checkNumber;

                    ScanningPageUtility.WriteToDebugLog( string.Format( "[{0}] - '{1}'", DateTime.Now.ToString( "o" ), scannedDoc.ScannedCheckMicrData ) );
                }

                // set the _currentMagtekScannedDoc in case we are going to scan the back of the image
                _currentMagtekScannedDoc = scannedDoc;
            }

            string imagePath = Path.GetTempPath();
            string docImageFileName = Path.Combine( imagePath, string.Format( "scanned_item_{0}.tif", Guid.NewGuid() ) );
            if ( File.Exists( docImageFileName ) )
            {
                File.Delete( docImageFileName );
            }

            try
            {
                string statusMsg = string.Empty;
                //Writing To File 
                ScanningPageUtility.batchPage.micrImage.TransmitCurrentImage( docImageFileName, ref statusMsg );
                if ( !File.Exists( docImageFileName ) )
                {
                    throw new Exception( "Unable to retrieve image" );
                }

                if ( scanningMagTekBackImage )
                {
                    scannedDoc.BackImageData = File.ReadAllBytes( docImageFileName );
                }
                else
                {
                    scannedDoc.FrontImageData = File.ReadAllBytes( docImageFileName );

                    // MagTek puts the symbol '?' for parts of the MICR that it can't read
                    bool gotValidMicr = !string.IsNullOrWhiteSpace( scannedDoc.AccountNumber ) && !scannedDoc.AccountNumber.Contains( '?' )
                        && !string.IsNullOrWhiteSpace( scannedDoc.RoutingNumber ) && !scannedDoc.RoutingNumber.Contains( '?' )
                        && !string.IsNullOrWhiteSpace( scannedDoc.CheckNumber ) && !scannedDoc.CheckNumber.Contains( '?' );

                    if ( scannedDoc.IsCheck && !gotValidMicr )
                    {
                        scannedDoc.BadMicr = true;
                    }
                }

                bool scanningChecks = RockConfig.Load().TenderTypeValueGuid.AsGuid() == Rock.Client.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid();
                if ( scannedDoc.BackImageData == null && rockConfig.PromptToScanRearImage )
                {
                    // scanning the front image, but still need to scan the back
                    HideUploadWarningPrompts( true );
                    // scanning the front image, but still need to scan the back
                    var message = string.Format( "Insert the {0} again facing the other direction to get an image of the back.", scanningChecks ? "check" : "item" );
                    this.DisplayAlertMessage( AlertMessageType.Info, message );
                    DisplayScannedDocInfo( scannedDoc );
                }
                else
                {
                    // scanned both sides (or just the front if they don't want to scan both sides )
                    scannedDoc.Upload = !scannedDoc.IsCheck || !( scannedDoc.BadMicr || scannedDoc.Duplicate );
                    this.ShowScannedDocStatusAndUpload( scannedDoc );
                }

                File.Delete( docImageFileName );
            }
            catch ( Exception ex )
            {
                if ( ex is AggregateException )
                {
                    ScanningPageUtility.ShowException( ( ex as AggregateException ).Flatten(), this.lblExceptions );
                }
                else
                {
                    ScanningPageUtility.ShowException( ex, this.lblExceptions );
                }
            }
            finally
            {
                ScanningPageUtility.batchPage.micrImage.ClearBuffer();
            }
        }

        #endregion

        #region MagTek USB

        private void imageSafeCallback( CheckData e )
        {
            HideStartScanningPrompts();

            System.Diagnostics.Debug.WriteLine( string.Format( "{0} : ImageSafe_CheckData", DateTime.Now.ToString( "o" ) ) );
            var currentPage = Application.Current.MainWindow.Content;

            if ( currentPage != this )
            {
                // only accept scans when the scanning page is showing
                ScanningPageUtility.batchPage.micrImage.ClearBuffer();
                return;
            }

            ScannedDocInfo scannedDoc = _currentMagtekScannedDoc;
            var rockConfig = RockConfig.Load();
            bool scanningImageSafeBackImage = false;
            if ( _currentMagtekScannedDoc != null && _currentMagtekScannedDoc.BackImageData == null && rockConfig.PromptToScanRearImage )
            {
                //// if we didn't get a routingnumber, and we are expecting a back scan, use the scan as the back image
                //// However, if we got a routing number, assuming we are scanning a new check regardless

                if ( string.IsNullOrWhiteSpace( e.RoutingNumber ) )
                {
                    scanningImageSafeBackImage = true;
                }
                else
                {
                    scanningImageSafeBackImage = false;
                }

            }
            if ( scanningImageSafeBackImage )
            {
                scannedDoc = _currentMagtekScannedDoc;
            }
            else
            {
                scannedDoc = new ScannedDocInfo();
                scannedDoc.CurrencyTypeValue = ScanningPageUtility.batchPage.SelectedCurrencyValue;
                scannedDoc.SourceTypeValue = ScanningPageUtility.batchPage.SelectedSourceTypeValue;

                if ( scannedDoc.IsCheck && !e.HasError )
                {
                    scannedDoc.ScannedCheckMicrData = e.ScannedCheckMicrData;
                    scannedDoc.RoutingNumber = e.RoutingNumber;
                    scannedDoc.AccountNumber = e.AccountNumber;
                    scannedDoc.CheckNumber = e.CheckNumber;
                    scannedDoc.ScannedCheckMicrData = e.ScannedCheckMicrData;
                    if ( !e.ScannedCheckMicrData.Contains( "??" ) )
                    {
                        scannedDoc.OtherData = ImageSafeHelper.GetOtherDataFromMicrData( e );
                    }

                    ScanningPageUtility.WriteToDebugLog( string.Format( "[{0}] - '{1}'", DateTime.Now.ToString( "o" ), scannedDoc.ScannedCheckMicrData ) );
                }


            }
            // set the _currentMagtekScannedDoc in case we are going to scan the back of the image
            _currentMagtekScannedDoc = scannedDoc;

            try
            {
                bool scanningChecks = RockConfig.Load().TenderTypeValueGuid.AsGuid() == Rock.Client.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid();
                if ( e.HasError )
                {
                    StringBuilder sb = e.Errors;
                    var timeoutError = sb.ToString().Contains( "Timeout" );
                    if ( timeoutError )
                    {
                        var noItemfound = string.Format( "No {0} detected in scanner. Make sure {0} are properly in the feed tray.", scanningChecks ? "checks" : "items" );
                        DisplayAlertMessage( AlertMessageType.Warning, noItemfound );
                    }
                    return;
                }


                //Bad Read
                //Bad Read
                // We set is back scan when we scan front after prompt and are scanning the back
                if ( !_isBackScan && ( e.ScannedCheckMicrData == null || e.ScannedCheckMicrData.Contains( "?" ) ) )
                {
                    scannedDoc.BadMicr = true;
                }

                scannedDoc.ImageFailure = e.ImageData == null;

                if ( _currentMagtekScannedDoc != null && _currentMagtekScannedDoc.FrontImageData != null )
                {
                    _currentMagtekScannedDoc.BackImageData = e.ImageData;
                }
                else
                {
                    _currentMagtekScannedDoc.FrontImageData = scannedDoc.FrontImageData;
                }
                if ( _currentMagtekScannedDoc.BackImageData == null && rockConfig.PromptToScanRearImage )
                {
                    // scanning the front image, but still need to scan the back
                    HideUploadWarningPrompts( true );
                    // scanning the front image, but still need to scan the back
                    var message = string.Format( "Insert the {0} again facing the other direction to get an image of the back.", scanningChecks ? "checks" : "items" );
                    this.DisplayAlertMessage( AlertMessageType.Info, message );

                    _currentMagtekScannedDoc.FrontImageData = e.ImageData;
                    _isBackScan = true;
                    DisplayScannedDocInfo( _currentMagtekScannedDoc );
                }
                else
                {


                    //// Non Prompt 
                    // mark it as Upload, but we'll set it to false if anything bad happens before we actually upload
                    scannedDoc.Upload = true;
                    scannedDoc.CurrencyTypeValue = ScanningPageUtility.batchPage.SelectedCurrencyValue;
                    scannedDoc.SourceTypeValue = ScanningPageUtility.batchPage.SelectedSourceTypeValue;
                    if ( scannedDoc.IsCheck )
                    {

                        if ( _isBackScan )
                        {
                            scannedDoc = _currentMagtekScannedDoc;
                            scannedDoc.BackImageData = e.ImageData;
                        }
                        else
                        {
                            //Check Bad Read without prompt
                            if ( e.ScannedCheckMicrData.Contains( "?" ) )
                            {
                                scannedDoc.BadMicr = true;
                            }
                            scannedDoc.FrontImageData = e.ImageData;
                        }

                        if ( !scannedDoc.BadMicr && !_isBackScan )
                        {
                            if ( !e.ScannedCheckMicrData.Contains( "??" ) )
                            {
                                scannedDoc.OtherData = ImageSafeHelper.GetOtherDataFromMicrData( e );
                            }
                        }
                    }

                    _isBackScan = false;


                    ShowScannedDocStatusAndUpload( scannedDoc );
                }
            }

            catch ( Exception ex )
            {
                System.Diagnostics.Debug.WriteLine( $"imageSafeCallback {ex}" );
            }
        }



        #endregion

        #region Image Upload related

        #endregion

        /// <summary>
        /// Handles the Click event of the btnClose control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnClose_Click( object sender, RoutedEventArgs e )
        {
            this.NavigationService.Navigate( ScanningPageUtility.batchPage );
        }

        /// <summary>
        /// Shows the upload stats.
        /// </summary>
        private void ShowUploadStats()
        {
            List<string> statsList = new List<string>();
            if ( ScanningPageUtility.ItemsUploaded > 0 )
            {
                statsList.Add( string.Format( "Uploaded: {0}", ScanningPageUtility.ItemsUploaded ) );
            }

            if ( ScanningPageUtility.ItemsSkipped > 0 )
            {
                statsList.Add( string.Format( "Skipped: {0}", ScanningPageUtility.ItemsSkipped ) );
            }

            lblScanItemCountInfo.Visibility = statsList.Any() ? Visibility.Visible : Visibility.Collapsed;
            lblScanItemCountInfo.Content = string.Join( ", ", statsList );
        }

        //
        /// <summary>
        /// Handles the Click event of the btnStart control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnStart_Click( object sender, RoutedEventArgs e )
        {
            this.lblAlert.Visibility = Visibility.Collapsed;

            var rockConfig = RockConfig.Load();
            if ( rockConfig.ScannerInterfaceType == RockConfig.InterfaceType.RangerApi )
            {
                RangerTransportStates[] xportStatesNotConnected = new RangerTransportStates[] { RangerTransportStates.TransportShutDown, RangerTransportStates.TransportShuttingDown, RangerTransportStates.TransportExceptionInProgress };

                var transportState = ( RangerTransportStates ) ScanningPageUtility.batchPage.rangerScanner.GetTransportState();
                if ( xportStatesNotConnected.Contains( transportState ) )
                {
                    ScanningPageUtility.batchPage.ConnectToScanner();
                }
                else
                {
                    StartScanning();
                }
            }
            else
            {
                if ( rockConfig.ScannerInterfaceType == RockConfig.InterfaceType.MagTekImageSafe )
                {
                    ImageSafeHelper.ProcessDocument( imageSafeCallback );
                }

            }
        }

        /// <summary>
        /// Starts the scanning as soon as items are in the hopper
        /// </summary>
        public void StartScanning()
        {
            ScanningPageUtility.KeepScanning = true;
            ScanningPageUtility.ResumeScanning();
        }

        /// <summary>
        /// Handles the Click event of the btnStopScanning control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnStopScanning_Click( object sender, RoutedEventArgs e )
        {
            HideUploadWarningPrompts( false );
            StopScanning();
            btnStart.IsEnabled = true;
        }

        /// <summary>
        /// Stops the scanning.
        /// </summary>
        private void StopScanning()
        {
            ScanningPageUtility.KeepScanning = false;
            if ( ScanningPageUtility.batchPage.rangerScanner != null )
            {
                // remove the StartRangerFeedingWhenReady (in case it is assigned) so it doesn't restart after getting into ReadyToFeed state
                ScanningPageUtility.batchPage.rangerScanner.TransportReadyToFeedState -= StartRangerFeedingWhenReady;
                ScanningPageUtility.batchPage.rangerScanner.StopFeeding();
            }
        }


        /// <summary>
        /// Starts the ranger feeding when ready.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        public void StartRangerFeedingWhenReady( object sender, AxRANGERLib._DRangerEvents_TransportReadyToFeedStateEvent e )
        {
            // only fire this event once
            ScanningPageUtility.batchPage.rangerScanner.TransportReadyToFeedState -= StartRangerFeedingWhenReady;

            if ( ScanningPageUtility.KeepScanning )
            {
                ScanningPageUtility.batchPage.rangerScanner.StartFeeding( FeedSource.FeedSourceMainHopper, FeedItemCount.FeedOne );
            }
        }

        /// <summary>
        /// Handles the Click event of the BtnImageToggle_FrontBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnImageToggle_FrontBack_Click( object sender, RoutedEventArgs e )
        {
            var parameter = ( ( Button ) e.OriginalSource ).CommandParameter as string;
            switch ( parameter )
            {
                case "Front":
                    this.imgScannedItemBack.Visibility = Visibility.Collapsed;
                    this.imgScannedItemFront.Visibility = Visibility.Visible;
                    break;
                case "Back":
                    this.imgScannedItemBack.Visibility = Visibility.Visible;
                    this.imgScannedItemFront.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnOptions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnOptions_Click( object sender, RoutedEventArgs e )
        {
            ScanningPageUtility.batchPage.NavigateToOptionsPage();
        }

        /// <summary>
        /// Hides the alert message.
        /// </summary>
        private void HideAlertMessage()
        {
            this.lblAlert.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Displays the alert message.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="message">The message.</param>
        private void DisplayAlertMessage( AlertMessageType messageType, string message )
        {
            this.lblAlert.Visibility = message.IsNotNullOrWhiteSpace() ? Visibility.Visible : Visibility.Collapsed;
            this.lblAlert.AlertType = messageType;
            this.lblAlert.Message = message;
        }
    }
}
