using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Baidu.VR.Zion
{
    public enum EnumPlatformType{
        VR,
        MobilePhone,
        PC,
    }

    public enum NetworkErrorCode
    {
        Success = 0,        //成功

        Protobuff_Err = 1, //协议解析失败
        SvrNoReady = 2, //服务器没有准备好
        Connect_Err = 3, //账号连接信息找不到
        Exception_Err = 4, //异常错误
        Back_Protobuff_Err = 5, //回包时 版本转换协议解析失败	
        UserEntering = 6, //用户正在进入（同一帐号同时登录时，可能会返回的错误）

        Login_HasOnline = 1001, //账号登录中
        Login_NameTooLong = 1002, //账号名过长
        Login_Bduss_Err = 1003, //Passport登录失败
        Login_Forbid = 1004, //禁止登录
        Login_PassErr = 1005, //登录密码错误
        Login_CommonError = 1006, //登录异常错误
        Login_UserName = 1007, //用户名非法
        Check_Version_E = 1008,//版本检测异常
        NO_PASSPORT_LOGIN = 1009,//正式环境只支持PASSPORT登录
        DB_Err_Connect = 1010, //连接数据库失败
        DB_Err_Select = 1011, //数据库查询失败
        DB_Err_Operation = 1012, //数据库操作失败
        Login_DENY_IP = 1013,//ip禁止登录
        Login_DENY_USERID = 1014,//用户禁止登录
        Login_DENY_HWID = 1015,//设备禁止登录
        Char_Add_NameErr = 1020, //角色名不合法
        Char_Add_NameRepeat = 1021, //角色名重复
        Char_Add_NumLmt = 1022, //角色数量超上限
        Char_Add_SexErr = 1023, //角色性别错误
        Char_Add_StaticErr = 1024, //角色StaticId错误
        Char_Add_UserErr = 1025, //账号ID错误
        User_No_Logined = 1030, //账号不是登录完成阶段
        Char_NotExist = 1031, //角色不存在
        Char_EnterRepeat = 1032, //重复登录
        Player_NoFind = 1040, //查找玩家失败
        Interaction_CmdErr = 1041, //交互协议命令处理异常
        HasOnline = 1042, //角色登录中
        CharIdError = 1043, //角色ID错误	
        NoMoreItem = 1044, //没有更多的项了（一般用于分页的请求）
        SrvNotFound = 1045, //服务器未找到
        FilterText = 1046, //文本中包含敏感词
        NotAvailableSrv = 1047, //没有可用的服务器
        NoFindNpcRefID = 1048, //没有找到npc的刷新信息
        OwnerHasPick = 1049, //重复拾取
        CanNotPick = 1050,   //npc不可以被拾取
        CacheSaveEmpty = 1051, //保存一个cache对象时，无法在cache中找到此对象
        CacheCreateExisted = 1052, //创建一个cache对象时，对象已经存在
        CacheNotExisted = 1053, //从cache读取对象时，对象不存在
        CacheCloseSaving = 1054, //保存或读取对象时，对象正在被closesave
        ServerTooBusy = 1055, //服务器过于繁忙
        NotFoundGameObject = 1056, //未找到GameObject
        GameObjectAlreadyExisted = 1057,  //GameObject已存在
        InvalidProxySession = 1058, // 无效的proxy session
        InvalidQueueToken = 1059, // 无效的queue token
        SrvFull = 1060,// 服务器已满，无法提供服务
        NetLockInUsing = 1061, // 网络锁已被占用
        NotFoundBlockImage = 1062, // 地块镜像未找到
        BlockImageFull = 1063, // 地块镜像已满
        FriendApplyNotExisted = 1064, // 好友申请不存在
        RoleNotExisted = 1065, // 角色不存在
        AlreadyFriends = 1066,// 已经是好友
        FriendNotExisted = 1067, // 好友不存在
        ItemNumNotEnough = 1068, // 物品数量不足
        PackSpaceNotEnough = 1069, // 背包空间不足
        MoneyNumNotEnough = 1070, // 金钱数量不足
        InvalidParam = 1071, // 请求参数错误
        RedisException = 1072, // redis执行异常
        RedisBadData = 1073, // redis数据错误，人为误修改了redis数据？
        PhoneRoomUserLimit = 1074, // 单个电话房间人数超上限了
        PhoneRoomNumLimit = 1075, // 电话房间总数超上限了
        PhoneAlreadyInRoom = 1076, // 用户已经电话中，无法继续发起新的通话
        PhoneRoomNotExist = 1077, // 房间已不存在
        PhoneRoomIllegalAccess = 1078, // 房间非法，该房间都没邀请这个角色
        AreaNotFind = 1079, // 区域没有找到
        AreaNoHaveVarMgr = 1080, // 区域没有属性管理器
        AreaReadVarErr = 1081, // 区域读取属性失败
        ScriptVarMsgErr = 1082, // 属性信息不完成
        ObyTypeNoFind = 1083, // 角色类型不存在
        ObyTypeErr = 1084, // 角色类型错误
        VoiceTypeNotSupport = 1085, // 语音类型不支持
        RalRequest = 1086, // ral请求错误
        VoiceUserBan = 1087, // 语音被禁言了
        CacheLoadErr = 1088, // cache加载失败
        NotifyMsgTypeNotSupport = 1089, // 消息通知类型不支持
        QueueUserFullLimit = 1090,// 排队人数已到上限
        HttpRequestCreate = 1091, // 创建http请求体错误
        HttpDoRequest = 1092, // 执行http请求错误
        HttpResponseStatus = 1093, // 返回http非200错误
        HttpResponseJsonParse = 1094, // 返回http的body解析json失败
        HttpResponseLogicCode = 1095, // 返回http逻辑code错误
        HttpResponseLogicData = 1096, // 返回http逻辑数据错误
        RecoveryAlreadyExisted = 1097, // recovery已经存在
        RecoveryNotExisted = 1098, // recovery不存在
        PhoneRoleNotExisted = 1099, // A给B打电话时，B已下线

        // world server 和 position server
        Reg_Pos_Out_Of_Range = 1100, // 向position server注册失败：位置超出范围
        Unsupported_Obj = 1101, // 不支持的对象，具体含义跟使用场景关联
        Reg_Pos_Obj_Exists = 1102, // 向position server注册失败：对象已经存在
        Cell_Not_Exists = 1103, // 向position server注册失败：cell不存在
        Block_Img_Exists = 1104, // 镜像已存在
        Block_Img_Init_Failed = 1105, // 镜像初始化失败
        Msg_Res_Timeout = 1106, // 消息回应超时
        Server_ID_Is_Zero = 1107, // 服务ID为0
        Invalid_IP_Address = 1108, // IP地址无效
        Server_Connect_Failed = 1109, // 服务器连接失败
        Space_Not_Exists = 1110, // 空间不存在
        Conn_ID_Is_Zero = 1111, // 连接ID为0
        Static_Npc_Not_Exists = 1112, // 静态NPC信息不存在
        Load_Cache_Failed = 1113, // 加载cache失败
        Load_Cache_Zero = 1114, // 加载cache条数为0
        ServerConfigError = 1115, // 服务配置错误
        HttpResponseError = 1116, // http响应错误
        LeaveScnPosUnInit = 1117, // 离开场景坐标没有初始化
        TransferDistanceErr = 1118, // 传送时检测距离失败
        TransferParamsErr = 1119, // 传送检测参数数量错误
        GetCurSpaceErr = 1120, // 获取当前空间错误
        InteractionParamErr = 1121, // 交互参数错误
        ClientCmdTypeErr = 1122, // 客户端交互类型错误
        RiskControl = 1123, // 命中风控
        GlobalVarListEmpty = 1124, // GlobalVar LIST POP时，已经没有元素

        CollectActionParamsErr = 1125, // 采集参数错误
        CollectTryCollect = 1126, // 采集失败
        AddItemFail = 1127, // 添加物品失败
        ChgMoneyFail = 1128, // 修改金钱失败
        NoFindObj = 1140, // 没有找到对象
        GetSeatErr = 1141, // 获取位置信息错误
        SetSeatErr = 1142, // 设置位置信息错误
        CollectMax = 1143, // 您采集的贝壳已达上限，请去别处逛逛吧！
        Trigger_Time_Err = 1144, // 触发时间异常


        LocalError = 30001,

        InvalidInputData,       //输入数据无效
        InvalidResData,         //返回数据无效
        NullResponse,           //返回为空
        Timeout,                //超时
        NullNetworker,          //无网络连接
    }

    public class VarTable
    {
        public bool ForceAll = false;

        public object GetVar(string key)
        {
            if (StringVars != null && StringVars.TryGetValue(key, out object ret))
                return ret;
            else
                return null;
        }

        public object GetVar(long key)
        {
            if (StringVars != null && NumberVars.TryGetValue(key, out object ret))
                return ret;
            else
                return null;
        }

        public object GetVar(float key)
        {
            if (StringVars != null && FloatVars.TryGetValue(key, out object ret))
                return ret;
            else
                return null;
        }

        public object GetVar(bool key)
        {
            if (StringVars != null && BooleanVars.TryGetValue(key, out object ret))
                return ret;
            else
                return null;
        }

        public Dictionary<string, object> StringVars = new Dictionary<string, object>();
        public Dictionary<long, object> NumberVars = new Dictionary<long, object>();
        public Dictionary<float, object> FloatVars = new Dictionary<float, object>();
        public Dictionary<bool, object> BooleanVars = new Dictionary<bool, object>();

        public List<object> DeletedVarKeys = new List<object>();
    }

    public class VarObj
    {
        public VarObj() { }
        public VarObj(object var, bool toOwnerClient, bool toOtherClient, bool toDb)
        {
            Var = var;
            ToOwnerClient = toOwnerClient;
            ToOtherClient = toOtherClient;
            ToDb = toDb;
        }

        public bool? ToOwnerClient = true;
        public bool? ToOtherClient = true;
        public bool? ToDb = true;

        public object Var = null;
    }

    public class VarPool
    {
        public bool ForceAll = true;
        public List<string> DelVars = new List<string>();

        public object GetVar(string name)
        {
            if (AllVars != null 
                && AllVars.TryGetValue(name, out VarObj obj) && obj != null)
            {
                return obj.Var;
            }
            else
                return null;
        }

        public Dictionary<string, VarObj> AllVars = new Dictionary<string, VarObj>();
    }

    public class ObjectInfo
    {
        public ulong InstanceId = 0;
        public string Name = null;
        public VarPool Pool = null;
    }
    
    public class TransformInfo
    {
        public TransformInfo() { }
        public TransformInfo(Vector3 pos, Vector3 dir)
        {
            Position = pos;
            Direction = dir;
        }

        public Vector3 Position;
        public Vector3 Direction;
    }

    public class MapObjectInfo
    {
        public ObjectInfo Object;

        public Vector3 Position { get { return TransInfo == null ? Vector3.zero:TransInfo.Position; } }
        public Vector3 Direction { get { return TransInfo == null ? Vector3.zero : TransInfo.Direction; } }

        public TransformInfo TransInfo;
        public Dictionary<int, TransformInfo> SubTransInfo;

        public ulong WorldId;
        public ulong SpaceId;
    }

    public class ClientFunction
    {
        public string strName;
        public string strDesc;
        public string strType;
        public string strTriggerType;
        public string strTriggerParam;
        public string strAction;
        public string strActionParam;
        public string strWhiteList;
        public string strParam;
    }
    
    public class CharInfo
    {
        public MapObjectInfo MapObj;
        public int Gender;
        public ulong? StaticId;
        public List<ClientFunction> clientFunctions;
    }

    public class Currency
    {
        public ulong nId;           // 货币Id
        public ulong nNum;          // 当前数量
        public string strName;         // 代币名称
        public string strDes;          // 代币描述
        public string strResourceDir;  // 代币资源路径
        public string strResourceName;  // 代币资源名称
    }

    public class PackItem
    {
        public ulong nStaticId;       // 物品静态Id
        public ulong nId;             // 物品实例ID
        public ulong nNum;            // 当前数量
        public ulong nPos;            // 背包内位置 
        public bool nNew;             // 是否查阅过
        public string strLink;        //上链信息
        public ulong limitTime;        //过期时间
    }

    public class PackItems
    {
        public Dictionary<ulong, PackItem> mapUpdate;       // 更新列表
        public List<ulong> dels;      // 删除列表
    }

    public class PlayerInfo
    {
        public CharInfo Char;
        public string headIcon;
        public uint? CliType;
        public uint? faceModeId;
        public string equipment;
        public uint? facesVersion;
        public ulong? nAppealNO;
        public ulong? nSitNO;
    }

    public class NpcInfo
    {
        public CharInfo Char;
        public ulong OwnerId;
    }

    public class GroundItemInfo
    {
        public MapObjectInfo msgMapObj;
    }

    public class WorldInfo
    {
        public ObjectInfo Object;
    }

    public class SpaceInfo
    {
        public ObjectInfo Object;
        public ulong WorldId;
    }

    public class UserInfo
    {
        public ulong UserId;
        public uint LastLoginTime;
        public uint LastLogoutTime;
        public string LastLoginIp;
        public bool IsFirst;
    }

    public class WorldList : System.EventArgs
    {
        public World[] list { get; set; }
    }

    public class SpaceList : System.EventArgs
    {
        public ulong worldId { get; set; }
        public Space[] list { get; set; }
    }

    public class RoleList : System.EventArgs
    {
        public Role[] list { get; set; }
    }

    public class World : System.EventArgs
    {
        public string name { get; set; }
        public string thumbnail { get; set; }
        public ulong id { get; set; }
    }

    public class Space : System.EventArgs
    {
        public string name { get; set; }
        public string thumbnail { get; set; }
        public ulong id { get; set; }
    }

    public class Role : System.EventArgs
    {
        public string name { get; set; }
        public int gender { get; set; }
        public ulong staticid { get; set; }
    }

    public enum FriendApplyStatus
    {
        UnknownApplyStatus = 0,   //Unknown
        ApplyTodo = 1,            //待处理
        ApplyOverdue = 2,         //已过期
        ApplyRefuse = 3,          //已拒绝
        ApplyAccept = 4,          //已同意
    }

    public class FriendList : System.EventArgs
    {
        public Friend[] friends { get; set; }
    }

    public class Friend : System.EventArgs
    {
        public ulong id { get; set; }
        public string name { get; set; }
        public string icon { get; set; }
        public string note { get; set; }
        public bool online { get; set; }
        public ulong agreeTimeSnamp { get; set; }
        public ulong applyTimeSnamp { get; set; }
        public FriendApplyStatus status { get; set; }
    }

    public class FriendApply : System.EventArgs
    {
        public ulong id { get; set; }
        public bool accept { get; set; }
    }

    public enum PhoneAnswerStatus
    {
        None = 0,   //None
        Accept = 1, //接
        Refuse = 2, //不接
        Busy = 3,   //占线
    }

    public class PhoneCallInfo : System.EventArgs
    {
        public string roomName { get; set; }
        public string token { get; set; }
    }

    public class Phone : System.EventArgs
    {
        public ulong id { get; set; }
        public string strSrcRoleName { get; set; }
        public string strSrcHeadIcon { get; set; }
        public PhoneAnswerStatus status { get; set; }
    }

    public class ConnectToProxySrv : System.EventArgs
    {
        public string address { get; set; }
        public string queueToken { get; set; }
    }

    public class LeftUsrCount : System.EventArgs
    {
        public uint nLeftUsrBefore { get; set; }
    }

    public class Area : System.EventArgs
    {
        public ulong id { get; set; }
        public VarPool pool { get; set; }
    }
    
    public class NetworkEventMessage : System.EventArgs
    {
        public string world { get; set; }
        public string space { get; set; }
        public ulong spaceId { get; set; }
        public string playerCG { get; set; }
        public string resourceName { get; set; }
        public string resourceDir { get; set; }
    }

    public enum NotifyType
    {
        None = 0,
        Server = 1, // 全服通知
        Space = 2, // 空间通知
        Area = 3, // 区域通知
        Role = 4, // 个人通知
    }

    public class NotifyMsg : System.EventArgs
    {
        public NotifyType type { get; set; }
        public string message { get; set; }
        public ulong worldId { get; set; }
    }

    public class BasicMsg : System.EventArgs
    {
        public string type { get; set; }
    }

    public class PhotoShareMsgData : System.EventArgs
    {
        public string from_role_name { get; set; }
        public int material_type { get; set; }
    }

    public class MapActivityMsgData : System.EventArgs
    {
        public string activity_id { get; set; }
        public int space_id { get; set; }
        public int area_id { get; set; }
        public string name { get; set; }
        public string desc { get; set; }
    }

    public class FireworksMsgData : System.EventArgs
    {
        public float duration { get; set; }
        public int subtype { get; set; }
    }

    public class RedEnvelopeMsgData : System.EventArgs
    {
        public float duration { get; set; }
        public int subtype { get; set; }
    }

    public class PhotoShareMsg : BasicMsg
    {
        public PhotoShareMsgData data { get; set; }
    }

    public class MapActivityMsg : BasicMsg
    {
        public MapActivityMsgData data { get; set; }
    }

    public class FireworksMsg : BasicMsg
    {
        public FireworksMsgData data { get; set; }
    }

    public class RedEnvelopeMsg : BasicMsg
    {
        public RedEnvelopeMsgData data { get; set; }
    }
}
