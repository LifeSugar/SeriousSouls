using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

namespace rei
{
    [System.Serializable]
    public class Item
    {
        public string itemName;
        public string itemDescription;
        public Sprite icon;
    }


    public static class GlobalFuntions
    {
        public static void DeepCopyWeapon(Weapon from, Weapon to)
        {
            //Item
            to.itemName = from.itemName;
            to.itemDescription = from.itemDescription;
            to.icon = from.icon;

            //Weapon
            to.oh_idle = from.oh_idle;
            to.th_idle = from.th_idle;
            to.itemType = from.itemType;

            to.actions = new List<Action>();
            for (int i = 0; i < from.actions.Count; i++)
            {
                Action a = new Action();
                DeepCopyActionToAction(a, from.actions[i]);
                to.actions.Add(a);
            }

            to.two_handedActions = new List<Action>();
            for (int i = 0; i < from.two_handedActions.Count; i++)
            {
                Action a = new Action();
                DeepCopyActionToAction(a, from.two_handedActions[i]);
                to.two_handedActions.Add(a);
            }

            to.parryMultiplier = from.parryMultiplier;
            to.backstabMultiplier = from.backstabMultiplier;
            to.backstabMultiplier = from.backstabMultiplier;
            to.LeftHandMirror = from.LeftHandMirror;
            to.modelPrefab = from.modelPrefab;
            to.l_model_pos = from.l_model_pos;
            to.l_model_eulers = from.l_model_eulers;
            to.r_model_pos = from.r_model_pos;
            to.r_model_eulers = from.r_model_eulers;
            to.model_scale = from.model_scale;
            to.weaponStats = new WeaponStats();
            DeepCopyWeaponStats(from.weaponStats, to.weaponStats);
        }

        // (1.1)
        public static void DeepCopyActionToAction(Action a, Action w_a)
        {
            a.input = w_a.input;
            a.targetAnim = w_a.targetAnim;
            a.audio_ids = w_a.audio_ids;
            a.type = w_a.type;
            a.spellClass = w_a.spellClass;
            a.canBeParried = w_a.canBeParried;
            a.changeSpeed = w_a.changeSpeed;
            a.animSpeed = w_a.animSpeed;
            a.canBackStab = w_a.canBackStab;
            a.canParry = w_a.canParry;
            a.overrideDamageAnim = w_a.overrideDamageAnim;
            a.damageAnim = w_a.damageAnim;
            a.staminaCost = w_a.staminaCost;
            a.fpCost = w_a.fpCost;

            DeepCopyStepsList(w_a, a);
        }

        public static void DeepCopyStepsList(Action from, Action to)
        {
            to.steps = new List<ActionStep>();
            for (int i = 0; i < from.steps.Count; i++)
            {
                ActionStep step = new ActionStep();
                DeepCopySteps(from.steps[i], step);
                to.steps.Add(step);
            }
        }

        public static void DeepCopySteps(ActionStep from, ActionStep to)
        {
            to.branches = new List<ActionAnim>();
            for (int i = 0; i < from.branches.Count; i++)
            {
                ActionAnim a = new ActionAnim();
                a.input = from.branches[i].input;
                a.targetAnim = from.branches[i].targetAnim;
                a.audio_ids = from.branches[i].audio_ids;
                to.branches.Add(a);
            }
        }

        public static void DeepCopyWeaponStats(WeaponStats from, WeaponStats to)
        {
            to.physical = from.physical;
            to.slash = from.slash;
            to.strike = from.strike;
            to.thrust = from.thrust;
            to.magic = from.magic;
        }


        //***********************Spells***************************

        public static void DeepCopySpell(Spell from, Spell to)
        {
            //Item
            to.itemName = from.itemName;
            to.itemDescription = from.itemDescription;
            to.icon = from.icon;

            //Spell
            to.itemType = from.itemType;
            to.spellType = from.spellType;
            to.spellClass = from.spellClass;
            to.projectile = from.projectile;
            to.spell_effect = from.spell_effect;
            to.particle_prefab = from.particle_prefab;

            to.actions = new List<SpellAction>();
            for (int i = 0; i < from.actions.Count; i++)
            {
                SpellAction a = new SpellAction();
                DeepCopySpellAction(from.actions[i], a);
                to.actions.Add(a);
            }
        }

        public static void DeepCopySpellAction(SpellAction from, SpellAction to)
        {
            to.input = from.input;
            to.targetAnim = from.targetAnim;
            to.throwAnim = from.throwAnim;
            to.castTime = from.castTime;
            to.staminaCost = from.staminaCost;
            to.focusCost = from.focusCost;
        }

