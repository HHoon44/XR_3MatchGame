using System.Collections.Generic;
using UnityEngine;
using XR_3MatchGame.Util;

namespace XR_3MatchGame_Util
{
    public class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        /// <summary>
        /// 풀을 담아놓을 딕셔너리
        /// </summary>
        private Dictionary<PoolType, object> poolDic = new Dictionary<PoolType, object>();

        /// <summary>
        /// 풀에 오브젝트를 등록하는 메서드
        /// </summary>
        /// <typeparam name="T">제한된 타입</typeparam>
        /// <param name="type">요청 풀 타입</param>
        /// <param name="obj">등록할 오브젝트</param>
        /// <param name="poolCount">생성 개수</param>
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

            for (int i = 0; i < poolCount; i++)
            {
                var poolableObj = Instantiate(obj);
                poolableObj.name = obj.name;
                poolableObj.transform.SetParent(pool.holder);
                poolableObj.gameObject.SetActive(false);

                pool.RegistPoolableObject(poolableObj);
            }
        }

        /// <summary>
        /// 풀 저장소에서 풀을 반환해주는 메서드
        /// </summary>
        /// <typeparam name="T">제한된 타입</typeparam>
        /// <param name="type">요청 풀 타입</param>
        /// <returns></returns>
        public ObjectPool<T> GetPool<T>(PoolType type)
            where T : MonoBehaviour, IPoolableObject
        {
            if (!poolDic.ContainsKey(type))
            {
                return null;
            }

            return poolDic[type] as ObjectPool<T>;
        }

        /// <summary>
        /// 풀 비우기 메서드
        /// </summary>
        /// <typeparam name="T">제한된 타입</typeparam>
        /// <param name="type">요청 풀 타입</param>
        public void ClearPool<T>(PoolType type)
            where T : MonoBehaviour, IPoolableObject
        {
            var pool = GetPool<T>(type)?.Pool;

            if (pool == null)
            {
                return;
            }

            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i] != null)
                {
                    Destroy(pool[i].gameObject);
                }
            }

            pool.Clear();
        }
    }
}