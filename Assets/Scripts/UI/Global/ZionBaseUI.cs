using Baidu.VR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Baidu.VR.Zion.Global.Event.UI
{
    public enum UIMode
    {
        Panel,
        Window,
        Tips,
        Suspension
    }

    public enum VRUILocationType
    { 
        Location2Dof,
        Location3Dof,
        Location6Dof
    }

    public class ZionBaseUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public bool hasModel = true;
        public UIMode uiMode = UIMode.Panel;
        public UnityEvent onOutsideClick = new UnityEvent();
        public VRUILocationType VRUiLocationType = VRUILocationType.Location2Dof;
        public bool isSocketPanel = false;
        private bool isPointerInside = false;
        public void OnPointerEnter(PointerEventData eventData)
        {
            isPointerInside = true;
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerInside = false;
        }
        // Start is called before the first frame update
        void Start()
        {

        }
        // Update is called once per frame
        void Update()
        {
            //if (MotionController.Instance.GetButtonDown(KeyEnums.Select) && isPointerInside)
            //{
            //    OutSideClick();
            //}
        }
        public virtual void OutSideClick()
        {
            onOutsideClick.Invoke();
        }
    }
}