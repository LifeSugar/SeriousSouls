using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace rei
{
    public class AIHandler : MonoBehaviour
    {
        // 枚举定义敌人AI状态
        public enum AIState
        {
            Idle, //在索敌范围和视锥以外
            Far, //在索敌范围内但不在视锥里 或者在索敌范围x1 到 索敌范围 x2之间，但在视锥内并且没被阻挡
            Close, //在索敌范围和视锥内，但是被阻挡
            InSight, //在索敌范围内，在视锥内，且没有被阻挡
            Attacking
        }

        [Header("AI Settings")] public float sight = 10f; // 感知距离
        public float fov_angle = 100f; // 视锥角度
        // public float attackRange = 2f; // 攻击范围
        // public float attackAngle = 30f; // 攻击角度
        // public float attackCooldown = 2f; // 攻击冷却时间

        public AIAttacks[] aiAttacks;

        public EnemyStates estates;

        [FormerlySerializedAs("player_states")]
        public PlayerState playerPlayerStates; //玩家角色状态

        public Transform target; // 目标玩家

        [SerializeField] private AIState aiState = AIState.Idle; // 当前状态

        // 冷却时间相关
        private float currentCooldown = 0f; // 当前整体攻击冷却时间

        // 用于引用第一版脚本的计时器与延迟逻辑
        public int closeCount = 10;
        int _close;
        public int frameCount = 30;
        int _frame;
        public float attackCount = 3; //攻击间隔
        [SerializeField]float _attack = 3;

        // 距离和角度计算
        [SerializeField] private float distanceToTarget;
        [SerializeField] private float angleToTarget;
        Vector3 dirToTarget; //指向目标的方向

        

        private float delta;

        [Header("Debug Settings")] public bool showDebugGizmos = true; // 是否显示调试信息

        void Start()
        {
            Init();
        }
        public void Init()
        {
            estates = GetComponent<EnemyStates>();
            estates.Init();
            playerPlayerStates = estates.player;
            target = playerPlayerStates.transform;
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
            distanceToTarget = DistanceFromTarget();
            angleToTarget = AngleToTarget();
            
            delta = Time.deltaTime;
            if (target == null)
            {
                ChangeState(AIState.Idle);
                return;
            }
            else
            {
                dirToTarget = target.position - transform.position;
                estates.dirToTarget = dirToTarget;
            }

            UpdateCooldowns(); // 更新冷却时间
            UpdateAIState(); // 更新AI状态
            if (!estates.dontDoAnything && !estates.isDead)
                HandleCurrentState(); // 根据当前状态执行逻辑

            // 执行EnemyStates中的Tick更新（例如动画等）
            estates.Tick(Time.deltaTime);
        }

        // 更新冷却时间
        private void UpdateCooldowns()
        {
            // 全局攻击冷却
            if (currentCooldown > 0)
            {
                currentCooldown -= Time.deltaTime;
                if (currentCooldown < 0)
                    currentCooldown = 0;
            }

            // 各个攻击动作的冷却
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
            // Debug.Log($"AI State changed to: {newState}");
        }

        [SerializeField] private bool inRange;
        [SerializeField] private bool inDoubleRange;
        [SerializeField] private bool inFOV;

        // 更新AI状态(根据目标距离和角度来确定基本状态)
        private void UpdateAIState()
        {
            distanceToTarget = DistanceFromTarget();
            angleToTarget = AngleToTarget();

            inRange = distanceToTarget <= sight;
            inDoubleRange = distanceToTarget <= sight * 2;
            inFOV = angleToTarget <= fov_angle;

            bool isBlocked = PerformRaycastBlocked();

            // InSight状态判定条件
            bool canBeInSight = inRange && inFOV && !isBlocked;

            // 如果当前已经是InSight状态，则只有当下面两种情况发生才离开InSight：
            // 1. 目标超过双倍距离：Idle
            // 2. 目标超过sight并且被遮挡：Idle
            if (aiState == AIState.InSight)
            {
                if (distanceToTarget > sight * 2)
                {
                    ChangeState(AIState.Idle);
                    return;
                }

                if (distanceToTarget > sight && isBlocked)
                {
                    ChangeState(AIState.Idle);
                    return;
                }

                // 如果不满足InSight条件，但也不满足上述两种退出条件，则保持InSight不变
                if (!canBeInSight)
                {
                    // 保持InSight状态
                    return;
                }

                // 如果仍能保持InSight条件则继续保持
                return;
            }

            // 当不在InSight时的状态判断逻辑
            // InSight判定
            if (canBeInSight)
            {
                ChangeState(AIState.InSight);
                return;
            }

            // 其他状态转换逻辑（可根据之前定义的状态需求自行补充或删除）
            // 例如：
            // Close: 在sight范围和FOV内，但是被阻挡
            if (inRange && inFOV && isBlocked)
            {
                ChangeState(AIState.Close);
                return;
            }

            // Far: 在原先逻辑中可以是 (inRange && !inFOV) 或 
            // (sight < distanceToTarget ≤ sight*2 且 inFOV 且 !blocked) 等条件
            if ((inRange && !inFOV) || (distanceToTarget > sight && inDoubleRange && inFOV && !isBlocked))
            {
                ChangeState(AIState.Far);
                return;
            }

            // Idle: 超出两倍范围或者既不在范围也不在视锥内且无法变成Far状态时
            if (distanceToTarget > sight * 2 || (!inRange && !inFOV))
            {
                ChangeState(AIState.Idle);
                return;
            }

            // 兜底保护
            ChangeState(AIState.Idle);
        }

        // 在Close和InSight状态中或者判断其转换时需要使用的射线检测
        private bool PerformRaycastBlocked()
        {
            if (target == null) return true;

            Vector3 origin = transform.position;
            origin.y += 1.5f;

            // 将射线目标点抬高1米
            Vector3 targetPoint = target.position + Vector3.up * 1.5f;
            Vector3 dir = targetPoint - origin;

            // 如果射线击中任何非玩家物体（ignoreLayers中）则视为阻挡
            if (Physics.Raycast(origin, dir, out RaycastHit hit, Mathf.Min(distanceToTarget, sight * 2), estates.obscaleLayerMask))
            {
                return true;
            }

            return false;
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
                    if (estates.canMove)
                    {
                        estates.rotateToTarget = true;
                        aiState = AIState.InSight;
                    }

                    break;
                default:
                    break;
            }
        }

        protected virtual void HandleIdleState()
        {
            // 空闲逻辑，比如巡逻或等待
            estates.rotateToTarget = false;
            estates.agent.isStopped = true;
        }

        protected virtual void HandleFarState()
        {
            estates.agent.isStopped = true;
            estates.rotateToTarget = false;
            MoveTowardsTarget();
        }

        protected virtual void HandleCloseState()
        {
            estates.rotateToTarget = true;
            // close状态下尝试通过射线检测确认是否可以看到玩家
            _close++;
            if (_close > closeCount)
            {
                _close = 0;
                // 如果玩家跑出可视角范围
                if (distanceToTarget > sight || angleToTarget > fov_angle)
                {
                    ChangeState(AIState.Far);
                    return;
                }
            }
        }

        protected virtual void HandleInSightState()
        {
            #region delay handler

            HandleCooldowns(); //攻击冷却开始计算
            estates.rotateToTarget = true;

            float d2 = Vector3.Distance(estates.targetDestionation, target.position); //储存的目标位置和玩家位置之间的距离
            if (d2 > 2) //如果目标位置与玩家位置存在偏差 则向目标移动
                GoToTarget(); //重新索敌
            if (distanceToTarget < 2) //当距离很近的时候，关闭寻路，开打
                estates.agent.isStopped = true;


            if (_attack > 0)
            {
                _attack -= Time.deltaTime;
                return;
            }
            
            // Debug.Log("塔塔开");

            _attack = attackCount;

            #endregion

            #region perform attack

            AIAttacks a = WillAttack(); //得到将要进行攻击的攻击动作
            estates.SetCurrentAttack(a); //设置estates的攻击动作，为后续伤害计算做准备

            if (a != null && playerPlayerStates.isDead == false) //获取了攻击动作而且玩家还活着
            {
                aiState = AIState.Attacking; //切换状态
                estates.anim.Play(a.targetAnim); //播放攻击动作动画
                estates.anim.SetBool("OnEmpty", false); //设置参数 同时estates.canMove = false
                estates.canMove = false;
                a._cool = a.cooldown; //重置这个动作的冷却时间
                estates.agent.isStopped = true; //关闭navi
                estates.rotateToTarget = false; //关闭自动转向，在动作事件中修正方向
                return;
            }

            #endregion

            return;
        }


        void HandleCooldowns()
        {
            foreach (var attack in aiAttacks)
            {
                if (attack._cool <= 0)
                    continue;

                attack._cool -= delta;
                if (attack._cool < 0)
                {
                    attack._cool = 0;
                }
            }
        }


        void GoToTarget()
        {
            
            estates.hasDestination = false;
            estates.SetDestination(target.position);
        }

        void MoveTowardsTarget()
        {
            
        }
        

        float DistanceFromTarget()
        {
            if (target == null)
                return 100;

            return Vector3.Distance(target.position, transform.position);
        }

        float AngleToTarget()
        {
            float a = 180;
            if (target)
            {
                Vector3 dirToTarget = target.position - transform.position;
                a = Vector3.Angle(dirToTarget, transform.forward);
            }

            return a;
        }

        public AIAttacks WillAttack()
        {
            int totalWeight = 0;
            List<AIAttacks> validAttacks = new List<AIAttacks>();
            if (estates.canMove == false)
                return null;

            // 筛选所有符合条件的攻击动作
            foreach (var attack in aiAttacks)
            {
                if (attack._cool > 0) continue;
                if (distanceToTarget > attack.attackRange) continue;
                if (angleToTarget < attack.minAngle || angleToTarget > attack.maxAngle) continue;
                if (attack.weight == 0) continue;

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
            

            // 在这里进行两倍范围内的线段绘制
            float doubleRange = sight * 2;
            Vector3 origin = transform.position;
            origin.y += 1.5f;

            Vector3 targetPoint = target.position + Vector3.up * 1.5f;
            Vector3 dir = targetPoint - origin;
            float distance = dir.magnitude;

            if (distance <= doubleRange)
            {
                // 在两倍范围内，进行射线检测看看是否有阻挡
                bool blocked = false;
                if (Physics.Raycast(origin, dir, out RaycastHit hit, Mathf.Min(distance, doubleRange), estates.obscaleLayerMask))
                {
                    // 如果击中非玩家物体，视为阻挡
                    blocked = true;
                    // Debug.Log(hit.collider.gameObject.name);
                }

                // 根据阻挡情况改变线条颜色
                Gizmos.color = blocked ? Color.blue : Color.green;
                Gizmos.DrawLine(origin, origin + dir.normalized * Mathf.Min(distance, doubleRange));

                //在目标位置画个小点标示目标位置
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(target.position, 0.1f);
            }
        }
    }

    [System.Serializable]
    public class AIAttacks
    {
        public int weight;
        [FormerlySerializedAs("minDistance")] public float attackRange;
        public float minAngle;
        public float maxAngle;

        public float cooldown = 2;
        public float _cool;

        public string targetAnim;

        public bool isDefaultDamageCollider;
        public GameObject[] damageCollider;
    }
}