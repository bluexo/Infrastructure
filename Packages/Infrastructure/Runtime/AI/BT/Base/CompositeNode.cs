using Origine;

using System;
using System.Collections.Generic;

namespace Origine.BT
{
    public class CompositeNode : BaseNode
    {
        public List<BaseNode> Childs { get; set; } = new List<BaseNode>();
        internal int RunningNodeIndex = 0;

        internal void AddChild(BaseNode childNode)
        {
            if (childNode == null || Childs.Contains(childNode))
                return;

            Childs.Add(childNode);
        }

        internal BaseNode GetChild(Guid id)
        {
            for (int i = 0; i < Childs.Count; i++)
            {
                BaseNode baseNode = Childs[i];
                if (baseNode == null)
                    continue;
                if (baseNode.ID == id)
                    return baseNode;
            }
            return null;
        }

        internal BaseNode this[int index]
        {
            get { return Childs[index]; }
        }

        internal override void SetContext(BehaviorTree tree, object context)
        {
            base.SetContext(tree, context);

            for (int i = 0; i < Childs.Count; i++)
            {
                Childs[i]?.SetContext(tree, context);
            }
        }

        internal override void SetProxyCreator(Func<BaseNode, BaseNodeProxy> creator)
        {
            base.SetProxyCreator(creator);

            for (int i = 0; i < Childs.Count; i++)
            {
                Childs[i]?.SetProxyCreator(creator);
            }
        }

        internal override void SetActive(bool active)
        {
            base.SetActive(active);
            for (int i = 0; i < Childs.Count; i++)
            {
                Childs[i].SetActive(active);
            }
        }

        internal override void Reset()
        {
            if (Status <= NodeStatus.Ready)
                return;
            RunningNodeIndex = 0;
            base.Reset();
            for (int i = 0; i < Childs.Count; i++)
            {
                Childs[i].Reset();
            }
        }

        internal override void Destroy()
        {
            if (Status <= NodeStatus.Ready)
                return;
            RunningNodeIndex = 0;
            base.Destroy();
            for (int i = 0; i < Childs.Count; i++)
            {
                Childs[i].Destroy();
            }
        }
    }
}