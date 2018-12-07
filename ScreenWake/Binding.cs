using System.ComponentModel;

namespace ScreenWake
{
    class Binding : INotifyPropertyChanged
    {
        public delegate void SetTimeout(int timeout);

        private readonly SetTimeout _callback;

        #region Binding
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, 
                new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public Binding(SetTimeout callback)
        {
            Persistance = false;
            _callback = callback;
        }

        private string powerLineState;
        public string PowerLineState
        {
            get { return powerLineState; }
            set
            {
                powerLineState = value;
                OnPropertyChanged("PowerLineState");
            }
        }

        private int displayTimeout;
        public int DisplayTimeout
        {
            get { return displayTimeout; }
            set
            {
                displayTimeout = value;
                OnPropertyChanged("DisplayTimeout");
                _callback(displayTimeout);

            }
        }

        private bool? persistance;
        public bool? Persistance
        {
            get { return persistance; }
            set
            {
                persistance = value;
                OnPropertyChanged("Persistance");
            }
        }
    }
}
