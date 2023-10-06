using System.Collections.Generic;
using UnityEngine;
using XR_3MatchGame.Util;

namespace XR_3MatchGame_Util
{
    public class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        private Dictionary<PoolType, object> poolDic = new Dictionary<PoolType, object>();

        public void RegistPool<T>(PoolType type, T obj, int poolCount = 1)
            where T : MonoBehaviour, IPoolableObject
        {
            ObjectPool<T> pool = null;

            if (poolDic.ContainsKey(type))
            {
                pool = poolDic[type] as ObjectPool<T>;
            }
            else
            {
                pool = new ObjectPool<T>();
                poolDic.Add(type, pool);
            }

            if (pool.holder == null)
            {
                pool.holder = new GameObject($"{type.ToString()}Holder").transform;
                pool.holder.parent = transform;
                pool.holder.position = Vector3.zero;
            }


        }
    }
}