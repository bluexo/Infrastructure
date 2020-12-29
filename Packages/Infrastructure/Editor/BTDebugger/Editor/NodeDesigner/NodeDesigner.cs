using Origine;
using Origine.BT;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTreeViewer
{
    public class NodeDesigner
    {
        public BaseNode baseNode;

        public NodeData NodeData;
        //节点位置
        public Rect Rect;
        //子节点
        public List<Transition> Transitions = new List<Transition>();

        public System.Guid ID => NodeData.ID;
    }
}
