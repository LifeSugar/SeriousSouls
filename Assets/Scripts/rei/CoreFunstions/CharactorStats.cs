using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rei
{
    [System.Serializable]
    public class CharacterStats {
        
        [Header("Current")]
        public float _health;           // 当前生命值
        public float _focus;            // 当前集中值
        public float _stamina;          // 当前精力值
        public int _souls;              // 当前拥有的魂

        public float _healthRecoverValue = 60; // 生命恢复值
        public float _focusRecoverValue = 80;  // 集中恢复值
        
        [Header("Base Power")]
        public int hp;                  // 最大生命值
        public int fp;                  // 最大集中值
        public int stamina;             // 最大精力
        public float equipLoad;         // 装备负重
        public float poise;             // 韧性
        public int itemDiscover;        // 掉落概率

        [Header("Attack Power")]
        public int R_weapon_1;          // 右手武器1的攻击力
        public int R_weapon_2;          // 右手武器2的攻击力
        public int R_weapon_3;          // 右手武器3的攻击力
        public int L_weapon_1;          // 左手武器1的攻击力
        public int L_weapon_2;          // 左手武器2的攻击力
        public int L_weapon_3;          // 左手武器3的攻击力

        [Header("Defence")]
        public int physical;            // 物理防御力
        public int vs_strike;           // 打击防御力
        public int vs_slash;            // 劈砍防御力
        public int vs_thrust;           // 刺击防御力
        public int magic;               // 魔法防御力

        [Header("Resistances")]
        public int bleed;               // 流血抗性
        public int poison;              // 毒抗性
        public int frost;               // 冰冻抗性
        public int curse;               // 诅咒抗性

        public int attumentSlots;       // 技能槽数量（可装备技能或法术的数量）

        // 初始化当前状态，将基础值赋给动态状态
        public void InitCurrent() {
            _health = hp;               // 设置当前生命值
            _focus = fp;                // 设置当前集中值
            _stamina = stamina;         // 设置当前体力值
        }

        public delegate void StatEffects(); // 状态效果的委托
        public StatEffects statEffect;      // 存储当前状态效果的委托实例

        // 增加生命值（提升基础最大生命值）
        public void AddHealth() {
            hp += 5;
        }

        // 减少生命值（减少基础最大生命值）
        public void RemoveHealth() {
            hp -= 5;
        }
    }

    // 角色属性类，用于角色的成长属性和技能点分配
    [System.Serializable]
    public class Attributes {
        public int level;               // 角色等级
        public int souls;               // 当前拥有的魂
        public int vigor;               // 体力
        public int attunement;          // 专注值
        public int endurance;           // 精力
        public int vitality;            // 负重
        public int strength;            // 力量
        public int dexterity;           // 敏捷
        public int intelligence;        // 智力
        public int faith;               // 信仰
        public int luck;                // 幸运
    }

    // 武器属性类，用于管理武器的各类伤害值
    [System.Serializable]
    public class WeaponStats {
        public int physical;            // 物理伤害
        public int strike;              // 打击伤害
        public int slash;               // 劈砍伤害
        public int thrust;              // 刺击伤害
        public int magic;               // 魔法伤害
    }
}