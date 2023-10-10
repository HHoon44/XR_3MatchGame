using System.Collections;
using UnityEditor.TextCore.Text;
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

        public int nextCol;     // ��� ���� X ��
        public int nextRow;     // ��� ���� Y ��

        public BlockType blockType = BlockType.None;

        private float swipeAngle = 0;
        private Vector2 firstTouchPosition;
        private Vector2 finalTouchPosition;
        private Vector2 tempPosition;

        private bool isRight;
        private bool isLeft;

        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private Block otherBlock;

        [SerializeField]
        private Block leftBlock;

        [SerializeField]
        private Block rightBlock;

        private GameManager gm;

        /// <summary>
        /// �� �ʱ� ���� �޼���
        /// </summary>
        /// <param name="col">X ��</param>
        /// <param name="row">Y ��</param>
        public void Initialize(int col, int row)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            var blockNum = Random.Range(1, 7);

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

            gm = GameManager.Instance;

            nextCol = col;
            nextRow = row;
        }

        private void Update()
        {
            nextCol = col;
            nextRow = row;

            #region �� ��������
            if (Mathf.Abs(nextCol - transform.position.x) > .1f)
            {
                tempPosition = new Vector2(nextCol, transform.position.y);
                transform.position = Vector2.Lerp(transform.position, tempPosition, .05f);
            }
            else
            {
                tempPosition = new Vector2(nextCol, transform.position.y);
                transform.position = tempPosition;
            }

            if (Mathf.Abs(nextRow - transform.position.y) > .1f)
            {
                tempPosition = new Vector2(transform.position.x, nextRow);
                transform.position = Vector2.Lerp(transform.position, tempPosition, .05f);
            }
            else
            {
                tempPosition = new Vector2(transform.position.x, nextRow);
                transform.position = tempPosition;
            }
            #endregion

            // ���� ���� ã���ϴ�
            //FindBlock();
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
            // ���콺 �巡�� ������ ����Ѵ�
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y,
                finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;

            MoveBlock();
        }

        /// <summary>
        /// ����� ������ �̿��ؼ� ���� �̵���Ű�� �޼���
        /// </summary>
        private void MoveBlock()
        {
            if ((swipeAngle > -45 && swipeAngle <= 45) && col < gm.Bounds.xMax)
            {
                // ���������� ��������
                for (int i = 0; i < gm.blocks.Count; i++)
                {
                    if (gm.blocks[i].col == col + 1 &&
                        gm.blocks[i].row == row)
                    {
                        // ��ǥ ���� ã�� col, row ���� ����
                        // ������ �̵��̹Ƿ� ��ǥ ���� col -1 �̵�
                        // ������ �̵��̹Ƿ� �̵� ���� col +1 �̵�
                        otherBlock = gm.blocks[i];
                        otherBlock.col -= 1;
                        col += 1;

                        gm.isMove = true;
                        FindBlock();
                        return;
                    }
                }
            }
            else if ((swipeAngle > 135 || swipeAngle <= -135) && col > gm.Bounds.xMin)
            {
                // �������� ��������
                for (int i = 0; i < gm.blocks.Count; i++)
                {
                    if (gm.blocks[i].col == col - 1 &&
                        gm.blocks[i].row == row)
                    {
                        // ��ǥ ���� ã�� col, row ���� ����
                        // ���� �̵��̹Ƿ� ��ǥ ���� col +1 �̵�
                        // ���� �̵��̹Ƿ� �̵� ���� col -1 �̵�
                        otherBlock = gm.blocks[i];
                        otherBlock.col += 1;
                        col -= 1;

                        gm.isMove = true;
                        FindBlock();
                        return;
                    }
                }
            }
            else if ((swipeAngle > 45 && swipeAngle <= 135) && row < gm.Bounds.yMax)
            {
                // �������� ��������
                for (int i = 0; i < gm.blocks.Count; i++)
                {
                    if (gm.blocks[i].col == col &&
                        gm.blocks[i].row == row + 1)
                    {
                        // ��ǥ ���� ã�� col, row ���� ����
                        // ���� �̵��̹Ƿ� ��ǥ ���� row -1 �̵�
                        // ���� �̵��̹Ƿ� �̵� ���� row +1 �̵�
                        otherBlock = gm.blocks[i];
                        otherBlock.row -= 1;
                        row += 1;

                        gm.isMove = true;
                        FindBlock();
                        return;
                    }
                }
            }
            else if ((swipeAngle < -45 && swipeAngle >= -135) && row > gm.Bounds.yMin)
            {
                // �Ʒ������� ��������
                for (int i = 0; i < gm.blocks.Count; i++)
                {
                    if (gm.blocks[i].col == col &&
                        gm.blocks[i].row == row - 1)
                    {
                        // ��ǥ ���� ã�� col, row ���� ����
                        // �Ʒ��� �̵��̹Ƿ� ��ǥ ���� row +1 �̵�
                        // �Ʒ��� �̵��̹Ƿ� �̵� ���� row 
                        otherBlock = gm.blocks[i];
                        otherBlock.row += 1;
                        row -= 1;

                        gm.isMove = true;
                        FindBlock();
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// �ڽŰ� ���� ���� �ִ��� Ȯ���ϴ� �޼���
        /// </summary>
        private void FindBlock()
        {
            Debug.Log("FindBlock");

            StartCoroutine(Test());

            IEnumerator Test()
            {
                Debug.Log("Test");

                // ��, �� ���� ���� �ִ��� Ȯ���մϴ�
                for (int i = 0; i < gm.blocks.Count; i++)
                {
                    if (col + 1 == gm.blocks[i].col && row == gm.blocks[i].row)
                    {
                        // ���� ���� ã���ϴ�
                        leftBlock = gm.blocks[i];
                    }

                    if (col - 1 == gm.blocks[i].col && row == gm.blocks[i].row)
                    {
                        // ������ ���� ã���ϴ�
                        rightBlock = gm.blocks[i];
                    }
                }

                if (leftBlock != null || rightBlock != null)
                {
                    if (leftBlock.blockType == this.blockType)
                    {
                        isLeft = true;
                    }

                    if (rightBlock.blockType == this.blockType)
                    {
                        isRight = true;
                    }
                }

                yield return new WaitForSeconds(2f);

                // ���� �� ���� ���� ���� ���̶��
                // ��Ȱ��ȭ
                if (isLeft && isRight)
                {
                    leftBlock.gameObject.SetActive(false);
                    rightBlock.gameObject.SetActive(false);
                    this.gameObject.SetActive(false);
                }
            }
        }
    }
}