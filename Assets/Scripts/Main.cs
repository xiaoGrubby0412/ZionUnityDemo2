using System;
using UnityEngine;
using Baidu.VR.Zion;

public class Main : MonoBehaviour
{
    public GameObject playerObj;
    // Start is called before the first frame update

    private void Awake()
    {
        playerObj.transform.localPosition = Vector3.zero;
        playerObj.transform.localScale = Vector3.one;
        playerObj.transform.localRotation = Quaternion.identity;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
