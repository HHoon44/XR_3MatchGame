using UnityEngine;
using UnityEngine.SceneManagement;
using XR_3MatchGame.Util;

namespace XR_3MatchGame
{
    public class Logo : MonoBehaviour
    {
        /// <summary>
        /// 로고 애니메이션에 바인딩할 메서드
        /// </summary>
        public void LogoAnimFun()
        {
            SceneManager.LoadScene(SceneType.Main.ToString());
        }
    }
}