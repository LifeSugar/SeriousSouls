using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace rei
{
    public class AIHandlerZaku : MonoBehaviour
    {
        // 枚举定义敌人AI状态
        public enum AIState
        {
            Idle,
            Far,
            Close,
            InSight,
            Attacking
        }

        [Header("AI Settings")] public float sight = 10f; // 感知距离
        public float fov_angle = 45f; // 视锥角度
        public float attackRange = 2f; // 攻击范围
        public float attackAngle = 30f; // 攻击角度
        public float attackCooldown = 2f; // 攻击冷却时间
        public AIAttacks[] aiAttacks;
        public EnemyStates estates;
        public Transform target; // 目标玩家

        private AIState aiState = AIState.Idle; // 当前状态
        private float currentCooldown = 0f; // 当前冷却时间
        
        [SerializeField]
        private float distanceToTarget;
        private float angleToTarget;

        [Header("Debug Settings")] public bool showDebugGizmos = true; // 是否显示调试信息

        void Start()
        {
            estates = GetComponent<EnemyStates>();
            estates.Init();
            InitDamageColliders();
        }

        void InitDamageColliders()
        {
            // 初始化攻击伤害
            foreach (var attack in aiAttacks)
            {
                foreach (var collider in attack.damageCollider)
                {
                    DamageCollider damageCollider = collider.GetComponent<DamageCollider>();
                    damageCollider.InitEnemy(estates);
                }
            }

            // 开启默认的碰撞盒（一般是手持武器）
            foreach (var collider in estates.defaultDamageCollider)
            {
                DamageCollider damageCollider = collider.GetComponent<DamageCollider>();
                damageCollider.InitEnemy(estates);
            }
        }

        private void Update()
        {
            UpdateCooldowns(); // 更新冷却时间
            UpdateAIState(); // 更新AI状态
            HandleCurrentState(); // 根据当前状态执行逻辑
        }

        // 更新冷却时间
        private void UpdateCooldowns()
        {
            if (currentCooldown > 0)
            {
                currentCooldown -= Time.deltaTime;
                currentCooldown = Mathf.Max(0, currentCooldown);
            }
            foreach (var attack in aiAttacks)
            {
                if (attack._cool <= 0) 
                    continue;

                attack._cool -= Time.deltaTime;
                if (attack._cool < 0)
                {
                    attack._cool = 0;
                }
            }
        }

        // 统一管理状态切换
        private void ChangeState(AIState newState)
        {
            if (aiState == newState) return; // 如果状态未改变，则无需切换
            aiState = newState;
            Debug.Log($"AI State changed to: {newState}"); // 打印状态切换日志
        }

        // 更新AI状态
        private void UpdateAIState()
        {
            if (target == null)
            {
                ChangeState(AIState.Idle);
                return;
            }

            //计算到目标的距离和角度
            distanceToTarget = Vector3.Distance(transform.position, target.position);
            angleToTarget = Vector3.Angle(transform.forward, target.position - transform.position);

            if (distanceToTarget > sight * 5)
            {
                ChangeState(AIState.Far);
            }
            else if (distanceToTarget < sight && angleToTarget < fov_angle)
            {
                ChangeState(AIState.Close);
            }
            else if (distanceToTarget < sight / 2)
            {
                ChangeState(AIState.InSight);
            }

            if (aiState == AIState.InSight && CanAttack())
            {
                ChangeState(AIState.Attacking);
            }
        }

        // 判断是否可以攻击
        private bool CanAttack()
        {
            if (currentCooldown > 0) return false;
            if (target == null) return false;

            return distanceToTarget <= attackRange && angleToTarget <= attackAngle;
        }

        // 根据当前状态执行对应的逻辑
        private void HandleCurrentState()
        {
            switch (aiState)
            {
                case AIState.Idle:
                    HandleIdleState();
                    break;
                case AIState.Far:
                    HandleFarState();
                    break;
                case AIState.Close:
                    HandleCloseState();
                    break;
                case AIState.InSight:
                    HandleInSightState();
                    break;
                case AIState.Attacking:
                    HandleAttackingState();
                    break;
            }
        }

        private void HandleIdleState()
        {
            // 空闲逻辑，比如巡逻或等待
            Debug.Log("Idle: Patrol or wait.");
        }

        float waitTime = 0f;
        private void HandleFarState()
        {
            if (target == null)
                return;

            waitTime += Time.deltaTime;
            
            if (waitTime > 2f) //每30帧后开始行动
            {
                waitTime = 0;

                if (distanceToTarget < sight) //到达可视距离
                {
                    if (angleToTarget < fov_angle) //到达可视范围（视锥）
                    {
                        aiState = AIState.Close; //切换为近处状态
                    }
                }
            }
            Debug.Log("Far: Moving towards target.");
            MoveTowardsTarget();
        }

        private void HandleCloseState()
        {
            // 近距离锁定玩家
            Debug.Log("Close: Preparing for combat.");
            LookAtTarget();
        }

        private void HandleInSightState()
        {
            // 锁定玩家的逻辑
            Debug.Log("InSight: Target locked, preparing to attack.");
        }

        private void HandleAttackingState()
        {
            // 攻击逻辑
            Debug.Log("Attacking: Executing attack.");
            PerformAttack();
        }

        // 示例行为方法
        private void MoveTowardsTarget()
        {
            // 移动到目标位置
        }

        private void LookAtTarget()
        {
            // 朝向目标
            if (target != null)
            {
                Vector3 direction = target.position - transform.position;
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        private void PerformAttack()
        {
            // 执行攻击逻辑
            currentCooldown = attackCooldown; // 重置冷却时间
        }

        public AIAttacks GetAttack()
        {
            int totalWeight = 0;
            List<AIAttacks> validAttacks = new List<AIAttacks>();
            // 筛选所有符合条件的攻击动作
            foreach (var attack in aiAttacks)
            {
                if (attack._cool > 0) continue; // 在冷却中，跳过
                if (distanceToTarget > attack.minDistance) continue; // 距离太近，跳过
                if (angleToTarget < attack.minAngle || angleToTarget > attack.maxAngle) continue; // 角度不符合，跳过
                if (attack.weight == 0) continue; // 权重为0，跳过

                totalWeight += attack.weight;
                validAttacks.Add(attack);
            }

            // 如果没有符合的攻击动作，返回空
            if (validAttacks.Count == 0)
                return null;

            // 基于权重随机选择攻击动作
            int randomValue = Random.Range(0, totalWeight);
            int cumulativeWeight = 0;

            foreach (var attack in validAttacks)
            {
                cumulativeWeight += attack.weight;
                if (cumulativeWeight > randomValue)
                {
                    return attack;
                }
            }

            return null; // 理论上不会执行到这里
        }

        // 显示调试Gizmos
        private void OnDrawGizmos()
        {
            if (!showDebugGizmos || target == null) return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, sight); // 感知范围

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange); // 攻击范围
        }
    }
}