using SOA_DataAccessLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoAEditor.Models
{
    class TreeView_helper
    {
        public static String getNodeType(String nodeName, Soa SampleSOA)
        {


            for (int processTypeIndex = 0; processTypeIndex < SampleSOA.CapabilityScope.Activities[0].ProcessTypes.Count(); processTypeIndex++)
            {
                if (nodeName.ToUpper().Equals(SampleSOA.CapabilityScope.Activities[0].ProcessTypes[processTypeIndex].name.ToUpper()))
                    return "taxonomy";
            }

            for (int techniqueIndex = 0; techniqueIndex < SampleSOA.CapabilityScope.Activities[0].Techniques.Count(); techniqueIndex++)
            {
                if (nodeName.ToUpper().Equals(SampleSOA.CapabilityScope.Activities[0].Techniques[techniqueIndex].name.ToUpper()))
                    return "technique";
            }



            for (int rangeIndex = 0; rangeIndex < SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases.Count(); rangeIndex++)
            {
                for (int assertIndex = 0; assertIndex < SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases[0].Assertions.Count(); assertIndex++)
                {
                    if (nodeName.ToUpper().Equals(SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases[rangeIndex].Assertions[assertIndex].Value.ToUpper()))
                        return "range";
                }
            }

            return "";
        }

        //gets the index of the selected assertion node in the assertion list from the xml file
        public static int getAssertionNodeIndex(String nodeName, Soa SampleSOA)
        {

            for (int rangeIndex = 0; rangeIndex < SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases.Count(); rangeIndex++)
            {
                for (int assertIndex = 0; assertIndex < SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases[0].Assertions.Count(); assertIndex++)
                {
                    if (nodeName.ToUpper().Equals(SampleSOA.CapabilityScope.Activities[0].Templates[0].CMCUncertaintyFunctions[0].Cases[rangeIndex].Assertions[assertIndex].Value.ToUpper()))
                        return assertIndex;
                }
            }

            return -1;

        }

        //public static Node getNodeObject(String nodeName, ObservableCollection<Node> rootNodes)
        //{                        
        //    return findNode(nodeName, rootNodes[0]);
        //}

        ////recursive function to find the node in the node tree
        //private static Node findNode(string nodeName, Node node)
        //{
        //    if (node.Name == nodeName)
        //        return node;
        //    else if (node.Children != null)
        //    {
        //        foreach (Node n in node.Children.ToList())
        //        {
        //            findNode(nodeName, n);
        //        }
        //    }

        //    return null;
        //}

       
    }
}