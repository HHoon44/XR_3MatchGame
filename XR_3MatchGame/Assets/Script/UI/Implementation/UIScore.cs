using TMPro;
using UnityEngine;
using XR_3MatchGame_InGame;

namespace XR_3MatchGame_UI
{
    public class UIScore : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI score;

        private GameManager gm;

        private void Start()
        {
            gm = GameManager.Instance;

            score.text = "Score : " + gm.Score.ToString();
        }

        private void Update()
        {
            if (gm.isChecking)
            {
                ScoreUpdate();
            }
        }

        public void ScoreUpdate()
        {
            score.text = "Score : " + gm.Score.ToString();
        }
    }
}