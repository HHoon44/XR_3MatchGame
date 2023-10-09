using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
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
            LoadAllAtlas();
        }

        /// <summary>
        /// 리소스 폴더 안 경로의 프리팹을
        /// 가져와서 반환하는 메서드
        /// </summary>
        /// <param name="path">원하는 프리팹이 존재하는 경로</param>
        /// <returns></returns>
        public GameObject LoadObject(string path)
        {
            // 에셋 폴더 안의 리소스 폴더에 접근하여
            // GameObject 형태로 반환 받는다
            return Resources.Load<GameObject>(path);
        }

        /// <summary>
        /// 리소스 폴더 안에 존재하는 아틀라스를 가져와서
        /// </summary>
        private void LoadAllAtlas()
        {
            var blockAtlase = Resources.LoadAll<SpriteAtlas>("Atlas/BlockAtlas");
            SpriteLoader.SetAtlas(blockAtlase);
        }

        /// <summary>
        /// 리소스 폴더 안에 존재하는
        /// 모든 프리팹을 불러오는 메서드
        /// </summary>
        private void LoadAddPrefabs()
        {
            // 
            LoadPoolableObject<Block>(PoolType.Block, "Prefabs/Object/Block", 60);
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