using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using XR_3MatchGame.Util;

namespace XR_3MatchGame_InGame
{
    public class LobbyManager : MonoBehaviour
    {
        public void ButtonOption()
        {

            switch (EventSystem.current.currentSelectedGameObject.transform.GetSiblingIndex())
            {
                // 상점 버튼
                case 0:
                    Debug.Log("상점 입니다.");
                    break;

                // 설정 버튼
                case 1:
                    Debug.Log("설정 입니다.");
                    break;
            }
        }

        /// <summary>
        /// 스테이지 선택창을 로드하는 메서드
        /// </summary>
        public void StageSelect()
        {
            SceneManager.LoadScene(SceneType.Select.ToString());
        }
    }
}
