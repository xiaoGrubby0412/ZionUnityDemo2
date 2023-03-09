using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Baidu.VR.Zion
{
    public class NetworkObject : MonoBehaviour
    {
        public Action<MapObjectInfo, ulong> OnReceiveData;
        public Action<List<ClientFunction>> OnReceiveClientFunction;
        public Action<Vector3, Quaternion> OnApplyTransform;
        public Action<Vector3> OnApplyScale;
        public Action<string> OnReceiveInteractionUpd;
        public Action<string> OnReceiveIntimacyDoUpdSrc;
        public Action<string> OnReceiveIntimacyDoUpdDst;
        
        public ulong Id;
        public ulong StaticId;

        public Vector3 Position { 
            get { return position; } 
            set {
                position = value;
                SyncTransform();
            } 
        }
        public Quaternion Rotation {
            get { return rotation; }
            set {
                rotation = value;
                SyncTransform();
            } 
        }


        public ulong OwnerId { get; set; }

        public bool Inited { get; set; }

        protected Vector3 position = Vector3.zero;
        protected Quaternion rotation = Quaternion.identity;
        protected float localscale = 1;

        private bool NeedSendTransform = false;

        [NonSerialized]
        public bool IsStatic = true;


        public virtual void ReceiveInteractionUpd(string cmd)
        {
            if (OnReceiveInteractionUpd != null)
                try { OnReceiveInteractionUpd.Invoke(cmd); } catch (Exception) { }
        }

        public virtual void ReceiveIntimacyDoUpdSrc(string actName)
        {
            if (OnReceiveIntimacyDoUpdSrc != null)
                try { OnReceiveIntimacyDoUpdSrc.Invoke(actName); } catch (Exception) { }
        }

        public virtual void ReceiveIntimacyDoUpdDst(string actName)
        {
            if (OnReceiveIntimacyDoUpdDst != null)
                try { OnReceiveIntimacyDoUpdDst.Invoke(actName); } catch (Exception) { }
        }

        public virtual void ReceiveClientFunctions(List<ClientFunction> clientFunctions)
        {
            if(OnReceiveClientFunction != null)
                try { OnReceiveClientFunction.Invoke(clientFunctions); } catch (Exception) { }
        }

        protected void SyncTransform()
        {
            ApplyTransform();
            NeedSendTransform = true;
        }

        protected virtual void ApplyTransform()
        {
            if (OnApplyTransform != null)
                try { OnApplyTransform.Invoke(Position, Rotation); } catch (Exception) { }
            else
            {
                transform.SetPositionAndRotation(Position, Rotation);
            }
        }

        protected void ApplyScale()
        {
            if (OnApplyScale != null)
                try { OnApplyScale.Invoke(new Vector3(localscale, localscale, localscale)); } catch (Exception) { }
            else
            {
                transform.localScale = new Vector3(localscale, localscale, localscale);
            }
        }

        protected static string CreateMoveNpcCmd(Vector3 position, Vector3 direction)
        {
            return string.Format(@"#moveNpc {0} {1} {2} {3} {4} {5}",
                position.x, position.y, position.z,
                direction.x, direction.y, direction.z);
        }
    }
}
