using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Baidu.VR.Zion
{
    public class UIClickHandler: MonoBehaviour, IPointerClickHandler
    {
        public Action<PointerEventData> onClick = null;

        public void OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke(eventData);
        }
    }
}