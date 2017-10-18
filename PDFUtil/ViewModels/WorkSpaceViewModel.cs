using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using PDFUtil.Commands;

namespace PDFUtil.ViewModels
{
    public abstract class WorkSpaceViewModel : ViewModelBase 
    {

        DelegateCommand _closeCommand;

        protected WorkSpaceViewModel()
        { 
        
        }

        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                    _closeCommand = new DelegateCommand(OnRequestClose);

                return _closeCommand;
            }
        }

        public event EventHandler RequestClose;

        void OnRequestClose()
        {
            EventHandler handler = this.RequestClose;
            if (handler != null)
                handler(this, EventArgs.Empty);

            IsOpen = false;
        }

        bool _isOpen;

        public bool IsOpen
        {
            get { return _isOpen; }
            set
            {
                _isOpen = value;
                this.OnPropertyChanged(p => p.IsOpen);
            }
        }

    }
}
