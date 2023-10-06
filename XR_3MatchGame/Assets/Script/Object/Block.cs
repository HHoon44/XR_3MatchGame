using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using XR_3MatchGame_InGame;
using XR_3MatchGame_Util;

namespace XR_3MatchGame_Object
{
    public class Block : MonoBehaviour, IPoolableObject
    {
        public bool CanRecycle { get; set; } = true;

        public int col;
        public int row;

        public int targetX;     // ���� ����ؾ��ϳ�?
        public int targetY;     // ���� ����ؾ��ϳ�?

        private float swipeAngle = 0;
        private Vector2 firstTouchPosition;
        private Vector2 finalTouchPosition;
        private Vector2 tempPosition;

        [SerializeField]
        private Block otherBlock;

        private GameManager gm;

        public void Initialize(int col, int row)
        {
            this.col = col;
            this.row = row;

            gm = GameManager.Instance;

            targetX = (int)transform.position.x;
            targetY = (int)transform.position.y;
        }

        private void Update()
        {
            targetX = col;
            targetY = row;

            if (Mathf.Abs(targetX - transform.position.x) > .1f)
            {
                tempPosition = new Vector2(targetX, transform.position.y);
                transform.position = Vector2.Lerp(transform.position, tempPosition, .4f);
            }
            else
            {
                tempPosition = new Vector2(targetX, transform.position.y);
                transform.position = tempPosition;

            }
        }


        private void OnMouseDown()
        {
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        private void OnMouseUp()
        {
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }

        private void CalculateAngle()
        {
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y,
                finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;

            Debug.Log(swipeAngle);

            MoveBlock();
        }

        private void MoveBlock()
        {
            /// ���⿡ ���� �ޱ�
            if (swipeAngle > -45 && swipeAngle <= 45)
            {
                // ���������� ��������
                for (int i = 0; i < gm.blocks.Count; i++)
                {
                    if (gm.blocks[i].col == col + 1 &&
                        gm.blocks[i].row == row)
                    {
                        otherBlock = gm.blocks[i];
                        otherBlock.col -= 1;
                        col += 1;
                        return;
                    }
                }
            }
            else if (swipeAngle > 45 && swipeAngle <= 135)
            {
                // �������� ��������
                for (int i = 0; i < gm.blocks.Count; i++)
                {
                    if (gm.blocks[i].col == col &&
                        gm.blocks[i].row == row + 1)
                    {
                        otherBlock = gm.blocks[i];
                        otherBlock.row -= 1;
                        row += 1;
                        return;
                    }
                }
            }
            else if (swipeAngle > 135 || swipeAngle <= -135)
            {
                // �������� ��������
                for (int i = 0; i < gm.blocks.Count; i++)
                {
                    if (gm.blocks[i].col == col - 1 &&
                        gm.blocks[i].row == row)
                    {
                        otherBlock = gm.blocks[i];
                        otherBlock.col += 1;
                        col -= 1;
                        return;
                    }
                }
            }
            else if (swipeAngle < -45 && swipeAngle >= -135)
            {
                // �Ʒ������� ��������
                for (int i = 0; i < gm.blocks.Count; i++)
                {
                    if (gm.blocks[i].col == col &&
                        gm.blocks[i].row == row - 1)
                    {
                        otherBlock = gm.blocks[i];
                        otherBlock.row += 1;
                        row -= 1;
                        return;
                    }
                }
            }
        }
    }
}