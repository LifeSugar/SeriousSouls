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
        public void Tick()
        {
            // 如果还未达到 frameCheck 帧，增加计数器并退出方法
            if (frameCount < frameCheck)
            {
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
                if (distance < 1.2f)
                {
                    itemCandidate = pick_items[i];
                }
                else
                {
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
                else
                {
                    // 如果交互对象超出交互范围，并且当前 interactionCandidate 是该对象，将 interactionCandidate 设为 null
                    if (interactionCandidate == interactions[i])
                        interactionCandidate = null;
                }
            }
        }

        // 拾取当前 itemCandidate 的方法
        public void PickCandidate(PlayerState playerStates)
        {
            // 如果没有 itemCandidate，直接返回
            if (itemCandidate == null)
                return;

            // 遍历 itemCandidate 中包含的所有物品
            foreach (var item in itemCandidate.items)
            {
                // 获取物品容器中的每一个物品并调用 AddItem 添加至玩家库存
                PickItemContainer c = item;
                AddItem(c.itemId, c.itemType, c.count, playerStates);
            }

            // 从 pick_items 列表中移除 itemCandidate
            if (pick_items.Contains(itemCandidate))
                pick_items.Remove(itemCandidate);

            // 销毁场景中的 itemCandidate，并将 itemCandidate 设为 null
            Destroy(itemCandidate.gameObject);
            itemCandidate = null;
        }

        // 根据物品ID、类型和状态管理器添加物品到玩家库存
        /// <summary>
        /// 将拾取的物品添加到玩家的库存中，并在 UI 中显示物品卡片。
        /// 根据物品类型（武器、消耗品、法术）分别处理逻辑，避免重复添加相同物品。
        /// </summary>
        /// <param name="id">物品的唯一标识符。</param>
        /// <param name="type">物品的类型（武器、消耗品、法术）。</param>
        /// <param name="playerStates">当前玩家状态对象，包含库存管理器。</param>
        public void AddItem(string id, ItemType type, int count, PlayerState playerStates)
        {
            
            // 1. 获取玩家的库存对象
            Inventory inventory = playerStates.inventoryManager.inventory;

            // 2. 根据物品类型分别处理
            switch (type)
            {
                // 处理武器类型的物品
                case ItemType.weapon:
                    // 从资源管理器中获取武器对象
                    Weapon weapon = ResourceManager.instance.GetWeapon(id);

                    // 检查库存中是否已经存在相同名称的武器
                    // if (!inventory.weapons.Exists(w => w.itemName == weapon.itemName))
                    // {
                    //     // 如果不存在，则添加到武器列表中
                    //     inventory.weapons.Add(weapon);
                    //
                    //     // 在 UI 中显示新增物品卡片
                    //     UIManager.instance.AddItemCard(weapon);
                    // }
                    
                    inventory.weapons.Add(weapon);

                    // 在 UI 中显示新增物品卡片
                    UIManager.instance.AddItemCard(weapon, count);
                    break;

                // 处理消耗品类型的物品
                case ItemType.item:
                    // 从资源管理器中获取消耗品对象
                    Consumable item = ResourceManager.instance.GetConsumable(id);

                    // 检查库存中是否已经存在相同名称的消耗品
                    if (!inventory.consumables.Exists(c => c[0].itemName == item.itemName))
                    {
                        // 如果不存在，则添加到消耗品列表中
                        List<Consumable> items = new List<Consumable>();
                        for (int i = 0; i < count; i++)
                            items.Add(item);
                        inventory.consumables.Add(items);

                        // 在 UI 中显示新增物品卡片
                        UIManager.instance.AddItemCard(item, count);
                    }
                    else
                    {
                        for (int i = 0; i < count; i++)
                            inventory.consumables.Find(c => c[0].itemName == item.itemName).Add(item);
                        UIManager.instance.AddItemCard(item, count);
                    }
                    break;

                // 处理法术类型的物品
                case ItemType.spell:
                    // 从资源管理器中获取法术对象
                    Spell spell = ResourceManager.instance.GetSpell(id);

                    // 检查库存中是否已经存在相同名称的法术
                    if (!inventory.spells.Exists(s => s.itemName == spell.itemName))
                    {
                        // 如果不存在，则添加到法术列表中
                        inventory.spells.Add(spell);

                        // 在 UI 中显示新增物品卡片
                        UIManager.instance.AddItemCard(spell, count);
                    }
                    break;
                case ItemType.key:
                    Key key = ResourceManager.instance.GetKey(id);
                    inventory.keys.Add(key);
                    UIManager.instance.AddItemCard(key, count);
                    break;
            }
            
            //更新库存UI
            InventoryUI.instance.UpdateInventoryUI(inventory);
        }

        public static PickableItemsManager instance;

        void Awake()
        {
            instance = this;
        }
    }
}