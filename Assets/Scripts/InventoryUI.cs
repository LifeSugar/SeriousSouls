using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace rei
{
    public class InventoryUI : MonoBehaviour
    {
        public Transform weaponGrid; // 武器网格区域
        public Transform itemGrid;   // 消耗品网格区域
        public Transform spellGrid;  // 法术网格区域

        public GameObject slotPrefab; // 用于显示物品的槽预制体

        /// <summary>
        /// 更新右侧库存网格UI
        /// </summary>
        /// <param name="inventory">玩家的库存对象</param>
        public void UpdateInventoryUI(Inventory inventory)
        {
            ClearGrid(weaponGrid);
            ClearGrid(itemGrid);
            ClearGrid(spellGrid);

            foreach (var weapon in inventory.weapons)
            {
                AddToGrid(weapon.icon, weaponGrid, weapon.itemName, ItemType.weapon);
            }

            foreach (var item in inventory.consumables)
            {
                AddToGrid(item.icon, itemGrid, item.itemName, ItemType.item);
            }

            foreach (var spell in inventory.spells)
            {
                AddToGrid(spell.icon, spellGrid, spell.itemName, ItemType.spell);
            }
        }

        /// <summary>
        /// 清空网格内容
        /// </summary>
        private void ClearGrid(Transform grid)
        {
            foreach (Transform child in grid)
            {
                Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// 添加物品到指定网格
        /// </summary>
        private void AddToGrid(Sprite icon, Transform grid, string itemName, ItemType itemType)
        {
            GameObject slot = Instantiate(slotPrefab, grid); // 实例化槽
            slot.GetComponent<Image>().sprite = icon; // 设置图标
            DraggableItem draggable = slot.AddComponent<DraggableItem>(); // 添加拖拽功能
            draggable.
                Setup(itemName, itemType); // 配置拖拽数据
        }
        
        
        
        public static InventoryUI instance;

        void Awake()
        {
            instance = this;
        }
    }
}