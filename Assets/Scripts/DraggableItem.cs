using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace rei
{
    public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public string itemName; // 物品名称
        public ItemType itemType; // 物品类型

        public Transform originalParent; // 原始父对象
        public Canvas canvas; // 用于在屏幕上拖拽的 Canvas
        
        private Image image;

        private void Start()
        {
            canvas = GetComponentInParent<Canvas>();
            image = GetComponent<Image>();
        }

        public void Setup(string name, ItemType type)
        {
            
            itemName = name;
            itemType = type;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log($"Dragging {gameObject.name}");
            if (image != null)
                image.raycastTarget = false;
            originalParent = transform.parent;
            transform.SetParent(canvas.transform); // 设置为 Canvas 的子物体
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position; // 跟随鼠标位置
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            transform.SetParent(originalParent);

            if (image != null)
                image.raycastTarget = true;
            Debug.Log($"Dropped on {gameObject.name}");
            transform.SetParent(originalParent); // 恢复原位置
        }
    }
}