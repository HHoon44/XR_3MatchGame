using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Unity.VisualScripting;
using UnityEngine;
using XR_3MatchGame.Util;
using XR_3MatchGame_Object;
using XR_3MatchGame_Resource;
using XR_3MatchGame_Util;

namespace XR_3MatchGame_InGame
{
    public class GameManager : Singleton<GameManager>
    {
        //public List<Block> blocks { get; private set; } = new List<Block>();

        public List<Block> blocks = new List<Block>();

        public Vector2Int Bounds
        {
            get
            {
                // (0 ~ 7)
                Vector2Int position = new Vector2Int(7, 7);

                return position;
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
            for (int row = 0; row < Bounds.y; row++)
            {
                for (int col = 0; col < Bounds.x; col++)
                {
                    // Ǯ���� ��� ������ ���� �����ɴϴ�
                    var block = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                    block.transform.position = new Vector3(col, row, 0);

                    block.Initialize(col, row);

                    blocks.Add(block);

                    block.gameObject.SetActive(true);
                }
            }

            CheckBlock();
        }

        /// <summary>
        /// ��� ���� �����ؼ� üŷ�ϴ� �޼���
        /// </summary>
        public void CheckBlock()
        {
            Block curBlock = null;

            StartCoroutine(LRCheck());

            IEnumerator LRCheck()
            {
                // ��� ���� ���� ������ ���� üũ�մϴ�
                // (0, 6) (7, 13) (14, 20) (21, 27) (28, 34) (35, 41) (42, 48)
                for (int i = 0; i < blocks.Count; i++)
                {
                    curBlock = blocks[i];

                    for (int j = 0; j < blocks.Count; j++)
                    {
                        // ����
                        if (curBlock.col - 1 == blocks[j].col && curBlock.row == blocks[j].row)
                        {
                            curBlock.leftBlock = blocks[j];

                            // Test
                            curBlock.leftType = blocks[j].blockType;
                        }

                        // ������
                        if (curBlock.col + 1 == blocks[j].col && curBlock.row == blocks[j].row)
                        {
                            curBlock.rightBlock = blocks[j];

                            // Test
                            curBlock.rightType = blocks[j].blockType;
                        }
                    }

                    /// ���� �۾� ����
                    // �� Ŭ���� �� �� ����
                    if (curBlock.leftBlock != null && curBlock.rightBlock != null)
                    {
                        // ���� ������ ���� ���ٸ� ���� Ŭ��� �����մϴ�
                        if (curBlock.blockType == curBlock.leftBlock.blockType && curBlock.blockType == curBlock.rightBlock.blockType)
                        {
                            Debug.Log("Check");

                            var blockPool = ObjectPoolManager.Instance.GetPool<Block>(PoolType.Block);

                            blockPool.ReturnPoolableObject(curBlock.leftBlock);
                            blockPool.ReturnPoolableObject(curBlock.rightBlock);
                            blockPool.ReturnPoolableObject(curBlock);

                            blocks.Remove(curBlock.leftBlock);
                            blocks.Remove(curBlock.rightBlock);
                            blocks.Remove(curBlock);

                            yield return new WaitForSeconds(1f);

                            // ����� ��ġ�� ���ο� ���� ��ġ�մϴ�
                            for (int newCol = curBlock.leftBlock.col; newCol <= curBlock.rightBlock.col; newCol++)
                            {
                                var newBlock = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                                newBlock.transform.position = new Vector3(newCol, curBlock.row, 0);
                                newBlock.Initialize(newCol, curBlock.row);
                                newBlock.gameObject.SetActive(true);

                                blocks.Add(newBlock);
                            }
                        }
                    }
                }

            }
        }
    }
}