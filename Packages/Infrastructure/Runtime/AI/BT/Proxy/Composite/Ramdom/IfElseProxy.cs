﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Origine.BT
{
    /// <summary>
    /// 执行第一个节点，
    /// 如果第一个节点成功则执行第二个节点，并将第二个节点结果作为当前的结果返回
    /// 如果第一个节点成功则执行第三个节点，并将第三个节点结果作为当前的结果返回
    /// </summary>
    [CompositeNode("IfElse")]
    public class IfElseProxy : BaseNodeProxy
    {
        private CompositeNode m_CompositeNode;

        public override void OnStart()
        {
            m_CompositeNode = Node as CompositeNode;

            if (m_CompositeNode.Childs.Count != 3)
            {
                m_CompositeNode.Status = NodeStatus.Error;
                return;
            }
        }

        public override void OnUpdate(float deltatime)
        {
            for (int i = m_CompositeNode.RunningNodeIndex; i < m_CompositeNode.Childs.Count; )
            {
                BaseNode childNode = m_CompositeNode.Childs[m_CompositeNode.RunningNodeIndex];
                childNode.Run(deltatime);
                NodeStatus childNodeStatus = childNode.Status;

                if (childNodeStatus == NodeStatus.Running)
                {
                    return;
                }

                if (childNodeStatus == NodeStatus.Error)
                {
                    m_CompositeNode.Status = childNodeStatus;
                    return;
                }

                if (m_CompositeNode.RunningNodeIndex == 0)
                {
                    if (childNodeStatus == NodeStatus.Succeed)
                    {
                        m_CompositeNode.RunningNodeIndex = 1;
                        i = 1;
                    }
                    else if (childNodeStatus == NodeStatus.Failed)
                    {
                        m_CompositeNode.RunningNodeIndex = 2;
                        i = 2;
                    }
                }
                else
                {
                    if (childNodeStatus == NodeStatus.Succeed)
                    {
                        m_CompositeNode.Status = NodeStatus.Succeed;
                        return;
                    }

                    if (childNodeStatus == NodeStatus.Failed)
                    {
                        m_CompositeNode.Status = NodeStatus.Succeed;
                        return;
                    }
                }
            }
        }
    }
}