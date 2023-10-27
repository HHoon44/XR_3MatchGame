using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;
using XR_3MatchGame.Util;
using XR_3MatchGame_Object;
using XR_3MatchGame_Resource;
using XR_3MatchGame_Util;

namespace XR_3MatchGame_InGame
{
    // -최적화-
    // 나중에 yield return new WaitForSeconds는 선언해놓고 사용하자

    public class GameManager : Singleton<GameManager>
    {
        public List<Block> blocks = new List<Block>();              // 인 게임 내에서 모든 블럭을 담아놓을 리스트

        public List<Block> checkBlocks = new List<Block>();         // 조건에 의해 사용해야할 블럭을 담아놓을 리스트

        public bool isStart = false;            // 블럭 체크를 실행할것인가?
        public bool isChecking = false;         // 현재 블럭 체크를 진행중인가?

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
            // 체크 시작
            //isChecking = true;

            Block curBlock = null;

            var blockPool = ObjectPoolManager.Instance.GetPool<Block>(PoolType.Block);
            var size = (BoardSize.x * BoardSize.y);

            // 폭탄 작업
            if (checkBlocks.Count != 0)
            {
                if (checkBlocks[checkBlocks.Count - 1].blockType == BlockType.Boom)
                {
                    // 폭탄은 마지막 인덱스에 존재
                    curBlock = checkBlocks[checkBlocks.Count - 1];

                    curBlock.spriteRenderer.sprite = SpriteLoader.GetSprite(AtlasType.BlockAtlas, curBlock.blockType.ToString());

                    yield return new WaitForSeconds(.4f);

                    switch (curBlock.boomType)
                    {
                        case BoomType.ColBoom:
                            var col_0 = checkBlocks[0].col;
                            var col_1 = checkBlocks[1].col;
                            var col_2 = checkBlocks[2].col;

                            var row_0 = checkBlocks[0].row;

                            // 풀에 반환
                            for (int i = 0; i < checkBlocks.Count - 1; i++)
                            {
                                blockPool.ReturnPoolableObject(checkBlocks[i]);
                            }

                            yield return new WaitForSeconds(.4f);

                            // 중요
                            checkBlocks.Clear();

                            // 삭제한 블럭들 위에 존재하는 블럭들 찾기
                            if (row_0 != (BoardSize.y - 1))
                            {
                                for (int i = 0; i < blocks.Count; i++)
                                {
                                    if ((col_0 == blocks[i].col || col_1 == blocks[i].col || col_2 == blocks[i].col) && row_0 < blocks[i].row)
                                    {
                                        checkBlocks.Add(blocks[i]);
                                    }
                                }
                            }

                            // 블럭 내려주는 작업
                            for (int i = 0; i < checkBlocks.Count; i++)
                            {
                                var targetRow = checkBlocks[i].row -= 1;

                                if (Mathf.Abs(targetRow - checkBlocks[i].transform.position.y) > .1f)
                                {
                                    Vector2 tempPosition = new Vector2(checkBlocks[i].transform.position.x, targetRow);
                                    checkBlocks[i].transform.position = Vector2.Lerp(checkBlocks[i].transform.position, tempPosition, .05f);
                                }
                            }

                            blocks.Remove(checkBlocks[0]);
                            blocks.Remove(checkBlocks[1]);
                            blocks.Remove(checkBlocks[2]);

                            // 생성 해줄 블럭들의 베이스가 될 Col, Row 값
                            var row_NewNum = checkBlocks.Count > 0 ? checkBlocks[checkBlocks.Count - 1].row + 1 : BoardSize.y - 1;

                            yield return new WaitForSeconds(.4f);

                            var newBlock_0 = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                            newBlock_0.transform.position = new Vector3(col_0, row_NewNum, 0);
                            newBlock_0.gameObject.SetActive(true);
                            newBlock_0.Initialize(col_0, row_NewNum);

                            var newBlock_1 = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                            newBlock_1.transform.position = new Vector3(col_1, row_NewNum, 0);
                            newBlock_1.gameObject.SetActive(true);
                            newBlock_1.Initialize(col_1, row_NewNum);

                            var newBlock_2 = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                            newBlock_2.transform.position = new Vector3(col_2, row_NewNum, 0);
                            newBlock_2.gameObject.SetActive(true);
                            newBlock_2.Initialize(col_2, row_NewNum);

                            blocks.Add(newBlock_0);
                            blocks.Add(newBlock_1);
                            blocks.Add(newBlock_2);

                            LRTBCheck();
                            checkBlocks.Clear();
                            break;

                        case BoomType.RowBoom:
                            break;
                    }
                }
                else
                {
                    checkBlocks.Clear();
                }
            }

