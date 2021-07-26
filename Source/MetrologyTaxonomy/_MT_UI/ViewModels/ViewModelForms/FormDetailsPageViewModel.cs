using MT_DataAccessLib;
using MT_UI.Pages;
using MT_UI.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Types = MT_UI.Services.Converters.Types;

namespace MT_UI.ViewModels.ViewModelForms
{
    class FormDetailsPageViewModel : Notifier
    {
        public FormDetailsPageViewModel()
        {
            Definition = Form.TaxonToSave.Definition;
            quantitiesDic = UomDataSource.getQuantities();
            // parse out taxon name
            parsing = true;
            if (Form.TaxonToSave.Name.Length > 0)
                ParseTaxon(Form.TaxonToSave);
            else
                Form.TaxonToSave.Name = "TestProcess.";
            parsing = false;
        }

        #region properties

        private bool parsing = false;

        private string quantityStr;
        public string QuantityStr
        {
            get { return quantityStr; }
            set
            {
                quantityStr = value;
                BuildTaxonName();
                OnPropertyChanged("QuantityStr");
            }
        }

        private string process;
        public string Process
        {
            get { return process; }
            set
            {
                process = value;
                BuildTaxonName();
                OnPropertyChanged("Process");
            }
        }


        private string definition;
        public string Definition
        {
            get { return definition; }
            set
            {
                definition = value;
                Form.TaxonToSave.Definition = definition;
                OnPropertyChanged("Definition");
            }
        }

        private Types types;
        public Types Types
        {
            get { return types; }
            set
            {
                types = value;
                TypeStr = value.ToString() + ".";
                OnPropertyChanged("Types");
               
            }
        }

        private string typeStr;
        public string TypeStr
        {
            get { return typeStr; }
            set
            {
                typeStr = value;
                BuildTaxonName();
                OnPropertyChanged("TypeStr");
            }
        }

        private Dictionary<string, UomDataSource.Quantity> quantitiesDic;

        private ObservableCollection<Quantity> quantities = new ObservableCollection<Quantity>();
        public ObservableCollection<Quantity> Quantities
        {
            get
            {
                if (quantities.Count == 0)
                {
                    foreach (KeyValuePair<string, UomDataSource.Quantity> entry in quantitiesDic)
                    {
                        // Format for Display      
                        if (entry.Value != null)
                            quantities.Add(Quantity.FormatUomQuantity(entry.Value));
                    }
                }                
                return quantities;
            }
        }

        private Quantity selectedQuantity;
        public Quantity SelectedQuantity
        {
            get { return selectedQuantity; }
            set
            {
                if (value == null) return;
                selectedQuantity = value;
                QuantityStr = value.QuantitiyName + ".";
                OnPropertyChanged("SelectedQuantity");
            }
        }

        #endregion

        #region tools        
        
        // Get the elements of the taxon from the name and set the xaml elmements up accordingly
        private void ParseTaxon(Taxon taxon)
        {
            // set types
            if (taxon.Name.ToLower().Contains("measure"))
            {
                Types = Types.Measure;
            }

            if (taxon.Name.ToLower().Contains("source"))
            {
                Types = Types.Source;
            }

            try
            {
                int firstDot = taxon.Name.IndexOf(".", 1) + 1;
                int secondDot = taxon.Name.IndexOf(".", firstDot) + 1;
                int thirdDot = taxon.Name.IndexOf(".", secondDot) + 1;

                var qname = taxon.Name.Substring(secondDot, (thirdDot - secondDot) -1);

                qname = qname.Replace(" ", "-").ToLower();
                var UomQuantity = UomDataSource.getQuantity(qname);
                if (UomQuantity != null) SelectedQuantity = Quantity.FormatUomQuantity(UomQuantity);

                Process = taxon.Name.Substring(thirdDot);
            }
            catch { }
        }

        private void BuildTaxonName()
        {
            if (!parsing)
            {
                string name = "TestProcess.";
                if (TypeStr != "" && TypeStr != null)
                {
                    name += TypeStr;
                }
                if (QuantityStr != "" && QuantityStr != null)
                {
                    name += QuantityStr;
                }
                if (Process != "" && Process != null)
                {
                    name += Process;
                }
                Form.TaxonToSave.Name = name;
            }            
        }

        #endregion
    }

    public class Quantity
    {
        private string baseName;
        public string BaseName
        {
            get { return baseName; }
            set { baseName = value; }
        }

        private string quantitiyName;
        public string QuantitiyName
        {
            get { return quantitiyName; }
            set { quantitiyName = value; }
        }

        public static Quantity FormatUomQuantity(UomDataSource.Quantity quantity)
        {
            var bname = quantity.UoM.name;
            var bnameArr = bname.Split("-");
            if (bnameArr.Length > 0)
            {

                for (int i = 0; i < bnameArr.Length; i++)
                {
                    bnameArr[i] = bnameArr[i][0].ToString().ToUpper() + bnameArr[i].Substring(1);
                }
                bname = string.Join(" ", bnameArr);
            }
            else
            {
                bname = bname[0].ToString().ToUpper() + bname.Substring(1);
            }

            var qname = quantity.name;
            var qnameArr = qname.Split("-");
            if (qnameArr.Length > 0)
            {
                for (int i = 0; i < qnameArr.Length; i++)
                {
                    qnameArr[i] = qnameArr[i][0].ToString().ToUpper() + qnameArr[i].Substring(1);
                }
                qname = string.Join("-", qnameArr);
            }
            else
            {
                qname = qname[0].ToString().ToUpper() + qname.Substring(1);
            }

            return new Quantity()
            {
                BaseName = bname,
                QuantitiyName = qname
            };
        }
    }
}
