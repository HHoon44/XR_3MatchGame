using System.Collections.Generic;
using UnityEngine;
using XR_3MatchGame.Util;
using XR_3MatchGame_Object;
using XR_3MatchGame_Resource;
using XR_3MatchGame_Util;

namespace XR_3MatchGame_InGame
{
    public class GameManager : Singleton<GameManager>
    {
        private Vector2Int boardSize = new Vector2Int(6, 6);
        public List<Block> blocks { get; private set; } = new List<Block>();

        public RectInt Bounds
        {
            get
            {
                Vector2Int position = new Vector2Int(-boardSize.x / 2, -boardSize.y / 2);

                return new RectInt(position, boardSize);
            }
        }

        protected override void Awake()
        {
            base.Awake();

            if (gameObject == null)
            {
                return;
            }

            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            XR_3MatchGame_Resource.ResourceManager.Instance.Initialize();

            StartSpawn();
        }

        /// <summary>
        /// 게임 시작 시
        /// 보드에 블럭을 세팅하는 메서드
        /// </summary>
        private void StartSpawn()
        {
            var blockPool = ObjectPoolManager.Instance.GetPool<Block>(PoolType.Block);
            
            var bounds = Bounds;

            for (int i = bounds.xMin; i <= bounds.xMax; i++)
            {
                for (int j = bounds.yMin; j <= bounds.yMax; j++)
                {
                    var block = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                    block.transform.position = new Vector3(i, j, 0);

                    // (-3, -3)
                    block.Initialize(i, j);

                    // 블럭 저장
                    blocks.Add(block);

                    block.gameObject.SetActive(true);
                }
            }
        }
    }
}