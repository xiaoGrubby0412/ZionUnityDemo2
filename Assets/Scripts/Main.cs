using System;
using UnityEngine;
using Baidu.VR.Zion;

public class Main : MonoBehaviour
{
    private GameObject playerObj;
    public Vector3 birthPoint = Vector3.zero;
    public static Main Instance;

    private void Awake()
    {
        Instance = this;
        playerObj = GameObject.Instantiate(Resources.Load("Player")) as GameObject;
        playerObj.transform.position = birthPoint;
        playerObj.transform.localScale = Vector3.one;
        playerObj.transform.rotation = Quaternion.identity;
        
        GameObject.Instantiate(Resources.Load("MainUiPanelCanvas"));
#if !UNITY_EDITOR
        #if UNITY_ANDROID || UNITY_IOS
            GameObject JoyStickCanvas = GameObject.Instantiate(Resources.Load("JoyStick/JoystickCanvas") as GameObject);
        #endif
#endif
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}