using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace soa_1_03
{
    [Serializable]
    public sealed class objTaxonomy
    {
        private List<taxonomyQuantity> _quantities;

        #region Constructor
        //private constructor
        private objTaxonomy()
        {
            this._quantities = new List<taxonomyQuantity>();
            string exePath = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase)).LocalPath;
            string filePath = exePath + @"\Quantity_Database.xml";
            bool result = CheckForQuantityDatabase(filePath);
            //if (!result)
            //{
                GetUniqueQuantityNames();
                this._quantities.Sort((x, y) => x.quantityName.CompareTo(y.quantityName));
                SerializeQuantitiesObjectToFile(filePath, this);
            //}

        }
        #endregion

        #region Singleton
        //Private static instance of the same class
        private static readonly objTaxonomy instance = null;

        static objTaxonomy()
        {
            instance = new objTaxonomy();
        }

        public static objTaxonomy GetInstance()
        {
            //return existing instance
            return instance;
        }
        #endregion

        public List<taxonomyQuantity> quantities
        {
            get { return _quantities; }
            set { _quantities = value; }
        }

        private void GetUniqueQuantityNames()
        {
            UoM myUom = UoM.GetInstance();

            foreach(basenames bn in myUom._lBasenames)
            {
                foreach(string qty in bn.quantities)
                {
                   List<string> currentQtyNames = new List<string>();
                   foreach(taxonomyQuantity tq in this._quantities)
                    {
                        currentQtyNames.Add(tq.quantityName);
                    }

                    bool containsQty = currentQtyNames.Contains(qty);
                    if (!containsQty)
                    {
                        taxonomyQuantity newTQ = new taxonomyQuantity();
                        newTQ.quantityName = qty;
                        taxonomyNames newTN = new taxonomyNames();
                        newTN.baseAltName = bn.name;
                        newTN.taxonomyNamesSymbols.Add (bn.symbol);
                        foreach(string s in bn.aliases)
                        {
                            newTN.taxonomyNamesSymbols.Add(s);
                        }
                        newTQ.baseAltNames.Add(newTN);
                        foreach (alternatives alt in bn.alts)
                        {
                            taxonomyNames newAltTN = new taxonomyNames();
                            newAltTN.baseAltName = alt.altName;
                            newAltTN.taxonomyNamesSymbols.Add(alt.altSymbol);
                            foreach(string s in alt.altAliases)
                            {
                                newAltTN.taxonomyNamesSymbols.Add(s);
                            }
                            newTQ.baseAltNames.Add(newAltTN);
                        }
                        this._quantities.Add(newTQ);

                    }

                }
            }
        }

        private bool CheckForQuantityDatabase(string filePath)
        {
            bool result = false;
            if (File.Exists(filePath)) { result = true; }
            return result;
        }

        private void SerializeQuantitiesObjectToFile(string filePath, objTaxonomy tax)
        {
            using (var writer = new StreamWriter(filePath))
            {
                XmlSerializer serializer = new XmlSerializer(tax.GetType());
                serializer.Serialize(writer, tax);
                writer.Flush();
                var xml = writer.ToString();
            }
        }
    }

    public class taxonomyQuantity
    {
        public List<taxonomyNames> baseAltNames;
        public string quantityName;

        public taxonomyQuantity()
        {
            this.baseAltNames = new List<taxonomyNames>();
        }
    }

    public class taxonomyNames
    {
        public List<string> taxonomyNamesSymbols;
        public string baseAltName;

        public taxonomyNames()
        {
            this.taxonomyNamesSymbols = new List<string>();
        }
    }
}
