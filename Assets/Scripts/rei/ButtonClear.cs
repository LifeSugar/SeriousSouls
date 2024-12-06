using UnityEngine;
using UnityEngine.UI;

namespace rei
{
    public class ButtonClear : MonoBehaviour
    {
        public Button clearButton;
        
        void OnDestroy()
        {
            if (clearButton != null)
            {
                clearButton.onClick.RemoveAllListeners(); // 清理事件，防止内存泄漏
            }
        }
        
    }
}