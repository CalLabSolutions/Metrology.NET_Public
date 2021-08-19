using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoA_Editor.ViewModels
{
    class DeleteDialogViewModel : Screen
    {
        private string message = "";
        public string Message
        {
            get { return message; }
            set { message = value; NotifyOfPropertyChange(() => Message); }
        }
    }
}
