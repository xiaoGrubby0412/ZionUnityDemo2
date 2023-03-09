using Baidu.VR;
using Baidu.VR.Zion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GesturePanel : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    private Touch oldTouch1;  //上次触摸点1(手指1)
    private Touch oldTouch2;  //上次触摸点2(手指2)
    public float RotationFactor = 0.3f;
    public float PixelScalingFactor = 0.01f;
    private bool isDragingJoystick = false;
    enum JoyStickDraggingTouch { NoTouch, Touch0, Touch1 };
    private JoyStickDraggingTouch joyStickDraggingTouch = JoyStickDraggingTouch.NoTouch;
    bool pointerDown = false;
    bool pointerDownFirstFrame = false;

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
    public enum UserInteractionOperationSet { MOVE, LOOK, ZOOM };
    public HashSet<UserInteractionOperationSet> opSet = new HashSet<UserInteractionOperationSet>();

    private void Start()
    {
        if (playerThirdCamera == null)
        {
            Debug.LogError("[Zion] LookPanel or playerThirdCamera Missing");
            return;
        }
        opSet.Add(UserInteractionOperationSet.MOVE);
        opSet.Add(UserInteractionOperationSet.LOOK);
        //opSet.Add(UserInteractionOperationSet.ZOOM);
    }
    void LateUpdate()
    {
        if (!pointerDown) return;
        //没有触摸，就是触摸点为0
        bool tempPointerDownFirstFrame = false;
        if (pointerDownFirstFrame)
        {
            tempPointerDownFirstFrame = true;
            pointerDownFirstFrame = false;
        }
        if (Input.touchCount <= 0) return;
        //Debug.Log("[Zion] touchCount" + Input.touchCount.ToString());
        if (Input.touchCount == 1)//单点触摸
        {
            Touch touch = Input.GetTouch(0);
            //是否在拖动摇杆
            bool isFirstTouchLandedInJoystickArea = touch.position.x < (0.33f * Screen.width) && touch.phase == TouchPhase.Began;
            bool isTouchEnded = touch.phase == TouchPhase.Ended;
            if (isFirstTouchLandedInJoystickArea) { joyStickDraggingTouch = JoyStickDraggingTouch.Touch0; } else if (isTouchEnded) { joyStickDraggingTouch = JoyStickDraggingTouch.NoTouch; }
            //若没有拖动摇杆且单手操作，视作转向
            if (joyStickDraggingTouch == JoyStickDraggingTouch.NoTouch) { _Rotate(touch); }//单点触摸， 水平上下旋转
            else { return; }
        }
        else if (Input.touchCount == 2)//多点触摸
        {
            Touch newTouch0 = Input.GetTouch(0);
            Touch newTouch1 = Input.GetTouch(1);
            //是否在拖动摇杆
            bool isFirstTouchLandedInJoystickArea0 = ((newTouch0.phase == TouchPhase.Began) || tempPointerDownFirstFrame) && newTouch0.position.x < (0.33f * Screen.width);
            bool isFirstTouchLandedInJoystickArea1 = ((newTouch1.phase == TouchPhase.Began) || tempPointerDownFirstFrame) && newTouch1.position.x < (0.33f * Screen.width);
            bool isTouchEnded = (newTouch0.phase == TouchPhase.Ended) && (newTouch1.phase == TouchPhase.Ended);
            if (isFirstTouchLandedInJoystickArea0) { joyStickDraggingTouch = JoyStickDraggingTouch.Touch0; }
            else if (isFirstTouchLandedInJoystickArea1) { joyStickDraggingTouch = JoyStickDraggingTouch.Touch1; }
            else if (isTouchEnded) { joyStickDraggingTouch = JoyStickDraggingTouch.NoTouch; }
            //若没有拖动摇杆且双手操作，视作缩放
            if (opSet.Contains(UserInteractionOperationSet.ZOOM) && joyStickDraggingTouch == JoyStickDraggingTouch.NoTouch) {
                _Zoom(newTouch0, newTouch1);
            }//多点触摸, 放大缩小
            //若左手拖动摇杆且右手同时操作，右手视作转向，反之同理
            else if (opSet.Contains(UserInteractionOperationSet.LOOK) && joyStickDraggingTouch == JoyStickDraggingTouch.Touch0)
            {
                _Rotate(newTouch1);
            }
            else if (joyStickDraggingTouch == JoyStickDraggingTouch.Touch1)
            {
                _Rotate(newTouch0);
            }
            else { Debug.LogWarning("[Zion] unKnown JoyStickDraggingTouch #: " + joyStickDraggingTouch.ToString()); }

        }
        else { Debug.LogWarning("[Zion] unKnown touchCount #: " + Input.touchCount.ToString()); return; }
    }

    void _Rotate(Touch touch)
    {
        Vector2 delta = touch.deltaPosition;
        //Debug.Log("[Zion] OnDrag " + delta.ToString());
        delta.x = delta.x * RotationFactor;
        delta.y = delta.y * RotationFactor;
        playerThirdCamera.rotationX += -delta.y;
        playerThirdCamera.rotationY += delta.x;
        //Debug.Log("[Zion] playerThirdCamera.rotationX " + playerThirdCamera.rotationX.ToString());
        playerThirdCamera.rotationX = playerThirdCamera.rotationX < 0 ? Mathf.Clamp(playerThirdCamera.rotationX, -45, 0) : Mathf.Clamp(playerThirdCamera.rotationX, 0, 80);//将旋转值限制在极限值以内
    }
    void _Zoom(Touch newTouch1, Touch newTouch2)
    {
        //第2点刚开始接触屏幕, 只记录，不做处理
        if (newTouch2.phase == TouchPhase.Began)
        {
            oldTouch2 = newTouch2;
            oldTouch1 = newTouch1;
            return;
        }
        //计算老的两点距离和新的两点间距离，变大要放大模型，变小要缩放模型
        float oldDistance = Vector2.Distance(oldTouch1.position, oldTouch2.position);
        float newDistance = Vector2.Distance(newTouch1.position, newTouch2.position);
        //两个距离之差，为正表示放大手势， 为负表示缩小手势
        float offset = newDistance - oldDistance;
        //放大因子， 一个像素按 0.01倍来算(100可调整)
        float scaleFactor = -offset * PixelScalingFactor;
        Debug.Log("[Zion] TouchZoomOffset " + offset.ToString());
        playerThirdCamera.Distance += scaleFactor;
        //将缩放值限制在极限值以内
        //记住最新的触摸点，下次使用
        oldTouch1 = newTouch1;
        oldTouch2 = newTouch2;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pointerDown = false;
        joyStickDraggingTouch = JoyStickDraggingTouch.NoTouch;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDown = true;
        pointerDownFirstFrame = true;
    }
}
