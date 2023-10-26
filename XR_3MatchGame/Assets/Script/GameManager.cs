using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using XR_3MatchGame.Util;
using XR_3MatchGame_Object;
using XR_3MatchGame_Resource;
using XR_3MatchGame_Util;

namespace XR_3MatchGame_InGame
{
    public class GameManager : Singleton<GameManager>
    {
        public List<Block> blocks = new List<Block>();              // �� ���� ������ ��� ���� ��Ƴ��� ����Ʈ

        public List<Block> checkBlocks = new List<Block>();         // ���ǿ� ���� ����ؾ��� ���� ��Ƴ��� ����Ʈ

        public bool isStart = false;        // �� üũ�� �����Ұ��ΰ�?
        public bool isChecking = false;        // ���� �� üũ�� �������ΰ�?

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
            /// ���� �� �� checkBlock�� ������ 3�� �ƴϸ� �ٷ� �� üŷ ����
            /// �ƴϸ� ��ź üŷ ����
            /// checkBlock Ŭ���� ���ִ°� ������ �ȵ�

            /// ��ź ���� ���� ���� ������ҵ�
            // üũ ����
            isChecking = true;

            Block curBlock = null;

            var blockPool = ObjectPoolManager.Instance.GetPool<Block>(PoolType.Block);
            var size = (BoardSize.x * BoardSize.y);

            if (checkBlocks.Count != 0)
            {
                if (checkBlocks[checkBlocks.Count - 1].blockType == BlockType.Boom)
                {
                    Debug.Log("��ź ����");

                    // ��ź�� ������ �ε����� ����
                    curBlock = checkBlocks[checkBlocks.Count - 1];

                    curBlock.spriteRenderer.sprite = SpriteLoader.GetSprite(AtlasType.BlockAtlas, curBlock.blockType.ToString());

                    yield return new WaitForSeconds(.4f);

                    switch (curBlock.boomType)
                    {
                        case BoomType.ColBoom:

                            // Ǯ�� ��ȯ
                            for (int i = 0; i < checkBlocks.Count - 1; i++)
                            {
                                blockPool.ReturnPoolableObject(checkBlocks[i]);
                            }

                            yield return new WaitForSeconds(.4f);

                            // ������ �� ���� �����ϴ� ������ �Ʒ��� ��������

                            var block_0 = checkBlocks[0];
                            var block_1 = checkBlocks[1];
                            var block_2 = checkBlocks[2];

                            checkBlocks.Clear();

                            Debug.Log(checkBlocks.Count);

                            if (block_0.row != (BoardSize.y - 1))
                            {
                                for (int i = 0; i < blocks.Count; i++)
                                { 
                                    // �������� ������
                                }
                            }

                            break;

                        case BoomType.RowBoom:
                            break;
                    }
                }
                else
                {
                    Debug.Log("��ź ����");

                    checkBlocks.Clear();
                }
            }

            /// ��ź ���� �������� 4���� ��Ȳ�� ���ϰ� �� ���ǿ� �´� �� ����
            /// �����ϸ� �� �ڸ��� ���ο� �� ��������! (���࿡ ���� ���� �����Ѵٸ� �� �̵����� �������)

