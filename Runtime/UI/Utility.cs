using System;

using UnityEngine;

namespace Origine
{
    /// <summary>
    /// 实用函数集。
    /// </summary>
    public static partial class Utility
    {
        public static GameObject FindByTitleCase(this GameObject obj, string name)
        {
            return Find(obj, name.TitleCase());
        }

        public static T FindByTitleCase<T>(this Transform transform, string name) where T : UnityEngine.Component
        {
            return Find<T>(transform, name.TitleCase());
        }

        public static T FindByTitleCase<T>(this GameObject obj, string name) where T : UnityEngine.Component
        {
            return Find<T>(obj, name.TitleCase());
        }

        public static GameObject Find(this GameObject obj, string name)
        {
            if (obj == null || string.IsNullOrEmpty(name))
                return null;
            Transform trans = FindByName(obj.transform, name);
            if (trans != null)
                return trans.gameObject;
            else
                return null;
        }


        public static T Find<T>(this Transform transform, string name) where T : UnityEngine.Component
        {
            Transform trans = FindByName(transform, name);
            if (trans != null)
                return trans.GetComponent<T>();

            return null;
        }

        public static T Find<T>(this GameObject obj, string name) where T : UnityEngine.Component
        {
            if (null == obj || string.IsNullOrEmpty(name))
                return null;

            return Find<T>(obj.transform, name);
        }

        public static Transform FindByName(Transform trans, string name)
        {
            if (trans == null)
                return null;

            if (trans.name == name)
                return trans;

            return FindInChild(trans, name);
        }

        public static Transform FindInChild(this Transform trans, string name)
        {
            if (trans == null)
                return null;

            Transform tempTrans = trans.Find(name);
            if (tempTrans != null)
                return tempTrans;

            for (int i = 0; i < trans.childCount; i++)
            {
                Transform child = trans.GetChild(i);
                Transform temp = FindInChild(child, name);
                if (temp != null)
                    return temp;
            }

            return null;
        }

        public static void SetActive(this GameObject go, bool active)
        {
            if (go == null)
                return;
            if (go.activeSelf != active)
                go.SetActive(active);
        }

        public static void SetActive<T>(this T instance, bool active) where T : UnityEngine.Component
        {
            if (instance == null)
                return;
            SetActive(instance.gameObject, active);
        }

        public static void SetLayer(this GameObject go, int layer, bool includeChild = true)
        {
            if (null == go)
                return;
            go.layer = layer;

            if (!includeChild) return;

            for (int i = 0, max = go.transform.childCount; i < max; ++i)
                SetLayer(go.transform.GetChild(i).gameObject, layer, includeChild);
        }

        public static void ResetTransform(this Transform transform)
        {
            if (null == transform)
                return;

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
    }
}
