using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace soa_1_03
{
    public class mClient : Inpc
    {
        #region Private Properties
        private string _company;
        private string _facility;
        private string _streeAddress01;
        private string _streeAddress02;
        private string _city;
        private string _state;
        private string _zip;
        private string _country;
        private string _labMgrEmail;
        private string _labMgrFirstName;
        private string _labMgrLastName;
        private string _labMgrPhone;
        #endregion

        #region Constructor

        #endregion

        #region Singleton
        private static readonly mClient instance = null;
        
        static mClient()
        {
            instance = new mClient();
        }

        public static mClient GetInstance()
        {
            return instance;
        }

        #endregion

        #region Public Properties
        [XmlElementAttribute (Order = 1)]
        public string company
        {
            get { return _company; }
            set
            {
                if (value != _company)
                {
                    _company = value;
                    OnPropertyChanged("company");
                }
            }
        }

        [XmlElementAttribute (Order = 2)]
        public string facility
        {
            get { return _facility; }
            set
            {
                if (value != _facility)
                {
                    _facility = value;
                    OnPropertyChanged("facility");
                }
            }
        }

        [XmlElementAttribute(Order = 3)]
        public string streetAddress01
        {
            get { return _streeAddress01; }
            set
            {
                if(value != _streeAddress01)
                {
                    _streeAddress01 = value;
                    OnPropertyChanged("streetAddress01");
                }
            }
        }

        [XmlElementAttribute(Order = 4)]
        public string streetAddress02
        {
            get { return _streeAddress02; }
            set
            {
                if (value != _streeAddress02)
                {
                    _streeAddress02 = value;
                    OnPropertyChanged("streetAddress02");
                }
            }
        }

        [XmlElementAttribute(Order = 5)]
        public string city
        {
            get { return _city; }
            set
            {
                if (value != _city)
                {
                    _city = value;
                    OnPropertyChanged("city");
                }
            }
        }

        [XmlElementAttribute(Order = 6)]
        public string state
        {
            get { return _state; }
            set
            {
                if (value != _state)
                {
                    _state = value;
                    OnPropertyChanged("state");
                }
            }
        }

        [XmlElementAttribute(Order = 7)]
        public string zip
        {
            get { return _zip; }
            set
            {
                if (value != _zip)
                {
                    _zip = value;
                    OnPropertyChanged("zip");
                }
            }
        }

        [XmlElementAttribute(Order = 8)]
        public string country
        {
            get { return _country; }
            set
            {
                if (value != _country)
                {
                    _country = value;
                    OnPropertyChanged("country");
                }
            }
        }

        [XmlElementAttribute(Order = 9)]
        public string labMgrFirstName
        {
            get { return _labMgrFirstName; }
            set
            {
                if (value != _labMgrFirstName)
                {
                    _labMgrFirstName = value;
                    OnPropertyChanged("labMgrFirstName");
                }
            }
        }

        [XmlElementAttribute(Order = 10)]
        public string labMgrLastName
        {
            get { return _labMgrLastName; }
            set
            {
                if (value != _labMgrLastName)
                {
                    _labMgrLastName = value;
                    OnPropertyChanged("labMgrLastName");
                }
            }
        }

        [XmlElementAttribute(Order = 11)]
        public string labMgrEmail
        {
            get { return _labMgrEmail; }
            set
            {
                if (value != _labMgrEmail)
                {
                    _labMgrEmail = value;
                    OnPropertyChanged("labMgrEmail");
                }
            }
        }

        [XmlElementAttribute(Order = 12)]
        public string labMgrPhone
        {
            get { return _labMgrPhone; }
            set
            {
                if (value != _labMgrPhone)
                {
                    _labMgrPhone = value;
                    OnPropertyChanged("labMgrPhone");
                }
            }
        }
        #endregion

        #region Methods

        #endregion

    }
}
