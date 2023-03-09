using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Baidu.VR.Zion
{
    public class SelfPlayerBinder_MobilePhone : MonoBehaviour
    {
        public float UpdatePositionInverval = 0.5f;
        public float UpdatePoseInterval = 0.5f;

        NetworkPlayer Me;
        Player player;

        bool Moving
        {
            get { return _moving; }
            set
            {
                if(value != _moving)
                {
                    _moving = value;
                    if(Me.Avatar != null)
                    {
                        if (_moving)
                            Me.Avatar.StartMove();
                        else
                            Me.Avatar.StopMove();
                    }
                }
            }
        }
        bool _moving = false;

        private Vector3 lastWorldPosition = Vector3.zero;
        private Quaternion lastWorldRotation = Quaternion.identity;

        private Vector3 lastLocalPosition = Vector3.zero;
        private Quaternion lastLocalRotation = Quaternion.identity;
        private Transform lastParent = null;

        // Start is called before the first frame update
        void Start()
        {
            Me = GlobalData.Instance.Me;
            player = GetComponent<Player>();

            if (player != null)
            {
                lastWorldPosition = player.trackingOriginTransform.position;
                lastWorldRotation = player.trackingOriginTransform.rotation;

                lastLocalPosition = player.trackingOriginTransform.localPosition;
                lastLocalRotation = player.trackingOriginTransform.localRotation;
                lastParent = player.trackingOriginTransform.parent;

                if (Me != null && Me.Avatar != null)
                {
                    Transform avatarTransform = Me.Avatar.transform;
                    avatarTransform.SetParent(player.transform);
                    avatarTransform.localPosition = Vector3.zero;
                    avatarTransform.localScale = Vector3.one;
                    avatarTransform.localRotation = Quaternion.identity;

                    NetworkAvatar avatar = Me.Avatar;
                    if (avatar != null)
                    {
                        SkinnedMeshRenderer[] smrs = avatar.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                        foreach (SkinnedMeshRenderer smr in smrs)
                        {
                            smr.updateWhenOffscreen = true;
                        }
                        if (player.hmdTransform != null && avatar.HeadAnchor != null)
                        {
                            avatar.HeadAnchor.SetParent(player.hmdTransform);
                            avatar.HeadAnchor.localPosition = Vector3.zero;
                            avatar.HeadAnchor.localScale = Vector3.one;
                            avatar.HeadAnchor.localRotation = Quaternion.identity;
                        }

                        if (avatar.LeftHandAnchor != null)
                        {
                            Destroy(avatar.LeftHandAnchor.gameObject);
                            avatar.LeftHandAnchor = null;
                        }

                        if (avatar.RightHandAnchor != null)
                        {
                            Destroy(avatar.RightHandAnchor.gameObject);
                            avatar.RightHandAnchor = null;
                        }

                        avatar.OnBindSelf?.Invoke();
                    }
                }
            }
        }

        private void OnEnable()
        {
            InvokeRepeating("UpdatePosition", UpdatePositionInverval, UpdatePositionInverval);
            InvokeRepeating("UpdatePose", UpdatePoseInterval, UpdatePoseInterval);
        }

        private void OnDisable()
        {
            CancelInvoke("UpdatePosition");
            CancelInvoke("UpdatePose");
        }

        private void UpdatePosition()
        {
            if (player != null)
            {
                Vector3 position = player.trackingOriginTransform.position;
                Quaternion rotation = player.trackingOriginTransform.rotation;

                if (position != lastWorldPosition
                    || rotation != lastWorldRotation)
                {
                    if (Me != null)
                    {
                        Me.CurrentDirection = rotation.eulerAngles;
                        Me.SendStartMove(position);
                    }

                    lastWorldPosition = position;
                    lastWorldRotation = rotation;

                    Moving = true;
                }
                else
                    Moving = false;

                //挂在别的物件下时特殊判断
                Transform parent = player.trackingOriginTransform.parent;
                if (parent != null)
                {
                    Vector3 localPosition = player.trackingOriginTransform.localPosition;
                    Quaternion localRotation = player.trackingOriginTransform.localRotation;
                    if (lastParent != parent)
                    {
                        Moving = false;
                        lastParent = parent;
                    }
                    else
                    {
                        if (localPosition != lastLocalPosition
                            || localRotation != lastLocalRotation)
                        {
                            lastLocalPosition = localPosition;
                            lastLocalRotation = localRotation;

                            Moving = true;
                        }
                        else
                            Moving = false;
                    }
                }
            }
        }
    }
}
