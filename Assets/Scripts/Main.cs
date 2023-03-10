using System;
using UnityEngine;
using Baidu.VR.Zion;

public class Main : MonoBehaviour
{
    private GameObject playerObj;

    private void Awake()
    {
        playerObj = GameObject.Instantiate(Resources.Load("Player")) as GameObject;
        playerObj.transform.localPosition = Vector3.zero;
        playerObj.transform.localScale = Vector3.one;
        playerObj.transform.localRotation = Quaternion.identity;
        
        GameObject mainUI = GameObject.Instantiate(Resources.Load("MainUiPanelCanvas")) as GameObject;
#if !UNITY_EDITOR
        #if UNITY_ANDROID || UNITY_IOS
            GameObject JoyStickCanvas = GameObject.Instantiate(Resources.Load("JoyStick/JoystickCanvas") as GameObject);
        #endif
#endif
    }

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
}