            for (int i = 0; i < blocks.Count; i++)
            {
                // Left, Right üũ
                if (blocks[i].leftBlock != null && blocks[i].rightBlock != null)
                {
                    // ���� Left Right�� üũ
                    if (blocks[i].blockType == blocks[i].leftBlock.blockType && blocks[i].blockType == blocks[i].rightBlock.blockType)
                    {
                        curBlock = blocks[i];

                        yield return new WaitForSeconds(.4f);

                        // Ǯ�� ��ȯ
                        blockPool.ReturnPoolableObject(curBlock.rightBlock);
                        blockPool.ReturnPoolableObject(curBlock.leftBlock);
                        blockPool.ReturnPoolableObject(curBlock);

                        yield return new WaitForSeconds(.4f);

                        var f_Col = curBlock.leftBlock.col;
                        var m_Col = curBlock.col;
                        var l_Col = curBlock.rightBlock.col;

                        // ���̽� Row ��
                        var b_Row = curBlock.row;

                        // üũ�Ϸ��� ���� �� ���� �����ϴ��� Ȯ��
                        // 6���� Ȯ��
                        if (curBlock.row != (BoardSize.y - 1))
                        {
                            for (int j = 0; j < blocks.Count; j++)
                            {
                                // ������ �����ϴ� ��� ���� ã���ϴ�
                                if ((blocks[j].col == f_Col || blocks[j].col == m_Col || blocks[j].col == l_Col) &&
                                    blocks[j].row > b_Row)
                                {
                                    checkBlocks.Add(blocks[j]);
                                }
                            }

                            // ã�� ������ �����ݴϴ�
                            for (int j = 0; j < checkBlocks.Count; j++)
                            {
                                var targetRow = checkBlocks[j].row -= 1;

                                if (Mathf.Abs(targetRow - checkBlocks[j].transform.position.y) > .1f)
                                {
                                    Vector2 tempPosition = new Vector2(checkBlocks[j].transform.position.x, targetRow);
                                    checkBlocks[j].transform.position = Vector2.Lerp(checkBlocks[j].transform.position, tempPosition, .05f);
                                }
                            }
                        }

                        // üũ�� ������ List���� ���� ���ݴϴ�
                        blocks.Remove(curBlock.rightBlock);
                        blocks.Remove(curBlock.leftBlock);
                        blocks.Remove(curBlock);

                        // ����ִ� ĭ�� ������ ���մϴ�
                        var emptyBlockCount = size - (blocks.Count);

                        // ����ִ� ĭ�� ���ο� ���� �߰��� �� ����� Col, Row ���� ��Ƴ����ϴ�
                        // Row ���� ���� �����ڸ� �̿��ؼ� �����մϴ�
                        var n_Col = f_Col;
                        var n_Row = checkBlocks.Count > 0 ? checkBlocks[checkBlocks.Count - 1].row + 1 : b_Row;

                        yield return new WaitForSeconds(.4f);

                        for (int j = 0; j < emptyBlockCount; j++)
                        {
                            // EX
                            // Col = 0 1 2
                            // Row = 1���� 6����
                            if (n_Col <= l_Col && n_Row < BoardSize.y)
                            {
                                var newBlock = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                                newBlock.transform.position = new Vector3(n_Col, n_Row, 0);
                                newBlock.gameObject.SetActive(true);
                                newBlock.Initialize(n_Col, n_Row);

                                blocks.Add(newBlock);

                                n_Col++;
                            }

                            if (n_Col > l_Col)
                            {
                                // ������ �Ѿ��ٸ� ������ �մϴ�
                                n_Col = f_Col;

                                // ���� ĭ�� ä��� ���ؼ� ++�� �մϴ�
                                n_Row++;
                            }
                        }

                        LRTBCheck();
                        checkBlocks.Clear();
                        i = 0;
                    }
                }

                // Top, Bottom üũ
                if (blocks[i].topBlock != null && blocks[i].bottomBlock != null)
                {
                    // i��°�� �����ϴ� ���� Top, Bottom�� üũ�մϴ�
                    if (blocks[i].blockType == blocks[i].topBlock.blockType &&
                        blocks[i].blockType == blocks[i].bottomBlock.blockType)
                    {
                        curBlock = blocks[i];

                        yield return new WaitForSeconds(.4f);

                        // ���� Ǯ�� ��ȯ �մϴ�
                        blockPool.ReturnPoolableObject(curBlock.topBlock);
                        blockPool.ReturnPoolableObject(curBlock.bottomBlock);
                        blockPool.ReturnPoolableObject(curBlock);

                        yield return new WaitForSeconds(.4f);

                        // Base�� �� Col, Row ���� ��Ƴ����ϴ�
                        var b_Col = curBlock.col;
                        var b_Row = curBlock.topBlock.row;

                        // Top���� �� �� ������ Ȯ���մϴ�
                        if (b_Row != (BoardSize.y - 1))
                        {
                            // ��ĭ ���� ������ ã���ϴ�
                            for (int j = 0; j < blocks.Count; j++)
                            {
                                if (blocks[j].col == b_Col && blocks[j].row > b_Row)
                                {
                                    this.checkBlocks.Add(blocks[j]);
                                }
                            }

                            for (int j = 0; j < this.checkBlocks.Count; j++)
                            {
                                // ã�� ������ ��ĭ�� �����ݴϴ�
                                var targetRow = this.checkBlocks[j].row -= 3;

                                if (Mathf.Abs(targetRow - this.checkBlocks[j].transform.position.y) > .1f)
                                {
                                    Vector2 tempPosition = new Vector2(this.checkBlocks[j].transform.position.x, targetRow);
                                    this.checkBlocks[j].transform.position = Vector2.Lerp(this.checkBlocks[j].transform.position, tempPosition, .05f);
                                }
                            }
                        }

                        // üũ�� ������ List���� ���� ���ݴϴ�
                        blocks.Remove(curBlock.topBlock);
                        blocks.Remove(curBlock.bottomBlock);
                        blocks.Remove(curBlock);

                        // ����ִ� ĭ�� ������ ���մϴ�
                        var emptyBlockCount = size - (blocks.Count);

                        var n_Row = this.checkBlocks.Count > 0 ? this.checkBlocks[this.checkBlocks.Count - 1].row + 1 : b_Row - 2;

                        yield return new WaitForSeconds(.4f);

                        for (int j = 0; j < emptyBlockCount; j++)
                        {
                            if (n_Row < BoardSize.y)
                            {
                                var newBlock = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                                newBlock.transform.position = new Vector3(b_Col, n_Row, 0);
                                newBlock.gameObject.SetActive(true);
                                newBlock.Initialize(b_Col, n_Row);

                                blocks.Add(newBlock);

                                n_Row++;
                            }
                        }

                        LRTBCheck();
                        this.checkBlocks.Clear();
                        i = 0;
                    }
                }
            }

            // üũ ����
            isChecking = false;
        }
    }
}