using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Baidu.VR.Zion
{
    public class NetworkSetting : MonoBehaviour
    {
        private static NetworkSetting Instance = null;
        private void Awake()
        {
            Instance = this;
        }

        public List<GameObject> ObjectPrefabs = new List<GameObject>();
        public Dictionary<ulong, GameObject> ObjectPrefabsDict = new Dictionary<ulong, GameObject>();

        private List<NetworkPlayer> Players = new List<NetworkPlayer>();
        private Dictionary<ulong, NetworkPlayer> PlayerMap = new Dictionary<ulong, NetworkPlayer>();

        private List<NetworkObject> NetworkObjects = new List<NetworkObject>();
        private Dictionary<ulong, NetworkObject> NetworkObjectMap = new Dictionary<ulong, NetworkObject>();

        public Dictionary<ulong, Stack<NetworkObject>> ObjectPool = new Dictionary<ulong, Stack<NetworkObject>>();

        public bool Inited = false;

        private void Start()
        {
            if (!Inited)
                Init();
        }

        public void Init()
        {
            //将ObjectPrefabs数据列入ObjectPrefabsDict中
            foreach (var prefab in ObjectPrefabs)
            {
                if (prefab == null)
                {
                    Debug.LogWarning("NetworkSetting Init Warning. Null in ObjectPrefabs.");
                    continue;
                }

                NetworkObject obj = prefab.GetComponent<NetworkObject>();
                if (obj == null)
                {
                    Debug.LogWarningFormat("NetworkSetting Init Warning. Fail to find NetworkObject in {0}.", prefab.name);
                    continue;
                }

                ObjectPrefabsDict[obj.StaticId] = prefab;
            }

            //收集静态网络对象
            NetworkObject[] objectsInScene = Resources.FindObjectsOfTypeAll<NetworkObject>();

            foreach (var obj in objectsInScene)
            {
                if (obj.gameObject.scene.IsValid() && obj.IsStatic)
                {
                    RecycleObject(obj);
                }
            }

            Inited = true;
        }

        public void AddPlayer(NetworkPlayer player)
        {
            if (player == null) return;

            ulong id = player.NetworkId;
            if (PlayerMap.ContainsKey(id))
            {
                Debug.LogErrorFormat("AddPlayer Error. Player with NetworkId {0} allready exist.", id);
                return;
            }

            PlayerMap[id] = player;
            Players.Add(player);

            player.Connected = true;
        }

        public bool DeletePlayer(ulong id)
        {
            NetworkPlayer player = null;
            if (!PlayerMap.TryGetValue(id, out player))
            {
                return false;
            }

            PlayerMap.Remove(id);
            Players.Remove(player);

            if (player != null)
            {
                player.Connected = false;
                player.OnDestroy();
            }
            return true;
        }

        public void DeleteAllPlayer()
        {
            foreach (var player in Players)
            {
                if (player != null)
                {
                    player.Connected = false;
                    player.OnDestroy();
                }
            }

            Players.Clear();
            PlayerMap.Clear();
        }

        public NetworkPlayer GetPlayer(ulong id)
        {
            NetworkPlayer ret = null;
            PlayerMap.TryGetValue(id, out ret);

            return ret;
        }

        public ReadOnlyCollection<NetworkPlayer> GetAllPlayer()
        {
            return Players.AsReadOnly();
        }

        public void AddObject(NetworkObject @object)
        {
            if (@object == null) return;

            ulong id = @object.Id;
            if (NetworkObjectMap.ContainsKey(id))
            {
                Debug.LogErrorFormat("AddObject Error. NetworkObject with Id {0} allready exist.", id);
                return;
            }

            NetworkObjectMap[id] = @object;
            NetworkObjects.Add(@object);
        }

        public bool DeleteObject(ulong id)
        {
            NetworkObject @object = null;
            if (!NetworkObjectMap.TryGetValue(id, out @object) || @object == null)
            {
                return false;
            }

            NetworkObjectMap.Remove(id);
            NetworkObjects.Remove(@object);

            RecycleObject(@object);

            return true;
        }

        public void DeleteAllObjects()
        {
            foreach (var removedObj in NetworkObjects)
            {
                RecycleObject(removedObj);
            }

            NetworkObjects.Clear();
            NetworkObjectMap.Clear();
        }

        public List<NetworkObject> RemoveAllObjects()
        {
            List<NetworkObject> ret = NetworkObjects.FindAll((NetworkObject obj) => { return obj != null && !obj.IsStatic; });

            NetworkObjects.RemoveAll((NetworkObject obj) => { return obj == null || !obj.IsStatic; });
            foreach (var removedObj in ret)
            {
                NetworkObjectMap.Remove(removedObj.Id);
            }

            return ret;
        }

        public NetworkObject GetObject(ulong id)
        {
            NetworkObject ret = null;
            NetworkObjectMap.TryGetValue(id, out ret);

            return ret;
        }

        public ReadOnlyCollection<NetworkObject> GetAllObjects()
        {
            return NetworkObjects.AsReadOnly();
        }

        private void RecycleObject(NetworkObject obj)
        {
            if (obj == null)
                return;

            obj.Inited = false;

            ulong staticId = obj.StaticId;

            Stack<NetworkObject> stack;
            if (!ObjectPool.TryGetValue(staticId, out stack) || stack == null)
            {
                stack = new Stack<NetworkObject>();
                ObjectPool[staticId] = stack;
            }

            stack.Push(obj);

#if UNITY_EDITOR
            if (!obj.IsStatic)
            {
                obj.gameObject.SetActive(false);
                obj.transform.SetParent(transform);
            }
#else
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(transform);
#endif
        }
    }
}