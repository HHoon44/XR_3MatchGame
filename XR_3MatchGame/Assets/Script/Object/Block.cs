using System.Collections;
using System.Collections.Generic;
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

        public int col;
        public int row;

        public int targetCol;     // ��� ���� X ��
        public int targetRow;     // ��� ���� Y ��

        public BlockType blockType = BlockType.None;

        private float swipeAngle = 0;
        private Vector2 firstTouchPosition;
        private Vector2 finalTouchPosition;
        private Vector2 tempPosition;

        private SpriteRenderer spriteRenderer;

        private Block otherBlock;

        public Block topBlock;      // ���� ���� ���� �����ϴ� ��
        public Block bottomBlock;   // ���� ���� �Ʒ��� �����ϴ� ��
        public Block leftBlock;     // ���� ���� ���ʿ� �����ϴ� ��
        public Block rightBlock;    // ���� ���� �����ʿ� �����ϴ� ��

        // Test
        public BlockType topType = BlockType.None;
        public BlockType bottomType = BlockType.None;
        public BlockType leftType = BlockType.None;
        public BlockType rightType = BlockType.None;

        private SwipeDir swipeDir = SwipeDir.None;

        private GameManager gm;

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

            // �������� ���� ��������Ʈ�� �����մϴ�
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

            // �� ���������� üũ �մϴ�
            BlockSwipe();
        }

        private void OnMouseDown()
        {
            // ���콺 Ŭ���� ���� ���� �� ��ġ�� �����Ѵ�
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        private void OnMouseUp()
        {
            // ���콺 Ŭ���� ������ �� ��ġ�� �����Ѵ�
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }

        /// <summary>
        /// ���콺 �巡�� ������ ����ϴ� �޼���
        /// </summary>
        private void CalculateAngle()
        {
            if (gm.isChecking == true)
            {
                // üũ�� �� �̵��� �����մϴ�
                return;
            }

            // ���콺 �巡�� ������ ����Ѵ�
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
                transform.position = Vector2.Lerp(transform.position, tempPosition, .05f);
            }
            else
            {
                tempPosition = new Vector2(targetCol, transform.position.y);
                transform.position = tempPosition;
            }

            if (Mathf.Abs(targetRow - transform.position.y) > .1f)
            {
                tempPosition = new Vector2(transform.position.x, targetRow);
                transform.position = Vector2.Lerp(transform.position, tempPosition, .05f);
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
                        StartCoroutine(ReturnBlock());
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
                        // �Ʒ��� �̵��̹Ƿ� ��ǥ ���� +1 �̵�
                        // �Ʒ��� �̵��̹Ƿ� �̵� ���� -1 �̵�
                        otherBlock = gm.blocks[i];
                        otherBlock.row += 1;
                        row -= 1;

                        swipeDir = SwipeDir.Bottom;
                        StartCoroutine(ReturnBlock());
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
                        // ���� �̵��̹Ƿ� ��ǥ ���� +1 �̵�
                        // ���� �̵��̹Ƿ� �̵� ���� -1 �̵�
                        otherBlock = gm.blocks[i];
                        otherBlock.col += 1;
                        col -= 1;

                        swipeDir = SwipeDir.Left;
                        StartCoroutine(ReturnBlock());
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
                        // ������ �̵��̹Ƿ� ��ǥ ���� -1 �̵�
                        // ������ �̵��̹Ƿ� �̵� ���� +1 �̵�
                        otherBlock = gm.blocks[i];
                        otherBlock.col -= 1;
                        col += 1;

                        swipeDir = SwipeDir.Right;
                        StartCoroutine(ReturnBlock());
                        return;
                    }
                }
            }
        }

        private IEnumerator ReturnBlock()
        {
            gm.isChecking = true;
            gm.LRTBCheck();

            // ���� �Ŵ����� ��� �ִ� �� ����Ʈ�� �����ɴϴ�
            var blocks = gm.blocks;

            // ���� �� ������ ���� ī��Ʈ �Դϴ�
            var topCount = 0;
            var bottomCount = 0;
            var middleCount = 0;
            var LeftCount = 0;
            var RightCount = 0;

            switch (swipeDir)
            {
                case SwipeDir.Top:
                    // Top
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        if ((row + 1 == blocks[i].row || row + 2 == blocks[i].row) &&
                            col == blocks[i].col)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                topCount++;
                            }
                        }
                    }

                    // Middle
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        if (col -1 == blocks[i].col || col + 1 == blocks[i].col &&
                            row == blocks[i].row)
                        {
                            middleCount++;
                        }
                    }

                    // Bottom
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        if ((row - 1 == blocks[i].row || row - 2 == blocks[i].row) &&
                            col == blocks[i].col)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                bottomCount++;
                            }
                        }
                    }

                    // Left
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        if ((col - 1 == blocks[i].col || col - 2 == blocks[i].col) &&
                            row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                LeftCount++;
                            }
                        }
                    }

                    // Right
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        if ((col + 1 == blocks[i].col || col + 2 == blocks[i].col) &&
                            row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                RightCount++;
                            }
                        }
                    }

                    if (topCount < 2 && bottomCount < 2 && LeftCount < 2 && RightCount < 2 && middleCount < 2)
                    {
                        yield return new WaitForSeconds(.2f);
                        otherBlock.row += 1;
                        row -= 1;
                    }
                    else
                    {
                        gm.isStart = true;
                    }
                    break;

                case SwipeDir.Bottom:
                    // Top
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        if ((row + 1 == blocks[i].row || row + 2 == blocks[i].row) &&
                            col == blocks[i].col)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                topCount++;
                            }
                        }
                    }

                    // Bottom
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        if ((row - 1 == blocks[i].row || row - 2 == blocks[i].row) &&
                            col == blocks[i].col)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                bottomCount++;
                            }
                        }
                    }

                    // Middle
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        if (col - 1 == blocks[i].col || col + 1 == blocks[i].col &&
                            row == blocks[i].row)
                        {
                            middleCount++;
                        }
                    }

                    // Left
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        if ((col - 1 == blocks[i].col || col - 2 == blocks[i].col) &&
                            row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                LeftCount++;
                            }
                        }
                    }

                    // Right
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        if ((col + 1 == blocks[i].col || col + 2 == blocks[i].col) &&
                            row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                RightCount++;
                            }
                        }
                    }

                    if (topCount < 2 && bottomCount < 2 && LeftCount < 2 && RightCount < 2 && middleCount < 2)
                    {
                        yield return new WaitForSeconds(.2f);
                        otherBlock.row -= 1;
                        row += 1;
                    }
                    else
                    {
                        gm.isStart = true;
                    }
                    break;

                case SwipeDir.Left:
                    // Top
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        if ((row + 1 == blocks[i].row || row + 2 == blocks[i].row) &&
                            col == blocks[i].col)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                topCount++;
                            }
                        }
                    }

                    // Bottom
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        if ((row - 1 == blocks[i].row || row - 2 == blocks[i].row) &&
                            col == blocks[i].col)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                bottomCount++;
                            }
                        }
                    }

                    // Middle
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        if (row - 1 == blocks[i].row || row + 1 == blocks[i].row &&
                            col == blocks[i].col)
                        {
                            middleCount++;
                        }
                    }

                    // Left
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        if ((col - 1 == blocks[i].col || col - 2 == blocks[i].col) &&
                            row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                LeftCount++;
                            }
                        }
                    }

                    // Right
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        if ((col + 1 == blocks[i].col || col + 2 == blocks[i].col) &&
                            row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                RightCount++;
                            }
                        }
                    }

                    if (topCount < 2 && bottomCount < 2 && LeftCount < 2 && RightCount < 2 && middleCount < 2)
                    {
                        yield return new WaitForSeconds(.2f);
                        otherBlock.col -= 1;
                        col += 1;
                    }
                    else
                    {
                        gm.isStart = true;
                    }
                    break;

                case SwipeDir.Right:
                    // Top
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        if ((row + 1 == blocks[i].row || row + 2 == blocks[i].row) &&
                            col == blocks[i].col)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                topCount++;
                            }
                        }
                    }

                    // Bottom
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        if ((row - 1 == blocks[i].row || row - 2 == blocks[i].row) &&
                            col == blocks[i].col)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                bottomCount++;
                            }
                        }
                    }

                    // Middle
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        if (row - 1 == blocks[i].row || row + 1 == blocks[i].row &&
                            col == blocks[i].col)
                        {
                            middleCount++;
                        }
                    }

                    // Left
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        if ((col - 1 == blocks[i].col || col - 2 == blocks[i].col) &&
                            row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                LeftCount++;
                            }
                        }
                    }

                    // Right
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        if ((col + 1 == blocks[i].col || col + 2 == blocks[i].col) &&
                            row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                RightCount++;
                            }
                        }
                    }

                    if (topCount < 2 && bottomCount < 2 && LeftCount < 2 && RightCount < 2 && middleCount < 2)
                    {
                        yield return new WaitForSeconds(.2f);
                        otherBlock.col += 1;
                        col -= 1;
                    }
                    else
                    {
                        gm.isStart = true;
                    }
                    break;
            }

            gm.LRTBCheck();
            gm.isChecking = false;

            yield return new WaitForSeconds(.5f);
        }
    }
}