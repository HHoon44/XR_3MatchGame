using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Unity.VisualScripting;
using UnityEngine;
using XR_3MatchGame.Util;
using XR_3MatchGame_Object;
using XR_3MatchGame_Resource;
using XR_3MatchGame_Util;

namespace XR_3MatchGame_InGame
{
    public class GameManager : Singleton<GameManager>
    {
        //public List<Block> blocks { get; private set; } = new List<Block>();

        public List<Block> blocks = new List<Block>();

        public Vector2Int Bounds
        {
            get
            {
                // (0 ~ 7)
                Vector2Int position = new Vector2Int(7, 7);

                return position;
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

            // 화면에 블럭을 세팅 합니다
            for (int row = 0; row < Bounds.y; row++)
            {
                for (int col = 0; col < Bounds.x; col++)
                {
                    // 풀에서 사용 가능한 블럭을 가져옵니다
                    var block = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                    block.transform.position = new Vector3(col, row, 0);

                    block.Initialize(col, row);

                    blocks.Add(block);

                    block.gameObject.SetActive(true);
                }
            }

            CheckBlock();
        }

        /// <summary>
        /// 모든 블럭에 접근해서 체킹하는 메서드
        /// </summary>
        public void CheckBlock()
        {
            Block curBlock = null;

            StartCoroutine(LRCheck());

            IEnumerator LRCheck()
            {
                // 모든 블럭의 왼쪽 오른쪽 블럭을 체크합니다
                // (0, 6) (7, 13) (14, 20) (21, 27) (28, 34) (35, 41) (42, 48)
                for (int i = 0; i < blocks.Count; i++)
                {
                    curBlock = blocks[i];

                    for (int j = 0; j < blocks.Count; j++)
                    {
                        // 왼쪽
                        if (curBlock.col - 1 == blocks[j].col && curBlock.row == blocks[j].row)
                        {
                            curBlock.leftBlock = blocks[j];

                            // Test
                            curBlock.leftType = blocks[j].blockType;
                        }

                        // 오른쪽
                        if (curBlock.col + 1 == blocks[j].col && curBlock.row == blocks[j].row)
                        {
                            curBlock.rightBlock = blocks[j];

                            // Test
                            curBlock.rightType = blocks[j].blockType;
                        }
                    }

                    /// 여기 작업 실행
                    // 블럭 클리어 및 블럭 세팅
                    if (curBlock.leftBlock != null && curBlock.rightBlock != null)
                    {
                        // 왼쪽 오른쪽 블럭이 같다면 라인 클리어를 실행합니다
                        if (curBlock.blockType == curBlock.leftBlock.blockType && curBlock.blockType == curBlock.rightBlock.blockType)
                        {
                            Debug.Log("Check");

                            var blockPool = ObjectPoolManager.Instance.GetPool<Block>(PoolType.Block);

                            blockPool.ReturnPoolableObject(curBlock.leftBlock);
                            blockPool.ReturnPoolableObject(curBlock.rightBlock);
                            blockPool.ReturnPoolableObject(curBlock);

                            blocks.Remove(curBlock.leftBlock);
                            blocks.Remove(curBlock.rightBlock);
                            blocks.Remove(curBlock);

                            yield return new WaitForSeconds(1f);

                            // 사라진 위치에 새로운 블럭을 배치합니다
                            for (int newCol = curBlock.leftBlock.col; newCol <= curBlock.rightBlock.col; newCol++)
                            {
                                var newBlock = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                                newBlock.transform.position = new Vector3(newCol, curBlock.row, 0);
                                newBlock.Initialize(newCol, curBlock.row);
                                newBlock.gameObject.SetActive(true);

                                blocks.Add(newBlock);
                            }
                        }
                    }
                }

            }
        }
    }
}