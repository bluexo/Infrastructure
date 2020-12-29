using Origine;
using System;
using System.Collections.Generic;

namespace BehaviorTreeViewer
{
    public class Debugger
    {
        public BehaviorTreeElement BehaviorTreeElement;
        public List<NodeDesigner> Nodes = new List<NodeDesigner>();

        /// <summary>
        /// 通过ID查找节点
        /// </summary>
        /// <param name="ID">节点ID</param>
        /// <returns></returns>
        public NodeDesigner FindNodeByID(Guid ID)
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                NodeDesigner node = Nodes[i];
                if (node != null && node.ID == ID)
                    return node;
            }
            return null;
        }
    }
}