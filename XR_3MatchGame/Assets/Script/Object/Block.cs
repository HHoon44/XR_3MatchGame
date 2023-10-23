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

        public int targetCol;     // 상대 블럭의 X 값
        public int targetRow;     // 상대 블럭의 Y 값

        public BlockType blockType = BlockType.None;

        private float swipeAngle = 0;
        private Vector2 firstTouchPosition;
        private Vector2 finalTouchPosition;
        private Vector2 tempPosition;

        private SpriteRenderer spriteRenderer;

        private Block otherBlock;

        public Block topBlock;      // 현재 블럭의 위에 존재하는 블럭
        public Block bottomBlock;   // 현재 블러의 아래에 존재하는 블럭
        public Block leftBlock;     // 현재 블럭의 왼쪽에 존재하는 블럭
        public Block rightBlock;    // 현재 블럭의 오른쪽에 존재하는 블럭

        // Test
        public BlockType topType = BlockType.None;
        public BlockType bottomType = BlockType.None;
        public BlockType leftType = BlockType.None;
        public BlockType rightType = BlockType.None;

        private SwipeDir swipeDir = SwipeDir.None;

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

            var blockNum = Random.Range(1, gm.BoardSize.x);

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
            if (gm.isChecking == true)
            {
                // 체크중 블럭 이동을 제어합니다
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
                Debug.Log("X_BlockSwipe");

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
                Debug.Log("Y_BlockSwipe");

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
                        // 위쪽 이동이므로 목표 블럭은 -1 이동
                        // 위쪽 이동이므로 이동 블럭은 +1 이동
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
                        // 아래쪽 이동이므로 목표 블럭은 +1 이동
                        // 아래쪽 이동이므로 이동 블럭은 -1 이동
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
                        // 왼쪽 이동이므로 목표 블럭은 +1 이동
                        // 왼쪽 이동이므로 이동 블럭은 -1 이동
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
                        // 오른쪽 이동이므로 목표 블럭은 -1 이동
                        // 오른쪽 이동이므로 이동 블럭은 +1 이동
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
            /// 이녀석 중요!!
            yield return new WaitForSeconds(.3f);

            gm.isChecking = true;
            gm.LRTBCheck();

            // 게임 매니저가 들고 있는 블럭 리스트를 가져옵니다
            var blocks = gm.blocks;

            // 같은 블럭 개수에 대한 카운트 입니다
            var topCount = 0;
            var bottomCount = 0;
            var middleCount = 0;
            var LeftCount = 0;
            var RightCount = 0;

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
                                topCount++;
                            }
                        }

                        // Middle
                        if ((col - 1 == blocks[i].col || col + 1 == blocks[i].col) && row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                middleCount++;
                            }
                        }

                        // Left
                        if ((col - 1 == blocks[i].col || col - 2 == blocks[i].col) && row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                LeftCount++;
                            }
                        }

                        // Right
                        if ((col + 1 == blocks[i].col || col + 2 == blocks[i].col) && row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                RightCount++;
                            }
                        }
                    }

                    if (topCount < 2 && middleCount < 2 && LeftCount < 2 && RightCount < 2)
                    {
                        // 2개 이상의 블럭이 존재하지 않으므로 이동한 블럭을 원 위치로 되돌립니다
                        yield return new WaitForSeconds(.2f);

                        otherBlock.row += 1;
                        row -= 1;
                    }
                    else
                    {
                        // 2개 이상의 블럭이 존재하므로 블럭 체크를 실행합니다
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
                                bottomCount++;
                            }
                        }

                        // Middle
                        if ((col - 1 == blocks[i].col || col + 1 == blocks[i].col) && row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                middleCount++;
                            }
                        }

                        // Left
                        if ((col - 1 == blocks[i].col || col - 2 == blocks[i].col) && row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                LeftCount++;
                            }
                        }

                        //Right
                        if ((col + 1 == blocks[i].col || col + 2 == blocks[i].col) && row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                RightCount++;
                            }
                        }
                    }

                    if (bottomCount < 2 && middleCount < 2 && LeftCount < 2 && RightCount < 2)
                    {
                        // 2개 이상의 블럭이 존재하지 않으므로 이동한 블럭을 원 위치로 되돌립니다
                        yield return new WaitForSeconds(.2f);

                        otherBlock.row -= 1;
                        row += 1;
                    }
                    else
                    {
                        // 2개 이상의 블럭이 존재하므로 블럭 체크를 실행합니다
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
                                topCount++;
                            }
                        }

                        // Bottom
                        if ((row - 1 == blocks[i].row || row - 2 == blocks[i].row) && col == blocks[i].col)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                bottomCount++;
                            }
                        }

                        // Middle
                        if ((row - 1 == blocks[i].row || row + 1 == blocks[i].row) && col == blocks[i].col)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                middleCount++;
                            }
                        }

                        // Left
                        if ((col - 1 == blocks[i].col || col - 2 == blocks[i].col) && row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                LeftCount++;
                            }
                        }
                    }

                    if (topCount < 2 && bottomCount < 2 && middleCount < 2 && LeftCount < 2)
                    {
                        // 2개 이상의 블럭이 존재하지 않으므로 이동한 블럭을 원 위치로 되돌립니다
                        yield return new WaitForSeconds(.2f);

                        otherBlock.col -= 1;
                        col += 1;
                    }
                    else
                    {
                        // 2개 이상의 블럭이 존재하므로 블럭 체크를 실행합니다
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
                                topCount++;
                            }
                        }

                        // Bottom
                        if ((row - 1 == blocks[i].row || row - 2 == blocks[i].row) && col == blocks[i].col)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                bottomCount++;
                            }
                        }

                        // Middle
                        if ((row - 1 == blocks[i].row || row + 1 == blocks[i].row) && col == blocks[i].col)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                middleCount++;
                            }
                        }

                        // Right
                        if ((col + 1 == blocks[i].col || col + 2 == blocks[i].col) && row == blocks[i].row)
                        {
                            if (blocks[i].blockType == blockType)
                            {
                                RightCount++;
                            }
                        }
                    }

                    if (topCount < 2 && bottomCount < 2 && middleCount < 2 && RightCount < 2)
                    {
                        // 2개 이상의 블럭이 존재하지 않으므로 이동한 블럭을 원 위치로 되돌립니다
                        yield return new WaitForSeconds(.2f);

                        otherBlock.col += 1;
                        col -= 1;
                    }
                    else
                    {
                        // 2개 이상의 블럭이 존재하므로 블럭 체크를 실행합니다
                        gm.isStart = true;
                    }
                    break;
            }

            // 마무리 작업
            gm.LRTBCheck();
            gm.isChecking = false;
        }
    }
}