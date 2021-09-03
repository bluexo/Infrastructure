namespace Origine
{
    /// <summary>
    /// 红点事件参数 
    /// </summary>
    public class RedDotEventArgs : GameEventArgs<RedDotEventArgs>
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 红点被点击后立即清除
        /// </summary>
        public bool ClickClear { get; set; } 

        public override void OnDestroy()
        {
            base.OnDestroy();
            Count = 0;
            Name = string.Empty;
        }
    }
}
