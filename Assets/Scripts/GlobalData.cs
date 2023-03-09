using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Baidu.VR.Zion
{
    public class GlobalData: Singleton<GlobalData>
    {
        //当前平台
        public EnumPlatformType Platform;

        //当前用户信息
        public UserInfo CurrentUser;

        //当前玩家
        public NetworkPlayer Me;
    }
}
