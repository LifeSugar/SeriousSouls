using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace rei
{
    public class UIManager : MonoBehaviour
    {
        public float lerpSpeed;
        public Slider health;
        public Slider health_vis; //辅助滑块，用于显示可见的部分（例如延迟的数值变化动画效果）, 下面同理
        public Slider focus;
        public Slider focus_vis;
        public Slider stamina;
        public Slider stamina_vis;

        public float sizeMultiplier = 3.0f;


        public Text souls;
        public Text itemCount;

        private int currentSouls;
        private int currentItemCount;

        public GameObject interactCanvas;
        public Text instruction;

        private int item_idx;
        public List<ItemCard> ItemCards; //捡到东西的时候现实的UI


        public void InitSouls(int souls)
        {
            currentSouls = souls;
        }

        public void InitSlider(StatSliderType t, int value)
        {
            Slider slider = null;
            Slider vis = null;

            switch (t)
            {
                case StatSliderType.health:
                    slider = health;
                    vis = health_vis;
                    break;

                case StatSliderType.focus:
                    slider = focus;
                    vis = focus_vis;
                    break;

                case StatSliderType.stamina:
                    slider = stamina;
                    vis = stamina_vis;
                    break;
                default:
                    break;
            }

            slider.maxValue = value;
            vis.maxValue = value;
            RectTransform r = slider.GetComponent<RectTransform>();
            RectTransform r_vis = vis.GetComponent<RectTransform>();

            float value_actual = value * sizeMultiplier;
            value_actual = Mathf.Clamp(value_actual, 0, 1000);

            r.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value_actual);
            r_vis.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value_actual);
        }

        public void Tick(CharacterStats stats, float deltaTime, StateManager states)
        {
            
            //这个方法还有可以优化的地方
            health.value = Mathf.Lerp(health.value, stats._health, deltaTime * lerpSpeed * 2);
            focus.value = Mathf.Lerp(focus.value, stats._focus, deltaTime * lerpSpeed * 2);
            stamina.value = stats._stamina;

            //平滑更新灵魂数的显示，转换为整数
            currentSouls =
                Mathf.RoundToInt(Mathf.Lerp(currentSouls, stats._souls,
                    deltaTime * lerpSpeed * 10));
            souls.text = currentSouls.ToString();
            if (states.inventoryManager.curConsumable != null)
            {
                itemCount.text = states.inventoryManager.curConsumable.itemCount.ToString();
            }
           

            health_vis.value = Mathf.Lerp(health_vis.value, stats._health, deltaTime * lerpSpeed);
            focus_vis.value = Mathf.Lerp(focus_vis.value, stats._focus, deltaTime * lerpSpeed);
            stamina_vis.value = Mathf.Lerp(stamina_vis.value, stats._stamina, deltaTime * lerpSpeed);
        }

        public void AffectAll(int h, int f, int s)
        {
            InitSlider(StatSliderType.health, h);
            InitSlider(StatSliderType.focus, f);
            InitSlider(StatSliderType.stamina, s);
        }

        //交互卡片
        public void OpenInteractCanvas(UIActionType t)
        {
            switch (t)
            {
                case UIActionType.interact:
                    instruction.text = "Interact : key";
                    break;
                case UIActionType.open:
                    instruction.text = "Open : key";
                    break;
                case UIActionType.pickup:
                    instruction.text = "Pickup : key";
                    break;
                case UIActionType.talk:
                    instruction.text = "Talk : key";
                    break;
                default:
                    break;
            }

            interactCanvas.SetActive(true);
        }

        public void CloseInteractCanvas()
        {
            interactCanvas.SetActive(false);
        }

        //捡到东西的UI
        public void AddItemCard(Item i)
        {
            Debug.Log(i.itemName);
            Debug.Log(i.icon != null);
            
            ItemCards[item_idx].itemName.text = i.itemName;
            ItemCards[item_idx].icon.sprite = i.icon;
            ItemCards[item_idx].gameObject.SetActive(true);
            item_idx++;
            Debug.Log("AddItemCard");
        }

        public void CloseItemCards()
        {
            for (int i = 0; i < ItemCards.Count; i++)
            {
                ItemCards[i].gameObject.SetActive(false);
            }
        }

        public static UIManager instance; //单例

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            CloseInteractCanvas();
            CloseItemCards();
        }
    }

    //三种数值条
    public enum StatSliderType
    {
        health,
        focus,
        stamina
    }

    //交互种类
    public enum UIActionType
    {
        pickup,
        interact,
        open,
        talk
    }
}