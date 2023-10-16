using System.Collections;
using System.Collections.Concurrent;
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
        public List<Block> blocks = new List<Block>();

        // Test
        public List<Block> rowBlocks = new List<Block>();

        public Vector2Int BoardSize
        {
            get
            {
                // (0 ~ 7)
                Vector2Int boardSize = new Vector2Int(7, 7);

                return boardSize;
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
            for (int row = 0; row < BoardSize.y; row++)
            {
                for (int col = 0; col < BoardSize.x; col++)
                {
                    // 풀에서 사용 가능한 블럭을 가져옵니다
                    var block = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                    block.transform.position = new Vector3(col, row, 0);

                    block.Initialize(col, row);

                    blocks.Add(block);

                    block.gameObject.SetActive(true);
                }
            }

            BlockCheck();
        }

        /// <summary>
        /// 블럭의 왼 오를 확인하는 메서드
        /// </summary>
        public void BlockCheck()
        {
            // (0, 6) (7, 13) (14, 20) (21, 27) (28, 34) (35, 41) (42, 48)
            // 모든 블럭의 왼 오 블럭을 체크합니다
            for (int i = 0; i < blocks.Count; i++)
            {
                for (int j = 0; j < blocks.Count; j++)
                {
                    // 왼쪽
                    if (blocks[i].col == 0)
                    {
                        // 왼쪽 맨 끝 블럭이라는 의미입니다
                        blocks[i].leftBlock = null;
                        blocks[i].leftType = BlockType.None;
                    }
                    else
                    {
                        if (blocks[i].col - 1 == blocks[j].col && blocks[i].row == blocks[j].row)
                        {
                            blocks[i].leftBlock = blocks[j];

                            // Test
                            blocks[i].leftType = blocks[j].blockType;
                        }
                    }

                    // 오른쪽
                    if (blocks[i].col == 6)
                    {
                        // 오른쪽 맨 끝 블럭이라는 의미입니다
                        blocks[i].rightBlock = null;
                        blocks[i].rightType = BlockType.None;
                    }
                    else
                    {
                        if (blocks[i].col + 1 == blocks[j].col && blocks[i].row == blocks[j].row)
                        {
                            blocks[i].rightBlock = blocks[j];

                            // Test
                            blocks[i].rightType = blocks[j].blockType;
                        }
                    }
                }
            }

            // 모든 블럭의 위 아래 블럭을 체크합니다
            for (int i = 0; i < blocks.Count; i++)
            {
                for (int j = 0; j < blocks.Count; j++)
                {
                    // 위쪽
                    if (blocks[i].row == 6)
                    {
                        blocks[i].topBlock = null;
                        blocks[i].topType = BlockType.None;
                    }
                    else
                    {
                        if (blocks[i].col == blocks[j].col && blocks[i].row + 1 == blocks[j].row)
                        {
                            blocks[i].topBlock = blocks[j];

                            // Test
                            blocks[i].topType = blocks[j].blockType;
                        }
                    }

                    // 아래쪽
                    if (blocks[i].row == 0)
                    {
                        blocks[i].bottomBlock = null;
                        blocks[i].bottomType = BlockType.None;
                    }
                    else
                    {
                        if (blocks[i].col == blocks[j].col && blocks[i].row - 1 == blocks[j].row)
                        {
                            blocks[i].bottomBlock = blocks[j];

                            // Test
                            blocks[i].bottomType = blocks[j].blockType;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 라인 클리어를 담당하는 메서드
        /// </summary>
        /// <returns></returns>
        IEnumerator LineClear()
        {
            var blockPool = ObjectPoolManager.Instance.GetPool<Block>(PoolType.Block);

            for (int i = 0; i < blocks.Count; i++)
            {
                // 블럭 클리어 및 블럭 세팅
                if (blocks[i].leftBlock != null && blocks[i].rightBlock != null)
                {
                    // 왼쪽 오른쪽으로 같은 블럭이 있는지 체크합니다
                    if (blocks[i].blockType == blocks[i].leftBlock.blockType &&
                        blocks[i].blockType == blocks[i].rightBlock.blockType)
                    {
                        yield return new WaitForSeconds(.5f);

                        blockPool.ReturnPoolableObject(blocks[i].leftBlock);
                        blockPool.ReturnPoolableObject(blocks[i].rightBlock);
                        blockPool.ReturnPoolableObject(blocks[i]);

                        yield return new WaitForSeconds(.5f);

                        /// 같은 col에 존재하는 블럭들을 한칸씩 내려주는 코드 작성
                        for (int d_Col = blocks[i].leftBlock.col; d_Col <= blocks[i].rightBlock.col; d_Col++)
                        {

                        }

                        // 기존의 블럭을 리스트에서 제거합니다
                        blocks.Remove(blocks[i].leftBlock);
                        blocks.Remove(blocks[i].rightBlock);
                        blocks.Remove(blocks[i]);
                    }
                }
            }
        }
    }
}