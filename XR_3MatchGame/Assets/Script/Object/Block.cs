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

        public int col;     // ���� ���� X ��
        public int row;     // ���� ���� Y ��

        public int targetCol;       // ��� ���� X ��
        public int targetRow;       // ��� ���� Y ��

        public Block topBlock;      // ���� �����ϴ� ��
        public Block bottomBlock;   // �Ʒ��� �����ϴ� ��
        public Block leftBlock;     // ���ʿ� �����ϴ� ��
        public Block rightBlock;    // �����ʿ� �����ϴ� ��

        private float swipeAngle = 0;           // �������� ����

        private Vector2 firstTouchPosition;     // ���콺 Ŭ�� ����
        private Vector2 finalTouchPosition;     // ���콺 Ŭ���� �������� ����
        private Vector2 tempPosition;

        private Block otherBlock;               // ���� ���� �ڸ��� �ٲ� ��
        private GameManager gm;

        public BlockType blockType = BlockType.None;        // ���� ���� Ÿ��
        public BoomType boomType = BoomType.None;           // ���࿡ ���� ��ź�̶�� � ��ź������ ���� Ÿ��
        private SwipeDir swipeDir = SwipeDir.None;

        // Test
        public BlockType topType = BlockType.None;
        public BlockType bottomType = BlockType.None;
        public BlockType leftType = BlockType.None;
        public BlockType rightType = BlockType.None;

        /// <summary>
        /// �� �ʱ� ���� �޼���
        /// </summary>
        /// <param name="col">X ��</param>
        /// <param name="row">Y ��</param>
        public void Initialize(int col, int row)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            gm = GameManager.Instance;

            var blockNum = Random.Range(1, gm.BoardSize.x);

            // �������� ���� ��������Ʈ�� ����
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
            // ���콺 Ŭ�� ��ġ ����
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        private void OnMouseUp()
        {
            // ���콺 Ŭ�� ���� ��ġ ����
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }

        /// <summary>
        /// ���콺 �巡�� ������ ����ϴ� �޼���
        /// </summary>
        private void CalculateAngle()
        {
            // üũ�� �϶� �Է� ����
            if (gm.isChecking == true)
            {
                return;
            }

            // ���콺 �巡�� ������ ����մϴ�
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y,
                finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;

            BlockMove();
        }

        /// <summary>
        /// ���� ���������� üũ�ϴ� �޼���
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
        /// ����� ������ �̿��ؼ� ���� �̵���Ű�� �޼���
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
                        // ���� �̵��̹Ƿ� ��ǥ ���� -1 �̵�
                        // ���� �̵��̹Ƿ� �̵� ���� +1 �̵�
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
                        // �Ʒ��� �̵��̹Ƿ� ��ǥ ���� + 1 �̵�
                        // �Ʒ��� �̵��̹Ƿ� �̵� ���� - 1 �̵�
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
                        // ���� �̵��̹Ƿ� ��ǥ ���� + 1 �̵�
                        // ���� �̵��̹Ƿ� �̵� ���� - 1 �̵�
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
                        // ������ �̵��̹Ƿ� ��ǥ ���� - 1 �̵�
                        // ������ �̵��̹Ƿ� �̵� ���� + 1 �̵�
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
        /// �������� �� ���� �� ��ġ �Ұ����� ���� �����ϴ� �޼���
        /// </summary>
        /// <returns></returns>
        private IEnumerator ReturnCheck()
        {
            gm.LRTBCheck();

            var blocks = gm.blocks;

            // ���� �� ����
            var tb_Count = 0;
            var bb_Count = 0;
            var mb_Count = 0;
            var lb_Count = 0;
            var rb_Count = 0;

            // ������ �ű� ���� ���� ����
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
                            // OtherBlock������ ��Ī�� �Ͼ���� üũ�ϰ�
                            // ���ٸ� �� ��ġ
                            otherBlock.row += 1;
                            row -= 1;
                        }
                        else
                        {
                            // OtherBlock�� ��ź�� �� �� �ִ°� üũ
                            BoomCheck(blocks, otherBlock);

                            // OtherBlock�� ��Ī�Ǵ� ���� ����
                            gm.isStart = true;
                        }
                    }
                    else
                    {
                        // ���� ���� ��ź�� �� �� �ִ°� üũ
                        BoomCheck(blocks, this);

                        // ���� ���� ��Ī�Ǵ� ���� ����
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

                        // OtherBlock�� ��Ī�Ǵ� ���� �ִ��� Ȯ��
                        if (OtherBlockCheck(tb_Count, bb_Count, mb_Count, lb_Count, rb_Count, blocks))
                        {
                            otherBlock.row -= 1;
                            row += 1;
                        }
                        else
                        {
                            // OtherBlock�� ��ź�� �� �� �ִ°� üũ
                            BoomCheck(blocks, otherBlock);

                            // OtherBlock�� ��Ī�Ǵ� ���� ����
                            gm.isStart = true;
                        }
                    }
                    else
                    {
                        // ���� ���� ��ź�� �� �� �ִ°� üũ
                        BoomCheck(blocks, this);

                        // ���� ���� ��Ī�Ǵ� ���� ����
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

                        // OtherBlock�� ��Ī�Ǵ� ���� �ִ��� Ȯ��
                        if (OtherBlockCheck(tb_Count, bb_Count, mb_Count, lb_Count, rb_Count, blocks))
                        {
                            otherBlock.col -= 1;
                            col += 1;
                        }
                        else
                        {
                            // OtherBlock�� ��ź�� �� �� �ִ°� üũ
                            BoomCheck(blocks, otherBlock);

                            // OtherBlock�� ��Ī�Ǵ� ���� ����
                            gm.isStart = true;
                        }
                    }
                    else
                    {
                        // ���� ���� ��ź�� �� �� �ִ°� üũ
                        BoomCheck(blocks, this);

                        // ���� ���� ��Ī�Ǵ� ���� ����
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

                        // OtherBlock�� ��Ī�Ǵ� ���� �ִ��� Ȯ��
                        if (OtherBlockCheck(tb_Count, bb_Count, mb_Count, lb_Count, rb_Count, blocks))
                        {
                            otherBlock.col += 1;
                            col -= 1;
                        }
                        else
                        {
                            // OtherBlock�� ��ź�� �� �� �ִ°� üũ
                            BoomCheck(blocks, otherBlock);

                            // OtherBlock�� ��Ī�Ǵ� ���� ����
                            gm.isStart = true;
                        }
                    }
                    else
                    {
                        // ���� ���� ��ź�� �� �� �ִ°� üũ
                        BoomCheck(blocks, this);

                        // ���� ���� ��Ī�Ǵ� ���� ����
                        gm.isStart = true;
                    }
                    break;
            }

            // false �κ� ������ �ؾ��ҵ�

            gm.LRTBCheck();
        }

        /// <summary>
        /// OtherBlock ��Ī�� Ž�����ִ� �޼���
        /// </summary>
        /// <param name="t_Count">Top ����</param>
        /// <param name="b_Count">Bottom ����</param>
        /// <param name="m_Count">Middle ����</param>
        /// <param name="l_Count">Left ����</param>
        /// <param name="r_Count">Right ����</param>
        /// <returns></returns>
        private bool OtherBlockCheck(int t_Count, int b_Count, int m_Count, int l_Count, int r_Count, List<Block> blocks)
        {
            t_Count = 0;
            b_Count = 0;
            m_Count = 0;
            l_Count = 0;
            r_Count = 0;
            var mb_Count2 = 0;

            // OtherBlock������ ���� üũ�� �Ͼ� �� �� �����Ƿ� üŷ
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
                // OtherBlock���� ��Ī �߻� ����
                return true;
            }
            else
            {
                // OtherBlock���� ��Ī �߻�
                return false;
            }
        }

        /// <summary>
        /// ���� ��ź ���θ� üũ�ϴ� �޼���
        /// </summary>
        /// <param name="blocks">�� ����</param>
        /// <param name="curBlock">���θ� üũ�� ��</param>
        private void BoomCheck(List<Block> blocks, Block curBlock)
        {
            #region Col üũ

            if (curBlock.blockType != BlockType.Boom)
            {
                gm.checkBlocks.Clear();

                for (int i = 0; i < blocks.Count; i++)
                {
                    // -1 -2 -3 Ž��
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

                        // ������ �ڸ��� ��ź�� ����
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
                    // -1 -2 +1 Ž��
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

                        // ������ �ڸ��� ��ź�� ����
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

                        // ������ �ڸ��� ��ź�� ����
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

                        // ������ �ڸ��� ��ź�� ����
                        gm.checkBlocks.Add(curBlock);
                        return;
                    }
                }
            }

            #endregion

            #region Row üũ

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

                        // ������ �ڸ��� ��ź�� ����
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

                        // ������ �ڸ��� ��ź�� ����
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

                        // ������ �ڸ��� ��ź�� ����
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

                        // ������ �ڸ��� ��ź�� ����
                        gm.checkBlocks.Add(curBlock);
                        return;
                    }
                }
            }

            #endregion
        }
    }
}