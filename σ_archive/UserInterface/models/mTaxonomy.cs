using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace soa_1_03.models
{
    public class mTaxonomy : Inpc
    {
        #region Private Fields
        private string _action;
        private string _taxonomy;
        private string _quantity;
        private string _fullTaxonomy;
        private List<dynamic> _dyRanges;
        private List<mRange> _pxRanges;
        private List<string> _pxRangeHeaders;
        private Dictionary<string, double> _range;
        private List<Dictionary<string, double>> _ranges;
        private List<mRequiredParams> _requiredParams;
        private List<mOptionalParams> _optionalParams;
        #endregion

        #region Constructor
        public mTaxonomy()
        {
            pxRanges = new List<mRange>();
            pxRangeHeaders = new List<string>();
            dyRanges = new List<dynamic>();
            requiredParams = new List<mRequiredParams>();
            optionalParams = new List<mOptionalParams>();
            range = new Dictionary<string, double>();
            ranges = new List<Dictionary<string, double>>();
        }
        #endregion

        #region Public Properties
        public string action
        {
            get { return _action; }
            set
            {
                if (value != _action)
                {
                    _action = value;
                    OnPropertyChanged("action");
                }
            }
        }

        public string taxonomy
        {
            get { return _taxonomy; }
            set
            {
                if (value != _taxonomy)
                {
                    _taxonomy = value;
                    CreateFullTaxonomy();
                    OnPropertyChanged("taxonomy");
                }
            }
        }

        public string quantity
        {
            get { return _quantity; }
            set
            {
                if (value != _quantity)
                {
                    _quantity = value;
                    CreateFullTaxonomy();
                    OnPropertyChanged("quantity");
                }
            }
        }

        public string fullTaxonomy
        {
            get { return _fullTaxonomy; }
            set
            {
                if (value != _fullTaxonomy)
                {
                    _fullTaxonomy = value;
                    OnPropertyChanged("value");
                }
            }
        }

        public Dictionary<string, double> range
        {
            get { return _range; }
            set
            {
                if (value != _range)
                {
                    _range = value;
                    OnPropertyChanged("range");
                }
            }
        }

        public List<Dictionary<string, double>> ranges
        {
            get { return _ranges; }
            set
            {
                if (value != _ranges)
                {
                    _ranges = value;
                    OnPropertyChanged("ranges");
                }
            }
        }

        public List<dynamic> dyRanges
        {
            get { return _dyRanges; }
            set
            {
                if (value != _dyRanges)
                {
                    _dyRanges = value;
                    OnPropertyChanged("dyRanges");
                }
            }
        }

        public List<string> pxRangeHeaders
        {
            get { return _pxRangeHeaders; }
            set
            {
                if (value != _pxRangeHeaders)
                {
                    _pxRangeHeaders = value;
                    OnPropertyChanged("pxRangeHeaders");
                }
            }
        }

        public List<mRange> pxRanges
        {
            get { return _pxRanges; }
            set
            {
                if (value != _pxRanges)
                {
                    _pxRanges = value;
                    OnPropertyChanged("pxRanges");
                }
            }
        }

        public List<mRequiredParams> requiredParams
        {
            get { return _requiredParams; }
            set
            {
                if (value != _requiredParams)
                {
                    _requiredParams = value;
                    //if (_requiredParams.Count != 0)
                    //{
                    //    CreateRange();
                    //}
                    //range = new mRange(requiredParams, optionalParams);
                    //ranges.Add(range);
                    OnPropertyChanged("requiredParams");
                }
            }
        }

        public List<mOptionalParams> optionalParams
        {
            get { return _optionalParams; }
            set
            {
                if (value != _optionalParams)
                {
                    _optionalParams = value;
                    OnPropertyChanged("optionalParams");
                }
            }
        }
        #endregion

        #region Methods
        private void CreateFullTaxonomy()
        {
            fullTaxonomy = string.Format("{0} ({1})", this.taxonomy, this.quantity);
        }

        private void CreateRange()
        {
            if (requiredParams.Count == 0 && optionalParams.Count == 0)
            {
                return;
            }
            range.Clear();
            ranges.Clear();
            if (requiredParams != null)
            {
                foreach (mRequiredParams p in requiredParams)
                {
                    range[p.parameter + " Min"] = 2.2;
                    range[p.parameter + " Max"] = 3.3;
                }
            }

            if (optionalParams != null)
            {
                foreach (mOptionalParams p in optionalParams)
                {
                    range[p.parameter + " Min"] = 4.4;
                    range[p.parameter + " Max"] = 5.5;
                }
            }

            range["Uncertainty"] = 6.6;
            ranges.Add(range);

            //IDictionary<string, object> r = new ExpandoObject();
            //foreach (mRequiredParams p in requiredParams)
            //{
            //    r[string.Format("{0}_Min", p.parameter)] = 5;
            //    r[string.Format("{0}_Max", p.parameter)] = 6;
            //}
            //foreach(mOptionalParams p in optionalParams)
            //{
            //    r[string.Format("{0}_Min", p.parameter)] = 7;
            //    r[string.Format("{0}_Max", p.parameter)] = 8;
            //}
            //r["uncertainty"] = 9;
        }
        #endregion
    }

    public class mRequiredParams : Inpc
    {
        #region Private Fields
        private string _parameter;
        private double _min;
        private double _max;
        #endregion

        #region Constructor

        #endregion

        #region Public Properties
        public string parameter
        {
            get { return _parameter; }
            set
            {
                if (value != _parameter)
                {
                    _parameter = value;
                    OnPropertyChanged("parameter");
                }
            }
        }

        public double min
        {
            get { return _min; }
            set
            {
                if (value != _min)
                {
                    _min = value;
                    OnPropertyChanged("min");
                }
            }
        }

        public double max
        {
            get { return _max; }
            set
            {
                if (value != _max)
                {
                    _max = value;
                    OnPropertyChanged("max");
                }
            }
        }
        #endregion

        #region Methods

        #endregion
    }

    public class mOptionalParams : Inpc
    {
        #region Private Fields
        private string _parameter;
        private double _min;
        private double _max;
        #endregion

        #region Constructor

        #endregion

        #region Public Properties
        public string parameter
        {
            get { return _parameter; }
            set
            {
                if (value != _parameter)
                {
                    _parameter = value;
                    OnPropertyChanged("parameter");
                }
            }
        }

        public double min
        {
            get { return _min; }
            set
            {
                if (value != _min)
                {
                    _min = value;
                    OnPropertyChanged("min");
                }
            }
        }

        public double max
        {
            get { return _max; }
            set
            {
                if (value != _max)
                {
                    _max = value;
                    OnPropertyChanged("max");
                }
            }
        }
        #endregion

        #region Methods

        #endregion
    }

    public class mRange : Inpc
    {
        #region Private Fields
        private double _p0_min;
        private double _p1_min;
        private double _p2_min;
        private double _p3_min;
        private double _p4_min;
        private double _p5_min;
        private double _p6_min;
        private double _p7_min;
        private double _p8_min;
        private double _p9_min;
        private double _p0_max;
        private double _p1_max;
        private double _p2_max;
        private double _p3_max;
        private double _p4_max;
        private double _p5_max;
        private double _p6_max;
        private double _p7_max;
        private double _p8_max;
        private double _p9_max;
        #endregion

        #region Public Properties

        public double p0_min
        {
            get { return _p0_min; }
            set
            {
                if (value != _p0_min)
                {
                    _p0_min = value;
                    OnPropertyChanged("p0_min");
                }
            }
        }
        public double p1_min
        {
            get { return _p1_min; }
            set
            {
                if (value != _p1_min)
                {
                    _p1_min = value;
                    OnPropertyChanged("p1_min");
                }
            }
        }
        public double p2_min
        {
            get { return _p2_min; }
            set
            {
                if (value != _p2_min)
                {
                    _p2_min = value;
                    OnPropertyChanged("p2_min");
                }
            }
        }
        public double p3_min
        {
            get { return _p3_min; }
            set
            {
                if (value != _p3_min)
                {
                    _p3_min = value;
                    OnPropertyChanged("p3_min");
                }
            }
        }
        public double p4_min
        {
            get { return _p4_min; }
            set
            {
                if (value != _p4_min)
                {
                    _p4_min = value;
                    OnPropertyChanged("p4_min");
                }
            }
        }
        public double p5_min
        {
            get { return _p5_min; }
            set
            {
                if (value != _p5_min)
                {
                    _p5_min = value;
                    OnPropertyChanged("p5_min");
                }
            }
        }
        public double p6_min
        {
            get { return _p6_min; }
            set
            {
                if (value != _p6_min)
                {
                    _p6_min = value;
                    OnPropertyChanged("p6_min");
                }
            }
        }
        public double p7_min
        {
            get { return _p7_min; }
            set
            {
                if (value != _p7_min)
                {
                    _p7_min = value;
                    OnPropertyChanged("p7_min");
                }
            }
        }
        public double p8_min
        {
            get { return _p8_min; }
            set
            {
                if (value != _p8_min)
                {
                    _p8_min = value;
                    OnPropertyChanged("p8_min");
                }
            }
        }
        public double p9_min
        {
            get { return _p9_min; }
            set
            {
                if (value != _p9_min)
                {
                    _p9_min = value;
                    OnPropertyChanged("p9_min");
                }
            }
        }
        public double p0_max
        {
            get { return _p0_max; }
            set
            {
                if (value != _p0_max)
                {
                    _p0_max = value;
                    OnPropertyChanged("p0_max");
                }
            }
        }
        public double p1_max
        {
            get { return _p1_max; }
            set
            {
                if (value != _p1_max)
                {
                    _p1_max = value;
                    OnPropertyChanged("p1_max");
                }
            }
        }
        public double p2_max
        {
            get { return _p2_max; }
            set
            {
                if (value != _p2_max)
                {
                    _p2_max = value;
                    OnPropertyChanged("p2_max");
                }
            }
        }
        public double p3_max
        {
            get { return _p3_max; }
            set
            {
                if (value != _p3_max)
                {
                    _p3_max = value;
                    OnPropertyChanged("p3_max");
                }
            }
        }
        public double p4_max
        {
            get { return _p4_max; }
            set
            {
                if (value != _p4_max)
                {
                    _p4_max = value;
                    OnPropertyChanged("p4_max");
                }
            }
        }
        public double p5_max
        {
            get { return _p5_max; }
            set
            {
                if (value != _p5_max)
                {
                    _p5_max = value;
                    OnPropertyChanged("p5_max");
                }
            }
        }
        public double p6_max
        {
            get { return _p6_max; }
            set
            {
                if (value != _p6_max)
                {
                    _p6_max = value;
                    OnPropertyChanged("p6_max");
                }
            }
        }
        public double p7_max
        {
            get { return _p7_max; }
            set
            {
                if (value != _p7_max)
                {
                    _p7_max = value;
                    OnPropertyChanged("p7_max");
                }
            }
        }
        public double p8_max
        {
            get { return _p8_max; }
            set
            {
                if (value != _p8_max)
                {
                    _p8_max = value;
                    OnPropertyChanged("p8_max");
                }
            }
        }
        public double p9_max
        {
            get { return _p9_max; }
            set
            {
                if (value != _p9_max)
                {
                    _p9_max = value;
                    OnPropertyChanged("p9_max");
                }
            }
        }
        #endregion
    }

    //public class mRange : Inpc
    //{
    //    public mRange(List<mRequiredParams> req, List<mOptionalParams> opt)
    //    {
    //        IDictionary<string, object> r = new ExpandoObject();
    //        CreateRange(req, opt, r);
    //    }

    //    private void CreateRange(List<mRequiredParams> req, List<mOptionalParams> opt, dynamic r)
    //    {
    //        if (req != null)
    //        {
    //            foreach (mRequiredParams p in req)
    //            {
    //                r[string.Format("{0}_Min", p.parameter)] = 5;
    //                r[string.Format("{0}_Max", p.parameter)] = 6;
    //            }
    //        }
    //        if (opt != null)
    //        {
    //            foreach (mOptionalParams p in opt)
    //            {
    //                r[string.Format("{0}_Min", p.parameter)] = 7;
    //                r[string.Format("{0}_Max", p.parameter)] = 8;
    //            }
    //        }
    //        r["uncertainty"] = 9;
    //    }
    //}

    //public class mRange : Inpc
    //{
    //    #region Private Fields
    //    private string _uncertainty;
    //    #endregion

    //    #region Constructor

    //    #endregion

    //    #region Public Properties
    //    public string uncertainty
    //    {
    //        get { return _uncertainty; }
    //        set
    //        {
    //            if (value != _uncertainty)
    //            {
    //                _uncertainty = value;
    //                OnPropertyChanged("uncertainty");
    //            }
    //        }
    //    }
    //    #endregion

    //    #region Methods

    //    #endregion
    //}

    //public class mRange
    //{
    //    #region Private Fields
    //    private List<mRequiredParams> _rangeReqParameters;
    //    private List<mOptionalParams> _rangeOptParameters;
    //    #endregion

    //    #region Constructor

    //    #endregion

    //    #region Public Properties

    //    #endregion

    //    #region Methods

    //    #endregion
    //}
}
