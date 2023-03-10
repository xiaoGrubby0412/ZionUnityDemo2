using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Baidu.VR.Zion
{
    public class Player : MonoBehaviour
    {
        public static Player Instance;

        public Transform trackingOriginTransform;
        
        public Transform trackingInterliningTransform;
        
        public Transform hmdTransform;

        private void Awake()
        {
            Instance = this;
        }
    }
}
