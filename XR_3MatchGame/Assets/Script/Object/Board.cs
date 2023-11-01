using UnityEngine;
using XR_3MatchGame.Util;
using XR_3MatchGame_InGame;
using XR_3MatchGame_Util;

namespace XR_3MatchGame_Object
{
    public class Board : MonoBehaviour
    {
        #region public



        #endregion

        #region private 

        private GameManager GM;

        #endregion

        private void Start()
        {
            GM = GameManager.Instance;

            // ���� ȭ�鿡 ����
            StartSpawn();
        }

        /// <summary>
        /// ���� ���� �� ���� ȭ�鿡 �����ϴ� �޼���
        /// </summary>
        private void StartSpawn()
        {
            var blockPool = ObjectPoolManager.Instance.GetPool<Block>(PoolType.Block);
            var size = GM.BoardSize;

            // �� ���� �۾�
            for (int row = 0; row < size.y; row++)
            {
                for (int col = 0; col < size.x; col++)
                {
                    var block = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                    block.transform.position = new Vector3(col, row, 0);
                    block.Initialize(col, row);
                    block.gameObject.SetActive(true);

                    // GM�� ����
                    GM.blocks.Add(block);
                }
            }

            GM.isChecking = true;
            GM.TBLRCheck();     // Board���� ó��
            StartCoroutine(GM.BlockClear()); // �̰͵� Board���� ó��
        }
    }
}