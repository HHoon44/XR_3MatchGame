using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XR_3MatchGame.Util;
using XR_3MatchGame_Object;
using XR_3MatchGame_Util;

namespace XR_3MatchGame_Resource
{
    public class ResourceManager : Singleton<ResourceManager>
    {
        public void Initialize()
        {
            LoadAddPrefabs();
        }

        public GameObject LoadObject(string path)
        {
            return Resources.Load<GameObject>(path);
        }

        private void LoadAddPrefabs()
        {
            LoadPoolableObject<Block>(PoolType.Block, "Prefabs/Object/Block0", 15);
            LoadPoolableObject<Block>(PoolType.Block, "Prefabs/Object/Block1", 15);
            LoadPoolableObject<Block>(PoolType.Block, "Prefabs/Object/Block2", 15);
            LoadPoolableObject<Block>(PoolType.Block, "Prefabs/Object/Block3", 15);
            LoadPoolableObject<Block>(PoolType.Block, "Prefabs/Object/Block4", 15);
            LoadPoolableObject<Block>(PoolType.Block, "Prefabs/Object/Block5", 15);
        }

        public void LoadPoolableObject<T>(PoolType type, string path, int poolCount = 1, Action loadComplete = null)
            where T : MonoBehaviour, IPoolableObject
        {
            var obj = LoadObject(path);

            var tComponent = obj.GetComponent<T>();

            ObjectPoolManager.Instance.RegistPool<T>(type, tComponent, poolCount);

            loadComplete?.Invoke();
        }
    }
}