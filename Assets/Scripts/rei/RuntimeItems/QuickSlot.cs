using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace rei.UI
{
    //这个就是左下角那四个槽
    [System.Serializable]
    public class QSlot
    {
        public Image icon; //图标
        public QSlotType type; //种类
    }

    public enum QSlotType
    {
        rh, //右手武器
        lh, //左手武器
        item, //道具
        spell //法术
    }


    public class QuickSlot : MonoBehaviour
    {
        public List<QSlot> slots = new List<QSlot>(); //储存所有快捷槽


        public void Init()
        {
            ClearIcons(); //初始化
        }

        public void ClearIcons()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].icon.gameObject.SetActive(false);
            }
        }


        public void UpdateSlot(QSlotType sType, Sprite spr)
        {
            QSlot q = GetSlot(sType); //获取种类
            q.icon.sprite = spr; //设置图标
            q.icon.gameObject.SetActive(true); //显示
        }


        //找到特定种类
        public QSlot GetSlot(QSlotType sType)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].type == sType)
                    return slots[i];
            }

            return null;
        }

        public static QuickSlot instance;

        void Awake()
        {
            instance = this;
        }
    }
}