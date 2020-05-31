using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoAEditor.Models;

namespace SoAEditor.ViewModels
{
    public class TaxonomyViewModel : Screen
    {
        private string _resultQuant;

        public string ResultQuant
        {
            get { return _resultQuant; }
            set { _resultQuant = value; NotifyOfPropertyChange(() => ResultQuant); }
        }

        private string _externalURL;

        public string ExternalURL
        {
            get { return _externalURL; }
            set { _externalURL = value; NotifyOfPropertyChange(() => ExternalURL); }
        }

        private string _embeddedDoc;

        public string EmbeddedDoc
        {
            get { return _embeddedDoc; }
            set { _embeddedDoc = value; NotifyOfPropertyChange(() => EmbeddedDoc); }
        }



        private ObservableCollection<TaxonomyInputParam> _inputParams;

        public ObservableCollection<TaxonomyInputParam> InputParams
        {
            get
            {
                return _inputParams;
            }
            set
            {
                Set(ref _inputParams, value);
            }
        }


        public TaxonomyViewModel()
        {
            InputParams = new ObservableCollection<TaxonomyInputParam>();
            {
                new TaxonomyInputParam("p1", "q1", "o1");
                new TaxonomyInputParam("p2", "q2", "o2");
            };
        }
    }
}
