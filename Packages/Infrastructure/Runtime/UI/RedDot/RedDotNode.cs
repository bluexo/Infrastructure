using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Origine
{
    public class RedDotNode
    {
        public RedDotNode Parent { get; private set; }
        public RedDotNode Root { get; private set; }

        public List<RedDotNode> Children { get; private set; } = new List<RedDotNode>();
        public RedDotRef Item { get; private set; }

        public RedDotNode(RedDotRef item)
        {
            Item = item;
            Root = this;
        }

        public RedDotNode AddChild(RedDotRef item)
        {
            var child = Children.Find(c => c.Item == item);
            if (child != null)
                return child;

            var nodeItem = new RedDotNode(item)
            {
                Root = Root,
                Parent = this
            };
            Children.Add(nodeItem);
            return nodeItem;
        }

        public void ForEach(Action<RedDotNode> nodeAction) => ForEach(this, nodeAction);

        private void ForEach(RedDotNode node, Action<RedDotNode> nodeAction = null)
        {
            if (node.Children.Count <= 0)
                return;

            foreach (var child in Children)
            {
                nodeAction?.Invoke(child);
                ForEach(child, nodeAction);
            }
        }
    }
}
