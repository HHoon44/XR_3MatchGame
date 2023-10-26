using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using XR_3MatchGame.Util;
using XR_3MatchGame_Object;
using XR_3MatchGame_Resource;
using XR_3MatchGame_Util;

namespace XR_3MatchGame_InGame
{
    public class GameManager : Singleton<GameManager>
    {
        public List<Block> blocks = new List<Block>();              // 인 게임 내에서 모든 블럭을 담아놓을 리스트

        public List<Block> checkBlocks = new List<Block>();         // 조건에 의해 사용해야할 블럭을 담아놓을 리스트

        public bool isStart = false;        // 블럭 체크를 실행할것인가?
        public bool isChecking = false;        // 현재 블럭 체크를 진행중인가?

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
            if (isStart == true)
            {
                isStart = false;
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
            /// 시작 할 때 checkBlock의 개수가 3이 아니면 바로 블럭 체킹 들어가고
            /// 아니면 폭탄 체킹 시작
            /// checkBlock 클리어 해주는거 잊으면 안됨

            /// 폭탄 블럭은 따로 설정 해줘야할듯
            // 체크 시작
            isChecking = true;

            Block curBlock = null;

            var blockPool = ObjectPoolManager.Instance.GetPool<Block>(PoolType.Block);
            var size = (BoardSize.x * BoardSize.y);

            if (checkBlocks.Count != 0)
            {
                if (checkBlocks[checkBlocks.Count - 1].blockType == BlockType.Boom)
                {
                    Debug.Log("폭탄 존재");

                    // 폭탄은 마지막 인덱스에 존재
                    curBlock = checkBlocks[checkBlocks.Count - 1];

                    curBlock.spriteRenderer.sprite = SpriteLoader.GetSprite(AtlasType.BlockAtlas, curBlock.blockType.ToString());

                    yield return new WaitForSeconds(.4f);

                    switch (curBlock.boomType)
                    {
                        case BoomType.ColBoom:

                            // 풀에 반환
                            for (int i = 0; i < checkBlocks.Count - 1; i++)
                            {
                                blockPool.ReturnPoolableObject(checkBlocks[i]);
                            }

                            yield return new WaitForSeconds(.4f);

                            // 삭제한 블럭 위에 존재하는 블럭들을 아래로 내려야함

                            var block_0 = checkBlocks[0];
                            var block_1 = checkBlocks[1];
                            var block_2 = checkBlocks[2];

                            checkBlocks.Clear();

                            Debug.Log(checkBlocks.Count);

                            if (block_0.row != (BoardSize.y - 1))
                            {
                                for (int i = 0; i < blocks.Count; i++)
                                { 
                                    // 나머지는 집가서
                                }
                            }

                            break;

                        case BoomType.RowBoom:
                            break;
                    }
                }
                else
                {
                    Debug.Log("폭탄 없음");

                    checkBlocks.Clear();
                }
            }

            /// 폭탄 블럭을 기준으로 4가지 상황를 비교하고 그 조건에 맞는 블럭 삭제
            /// 삭제하면 그 자리에 새로운 블럭 나오도록! (만약에 위에 블럭이 존재한다면 블럭 이동부터 해줘야함)

            for (int i = 0; i < blocks.Count; i++)
            {
                // Left, Right 체크
                if (blocks[i].leftBlock != null && blocks[i].rightBlock != null)
                {
                    // 블럭의 Left Right를 체크
                    if (blocks[i].blockType == blocks[i].leftBlock.blockType && blocks[i].blockType == blocks[i].rightBlock.blockType)
                    {
                        curBlock = blocks[i];

                        yield return new WaitForSeconds(.4f);

                        // 풀에 반환
                        blockPool.ReturnPoolableObject(curBlock.rightBlock);
                        blockPool.ReturnPoolableObject(curBlock.leftBlock);
                        blockPool.ReturnPoolableObject(curBlock);

                        yield return new WaitForSeconds(.4f);

                        var f_Col = curBlock.leftBlock.col;
                        var m_Col = curBlock.col;
                        var l_Col = curBlock.rightBlock.col;

                        // 베이스 Row 값
                        var b_Row = curBlock.row;

                        // 체크하려는 블럭이 맨 위에 존재하는지 확인
                        // 6인지 확인
                        if (curBlock.row != (BoardSize.y - 1))
                        {
                            for (int j = 0; j < blocks.Count; j++)
                            {
                                // 조건을 충족하는 모든 블럭을 찾습니다
                                if ((blocks[j].col == f_Col || blocks[j].col == m_Col || blocks[j].col == l_Col) &&
                                    blocks[j].row > b_Row)
                                {
                                    checkBlocks.Add(blocks[j]);
                                }
                            }

                            // 찾은 블럭들을 내려줍니다
                            for (int j = 0; j < checkBlocks.Count; j++)
                            {
                                var targetRow = checkBlocks[j].row -= 1;

                                if (Mathf.Abs(targetRow - checkBlocks[j].transform.position.y) > .1f)
                                {
                                    Vector2 tempPosition = new Vector2(checkBlocks[j].transform.position.x, targetRow);
                                    checkBlocks[j].transform.position = Vector2.Lerp(checkBlocks[j].transform.position, tempPosition, .05f);
                                }
                            }
                        }

                        // 체크된 블럭들은 List에서 삭제 해줍니다
                        blocks.Remove(curBlock.rightBlock);
                        blocks.Remove(curBlock.leftBlock);
                        blocks.Remove(curBlock);

                        // 비어있는 칸의 개수를 구합니다
                        var emptyBlockCount = size - (blocks.Count);

                        // 비어있는 칸에 새로운 블럭을 추가할 때 사용할 Col, Row 값을 담아놓습니다
                        // Row 값은 조건 연산자를 이용해서 설정합니다
                        var n_Col = f_Col;
                        var n_Row = checkBlocks.Count > 0 ? checkBlocks[checkBlocks.Count - 1].row + 1 : b_Row;

                        yield return new WaitForSeconds(.4f);

                        for (int j = 0; j < emptyBlockCount; j++)
                        {
                            // EX
                            // Col = 0 1 2
                            // Row = 1부터 6까지
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
                                // 범위를 넘었다면 재조정 합니다
                                n_Col = f_Col;

                                // 다음 칸을 채우기 위해서 ++을 합니다
                                n_Row++;
                            }
                        }

                        LRTBCheck();
                        checkBlocks.Clear();
                        i = 0;
                    }
                }

                // Top, Bottom 체크
                if (blocks[i].topBlock != null && blocks[i].bottomBlock != null)
                {
                    // i번째에 존재하는 블럭의 Top, Bottom을 체크합니다
                    if (blocks[i].blockType == blocks[i].topBlock.blockType &&
                        blocks[i].blockType == blocks[i].bottomBlock.blockType)
                    {
                        curBlock = blocks[i];

                        yield return new WaitForSeconds(.4f);

                        // 블럭을 풀에 반환 합니다
                        blockPool.ReturnPoolableObject(curBlock.topBlock);
                        blockPool.ReturnPoolableObject(curBlock.bottomBlock);
                        blockPool.ReturnPoolableObject(curBlock);

                        yield return new WaitForSeconds(.4f);

                        // Base가 될 Col, Row 값을 담아놓습니다
                        var b_Col = curBlock.col;
                        var b_Row = curBlock.topBlock.row;

                        // Top블럭이 맨 위 블럭인지 확인합니다
                        if (b_Row != (BoardSize.y - 1))
                        {
                            // 한칸 내릴 블럭들을 찾습니다
                            for (int j = 0; j < blocks.Count; j++)
                            {
                                if (blocks[j].col == b_Col && blocks[j].row > b_Row)
                                {
                                    this.checkBlocks.Add(blocks[j]);
                                }
                            }

                            for (int j = 0; j < this.checkBlocks.Count; j++)
                            {
                                // 찾은 블럭들의 한칸씩 내려줍니다
                                var targetRow = this.checkBlocks[j].row -= 3;

                                if (Mathf.Abs(targetRow - this.checkBlocks[j].transform.position.y) > .1f)
                                {
                                    Vector2 tempPosition = new Vector2(this.checkBlocks[j].transform.position.x, targetRow);
                                    this.checkBlocks[j].transform.position = Vector2.Lerp(this.checkBlocks[j].transform.position, tempPosition, .05f);
                                }
                            }
                        }

                        // 체크된 블럭들은 List에서 삭제 해줍니다
                        blocks.Remove(curBlock.topBlock);
                        blocks.Remove(curBlock.bottomBlock);
                        blocks.Remove(curBlock);

                        // 비어있는 칸의 개수를 구합니다
                        var emptyBlockCount = size - (blocks.Count);

                        var n_Row = this.checkBlocks.Count > 0 ? this.checkBlocks[this.checkBlocks.Count - 1].row + 1 : b_Row - 2;

                        yield return new WaitForSeconds(.4f);

                        for (int j = 0; j < emptyBlockCount; j++)
                        {
                            if (n_Row < BoardSize.y)
                            {
                                var newBlock = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                                newBlock.transform.position = new Vector3(b_Col, n_Row, 0);
                                newBlock.gameObject.SetActive(true);
                                newBlock.Initialize(b_Col, n_Row);

                                blocks.Add(newBlock);

                                n_Row++;
                            }
                        }

                        LRTBCheck();
                        this.checkBlocks.Clear();
                        i = 0;
                    }
                }
            }

            // 체크 종료
            isChecking = false;
        }
    }
}