using UnityEngine;
using UnityEngine.SceneManagement;

public class StartController : MonoBehaviour
{

    public void PlayBtnFun()
    {
        SceneManager.LoadScene("InGame");
    }
}