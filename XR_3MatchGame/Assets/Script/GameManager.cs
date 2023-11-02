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
    // -최적화-
    // 나중에 yield return new WaitForSeconds는 선언해놓고 사용하자
    public class GameManager : Singleton<GameManager>
    {
        /// <summary>
        /// 현재 게임 상태 프로퍼티
        /// </summary>
        //public GameState GameState { get; private set; }

        // Test
        public GameState GameState;

        /// <summary>
        /// 현재 게임의 점수 프로퍼티
        /// </summary>
        public int Score { get; private set; }

        /// <summary>
        /// 보드 컴포넌트 프로퍼티
        /// </summary>
        public Board Board { get; private set; }

        [Header("Blocks")]
        public List<Block> blocks = new List<Block>();               // 인 게임 내에서 모든 블럭을 담아놓을 리스트
        public List<Block> downBlocks = new List<Block>();           // 내릴 블럭을 담아놓을 리스트
        public List<Block> delBlocks = new List<Block>();            // 삭제할 블럭을 담아놓을 리스트

        #region Public

        public bool isStart = false;                                // 블럭 체크를 실행할것인가?

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

        public void Initialize(Board board)
        {
            // 게임 시작
            GameState = GameState.Play;
            Board = board;
            XR_3MatchGame_Resource.ResourceManager.Instance.Initialize();
        }

        /// <summary>
        /// 스코어를 업데이트 하는 메서드
        /// </summary>
        /// <param name="score">스코어</param>
        public void ScoreUpdate(int score)
        {
            Score += score;
        }

        public void GameStateUpdate(GameState gameState)
        {
            GameState = gameState;
        }
    }
}