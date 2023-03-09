using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Baidu.VR.Zion
{
    public class PlayerJoystickController : MonoBehaviour
    {
        public AbstractJoystick moveJoystick;
        public float moveSpeed = 5.0f;

        public float moveJoystickLimit = 0.01f;

        public float offset = 2;
        //相机距离的物体的最近和最远距离
        private float minDistance = 0.5f;
        private float maxDistance = 10;
        private float thirdCamMinHeight = 0.2f;

        public float PixelScalingFactor = 5f;
        public float rotateSpeed = 200.0f;
        public float rotSpeed = 10.0f;
        public float headRotationLimit = 90.0f;

        public bool needFixGroundPos = true;
        public bool needCheckWall = true;
        public LayerMask groundLayer;
        public LayerMask wallLayer;
        public float fixPlayerPosYOffset = 0.5f;
        public float fixPlayerPosYMaxDist= 20.0f;

        public bool useGravity = true;
        public float gravityAccel = 9.8f;
        private float gravityCurSpeed = 0.0f;
        private bool onGround = true;

        private Transform _playerTrans = null;
        private Transform playerTrans
        {
            get
            {
                if (_playerTrans == null)
                {
                    _playerTrans = Player.Instance.trackingOriginTransform;
                }
                return _playerTrans;
            }
        }

        private PlayerThirdCamera _playerThirdCamera = null;
        private PlayerThirdCamera playerThirdCamera
        {
            get
            {
                if (_playerThirdCamera == null)
                {
                    _playerThirdCamera = Player.Instance.gameObject.GetComponent<PlayerThirdCamera>();
                }
                return _playerThirdCamera;
            }
        }
        Quaternion cameraYRotate
        {
            get
            {
#if ZION_VR
                if (Player.Instance != null && Player.Instance.hmdTransform != null)
                    return Quaternion.Euler(0.0f, Player.Instance.hmdTransform.rotation.eulerAngles.y, 0.0f);
                else
                    return Quaternion.identity;
#else
                if (playerThirdCamera != null)
                    return Quaternion.Euler(0.0f, playerThirdCamera.rotationY, 0.0f);
                else
                    return Quaternion.identity;
#endif
            }
        }
        public enum UserInteractionOperationSet { MOVE, LOOK, ZOOM };
        public HashSet<UserInteractionOperationSet> opSet = new HashSet<UserInteractionOperationSet>();

        private void Awake()
        {
            SetMoveAll();
            wallLayer = 1 << LayerMask.NameToLayer("Wall");
            opSet.Add(UserInteractionOperationSet.MOVE);
            opSet.Add(UserInteractionOperationSet.LOOK);
            //opSet.Add(UserInteractionOperationSet.ZOOM);
        }

        private void Start()
        {
            if (needFixGroundPos && playerTrans != null)
            {
                Vector3? fixedPos = FixPlayerPosition(playerTrans.position);
                if (fixedPos.HasValue)
                {
                    playerTrans.position = fixedPos.Value;
                }
            }
        }

        public void SetMoveElevatorOnly()
        {
            groundLayer = 1 << LayerMask.NameToLayer("ElevatorGround");
        }

        public void SetMoveAll()
        {
            groundLayer = 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("ElevatorGround");
        }

        private void Update()
        {
#if !ZION_VR
            if (opSet.Contains(UserInteractionOperationSet.ZOOM))
                _Zoom();
            if (opSet.Contains(UserInteractionOperationSet.LOOK))
                _Rotate();
#endif
            if (opSet.Contains(UserInteractionOperationSet.MOVE))
                _Move();
        }

        private bool CheckHitWall(Vector3 oldPos, Vector3 newPos)
        {
            Vector3 movement = newPos - oldPos;
            return Physics.Raycast(oldPos + new Vector3(0.0f, fixPlayerPosYOffset, 0.0f), movement, movement.magnitude, wallLayer);
        }
        private Vector3? FixPlayerPosition(Vector3 pos)
        {
            RaycastHit hit;
            if (Physics.Raycast(pos + new Vector3(0.0f, fixPlayerPosYOffset, 0.0f), Vector3.down, out hit, fixPlayerPosYMaxDist, groundLayer))
            {
                return hit.point;
            }
            return null;
        }

        void _Rotate()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            if (Input.GetMouseButton(1)) //右键按下
            {
                float rx = playerThirdCamera.rotationX;
                float ry = playerThirdCamera.rotationY;
                float axisMouseY = Input.GetAxis("Mouse Y");
                playerThirdCamera.rotationX -= axisMouseY * rotSpeed;
                playerThirdCamera.rotationY += Input.GetAxis("Mouse X") * rotSpeed;
                playerThirdCamera.rotationX = Mathf.Clamp(playerThirdCamera.rotationX, -45, 80);
                if (playerThirdCamera.GetThirdCamera().transform.localPosition.y < thirdCamMinHeight && axisMouseY > 0)
                {
                    playerThirdCamera.rotationX = rx;
                    playerThirdCamera.rotationY = ry;
                }
            }
#endif
        }
        void _Zoom()
        {
            float deltaScroll = Input.GetAxis("Mouse ScrollWheel");//中键滚动缩放
            float ptc_d = playerThirdCamera.Distance;
            playerThirdCamera.Distance = Mathf.Clamp(playerThirdCamera.Distance + deltaScroll * PixelScalingFactor, minDistance, maxDistance);
            if (playerThirdCamera.GetThirdCamera().transform.localPosition.y < thirdCamMinHeight && playerThirdCamera.Distance > ptc_d)
            {
                playerThirdCamera.Distance = ptc_d;
            }
        }
        void _Move()
        {
#if !ZION_VR
            if (playerThirdCamera == null)
                return;
#endif
            float moveHorizontal = 0;
            float moveVertical = 0;
#if ZION_ANDROID || ZION_IOS || ZION_VR
            moveJoystick = FixedJoystick.Instance;
            if (moveJoystick != null)
            {
                moveHorizontal = moveJoystick.Horizontal;
                moveVertical = moveJoystick.Vertical;
            }
#elif ZION_WINDOWS || ZION_OSX
            if (Input.GetKey(KeyCode.W))//按键盘W向上移动
            {
                moveVertical = 1;
            }
            else if (Input.GetKey(KeyCode.S))//按键盘S向下移动
            {
                moveVertical = -1;
            }
            else if (Input.GetKey(KeyCode.A))//按键盘A向左移动
            {
                moveHorizontal = -1;
            }
            else if (Input.GetKey(KeyCode.D))//按键盘D向右移动
            {
                moveHorizontal = 1;
            }
#endif
            if (playerTrans != null && (Mathf.Abs(moveHorizontal) >= moveJoystickLimit || (Mathf.Abs(moveVertical) >= moveJoystickLimit) || (useGravity && !onGround)))
            {
                Vector3 movement = cameraYRotate * (Vector3.right * moveHorizontal + Vector3.forward * moveVertical).normalized;
#if !ZION_VR
                playerTrans.forward = movement;
#endif
                Vector3 newPos = playerTrans.position + movement * moveSpeed * Time.deltaTime;
                if (needCheckWall)
                {
                    if (CheckHitWall(playerTrans.position, newPos))
                    {
                        return;
                    }
                }

                bool appliedManualMove = false;

                if (needFixGroundPos)
                {
                    Vector3? fixedPos = FixPlayerPosition(newPos);
                    if (fixedPos.HasValue)
                    {
                        if (useGravity)
                        {
                            if (playerTrans.position.y <= fixedPos.Value.y)
                            {
                                gravityCurSpeed = 0.0f;
                                playerTrans.position = fixedPos.Value;
                            }
                            else
                            {
                                gravityCurSpeed += Time.deltaTime * gravityAccel;
                                float newY = playerTrans.position.y - gravityCurSpeed * Time.deltaTime;
                                if (newY > fixedPos.Value.y)
                                {
                                    onGround = false;
                                }
                                else
                                {
                                    onGround = true;
                                    newY = fixedPos.Value.y;
                                }
                                playerTrans.position = new Vector3(fixedPos.Value.x, newY, fixedPos.Value.z);
                            }
                        }
                        else
                        {
                            playerTrans.position = fixedPos.Value;
                        }
                        appliedManualMove = true;
                    }
                }
                else
                {
                    playerTrans.position = newPos;
                    appliedManualMove = true;
                }

                if (appliedManualMove)
                {
                    // 用户手动移动
                    //MessageDispatcher.SendMessage("UserManualMove");
                }
            }

        }
    }
}