using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;
using XR_3MatchGame.Util;
using XR_3MatchGame_InGame;
using XR_3MatchGame_Resource;
using XR_3MatchGame_Util;

namespace XR_3MatchGame_Object
{
    public class Block : MonoBehaviour, IPoolableObject
    {
        public bool CanRecycle { get; set; } = true;

        public int BlockScore 
        {
            get
            {
                int blockScore = 5;
                return blockScore;
            }
        }

        public SpriteRenderer spriteRenderer;

        public int col;     // 현재 블럭의 X 값
        public int row;     // 현재 블럭의 Y 값

        public int targetCol;       // 상대 블럭의 X 값
        public int targetRow;       // 상대 블럭의 Y 값

        public Block topBlock;      // 위에 존재하는 블럭
        public Block bottomBlock;   // 아래에 존재하는 블럭
        public Block leftBlock;     // 왼쪽에 존재하는 블럭
        public Block rightBlock;    // 오른쪽에 존재하는 블럭

        private float swipeAngle = 0;           // 스와이프 각도

        private Vector2 firstTouchPosition;     // 마우스 클릭 지점
        private Vector2 finalTouchPosition;     // 마우스 클릭을 마무리한 지점
        private Vector2 tempPosition;

        private Block otherBlock;               // 현재 블럭과 자리를 바꿀 블럭
        private GameManager gm;

        public BlockType blockType = BlockType.None;        // 현재 블럭의 타입
        public BoomType boomType = BoomType.None;           // 만약에 블럭이 폭탄이라면 어떤 폭탄인지에 대한 타입
        private SwipeDir swipeDir = SwipeDir.None;

        // Test
        public BlockType topType = BlockType.None;
        public BlockType bottomType = BlockType.None;
        public BlockType leftType = BlockType.None;
        public BlockType rightType = BlockType.None;

        /// <summary>
        /// 블럭 초기 세팅 메서드
        /// </summary>
        /// <param name="col">X 값</param>
        /// <param name="row">Y 값</param>
        public void Initialize(int col, int row)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            gm = GameManager.Instance;

            var blockNum = Random.Range(1, gm.BoardSize.x);

            // 랜덤으로 블럭의 스프라이트를 설정
            switch (blockNum)
            {
                case (int)BlockType.Blue:
                    spriteRenderer.sprite = SpriteLoader.GetSprite(AtlasType.BlockAtlas, BlockType.Blue.ToString());
                    blockType = BlockType.Blue;
                    break;

                case (int)BlockType.Cream:
                    spriteRenderer.sprite = SpriteLoader.GetSprite(AtlasType.BlockAtlas, BlockType.Cream.ToString());
                    blockType = BlockType.Cream;
                    break;

                case (int)BlockType.DarkBlue:
                    spriteRenderer.sprite = SpriteLoader.GetSprite(AtlasType.BlockAtlas, BlockType.DarkBlue.ToString());
                    blockType = BlockType.DarkBlue;
                    break;

                case (int)BlockType.Green:
                    spriteRenderer.sprite = SpriteLoader.GetSprite(AtlasType.BlockAtlas, BlockType.Green.ToString());
                    blockType = BlockType.Green;
                    break;

                case (int)BlockType.Pink:
                    spriteRenderer.sprite = SpriteLoader.GetSprite(AtlasType.BlockAtlas, BlockType.Pink.ToString());
                    blockType = BlockType.Pink;
                    break;

                case (int)BlockType.Purple:
                    spriteRenderer.sprite = SpriteLoader.GetSprite(AtlasType.BlockAtlas, BlockType.Purple.ToString());
                    blockType = BlockType.Purple;
                    break;
            }

            this.col = col;
            this.row = row;

            targetCol = col;
            targetRow = row;
        }

        private void Update()
        {
            targetCol = col;
            targetRow = row;

            BlockSwipe();
        }

