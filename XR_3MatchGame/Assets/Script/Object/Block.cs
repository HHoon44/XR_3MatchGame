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
                        StartCoroutine(ReturnCheck());
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
                        StartCoroutine(ReturnCheck());
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
                        StartCoroutine(ReturnCheck());
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
                        StartCoroutine(ReturnCheck());
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 스와이프 한 블럭을 원 위치 할건지에 대해 조사하는 메서드
        /// </summary>
        /// <returns></returns>
        private IEnumerator ReturnCheck()
        {
            gm.LRTBCheck();

            var blocks = gm.blocks;

            // 같은 블럭 개수
            var tb_Count = 0;
            var bb_Count = 0;
            var mb_Count = 0;
            var lb_Count = 0;
            var rb_Count = 0;

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
                                tb_Count++;
                            }
                        }

                        // Middle
                        if ((col - 1 == blocks[i].col || col + 1 == blocks[i].col) && row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                mb_Count++;
                            }
                        }

                        // Left
                        if ((col - 1 == blocks[i].col || col - 2 == blocks[i].col) && row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                lb_Count++;
                            }
                        }

                        // Right
                        if ((col + 1 == blocks[i].col || col + 2 == blocks[i].col) && row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                rb_Count++;
                            }
                        }
                    }

                    if (tb_Count < 2 && mb_Count < 2 && lb_Count < 2 && rb_Count < 2)
                    {
                        yield return new WaitForSeconds(.2f);

                        if (OtherBlockCheck(tb_Count, bb_Count, mb_Count, lb_Count, rb_Count, blocks))
                        {
                            // OtherBlock에서도 매칭이 일어나는지 체크하고
                            // 없다면 원 위치
                            otherBlock.row += 1;
                            row -= 1;
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

                case SwipeDir.Bottom:
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        // Bottom
                        if ((row - 1 == blocks[i].row || row - 2 == blocks[i].row) && col == blocks[i].col)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                bb_Count++;
                            }
                        }

                        // Middle
                        if ((col - 1 == blocks[i].col || col + 1 == blocks[i].col) && row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                mb_Count++;
                            }
                        }

                        // Left
                        if ((col - 1 == blocks[i].col || col - 2 == blocks[i].col) && row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                lb_Count++;
                            }
                        }

                        //Right
                        if ((col + 1 == blocks[i].col || col + 2 == blocks[i].col) && row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                rb_Count++;
                            }
                        }
                    }

                    if (bb_Count < 2 && mb_Count < 2 && lb_Count < 2 && rb_Count < 2)
                    {
                        yield return new WaitForSeconds(.2f);

                        // OtherBlock도 매칭되는 블럭이 있는지 확인
                        if (OtherBlockCheck(tb_Count, bb_Count, mb_Count, lb_Count, rb_Count, blocks))
                        {
                            otherBlock.row -= 1;
                            row += 1;
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
                                tb_Count++;
                            }
                        }

                        // Bottom
                        if ((row - 1 == blocks[i].row || row - 2 == blocks[i].row) && col == blocks[i].col)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                bb_Count++;
                            }
                        }

                        // Middle
                        if ((row - 1 == blocks[i].row || row + 1 == blocks[i].row) && col == blocks[i].col)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                mb_Count++;
                            }
                        }

                        // Left
                        if ((col - 1 == blocks[i].col || col - 2 == blocks[i].col) && row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                lb_Count++;
                            }
                        }
                    }

                    if (tb_Count < 2 && bb_Count < 2 && mb_Count < 2 && lb_Count < 2)
                    {
                        yield return new WaitForSeconds(.2f);

                        // OtherBlock도 매칭되는 블럭이 있는지 확인
                        if (OtherBlockCheck(tb_Count, bb_Count, mb_Count, lb_Count, rb_Count, blocks))
                        {
                            otherBlock.col -= 1;
                            col += 1;
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
                                tb_Count++;
                            }
                        }

                        // Bottom
                        if ((row - 1 == blocks[i].row || row - 2 == blocks[i].row) && col == blocks[i].col)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                bb_Count++;
                            }
                        }

                        // Middle
                        if ((row - 1 == blocks[i].row || row + 1 == blocks[i].row) && col == blocks[i].col)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                mb_Count++;
                            }
                        }

                        // Right
                        if ((col + 1 == blocks[i].col || col + 2 == blocks[i].col) && row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                rb_Count++;
                            }
                        }
                    }

                    if (tb_Count < 2 && bb_Count < 2 && mb_Count < 2 && rb_Count < 2)
                    {
                        yield return new WaitForSeconds(.2f);

                        // OtherBlock도 매칭되는 블럭이 있는지 확인
                        if (OtherBlockCheck(tb_Count, bb_Count, mb_Count, lb_Count, rb_Count, blocks))
                        {
                            otherBlock.col += 1;
                            col -= 1;
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
            }

            // false 부분 생각좀 해야할듯

            gm.LRTBCheck();
        }

        /// <summary>
        /// OtherBlock 매칭을 탐색해주는 메서드
        /// </summary>
        /// <param name="t_Count">Top 개수</param>
        /// <param name="b_Count">Bottom 개수</param>
        /// <param name="m_Count">Middle 개수</param>
        /// <param name="l_Count">Left 개수</param>
        /// <param name="r_Count">Right 개수</param>
        /// <returns></returns>
        private bool OtherBlockCheck(int t_Count, int b_Count, int m_Count, int l_Count, int r_Count, List<Block> blocks)
        {
            t_Count = 0;
            b_Count = 0;
            m_Count = 0;
            l_Count = 0;
            r_Count = 0;
            var mb_Count2 = 0;

            // OtherBlock에서도 라인 체크가 일어 날 수 있으므로 체킹
            for (int i = 0; i < blocks.Count; i++)
            {
                // Top
                if ((otherBlock.row + 1 == blocks[i].row || otherBlock.row + 2 == blocks[i].row) && otherBlock.col == blocks[i].col)
                {
                    if (otherBlock.blockType == blocks[i].blockType)
                    {
                        t_Count++;
                    }
                }

                // Horizontal Middle
                if ((otherBlock.col + 1 == blocks[i].col || otherBlock.col - 1 == blocks[i].col) && otherBlock.row == blocks[i].row)
                {
                    if (otherBlock.blockType == blocks[i].blockType)
                    {
                        m_Count++;
                    }
                }

                // Vertical Middle
                if ((otherBlock.row + 1 == blocks[i].row || otherBlock.row - 1 == blocks[i].row) && otherBlock.col == blocks[i].col)
                {
                    if (otherBlock.blockType == blocks[i].blockType)
                    {
                        mb_Count2++;
                    }
                }

                // Bottom
                if ((otherBlock.row - 1 == blocks[i].row || otherBlock.row - 2 == blocks[i].row) && otherBlock.col == blocks[i].col)
                {
                    if (otherBlock.blockType == blocks[i].blockType)
                    {
                        b_Count++;
                    }
                }

                // Left
                if ((otherBlock.col - 1 == blocks[i].col || otherBlock.col - 2 == blocks[i].col) && otherBlock.row == blocks[i].row)
                {
                    if (otherBlock.blockType == blocks[i].blockType)
                    {
                        l_Count++;
                    }
                }

                // Right
                if ((otherBlock.col + 1 == blocks[i].col || otherBlock.col + 2 == blocks[i].col) && otherBlock.row == blocks[i].row)
                {
                    if (otherBlock.blockType == blocks[i].blockType)
                    {
                        r_Count++;
                    }
                }
            }

            if (t_Count < 2 && m_Count < 2 && mb_Count2 < 2 && b_Count < 2 && l_Count < 2 && r_Count < 2)
            {
                // OtherBlock에서 매칭 발생 안함
                return true;
            }
            else
            {
                // OtherBlock에서 매칭 발생
                return false;
            }
        }

        /// <summary>
        /// 블럭의 폭탄 여부를 체크하는 메서드
        /// </summary>
        /// <param name="blocks">블럭 모음</param>
        /// <param name="curBlock">여부를 체크할 블럭</param>
        private void BoomCheck(List<Block> blocks, Block curBlock)
        {
            #region Col 체크

            if (curBlock.blockType != BlockType.Boom)
            {
                gm.checkBlocks.Clear();

                for (int i = 0; i < blocks.Count; i++)
                {
                    // -1 -2 -3 탐색
                    if ((curBlock.col - 1 == blocks[i].col || curBlock.col - 2 == blocks[i].col || curBlock.col - 3 == blocks[i].col) && curBlock.row == blocks[i].row)
                    {
                        if (curBlock.blockType == blocks[i].blockType)
                        {
                            gm.checkBlocks.Add(blocks[i]);
                        }
                    }

                    if (gm.checkBlocks.Count == 3)
                    {
                        curBlock.blockType = BlockType.Boom;
                        curBlock.boomType = BoomType.ColBoom;

                        // 마지막 자리에 폭탄을 저장
                        gm.checkBlocks.Add(curBlock);
                        return;
                    }
                }
            }

            if (curBlock.blockType != BlockType.Boom)
            {
                gm.checkBlocks.Clear();

                for (int i = 0; i < blocks.Count; i++)
                {
                    // -1 -2 +1 탐색
                    if ((curBlock.col - 1 == blocks[i].col || curBlock.col - 2 == blocks[i].col || curBlock.col + 1 == blocks[i].col) && curBlock.row == blocks[i].row)
                    {
                        if (curBlock.blockType == blocks[i].blockType)
                        {
                            gm.checkBlocks.Add(blocks[i]);
                        }
                    }

                    if (gm.checkBlocks.Count == 3)
                    {

                        curBlock.blockType = BlockType.Boom;
                        curBlock.boomType = BoomType.ColBoom;

                        // 마지막 자리에 폭탄을 저장
                        gm.checkBlocks.Add(curBlock);
                        return;
                    }
                }
            }

            if (curBlock.blockType != BlockType.Boom)
            {
                gm.checkBlocks.Clear();

                for (int i = 0; i < blocks.Count; i++)
                {
                    // -1 +1 +2
                    if ((curBlock.col - 1 == blocks[i].col || curBlock.col + 1 == blocks[i].col || curBlock.col + 2 == blocks[i].col) && curBlock.row == blocks[i].row)
                    {
                        if (curBlock.blockType == blocks[i].blockType)
                        {
                            gm.checkBlocks.Add(blocks[i]);
                        }
                    }

                    if (gm.checkBlocks.Count == 3)
                    {
                        curBlock.blockType = BlockType.Boom;
                        curBlock.boomType = BoomType.ColBoom;

                        // 마지막 자리에 폭탄을 저장
                        gm.checkBlocks.Add(curBlock);
                        return;
                    }
                }
            }

            if (curBlock.blockType != BlockType.Boom)
            {
                gm.checkBlocks.Clear();

                for (int i = 0; i < blocks.Count; i++)
                {
                    // +1 +2 +3
                    if ((curBlock.col + 1 == blocks[i].col || curBlock.col + 2 == blocks[i].col || curBlock.col + 3 == blocks[i].col) && curBlock.row == blocks[i].row)
                    {
                        if (curBlock.blockType == blocks[i].blockType)
                        {
                            gm.checkBlocks.Add(blocks[i]);
                        }
                    }

                    if (gm.checkBlocks.Count == 3)
                    {
                        curBlock.blockType = BlockType.Boom;
                        curBlock.boomType = BoomType.ColBoom;

                        // 마지막 자리에 폭탄을 저장
                        gm.checkBlocks.Add(curBlock);
                        return;
                    }
                }
            }

            #endregion

            #region Row 체크

            if (curBlock.blockType != BlockType.Boom)
            {
                gm.checkBlocks.Clear();

                for (int i = 0; i < blocks.Count; i++)
                {
                    // -1 -2 -3
                    if ((curBlock.row - 1 == blocks[i].row || curBlock.row - 2 == blocks[i].row || curBlock.row - 3 == blocks[i].row) && curBlock.col == blocks[i].col)
                    {
                        if (curBlock.blockType == blocks[i].blockType)
                        {
                            gm.checkBlocks.Add(blocks[i]);
                        }
                    }

                    if (gm.checkBlocks.Count == 3)
                    {
                        curBlock.blockType = BlockType.Boom;
                        curBlock.boomType = BoomType.RowBoom;

                        // 마지막 자리에 폭탄을 저장
                        gm.checkBlocks.Add(curBlock);
                        return;
                    }
                }
            }

            if (curBlock.blockType != BlockType.Boom)
            {
                gm.checkBlocks.Clear();

                for (int i = 0; i < blocks.Count; i++)
                {
                    // -1 -2 +1
                    if ((curBlock.row - 1 == blocks[i].row || curBlock.row - 2 == blocks[i].row || curBlock.row + 1 == blocks[i].row) && curBlock.col == blocks[i].col)
                    {
                        if (curBlock.blockType == blocks[i].blockType)
                        {
                            gm.checkBlocks.Add(blocks[i]);
                        }
                    }

                    if (gm.checkBlocks.Count == 3)
                    {
                        curBlock.blockType = BlockType.Boom;
                        curBlock.boomType = BoomType.RowBoom;

                        // 마지막 자리에 폭탄을 저장
                        gm.checkBlocks.Add(curBlock);
                        return;
                    }
                }
            }

            if (curBlock.blockType != BlockType.Boom)
            {
                gm.checkBlocks.Clear();

                for (int i = 0; i < blocks.Count; i++)
                {
                    //-1 +1 +2
                    if ((curBlock.row - 1 == blocks[i].row || curBlock.row + 1 == blocks[i].row || curBlock.row + 2 == blocks[i].row) && curBlock.col == blocks[i].col)
                    {
                        if (curBlock.blockType == blocks[i].blockType)
                        {
                            gm.checkBlocks.Add(blocks[i]);
                        }
                    }

                    if (gm.checkBlocks.Count == 3)
                    {
                        curBlock.blockType = BlockType.Boom;
                        curBlock.boomType = BoomType.RowBoom;

                        // 마지막 자리에 폭탄을 저장
                        gm.checkBlocks.Add(curBlock);
                        return;
                    }
                }
            }

            if (curBlock.blockType != BlockType.Boom)
            {
                gm.checkBlocks.Clear();

                for (int i = 0; i < blocks.Count; i++)
                {
                    // +1 +2 +3
                    if ((curBlock.row + 1 == blocks[i].row || curBlock.row + 2 == blocks[i].row || curBlock.row + 3 == blocks[i].row) && curBlock.col == blocks[i].col)
                    {
                        if (curBlock.blockType == blocks[i].blockType)
                        {
                            gm.checkBlocks.Add(blocks[i]);
                        }
                    }

                    if (gm.checkBlocks.Count == 3)
                    {
                        curBlock.blockType = BlockType.Boom;
                        curBlock.boomType = BoomType.RowBoom;

                        // 마지막 자리에 폭탄을 저장
                        gm.checkBlocks.Add(curBlock);
                        return;
                    }
                }
            }

            #endregion
        }
    }
}