using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        public List<Block> downBlcok = new List<Block>();

        // Test
        public IEnumerator test_Coroutine;

        public bool isCheck = false;

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

        private void Update()
        {
            if (isCheck == true)
            {
                isCheck = false;
                StartCoroutine(BlockClear());
            }
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

            LRTBCheck();
            StartCoroutine(BlockClear());
        }

        /// <summary>
        /// 블럭의 Top, Bottom, Left, Right를 확인하는 메서드
        /// </summary>
        public void LRTBCheck()
        {
            // 모든 블럭의 Left, Right 블럭을 체크합니다
            for (int i = 0; i < blocks.Count; i++)
            {
                for (int j = 0; j < blocks.Count; j++)
                {
                    // Left
                    if (blocks[i].col == 0)
                    {
                        // Left 맨 끝 블럭이라는 의미입니다
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

                    // Right
                    if (blocks[i].col == 6)
                    {
                        // Right 맨 끝 블럭이라는 의미입니다
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

            // 모든 블럭의 Top, Bottom 블럭을 체크합니다
            for (int i = 0; i < blocks.Count; i++)
            {
                for (int j = 0; j < blocks.Count; j++)
                {
                    // Top
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

                    // Bottom
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
        /// 블럭 클리어 및 블럭 생성을 담당하는 메서드
        /// </summary>
        /// <returns></returns>
        public IEnumerator BlockClear()
        {
            var blockPool = ObjectPoolManager.Instance.GetPool<Block>(PoolType.Block);

            // 현재 블럭
            Block curBlock = null;

            for (int i = 0; i < blocks.Count; i++)
            {
                if (blocks[i].leftBlock != null &&
                    blocks[i].rightBlock != null)
                {
                    // i번째에 존재하는 블럭의 Left, Right를 체크합니다
                    if (blocks[i].blockType == blocks[i].leftBlock.blockType &&
                        blocks[i].blockType == blocks[i].rightBlock.blockType)
                    {
                        curBlock = blocks[i];

                        // 딜레이 주고 라인 체크를 시작합니다
                        yield return new WaitForSeconds(.4f);

                        blockPool.ReturnPoolableObject(curBlock.leftBlock);
                        blockPool.ReturnPoolableObject(curBlock.rightBlock);
                        blockPool.ReturnPoolableObject(curBlock);

                        yield return new WaitForSeconds(.4f);

                        // 처음과 마지막 Col 값을 담아놓습니다
                        var c_Col = curBlock.leftBlock.col;
                        var l_Col = curBlock.rightBlock.col;

                        // 처음 Row 값을 담아놓습니다
                        var c_Row = curBlock.row;

                        // 현재 블럭이 맨 위의 블럭인지 체크 합니다
                        if (curBlock.row != (BoardSize.y - 1))
                        {
                            // 한칸 내릴 블럭들을 찾습니다
                            for (int j = 0; j < blocks.Count; j++)
                            {
                                if (c_Col < (l_Col + 1))
                                {
                                    if (c_Col == blocks[j].col && c_Row < blocks[j].row)
                                    {
                                        downBlcok.Add(blocks[j]);
                                        c_Col++;
                                    }

                                    if (c_Col > l_Col)
                                    {
                                        // Col이 범위를 벗어나지 않도록 조절합니다
                                        c_Col = curBlock.leftBlock.col;
                                    }
                                }
                            }

                            for (int j = 0; j < downBlcok.Count; j++)
                            {
                                // 찾은 블럭들의 한칸씩 내려줍니다
                                var targetRow = downBlcok[j].row -= 1;

                                if (Mathf.Abs(targetRow - downBlcok[j].transform.position.y) > .1f)
                                {
                                    Vector2 tempPosition = new Vector2(downBlcok[j].transform.position.x, targetRow);
                                    downBlcok[j].transform.position = Vector2.Lerp(downBlcok[j].transform.position, tempPosition, .05f);
                                }
                            }
                        }

                        // 체크된 블럭들은 List에서 삭제 해줍니다
                        blocks.Remove(curBlock.leftBlock);
                        blocks.Remove(curBlock.rightBlock);
                        blocks.Remove(curBlock);

                        // 비어있는 칸의 개수를 구합니다
                        var emptyBlockCount = (BoardSize.x * BoardSize.y) - (blocks.Count);

                        Debug.Log(emptyBlockCount);

                        // 비어있는 위치의 Col, Row 값을 저장 해놓습니다
                        // ( Row에 +1을 해줘야 빈칸의 첫번째 Row 값을 알 수있음 )
                        // c_Row로 설정되면 맨 위의 블럭이라는 뜻 입니다
                        var n_Col = c_Col;
                        var n_Row = downBlcok.Count > 0 ? downBlcok[downBlcok.Count - 1].row + 1 : c_Row;

                        yield return new WaitForSeconds(.4f);

                        if (downBlcok.Count == 0)
                        {
                            for (int j = 0; j < emptyBlockCount; j++)
                            {
                                // 한칸 내릴 블럭이 없다면
                                // 바로 블럭을 생성 합니다
                                if (n_Col <= l_Col && n_Row < BoardSize.y)
                                {
                                    var newBlock = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                                    newBlock.transform.position = new Vector3(n_Col, n_Row, 0);
                                    newBlock.gameObject.SetActive(true);
                                    newBlock.Initialize(n_Col, n_Row);

                                    blocks.Add(newBlock);

                                    n_Col++;
                                }
                            }
                        }
                        else
                        {
                            /// 블럭 찾기 여기서 오류 나는듯
                            for (int j = 0; j < emptyBlockCount; j++)
                            {
                                if (n_Col <= l_Col && n_Row < BoardSize.y)
                                {
                                    var newBlock = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                                    newBlock.transform.position = new Vector3(n_Col, n_Row, 0);
                                    newBlock.gameObject.SetActive(true);
                                    newBlock.Initialize(n_Col, n_Row);

                                    blocks.Add(newBlock);

                                    n_Col++;
                                }

                                if (n_Col > l_Col)
                                {
                                    // ++ 과정에서 Col이 범위를 넘었을 경우 다시 초기화 해줍니다
                                    n_Col = c_Col;

                                    // 현재 Row에 블럭을 생성하였으므로 ++을 해줍니다
                                    n_Row++;
                                }
                            }
                        }

                        // 작업이 완료 되었다면 비워줍니다
                        downBlcok.Clear();

                        /// 이게 맞는거 같음
                        i = 0;
                    }
                }

            }
        }
    }
}