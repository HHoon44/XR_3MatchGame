using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XR_3MatchGame.Util;
using XR_3MatchGame_Object;
using XR_3MatchGame_Resource;
using XR_3MatchGame_Util;

namespace XR_3MatchGame_InGame
{
    public class GameManager : Singleton<GameManager>
    {
        public List<Block> blocks = new List<Block>();

        // Test
        public List<Block> downBlcok = new List<Block>();

        // Test
        public IEnumerator test_Coroutine;

        public bool isCheck = false;

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
            if (isCheck == true)
            {
                isCheck = false;
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
            var blockPool = ObjectPoolManager.Instance.GetPool<Block>(PoolType.Block);

            // ���� ��
            Block curBlock = null;

            for (int i = 0; i < blocks.Count; i++)
            {
                if (blocks[i].leftBlock != null &&
                    blocks[i].rightBlock != null)
                {
                    // i��°�� �����ϴ� ���� Left, Right�� üũ�մϴ�
                    if (blocks[i].blockType == blocks[i].leftBlock.blockType &&
                        blocks[i].blockType == blocks[i].rightBlock.blockType)
                    {
                        curBlock = blocks[i];

                        // ������ �ְ� ���� üũ�� �����մϴ�
                        yield return new WaitForSeconds(.4f);

                        blockPool.ReturnPoolableObject(curBlock.leftBlock);
                        blockPool.ReturnPoolableObject(curBlock.rightBlock);
                        blockPool.ReturnPoolableObject(curBlock);

                        yield return new WaitForSeconds(.4f);

                        // ó���� ������ Col ���� ��Ƴ����ϴ�
                        var c_Col = curBlock.leftBlock.col;
                        var l_Col = curBlock.rightBlock.col;

                        // ó�� Row ���� ��Ƴ����ϴ�
                        var c_Row = curBlock.row;

                        // ���� ���� �� ���� ������ üũ �մϴ�
                        if (curBlock.row != (BoardSize.y - 1))
                        {
                            // ��ĭ ���� ������ ã���ϴ�
                            for (int j = 0; j < blocks.Count; j++)
                            {
                                if (c_Col < (l_Col + 1))
                                {
                                    if (c_Col == blocks[j].col && c_Row < blocks[j].row)
                                    {
                                        downBlcok.Add(blocks[j]);
                                        c_Col++;
                                    }

                                    if (c_Col > l_Col)
                                    {
                                        // Col�� ������ ����� �ʵ��� �����մϴ�
                                        c_Col = curBlock.leftBlock.col;
                                    }
                                }
                            }

                            for (int j = 0; j < downBlcok.Count; j++)
                            {
                                // ã�� ������ ��ĭ�� �����ݴϴ�
                                var targetRow = downBlcok[j].row -= 1;

                                if (Mathf.Abs(targetRow - downBlcok[j].transform.position.y) > .1f)
                                {
                                    Vector2 tempPosition = new Vector2(downBlcok[j].transform.position.x, targetRow);
                                    downBlcok[j].transform.position = Vector2.Lerp(downBlcok[j].transform.position, tempPosition, .05f);
                                }
                            }
                        }

                        // üũ�� ������ List���� ���� ���ݴϴ�
                        blocks.Remove(curBlock.leftBlock);
                        blocks.Remove(curBlock.rightBlock);
                        blocks.Remove(curBlock);

                        // ����ִ� ĭ�� ������ ���մϴ�
                        var emptyBlockCount = (BoardSize.x * BoardSize.y) - (blocks.Count);

                        Debug.Log(emptyBlockCount);

                        // ����ִ� ��ġ�� Col, Row ���� ���� �س����ϴ�
                        // ( Row�� +1�� ����� ��ĭ�� ù��° Row ���� �� ������ )
                        // c_Row�� �����Ǹ� �� ���� ���̶�� �� �Դϴ�
                        var n_Col = c_Col;
                        var n_Row = downBlcok.Count > 0 ? downBlcok[downBlcok.Count - 1].row + 1 : c_Row;

                        yield return new WaitForSeconds(.4f);

                        if (downBlcok.Count == 0)
                        {
                            for (int j = 0; j < emptyBlockCount; j++)
                            {
                                // ��ĭ ���� ���� ���ٸ�
                                // �ٷ� ���� ���� �մϴ�
                                if (n_Col <= l_Col && n_Row < BoardSize.y)
                                {
                                    var newBlock = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                                    newBlock.transform.position = new Vector3(n_Col, n_Row, 0);
                                    newBlock.gameObject.SetActive(true);
                                    newBlock.Initialize(n_Col, n_Row);

                                    blocks.Add(newBlock);

                                    n_Col++;
                                }
                            }
                        }
                        else
                        {
                            /// �� ã�� ���⼭ ���� ���µ�
                            for (int j = 0; j < emptyBlockCount; j++)
                            {
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
                                    // ++ �������� Col�� ������ �Ѿ��� ��� �ٽ� �ʱ�ȭ ���ݴϴ�
                                    n_Col = c_Col;

                                    // ���� Row�� ���� �����Ͽ����Ƿ� ++�� ���ݴϴ�
                                    n_Row++;
                                }
                            }
                        }

                        // �۾��� �Ϸ� �Ǿ��ٸ� ����ݴϴ�
                        downBlcok.Clear();

                        /// �̰� �´°� ����
                        i = 0;
                    }
                }

            }
        }
    }
}