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

            // 블럭을 화면에 세팅
            StartSpawn();
        }

        /// <summary>
        /// 게임 시작 시 블럭을 화면에 세팅하는 메서드
        /// </summary>
        private void StartSpawn()
        {
            var blockPool = ObjectPoolManager.Instance.GetPool<Block>(PoolType.Block);
            var size = GM.BoardSize;

            // 블럭 세팅 작업
            for (int row = 0; row < size.y; row++)
            {
                for (int col = 0; col < size.x; col++)
                {
                    var block = blockPool.GetPoolableObject(obj => obj.CanRecycle);
                    block.transform.position = new Vector3(col, row, 0);
                    block.Initialize(col, row);
                    block.gameObject.SetActive(true);

                    // GM에 저장
                    GM.blocks.Add(block);
                }
            }

            GM.isChecking = true;
            GM.TBLRCheck();     // Board에서 처리
            StartCoroutine(GM.BlockClear()); // 이것도 Board에서 처리
        }
    }
}