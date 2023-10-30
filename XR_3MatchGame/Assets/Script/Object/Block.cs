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
                        // �Ʒ��� �̵��̹Ƿ� ��ǥ ���� + 1 �̵�
                        // �Ʒ��� �̵��̹Ƿ� �̵� ���� - 1 �̵�
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
                        // ���� �̵��̹Ƿ� ��ǥ ���� + 1 �̵�
                        // ���� �̵��̹Ƿ� �̵� ���� - 1 �̵�
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
                        // ������ �̵��̹Ƿ� ��ǥ ���� - 1 �̵�
                        // ������ �̵��̹Ƿ� �̵� ���� + 1 �̵�
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
        /// ���������� ���� üŷ�ϴ� �޼���
        /// </summary>
        /// <returns></returns>
        private IEnumerator BlockCheck()
        {
            gm.LRTBCheck();

            var blocks = gm.blocks;

            // ���� �� ����
            var count_T = 0;
            var count_B = 0;
            var count_M = 0;
            var count_L = 0;
            var count_R = 0;

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

                    // �̵��� ��ġ�� ��Ī�Ǵ� ���� ���ٸ�
                    if (count_T < 2 && count_M < 2 && count_L < 2 && count_R < 2)
                    {
                        yield return new WaitForSeconds(.2f);

                        // OtherBlock�� ��Ī ���� �Ǵ�
                        if (OtherBlockCheck(count_T, count_B, count_M, count_L, count_R, blocks))
                        {
                            // �� ����ġ
                            otherBlock.row += 1;
                            row -= 1;

                            yield return new WaitForSeconds(.5f);

                            gm.isChecking = false;
                        }
                        else
                        {
                            // OtherBlock ��ź ���� üũ
                            BoomCheck(blocks, otherBlock);

                            // �� ��Ī ����
                            gm.isStart = true;
                        }
                    }
                    else
                    {
                        // �� ��ź ���� üũ
                        BoomCheck(blocks, this);

                        // �� ��Ī ����
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

                        // OtherBlock ��Ī ���� �Ǵ�
                        if (OtherBlockCheck(count_T, count_B, count_M, count_L, count_R, blocks))
                        {
                            otherBlock.row -= 1;
                            row += 1;

                            yield return new WaitForSeconds(.5f);

                            gm.isChecking = false;
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

                        // OtherBlock�� ��Ī�Ǵ� ���� �ִ��� Ȯ��
                        if (OtherBlockCheck(count_T, count_B, count_M, count_L, count_R, blocks))
                        {
                            otherBlock.col -= 1;
                            col += 1;

                            yield return new WaitForSeconds(.5f);

                            gm.isChecking = false;
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

                        // OtherBlock ��Ī ���� �Ǵ�
                        if (OtherBlockCheck(count_T, count_B, count_M, count_L, count_R, blocks))
                        {
                            otherBlock.col += 1;
                            col -= 1;

                            yield return new WaitForSeconds(.5f);

                            gm.isChecking = false;
                        }
                        else
                        {
                            // OtherBlock ��ź ���� üũ
                            BoomCheck(blocks, otherBlock);

                            // OtherBlock ��Ī ����
                            gm.isStart = true;
                        }
                    }
                    else
                    {
                        // ���� �� ��ź ���� üũ
                        BoomCheck(blocks, this);

                        // ���� �� ��Ī ����
                        gm.isStart = true;
                    }
                    break;
            }

            gm.LRTBCheck();
        }

        /// <summary>
        /// OtherBlock ��Ī�� Ž�����ִ� �޼���
        /// </summary>
        /// <param name="count_T">Top ����</param>
        /// <param name="count_B">Bottom ����</param>
        /// <param name="count_M">Middle ����</param>
        /// <param name="count_L">Left ����</param>
        /// <param name="count_R">Right ����</param>
        /// <returns></returns>
        private bool OtherBlockCheck(int count_T, int count_B, int count_M, int count_L, int count_R, List<Block> blocks)
        {
            // �����ϱ� ���� 0���� �ʱ�ȭ
            count_T = 0;
            count_B = 0;
            count_M = 0;
            var count_M2 = 0;
            count_L = 0;
            count_R = 0;

            // OtherBlock ��Ī �� Ž�� �۾�
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
                // Horizontal���� ��Ī�Ǵ� ���� �����Ƿ� ����
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
                // ��Ī �߻� ����
                return true;
            }
            else
            {
                // ��Ī �߻�
                return false;
            }
        }

        /// <summary>
        /// ��ź ���θ� üũ�ϴ� �޼���
        /// </summary>
        /// <param name="blocks">�� ����</param>
        /// <param name="curBlock">���θ� üũ�� ��</param>
        private void BoomCheck(List<Block> blocks, Block curBlock)
        {
            #region Col üũ (3:0, 2:1, 1:2. 0:3)

            if (curBlock.blockType != BlockType.Boom)
            {
                gm.delBlock.Clear();

                for (int i = 0; i < blocks.Count; i++)
                {
                    // -1 -2 -3 Ž��
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

                        // ������ �ڸ��� ��ź ����
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
                    // -1 -2 +1 Ž��
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

                        // ������ �ڸ��� ��ź ����
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

                        // ������ �ڸ��� ��ź�� ����
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

                        // ������ �ڸ��� ��ź�� ����
                        gm.delBlock.Add(curBlock);
                        return;
                    }
                }
            }

            #endregion

            #region Row üũ (3:0, 2:1, 1:2, 0:3)

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

                        // ������ �ڸ��� ��ź�� ����
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

                        // ������ �ڸ��� ��ź�� ����
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

                        // ������ �ڸ��� ��ź�� ����
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

                        // ������ �ڸ��� ��ź�� ����
                        gm.delBlock.Add(curBlock);
                        return;
                    }
                }
            }

            #endregion
        }
    }
}