using System.Collections;
using System.Collections.Generic;
using Baidu.VR.Zion;
using UnityEngine;
using UnityEngine.UI;

public class MainUIPanel : MonoBehaviour
{
    public Button btn;
    // Start is called before the first frame update
    void Start()
    {
        btn.onClick.AddListener(() =>
        {
            Player.Instance.transform.position = Main.Instance.birthPoint;
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
