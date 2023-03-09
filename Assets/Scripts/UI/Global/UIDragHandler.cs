using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Baidu.VR.Zion
{
    public class UIDragHandler : MonoBehaviour, IDragHandler, IBeginDragHandler,IEndDragHandler
    {
        public Action<PointerEventData> onBeginDrag = null;
        public Action<PointerEventData> onDrag = null;
        public Action<PointerEventData> onEndDrag = null;

        public void OnBeginDrag(PointerEventData eventData)
        {
            onBeginDrag?.Invoke(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            onDrag?.Invoke(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            onEndDrag?.Invoke(eventData);
        }
    }
}