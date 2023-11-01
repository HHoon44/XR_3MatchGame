using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;
using XR_3MatchGame.Util;
using XR_3MatchGame_Object;
using XR_3MatchGame_Resource;
using XR_3MatchGame_Util;

namespace XR_3MatchGame_InGame
{
    // -����ȭ-
    // ���߿� yield return new WaitForSeconds�� �����س��� �������
    public class GameManager : Singleton<GameManager>
    {
        /// <summary>
        /// ���� ���� ���� ������Ƽ
        /// </summary>
        public GameState GameState { get; private set; }

        /// <summary>
        /// ���� ������ ���� ������Ƽ
        /// </summary>
        public int Score { get; private set; }

        /// <summary>
        /// ���� ������Ʈ ������Ƽ
        /// </summary>
        public Board Board { get; private set; }

        public List<Block> blocks = new List<Block>();              // �� ���� ������ ��� ���� ��Ƴ��� ����Ʈ
        public List<Block> downBlock = new List<Block>();           // ���� ���� ��Ƴ��� ����Ʈ
        public List<Block> delBlock = new List<Block>();            // ������ ���� ��Ƴ��� ����Ʈ

        #region Public

        public bool isStart = false;                                // �� üũ�� �����Ұ��ΰ�?
        public bool isChecking = false;                             // ���� �� üũ�� �������ΰ�?

        #endregion

        #region Private

        #endregion

