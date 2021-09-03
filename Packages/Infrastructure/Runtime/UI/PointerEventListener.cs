
using System;
using System.Collections;

using UnityEngine;
using UnityEngine.EventSystems;

namespace Origine
{
    public class PointerEventListener : MonoBehaviour,
        IPointerClickHandler,
        IPointerDownHandler,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerUpHandler
    {
        public Action<PointerEventData> onClick;
        public Action<PointerEventData> onDown;
        public Action<PointerEventData> onEnter;
        public Action<PointerEventData> onExit;
        public Action<PointerEventData> onUp;

        static public PointerEventListener GetOrAdd(GameObject go)
        {
            if (!go.TryGetComponent(out PointerEventListener listener))
                listener = go.AddComponent<PointerEventListener>();
            return listener;
        }

        public void OnPointerClick(PointerEventData eventData) => onClick?.Invoke(eventData);

        public void OnPointerDown(PointerEventData eventData) => onDown?.Invoke(eventData);

        public void OnPointerEnter(PointerEventData eventData) => onEnter?.Invoke(eventData);

        public void OnPointerExit(PointerEventData eventData) => onExit?.Invoke(eventData);

        public void OnPointerUp(PointerEventData eventData) => onUp?.Invoke(eventData);

        public void Coroutine(IEnumerator routine) => StartCoroutine(routine);
    }

}