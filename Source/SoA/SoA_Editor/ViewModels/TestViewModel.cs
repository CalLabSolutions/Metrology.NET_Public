using Caliburn.Micro;
using SoA_Editor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoA_Editor.ViewModels
{
    public class TestViewModel : Screen
    {
        public TestViewModel()
        {
            mRootNodes = new ObservableCollection<Node>();

            // Test data for example purposes
            Node root = new Node() { Name = "Root" };

            Node a = new Node(root) { Name = "Node A" };
            
            root.Children.Add(a);

            Node b = new Node(root) { Name = "Node B" };
            root.Children.Add(b);

            Node c = new Node(b) { Name = "Node C" };
            b.Children.Add(c);

            Node d = new Node(b) { Name = "Node D" };
            b.Children.Add(d);

            Node e = new Node(root) { Name = "Node E" };
            root.Children.Add(e);

            Node f = new Node(e) { Name = "Node F" };
            e.Children.Add(f);

            mRootNodes.Add(root);


            Node foundNode = findNode(root, "Node G");
        }

        public Node findNode(Node root, string findStr)
        {
            if (root.Name.Equals(findStr))
                return root;

            foreach (Node n in root.Children)
            {
                Node result = findNode(n, findStr);
                if (result != null)
                    return result;
            }

            return null;
        }


        private ObservableCollection<Node> mRootNodes;

        public IEnumerable<Node> RootNodes { get { return mRootNodes; } }
        /*
        public void show(System.Windows.Controls.TextBlock lbl)
        {

        }
        */
        public void show(Object obj)
        {
           
        }
    }
}