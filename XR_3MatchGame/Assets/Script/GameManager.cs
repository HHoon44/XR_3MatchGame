using UnityEngine;

namespace XR_3MatchGame_Manager
{
    public class GameManager : MonoBehaviour
    {
        public Vector2Int boardSize = new Vector2Int(6, 6);

        public RectInt Bounds
        {
            get
            {
                Vector2Int position = new Vector2Int(-boardSize.x / 2, -boardSize.y / 2);

                return new RectInt(position, boardSize);
            }
        }

        private void Start()
        {
            StartSpawn();
        }

        /// <summary>
        /// 게임 시작 시
        /// 보드에 블럭을 세팅하는 메서드
        /// </summary>
        private void StartSpawn()
        {
            var bounds = Bounds;

            for (int i = bounds.xMin; i <= bounds.xMax; i++)
            {
                for (int j = bounds.yMin; j <= bounds.yMax; j++)
                {
                    // 오브젝트 생성
                }
            }
        }
    }
}