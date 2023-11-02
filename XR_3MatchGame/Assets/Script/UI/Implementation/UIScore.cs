using TMPro;
using UnityEngine;
using XR_3MatchGame_InGame;

namespace XR_3MatchGame_UI
{
    public class UIScore : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI score;

        private GameManager GM;

        private void Start()
        {
            GM = GameManager.Instance;

            score.text = "Score : " + GM.Score.ToString();
        }

        private void Update()
        {
            if (GM.GameState == XR_3MatchGame.Util.GameState.Play)
            {
                ScoreUpdate();
            }
        }

        public void ScoreUpdate()
        {
            score.text = "Score : " + GM.Score.ToString();
        }
    }
}