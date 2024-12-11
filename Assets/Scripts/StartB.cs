using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartB : MonoBehaviour
{
    public string level1;
    void Start () {
        this.GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        SceneManager.LoadScene(level1);
    }
}
