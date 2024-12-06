using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rei
{
    public class InventoryManager : MonoBehaviour
    {
        [Header("Inventory")]
        public Inventory inventory;
        
        [Header("Runtime")]

        public int r_idx;
        public int l_idx;
        public int sp_idx;
        public int consumable_idx;

        public List<RuntimeWeapon> runtime_r_weapons = new List<RuntimeWeapon>();
        public List<RuntimeWeapon> runtime_l_weapons = new List<RuntimeWeapon>();
        public List<RuntimeSpell> runtime_spells = new List<RuntimeSpell>();
        public List<RuntimeConsumable> runtime_consumables = new List<RuntimeConsumable>();

        public RuntimeConsumable curConsumable;
        public RuntimeSpell currentSpell;
        public RuntimeWeapon rightHandWeapon;
        public RuntimeWeapon leftHandWeapon;
        public bool hasLeftHandWeapon = false;
        public bool hasRightHandWeapon = false;


        [Header("Colliders")]
        public GameObject parryCollider;
        public GameObject breathCollider;
        public GameObject blockCollider;

        PlayerState _playerStates;

        public void Init(PlayerState st)
        {
            _playerStates = st;
            UI.QuickSlot.instance.Init();
        }

        public void LoadInventory()
        {
        }

        public Weapon GetCurrentWeapon(bool isLeftHand)
        {
            if (isLeftHand)
                return leftHandWeapon.instance;
            else
                return rightHandWeapon.instance;
        }

        public void ChangeToNextWeapon(bool isLeft)
        {
            // 如果是左手武器
            if (isLeft)
            {
                // 检查是否已到达左手武器列表的末尾
                if (l_idx < runtime_l_weapons.Count - 1)
                    l_idx++; // 如果没有，增加左手武器索引，指向下一个武器
                else
                    l_idx = 0; // 如果达到列表末尾，循环回到第一个武器

                // 调用 EquipWeapon 方法，装备左手的当前武器
                if (runtime_l_weapons[l_idx] != null)
                    EquipWeapon(runtime_l_weapons[l_idx], true);
                else
                { 
                    _playerStates.anim.SetBool("mirror", isLeft);
                    _playerStates.anim.Play("change weapon");
                    // var currentWeapon = gameObject.GetComponentInChildren<WeaponHook>()
                    if (leftHandWeapon) leftHandWeapon.weaponModel.SetActive(false);
                    hasLeftHandWeapon = false;
                    leftHandWeapon = null;
                    // 更新UI快捷槽中的图标
                    UI.QuickSlot uiSlot = UI.QuickSlot.instance;
                    uiSlot.UpdateSlot((isLeft) ? UI.QSlotType.lh : UI.QSlotType.rh,null);
                }
                    
            }
            else
            {
                // 如果是右手武器，检查是否已到达右手武器列表的末尾
                if (r_idx < runtime_r_weapons.Count - 1)
                    r_idx++; // 增加右手武器索引
                else
                    r_idx = 0; // 循环回到第一个武器

                // 调用 EquipWeapon 方法，装备右手的当前武器
                if (runtime_r_weapons[r_idx] != null)
                    EquipWeapon(runtime_r_weapons[r_idx], false);
                else
                {
                    _playerStates.anim.SetBool("mirror", isLeft);
                    _playerStates.anim.Play("change weapon");
                    // Transform p = _playerStates.anim.GetBoneTransform(HumanBodyBones.RightHand);
                    // var currentWeapon = p.GetComponentInChildren<WeaponHook>();
                    // if (currentWeapon)
                    //     currentWeapon.gameObject.SetActive(false);
                    if (rightHandWeapon) rightHandWeapon.weaponModel.SetActive(false);
                    hasRightHandWeapon = false;
                    rightHandWeapon = null;
                    // 更新UI快捷槽中的图标
                    UI.QuickSlot uiSlot = UI.QuickSlot.instance;
                    uiSlot.UpdateSlot((isLeft) ? UI.QSlotType.lh : UI.QSlotType.rh, null);
                }
                    
            }

            // 更新动作管理器中的单手武器动作
            _playerStates.actionManager.UpdateActionsOneHanded();
        }

        //用于将指定的 RuntimeWeapon 武器实例装备到角色的左手或右手。该方法处理了旧武器的隐藏、新武器的显示、动画状态的设置，以及更新 UI 中快捷槽的图标
        public void EquipWeapon(RuntimeWeapon w, bool isLeft = false)
        {
            // 如果是左手武器
            if (isLeft)
            {
                // 如果左手已装备武器，将其隐藏
                if (leftHandWeapon != null)
                {
                    leftHandWeapon.weaponModel.SetActive(false);
                }

                // 设置左手武器为当前装备的武器
                leftHandWeapon = w;
            }
            else
            {
                // 如果右手已装备武器，将其隐藏
                if (rightHandWeapon != null)
                {
                    rightHandWeapon.weaponModel.SetActive(false);
                }

                // 设置右手武器为当前装备的武器
                rightHandWeapon = w;
            }

            // 获取对应的idle动画名称，根据是否为左手添加后缀 "_l" 或 "_r"
            string targetIdle = w.instance.oh_idle;
            targetIdle += (isLeft) ? "_l" : "_r";

            // 如果是左手武器，设置动作镜像
            if (isLeft == true)
            {
                for (int i = 0; i < leftHandWeapon.instance.actions.Count; i++)
                {
                    leftHandWeapon.instance.actions[i].mirror = true; // 将每个动作设置为镜像
                }
            }

            // 设置动画控制器中的 "mirror" 参数，以适应左手或右手武器的动画
            _playerStates.anim.SetBool("mirror", isLeft);
            // 播放换武器动画，然后切换到目标idle动画
            _playerStates.anim.Play("change weapon");
            _playerStates.anim.Play(targetIdle);

            // 更新UI快捷槽中的图标
            UI.QuickSlot uiSlot = UI.QuickSlot.instance;
            uiSlot.UpdateSlot((isLeft) ? UI.QSlotType.lh : UI.QSlotType.rh, w.instance.icon);

            // 激活新武器的模型，使其在游戏中可见
            w.weaponModel.SetActive(true);
        }


        //将传入的 Weapon 对象转换为 RuntimeWeapon 对象。RuntimeWeapon 是武器在运行时的实例对象，包含实际的武器模型、碰撞器和与动画的绑定等。
        public RuntimeWeapon WeaponToRuntimeWeapon(Weapon w, int index, bool isLeftHand = false)
        {
            // 创建一个新的空游戏对象，用于承载 RuntimeWeapon 组件
            GameObject g0 = new GameObject();
            g0.AddComponent<RuntimeWeapon>(); // 添加 RuntimeWeapon 组件
            RuntimeWeapon ist = g0.GetComponent<RuntimeWeapon>(); // 获取并存储 RuntimeWeapon 组件

            // 复制 Weapon 对象的属性到 RuntimeWeapon 的 instance 属性中
            ist.instance = new Weapon();
            GlobalFuntions.DeepCopyWeapon(w, ist.instance); // 深拷贝 Weapon 的数据到 ist.instance
            g0.name = w.itemName; // 设置对象名称为武器的名称

            // 实例化武器模型，并将其设置为手部的子对象
            ist.weaponModel = Instantiate(ist.instance.modelPrefab);
            Transform p =
                _playerStates.anim.GetBoneTransform((isLeftHand)
                    ? HumanBodyBones.LeftHand
                    : HumanBodyBones.RightHand); // 获取手部骨骼的 Transform
            ist.weaponModel.transform.parent = p; // 设置模型的父对象为手部骨骼

            // 设置模型的位置、旋转和缩放
            ist.weaponModel.transform.localPosition =
                (isLeftHand) ? ist.instance.l_model_pos : ist.instance.r_model_pos;
            ist.weaponModel.transform.localEulerAngles =
                (isLeftHand) ? ist.instance.l_model_eulers : ist.instance.r_model_eulers;
            ist.weaponModel.transform.localScale = ist.instance.model_scale;

            // 获取武器模型中的 WeaponHook 组件，用于控制武器的碰撞器
            ist.w_hook = ist.weaponModel.GetComponentInChildren<WeaponHook>();
            ist.w_hook.InitDamageColliders(_playerStates); // 初始化碰撞器

            // 根据武器是否是左手装备，将 RuntimeWeapon 添加到相应的列表中
            if (isLeftHand)
                runtime_l_weapons[index] = ist; // 添加到左手武器列表
            else
                runtime_r_weapons[index] = ist; // 添加到右手武器列表

            // 初始状态下将武器模型设为不可见，交由 EquipWeapon 方法处理显示和隐藏
            ist.weaponModel.SetActive(false);

            return ist; // 返回生成的 RuntimeWeapon 实例
        }

        public void ChangeToNextSpell()
        {
            if (sp_idx < runtime_l_weapons.Count - 1)
                sp_idx++;
            else
                sp_idx = 0;
            EquipSpells(runtime_spells[sp_idx]);
        }

        //将传入的 RuntimeSpell 实例设置为当前装备的法术，同时更新 UI 中的快捷槽图标
        public void EquipSpells(RuntimeSpell spell)
        {
            // 将传入的法术实例设置为当前装备的法术
            currentSpell = spell;

            // 获取 UI 快捷槽单例
            UI.QuickSlot uiSlot = UI.QuickSlot.instance;

            // 更新快捷槽的图标，UI.QSlotType.spell 表示法术槽
            uiSlot.UpdateSlot(UI.QSlotType.spell, spell.instance.icon);
        }

        //将一个 Spell 对象（静态数据）转换为 RuntimeSpell 对象（运行时实例）
        public RuntimeSpell SpellToRuntimeSpell(Spell s, bool isLeft = false)
        {
            // 创建一个新的空 GameObject，作为运行时法术实例的承载对象
            GameObject g0 = new GameObject();

            // 添加 RuntimeSpell 组件到 GameObject
            RuntimeSpell inst = g0.AddComponent<RuntimeSpell>();

            // 创建一个新的 Spell 实例，并将静态 Spell 对象的数据复制到该实例
            inst.instance = new Spell();
            GlobalFuntions.DeepCopySpell(s, inst.instance); // 使用 DeepCopySpell 方法复制数据
            g0.name = s.itemName; // 设置 GameObject 的名称为法术的名称

            // 将生成的 RuntimeSpell 对象添加到运行时的法术列表中
            runtime_spells.Add(inst);

            // 返回生成的 RuntimeSpell 实例
            return inst;
        }

        //用于创建一个法术的粒子效果，并将其附加到指定的位置
        public void CreateSpellParticle(RuntimeSpell inst, bool isLeft, bool parentUnderRoot = false)
        {
            // 检查当前法术实例是否已有粒子效果，如果没有则实例化一个新的
            if (inst.currentParticle == null)
            {
                // 实例化粒子效果预制体并赋值给 currentParticle
                inst.currentParticle = Instantiate(inst.instance.particle_prefab) as GameObject;

                // 获取粒子效果中的 ParticleHook 组件，用于控制粒子的行为
                inst.p_hook = inst.currentParticle.GetComponentInChildren<ParticleHook>();

                // 初始化
                inst.p_hook.Init();
            }

            // 如果不挂载在角色根部
            if (!parentUnderRoot)
            {
                // 获取手部骨骼的 Transform，根据 isLeft 参数判断是左手还是右手
                Transform p =
                    _playerStates.anim.GetBoneTransform((isLeft) ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);

                // 将粒子效果挂载在手部骨骼上
                inst.currentParticle.transform.parent = p;

                // 设置粒子效果的本地旋转为默认值（无旋转）
                inst.currentParticle.transform.localRotation = Quaternion.identity;

                // 设置粒子效果的位置为手部骨骼的中心
                inst.currentParticle.transform.localPosition = Vector3.zero;
            }
            else
            {
                // 将粒子效果挂载在角色的根 Transform 上
                inst.currentParticle.transform.parent = transform;

                // 设置粒子效果的本地旋转为默认值（无旋转）
                inst.currentParticle.transform.localRotation = Quaternion.identity;

                // 设置粒子效果的位置，使其位于角色根位置上方
                inst.currentParticle.transform.localPosition = new Vector3(0, 1.5f, 0.65f);
            }
            // inst.currentParticle.SetActive(false);
        }


        //以下三个方法用于处理消耗品的转换，装备和切换（果粒橙和脉动）

        //将消耗品（静态对象）转换为正在装备的消耗品（运行实例）
        public RuntimeConsumable ConsumableToRuntimeConsumable(List<Consumable> c, int index)
        {
            // 创建一个新的空 GameObject 作为消耗品的运行时实例
            GameObject g0 = new GameObject();
            RuntimeConsumable inst = g0.AddComponent<RuntimeConsumable>(); // 添加 RuntimeConsumable 组件
            g0.name = c[0].itemName; // 设置 GameObject 的名称为消耗品名称

            // 创建一个新的 Consumable 实例，并将原始数据完全复制到 inst.instance
            inst.instance = new Consumable();
            GlobalFuntions.DeepCopyConsumable(inst.instance, c[0]); // 使用 DeepCopyConsumable 方法复制数据

            // 如果消耗品有对应的模型预制体，则实例化该模型
            if (inst.instance.itemPrefab != null)
            {
                GameObject model = Instantiate(inst.instance.itemPrefab) as GameObject; // 实例化模型
                Transform p = _playerStates.anim.GetBoneTransform(HumanBodyBones.RightHand); // 获取角色右手的 Transform
                model.transform.parent = p; // 将模型作为右手的子对象

                // 设置模型的位置、旋转和缩放
                model.transform.localPosition = inst.instance.r_model_pos;
                model.transform.localEulerAngles = inst.instance.r_model_eulers;

                Vector3 targetScale = inst.instance.model_scale;
                if (targetScale == Vector3.zero) // 如果缩放未设置，默认为 Vector3.one
                    targetScale = Vector3.one;
                model.transform.localScale = targetScale;

                inst.itemModel = model; // 将模型赋值给 RuntimeConsumable 的 itemModel 属性
                inst.itemModel.SetActive(false); // 初始状态下隐藏模型
                inst.itemCount = c.Count;
            }

            runtime_consumables[index] = inst; // 将生成的 RuntimeConsumable 添加到运行时消耗品列表中
            return inst; // 返回生成的 RuntimeConsumable 实例
        }

        //装备
        public void EquipConsumable(RuntimeConsumable consum)
        {
            curConsumable = consum; // 将当前装备的消耗品设置为传入的 RuntimeConsumable 实例

            UI.QuickSlot uiSlot = UI.QuickSlot.instance; // 获取 UI 快捷槽单例
            uiSlot.UpdateSlot(UI.QSlotType.item, consum.instance.icon); // 更新快捷槽的图标为消耗品的图标
        }

        //切换
        public void ChangeToNextConsumable()
        {
            
            // 检查当前索引是否小于列表的最后一个索引
            if (consumable_idx < runtime_consumables.Count - 1)
            {
                consumable_idx++; // 如果是，增加索引，指向下一个消耗品
                if (runtime_consumables[consumable_idx] == null)
                {
                    if (consumable_idx < runtime_consumables.Count - 1)
                        consumable_idx++;
                    else
                    {
                        consumable_idx = 0;
                    }
                }
            }
            else
                consumable_idx = 0; // 如果达到列表末尾，循环回到第一个消耗品

            // 装备新的消耗品
            if (runtime_consumables[consumable_idx] != null)
                EquipConsumable(runtime_consumables[consumable_idx]);
        }

        public void OpenAllDamageColliders()
        {
            // 如果右手武器的 WeaponHook 存在，打开其伤害碰撞器
            if (rightHandWeapon != null && rightHandWeapon.w_hook != null)
                rightHandWeapon.w_hook.OpenDamageColliders();

            // 如果左手装备了武器且 WeaponHook 存在，打开左手武器的伤害碰撞器
            if (hasLeftHandWeapon)
            {
                if (leftHandWeapon.w_hook != null)
                    leftHandWeapon.w_hook.OpenDamageColliders();
            }
        }

        public void CloseAllDamageColliders()
        {
            // 如果右手武器的 WeaponHook 存在，关闭其伤害碰撞器
            if (rightHandWeapon != null && rightHandWeapon.w_hook != null)
                rightHandWeapon.w_hook.CloseDamageColliders();

            // 如果左手装备了武器且 WeaponHook 存在，关闭左手武器的伤害碰撞器
            if (hasLeftHandWeapon)
            {
                if (leftHandWeapon.w_hook != null)
                    leftHandWeapon.w_hook.CloseDamageColliders();
            }
        }

        public void InitAllDamageColliders(PlayerState playerStates)
        {
            // 初始化右手武器的碰撞器，将 StateManager 传入以提供角色的状态信息
            if (rightHandWeapon.w_hook != null)
                rightHandWeapon.w_hook.InitDamageColliders(playerStates);

            // 如果左手装备了武器，初始化左手武器的碰撞器
            if (hasLeftHandWeapon)
            {
                if (leftHandWeapon.w_hook != null)
                    leftHandWeapon.w_hook.InitDamageColliders(playerStates);
            }
        }


        //以下三加三个方法在UI中被调用
        public void EquipWeaponUI(string itemName, bool isLeftHand, int index)
        {
            Weapon weapon = inventory.weapons.Find(w => w.itemName == itemName);
            if (weapon != null)
            {
                WeaponToRuntimeWeapon(weapon, index, isLeftHand); // 调用现有逻辑
                if (!isLeftHand)
                {
                    if (r_idx == index)
                        EquipWeapon(runtime_r_weapons[r_idx], isLeftHand);
                    // 更新动作管理器中的单手武器动作
                    _playerStates.actionManager.UpdateActionsOneHanded();
                }
                else
                {
                    if (l_idx == index)
                        EquipWeapon(runtime_l_weapons[l_idx], isLeftHand);
                    // 更新动作管理器中的单手武器动作
                    _playerStates.actionManager.UpdateActionsOneHanded();
                }
            }
        }
        
        public void RemoveWeaponFromInventory(string weaponName)
        {
            for (int i = 0; i < inventory.weapons.Count; i++)
            {
                if (inventory.weapons[i].itemName == weaponName)
                {
                    inventory.weapons.RemoveAt(i);
                    return;
                }
            }
        }
        public void EquipConsumableUI(string itemName, int index)
        {
            List<Consumable> items = inventory.consumables.Find(c => c[0].itemName == itemName);
            if (items != null)
            {
                ConsumableToRuntimeConsumable(items, index); // 调用现有逻辑

                if (consumable_idx == index)
                {
                    EquipConsumable(runtime_consumables[index]);
                }
            }
        }
        
        public void RemoveItemFromInventory(string itemName)
        {
            for (int i = 0; i < inventory.consumables.Count; i++)
            {
                if (inventory.consumables[i][0].itemName == itemName)
                {
                    inventory.consumables.RemoveAt(i);
                    return;
                }
            }
        }

        public void EquipSpellUI(string itemName, int index)
        {
            Spell spell = inventory.spells.Find(s => s.itemName == itemName);
            if (spell != null)
            {
                SpellToRuntimeSpell(spell); // 调用现有逻辑

                if (sp_idx == index)
                {
                    EquipSpells(runtime_spells[index]);
                }
            }
        }
        
        
        public void RemoveSpellFromInventory(string spellName)
        {
            for (int i = 0; i < inventory.spells.Count; i++)
            {
                if (inventory.spells[i].itemName == spellName)
                {
                    inventory.spells.RemoveAt(i);
                    return;
                }
            }
        }
        
        public void AddWeaponToInventory(Weapon weapon)
        {
            inventory.weapons.Add(weapon);
        }

        public void AddItemToInventory(List<Consumable> items)
        {
            inventory.consumables.Add(items);
        }

        public void AddSpellToInventory(Spell spell)
        {
            inventory.spells.Add(spell);
        }

        //招架

        // 打开招架碰撞器，使角色能够执行招架动作
        public void OpenParryCollider()
        {
            parryCollider.SetActive(true);
        }

        // 关闭招架碰撞器，停止招架判定
        public void CloseParryCollider()
        {
            parryCollider.SetActive(false);
        }

        //法术

        // 打开法术碰撞器，用于持续施法的碰撞判定
        public void OpenBreathCollider()
        {
            breathCollider.SetActive(true);
        }

        // 关闭呼吸法术碰撞器
        public void CloseBreathCollider()
        {
            breathCollider.SetActive(false);
        }

        // 打开阻挡碰撞器，用于防御
        public void OpenBlockCollider()
        {
            blockCollider.SetActive(true);
        }

        // 关闭阻挡碰撞器
        public void CloseBlockCollider()
        {
            blockCollider.SetActive(false);
        }

        // 发射法术粒子效果，播放一次法术粒子
        public void EmitSpellParticle()
        {
            currentSpell.p_hook.Emit(1);
        }
    }


    [System.Serializable]
    public class Weapon : Item
    {
        // 单手（one-handed）和双手（two-handed）模式的闲置动画名称
        public string oh_idle; // 单手模式的idle动画名称
        public string th_idle; // 双手模式的idle动画名称

        public ItemType itemType; // 武器类型，例如近战、远程等

        public List<Action> actions; // 单手模式下的动作列表
        public List<Action> two_handedActions; // 双手模式下的动作列表

        public float parryMultiplier; // 招架倍率，用于计算招架效果的强度
        public float backstabMultiplier; // 背刺倍率，用于计算背刺效果的强度
        public bool LeftHandMirror; // 是否镜像左手动作（左右手动作互换时）

        public GameObject modelPrefab; // 武器的模型预制体，用于实例化武器模型

        public WeaponStats weaponStats; // 武器的各项属性，例如攻击力、防御力等

        // 根据动作输入（ActionInput）获取对应的动作（Action）
        public Action GetAction(List<Action> l, ActionInput inp)
        {
            if (l == null)
            {
                Debug.Log("this weapon dont have actions");
                return null;
            }

            Debug.Log(l.Count);


            // 遍历动作列表，找到与输入匹配的动作
            for (int i = 0; i < l.Count; i++)
            {
                if (l[i].input == inp) // 如果动作的输入与指定输入匹配，返回该动作
                {
                    return l[i];
                }
            }

            return null; // 若未找到匹配的动作，返回null
        }

        // 武器在右手或左手的模型位置、旋转角度和缩放
        public Vector3 r_model_pos; // 右手模型位置偏移
        public Vector3 l_model_pos; // 左手模型位置偏移
        public Vector3 r_model_eulers; // 右手模型旋转角度
        public Vector3 l_model_eulers; // 左手模型旋转角度
        public Vector3 model_scale; // 武器模型的缩放比例
    }
    
    [System.Serializable]
    public class Inventory
    {
        public List<Weapon> weapons = new List<Weapon>();
        public List<List<Consumable>> consumables = new List<List<Consumable>>();
        public List<Spell> spells = new List<Spell>();
    }

    [System.Serializable]
    public class Spell : Item
    {
        public SpellType spellType;
        public SpellClass spellClass;
        public ItemType itemType;

        public List<SpellAction> actions = new List<SpellAction>();
        public GameObject projectile;
        public GameObject particle_prefab;
        public string spell_effect;

        public SpellAction GetAction(List<SpellAction> l, ActionInput inp)
        {
            if (l == null)
                return null;

            for (int i = 0; i < l.Count; i++)
            {
                if (l[i].input == inp)
                {
                    return l[i];
                }
            }

            return null;
        }
    }

    [System.Serializable]
    public class Consumable : Item
    {
        public string consumableEffect;
        public string audio_id;
        public string targetAnim;
        public ItemType itemType;

        public ConsumbleEffect Effect;

        public GameObject itemPrefab;
        public Vector3 r_model_pos;
        public Vector3 r_model_eulers;
        public Vector3 model_scale;
    }

    public delegate void ConsumbleEffect(PlayerState playerStates);

    public enum ItemType
    {
        weapon,
        item,
        spell
    }
}