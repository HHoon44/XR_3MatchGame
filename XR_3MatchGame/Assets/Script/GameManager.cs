using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
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
        public List<Block> blocks = new List<Block>();              // �� ���� ������ ��� ���� ��Ƴ��� ����Ʈ

        public List<Block> checkBlocks = new List<Block>();         // ���ǿ� ���� ����ؾ��� ���� ��Ƴ��� ����Ʈ

        public bool isStart = false;            // �� üũ�� �����Ұ��ΰ�?
        public bool isChecking = false;         // ���� �� üũ�� �������ΰ�?

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
            XR_3MatchGame_Resource.ResourceManager.Instance.Initialize();
            StartSpawn();
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
        /// ���� ���� ��
        /// ���忡 ���� �����ϴ� �޼���
        /// </summary>
        private void StartSpawn()
        {
            var blockPool = ObjectPoolManager.Instance.GetPool<Block>(PoolType.Block);

            // ȭ�鿡 ���� ���� �մϴ�
            for (int row = 0; row < BoardSize.y; row++)
            {
                for (int col = 0; col < BoardSize.x; col++)
                {
                    // Ǯ���� ��� ������ ���� �����ɴϴ�
                    var block = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                    block.transform.position = new Vector3(col, row, 0);

                    block.Initialize(col, row);

                    blocks.Add(block);

                    block.gameObject.SetActive(true);
                }
            }

            LRTBCheck();
            StartCoroutine(BlockClear());
        }

        /// <summary>
        /// ���� Top, Bottom, Left, Right�� Ȯ���ϴ� �޼���
        /// </summary>
        public void LRTBCheck()
        {
            // ��� ���� Left, Right ���� üũ�մϴ�
            for (int i = 0; i < blocks.Count; i++)
            {
                for (int j = 0; j < blocks.Count; j++)
                {
                    // Left
                    if (blocks[i].col == 0)
                    {
                        // Left �� �� ���̶�� �ǹ��Դϴ�
                        blocks[i].leftBlock = null;
                        blocks[i].leftType = BlockType.None;
                    }
                    else
                    {
                        if (blocks[i].col - 1 == blocks[j].col && blocks[i].row == blocks[j].row)
                        {
                            blocks[i].leftBlock = blocks[j];

                            // Test
                            blocks[i].leftType = blocks[j].blockType;
                        }
                    }

                    // Right
                    if (blocks[i].col == 6)
                    {
                        // Right �� �� ���̶�� �ǹ��Դϴ�
                        blocks[i].rightBlock = null;
                        blocks[i].rightType = BlockType.None;
                    }
                    else
                    {
                        if (blocks[i].col + 1 == blocks[j].col && blocks[i].row == blocks[j].row)
                        {
                            blocks[i].rightBlock = blocks[j];

                            // Test
                            blocks[i].rightType = blocks[j].blockType;
                        }
                    }
                }
            }

            // ��� ���� Top, Bottom ���� üũ�մϴ�
            for (int i = 0; i < blocks.Count; i++)
            {
                for (int j = 0; j < blocks.Count; j++)
                {
                    // Top
                    if (blocks[i].row == 6)
                    {
                        blocks[i].topBlock = null;
                        blocks[i].topType = BlockType.None;
                    }
                    else
                    {
                        if (blocks[i].col == blocks[j].col && blocks[i].row + 1 == blocks[j].row)
                        {
                            blocks[i].topBlock = blocks[j];

                            // Test
                            blocks[i].topType = blocks[j].blockType;
                        }
                    }

                    // Bottom
                    if (blocks[i].row == 0)
                    {
                        blocks[i].bottomBlock = null;
                        blocks[i].bottomType = BlockType.None;
                    }
                    else
                    {
                        if (blocks[i].col == blocks[j].col && blocks[i].row - 1 == blocks[j].row)
                        {
                            blocks[i].bottomBlock = blocks[j];

                            // Test
                            blocks[i].bottomType = blocks[j].blockType;
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
            // üũ ����
            //isChecking = true;

            Block curBlock = null;

            var blockPool = ObjectPoolManager.Instance.GetPool<Block>(PoolType.Block);
            var size = (BoardSize.x * BoardSize.y);

            // ��ź �۾�
            if (checkBlocks.Count != 0)
            {
                if (checkBlocks[checkBlocks.Count - 1].blockType == BlockType.Boom)
                {
                    // ��ź�� ������ �ε����� ����
                    curBlock = checkBlocks[checkBlocks.Count - 1];

                    curBlock.spriteRenderer.sprite = SpriteLoader.GetSprite(AtlasType.BlockAtlas, curBlock.blockType.ToString());

                    yield return new WaitForSeconds(.4f);

                    switch (curBlock.boomType)
                    {
                        case BoomType.ColBoom:
                            var col_0 = checkBlocks[0].col;
                            var col_1 = checkBlocks[1].col;
                            var col_2 = checkBlocks[2].col;

                            var row_0 = checkBlocks[0].row;

                            // Ǯ�� ��ȯ
                            for (int i = 0; i < checkBlocks.Count - 1; i++)
                            {
                                blockPool.ReturnPoolableObject(checkBlocks[i]);
                            }

                            yield return new WaitForSeconds(.4f);

                            // �߿�
                            checkBlocks.Clear();

                            // ������ ���� ���� �����ϴ� ���� ã��
                            if (row_0 != (BoardSize.y - 1))
                            {
                                for (int i = 0; i < blocks.Count; i++)
                                {
                                    if ((col_0 == blocks[i].col || col_1 == blocks[i].col || col_2 == blocks[i].col) && row_0 < blocks[i].row)
                                    {
                                        checkBlocks.Add(blocks[i]);
                                    }
                                }
                            }

                            // �� �����ִ� �۾�
                            for (int i = 0; i < checkBlocks.Count; i++)
                            {
                                var targetRow = checkBlocks[i].row -= 1;

                                if (Mathf.Abs(targetRow - checkBlocks[i].transform.position.y) > .1f)
                                {
                                    Vector2 tempPosition = new Vector2(checkBlocks[i].transform.position.x, targetRow);
                                    checkBlocks[i].transform.position = Vector2.Lerp(checkBlocks[i].transform.position, tempPosition, .05f);
                                }
                            }

                            blocks.Remove(checkBlocks[0]);
                            blocks.Remove(checkBlocks[1]);
                            blocks.Remove(checkBlocks[2]);

                            // ���� ���� ������ ���̽��� �� Col, Row ��
                            var row_NewNum = checkBlocks.Count > 0 ? checkBlocks[checkBlocks.Count - 1].row + 1 : BoardSize.y - 1;

                            yield return new WaitForSeconds(.4f);

                            var newBlock_0 = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                            newBlock_0.transform.position = new Vector3(col_0, row_NewNum, 0);
                            newBlock_0.gameObject.SetActive(true);
                            newBlock_0.Initialize(col_0, row_NewNum);

                            var newBlock_1 = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                            newBlock_1.transform.position = new Vector3(col_1, row_NewNum, 0);
                            newBlock_1.gameObject.SetActive(true);
                            newBlock_1.Initialize(col_1, row_NewNum);

                            var newBlock_2 = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                            newBlock_2.transform.position = new Vector3(col_2, row_NewNum, 0);
                            newBlock_2.gameObject.SetActive(true);
                            newBlock_2.Initialize(col_2, row_NewNum);

                            blocks.Add(newBlock_0);
                            blocks.Add(newBlock_1);
                            blocks.Add(newBlock_2);

                            LRTBCheck();
                            checkBlocks.Clear();
                            break;

                        case BoomType.RowBoom:
                            break;
                    }
                }
                else
                {
                    checkBlocks.Clear();
                }
            }

            // �Ϲ� �� �۾�
            for (int i = 0; i < blocks.Count; i++)
            {
                // Left, Right üũ
                if (blocks[i].leftBlock != null && blocks[i].rightBlock != null)
                {
                    if (blocks[i].leftBlock.blockType == blocks[i].blockType && blocks[i].rightBlock.blockType == blocks[i].blockType)
                    {
                        curBlock = blocks[i];

                        yield return new WaitForSeconds(.4f);

                        var col_L = curBlock.leftBlock.col;
                        var col_M = curBlock.col;
                        var col_R = curBlock.rightBlock.col;

                        var row_M = curBlock.row;

                        // Ǯ�� ��ȯ
                        blockPool.ReturnPoolableObject(curBlock.leftBlock);
                        blockPool.ReturnPoolableObject(curBlock.rightBlock);
                        blockPool.ReturnPoolableObject(curBlock);

                        // �� ���� �ִ� ������ Ȯ��
                        if (curBlock.row != (BoardSize.y - 1))
                        {
                            for (int j = 0; j < blocks.Count; j++)
                            {
                                // ������ ������ ���� �����ϴ� ������ Ž��
                                if ((blocks[j].col == col_L || blocks[j].col == col_M || blocks[j].col == col_R) && blocks[j].row > row_M)
                                {
                                    checkBlocks.Add(blocks[j]);
                                }
                            }
                        }

                        yield return new WaitForSeconds(.4f);

                        // ���� ������ �۾�
                        for (int j = 0; j < checkBlocks.Count; j++)
                        {
                            var targetRow = checkBlocks[j].row -= 1;

                            if (Mathf.Abs(targetRow - checkBlocks[j].transform.position.y) > .1f)
                            {
                                Vector2 tempPosition = new Vector2(checkBlocks[j].transform.position.x, targetRow);

                                checkBlocks[j].transform.position = Vector2.Lerp(checkBlocks[j].transform.position, tempPosition, .05f);
                            }
                        }

                        blocks.Remove(curBlock.rightBlock);
                        blocks.Remove(curBlock.leftBlock);
                        blocks.Remove(curBlock);

                        // ����ִ� ĭ�� ����
                        var emptyBlockCount = size - blocks.Count;

                        var col_NewNum = col_L;
                        var row_Newnum = checkBlocks.Count > 0 ? checkBlocks[checkBlocks.Count - 1].row + 1 : row_M;

                        yield return new WaitForSeconds(.4f);

                        for (int j = 0; j < emptyBlockCount; j++)
                        {
                            if (col_NewNum <= col_R && row_Newnum < BoardSize.y)
                            {
                                var newBlock = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                                newBlock.transform.position = new Vector3(col_NewNum, row_Newnum, 0);
                                newBlock.gameObject.SetActive(true);
                                newBlock.Initialize(col_NewNum, row_Newnum);

                                blocks.Add(newBlock);

                                col_NewNum++;
                            }

                            if (col_NewNum > col_R)
                            {
                                // ���� ���� ä��� ���� �۾�
                                col_NewNum = col_L;
                                row_Newnum++;
                            }
                        }

                        // �߿�
                        checkBlocks.Clear();
                        LRTBCheck();
                        i = 0;
                    }
                }

                // Top, Bottom üũ
                if (blocks[i].topBlock != null && blocks[i].bottomBlock != null)
                {
                    if (blocks[i].topBlock.blockType == blocks[i].blockType && blocks[i].bottomBlock.blockType == blocks[i].blockType)
                    {
                        curBlock = blocks[i];

                        yield return new WaitForSeconds(.4f);

                        // Ǯ�� ��ȯ
                        blockPool.ReturnPoolableObject(curBlock.topBlock);
                        blockPool.ReturnPoolableObject(curBlock.bottomBlock);
                        blockPool.ReturnPoolableObject(curBlock);

                        var col_B = curBlock.col;
                        var row_B = curBlock.topBlock.row;

                        // �� �� ������ Ȯ��
                        if (row_B != (BoardSize.y - 1))
                        {
                            // ���� �� Ž��
                            for (int j = 0; j < blocks.Count; j++)
                            {
                                if (blocks[j].col == col_B && blocks[j].row > row_B)
                                {
                                    checkBlocks.Add(blocks[j]);
                                }
                            }
                        }

                        yield return new WaitForSeconds(.4f);

                        // �� ������ �۾�
                        for (int j = 0; j < checkBlocks.Count; j++)
                        {
                            var targetRow = checkBlocks[j].row -= 3;

                            if (Mathf.Abs(targetRow - checkBlocks[j].transform.position.y) > .1f)
                            {
                                Vector2 tempPosition = new Vector2(checkBlocks[j].transform.position.x, targetRow);

                                checkBlocks[j].transform.position = Vector2.Lerp(checkBlocks[j].transform.position, tempPosition, .05f);
                            }
                        }

                        blocks.Remove(curBlock.topBlock);
                        blocks.Remove(curBlock.bottomBlock);
                        blocks.Remove(curBlock);

                        // ����ִ� ĭ ����
                        var emptyBlockCount = size - (blocks.Count);

                        var n_Row = checkBlocks.Count > 0 ? checkBlocks[checkBlocks.Count - 1].row + 1 : row_B - 2;

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

                        // �߿�
                        checkBlocks.Clear();
                        LRTBCheck();
                        i = 0;
                    }
                }
            }

            yield return new WaitForSeconds(.4f);

            // üũ ����
            isChecking = false;

            Debug.Log("BlockClear End");
        }
    }
}