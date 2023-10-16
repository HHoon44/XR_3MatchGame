using System;
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
            LoadAllPrefabs();
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
            var blockAtlas = Resources.LoadAll<SpriteAtlas>("Atlas/BlockAtlas");
            SpriteLoader.SetAtlas(blockAtlas);
        }

        /// <summary>
        /// 리소스 폴더 안에 존재하는
        /// 모든 프리팹을 불러오는 메서드
        /// </summary>
        private void LoadAllPrefabs()
        {
            LoadPoolableObject<Block>(PoolType.Block, "Prefabs/Object/Block", 80);
        }

        /// <summary>
        /// 오브젝트 풀로 사용할 프리팹을 생성하고 풀에 등록하는 메서드
        /// </summary>
        /// <typeparam name="T">프리팹의 컴포넌트</typeparam>
        /// <param name="type">프리팹의 타입</param>
        /// <param name="path">프리팹이 존재하는 경로</param>
        /// <param name="poolCount">풀링 개수</param>
        /// <param name="loadComplete">생성 후 실행할 메서드</param>
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