using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Baidu.VR.Zion
{
    public class PlayerThirdCamera : MonoBehaviour
    {
        private float _min_distance = 1f;
        private float _max_distance = 10f;
        public bool ForceOverrideDistance = false;

        public float MIN_PLAYERTHIRDCAMERA_DISTANCE
        {
            get { return _min_distance; }
            set
            {
                if (_distance < value)
                    _distance = value;
                _min_distance = value;
            }
        }

        public float MAX_PLAYERTHIRDCAMERA_DISTANCE
        {
            get { return _max_distance; }
            set
            {
                if (_distance > value)
                    _distance = value;
                _max_distance = value;
            }
        }

        private float _distance = 2.0f;

        public float Distance
        {
            get { return _distance; }
            set
            {
                if (ForceOverrideDistance)
                {
                    _distance = value;
                }
                else
                {
                    _distance = value < MIN_PLAYERTHIRDCAMERA_DISTANCE ? MIN_PLAYERTHIRDCAMERA_DISTANCE : Mathf.Clamp(value, MIN_PLAYERTHIRDCAMERA_DISTANCE, MAX_PLAYERTHIRDCAMERA_DISTANCE);
                }
            }
        }
        public float rotationX = 0.0f;
        public float rotationY = 0.0f;

        private float rotationXLimit = 90.0f;

        public Camera thirdCamera = null;

        private void Awake()
        {

        }

        private void OnDestroy()
        {

        }

        private void LateUpdate()
        {
            if (Player.Instance.hmdTransform != null && thirdCamera != null)
            {
                rotationX = Mathf.Clamp(rotationX, -rotationXLimit, rotationXLimit);
                thirdCamera.transform.rotation = Quaternion.Euler(rotationX, rotationY, 0.0f);
                thirdCamera.transform.position = Player.Instance.hmdTransform.position + thirdCamera.transform.rotation * Vector3.back * _distance;
            }
        }

        public Camera GetThirdCamera()
        {
            return thirdCamera;
        }
    }
}
