using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Baidu.VR.Zion
{
    public class PlayerJoystickController : MonoBehaviour
    {
        public bool CanRotateAndMove = true;
        public AbstractJoystick moveJoystick;
        public GPUSkinningPlayerMono mono;
        public float moveSpeed;
        public float moveJoystickLimit;

        public float offset;

        //相机距离的物体的最近和最远距离
        public float minDistance;
        public float maxDistance;
        public float thirdCamMinHeight;

        public float PixelScalingFactor;
        public float rotateSpeed;
        public float rotSpeed;
        public float headRotationLimit;

        public float fixedTime;
        public bool needFixGroundPos;
        public bool needCheckWall;
        public bool needCheckWater;
        public LayerMask groundLayer;
        public LayerMask wallLayer;
        public LayerMask waterLayer;
        public float fixPlayerPosYOffset;
        public float fixPlayerPosForward;
        public float fixPlayerPosYMaxDist;
        public float fixPlayerPosGround;

        public bool useGravity;
        public float gravityAccel;
        public float gravityCurSpeed;
        public bool onGround;

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
                if (playerThirdCamera != null)
                    return Quaternion.Euler(0.0f, playerThirdCamera.rotationY, 0.0f);
                else
                    return Quaternion.identity;
            }
        }

        private void Start()
        {
            this.CanRotateAndMove = true;
            this.moveSpeed = 10.0f;
            this.moveJoystickLimit = 0.01f;
            this.offset = 2;
            //相机距离的物体的最近和最远距离
            this.minDistance = 0.5f;
            this.maxDistance = 10;

            this.thirdCamMinHeight = 0.2f;
            this.PixelScalingFactor = 5f;
            this.rotateSpeed = 200.0f;
            this.rotSpeed = 10.0f;
            this.headRotationLimit = 90.0f;
            this.wallLayer = 1 << LayerMask.NameToLayer("Wall") | 1 << LayerMask.NameToLayer("Ground");
            this.groundLayer = 1 << LayerMask.NameToLayer("Ground");
            this.waterLayer = 1 << LayerMask.NameToLayer("Water");
            this.needCheckWall = true;
            this.needFixGroundPos = true;
            this.needCheckWater = false;
            this.fixedTime = 0.02f;
            this.fixPlayerPosYOffset = 1.5f;
            this.fixPlayerPosForward = 0.5f;
            this.fixPlayerPosYMaxDist = 30f;
            this.fixPlayerPosGround = 1f;
            this.gravityAccel = 980f;
            this.gravityCurSpeed = 0.0f;
            this.onGround = true;
            this.useGravity = true;

#if !UNITY_EDITOR
        #if UNITY_ANDROID || UNITY_IOS
            this.moveJoystick = FixedJoystick.Instance;
        #endif
#endif
            Transform unitTransform = Player.Instance.transform;
            if (this.needFixGroundPos && unitTransform != null)
            {
                Vector3? fixedPos = this.FixPlayerPosition(unitTransform.position);
                if (fixedPos.HasValue)
                {
                    unitTransform.position = fixedPos.Value;
                }
            }
        }


        private void Update()
        {
            this.Move();
            this._Rotate();
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

        private bool CheckHitWall(Vector3 oldPos, Vector3 newPos)
        {
            Vector3 dir = (newPos - oldPos).normalized;
            Vector3 p0 = oldPos + new Vector3(0.0f, this.fixPlayerPosYOffset, 0.0f);
            RaycastHit hit;
            if (Physics.Raycast(p0, dir, out hit,
                    this.fixPlayerPosForward, this.wallLayer))
            {
                return true;
            }
            else
            {
                Transform unit = Player.Instance.transform;
                Vector3 p = unit.position + unit.forward * this.fixPlayerPosForward;
                Vector3 oPos = p + new Vector3(0.0f, this.fixPlayerPosYOffset, 0.0f);

                Vector3 oPosL = oPos + -unit.right * this.moveSpeed * this.fixedTime;
                Vector3 dirL = (oPosL - p0).normalized;
                if (Physics.Raycast(p0, dirL, out hit, this.fixPlayerPosForward, this.wallLayer))
                {
                    return true;
                }
                else
                {
                    Vector3 oPosR = oPos + unit.right * this.moveSpeed * this.fixedTime;
                    Vector3 dirR = (oPosR - p0).normalized;
                    if (Physics.Raycast(p0, dirR, out hit, this.fixPlayerPosForward, this.wallLayer))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private Vector3 FixNewPos(Vector3 oldPos, Vector3 newPos)
        {
            Vector3 dir = newPos - oldPos;
            Vector3 p0 = oldPos + new Vector3(0.0f, this.fixPlayerPosYOffset, 0.0f);
            RaycastHit hit;
            if (Physics.Raycast(p0, dir, out hit,
                    dir.magnitude, this.wallLayer))
            {
                //Debug.LogError("in FixNewPos return hit.point == " + hit.point);
                return hit.point - dir.normalized * 0.5f;
            }

            return newPos;
        }

        private bool CheckHitWater(Vector3 pos)
        {
            RaycastHit hit;
            if (Physics.Raycast(pos + new Vector3(0.0f, this.fixPlayerPosYOffset, 0.0f), Vector3.down, out hit,
                    this.fixPlayerPosYMaxDist, this.waterLayer))
            {
                //Debug.Log("hit water.point ----------- " + hit.point);
                return true;
            }

            return false;
        }

        private Vector3? FixPlayerPosition(Vector3 pos)
        {
            Vector3 oPos = pos + new Vector3(0.0f, this.fixPlayerPosYOffset, 0.0f);

            RaycastHit hit;
            if (Physics.Raycast(oPos, Vector3.down, out hit,
                    this.fixPlayerPosYMaxDist, this.groundLayer))
            {
                RaycastHit hitF;
                Vector3 oPosF = oPos + Player.Instance.transform.forward * this.moveSpeed *
                    this.fixedTime;
                if (Physics.Raycast(oPosF, Vector3.down, out hitF,
                        this.fixPlayerPosYMaxDist, this.groundLayer))
                {
                    float c = hitF.point.y - hit.point.y;
                    if (c > this.fixPlayerPosGround)
                    {
                        //Debug.LogError("cccc == " + c + "PointF == " + hitF.point + " Point == " + hit.point);
                        return hitF.point;
                    }
                    else
                    {
                        Vector3 oPosL = oPos + -Player.Instance.transform.right * this.moveSpeed *
                            this.fixedTime;
                        if (Physics.Raycast(oPosL, Vector3.down, out hitF,
                                this.fixPlayerPosYMaxDist, this.groundLayer))
                        {
                            //Debug.LogError("lllllllll");
                            c = hitF.point.y - hit.point.y;
                            if (c > this.fixPlayerPosGround)
                            {
                                //Debug.LogError("cccc == " + c + "PointL == " + hitF.point + " Point == " + hit.point);
                                return hitF.point;
                            }
                            else
                            {
                                Vector3 oPosR = oPos + Player.Instance.transform.right * this.moveSpeed *
                                    this.fixedTime;
                                if (Physics.Raycast(oPosR, Vector3.down, out hitF,
                                        this.fixPlayerPosYMaxDist, this.groundLayer))
                                {
                                    //Debug.LogError("rrrrrrrrr");
                                    c = hitF.point.y - hit.point.y;
                                    if (c > this.fixPlayerPosGround)
                                    {
                                        //Debug.LogError("cccc == " + c + "PointR == " + hitF.point + " Point == " + hit.point);
                                        return hitF.point;
                                    }
                                }
                            }
                        }
                    }
                }

                return hit.point;
            }


            return null;
        }

        private bool CheckFixedPos(Vector3 newPos, Vector3 fixedPos)
        {
            Vector3 p0 = new Vector3(newPos.x, newPos.y + this.fixPlayerPosYOffset, newPos.z);
            Vector3 p1 = new Vector3(fixedPos.x, fixedPos.y + this.fixPlayerPosYOffset, fixedPos.z);
            RaycastHit hit;
            Vector3 dir = p1 - p0;
            if (Physics.Raycast(p0, dir, out hit, dir.magnitude, this.wallLayer))
            {
                return false;
            }

            if (Physics.Raycast(p1, -dir, out hit, dir.magnitude, this.wallLayer))
            {
                return false;
            }

            return true;
        }


        public void StartMove()
        {
            this.mono.Player.CrossFade("run", 0.1f);
        }

        public void StopMove()
        {
            this.mono.Player.CrossFade("idle", 0.1f);
        }

        // Global.Unit移动控制器，传值给Camera.Unit，拆解相机与控制的强耦合
        private void Move()
        {
            if (this.mono == null)
            {
                //self.modelAnim = GlobalComponent.Instance.Unit.GetComponentInChildren<Animator>();
                this.mono = Player.Instance.GetComponentInChildren<GPUSkinningPlayerMono>();
            }

            if (this.mono == null || this.mono.Player == null)
            {
                return;
            }

            // 速度大于 2.6 奔跑，0.1~2.6之间行走


            float gravityCurSpeed = 0.0f;
            float moveHorizontal = 0;
            float moveVertical = 0;

            Transform unitTransform = Player.Instance.transform;

#if UNITY_EDITOR || UNITY_WII

            if (Input.GetKey(KeyCode.W)) //按键盘W向上移动
            {
                this.StartMove();
                moveVertical = 1;
            }
            else if (Input.GetKey(KeyCode.S)) //按键盘S向下移动
            {
                this.StartMove();
                moveVertical = -1;
            }
            else if (Input.GetKey(KeyCode.A)) //按键盘A向左移动
            {
                this.StartMove();
                moveHorizontal = -1;
            }
            else if (Input.GetKey(KeyCode.D)) //按键盘D向右移动
            {
                this.StartMove();
                moveHorizontal = 1;
            }
            else
            {
                this.StopMove();
            }

#elif UNITY_ANDROID || UNITY_IOS
            if (this.moveJoystick != null)
            {
                moveHorizontal = this.moveJoystick.Horizontal;
                moveVertical = this.moveJoystick.Vertical;

                if (gravityCurSpeed != 0 || moveHorizontal != 0)
                {
                    this.StartMove();
                }
                else
                {
                    this.StopMove();
                }
            }
#endif


            bool ifJoy = Mathf.Abs(moveHorizontal) >= this.moveJoystickLimit ||
                         (Mathf.Abs(moveVertical) >= this.moveJoystickLimit);

            if (unitTransform != null && (ifJoy || (this.useGravity && !this.onGround)))
            {
                Vector3 movement = Vector3.zero;

                if (ifJoy)
                {
                    movement = cameraYRotate
                               * (Vector3.right * moveHorizontal + Vector3.forward * moveVertical).normalized;
                    // Debug.Log("movement ============== " + movement);

                    unitTransform.forward = movement;

                    //Debug.Log("playerTrans.forward ========" + unitTransform.forward);   
                }

                Vector3 newPos = unitTransform.position + movement * this.moveSpeed * Time.deltaTime;
                // Debug.Log("newPos ---------- " + newPos);

                if (this.needCheckWall && ifJoy)
                {
                    if (this.CheckHitWall(unitTransform.position, newPos))
                    {
                        return;
                    }

                    float len = (newPos - unitTransform.position).magnitude;
                    if (len > this.fixPlayerPosForward)
                    {
                        newPos = this.FixNewPos(unitTransform.position, newPos);
                    }

                    // RaycastHit hit;
                    // Vector3 uPos, dPos;
                    // Vector3 oPos = newPos + new Vector3(0.0f, self.fixPlayerPosYOffset, 0.0f);
                    // if (Physics.Raycast(oPos, -unitTransform.up, out hit, self.fixPlayerPosYMaxDist, self.groundLayer))
                    // {
                    //     dPos = hit.point;
                    //     if (Physics.Raycast(dPos, unitTransform.up, out hit, self.fixPlayerPosYMaxDist, self.wallLayer))
                    //     {
                    //         uPos = hit.point;
                    //         if ((uPos - dPos).magnitude <= self.fixPlayerPosYOffset)
                    //         {
                    //             return;
                    //         }
                    //     }
                    // }
                }

                if (this.needFixGroundPos)
                {
                    Vector3? fixedPos = this.FixPlayerPosition(newPos);
                    // Debug.Log("fixedPos ----------- " + fixedPos);
                    if (fixedPos.HasValue)
                    {
                        if (this.CheckFixedPos(newPos, fixedPos.Value) == false)
                        {
                            return;
                        }

                        if (this.useGravity)
                        {
                            if (unitTransform.position.y <= fixedPos.Value.y)
                            {
                                unitTransform.position = fixedPos.Value;
                                this.onGround = true;
                                // Debug.Log("[playerTrans.position] -------- " + unitTransform.position);
                            }
                            else
                            {
                                gravityCurSpeed += Time.deltaTime * this.gravityAccel;
                                float newY = unitTransform.position.y - gravityCurSpeed * Time.deltaTime;
                                // Debug.Log("newY ------------- " + newY);

                                if (newY > fixedPos.Value.y)
                                {
                                    this.onGround = false;
                                }
                                else
                                {
                                    this.onGround = true;
                                    newY = fixedPos.Value.y;
                                }

                                unitTransform.position = new Vector3(fixedPos.Value.x, newY, fixedPos.Value.z);
                                // Debug.Log("[playerTrans.position] -------- " + unitTransform.position);
                            }
                        }
                        else
                        {
                            unitTransform.position = fixedPos.Value;
                            // Debug.Log("[playerTrans.position] -------- " + unitTransform.position);
                        }
                        //appliedManualMove = true;
                    }
                    else
                    {
                        //unitTransform.position = newPos;
                        //appliedManualMove = true;
                    }
                }
                else
                {
                    unitTransform.position = newPos;
                    //appliedManualMove = true;
                }
            }
        }
    }
}