using System.Collections;
using UnityEngine;
using XR_3MatchGame_InGame;
using XR_3MatchGame_Util;

namespace XR_3MatchGame_Object
{
    public class Block : MonoBehaviour, IPoolableObject
    {
        public bool CanRecycle { get; set; } = true;

        public int col;
        public int row;

        public int nextCol;     // 상대 블럭의 X 값
        public int nextRow;     // 상대 블럭의 Y 값

        private float swipeAngle = 0;
        private Vector2 firstTouchPosition;
        private Vector2 finalTouchPosition;
        private Vector2 tempPosition;

        [SerializeField]
        private Block otherBlock;

        private GameManager gm;

        /// <summary>
        /// 블럭 초기 세팅 메서드
        /// </summary>
        /// <param name="col">X 값</param>
        /// <param name="row">Y 값</param>
        public void Initialize(int col, int row)
        {
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
        /// 계산한 각도를 이용해서 블럭을 이동시키는 메서드
        /// </summary>
        private void MoveBlock()
        {
            /// 여기에 조건 달기
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
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 자신과 같은 블럭이 있는지 확인하는 메서드
        /// </summary>
        private void FindBlock()
        {
            Debug.Log("블럭 찾기 메서드 입니다.");
        }
    }
}