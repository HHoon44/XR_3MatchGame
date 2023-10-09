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
        /// ���ҽ� ���� �� ����� ��������
        /// �����ͼ� ��ȯ�ϴ� �޼���
        /// </summary>
        /// <param name="path">���ϴ� �������� �����ϴ� ���</param>
        /// <returns></returns>
        public GameObject LoadObject(string path)
        {
            // ���� ���� ���� ���ҽ� ������ �����Ͽ�
            // GameObject ���·� ��ȯ �޴´�
            return Resources.Load<GameObject>(path);
        }

        /// <summary>
        /// ���ҽ� ���� �ȿ� �����ϴ� ��Ʋ�󽺸� �����ͼ�
        /// </summary>
        private void LoadAllAtlas()
        {
            var blockAtlase = Resources.LoadAll<SpriteAtlas>("Atlas/BlockAtlas");
            SpriteLoader.SetAtlas(blockAtlase);
        }

        /// <summary>
        /// ���ҽ� ���� �ȿ� �����ϴ�
        /// ��� �������� �ҷ����� �޼���
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