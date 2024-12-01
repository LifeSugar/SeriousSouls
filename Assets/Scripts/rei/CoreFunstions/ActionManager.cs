using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rei
{
    public class ActionManager : MonoBehaviour
    {
        public Weapon unarmedAction;
        // 当前动作索引，用于跟踪动作序列中的位置
        public int actionIndex;

        // 存储可用动作槽的列表，每个槽代表一个动作（如攻击、格挡等）
        public List<Action> actionSlots = new List<Action>();

        // 引用 StateManager，用于管理玩家的状态（如健康、耐力）
        PlayerState _playerStates;

        // 构造函数：通过枚举值为每个动作槽赋值初始动作，设置输入类型
        ActionManager()
        {
            for (int i = 0; i < 4; i++)
            {
                Action a = new Action();
                a.input = (ActionInput)i;
                actionSlots.Add(a); //四个输入动作载入Slot
            }
        }

        // 初始化函数，设置 ActionManager 的 StateManager 引用
        public void Init(PlayerState st)
        {
            _playerStates = st;
            // 初始化单手武器的动作，仅在游戏开始时更新动作
            unarmedAction = ResourceManager.instance.GetWeapon("Unarmed");
            UpdateActionsOneHanded();
            
            
        }

        // 清空所有动作槽，将动作槽属性重置为默认值
        void EmptyAllSlots()
        {
            for (int i = 0; i < 4; i++)
            {
                Action a = GlobalFuntions.GetAction((ActionInput)i, actionSlots);
                a.targetAnim = null;
                a.audio_ids = null;
                a.steps = null;
                a.mirror = false;
                a.type = ActionType.attack;
                a.canBeParried = true;
                a.changeSpeed = true;
                a.animSpeed = 1;
                a.canBackStab = false;
            }
        }

        // 更新单手武器的动作设置
        public void UpdateActionsOneHanded()
        {
            EmptyAllSlots(); // 清空所有现有动作

            if (_playerStates.inventoryManager.rightHandWeapon != null)
                _playerStates.inventoryManager.hasRightHandWeapon = true;
            else
                _playerStates.inventoryManager.hasRightHandWeapon = false;

            if (_playerStates.inventoryManager.leftHandWeapon != null)
                _playerStates.inventoryManager.hasLeftHandWeapon = true;
            else
                _playerStates.inventoryManager.hasLeftHandWeapon = false;

             
           
            
            //如果装备了右手武器，则将右手武器动作映射rb，rt，否则映射空手rb，rt
             if (_playerStates.inventoryManager.hasRightHandWeapon)
             {
                 GlobalFuntions.DeepCopyAction(_playerStates.inventoryManager.rightHandWeapon.instance, ActionInput.rb,
                     ActionInput.rb, actionSlots);
                 GlobalFuntions.DeepCopyAction(_playerStates.inventoryManager.rightHandWeapon.instance, ActionInput.rt,
                     ActionInput.rt, actionSlots);
                 GlobalFuntions.DeepCopyAction(_playerStates.inventoryManager.rightHandWeapon.instance, ActionInput.lt, ActionInput.lt,actionSlots);
             }
             else
             {
                 Debug.Log("righthand unarmed");
                 GlobalFuntions.DeepCopyAction(unarmedAction, ActionInput.rb, ActionInput.rb, actionSlots);
                 GlobalFuntions.DeepCopyAction(unarmedAction, ActionInput.rt, ActionInput.rt, actionSlots);
             }
             // 如果装备的是左手武器，则为左手输入映射相应动作；否则，将空手动作映射到左手输入
             if (_playerStates.inventoryManager.hasLeftHandWeapon)
             {
                 GlobalFuntions.DeepCopyAction(_playerStates.inventoryManager.leftHandWeapon.instance, ActionInput.rb,
                     ActionInput.lb, actionSlots, true);
                 // GlobalFuntions.DeepCopyAction(_playerStates.inventoryManager.leftHandWeapon.instance, ActionInput.rt,
                 //     ActionInput.lt, actionSlots, true);
             }
             else
             {
                 GlobalFuntions.DeepCopyAction(unarmedAction, ActionInput.rb, ActionInput.lb, actionSlots, true);
                 // GlobalFuntions.DeepCopyAction(unarmedAction, ActionInput.rt, ActionInput.lt, actionSlots, true);
             }
            

            
        }

        // 更新双手武器的动作设置，清空槽并分配双手动作
        public void UpdateActionsTwoHanded()
        {
            EmptyAllSlots(); // 清空所有现有动作
            Weapon w = _playerStates.inventoryManager.rightHandWeapon.instance;

            // 将右手武器的双手动作分配给动作槽
            for (int i = 0; i < w.two_handedActions.Count; i++)
            {
                Action a = GlobalFuntions.GetAction(w.two_handedActions[i].input, actionSlots);
                a.steps = w.two_handedActions[i].steps;
                a.type = w.two_handedActions[i].type;
            }
        }

        // 根据 StateManager 判断当前按下的输入（rb, lb, rt, lt）
        public ActionInput GetActionInput(PlayerState st)
        {
            if (st.rb)
            {
                // Debug.Log("RB pressed");
                return ActionInput.rb;
            }
                
            if (st.lb)
                return ActionInput.lb;
            if (st.rt)
                return ActionInput.rt;
            if (st.lt)
                return ActionInput.lt;

            return ActionInput.rb; // 如果未检测到输入，默认返回 rb
        }

        // 返回与 StateManager 中检测到的输入相对应的当前动作槽
        public Action GetActionSlot(PlayerState st)
        {
            ActionInput a_input = GetActionInput(st);
            return GlobalFuntions.GetAction(a_input, actionSlots); // 根据输入获取动作
        }

        // 根据给定的 ActionInput 从动作槽中检索特定动作
        public Action GetActionFromInput(ActionInput a_input)
        {
            return GlobalFuntions.GetAction(a_input, actionSlots);
        }
    }


    // 定义按键输入类型，用于触发不同的动作
    public enum ActionInput
    {
        rb, // 右手攻击按钮
        lb, // 左手攻击按钮
        rt, // 右手扳机
        lt // 左手扳机，战技
    }

// 定义动作类型，表示角色可以执行的不同动作
    public enum ActionType
    {
        attack, // 攻击动作
        block, // 格挡动作
        spells, // 施法动作
        parry // 招架动作
    }

// 定义法术类别
    public enum SpellClass
    {
        pyromancy, //
        miracles, // 
        sorcery // 
    }

// 定义法术类型
    public enum SpellType
    {
        projectile, // 投射类型
        buff, // 增益类型
        looping // 循环施法类型
    }

// 动作类，包含动作的各类属性和行为
    [System.Serializable]
    public class Action
    {
        public ActionInput input; // 触发动作的输入按键
        public ActionType type; // 动作的类型
        public SpellClass spellClass; // 法术类别（如果动作是法术）
        public string targetAnim; // 动作对应的动画名称
        public string audio_ids; // 动作对应的音效ID
        public List<ActionStep> steps; // 动作的多个步骤（用于连击等）
        public bool mirror = false; // 是否为镜像动作（用于双手武器）
        public bool canBeParried = true; // 是否可以被招架
        public bool changeSpeed = false; // 是否改变动画播放速度
        public float animSpeed = 1; // 动画速度倍率
        public bool canParry = false; // 是否可以进行招架
        public bool canBackStab = false; // 是否可以进行背刺
        public float staminaCost; // 体力消耗
        public float fpCost; // 法力消耗

        // 获取当前步骤中的一个动作，并递增索引
        public ActionStep GetActionStep(ref int indx)
        {
            // 检查步骤列表（steps）是否包含至少一个元素
            if (steps.Count > 0)
            {
                // 如果索引超出了步骤列表的范围，将索引重置为0
                if (indx > steps.Count - 1)
                    indx = 0;
        
                // 取出当前索引处的动作步骤（ActionStep）
                ActionStep retVal = steps[indx];

                // 更新索引以指向下一个步骤；如果索引超出范围，则重置为0，否则递增
                if (indx > steps.Count - 1)
                    indx = 0;
                else
                    indx++;

                // 返回当前步骤
                return retVal;
            }

            // 如果步骤列表为空，返回null
            return null;
        }

        [HideInInspector] public float parryMultiplier; // 招架时的伤害倍率
        [HideInInspector] public float backstabMultiplier; // 背刺时的伤害倍率

        public bool overrideDamageAnim; // 是否覆盖默认受伤动画
        public string damageAnim; // 自定义的受伤动画名称
    }

// 定义动作的多个步骤或分支，支持连击或不同的按键分支
    [System.Serializable]
    public class ActionStep
    {
        public List<ActionAnim> branches = new List<ActionAnim>(); // 动作的不同分支

        // 根据输入获取对应的分支动画
        public ActionAnim GetBranch(ActionInput inp)
        {
            for (int i = 0; i < branches.Count; i++)
            {
                if (branches[i].input == inp)
                    return branches[i];
            }

            return branches[0];
        }
    }

// 定义单个动画分支的属性
    [System.Serializable]
    public class ActionAnim
    {
        public ActionInput input; // 触发该动画的输入
        public string targetAnim; // 动画名称
        public string audio_ids; // 音效ID
    }

// 定义法术动作的属性
    [System.Serializable]
    public class SpellAction
    {
        public ActionInput input; // 触发法术的输入
        public string targetAnim; // 施法动画名称
        public string throwAnim; // 投掷动画名称（如果是投掷类法术）
        public float castTime; // 施法所需时间
        public float focusCost; // 法术专注值消耗
        public float staminaCost; // 体力消耗
    }
}