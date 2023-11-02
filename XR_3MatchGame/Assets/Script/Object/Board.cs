using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using XR_3MatchGame.Util;
using XR_3MatchGame_InGame;
using XR_3MatchGame_Resource;
using XR_3MatchGame_Util;
using static UnityEngine.PlayerLoop.EarlyUpdate;

namespace XR_3MatchGame_Object
{
    public class Board : MonoBehaviour
    {
        #region public



        #endregion

        #region private 

        private GameManager GM;

        #endregion

        private void Start()
        {
            GM = GameManager.Instance;
            GM.Initialize(this);

            // 블럭을 화면에 세팅
            StartSpawn();
        }

        private void Update()
        {
            if (GM.isStart == true)
            {
                GM.isStart = false;
                StartCoroutine(BlockClear());
            }
        }

        /// <summary>
        /// 게임 시작 시 블럭을 화면에 세팅하는 메서드
        /// </summary>
        private void StartSpawn()
        {
            var blockPool = ObjectPoolManager.Instance.GetPool<Block>(PoolType.Block);
            var size = GM.BoardSize;

            // 블럭 세팅 작업
            for (int row = 0; row < size.y; row++)
            {
                for (int col = 0; col < size.x; col++)
                {
                    var block = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                    block.transform.position = new Vector3(col, row, 0);
                    block.Initialize(col, row);
                    block.gameObject.SetActive(true);

                    // GM에 저장
                    GM.blocks.Add(block);
                }
            }

            GM.GameStateUpdate(GameState.Checking);
            BlockUpdate();
            StartCoroutine(BlockClear());
        }

