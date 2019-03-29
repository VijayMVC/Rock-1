
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Rock.Apps.CheckScannerUtility.Models
{
    [System.Diagnostics.DebuggerDisplay( "{Id}:{AccountDisplayName}|{IsAccountChecked}" )]
    public class DisplayAccountModel : INotifyPropertyChanged
    {
        private bool _accountIsChecked;
        private string _accountDisplayName;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Id { get; set; }

        public string AccountDisplayName
        {
            get { return _accountDisplayName; }
            set
            {
                _accountDisplayName = value;
                PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( "AccountDisplayName" ) );
            }
        }
        public bool IsAccountChecked
        {
            get => _accountIsChecked;
            set
            {
                _accountIsChecked = value;
                PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( "IsAccountChecked" ) );
            }
        }

        public ObservableCollection<DisplayAccountModel> Children { get; set; }



    }
}
