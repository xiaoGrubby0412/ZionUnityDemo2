using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

namespace Baidu.VR.Zion
{
    public class NetworkPlayer
    {
        public NetworkPlayer(ulong networkId)
        {
            NetworkId = networkId;
            HashId = "";
        }

        public NetworkPlayer(ulong networkId, string hashId)
        {
            NetworkId = networkId;
            HashId = hashId;
        }

        public readonly ulong NetworkId;
        public readonly string HashId;

        public string name;
        public string headIcon;
        public string faces;
        public long? nAppealNO;
        public long? nSitNO;
        public uint facesVersion;
        public uint? faceModeId;
        public string equipment;
        public bool Connected;
        public ulong UniqueReliableMessageIdCounter { get; private set; }

        public ulong AvatarStaticId;
        public NetworkAvatar Avatar;

        public EnumPlatformType Platform = EnumPlatformType.VR;

        public Vector3 CurrentPosition { get; set; }
        public Vector3 CurrentDirection { get; set; }

        //移动速度
        public float MoveSpeed = 2.0f;

        public ulong GetNextReliableId() { return ++UniqueReliableMessageIdCounter; }

        const float MaxMoveSynDuration = 0.5f;

        //根据faceModeId 加载默认脸部数据, 根据equipment 加载换装数据
        public void ReceivePlayerInfo(PlayerInfo data)
        {
            try { Avatar?.ReceivePlayerInfo(data); } catch (Exception) { }
        }

        //更新捏脸玩家脸部数据
        public void ReceivePlayerFaceData(string faces)
        {
            try { Avatar?.ReceivePlayerFaceData(faces); } catch (Exception) { }
        }

        public void SendStartMove(Vector3 TargetPosition) {
            //var Message = new Global.Message.MovePlayer();
            //Message.playerId = NetworkId;
            //Message.rotation = CurrentDirection;
            //Message.from = CurrentPosition;
            //Message.position = TargetPosition;

            //MessageDispatcher.SendMessageData(Global.Message.Type.MovePlayer.ToString(), Message);
        }

        public void ReceiveMoveData(PlayerWalkData data)
        {
            Vector3 startPosition = (Avatar != null)? Avatar.gameObject.transform.position:data.StartPos;
            Vector3 targetPosition = data.EndPos;
            Vector3 targetDirection = data.Direction;

            float duration = Mathf.Min(MaxMoveSynDuration, Vector3.Distance(startPosition, targetPosition) / MoveSpeed);

            MoveTo(targetPosition, targetDirection, duration);
        }

        public void OnDestroy()
        {
            if (AvatarMoveTweener != null)
            {
                if (AvatarMoveTweener.active)
                    AvatarMoveTweener.Kill();
                AvatarMoveTweener = null;
            }

            if (Avatar != null)
            {
                UnityEngine.Object.Destroy(Avatar.gameObject);
            }
        }

        private void MoveTo(Vector3 TargetPosition, Vector3 TargetDirection, float duration)
        {
            if (AvatarMoveTweener != null)
            {
                AvatarMoveTweener.Kill();
                AvatarMoveTweener = null;
            }

            if (Avatar != null)
            {
                AvatarMoveTweener = DOTween.Sequence();
                AvatarMoveTweener.Append(Avatar.transform.DOMove(TargetPosition, duration).SetEase(Ease.Linear));
                AvatarMoveTweener.Join(Avatar.transform.DORotate(TargetDirection, duration).SetEase(Ease.Linear));
                AvatarMoveTweener.onUpdate += OnAvatarMoveUpdate;
                AvatarMoveTweener.onComplete += OnAvatarMoveComplete;
            }
        }

        private Sequence AvatarMoveTweener;

        private void OnAvatarMoveUpdate()
        {
            if (Avatar != null)
            {
                CurrentPosition = Avatar.transform.position;
                CurrentDirection = Avatar.transform.rotation.eulerAngles;
            }
        }

        private void OnAvatarMoveComplete()
        {
            if(Avatar != null)
            {
                CurrentPosition = Avatar.transform.position;
                CurrentDirection = Avatar.transform.rotation.eulerAngles;
            }
        }

        public bool GetNAppealNO(out long _va) {

            if (nAppealNO.HasValue)
            {
                _va = (long)nAppealNO.Value;
            }
            else {
                _va = 0;
            }
            return nAppealNO.HasValue;
        }
    }
}
