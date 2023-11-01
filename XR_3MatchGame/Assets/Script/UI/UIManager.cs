using TMPro;
using UnityEngine;
using XR_3MatchGame.Util;
using XR_3MatchGame_InGame;

namespace XR_3MatchGame_UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject board;

        [SerializeField]
        private GameObject endUI;

        [SerializeField]
        private TextMeshProUGUI endDetail;

        [SerializeField]
        private GameObject endBack;

        private void Update()
        {
            if (!endUI.activeSelf)
            {
                if (GameManager.Instance.GameState == GameState.End)
                {
                    board.SetActive(false);
                    endUI.SetActive(true);
                    endBack.SetActive(true);

                    endDetail.text = "축하드립니다!";

                }
            }
        }
    }
}