        public Vector2Int BoardSize
        {
            get
            {
                // (0 ~ 7)
                Vector2Int boardSize = new Vector2Int(7, 7);

                return boardSize;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            if (gameObject == null)
            {
                return;
            }

            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            // ���� ����
            GameState = GameState.Play;

            Board = GameObject.Find("Board").GetComponent<Board>();

            XR_3MatchGame_Resource.ResourceManager.Instance.Initialize();
        }

        private void Update()
        {
            if (isStart == true)
            {
                isStart = false;
                StartCoroutine(BlockClear());
            }
        }

        /// <summary>
        /// ���� Top, Bottom, Left, Right�� Ȯ���ϴ� �޼���
        /// </summary>
        public void TBLRCheck()
        {
            // ��� �� Ž��
            for (int i = 0; i < blocks.Count; i++)
            {
                for (int j = 0; j < blocks.Count; j++)
                {
                    // Top
                    if (blocks[i].row == 6)
                    {
                        blocks[i].topBlock = null;
                        blocks[i].Top_T = BlockType.None;
                    }
                    else
                    {
                        if (blocks[i].row + 1 == blocks[j].row && blocks[i].col == blocks[j].col)
                        {
                            blocks[i].topBlock = blocks[j];

                            // Test
                            blocks[i].Top_T = blocks[j].blockType;
                        }
                    }

                    // Bottom
                    if (blocks[i].row == 0)
                    {
                        blocks[i].bottomBlock = null;
                        blocks[i].Bottom_T = BlockType.None;
                    }
                    else
                    {
                        if (blocks[i].row - 1 == blocks[j].row && blocks[i].col == blocks[j].col)
                        {
                            blocks[i].bottomBlock = blocks[j];

                            // Test
                            blocks[i].Bottom_T = blocks[j].blockType;
                        }
                    }

                    // Left
                    if (blocks[i].col == 0)
                    {
                        // ���� ���� Col = 0�� �����ϴ� ��
                        blocks[i].leftBlock = null;
                        blocks[i].Left_T = BlockType.None;
                    }
                    else
                    {
                        if (blocks[i].col - 1 == blocks[j].col && blocks[i].row == blocks[j].row)
                        {
                            blocks[i].leftBlock = blocks[j];

                            // Test
                            blocks[i].Left_T = blocks[j].blockType;
                        }
                    }

                    // Right
                    if (blocks[i].col == 6)
                    {
                        // ���� ���� Col = 6�� �����ϴ� ��
                        blocks[i].rightBlock = null;
                        blocks[i].Right_T = BlockType.None;
                    }
                    else
                    {
                        if (blocks[i].col + 1 == blocks[j].col && blocks[i].row == blocks[j].row)
                        {
                            blocks[i].rightBlock = blocks[j];

                            // Test
                            blocks[i].Right_T = blocks[j].blockType;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// �� Ŭ���� �� �� ������ ����ϴ� �޼���
        /// </summary>
        /// <returns></returns>
        public IEnumerator BlockClear()
        {
            Block curBlock = null;
            var blockPool = ObjectPoolManager.Instance.GetPool<Block>(PoolType.Block);
            var size = (BoardSize.x * BoardSize.y);

            // ��ź �۾� 
            if (delBlock.Count != 0)
            {
                if (delBlock[delBlock.Count - 1].blockType == BlockType.Boom)
                {
                    // ��ź�� ������ �ε����� ����
                    curBlock = delBlock[delBlock.Count - 1];
                    curBlock.spriteRenderer.sprite = SpriteLoader.GetSprite(AtlasType.BlockAtlas, curBlock.blockType.ToString());

                    yield return new WaitForSeconds(.4f);

                    switch (curBlock.boomType)
                    {
                        case BoomType.ColBoom:
                            var col_0 = delBlock[0].col;
                            var col_1 = delBlock[1].col;
                            var col_2 = delBlock[2].col;

                            var row_B = delBlock[0].row;

                            // Ǯ�� ��ȯ
                            for (int i = 0; i < delBlock.Count - 1; i++)
                            {
                                blockPool.ReturnPoolableObject(delBlock[i]);

                                // ���� �۾�
                                Score += delBlock[i].BlockScore;
                            }

                            yield return new WaitForSeconds(.4f);

                            // ������ ���� ���� �����ϴ� ���� ã��
                            if (row_B != (BoardSize.y - 1))
                            {
                                for (int i = 0; i < blocks.Count; i++)
                                {
                                    if ((col_0 == blocks[i].col || col_1 == blocks[i].col || col_2 == blocks[i].col) && row_B < blocks[i].row)
                                    {
                                        // ���� ���� ����
                                        downBlock.Add(blocks[i]);
                                    }
                                }
                            }

                            // �� �����ִ� �۾�
                            for (int i = 0; i < downBlock.Count; i++)
                            {
                                var targetRow = downBlock[i].row -= 1;

                                if (Mathf.Abs(targetRow - downBlock[i].transform.position.y) > .1f)
                                {
                                    Vector2 tempPosition = new Vector2(downBlock[i].transform.position.x, targetRow);
                                    downBlock[i].transform.position = Vector2.Lerp(downBlock[i].transform.position, tempPosition, .05f);
                                }
                            }

                            TBLRCheck();

                            blocks.Remove(delBlock[0]);
                            blocks.Remove(delBlock[1]);
                            blocks.Remove(delBlock[2]);

                            // ���� ������ ���̽��� �� Col, Row ��
                            var row_NewNum = downBlock.Count > 0 ? downBlock[downBlock.Count - 1].row + 1 : BoardSize.y - 1;

                            yield return new WaitForSeconds(.4f);

                            var newBlock_0 = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                            newBlock_0.transform.position = new Vector3(col_0, row_NewNum, 0);
                            newBlock_0.gameObject.SetActive(true);
                            newBlock_0.Initialize(col_0, row_NewNum);
                            blocks.Add(newBlock_0);

                            var newBlock_1 = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                            newBlock_1.transform.position = new Vector3(col_1, row_NewNum, 0);
                            newBlock_1.gameObject.SetActive(true);
                            newBlock_1.Initialize(col_1, row_NewNum);
                            blocks.Add(newBlock_1);

                            var newBlock_2 = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                            newBlock_2.transform.position = new Vector3(col_2, row_NewNum, 0);
                            newBlock_2.gameObject.SetActive(true);
                            newBlock_2.Initialize(col_2, row_NewNum);
                            blocks.Add(newBlock_2);

                            delBlock.Clear();
                            downBlock.Clear();
                            TBLRCheck();
                            break;

                        case BoomType.RowBoom:
                            var row_0 = delBlock[0].row;    // 1
                            var row_1 = delBlock[1].row;    // 2
                            var row_2 = delBlock[2].row;    // 4
                            var row_3 = delBlock[3].row;    // 3 ��ź row

                            var col_B = delBlock[0].col;

                            // Ǯ�� ��ȯ
                            for (int i = 0; i < delBlock.Count - 1; i++)
                            {
                                blockPool.ReturnPoolableObject(delBlock[i]);

                                Score += delBlock[i].BlockScore;
                            }

                            yield return new WaitForSeconds(.4f);

                            // ��ź ������ �۾�
                            // ��ź�� �� �Ʒ� ������ Ȯ��
                            if (row_3 > row_0)
                            {
                                var boomTargetRow = delBlock[3].row = row_0;

                                if (Mathf.Abs(boomTargetRow - delBlock[3].transform.position.y) > .1f)
                                {
                                    Vector2 tempPosition = new Vector2(delBlock[3].transform.position.x, boomTargetRow);
                                    delBlock[3].transform.position = Vector2.Lerp(delBlock[3].transform.position, tempPosition, .05f);
                                }
                            }

                            /// ���� �� ���� �����ϴ� �� ã��

                            // �� �� ���� �ƴ϶�� ���� �� ã�� ����
                            if (row_3 != 6 && row_2 != 6)
                            {
                                if (row_2 > row_3)
                                {
                                    // �Ϲ� ���� ��ź �� ���� ���� �ִ� ���
                                    for (int i = 0; i < blocks.Count; i++)
                                    {
                                        // Row�� Ŀ���ϰ� Col�� ���ƾ� �Ѵ�
                                        if (row_2 < blocks[i].row && col_B == blocks[i].col)
                                        {
                                            downBlock.Add(blocks[i]);
                                        }
                                    }

                                }
                                else if (row_2 < row_3)
                                {
                                    // ��ź ���� �Ϻ� �� ���� ���� �ִ� ���
                                    for (int i = 0; i < blocks.Count; i++)
                                    {
                                        // Row�� Ŀ���ϰ� Col�� ���ƾ� �Ѵ�
                                        if (row_3 < blocks[i].row && col_B == blocks[i].col)
                                        {
                                            downBlock.Add(blocks[i]);
                                        }
                                    }
                                }
                            }

                            for (int i = 0; i < downBlock.Count; i++)
                            {
                                var targetRow = downBlock[i].row -= 3;

                                if (Mathf.Abs(targetRow - downBlock[i].transform.position.y) > .1f)
                                {
                                    Vector2 tempPosition = new Vector2(downBlock[i].transform.position.x, targetRow);
                                    downBlock[i].transform.position = Vector2.Lerp(downBlock[i].transform.position, tempPosition, .05f);
                                }
                            }

                            blocks.Remove(delBlock[0]);
                            blocks.Remove(delBlock[1]);
                            blocks.Remove(delBlock[2]);

                            yield return new WaitForSeconds(.4f);

                            // ���ο� Row ��
                            var newRow = downBlock.Count > 0 ? downBlock[downBlock.Count - 1].row + 1 : delBlock[3].row + 1;
                            var emptyBlockCount = size - blocks.Count;

                            for (int i = 0; i < emptyBlockCount; i++)
                            {
                                if (newRow < BoardSize.y)
                                {
                                    var newBlock = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                                    newBlock.transform.position = new Vector3(col_B, newRow, 0);
                                    newBlock.gameObject.SetActive(true);
                                    newBlock.Initialize(col_B, newRow);
                                    blocks.Add(newBlock);

                                    newRow++;
                                }
                            }

                            TBLRCheck();
                            delBlock.Clear();
                            downBlock.Clear();
                            break;
                    }
                }
            }

            yield return new WaitForSeconds(.4f);

            // �Ϲ� �� �۾�
            for (int i = 0; i < blocks.Count; i++)
            {
                // Left, Right
                if (blocks[i].leftBlock != null && blocks[i].rightBlock != null)
                {
                    // üũ�� ��
                    curBlock = blocks[i];

                    if (curBlock.leftBlock.blockType == curBlock.blockType && curBlock.rightBlock.blockType == curBlock.blockType)
                    {
                        // ������ ���� ���� ����ҿ� ����
                        delBlock.Add(curBlock);
                        delBlock.Add(curBlock.leftBlock);
                        delBlock.Add(curBlock.rightBlock);

                        var col_L = curBlock.leftBlock.col;
                        var col_M = curBlock.col;
                        var col_R = curBlock.rightBlock.col;
                        var row_M = curBlock.row;

                        // Ǯ ��ȯ �� ���� ������Ʈ
                        for (int j = 0; j < delBlock.Count; j++)
                        {
                            blockPool.ReturnPoolableObject(delBlock[i]);
                            ScoreUpdate(delBlock[j].BlockScore);
                            blocks.Remove(delBlock[j]);
                        }

                        // �� ���� �ִ� ������ Ȯ��
                        if (row_M != (BoardSize.y - 1))
                        {
                            downBlock.Clear();

                            // ���� �� ���� �����ϴ� �� Ž��
                            for (int j = 0; j < blocks.Count; j++)
                            {
                                if ((blocks[j].col == col_L || blocks[j].col == col_M || blocks[j].col == col_R) && blocks[j].row > row_M)
                                {
                                    // ���� �� ����
                                    downBlock.Add(blocks[j]);
                                }
                            }
                        }

                        yield return new WaitForSeconds(.4f);

                        // ���� ������ �۾�
                        for (int j = 0; j < downBlock.Count; j++)
                        {
                            var targetRow = downBlock[j].row -= 1;

                            if (Mathf.Abs(targetRow - downBlock[j].transform.position.y) > .1f)
                            {
                                Vector2 tempPosition = new Vector2(downBlock[j].transform.position.x, targetRow);
                                downBlock[j].transform.position = Vector2.Lerp(downBlock[j].transform.position, tempPosition, .05f);
                            }
                        }

                        // ����ִ� ĭ�� ����
                        var emptyCount = size - blocks.Count;
                        var col_NewNum = col_L;
                        var row_NewNum = downBlock.Count > 0 ? downBlock[downBlock.Count - 1].row + 1 : row_M;

                        yield return new WaitForSeconds(.4f);

                        // �� ������ �� ���� �۾�
                        for (int j = 0; j < emptyCount; j++)
                        {
                            if (col_NewNum <= col_R && row_NewNum < BoardSize.y)
                            {
                                var newBlock = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                                newBlock.transform.position = new Vector3(col_NewNum, row_NewNum, 0);
                                newBlock.gameObject.SetActive(true);
                                newBlock.Initialize(col_NewNum, row_NewNum);

                                blocks.Add(newBlock);

                                col_NewNum++;
                            }

                            if (col_NewNum > col_R)
                            {
                                // ���� ���� ä��� ���� �۾�
                                col_NewNum = col_L;
                                row_NewNum++;
                            }
                        }

                        delBlock.Clear();
                        downBlock.Clear();
                        TBLRCheck();

                        // Left, Right �� ��Ī�� ������ �ѹ��� 0���� ���� -> ���� �ѹ� ����
                        // i = 0;
                    }
                }

                yield return new WaitForSeconds(.4f);

                // Top, Bottom
                if (blocks[i].topBlock != null && blocks[i].bottomBlock != null)
                {
                    if (blocks[i].topBlock.blockType == blocks[i].blockType && blocks[i].bottomBlock.blockType == blocks[i].blockType)
                    {
                        // curBlock = blocks[i];
                        delBlock.Add(curBlock.topBlock);
                        delBlock.Add(curBlock.bottomBlock);
                        delBlock.Add(curBlock);

                        for (int j = 0; j < delBlock.Count; j++)
                        {
                            blockPool.ReturnPoolableObject(delBlock[i]);
                            ScoreUpdate(delBlock[j].BlockScore);
                            blocks.Remove(delBlock[j]);
                        }

                        var col_B = curBlock.col;
                        var row_B = curBlock.topBlock.row;

                        downBlock.Clear();

                        // �� �� ������ Ȯ��
                        if (row_B != (BoardSize.y - 1))
                        {
                            // ���� �� Ž��
                            for (int j = 0; j < blocks.Count; j++)
                            {
                                if ((col_B == blocks[j].col) && (row_B < blocks[j].row))
                                {
                                    downBlock.Add(blocks[j]);
                                }
                            }
                        }

                        yield return new WaitForSeconds(.4f);

                        // �� ������ �۾�
                        for (int j = 0; j < downBlock.Count; j++)
                        {
                            var targetRow = downBlock[j].row -= 3;

                            if (Mathf.Abs(targetRow - downBlock[j].transform.position.y) > .1f)
                            {
                                Vector2 tempPosition = new Vector2(downBlock[j].transform.position.x, targetRow);
                                downBlock[j].transform.position = Vector2.Lerp(downBlock[j].transform.position, tempPosition, .05f);
                            }
                        }

                        // ����ִ� ĭ ����
                        var emptyBlockCount = size - blocks.Count;

                        var n_Row = downBlock.Count > 0 ? downBlock[downBlock.Count - 1].row + 1 : row_B - 2;

                        yield return new WaitForSeconds(.4f);

                        for (int j = 0; j < emptyBlockCount; j++)
                        {
                            if (n_Row < BoardSize.y)
                            {
                                var newBlock = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                                newBlock.transform.position = new Vector3(col_B, n_Row, 0);
                                newBlock.gameObject.SetActive(true);
                                newBlock.Initialize(col_B, n_Row);

                                blocks.Add(newBlock);

                                n_Row++;
                            }
                        }

                        delBlock.Clear();
                        downBlock.Clear();
                        TBLRCheck();

                        // Top, Bottom �� ��Ī�� ������ �ѹ��� 0���� ����
                        // i = 0;
                    }
                }
            }

            yield return new WaitForSeconds(.4f);

            // üũ ����
            isChecking = false;

            // ���� Ŭ���� ����
            if (Score >= 100)
            {
                for (int i = 0; i < blocks.Count; i++)
                {
                    blockPool.ReturnPoolableObject(blocks[i]);
                }

                GameState = GameState.End;
            }
        }

        /// <summary>
        /// ���ھ ������Ʈ �ϴ� �޼���
        /// </summary>
        /// <param name="score">���ھ�</param>
        private void ScoreUpdate(int score)
        {
            Score += score;
        }
    }
}