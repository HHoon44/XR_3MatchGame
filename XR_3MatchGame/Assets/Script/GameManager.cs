using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using UnityEditor;
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
        /// <summary>
        /// 현재 게임 상태 프로퍼티
        /// </summary>
        public GameState GameState { get; private set; }

        /// <summary>
        /// 현재 게임의 점수 프로퍼티
        /// </summary>
        public int Score { get; private set; }

        /// <summary>
        /// 보드 컴포넌트 프로퍼티
        /// </summary>
        public Board Board { get; private set; }

        public List<Block> blocks = new List<Block>();              // 인 게임 내에서 모든 블럭을 담아놓을 리스트
        public List<Block> downBlock = new List<Block>();           // 내릴 블럭을 담아놓을 리스트
        public List<Block> delBlock = new List<Block>();            // 삭제할 블럭을 담아놓을 리스트

        #region Public

        public bool isStart = false;                                // 블럭 체크를 실행할것인가?
        public bool isChecking = false;                             // 현재 블럭 체크를 진행중인가?

        #endregion

        #region Private

        #endregion

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
            Initialize();
        }

        private void Initialize()
        {
            // 게임 시작
            GameState = GameState.Play;

            Board = GameObject.Find("Board").GetComponent<Board>();

            XR_3MatchGame_Resource.ResourceManager.Instance.Initialize();
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
        /// 블럭의 Top, Bottom, Left, Right를 확인하는 메서드
        /// </summary>
        public void TBLRCheck()
        {
            // 모든 블럭 탐색
            for (int i = 0; i < blocks.Count; i++)
            {
                for (int j = 0; j < blocks.Count; j++)
                {
                    // Top
                    if (blocks[i].row == 6)
                    {
                        blocks[i].topBlock = null;
                        blocks[i].Top_T = BlockType.None;
                    }
                    else
                    {
                        if (blocks[i].row + 1 == blocks[j].row && blocks[i].col == blocks[j].col)
                        {
                            blocks[i].topBlock = blocks[j];

                            // Test
                            blocks[i].Top_T = blocks[j].blockType;
                        }
                    }

                    // Bottom
                    if (blocks[i].row == 0)
                    {
                        blocks[i].bottomBlock = null;
                        blocks[i].Bottom_T = BlockType.None;
                    }
                    else
                    {
                        if (blocks[i].row - 1 == blocks[j].row && blocks[i].col == blocks[j].col)
                        {
                            blocks[i].bottomBlock = blocks[j];

                            // Test
                            blocks[i].Bottom_T = blocks[j].blockType;
                        }
                    }

                    // Left
                    if (blocks[i].col == 0)
                    {
                        // 현재 블럭은 Col = 0에 존재하는 블럭
                        blocks[i].leftBlock = null;
                        blocks[i].Left_T = BlockType.None;
                    }
                    else
                    {
                        if (blocks[i].col - 1 == blocks[j].col && blocks[i].row == blocks[j].row)
                        {
                            blocks[i].leftBlock = blocks[j];

                            // Test
                            blocks[i].Left_T = blocks[j].blockType;
                        }
                    }

                    // Right
                    if (blocks[i].col == 6)
                    {
                        // 현재 블럭은 Col = 6에 존재하는 블럭
                        blocks[i].rightBlock = null;
                        blocks[i].Right_T = BlockType.None;
                    }
                    else
                    {
                        if (blocks[i].col + 1 == blocks[j].col && blocks[i].row == blocks[j].row)
                        {
                            blocks[i].rightBlock = blocks[j];

                            // Test
                            blocks[i].Right_T = blocks[j].blockType;
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
            Block curBlock = null;
            var blockPool = ObjectPoolManager.Instance.GetPool<Block>(PoolType.Block);
            var size = (BoardSize.x * BoardSize.y);

            // 폭탄 작업 
            if (delBlock.Count != 0)
            {
                if (delBlock[delBlock.Count - 1].blockType == BlockType.Boom)
                {
                    // 폭탄은 마지막 인덱스에 존재
                    curBlock = delBlock[delBlock.Count - 1];
                    curBlock.spriteRenderer.sprite = SpriteLoader.GetSprite(AtlasType.BlockAtlas, curBlock.blockType.ToString());

                    yield return new WaitForSeconds(.4f);

                    switch (curBlock.boomType)
                    {
                        case BoomType.ColBoom:
                            var col_0 = delBlock[0].col;
                            var col_1 = delBlock[1].col;
                            var col_2 = delBlock[2].col;

                            var row_B = delBlock[0].row;

                            // 풀에 반환
                            for (int i = 0; i < delBlock.Count - 1; i++)
                            {
                                blockPool.ReturnPoolableObject(delBlock[i]);

                                // 점수 작업
                                Score += delBlock[i].BlockScore;
                            }

                            yield return new WaitForSeconds(.4f);

                            // 삭제한 블럭들 위에 존재하는 블럭들 찾기
                            if (row_B != (BoardSize.y - 1))
                            {
                                for (int i = 0; i < blocks.Count; i++)
                                {
                                    if ((col_0 == blocks[i].col || col_1 == blocks[i].col || col_2 == blocks[i].col) && row_B < blocks[i].row)
                                    {
                                        // 내릴 블럭을 저장
                                        downBlock.Add(blocks[i]);
                                    }
                                }
                            }

                            // 블럭 내려주는 작업
                            for (int i = 0; i < downBlock.Count; i++)
                            {
                                var targetRow = downBlock[i].row -= 1;

                                if (Mathf.Abs(targetRow - downBlock[i].transform.position.y) > .1f)
                                {
                                    Vector2 tempPosition = new Vector2(downBlock[i].transform.position.x, targetRow);
                                    downBlock[i].transform.position = Vector2.Lerp(downBlock[i].transform.position, tempPosition, .05f);
                                }
                            }

                            TBLRCheck();

                            blocks.Remove(delBlock[0]);
                            blocks.Remove(delBlock[1]);
                            blocks.Remove(delBlock[2]);

                            // 생성 블럭들의 베이스가 될 Col, Row 값
                            var row_NewNum = downBlock.Count > 0 ? downBlock[downBlock.Count - 1].row + 1 : BoardSize.y - 1;

                            yield return new WaitForSeconds(.4f);

                            var newBlock_0 = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                            newBlock_0.transform.position = new Vector3(col_0, row_NewNum, 0);
                            newBlock_0.gameObject.SetActive(true);
                            newBlock_0.Initialize(col_0, row_NewNum);
                            blocks.Add(newBlock_0);

                            var newBlock_1 = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                            newBlock_1.transform.position = new Vector3(col_1, row_NewNum, 0);
                            newBlock_1.gameObject.SetActive(true);
                            newBlock_1.Initialize(col_1, row_NewNum);
                            blocks.Add(newBlock_1);

                            var newBlock_2 = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                            newBlock_2.transform.position = new Vector3(col_2, row_NewNum, 0);
                            newBlock_2.gameObject.SetActive(true);
                            newBlock_2.Initialize(col_2, row_NewNum);
                            blocks.Add(newBlock_2);

                            delBlock.Clear();
                            downBlock.Clear();
                            TBLRCheck();
                            break;

                        case BoomType.RowBoom:
                            var row_0 = delBlock[0].row;    // 1
                            var row_1 = delBlock[1].row;    // 2
                            var row_2 = delBlock[2].row;    // 4
                            var row_3 = delBlock[3].row;    // 3 폭탄 row

                            var col_B = delBlock[0].col;

                            // 풀에 반환
                            for (int i = 0; i < delBlock.Count - 1; i++)
                            {
                                blockPool.ReturnPoolableObject(delBlock[i]);

                                Score += delBlock[i].BlockScore;
                            }

                            yield return new WaitForSeconds(.4f);

                            // 폭탄 내리는 작업
                            // 폭탄이 맨 아래 블럭인지 확인
                            if (row_3 > row_0)
                            {
                                var boomTargetRow = delBlock[3].row = row_0;

                                if (Mathf.Abs(boomTargetRow - delBlock[3].transform.position.y) > .1f)
                                {
                                    Vector2 tempPosition = new Vector2(delBlock[3].transform.position.x, boomTargetRow);
                                    delBlock[3].transform.position = Vector2.Lerp(delBlock[3].transform.position, tempPosition, .05f);
                                }
                            }

                            /// 삭제 블럭 위에 존재하는 블럭 찾기

                            // 맨 위 블럭이 아니라면 내릴 블럭 찾기 실행
                            if (row_3 != 6 && row_2 != 6)
                            {
                                if (row_2 > row_3)
                                {
                                    // 일반 블럭이 폭탄 블럭 보다 위에 있는 경우
                                    for (int i = 0; i < blocks.Count; i++)
                                    {
                                        // Row는 커야하고 Col은 같아야 한다
                                        if (row_2 < blocks[i].row && col_B == blocks[i].col)
                                        {
                                            downBlock.Add(blocks[i]);
                                        }
                                    }

                                }
                                else if (row_2 < row_3)
                                {
                                    // 폭탄 블럭이 일본 블럭 보다 위에 있는 경우
                                    for (int i = 0; i < blocks.Count; i++)
                                    {
                                        // Row는 커야하고 Col은 같아야 한단
                                        if (row_3 < blocks[i].row && col_B == blocks[i].col)
                                        {
                                            downBlock.Add(blocks[i]);
                                        }
                                    }
                                }
                            }

                            for (int i = 0; i < downBlock.Count; i++)
                            {
                                var targetRow = downBlock[i].row -= 3;

                                if (Mathf.Abs(targetRow - downBlock[i].transform.position.y) > .1f)
                                {
                                    Vector2 tempPosition = new Vector2(downBlock[i].transform.position.x, targetRow);
                                    downBlock[i].transform.position = Vector2.Lerp(downBlock[i].transform.position, tempPosition, .05f);
                                }
                            }

                            blocks.Remove(delBlock[0]);
                            blocks.Remove(delBlock[1]);
                            blocks.Remove(delBlock[2]);

                            yield return new WaitForSeconds(.4f);

                            // 새로운 Row 값
                            var newRow = downBlock.Count > 0 ? downBlock[downBlock.Count - 1].row + 1 : delBlock[3].row + 1;
                            var emptyBlockCount = size - blocks.Count;

                            for (int i = 0; i < emptyBlockCount; i++)
                            {
                                if (newRow < BoardSize.y)
                                {
                                    var newBlock = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                                    newBlock.transform.position = new Vector3(col_B, newRow, 0);
                                    newBlock.gameObject.SetActive(true);
                                    newBlock.Initialize(col_B, newRow);
                                    blocks.Add(newBlock);

                                    newRow++;
                                }
                            }

                            TBLRCheck();
                            delBlock.Clear();
                            downBlock.Clear();
                            break;
                    }
                }
            }

            yield return new WaitForSeconds(.4f);

            // 일반 블럭 작업
            for (int i = 0; i < blocks.Count; i++)
            {
                // Left, Right
                if (blocks[i].leftBlock != null && blocks[i].rightBlock != null)
                {
                    // 체크할 블럭
                    curBlock = blocks[i];

                    if (curBlock.leftBlock.blockType == curBlock.blockType && curBlock.rightBlock.blockType == curBlock.blockType)
                    {
                        // 삭제할 블럭들 삭제 저장소에 저장
                        delBlock.Add(curBlock);
                        delBlock.Add(curBlock.leftBlock);
                        delBlock.Add(curBlock.rightBlock);

                        var col_L = curBlock.leftBlock.col;
                        var col_M = curBlock.col;
                        var col_R = curBlock.rightBlock.col;
                        var row_M = curBlock.row;

                        // 풀 반환 및 점수 업데이트
                        for (int j = 0; j < delBlock.Count; j++)
                        {
                            blockPool.ReturnPoolableObject(delBlock[i]);
                            ScoreUpdate(delBlock[j].BlockScore);
                            blocks.Remove(delBlock[j]);
                        }

                        // 맨 위에 있는 블럭인지 확인
                        if (row_M != (BoardSize.y - 1))
                        {
                            downBlock.Clear();

                            // 삭제 블럭 위에 존재하는 블럭 탐색
                            for (int j = 0; j < blocks.Count; j++)
                            {
                                if ((blocks[j].col == col_L || blocks[j].col == col_M || blocks[j].col == col_R) && blocks[j].row > row_M)
                                {
                                    // 내릴 블럭 저장
                                    downBlock.Add(blocks[j]);
                                }
                            }
                        }

                        yield return new WaitForSeconds(.4f);

                        // 블럭을 내리는 작업
                        for (int j = 0; j < downBlock.Count; j++)
                        {
                            var targetRow = downBlock[j].row -= 1;

                            if (Mathf.Abs(targetRow - downBlock[j].transform.position.y) > .1f)
                            {
                                Vector2 tempPosition = new Vector2(downBlock[j].transform.position.x, targetRow);
                                downBlock[j].transform.position = Vector2.Lerp(downBlock[j].transform.position, tempPosition, .05f);
                            }
                        }

                        // 비어있는 칸의 개수
                        var emptyCount = size - blocks.Count;
                        var col_NewNum = col_L;
                        var row_NewNum = downBlock.Count > 0 ? downBlock[downBlock.Count - 1].row + 1 : row_M;

                        yield return new WaitForSeconds(.4f);

                        // 빈 공간에 블럭 생성 작업
                        for (int j = 0; j < emptyCount; j++)
                        {
                            if (col_NewNum <= col_R && row_NewNum < BoardSize.y)
                            {
                                var newBlock = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                                newBlock.transform.position = new Vector3(col_NewNum, row_NewNum, 0);
                                newBlock.gameObject.SetActive(true);
                                newBlock.Initialize(col_NewNum, row_NewNum);

                                blocks.Add(newBlock);

                                col_NewNum++;
                            }

                            if (col_NewNum > col_R)
                            {
                                // 다음 줄을 채우기 위한 작업
                                col_NewNum = col_L;
                                row_NewNum++;
                            }
                        }

                        delBlock.Clear();
                        downBlock.Clear();
                        TBLRCheck();

                        // Left, Right 블럭 매칭이 끝나면 한번더 0부터 시작 -> 여기 한번 점검
                        // i = 0;
                    }
                }

                yield return new WaitForSeconds(.4f);

                // Top, Bottom
                if (blocks[i].topBlock != null && blocks[i].bottomBlock != null)
                {
                    if (blocks[i].topBlock.blockType == blocks[i].blockType && blocks[i].bottomBlock.blockType == blocks[i].blockType)
                    {
                        // curBlock = blocks[i];
                        delBlock.Add(curBlock.topBlock);
                        delBlock.Add(curBlock.bottomBlock);
                        delBlock.Add(curBlock);

                        for (int j = 0; j < delBlock.Count; j++)
                        {
                            blockPool.ReturnPoolableObject(delBlock[i]);
                            ScoreUpdate(delBlock[j].BlockScore);
                            blocks.Remove(delBlock[j]);
                        }

                        var col_B = curBlock.col;
                        var row_B = curBlock.topBlock.row;

                        downBlock.Clear();

                        // 맨 위 블럭인지 확인
                        if (row_B != (BoardSize.y - 1))
                        {
                            // 내릴 블럭 탐색
                            for (int j = 0; j < blocks.Count; j++)
                            {
                                if ((col_B == blocks[j].col) && (row_B < blocks[j].row))
                                {
                                    downBlock.Add(blocks[j]);
                                }
                            }
                        }

                        yield return new WaitForSeconds(.4f);

                        // 블럭 내리는 작업
                        for (int j = 0; j < downBlock.Count; j++)
                        {
                            var targetRow = downBlock[j].row -= 3;

                            if (Mathf.Abs(targetRow - downBlock[j].transform.position.y) > .1f)
                            {
                                Vector2 tempPosition = new Vector2(downBlock[j].transform.position.x, targetRow);
                                downBlock[j].transform.position = Vector2.Lerp(downBlock[j].transform.position, tempPosition, .05f);
                            }
                        }

                        // 비어있는 칸 개수
                        var emptyBlockCount = size - blocks.Count;

                        var n_Row = downBlock.Count > 0 ? downBlock[downBlock.Count - 1].row + 1 : row_B - 2;

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

                        delBlock.Clear();
                        downBlock.Clear();
                        TBLRCheck();

                        // Top, Bottom 블럭 매칭이 끝나면 한번더 0부터 시작
                        // i = 0;
                    }
                }
            }

            yield return new WaitForSeconds(.4f);

            // 체크 종료
            isChecking = false;

            // 게임 클리어 조건
            if (Score >= 100)
            {
                for (int i = 0; i < blocks.Count; i++)
                {
                    blockPool.ReturnPoolableObject(blocks[i]);
                }

                GameState = GameState.End;
            }
        }

        /// <summary>
        /// 스코어를 업데이트 하는 메서드
        /// </summary>
        /// <param name="score">스코어</param>
        private void ScoreUpdate(int score)
        {
            Score += score;
        }
    }
}