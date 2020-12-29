
using System;

namespace Origine.BT
{
    public abstract class BaseNode
    {
        #region
        public Guid ID { get; private set; }
        public NodeData NodeData { get; private set; }
        public object Context { get; private set; }
        public BaseNodeProxy NodeProxy { get; private set; }
        public ProxyData ProxyData { get; private set; }
        public BehaviorTree Tree { get; private set; }

        public NodeStatus Status { get; set; }
        public string ClassType { get; private set; }
        public NodeType NodeType { get; set; }
        public bool Active { get; set; }
        #endregion

        #region 数据相关

        internal virtual void SetData(NodeData data)
        {
            NodeData = data;
            ID = NodeData.ID;
            ClassType = data.ClassType;
        }

        internal virtual void SetContext(BehaviorTree tree, object context)
        {
            Tree = tree;
            Context = context;
        }

        internal void SetProxyData(ProxyData proxyData)
        {
            ProxyData = proxyData;
        }

        internal virtual void SetProxyCreator(Func<BaseNode, BaseNodeProxy> creator)
        {
            NodeProxy = creator(this);
        }

        #endregion

        #region 外部调用

        /// <summary>
        /// 生命周期驱动函数
        /// </summary>
        /// <param name="deltatime">帧时间</param>
        internal virtual void Run(float deltatime)
        {
            if (Status == NodeStatus.Error)
            {
                return;
            }

            if (Status == NodeStatus.None)
            {
                Status = NodeStatus.Ready;
                OnAwake();
            }

            if (Status == NodeStatus.Ready)
            {
                Status = NodeStatus.Running;
                SetActive(true);
                OnStart();
            }

            if (Active && Status == NodeStatus.Running)
            {
                OnUpdate(deltatime);
            }
        }

        internal virtual void SetActive(bool active)
        {
            if (Status < NodeStatus.Running)
                return;

            if (Active == active)
                return;

            Active = active;

            if (active) OnEnable();
            else OnDisable();
        }

        internal virtual void Reset()
        {
            if (Status < NodeStatus.Running)
                return;

            SetActive(false);
            Status = NodeStatus.Ready;
            OnReset();
        }

        internal virtual void Destroy()
        {
            if (Status < NodeStatus.Ready)
                return;

            SetActive(false);
            OnDestroy();
            Context = null;
            NodeProxy = null;
        }

        #endregion

        #region 生命周期函数

        /// <summary>
        /// 数据初始化
        /// </summary>
        internal virtual void OnAwake()
        {
            NodeProxy?.OnAwake();
        }

        /// <summary>
        /// 节点激活
        /// </summary>
        internal virtual void OnEnable()
        {
            NodeProxy?.OnEnable();
        }

        /// <summary>
        /// 节点暂停
        /// </summary>
        internal virtual void OnDisable()
        {
            NodeProxy?.OnDisable();
        }

        /// <summary>
        /// 节点进入时执行OnStart
        /// </summary>
        internal virtual void OnStart()
        {
            NodeProxy?.OnStart();
        }

        /// <summary>
        /// 状态NodeStatus=Running 时每帧执行OnUpdate
        /// </summary>
        internal virtual void OnUpdate(float deltatime)
        {
            NodeProxy?.OnUpdate(deltatime);
        }

        /// <summary>
        /// 节点重置执行OnReset
        /// </summary>
        internal virtual void OnReset()
        {
            NodeProxy?.OnReset();
        }

        /// <summary>
        /// 节点退出执行OnDestroy
        /// </summary>
        internal virtual void OnDestroy()
        {
            NodeProxy?.OnDestroy();
        }

        #endregion
    }
}