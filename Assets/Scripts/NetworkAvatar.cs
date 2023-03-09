using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

namespace Baidu.VR.Zion
{
    public class NetworkAvatar : MonoBehaviour
    {
        public Action<PlayerInfo> OnReceivePlayerInfo;
        public Action<string> OnReceivePlayerFaceData;

        public Transform HeadAnchor;
        public Transform LeftHandAnchor;
        public Transform RightHandAnchor;

        public GameObject headObject;
        public GameObject hairObject;

        public Animator modelAnim;

        public UnityEvent OnStartMove;
        public UnityEvent OnStopMove;

        public UnityEvent OnBindSelf;

        public bool OnFaceChanged = true;

        public string name;

        public void StartMove()
        {
            modelAnim?.SetFloat("speed", 5.0f);
            try { OnStartMove?.Invoke(); } catch (Exception) { }
        }

        public void StopMove()
        {
            modelAnim?.SetFloat("speed", 0.0f);
            try { OnStopMove?.Invoke(); } catch (Exception) { }
        }

        #region name,faces,equipment
        public virtual void ReceivePlayerInfo(PlayerInfo data)
        {
            name = data.Char?.MapObj?.Object?.Name;
            if (OnReceivePlayerInfo != null)
                try { OnReceivePlayerInfo.Invoke(data); } catch (Exception) { }
        }

        public virtual void ReceivePlayerFaceData(string faces)
        {
            if (OnReceivePlayerFaceData != null)
                try { OnReceivePlayerFaceData.Invoke(faces); } catch (Exception) { }
        }
        #endregion
    }
}
