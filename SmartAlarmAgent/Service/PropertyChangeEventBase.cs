
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SmartAlarmAgent.Service
{
    public abstract class PropertyChangeEventBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected virtual void OnPropertyChanged(Object sender, string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
            }
        }
        public void Set(ref string _text, string value)
        {
           // if (_text != null)
           //     _text = value;

            if (_text != value)
                _text = value;
        }
        public void Set(ref bool _isCheck, bool value)
        {
            _isCheck = value;
            OnPropertyChanged("IsChecked");
        }

        protected virtual void SetProperty<T>(ref T member, T val,
           [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(member, val)) return;

            member = val;
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
