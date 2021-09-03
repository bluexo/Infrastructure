using System;
using System.Collections;

using UnityEngine;

namespace Origine
{
    public interface IRedDotSystem
    {
        IEnumerator InitializeAsync(string assetId);

        /// <summary>
        /// 订阅一个值,每秒钟判断一次是否符合给定条件,符合则触发事件,反之清除事件
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="source"></param>
        /// <param name="propertySelector"></param>
        /// <param name="eventName"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        IDisposable ObserveValue<TSource, TProperty>(TSource source, 
            Func<TSource, TProperty> propertySelector, 
            string eventName, 
            Func<TProperty, bool> func) where TSource : class;

        void ClearObserveable();

        void RegisterHandler(object handler);
        void UnregisterHandler(object handler);

        void AddRedDot<T>(T graphic, string eventName) where T : Component;
        void AddRedDot(GameObject graphic, string eventName);

        void RemoveRedDot<T>(T graphic) where T : Component;
        void RemoveRedDot(GameObject graphic);
   
        void PublishEvent(string eventName, int count = 0, bool clickClear = false);
        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="condition">True 发布事件，False 取消事件</param>
        /// <param name="eventName"></param>
        void HandleEvent(bool condition, string eventName, int count = 0);
        void ClearEvent(string eventName);
    }
}
