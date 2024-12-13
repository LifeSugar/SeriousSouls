using System.Collections;
using rei;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartB : MonoBehaviour
{
    public string level1;
    public ScreenFadeController screenFade;
    void Start () {
        this.GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        StartCoroutine(StartGame());
    }

    IEnumerator StartGame()
    {
        // 等待 FadeIn 协程完成
        yield return StartCoroutine(screenFade.FadeIn());

        // FadeIn 完成后等待 1.5 秒（可选）
        yield return new WaitForSeconds(1.5f);

        // 加载关卡
        SceneManager.LoadScene(level1);
        
    }
}
