using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        public List<Block> rowBlocks = new List<Block>();

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

            BlockCheck();
        }

        /// <summary>
        /// ���� �� ���� Ȯ���ϴ� �޼���
        /// </summary>
        public void BlockCheck()
        {
            // (0, 6) (7, 13) (14, 20) (21, 27) (28, 34) (35, 41) (42, 48)
            // ��� ���� �� �� ���� üũ�մϴ�
            for (int i = 0; i < blocks.Count; i++)
            {
                for (int j = 0; j < blocks.Count; j++)
                {
                    // ����
                    if (blocks[i].col == 0)
                    {
                        // ���� �� �� ���̶�� �ǹ��Դϴ�
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

                    // ������
                    if (blocks[i].col == 6)
                    {
                        // ������ �� �� ���̶�� �ǹ��Դϴ�
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

            // ��� ���� �� �Ʒ� ���� üũ�մϴ�
            for (int i = 0; i < blocks.Count; i++)
            {
                for (int j = 0; j < blocks.Count; j++)
                {
                    // ����
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

                    // �Ʒ���
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
        /// ���� Ŭ��� ����ϴ� �޼���
        /// </summary>
        /// <returns></returns>
        IEnumerator LineClear()
        {
            var blockPool = ObjectPoolManager.Instance.GetPool<Block>(PoolType.Block);

            for (int i = 0; i < blocks.Count; i++)
            {
                // �� Ŭ���� �� �� ����
                if (blocks[i].leftBlock != null && blocks[i].rightBlock != null)
                {
                    // ���� ���������� ���� ���� �ִ��� üũ�մϴ�
                    if (blocks[i].blockType == blocks[i].leftBlock.blockType &&
                        blocks[i].blockType == blocks[i].rightBlock.blockType)
                    {
                        yield return new WaitForSeconds(.5f);

                        blockPool.ReturnPoolableObject(blocks[i].leftBlock);
                        blockPool.ReturnPoolableObject(blocks[i].rightBlock);
                        blockPool.ReturnPoolableObject(blocks[i]);

                        yield return new WaitForSeconds(.5f);

                        /// ���� col�� �����ϴ� ������ ��ĭ�� �����ִ� �ڵ� �ۼ�
                        for (int d_Col = blocks[i].leftBlock.col; d_Col <= blocks[i].rightBlock.col; d_Col++)
                        {

                        }

                        // ������ ���� ����Ʈ���� �����մϴ�
                        blocks.Remove(blocks[i].leftBlock);
                        blocks.Remove(blocks[i].rightBlock);
                        blocks.Remove(blocks[i]);
                    }
                }
            }
        }
    }
}