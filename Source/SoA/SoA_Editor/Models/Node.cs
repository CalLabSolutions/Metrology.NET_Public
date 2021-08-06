﻿using Caliburn.Micro;
using System.Collections.ObjectModel;

namespace SoA_Editor.Models
{
    public interface INode
    {
        string Name { get; set; }
        bool IsExpanded { get; set; }
        NodeType Type { get; set; }
        ObservableCollection<Node> Children { get; set; }
        Node Parent { get; set; }
    }

    public class Node : PropertyChangedBase, INode
    {
        private ObservableCollection<Node> mChildren;

        public NodeType Type
        {
            get { return _type; }
            set { _type = value; NotifyOfPropertyChange(() => Type); }
        }

        private NodeType _type;

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
        public Node Parent { get; set; }

        public Node(Node parent = null)
        {
            mChildren = new ObservableCollection<Node>();
            IsExpanded = false;
            Parent = parent;
        }
    }

    public class TaxonNode : Node
    {
        public TaxonNode()
        {
            Type = NodeType.Taxon;
        }
    }

    public class TechniqueNode : Node
    {
        public TechniqueNode(TaxonNode node)
        {
            Parent = node;
            Type = NodeType.Technique;
        }
    }

    public class AssertionNode : Node
    {
        public AssertionNode(TechniqueNode node)
        {
            Parent = node;
            Type = NodeType.Assertion;
        }
    }

    public class RangeNode : Node
    {
        public RangeNode(AssertionNode node)
        {
            Parent = node;
            Type = NodeType.Range;
        }
    }

    public enum NodeType
    {
        Base, Taxon, Technique, Assertion, Range
    }
}