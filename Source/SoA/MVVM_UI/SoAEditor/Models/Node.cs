using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoAEditor.Models
{
    public class Node : PropertyChangedBase
    {
        private ObservableCollection<Node> mChildren;

        // Add all of the properties of a node here. In this example,
        // all we have is a name and whether we are expanded.
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyOfPropertyChange(() => Name);
                }
            }
        }
        private string _name;

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    NotifyOfPropertyChange(() => IsExpanded);
                }
            }
        }
        private bool _isExpanded;

        // Children are required to use this in a TreeView
        //public IList<Node> Children { get { return mChildren; } set { } }
        public ObservableCollection<Node> Children { get { return mChildren; } set { } }

        // Parent is optional. Include if you need to climb the tree
        // from code. Not usually necessary.
        public Node Parent { get; private set; }

        public Node(Node parent = null)
        {
            mChildren = new ObservableCollection<Node>();
            IsExpanded = true;
            Parent = parent;
        }

         
    }
}