        /// <summary>
        /// 블럭의 주위의 블럭들에 대한 데이터를 업데이트하는 메서드
        /// </summary>
        public void BlockUpdate()
        {
            var blocks = GM.blocks;

            // 모든 블럭 탐색
            // Top = 6, Bottom = 0
            // Left = 0, Right = 6
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
        /// 폭탄 여부를 체크하는 메서드
        /// </summary>
        /// <param name="blocks">블럭 모음</param>
        /// <param name="curBlock">여부를 체크할 블럭</param>
        public void BoomCheck(Block curBlock)
        {
            var blocks = GM.blocks;
            var delBlocks = GM.delBlocks;

            #region Col 체크 (3:0, 2:1, 1:2. 0:3)

            // -1 -2 -3
            if (curBlock.blockType != BlockType.Boom)
            {
                delBlocks.Clear();

                for (int i = 0; i < blocks.Count; i++)
                {
                    if ((curBlock.col - 1 == blocks[i].col || curBlock.col - 2 == blocks[i].col || curBlock.col - 3 == blocks[i].col) && curBlock.row == blocks[i].row)
                    {
                        if (curBlock.blockType == blocks[i].blockType)
                        {
                            delBlocks.Add(blocks[i]);
                        }
                    }

                    if (delBlocks.Count == 3)
                    {
                        curBlock.blockType = BlockType.Boom;
                        curBlock.boomType = BoomType.ColBoom;

                        // 마지막 자리에 폭탄 저장
                        delBlocks.Add(curBlock);
                        return;
                    }
                }
            }

            // -1 -2 +1
            if (curBlock.blockType != BlockType.Boom)
            {
                delBlocks.Clear();

                for (int i = 0; i < blocks.Count; i++)
                {
                    if ((curBlock.col - 1 == blocks[i].col || curBlock.col - 2 == blocks[i].col || curBlock.col + 1 == blocks[i].col) && curBlock.row == blocks[i].row)
                    {
                        if (curBlock.blockType == blocks[i].blockType)
                        {
                            delBlocks.Add(blocks[i]);
                        }
                    }

                    if (delBlocks.Count == 3)
                    {
                        curBlock.blockType = BlockType.Boom;
                        curBlock.boomType = BoomType.ColBoom;

                        // 마지막 자리에 폭탄 저장
                        delBlocks.Add(curBlock);
                        return;
                    }
                }
            }

            // -1 +1 +2
            if (curBlock.blockType != BlockType.Boom)
            {
                delBlocks.Clear();

                for (int i = 0; i < blocks.Count; i++)
                {
                    if ((curBlock.col - 1 == blocks[i].col || curBlock.col + 1 == blocks[i].col || curBlock.col + 2 == blocks[i].col) && curBlock.row == blocks[i].row)
                    {
                        if (curBlock.blockType == blocks[i].blockType)
                        {
                            delBlocks.Add(blocks[i]);
                        }
                    }

                    if (delBlocks.Count == 3)
                    {
                        curBlock.blockType = BlockType.Boom;
                        curBlock.boomType = BoomType.ColBoom;

                        // 마지막 자리에 폭탄을 저장
                        delBlocks.Add(curBlock);
                        return;
                    }
                }
            }

            // +1 +2 +3
            if (curBlock.blockType != BlockType.Boom)
            {
                delBlocks.Clear();

                for (int i = 0; i < blocks.Count; i++)
                {
                    if ((curBlock.col + 1 == blocks[i].col || curBlock.col + 2 == blocks[i].col || curBlock.col + 3 == blocks[i].col) && curBlock.row == blocks[i].row)
                    {
                        if (curBlock.blockType == blocks[i].blockType)
                        {
                            delBlocks.Add(blocks[i]);
                        }
                    }

                    if (delBlocks.Count == 3)
                    {
                        curBlock.blockType = BlockType.Boom;
                        curBlock.boomType = BoomType.ColBoom;

                        // 마지막 자리에 폭탄을 저장
                        delBlocks.Add(curBlock);
                        return;
                    }
                }
            }

            #endregion

            #region Row 체크 (3:0, 2:1, 1:2, 0:3)

            if (curBlock.blockType != BlockType.Boom)
            {
                delBlocks.Clear();

                for (int i = 0; i < blocks.Count; i++)
                {
                    // -1 -2 -3
                    if ((curBlock.row - 1 == blocks[i].row || curBlock.row - 2 == blocks[i].row || curBlock.row - 3 == blocks[i].row) && curBlock.col == blocks[i].col)
                    {
                        if (curBlock.blockType == blocks[i].blockType)
                        {
                            delBlocks.Add(blocks[i]);
                        }
                    }

                    if (delBlocks.Count == 3)
                    {
                        curBlock.blockType = BlockType.Boom;
                        curBlock.boomType = BoomType.RowBoom;

                        // 마지막 자리에 폭탄을 저장
                        delBlocks.Add(curBlock);
                        return;
                    }
                }
            }

            if (curBlock.blockType != BlockType.Boom)
            {
                delBlocks.Clear();

                for (int i = 0; i < blocks.Count; i++)
                {
                    // -1 -2 +1
                    if ((curBlock.row - 1 == blocks[i].row || curBlock.row - 2 == blocks[i].row || curBlock.row + 1 == blocks[i].row) && curBlock.col == blocks[i].col)
                    {
                        if (curBlock.blockType == blocks[i].blockType)
                        {
                            delBlocks.Add(blocks[i]);
                        }
                    }

                    if (delBlocks.Count == 3)
                    {
                        curBlock.blockType = BlockType.Boom;
                        curBlock.boomType = BoomType.RowBoom;

                        // 마지막 자리에 폭탄을 저장
                        delBlocks.Add(curBlock);
                        return;
                    }
                }
            }

            if (curBlock.blockType != BlockType.Boom)
            {
                delBlocks.Clear();

                for (int i = 0; i < blocks.Count; i++)
                {
                    //-1 +1 +2
                    if ((curBlock.row - 1 == blocks[i].row || curBlock.row + 1 == blocks[i].row || curBlock.row + 2 == blocks[i].row) && curBlock.col == blocks[i].col)
                    {
                        if (curBlock.blockType == blocks[i].blockType)
                        {
                            delBlocks.Add(blocks[i]);
                        }
                    }

                    if (delBlocks.Count == 3)
                    {
                        curBlock.blockType = BlockType.Boom;
                        curBlock.boomType = BoomType.RowBoom;

                        // 마지막 자리에 폭탄을 저장
                        delBlocks.Add(curBlock);
                        return;
                    }
                }
            }

            if (curBlock.blockType != BlockType.Boom)
            {
                delBlocks.Clear();

                for (int i = 0; i < blocks.Count; i++)
                {
                    // +1 +2 +3
                    if ((curBlock.row + 1 == blocks[i].row || curBlock.row + 2 == blocks[i].row || curBlock.row + 3 == blocks[i].row) && curBlock.col == blocks[i].col)
                    {
                        if (curBlock.blockType == blocks[i].blockType)
                        {
                            delBlocks.Add(blocks[i]);
                        }
                    }

                    if (delBlocks.Count == 3)
                    {
                        curBlock.blockType = BlockType.Boom;
                        curBlock.boomType = BoomType.RowBoom;

                        // 마지막 자리에 폭탄을 저장
                        delBlocks.Add(curBlock);
                        return;
                    }
                }
            }

            #endregion
        }

        public IEnumerator BlockCheck(Block curBlock)
        {
            yield return null;
        }

        /// <summary>
        /// 블럭 클리어 및 블럭 생성을 담당하는 메서드
        /// </summary>
        /// <returns></returns>
        public IEnumerator BlockClear()
        {
            Block curBlock = null;
            var blockPool = ObjectPoolManager.Instance.GetPool<Block>(PoolType.Block);
            var size = (GM.BoardSize.x * GM.BoardSize.y);

            var blocks = GM.blocks;
            var delBlocks = GM.delBlocks;
            var downBlocks = GM.downBlocks;

            // 폭탄 검사 작업
            for (int i = 0; i < blocks.Count; i++)
            {
                if (delBlocks.Count != 4)
                {
                    BoomCheck(blocks[i]);
                }
            }

            // 폭탄 작업 
            if (delBlocks.Count != 0)
            {
                if (delBlocks[delBlocks.Count - 1].blockType == BlockType.Boom)
                {
                    // 폭탄은 마지막 인덱스에 존재
                    curBlock = delBlocks[delBlocks.Count - 1];
                    curBlock.spriteRenderer.sprite = SpriteLoader.GetSprite(AtlasType.BlockAtlas, curBlock.blockType.ToString());

                    yield return new WaitForSeconds(.4f);

                    switch (curBlock.boomType)
                    {
                        case BoomType.ColBoom:
                            var col_0 = delBlocks[0].col;
                            var col_1 = delBlocks[1].col;
                            var col_2 = delBlocks[2].col;

                            var row_B = delBlocks[0].row;

                            // 풀에 반환
                            for (int i = 0; i < delBlocks.Count - 1; i++)
                            {
                                blockPool.ReturnPoolableObject(delBlocks[i]);

                                // 점수 작업
                                GM.ScoreUpdate(delBlocks[i].BlockScore);
                            }

                            yield return new WaitForSeconds(.4f);

                            // 삭제한 블럭들 위에 존재하는 블럭들 찾기
                            if (row_B != (GM.BoardSize.y - 1))
                            {
                                for (int i = 0; i < blocks.Count; i++)
                                {
                                    if ((col_0 == blocks[i].col || col_1 == blocks[i].col || col_2 == blocks[i].col) && row_B < blocks[i].row)
                                    {
                                        // 내릴 블럭을 저장
                                        downBlocks.Add(blocks[i]);
                                    }
                                }
                            }

                            // 블럭 내려주는 작업
                            for (int i = 0; i < downBlocks.Count; i++)
                            {
                                var targetRow = downBlocks[i].row -= 1;

                                if (Mathf.Abs(targetRow - downBlocks[i].transform.position.y) > .1f)
                                {
                                    Vector2 tempPosition = new Vector2(downBlocks[i].transform.position.x, targetRow);
                                    downBlocks[i].transform.position = Vector2.Lerp(downBlocks[i].transform.position, tempPosition, .05f);
                                }
                            }

                            BlockUpdate();

                            blocks.Remove(delBlocks[0]);
                            blocks.Remove(delBlocks[1]);
                            blocks.Remove(delBlocks[2]);

                            // 생성 블럭들의 베이스가 될 Col, Row 값
                            var row_NewNum = downBlocks.Count > 0 ? downBlocks[downBlocks.Count - 1].row + 1 : GM.BoardSize.y - 1;

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

                            delBlocks.Clear();
                            downBlocks.Clear();
                            BlockUpdate();
                            break;

                        case BoomType.RowBoom:
                            var row_0 = delBlocks[0].row;    // 1
                            var row_1 = delBlocks[1].row;    // 2
                            var row_2 = delBlocks[2].row;    // 4
                            var row_3 = delBlocks[3].row;    // 3 폭탄 row

                            var col_B = delBlocks[0].col;

                            // 풀에 반환
                            for (int i = 0; i < delBlocks.Count - 1; i++)
                            {
                                blockPool.ReturnPoolableObject(delBlocks[i]);

                                GM.ScoreUpdate(delBlocks[i].BlockScore);
                            }

                            yield return new WaitForSeconds(.4f);

                            // 폭탄 내리는 작업
                            // 폭탄이 맨 아래 블럭인지 확인
                            if (row_3 > row_0)
                            {
                                var boomTargetRow = delBlocks[3].row = row_0;

                                if (Mathf.Abs(boomTargetRow - delBlocks[3].transform.position.y) > .1f)
                                {
                                    Vector2 tempPosition = new Vector2(delBlocks[3].transform.position.x, boomTargetRow);
                                    delBlocks[3].transform.position = Vector2.Lerp(delBlocks[3].transform.position, tempPosition, .05f);
                                }
                            }

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
                                            downBlocks.Add(blocks[i]);
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
                                            downBlocks.Add(blocks[i]);
                                        }
                                    }
                                }
                            }

                            for (int i = 0; i < downBlocks.Count; i++)
                            {
                                var targetRow = downBlocks[i].row -= 3;

                                if (Mathf.Abs(targetRow - downBlocks[i].transform.position.y) > .1f)
                                {
                                    Vector2 tempPosition = new Vector2(downBlocks[i].transform.position.x, targetRow);
                                    downBlocks[i].transform.position = Vector2.Lerp(downBlocks[i].transform.position, tempPosition, .05f);
                                }
                            }

                            blocks.Remove(delBlocks[0]);
                            blocks.Remove(delBlocks[1]);
                            blocks.Remove(delBlocks[2]);

                            yield return new WaitForSeconds(.4f);

                            // 새로운 Row 값
                            var newRow = downBlocks.Count > 0 ? downBlocks[downBlocks.Count - 1].row + 1 : delBlocks[3].row + 1;
                            var emptyBlockCount = size - blocks.Count;

                            for (int i = 0; i < emptyBlockCount; i++)
                            {
                                if (newRow < GM.BoardSize.y)
                                {
                                    var newBlock = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                                    newBlock.transform.position = new Vector3(col_B, newRow, 0);
                                    newBlock.gameObject.SetActive(true);
                                    newBlock.Initialize(col_B, newRow);
                                    blocks.Add(newBlock);

                                    newRow++;
                                }
                            }

                            BlockUpdate();
                            delBlocks.Clear();
                            downBlocks.Clear();
                            break;
                    }
                }
            }

            yield return new WaitForSeconds(.4f);

            // 일반 블럭 작업
            for (int i = 0; i < blocks.Count; i++)
            {
                curBlock = blocks[i];

                // Left, Right
                if (curBlock.leftBlock != null && curBlock.rightBlock != null)
                {
                    // 체크할 블럭

                    if (curBlock.leftBlock.blockType == curBlock.blockType && curBlock.rightBlock.blockType == curBlock.blockType)
                    {
                        // 삭제할 블럭들 삭제 저장소에 저장
                        delBlocks.Add(curBlock);
                        delBlocks.Add(curBlock.leftBlock);
                        delBlocks.Add(curBlock.rightBlock);

                        var col_L = curBlock.leftBlock.col;
                        var col_M = curBlock.col;
                        var col_R = curBlock.rightBlock.col;
                        var row_M = curBlock.row;

                        // 풀 반환 및 점수 업데이트
                        for (int j = 0; j < delBlocks.Count; j++)
                        {
                            blockPool.ReturnPoolableObject(delBlocks[j]);
                            GM.ScoreUpdate(delBlocks[j].BlockScore);
                            blocks.Remove(delBlocks[j]);
                        }

                        // 맨 위에 있는 블럭인지 확인
                        if (row_M != (GM.BoardSize.y - 1))
                        {
                            downBlocks.Clear();

                            // 삭제 블럭 위에 존재하는 블럭 탐색
                            for (int j = 0; j < blocks.Count; j++)
                            {
                                if ((blocks[j].col == col_L || blocks[j].col == col_M || blocks[j].col == col_R) && blocks[j].row > row_M)
                                {
                                    // 내릴 블럭 저장
                                    downBlocks.Add(blocks[j]);
                                }
                            }
                        }

                        yield return new WaitForSeconds(.4f);

                        // 블럭을 내리는 작업
                        for (int j = 0; j < downBlocks.Count; j++)
                        {
                            var targetRow = downBlocks[j].row -= 1;

                            if (Mathf.Abs(targetRow - downBlocks[j].transform.position.y) > .1f)
                            {
                                Vector2 tempPosition = new Vector2(downBlocks[j].transform.position.x, targetRow);
                                downBlocks[j].transform.position = Vector2.Lerp(downBlocks[j].transform.position, tempPosition, .05f);
                            }
                        }

                        // 비어있는 칸의 개수
                        var emptyCount = size - blocks.Count;
                        var col_NewNum = col_L;
                        var row_NewNum = downBlocks.Count > 0 ? downBlocks[downBlocks.Count - 1].row + 1 : row_M;

                        yield return new WaitForSeconds(.4f);

                        // 빈 공간에 블럭 생성 작업
                        for (int j = 0; j < emptyCount; j++)
                        {
                            if (col_NewNum <= col_R && row_NewNum < GM.BoardSize.y)
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

                        delBlocks.Clear();
                        downBlocks.Clear();
                        BlockUpdate();

                        // Left, Right 블럭 매칭이 끝나면 한번더 0부터 시작 -> 여기 한번 점검
                        /// i = 0;
                    }
                }

                // Top, Bottom
                if (curBlock.topBlock != null && curBlock.bottomBlock != null)
                {
                    if (curBlock.topBlock.blockType == curBlock.blockType && curBlock.bottomBlock.blockType == curBlock.blockType)
                    {
                        // curBlock = blocks[i];
                        delBlocks.Add(curBlock.topBlock);
                        delBlocks.Add(curBlock.bottomBlock);
                        delBlocks.Add(curBlock);

                        for (int j = 0; j < delBlocks.Count; j++)
                        {
                            blockPool.ReturnPoolableObject(delBlocks[j]);
                            GM.ScoreUpdate(delBlocks[j].BlockScore);
                            blocks.Remove(delBlocks[j]);
                        }

                        var col_B = curBlock.col;
                        var row_B = curBlock.topBlock.row;

                        downBlocks.Clear();

                        // 맨 위 블럭인지 확인
                        if (row_B != (GM.BoardSize.y - 1))
                        {
                            // 내릴 블럭 탐색
                            for (int j = 0; j < blocks.Count; j++)
                            {
                                if ((col_B == blocks[j].col) && (row_B < blocks[j].row))
                                {
                                    downBlocks.Add(blocks[j]);
                                }
                            }
                        }

                        yield return new WaitForSeconds(.4f);

                        // 블럭 내리는 작업
                        for (int j = 0; j < downBlocks.Count; j++)
                        {
                            var targetRow = downBlocks[j].row -= 3;

                            if (Mathf.Abs(targetRow - downBlocks[j].transform.position.y) > .1f)
                            {
                                Vector2 tempPosition = new Vector2(downBlocks[j].transform.position.x, targetRow);
                                downBlocks[j].transform.position = Vector2.Lerp(downBlocks[j].transform.position, tempPosition, .05f);
                            }
                        }

                        // 비어있는 칸 개수
                        var emptyBlockCount = size - blocks.Count;

                        var n_Row = downBlocks.Count > 0 ? downBlocks[downBlocks.Count - 1].row + 1 : row_B - 2;

                        yield return new WaitForSeconds(.4f);

                        for (int j = 0; j < emptyBlockCount; j++)
                        {
                            if (n_Row < GM.BoardSize.y)
                            {
                                var newBlock = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                                newBlock.transform.position = new Vector3(col_B, n_Row, 0);
                                newBlock.gameObject.SetActive(true);
                                newBlock.Initialize(col_B, n_Row);

                                blocks.Add(newBlock);

                                n_Row++;
                            }
                        }

                        delBlocks.Clear();
                        downBlocks.Clear();
                        BlockUpdate();

                        // Top, Bottom 블럭 매칭이 끝나면 한번더 0부터 시작
                        /// i = 0;
                    }
                }
            }

            yield return new WaitForSeconds(.4f);

            // 체크 종료
            GM.GameStateUpdate(GameState.Play);

            /*
            // 게임 클리어 조건
            if (GM.Score >= 100)
            {
                for (int i = 0; i < blocks.Count; i++)
                {
                    blockPool.ReturnPoolableObject(blocks[i]);
                }

                GM.GameStateUpdate(GameState.End);
            }
            */
        }
    }
}