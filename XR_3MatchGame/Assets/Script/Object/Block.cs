using System.Collections;
using System.Net.NetworkInformation;
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

        public int targetCol;     // 상대 블럭의 X 값
        public int targetRow;     // 상대 블럭의 Y 값

        public BlockType blockType = BlockType.None;

        private float swipeAngle = 0;
        private Vector2 firstTouchPosition;
        private Vector2 finalTouchPosition;
        private Vector2 tempPosition;

        private bool isRight;
        private bool isLeft;

        private SpriteRenderer spriteRenderer;

        private Block otherBlock;

        private Block leftBlock;    // 현재 블럭의 왼쪽에 존재하는 블럭
        private Block rightBlock;   // 현재 블럭의 오른쪽에 존재하는 블럭
        private Block topBlock;     // 현재 블럭의 위에 존재하는 블럭
        private Block bottomBlock;  // 현재 블럭의 아래에 존재하는 블럭

        private GameManager gm;

        /// <summary>
        /// 블럭 초기 세팅 메서드
        /// </summary>
        /// <param name="col">X 값</param>
        /// <param name="row">Y 값</param>
        public void Initialize(int col, int row)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            gm = GameManager.Instance;

            var blockNum = Random.Range(1, 7);

            // 랜덤으로 블럭의 스프라이트를 설정합니다
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

            // 블럭 스와이프를 체크 합니다
            BlockSwipe();
        }

        private void OnMouseDown()
        {
            // 마우스 클릭을 시작 했을 때 위치를 저장한다
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        private void OnMouseUp()
        {
            // 마우스 클릭을 끝냈을 때 위치를 저장한다
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }

        /// <summary>
        /// 마우스 드래그 각도를 계산하는 메서드
        /// </summary>
        private void CalculateAngle()
        {
            // 마우스 드래그 각도를 계산한다
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y,
                finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;

            MoveBlock();
        }

        /// <summary>
        /// 블럭을 스와이프를 체크하는 메서드
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
        /// 계산한 각도를 이용해서 블럭을 이동시키는 메서드
        /// </summary>
        private void MoveBlock()
        {
            if ((swipeAngle > -45 && swipeAngle <= 45) && col < gm.Bounds.xMax)
            {
                // 오른쪽으로 스와이프
                for (int i = 0; i < gm.blocks.Count; i++)
                {
                    if (gm.blocks[i].col == col + 1 &&
                        gm.blocks[i].row == row)
                    {
                        // 목표 블럭을 찾아 col, row 값을 수정
                        // 오른쪽 이동이므로 목표 블럭은 col -1 이동
                        // 오른쪽 이동이므로 이동 블럭은 col +1 이동
                        otherBlock = gm.blocks[i];
                        otherBlock.col -= 1;
                        col += 1;

                        FindBlock();
                        return;
                    }
                }
            }
            else if ((swipeAngle > 135 || swipeAngle <= -135) && col > gm.Bounds.xMin)
            {
                // 왼쪽으로 스와이프
                for (int i = 0; i < gm.blocks.Count; i++)
                {
                    if (gm.blocks[i].col == col - 1 &&
                        gm.blocks[i].row == row)
                    {
                        // 목표 블럭을 찾아 col, row 값을 수정
                        // 왼쪽 이동이므로 목표 블럭은 col +1 이동
                        // 왼쪽 이동이므로 이동 블럭은 col -1 이동
                        otherBlock = gm.blocks[i];
                        otherBlock.col += 1;
                        col -= 1;

                        FindBlock();
                        return;
                    }
                }
            }
            else if ((swipeAngle > 45 && swipeAngle <= 135) && row < gm.Bounds.yMax)
            {
                // 위쪽으로 스와이프
                for (int i = 0; i < gm.blocks.Count; i++)
                {
                    if (gm.blocks[i].col == col &&
                        gm.blocks[i].row == row + 1)
                    {
                        // 목표 블럭을 찾아 col, row 값을 수정
                        // 위쪽 이동이므로 목표 블럭은 row -1 이동
                        // 위쪽 이동이므로 이동 블럭은 row +1 이동
                        otherBlock = gm.blocks[i];
                        otherBlock.row -= 1;
                        row += 1;

                        FindBlock();
                        return;
                    }
                }
            }
            else if ((swipeAngle < -45 && swipeAngle >= -135) && row > gm.Bounds.yMin)
            {
                // 아래쪽으로 스와이프
                for (int i = 0; i < gm.blocks.Count; i++)
                {
                    if (gm.blocks[i].col == col &&
                        gm.blocks[i].row == row - 1)
                    {
                        // 목표 블럭을 찾아 col, row 값을 수정
                        // 아래쪽 이동이므로 목표 블럭은 row +1 이동
                        // 아래쪽 이동이므로 이동 블럭은 row 
                        otherBlock = gm.blocks[i];
                        otherBlock.row += 1;
                        row -= 1;

                        FindBlock();
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 자신과 같은 블럭이 있는지 확인하는 메서드
        /// </summary>
        public void FindBlock()
        {
            Debug.Log("FindBlock");

            StartCoroutine(Test());

            IEnumerator Test()
            {
                Debug.Log("Test");

                // 좌, 우 같은 블럭이 있는지 확인합니다
                for (int i = 0; i < gm.blocks.Count; i++)
                {
                    if (col + 1 == gm.blocks[i].col && row == gm.blocks[i].row)
                    {
                        // 왼쪽 블럭을 찾습니다
                        leftBlock = gm.blocks[i];
                    }

                    if (col - 1 == gm.blocks[i].col && row == gm.blocks[i].row)
                    {
                        // 오른쪽 블럭을 찾습니다
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


                // 양쪽 다 현재 블럭과 같은 블럭이라면 비활성화
                if (isLeft && isRight)
                {
                    yield return new WaitForSeconds(2f);
                    leftBlock.gameObject.SetActive(false);
                    rightBlock.gameObject.SetActive(false);
                    this.gameObject.SetActive(false);
                }
            }
        }
    }
}