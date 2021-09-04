﻿using System;
using System.Xml.Serialization;

namespace Origine
{
    public class Transition
    {
        public NodeDesigner FromNode;
        public NodeDesigner ToNode;
    
        public void Set(NodeDesigner toNode, NodeDesigner fromNode)
        {
            this.ToNode = toNode;
            this.FromNode = fromNode;
        }
    }
}