        //***********************Consumables***************************

        public static void DeepCopyConsumable(Consumable to, Consumable from)
        {
            to.itemName = from.itemName;
            to.icon = from.icon;
            to.itemDescription = from.itemDescription;

            to.itemType = from.itemType;
            to.consumableEffect = from.consumableEffect;
            to.targetAnim = from.targetAnim;
            to.audio_id = from.audio_id;
            to.itemPrefab = from.itemPrefab;
            to.model_scale = from.model_scale;
            to.r_model_eulers = from.r_model_eulers;
            to.r_model_pos = from.r_model_pos;
        }


        //----------------------------------------------For ActionManager to StateManager--------------------------------------------------------------------------
        //(2)
        public static void DeepCopyAction(Weapon w, ActionInput inp, ActionInput assign, List<Action> actionList,
            bool isLeftHand = false)
        {
            Action a = GetAction(assign, actionList);
            // Debug.Log(a.input.ToString());
            // Debug.Log(w.itemName + "114");
            Action w_a = w.GetAction(w.actions, inp);
            if (w_a == null)
            {
                Debug.Log("weapon action not found");
                return;
            }

            DeepCopyStepsList(w_a, a);
            a.type = w_a.type;
            a.targetAnim = w_a.targetAnim;
            a.audio_ids = w_a.audio_ids;
            a.spellClass = w_a.spellClass;
            a.canBeParried = w_a.canBeParried;
            a.changeSpeed = w_a.changeSpeed;
            a.animSpeed = w_a.animSpeed;
            a.canBackStab = w_a.canBackStab;
            a.canParry = w_a.canParry;
            a.overrideDamageAnim = w_a.overrideDamageAnim;
            a.damageAnim = w_a.damageAnim;
            a.parryMultiplier = w.parryMultiplier;
            a.backstabMultiplier = w.backstabMultiplier;
            a.staminaCost = w_a.staminaCost;
            a.fpCost = w_a.fpCost;

            if (isLeftHand)
            {
                a.mirror = true;
            }
        }

        // private Getter (Get actionSlots)
        public static Action
            GetAction(ActionInput inp, List<Action> actionSlots) //从Action Slots中提取对映输入的Action（里面有step，step里有branch）
        {
            for (int i = 0; i < actionSlots.Count; i++)
            {
                if (actionSlots[i].input == inp)
                    return actionSlots[i];
            }

            return null;
        }
    }


    public static class GlobalStrings
    {
        // 通用的输入名称
        public static string DPadVertical;
        public static string DPadHorizontal;
        public static string Vertical = "Vertical";
        public static string Horizontal = "Horizontal";
        public static string B = "B";
        public static string A = "A";
        public static string X = "X";
        public static string Y = "Y";
        public static string RT = "RT";
        public static string LT = "LT";
        public static string RB = "RB";
        public static string LB = "LB";
        public static string L = "L";
        public static string R = "R";
        public static string RightVertical;
        public static string RightHorizontal;

        public static string View = "View";
        public static string Menu = "Menu";

        public static bool isWindows;

        // Animator Parameters
        public static string vertical = "vertical";
        public static string horizontal = "horizontal";
        public static string mirror = "mirror";
        public static string parry_attack = "parry_attack";
        public static string animSpeed = "animSpeed";
        public static string onGround = "onGround";
        public static string run = "run";
        public static string two_handed = "two_handed";
        public static string interacting = "interacting";
        public static string blocking = "blocking";
        public static string isLeft = "isLeft";
        public static string canMove = "canMove";
        public static string lockon = "lockon";

        // Animator States
        public static string Rolls = "Rolls";
        public static string attack_interrupt = "attack_interrupt";
        public static string parry_received = "parry_received";
        public static string backstab_received = "backstab_received";

        // Data
        public static string itemFolder = "/Items/";

        //获取应用程序的存储路径
        public static string SaveLocation()
        {
            string r = Application.streamingAssetsPath;
            if (!Directory.Exists(r))
            {
                Directory.CreateDirectory(r);
            }

            return r;
        }
        
        // 静态构造函数
        static GlobalStrings()
        {
            isWindows = (Application.platform == RuntimePlatform.WindowsPlayer ||
                         Application.platform == RuntimePlatform.WindowsEditor);

            DPadVertical = isWindows ? "DPadVerticalW" : "DPadVertical";
            DPadHorizontal = isWindows ? "DPadHorizontalW" : "DPadHorizontal";
            RightVertical = isWindows ? "RightVerticalW" : "RightVertical";
            RightHorizontal = isWindows ? "RightHorizontalW" : "RightHorizontal";
        }
    }
}