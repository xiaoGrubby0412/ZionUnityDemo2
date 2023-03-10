using System;
using UnityEngine;
using Baidu.VR.Zion;

public class Main : MonoBehaviour
{
    private GameObject playerObj;
    public Vector3 birthPoint = Vector3.zero;

    private void Awake()
    {
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

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
}