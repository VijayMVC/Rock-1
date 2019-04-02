﻿

using System.ComponentModel;

namespace Rock.Apps.CheckScannerUtility.Models
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class DisplayFinancialTransactionDetailModel: INotifyPropertyChanged
    {
        private Rock.Client.FinancialTransactionDetail _financialTransactionDetail;
        private decimal? _amount;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayFinancialTransactionDetailModel"/> class.
        /// </summary>
        /// <param name="financialTransactionDetail">The financial transaction detail.</param>
        public DisplayFinancialTransactionDetailModel( Rock.Client.FinancialTransactionDetail financialTransactionDetail )
        {
            _financialTransactionDetail = financialTransactionDetail;
            _amount = financialTransactionDetail.Amount;
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id => _financialTransactionDetail.Id;


        /// <summary>
        /// Gets the display name of the account.
        /// </summary>
        /// <value>
        /// The display name of the account.
        /// </value>
        public string AccountDisplayName
        {
            get => _financialTransactionDetail.Account.PublicName.IsNotNullOrWhiteSpace() == true ? _financialTransactionDetail.Account.Name : _financialTransactionDetail.Account.PublicName;
        }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        public decimal? Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( "Amount" ) );
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
