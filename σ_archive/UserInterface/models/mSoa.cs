using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace soa_1_03
{
    public class mSoa : Inpc
    {

        #region Members
        private ObservableCollection<mSoaTaxonomy> _soaTaxonomies;
        private string _soaAction;
        #endregion

        #region Constructor
        public mSoa()
        {
            //actions = new ObservableCollection<mSoaAction>() { new mSoaAction() { action = "Measure" }, new mSoaAction() { action = "Source" } };
            _soaTaxonomies = new ObservableCollection<mSoaTaxonomy>();
        }
        #endregion

        #region Singleton
        //private static readonly mSoa instance = null;

        //static mSoa()
        //{
        //    instance = new mSoa();
        //}

        //public static mSoa GetInstance()
        //{
        //    return instance;
        //}
        #endregion

        #region Properties
        [XmlElementAttribute (Order = 1)]
        public string soaAction
        {
            get { return _soaAction; }
            set
            {
                if ( value != _soaAction)
                {
                    _soaAction = value;
                    OnPropertyChanged("soaAction");
                }
            }
        }

        [XmlElementAttribute (Order = 2)]
        public ObservableCollection<mSoaTaxonomy> soaTaxonomies
        {
            get { return _soaTaxonomies; }
            set
            {
                if(value != _soaTaxonomies)
                {
                    _soaTaxonomies = value;
                    OnPropertyChanged("soaTaxonomies");
                }
            }
        }


        //public ObservableCollection<mSoaAction> actions
        //{
        //    get
        //    {
        //        return _actions;
        //    }

        //    set
        //    {
        //        _actions = value;
        //        OnPropertyChanged("actions");
        //    }
        //}
        #endregion

        #region Methods

        #endregion
    }

    //public class mSoaAction
    //{
    //    #region Members

    //    #endregion

    //    #region Constructor
    //    public mSoaAction()
    //    {
    //        soaTaxonomies = new ObservableCollection<mSoaTaxonomy>();
    //    }
    //    #endregion

    //    #region Properties
    //    public string action { get; set; }
    //    public ObservableCollection<mSoaTaxonomy> soaTaxonomies { get; set; }
    //    #endregion

    //    #region Methods

    //    #endregion
    //}

    public class mSoaTaxonomy :Inpc
    {
        #region Members
        private string _soaQuantity;
        private string _soaName;
        private string _soaSymbol;
        private string _soaTaxonomyDisplayString;
        private ObservableCollection<mSoaTechniqueDescriptor> _soaTechniqueDescriptors;
        //private ObservableCollection<mSoaTechnique> _soaTechniques;
        #endregion

        #region Constructor
        public mSoaTaxonomy()
        {
            //_soaTechniques = new ObservableCollection<mSoaTechnique>();
            _soaTechniqueDescriptors = new ObservableCollection<mSoaTechniqueDescriptor>();
        }
        #endregion

        #region Properties
        [XmlElementAttribute(Order = 1)]
        public string soaQuantity
        { get { return _soaQuantity; }
          set
            {
                if (value != _soaQuantity)
                {
                    _soaQuantity = value;
                    OnPropertyChanged("soaQauntity");
                }
            }
        }
        [XmlElementAttribute(Order = 2)]
        public string soaName { get; set; }
        [XmlElementAttribute(Order = 3)]
        public string soaSymbol { get; set; }

        public string soaTaxonomyDisplayString
        {
            get
            {
                string s = string.Format("{0} [{1} ({2})]", soaQuantity, soaName, soaSymbol);
                return s;
            }
        }

        [XmlElementAttribute(Order = 4)]
        public ObservableCollection<mSoaTechniqueDescriptor> soaTechniqueDescriptors
        {
            get { return _soaTechniqueDescriptors; }
            set
            {
                if (value != _soaTechniqueDescriptors)
                {
                    _soaTechniqueDescriptors = value;
                    OnPropertyChanged("soaTechniqueDescriptors");
                }
            }

        }
        #endregion

        #region Methods

        #endregion
    }
    
    public class mSoaTechniqueDescriptor :Inpc
    {
        private string _descriptor;
        private ObservableCollection<mSoaTechnique> _soaTechniques;

        public mSoaTechniqueDescriptor()
        {
            _soaTechniques = new ObservableCollection<mSoaTechnique>();
        }

        [XmlElementAttribute(Order = 1)]
        public string descriptor
        {
            get { return _descriptor; }
            set
            {
                if (value != _descriptor)
                {
                    _descriptor = value;
                    OnPropertyChanged("descriptor");
                }
            }
        }

        [XmlElementAttribute(Order = 2)]
        public ObservableCollection<mSoaTechnique> soaTechniques
        {
            get { return _soaTechniques; }
            set
            {
                if (value != _soaTechniques)
                {
                    _soaTechniques = value;
                    OnPropertyChanged("soaTechniques");
                }
            }
        }
    }
    public class mSoaTechnique
    {
        #region Members

        #endregion

        #region Constructor

        #endregion

        #region Properties
        //public string descriptor1 { get; set; }
        //public string descriptor2 { get; set; }
        public string rangeMin { get; set; }
        public string rangeMax { get; set; }
        public string uncertainty { get; set; }
        #endregion

        #region Methods

        #endregion
    }
}
