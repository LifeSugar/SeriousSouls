using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace rei
{
    public class EquipSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public PlayerState playerState;
        public ItemType slotType; // 当前装备槽的类型
        public bool isLeft; // 是否是左手装备槽
        public int index; // 装备槽索引
        public Image indexMarker;
        public Image icon; // 图标显示的 Image 组件
        public Sprite defaultIcon; // 没有装备时显示的默认灰色图标
        public Outline outline; // 高亮边框的 Outline 组件

        private Color defaultOutlineColor; // 默认的边框颜色
        private Color highlightColor = new Color(0.5f, 0.6f, 1f, 1f); // 灰蓝色（高亮时的颜色）

        private void Start()
        {
            playerState = GameObject.Find("PlayerController").GetComponent<PlayerState>();
            if (outline != null)
            {
                defaultOutlineColor = outline.effectColor; // 保存初始边框颜色
            }

            UpdateSlotIcon(); // 初始化时更新图标
        }

        /// <summary>
        /// 当物品被拖放到该槽时触发
        /// </summary>
        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log($"Pointer Enter: {eventData.pointerEnter?.name}");
            Debug.Log("OnDrop");
            DraggableItem draggedItem = eventData.pointerDrag.GetComponent<DraggableItem>();
            if (draggedItem != null && draggedItem.itemType == slotType)
            {
                EquipItem(draggedItem.itemName, draggedItem.itemType);
                Destroy(eventData.pointerDrag); // 移除拖拽图标
                UpdateSlotIcon(); // 拖拽后更新图标
            }
        }

        /// <summary>
        /// 装备物品到指定槽
        /// </summary>
        private void EquipItem(string itemName, ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.weapon:
                {
                    // 获取当前槽位上的武器
                    var runtimeWeaponList = isLeft
                        ? playerState.inventoryManager.runtime_l_weapons
                        : playerState.inventoryManager.runtime_r_weapons;

                    if (index >= 0 && index < runtimeWeaponList.Count && runtimeWeaponList[index] != null)
                    {
                        // 如果槽位已装备武器，将其放回库存
                        var currentWeapon = runtimeWeaponList[index].instance;
                        playerState.inventoryManager.AddWeaponToInventory(currentWeapon);
                    }

                    // 装备新武器
                    playerState.inventoryManager.EquipWeaponUI(itemName, isLeft, index);
                    playerState.inventoryManager.RemoveWeaponFromInventory(itemName);
                }
                    break;

                case ItemType.item:
                {
                    var runtimeItemList = playerState.inventoryManager.runtime_consumables;

                    if (index >= 0 && index < runtimeItemList.Count && runtimeItemList[index] != null)
                    {
                        var currentItem = runtimeItemList[index].instance;
                        if (itemName != currentItem.itemName)// 如果槽位已装备别的消耗品，将其放回库存
                        {
                            var currentItems = new List<Consumable>();
                            for (int i = 0; i < runtimeItemList[index].itemCount; i++)
                            {
                                currentItems.Add(runtimeItemList[index].instance);
                            }
                            playerState.inventoryManager.AddItemToInventory(currentItems);
                            playerState.inventoryManager.EquipConsumableUI(itemName, index);
                            playerState.inventoryManager.RemoveItemFromInventory(itemName);
                            break;
                        }
                        else
                        {
                            int adds = playerState.inventoryManager.inventory.consumables.Find(c => c[0].itemName == itemName).Count;
                            playerState.inventoryManager.RemoveItemFromInventory(itemName);
                            runtimeItemList[index].itemCount += adds;
                            break;
                        }
                    }

                    // 装备新消耗品
                    playerState.inventoryManager.EquipConsumableUI(itemName, index);
                    playerState.inventoryManager.RemoveItemFromInventory(itemName);
                }
                    break;

                case ItemType.spell:
                {
                    var runtimeSpellList = playerState.inventoryManager.runtime_spells;

                    if (index >= 0 && index < runtimeSpellList.Count && runtimeSpellList[index] != null)
                    {
                        // 如果槽位已装备法术，将其放回库存
                        var currentSpell = runtimeSpellList[index].instance;
                        playerState.inventoryManager.AddSpellToInventory(currentSpell);
                    }

                    // 装备新法术
                    playerState.inventoryManager.EquipSpellUI(itemName, index);
                    playerState.inventoryManager.RemoveSpellFromInventory(itemName);
                }
                    break;
            }

            // 更新槽位图标
            UpdateSlotIcon();

            // 更新库存 UI
            InventoryUI.instance.UpdateInventoryUI(playerState.inventoryManager.inventory);
        }

        /// <summary>
        /// 更新装备槽的图标
        /// </summary>
        public void UpdateSlotIcon()
        {
            if (playerState == null || playerState.inventoryManager == null) return;

            switch (slotType)
            {
                case ItemType.weapon:
                    var weaponList = isLeft
                        ? playerState.inventoryManager.runtime_l_weapons
                        : playerState.inventoryManager.runtime_r_weapons;

                    if (index >= 0 && index < weaponList.Count && weaponList[index] != null &&
                        weaponList[index].instance != null)
                    {
                        icon.sprite = weaponList[index].instance.icon; // 更新图标
                        icon.enabled = true; // 确保图标可见
                    }
                    else
                    {
                        icon.sprite = defaultIcon; // 设置为默认灰色图标
                        icon.enabled = true;
                    }

                    break;

                case ItemType.item:
                    var itemList = playerState.inventoryManager.runtime_consumables;
                    if (index >= 0 && index < itemList.Count && itemList[index] != null &&
                        itemList[index].instance != null)
                    {
                        icon.sprite = itemList[index].instance.icon; // 更新图标
                        icon.enabled = true;
                    }
                    else
                    {
                        icon.sprite = defaultIcon; // 设置为默认灰色图标
                        icon.enabled = true;
                    }

                    break;

                case ItemType.spell:
                    var spellList = playerState.inventoryManager.runtime_spells;
                    if (index >= 0 && index < spellList.Count && spellList[index] != null &&
                        spellList[index].instance != null)
                    {
                        icon.sprite = spellList[index].instance.icon; // 更新图标
                        icon.enabled = true;
                    }
                    else
                    {
                        icon.sprite = defaultIcon; // 设置为默认灰色图标
                        icon.enabled = true;
                    }

                    break;
            }
        }

        public void UpdateMarker()
        {
            switch (slotType)
            {
                case (ItemType.item):
                    indexMarker.enabled = playerState.inventoryManager.consumable_idx == index;
                    break;
                case (ItemType.spell):
                    indexMarker.enabled = playerState.inventoryManager.sp_idx == index;
                    break;
                case (ItemType.weapon):
                    indexMarker.enabled =
                        isLeft
                            ? playerState.inventoryManager.l_idx == index
                            : playerState.inventoryManager.r_idx == index;
                    break;
                default:
                    indexMarker.enabled = false;
                    break;
            }
        }

        private void Update()
        {
            UpdateMarker();
        }

        /// <summary>
        /// 当鼠标悬停在槽上时触发
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (outline != null)
            {
                outline.effectColor = highlightColor; // 设置高亮颜色
            }
        }

        /// <summary>
        /// 当鼠标离开槽时触发
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (outline != null)
            {
                outline.effectColor = defaultOutlineColor; // 恢复默认颜色
            }

            // UpdateSlotIcon();
        }
    }
}