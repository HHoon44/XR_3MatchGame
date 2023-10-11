using System.Collections;
using System.Collections.Generic;
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
        private Vector2Int boardSize = new Vector2Int(6, 6);

        public List<Block> blocks { get; private set; } = new List<Block>();

        public RectInt Bounds
        {
            get
            {
                Vector2Int position = new Vector2Int(-boardSize.x / 2, -boardSize.y / 2);

                return new RectInt(position, boardSize);
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

            var bounds = Bounds;

            for (int i = bounds.xMin; i <= bounds.xMax; i++)
            {
                for (int j = bounds.yMin; j <= bounds.yMax; j++)
                {
                    // ��� ������ ���� �����ͼ� �����մϴ�
                    var block = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                    block.transform.position = new Vector3(i, j, 0);

                    // ���� ���� ������ ��ġ ���� ���� �մϴ�
                    block.Initialize(i, j);

                    // ȭ�鿡 ����� ���� �����մϴ�
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
            // X�� Y�� �ּҸ� �����ɴϴ�
            var colValue = Bounds.xMin;
            var rowValue = Bounds.yMin;

            Block curBlock = null;

            // ��� ���� �ݺ��ϸ鼭 ���� ��,�츦 üŷ�ϵ��� �մϴ�
            for (int i = 0; i < blocks.Count; i++)
            {
                /// ���⼭ ���� ����!! �ƴ� �� i�� 43�̳İ�
                /// ���� �߸��Ȱ� ����

                // �� �Ʒ������� �� üŷ�� �����մϴ�
                if (blocks[i].col == colValue && blocks[i].row == rowValue)
                {
                    if (rowValue > Bounds.yMax)
                    {
                        // ����ϴ� row ������ Y�� �ִ밪���� �������ٸ�
                        // ��� ���� Ȯ�� �� ���̹Ƿ� return
                        return;
                    }

                    // ��, �츦 üŷ�� ���� �����ɴϴ�
                    curBlock = blocks[i];

                    // curBlock�� ��, �츦 üŷ�մϴ�
                    StartCoroutine(LeftRightCheck(curBlock));

                    colValue++;

                    if (colValue > Bounds.xMax)
                    {
                        colValue = Bounds.xMin;

                        // ���� Row�� ��� Col�� Ȯ�������Ƿ� Row���� ++
                        rowValue++;
                    }
                }


            }

            // ���� ���� ��, �츦 üŷ�ϴ� �޼���
            IEnumerator LeftRightCheck(Block curBlock)
            {
                for (int i = 0; i < blocks.Count; i++)
                {
                    // curBlock�� ���ʿ� �����ϴ� ���� ã���ϴ�
                    if (curBlock.col - 1 == blocks[i].col &&
                        curBlock.row == blocks[i].row)
                    {
                        curBlock.leftBlock = blocks[i];
                    }

                    // curBlock�� �����ʿ� �����ϴ� ���� ã���ϴ�
                    if (curBlock.col + 1 == blocks[i].col &&
                        curBlock.row == blocks[i].row)
                    {
                        curBlock.rightBlock = blocks[i];
                    }
                }

                yield return null;
            }

        }
    }
}