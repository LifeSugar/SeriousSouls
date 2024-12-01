using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rei
{
    public class PickableItemsManager : MonoBehaviour
{
    // 用于存储场景中所有的可交互对象
    public List<WorldInteraction> interactions = new List<WorldInteraction>();
    // 用于存储场景中所有的可拾取物品
    public List<PickableItem> pick_items = new List<PickableItem>();
    // 当前可拾取的物品候选对象
    public PickableItem itemCandidate;
    // 当前可交互的候选对象
    public WorldInteraction interactionCandidate;

    // 用于控制帧更新的计数器
    int frameCount;
    // 每隔 frameCheck 帧检查一次物品和交互对象
    public int frameCheck = 15;

    // 每帧更新调用，检查物品和交互对象的距离
    public void Tick() {
        // 如果还未达到 frameCheck 帧，增加计数器并退出方法
        if (frameCount < frameCheck) {
            frameCount++;
            return;
        }
        // 重置计数器
        frameCount = 0;

        // 遍历所有可拾取物品，检查是否在拾取范围内
        for (int i = 0; i < pick_items.Count; i++)
        {
            // 计算玩家和物品之间的距离
            float distance = Vector3.Distance(pick_items[i].transform.position, transform.position);

            // 如果物品在拾取范围内（小于2个单位），将其设置为 itemCandidate
            if (distance < 2)
            {
                itemCandidate = pick_items[i];
            }
            else {
                // 如果物品超出拾取范围，并且当前 itemCandidate 是该物品，将 itemCandidate 设为 null
                if (itemCandidate == pick_items[i])
                    itemCandidate = null;
            }
        }

        // 遍历所有可交互对象，检查是否在交互范围内
        for (int i = 0; i < interactions.Count; i++)
        {
            // 计算玩家和交互对象之间的距离
            float d = Vector3.Distance(interactions[i].transform.position, transform.position);
            // 如果交互对象在范围内（小于2个单位），将其设置为 interactionCandidate
            if (d < 2)
            {
                interactionCandidate = interactions[i];
            }
            else {
                // 如果交互对象超出交互范围，并且当前 interactionCandidate 是该对象，将 interactionCandidate 设为 null
                if (interactionCandidate == interactions[i])
                    interactionCandidate = null;
            }
        }
    }

    // 拾取当前 itemCandidate 的方法
    public void PickCandidate(PlayerState playerStates) {
        // 如果没有 itemCandidate，直接返回
        if (itemCandidate == null)
            return;
        
        // 遍历 itemCandidate 中包含的所有物品
        for (int i = 0; i < itemCandidate.items.Length; i++)
        {
            // 获取物品容器中的每一个物品并调用 AddItem 添加至玩家库存
            PickItemContainer c = itemCandidate.items[i];
            AddItem(c.itemId, c.itemType, playerStates);
        }

        // 从 pick_items 列表中移除 itemCandidate
        if (pick_items.Contains(itemCandidate))
            pick_items.Remove(itemCandidate);

        // 销毁场景中的 itemCandidate，并将 itemCandidate 设为 null
        Destroy(itemCandidate.gameObject);
        itemCandidate = null;
    }

    // 根据物品ID、类型和状态管理器添加物品到玩家库存
    void AddItem(string id, ItemType type, PlayerState playerStates) {
        // 获取玩家的 InventoryManager
        InventoryManager inv = playerStates.inventoryManager;
        switch (type)
        {
            
            //这里逻辑再仔细想想，主要是没有真正的“库存”
            // 如果物品是武器类型
            case ItemType.weapon:
                // 检查玩家是否已经拥有该武器
                for (int k = 0; k < inv.runtime_r_weapons.Count; k++)
                {
                    if (id == inv.runtime_r_weapons[k].name) {
                        // 如果拥有该武器，则显示通知卡片并返回
                        Item b = ResourceManager.instance.GetItem(id);
                        UIManager.instance.AddItemCard(b);
                        return;
                    }
                }
                // 如果玩家未拥有该武器，将武器添加到玩家右手slot中
                Debug.Log("get" + id);
                inv.WeaponToRuntimeWeapon(ResourceManager.instance.GetWeapon(id));
                // inv.WeaponToRuntimeWeapon(ResourceManager.instance.GetWeapon(id), true);
                break;

            // 如果物品是消耗品类型
            case ItemType.item:
                // 检查玩家是否已经拥有该消耗品
                for (int j = 0; j < inv.runtime_consumables.Count; j++) {
                    if (id == inv.runtime_consumables[j].name)
                    {
                        // 如果拥有该消耗品，则增加数量并显示通知卡片
                        inv.runtime_consumables[j].itemCount++;
                        Item b = ResourceManager.instance.GetItem(id);
                        UIManager.instance.AddItemCard(b);
                        return;
                    }
                }
                // 如果玩家未拥有该消耗品，将消耗品添加到玩家库存中
                inv.ConsumableToRuntimeConsumable(ResourceManager.instance.GetConsumable(id));
                break;

            // 如果物品是法术类型
            case ItemType.spell:
                // 检查玩家是否已经拥有该法术
                for (int k = 0; k < inv.runtime_spells.Count; k++)
                {
                    if (id == inv.runtime_spells[k].name)
                    {
                        // 如果拥有该法术，则显示通知卡片并返回
                        Item b = ResourceManager.instance.GetItem(id);
                        UIManager.instance.AddItemCard(b);
                        return;
                    }
                }
                // 如果玩家未拥有该法术，将法术添加到玩家库存中
                inv.SpellToRuntimeSpell(ResourceManager.instance.GetSpell(id));
                break;
        }

        // 显示拾取的物品通知卡片
        Item i = ResourceManager.instance.GetItem(id);
        UIManager.instance.AddItemCard(i);
    }
}
}