            // 일반 블럭 작업
            for (int i = 0; i < blocks.Count; i++)
            {
                // Left, Right 체크
                if (blocks[i].leftBlock != null && blocks[i].rightBlock != null)
                {
                    if (blocks[i].leftBlock.blockType == blocks[i].blockType && blocks[i].rightBlock.blockType == blocks[i].blockType)
                    {
                        curBlock = blocks[i];

                        yield return new WaitForSeconds(.4f);

                        var col_L = curBlock.leftBlock.col;
                        var col_M = curBlock.col;
                        var col_R = curBlock.rightBlock.col;

                        var row_M = curBlock.row;

                        // 풀에 반환
                        blockPool.ReturnPoolableObject(curBlock.leftBlock);
                        blockPool.ReturnPoolableObject(curBlock.rightBlock);
                        blockPool.ReturnPoolableObject(curBlock);

                        // 맨 위에 있는 블럭인지 확인
                        if (curBlock.row != (BoardSize.y - 1))
                        {
                            for (int j = 0; j < blocks.Count; j++)
                            {
                                // 삭제한 블럭들의 위에 존재하는 블럭들을 탐색
                                if ((blocks[j].col == col_L || blocks[j].col == col_M || blocks[j].col == col_R) && blocks[j].row > row_M)
                                {
                                    checkBlocks.Add(blocks[j]);
                                }
                            }
                        }

                        yield return new WaitForSeconds(.4f);

                        // 블럭을 내리는 작업
                        for (int j = 0; j < checkBlocks.Count; j++)
                        {
                            var targetRow = checkBlocks[j].row -= 1;

                            if (Mathf.Abs(targetRow - checkBlocks[j].transform.position.y) > .1f)
                            {
                                Vector2 tempPosition = new Vector2(checkBlocks[j].transform.position.x, targetRow);

                                checkBlocks[j].transform.position = Vector2.Lerp(checkBlocks[j].transform.position, tempPosition, .05f);
                            }
                        }

                        blocks.Remove(curBlock.rightBlock);
                        blocks.Remove(curBlock.leftBlock);
                        blocks.Remove(curBlock);

                        // 비어있는 칸의 개수
                        var emptyBlockCount = size - blocks.Count;

                        var col_NewNum = col_L;
                        var row_Newnum = checkBlocks.Count > 0 ? checkBlocks[checkBlocks.Count - 1].row + 1 : row_M;

                        yield return new WaitForSeconds(.4f);

                        for (int j = 0; j < emptyBlockCount; j++)
                        {
                            if (col_NewNum <= col_R && row_Newnum < BoardSize.y)
                            {
                                var newBlock = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                                newBlock.transform.position = new Vector3(col_NewNum, row_Newnum, 0);
                                newBlock.gameObject.SetActive(true);
                                newBlock.Initialize(col_NewNum, row_Newnum);

                                blocks.Add(newBlock);

                                col_NewNum++;
                            }

                            if (col_NewNum > col_R)
                            {
                                // 다음 줄을 채우기 위한 작업
                                col_NewNum = col_L;
                                row_Newnum++;
                            }
                        }

                        // 중요
                        checkBlocks.Clear();
                        LRTBCheck();
                        i = 0;
                    }
                }

                // Top, Bottom 체크
                if (blocks[i].topBlock != null && blocks[i].bottomBlock != null)
                {
                    if (blocks[i].topBlock.blockType == blocks[i].blockType && blocks[i].bottomBlock.blockType == blocks[i].blockType)
                    {
                        curBlock = blocks[i];

                        yield return new WaitForSeconds(.4f);

                        // 풀에 반환
                        blockPool.ReturnPoolableObject(curBlock.topBlock);
                        blockPool.ReturnPoolableObject(curBlock.bottomBlock);
                        blockPool.ReturnPoolableObject(curBlock);

                        var col_B = curBlock.col;
                        var row_B = curBlock.topBlock.row;

                        // 맨 위 블럭인지 확인
                        if (row_B != (BoardSize.y - 1))
                        {
                            // 내릴 블럭 탐색
                            for (int j = 0; j < blocks.Count; j++)
                            {
                                if (blocks[j].col == col_B && blocks[j].row > row_B)
                                {
                                    checkBlocks.Add(blocks[j]);
                                }
                            }
                        }

                        yield return new WaitForSeconds(.4f);

                        // 블럭 내리는 작업
                        for (int j = 0; j < checkBlocks.Count; j++)
                        {
                            var targetRow = checkBlocks[j].row -= 3;

                            if (Mathf.Abs(targetRow - checkBlocks[j].transform.position.y) > .1f)
                            {
                                Vector2 tempPosition = new Vector2(checkBlocks[j].transform.position.x, targetRow);

                                checkBlocks[j].transform.position = Vector2.Lerp(checkBlocks[j].transform.position, tempPosition, .05f);
                            }
                        }

                        blocks.Remove(curBlock.topBlock);
                        blocks.Remove(curBlock.bottomBlock);
                        blocks.Remove(curBlock);

                        // 비어있는 칸 개수
                        var emptyBlockCount = size - (blocks.Count);

                        var n_Row = checkBlocks.Count > 0 ? checkBlocks[checkBlocks.Count - 1].row + 1 : row_B - 2;

                        yield return new WaitForSeconds(.4f);

                        for (int j = 0; j < emptyBlockCount; j++)
                        {
                            if (n_Row < BoardSize.y)
                            {
                                var newBlock = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                                newBlock.transform.position = new Vector3(col_B, n_Row, 0);
                                newBlock.gameObject.SetActive(true);
                                newBlock.Initialize(col_B, n_Row);

                                blocks.Add(newBlock);

                                n_Row++;
                            }
                        }

                        // 중요
                        checkBlocks.Clear();
                        LRTBCheck();
                        i = 0;
                    }
                }
            }

            yield return new WaitForSeconds(.4f);

            // 체크 종료
            isChecking = false;

            Debug.Log("BlockClear End");
        }
    }
}