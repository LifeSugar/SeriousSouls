using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextManager : MonoBehaviour
{
[SerializeField] private float letterPerSecond;//显示的速度
    
    private Text dialogText;
    public string dialog= "需要逐渐显示的文本啊";
    public bool CanRead=true ;

    public TextManager tx;

    private void Start()
    {
        dialogText = GetComponent<Text>();
        StartCoroutine(TypeDialog(dialog));
    }

    public IEnumerator TypeDialog(string dialog)//协程
    {
        
        dialogText.text = "";
        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f/letterPerSecond);//字体显示停顿时间
        }
        if (tx != null)
        {
            tx.gameObject.SetActive(true);
        }

    }
}