        private void OnMouseDown()
        {
            // 마우스 클릭 위치 저장
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        private void OnMouseUp()
        {
            // 마우스 클릭 끝난 위치 저장
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }

        /// <summary>
        /// 마우스 드래그 각도를 계산하는 메서드
        /// </summary>
        private void CalculateAngle()
        {
            // 체크중 일땐 입력 막기
            if (gm.isChecking == true)
            {
                return;
            }

            // 마우스 드래그 각도를 계산합니다
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y,
                finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;

            BlockMove();
        }

        /// <summary>
        /// 블럭을 스와이프를 체크하는 메서드
        /// </summary>
        private void BlockSwipe()
        {
            if (Mathf.Abs(targetCol - transform.position.x) > .1f)
            {
                tempPosition = new Vector2(targetCol, transform.position.y);
                transform.position = Vector2.Lerp(transform.position, tempPosition, .3f);
            }
            else
            {
                tempPosition = new Vector2(targetCol, transform.position.y);
                transform.position = tempPosition;
            }

            if (Mathf.Abs(targetRow - transform.position.y) > .1f)
            {
                tempPosition = new Vector2(transform.position.x, targetRow);
                transform.position = Vector2.Lerp(transform.position, tempPosition, .3f);
            }
            else
            {
                tempPosition = new Vector2(transform.position.x, targetRow);
                transform.position = tempPosition;
            }
        }

        /// <summary>
        /// 계산한 각도를 이용해서 블럭을 이동시키는 메서드
        /// </summary>
        private void BlockMove()
        {
            gm.isChecking = true;

            // Top
            if ((swipeAngle > 45 && swipeAngle <= 135) && row < gm.BoardSize.y)
            {
                for (int i = 0; i < gm.blocks.Count; i++)
                {
                    if (gm.blocks[i].col == col &&
                        gm.blocks[i].row == row + 1)
                    {
                        // 위쪽 이동이므로 목표 블럭은 -1 이동
                        // 위쪽 이동이므로 이동 블럭은 +1 이동
                        otherBlock = gm.blocks[i];
                        otherBlock.row -= 1;
                        row += 1;

                        swipeDir = SwipeDir.Top;
                        StartCoroutine(BlockCheck());
                        return;
                    }
                }
            }
            // Bottom
            else if ((swipeAngle < -45 && swipeAngle >= -135) && row > 0)
            {
                for (int i = 0; i < gm.blocks.Count; i++)
                {
                    if (gm.blocks[i].col == col &&
                        gm.blocks[i].row == row - 1)
                    {
                        // 아래쪽 이동이므로 목표 블럭은 + 1 이동
                        // 아래쪽 이동이므로 이동 블럭은 - 1 이동
                        otherBlock = gm.blocks[i];
                        otherBlock.row += 1;
                        row -= 1;

                        swipeDir = SwipeDir.Bottom;
                        StartCoroutine(BlockCheck());
                        return;
                    }
                }
            }
            // Left
            else if ((swipeAngle > 135 || swipeAngle <= -135) && col > 0)
            {
                for (int i = 0; i < gm.blocks.Count; i++)
                {
                    if (gm.blocks[i].col == col - 1 &&
                        gm.blocks[i].row == row)
                    {
                        // 왼쪽 이동이므로 목표 블럭은 + 1 이동
                        // 왼쪽 이동이므로 이동 블럭은 - 1 이동
                        otherBlock = gm.blocks[i];
                        otherBlock.col += 1;
                        col -= 1;

                        swipeDir = SwipeDir.Left;
                        StartCoroutine(BlockCheck());
                        return;
                    }
                }
            }
            // Right
            else if ((swipeAngle > -45 && swipeAngle <= 45) && col < gm.BoardSize.x)
            {
                for (int i = 0; i < gm.blocks.Count; i++)
                {
                    if (gm.blocks[i].col == col + 1 &&
                        gm.blocks[i].row == row)
                    {
                        // 오른쪽 이동이므로 목표 블럭은 - 1 이동
                        // 오른쪽 이동이므로 이동 블럭은 + 1 이동
                        otherBlock = gm.blocks[i];
                        otherBlock.col -= 1;
                        col += 1;

                        swipeDir = SwipeDir.Right;
                        StartCoroutine(BlockCheck());
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 스와이프된 블럭을 체킹하는 메서드
        /// </summary>
        /// <returns></returns>
        private IEnumerator BlockCheck()
        {
            gm.LRTBCheck();

            var blocks = gm.blocks;

            // 같은 블럭 개수
            var count_T = 0;
            var count_B = 0;
            var count_M = 0;
            var count_L = 0;
            var count_R = 0;

            // 유저가 옮긴 블럭에 대한 로직
            switch (swipeDir)
            {
                case SwipeDir.Top:
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        // Top
                        if ((row + 1 == blocks[i].row || row + 2 == blocks[i].row) && col == blocks[i].col)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                count_T++;
                            }
                        }

                        // Middle
                        if ((col - 1 == blocks[i].col || col + 1 == blocks[i].col) && row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                count_M++;
                            }
                        }

                        // Left
                        if ((col - 1 == blocks[i].col || col - 2 == blocks[i].col) && row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                count_L++;
                            }
                        }

                        // Right
                        if ((col + 1 == blocks[i].col || col + 2 == blocks[i].col) && row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                count_R++;
                            }
                        }
                    }

                    // 이동한 위치에 매칭되는 블럭이 없다면
                    if (count_T < 2 && count_M < 2 && count_L < 2 && count_R < 2)
                    {
                        yield return new WaitForSeconds(.2f);

                        // OtherBlock의 매칭 여부 판단
                        if (OtherBlockCheck(count_T, count_B, count_M, count_L, count_R, blocks))
                        {
                            // 블럭 원위치
                            otherBlock.row += 1;
                            row -= 1;

                            yield return new WaitForSeconds(.5f);

                            gm.isChecking = false;
                        }
                        else
                        {
                            // OtherBlock 폭탄 여부 체크
                            BoomCheck(blocks, otherBlock);

                            // 블럭 매칭 시작
                            gm.isStart = true;
                        }
                    }
                    else
                    {
                        // 블럭 폭탄 여부 체크
                        BoomCheck(blocks, this);

                        // 블럭 매칭 시작
                        gm.isStart = true;
                    }
                    break;

                case SwipeDir.Bottom:
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        // Bottom
                        if ((row - 1 == blocks[i].row || row - 2 == blocks[i].row) && col == blocks[i].col)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                count_B++;
                            }
                        }

                        // Middle
                        if ((col - 1 == blocks[i].col || col + 1 == blocks[i].col) && row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                count_M++;
                            }
                        }

                        // Left
                        if ((col - 1 == blocks[i].col || col - 2 == blocks[i].col) && row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                count_L++;
                            }
                        }

                        //Right
                        if ((col + 1 == blocks[i].col || col + 2 == blocks[i].col) && row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                count_R++;
                            }
                        }
                    }

                    if (count_B < 2 && count_M < 2 && count_L < 2 && count_R < 2)
                    {
                        yield return new WaitForSeconds(.2f);

                        // OtherBlock 매칭 여부 판단
                        if (OtherBlockCheck(count_T, count_B, count_M, count_L, count_R, blocks))
                        {
                            otherBlock.row -= 1;
                            row += 1;

                            yield return new WaitForSeconds(.5f);

                            gm.isChecking = false;
                        }
                        else
                        {
                            // OtherBlock이 폭탄이 될 수 있는가 체크
                            BoomCheck(blocks, otherBlock);

                            // OtherBlock에 매칭되는 블럭이 존재
                            gm.isStart = true;
                        }
                    }
                    else
                    {
                        // 현재 블럭이 폭탄이 될 수 있는가 체크
                        BoomCheck(blocks, this);

                        // 현재 블럭과 매칭되는 블럭이 존재
                        gm.isStart = true;
                    }
                    break;

                case SwipeDir.Left:
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        // Top
                        if ((row + 1 == blocks[i].row || row + 2 == blocks[i].row) && col == blocks[i].col)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                count_T++;
                            }
                        }

                        // Bottom
                        if ((row - 1 == blocks[i].row || row - 2 == blocks[i].row) && col == blocks[i].col)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                count_B++;
                            }
                        }

                        // Middle
                        if ((row - 1 == blocks[i].row || row + 1 == blocks[i].row) && col == blocks[i].col)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                count_M++;
                            }
                        }

                        // Left
                        if ((col - 1 == blocks[i].col || col - 2 == blocks[i].col) && row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                count_L++;
                            }
                        }
                    }

                    if (count_T < 2 && count_B < 2 && count_M < 2 && count_L < 2)
                    {
                        yield return new WaitForSeconds(.2f);

                        // OtherBlock도 매칭되는 블럭이 있는지 확인
                        if (OtherBlockCheck(count_T, count_B, count_M, count_L, count_R, blocks))
                        {
                            otherBlock.col -= 1;
                            col += 1;

                            yield return new WaitForSeconds(.5f);

                            gm.isChecking = false;
                        }
                        else
                        {
                            // OtherBlock이 폭탄이 될 수 있는가 체크
                            BoomCheck(blocks, otherBlock);

                            // OtherBlock에 매칭되는 블럭이 존재
                            gm.isStart = true;
                        }
                    }
                    else
                    {
                        // 현재 블럭이 폭탄이 될 수 있는가 체크
                        BoomCheck(blocks, this);

                        // 현재 블럭과 매칭되는 블럭이 존재
                        gm.isStart = true;
                    }
                    break;

                case SwipeDir.Right:
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        // Top
                        if ((row + 1 == blocks[i].row || row + 2 == blocks[i].row) && col == blocks[i].col)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                count_T++;
                            }
                        }

                        // Bottom
                        if ((row - 1 == blocks[i].row || row - 2 == blocks[i].row) && col == blocks[i].col)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                count_B++;
                            }
                        }

                        // Middle
                        if ((row - 1 == blocks[i].row || row + 1 == blocks[i].row) && col == blocks[i].col)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                count_M++;
                            }
                        }

                        // Right
                        if ((col + 1 == blocks[i].col || col + 2 == blocks[i].col) && row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                count_R++;
                            }
                        }
                    }

                    if (count_T < 2 && count_B < 2 && count_M < 2 && count_R < 2)
                    {
                        yield return new WaitForSeconds(.2f);

                        // OtherBlock 매칭 여부 판단
                        if (OtherBlockCheck(count_T, count_B, count_M, count_L, count_R, blocks))
                        {
                            otherBlock.col += 1;
                            col -= 1;

                            yield return new WaitForSeconds(.5f);

                            gm.isChecking = false;
                        }
                        else
                        {
                            // OtherBlock 폭탄 여부 체크
                            BoomCheck(blocks, otherBlock);

                            // OtherBlock 매칭 시작
                            gm.isStart = true;
                        }
                    }
                    else
                    {
                        // 현재 블럭 폭탄 여부 체크
                        BoomCheck(blocks, this);

                        // 현재 블럭 매칭 시작
                        gm.isStart = true;
                    }
                    break;
            }

            gm.LRTBCheck();
        }

        /// <summary>
        /// OtherBlock 매칭을 탐색해주는 메서드
        /// </summary>
        /// <param name="count_T">Top 개수</param>
        /// <param name="count_B">Bottom 개수</param>
        /// <param name="count_M">Middle 개수</param>
        /// <param name="count_L">Left 개수</param>
        /// <param name="count_R">Right 개수</param>
        /// <returns></returns>
        private bool OtherBlockCheck(int count_T, int count_B, int count_M, int count_L, int count_R, List<Block> blocks)
        {
            // 재사용하기 위해 0으로 초기화
            count_T = 0;
            count_B = 0;
            count_M = 0;
            var count_M2 = 0;
            count_L = 0;
            count_R = 0;

            // OtherBlock 매칭 블럭 탐색 작업
            for (int i = 0; i < blocks.Count; i++)
            {
                // Top
                if ((otherBlock.row + 1 == blocks[i].row || otherBlock.row + 2 == blocks[i].row) && otherBlock.col == blocks[i].col)
                {
                    if (otherBlock.blockType == blocks[i].blockType)
                    {
                        count_T++;
                    }
                }

                // Horizontal Middle
                if ((otherBlock.col + 1 == blocks[i].col || otherBlock.col - 1 == blocks[i].col) && otherBlock.row == blocks[i].row)
                {
                    if (otherBlock.blockType == blocks[i].blockType)
                    {
                        count_M++;
                    }
                }

                // Vertical Middle
                // Horizontal에서 매칭되는 블럭이 없으므로 재사용
                if ((otherBlock.row + 1 == blocks[i].row || otherBlock.row - 1 == blocks[i].row) && otherBlock.col == blocks[i].col)
                {
                    if (otherBlock.blockType == blocks[i].blockType)
                    {
                        count_M2++;
                    }
                }

                // Bottom
                if ((otherBlock.row - 1 == blocks[i].row || otherBlock.row - 2 == blocks[i].row) && otherBlock.col == blocks[i].col)
                {
                    if (otherBlock.blockType == blocks[i].blockType)
                    {
                        count_B++;
                    }
                }

                // Left
                if ((otherBlock.col - 1 == blocks[i].col || otherBlock.col - 2 == blocks[i].col) && otherBlock.row == blocks[i].row)
                {
                    if (otherBlock.blockType == blocks[i].blockType)
                    {
                        count_L++;
                    }
                }

                // Right
                if ((otherBlock.col + 1 == blocks[i].col || otherBlock.col + 2 == blocks[i].col) && otherBlock.row == blocks[i].row)
                {
                    if (otherBlock.blockType == blocks[i].blockType)
                    {
                        count_R++;
                    }
                }
            }

            if (count_T < 2 && count_M < 2 && count_M2 < 2 && count_B < 2 && count_L < 2 && count_R < 2)
            {
                // 매칭 발생 안함
                return true;
            }
            else
            {
                // 매칭 발생
                return false;
            }
        }

        /// <summary>
        /// 폭탄 여부를 체크하는 메서드
        /// </summary>
        /// <param name="blocks">블럭 모음</param>
        /// <param name="curBlock">여부를 체크할 블럭</param>
        private void BoomCheck(List<Block> blocks, Block curBlock)
        {
            #region Col 체크 (3:0, 2:1, 1:2. 0:3)

            if (curBlock.blockType != BlockType.Boom)
            {
                gm.delBlock.Clear();

                for (int i = 0; i < blocks.Count; i++)
                {
                    // -1 -2 -3 탐색
                    if ((curBlock.col - 1 == blocks[i].col || curBlock.col - 2 == blocks[i].col || curBlock.col - 3 == blocks[i].col) && curBlock.row == blocks[i].row)
                    {
                        if (curBlock.blockType == blocks[i].blockType)
                        {
                            gm.delBlock.Add(blocks[i]);
                        }
                    }

                    if (gm.delBlock.Count == 3)
                    {
                        curBlock.blockType = BlockType.Boom;
                        curBlock.boomType = BoomType.ColBoom;

                        // 마지막 자리에 폭탄 저장
                        gm.delBlock.Add(curBlock);
                        return;
                    }
                }
            }

            if (curBlock.blockType != BlockType.Boom)
            {
                gm.delBlock.Clear();

                for (int i = 0; i < blocks.Count; i++)
                {
                    // -1 -2 +1 탐색
                    if ((curBlock.col - 1 == blocks[i].col || curBlock.col - 2 == blocks[i].col || curBlock.col + 1 == blocks[i].col) && curBlock.row == blocks[i].row)
                    {
                        if (curBlock.blockType == blocks[i].blockType)
                        {
                            gm.delBlock.Add(blocks[i]);
                        }
                    }

                    if (gm.delBlock.Count == 3)
                    {
                        curBlock.blockType = BlockType.Boom;
                        curBlock.boomType = BoomType.ColBoom;

                        // 마지막 자리에 폭탄 저장
                        gm.delBlock.Add(curBlock);
                        return;
                    }
                }
            }

            if (curBlock.blockType != BlockType.Boom)
            {
                gm.delBlock.Clear();

                for (int i = 0; i < blocks.Count; i++)
                {
                    // -1 +1 +2
                    if ((curBlock.col - 1 == blocks[i].col || curBlock.col + 1 == blocks[i].col || curBlock.col + 2 == blocks[i].col) && curBlock.row == blocks[i].row)
                    {
                        if (curBlock.blockType == blocks[i].blockType)
                        {
                            gm.delBlock.Add(blocks[i]);
                        }
                    }

                    if (gm.delBlock.Count == 3)
                    {
                        curBlock.blockType = BlockType.Boom;
                        curBlock.boomType = BoomType.ColBoom;

                        // 마지막 자리에 폭탄을 저장
                        gm.delBlock.Add(curBlock);
                        return;
                    }
                }
            }

            if (curBlock.blockType != BlockType.Boom)
            {
                gm.delBlock.Clear();

                for (int i = 0; i < blocks.Count; i++)
                {
                    // +1 +2 +3
                    if ((curBlock.col + 1 == blocks[i].col || curBlock.col + 2 == blocks[i].col || curBlock.col + 3 == blocks[i].col) && curBlock.row == blocks[i].row)
                    {
                        if (curBlock.blockType == blocks[i].blockType)
                        {
                            gm.delBlock.Add(blocks[i]);
                        }
                    }

                    if (gm.delBlock.Count == 3)
                    {
                        curBlock.blockType = BlockType.Boom;
                        curBlock.boomType = BoomType.ColBoom;

                        // 마지막 자리에 폭탄을 저장
                        gm.delBlock.Add(curBlock);
                        return;
                    }
                }
            }

            #endregion

            #region Row 체크 (3:0, 2:1, 1:2, 0:3)

            if (curBlock.blockType != BlockType.Boom)
            {
                gm.delBlock.Clear();

                for (int i = 0; i < blocks.Count; i++)
                {
                    // -1 -2 -3
                    if ((curBlock.row - 1 == blocks[i].row || curBlock.row - 2 == blocks[i].row || curBlock.row - 3 == blocks[i].row) && curBlock.col == blocks[i].col)
                    {
                        if (curBlock.blockType == blocks[i].blockType)
                        {
                            gm.delBlock.Add(blocks[i]);
                        }
                    }

                    if (gm.delBlock.Count == 3)
                    {
                        curBlock.blockType = BlockType.Boom;
                        curBlock.boomType = BoomType.RowBoom;

                        // 마지막 자리에 폭탄을 저장
                        gm.delBlock.Add(curBlock);
                        return;
                    }
                }
            }

            if (curBlock.blockType != BlockType.Boom)
            {
                gm.delBlock.Clear();

                for (int i = 0; i < blocks.Count; i++)
                {
                    // -1 -2 +1
                    if ((curBlock.row - 1 == blocks[i].row || curBlock.row - 2 == blocks[i].row || curBlock.row + 1 == blocks[i].row) && curBlock.col == blocks[i].col)
                    {
                        if (curBlock.blockType == blocks[i].blockType)
                        {
                            gm.delBlock.Add(blocks[i]);
                        }
                    }

                    if (gm.delBlock.Count == 3)
                    {
                        curBlock.blockType = BlockType.Boom;
                        curBlock.boomType = BoomType.RowBoom;

                        // 마지막 자리에 폭탄을 저장
                        gm.delBlock.Add(curBlock);
                        return;
                    }
                }
            }

            if (curBlock.blockType != BlockType.Boom)
            {
                gm.delBlock.Clear();

                for (int i = 0; i < blocks.Count; i++)
                {
                    //-1 +1 +2
                    if ((curBlock.row - 1 == blocks[i].row || curBlock.row + 1 == blocks[i].row || curBlock.row + 2 == blocks[i].row) && curBlock.col == blocks[i].col)
                    {
                        if (curBlock.blockType == blocks[i].blockType)
                        {
                            gm.delBlock.Add(blocks[i]);
                        }
                    }

                    if (gm.delBlock.Count == 3)
                    {
                        curBlock.blockType = BlockType.Boom;
                        curBlock.boomType = BoomType.RowBoom;

                        // 마지막 자리에 폭탄을 저장
                        gm.delBlock.Add(curBlock);
                        return;
                    }
                }
            }

            if (curBlock.blockType != BlockType.Boom)
            {
                gm.delBlock.Clear();

                for (int i = 0; i < blocks.Count; i++)
                {
                    // +1 +2 +3
                    if ((curBlock.row + 1 == blocks[i].row || curBlock.row + 2 == blocks[i].row || curBlock.row + 3 == blocks[i].row) && curBlock.col == blocks[i].col)
                    {
                        if (curBlock.blockType == blocks[i].blockType)
                        {
                            gm.delBlock.Add(blocks[i]);
                        }
                    }

                    if (gm.delBlock.Count == 3)
                    {
                        curBlock.blockType = BlockType.Boom;
                        curBlock.boomType = BoomType.RowBoom;

                        // 마지막 자리에 폭탄을 저장
                        gm.delBlock.Add(curBlock);
                        return;
                    }
                }
            }

            #endregion
        }
    }
}