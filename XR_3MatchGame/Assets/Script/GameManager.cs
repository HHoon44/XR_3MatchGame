using System.Collections;
using System.Collections.Generic;
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

            for (int row = Bounds.yMin; row <= Bounds.yMax; row++)
            {
                for (int col = Bounds.xMin; col <= Bounds.xMax; col++)
                {
                    // 사용 가능한 블럭을 가져와서 세팅합니다
                    var block = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                    block.transform.position = new Vector3(col, row, 0);

                    // 현재 블럭이 생성될 위치 값을 전달 합니다
                    block.Initialize(col, row);

                    // 화면에 띄워진 블럭을 저장합니다
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
            // X와 Y의 최소를 가져옵니다
            var colValue = Bounds.xMin;
            var rowValue = Bounds.yMin;

            Block curBlock = null;

            // 모든 블럭을 반복하면서 블럭의 좌,우를 체킹하도록 합니다
            for (int i = 0; i < blocks.Count; i++)
            {
                // 맨 아래서부터 블럭 체킹을 시작합니다
                if (blocks[i].col == colValue && blocks[i].row == rowValue)
                {
                    if (rowValue > Bounds.yMax)
                    {
                        // 사용하는 row 변수가 Y의 최대값보다 높아진다면
                        // 모든 블럭을 확인 한 것이므로 return
                        return;
                    }

                    // 좌, 우를 체킹할 블럭을 가져옵니다
                    curBlock = blocks[i];

                    // curBlock의 좌, 우를 체킹합니다
                    StartCoroutine(LeftRightCheck(curBlock));

                    colValue++;

                    if (colValue > Bounds.xMax)
                    {
                        colValue = Bounds.xMin;

                        // 현재 Row의 모든 Col을 확인했으므로 Row값을 ++
                        rowValue++;
                    }
                }

                // 현재 블럭의 좌, 우를 체킹하는 메서드
                IEnumerator LeftRightCheck(Block curBlock)
                {
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        // curBlock의 왼쪽에 존재하는 블럭을 찾습니다
                        if (curBlock.col - 1 == blocks[i].col &&
                            curBlock.row == blocks[i].row)
                        {
                            curBlock.leftBlock = blocks[i];
                        }

                        // curBlock의 오른쪽에 존재하는 블럭을 찾습니다
                        if (curBlock.col + 1 == blocks[i].col &&
                            curBlock.row == blocks[i].row)
                        {
                            curBlock.rightBlock = blocks[i];
                        }

                        if (curBlock.rightBlock != null &&
                            curBlock.leftBlock != null)
                        {
                            if (curBlock.blockType == curBlock.leftBlock.blockType &&
                                curBlock.blockType == curBlock.rightBlock.blockType)
                            {
                                // 현재 블럭의 타입이 왼쪽과 오른쪽에 존재하는 타입과 같다면
                                // 라인 클리어를 실행합니다.
                                // Block의 풀을 가져와서 같은 블럭을 모두 풀에 반환 합니다
                                var blockPool = ObjectPoolManager.Instance.GetPool<Block>(PoolType.Block);

                                yield return new WaitForSeconds(2f);

                                blockPool.ReturnPoolableObject(curBlock.rightBlock);
                                blockPool.ReturnPoolableObject(curBlock.leftBlock);
                                blockPool.ReturnPoolableObject(curBlock);
                            }
                        }
                        else
                        {
                            yield return null;
                        }
                    }
                }

            }
        }
    }
}