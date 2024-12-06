using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


namespace rei
{
    public class EnemyAIHandler : MonoBehaviour
    {
        public AIAttacks[] ai_attacks;

        public EnemyStates estates;

        [FormerlySerializedAs("player_states")] public PlayerState playerPlayerStates; //角色
        public Transform target;

        //管理可视范围
        public float sight;
        public float fov_angle;

        public int closeCount = 10;
        int _close;

        public int frameCount = 30;
        int _frame;

        public int attackCount = 30;
        int _attack;

        float dis; //和目标的距离
        float angle; //和目标的角度
        float delta;
        Vector3 dirToTarget;//指向目标的方向
        void Start()
        {
            Init();
        }

        public void Init()
        {
            if (estates == null)
                estates = GetComponent<EnemyStates>();

            estates.Init(); //初始化自身
            playerPlayerStates = estates.player;
            target = playerPlayerStates.transform;
            InitDamageColliders();//初始化伤害盒
        }

        void InitDamageColliders()
        {
            for (int i = 0; i < ai_attacks.Length; i++) //初始化攻击伤害
            {
                for (int j = 0; j < ai_attacks[i].damageCollider.Length; j++)
                {
                    DamageCollider d = ai_attacks[i].damageCollider[j].GetComponent<DamageCollider>();
                    d.InitEnemy(estates);
                }
            }

            for (int i = 0; i < estates.defaultDamageCollider.Length; i++) //开启默认的碰撞盒（一般是手持武器
            {
                DamageCollider d = estates.defaultDamageCollider[i].GetComponent<DamageCollider>();
                d.InitEnemy(estates);
            }
        }

        public enum AIState //状态
        {
            far, //远
            close, //进
            inSight, //视野内
            attacking //正在攻击
        }

        public AIState aiState;

        void Update()
        {
            delta = Time.deltaTime;
            dis = DistanceFromTarget();
            angle = AngleToTarget();
            if (target)
                dirToTarget = target.position - transform.position;
            estates.dirToTarget = dirToTarget;

            switch (aiState)
            {
                case AIState.far:
                    HandleFarSight();
                    break;
                case AIState.close:
                    HandleCloseSight();
                    break;
                case AIState.inSight:
                    Insight();
                    break;
                case AIState.attacking:
                    if (estates.canMove)
                    {
                        estates.rotateToTarget = true;
                        aiState = AIState.inSight;
                    }

                    break;
                default:
                    break;
            }

            estates.Tick(delta);
        }

        void GoToTarget()
        {
            estates.hasDestination = false;//更新状态
            estates.SetDestination(target.position); //更新目标位置到玩家并向目标移动
        }

        void Insight()
        {
            #region delay handler

            HandleCooldowns();//攻击冷却开始计算

            float d2 = Vector3.Distance(estates.targetDestionation, target.position); //储存的目标位置和玩家位置之间的距离
            if (d2 > 2 || dis > sight * 5) //如果目标位置与玩家位置存在偏差，或者距离太远，则向目标移动
                GoToTarget();//重新索敌
            if (dis < 2)//当距离很近的时候，关闭寻路，开打
                estates.agent.isStopped = true;


            if (_attack > 0) //每三十帧调用后续逻辑一次
            {
                _attack--;
                return;
            }

            _attack = attackCount;

            #endregion

            #region perform attack

            AIAttacks a = WillAttack(); //得到将要进行攻击的攻击动作
            estates.SetCurrentAttack(a); //设置estates的攻击动作，为后续伤害计算做准备

            if (a != null && playerPlayerStates.isDead == false) //获取了攻击动作而且玩家还活着
            {
                aiState = AIState.attacking; //切换状态
                estates.anim.Play(a.targetAnim); //播放攻击动作动画
                estates.anim.SetBool("OnEmpty", false); //设置参数 同时estates.canMove = false
                estates.canMove = false;
                a._cool = a.cooldown;//重置这个动作的冷却时间
                estates.agent.isStopped = true; //关闭navi
                estates.rotateToTarget = false; //关闭自动转向，在动作事件中修正方向
                return;
            }

            #endregion

            return;
        }

        void HandleCooldowns()
        {
            foreach (var attack in ai_attacks)
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

        public AIAttacks WillAttack()
        {
            int totalWeight = 0;
            List<AIAttacks> validAttacks = new List<AIAttacks>();

            // 筛选所有符合条件的攻击动作
            foreach (var attack in ai_attacks)
            {
                if (attack._cool > 0) continue; // 在冷却中，跳过
                if (dis > attack.minDistance) continue; // 距离太近，跳过
                if (angle < attack.minAngle || angle > attack.maxAngle) continue; // 角度不符合，跳过
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

        void HandleFarSight()
        {
            if (target == null)
                return;

            _frame++; //开始计数 
            if (_frame > frameCount) //每30帧后开始行动
            {
                _frame = 0;

                if (dis < sight)//到达可视距离
                {
                    if (angle < fov_angle)//到达可视范围（视锥）
                    {
                        aiState = AIState.close;//切换为近处状态
                    }
                }
            }
        }

        void HandleCloseSight()
        {
            _close++;
            if (_close > closeCount)//十帧后开始监测是否跑出可视范围
            {
                _close = 0;

                if (dis > sight || angle > fov_angle) //跑出可视角范围
                {
                    aiState = AIState.far;
                    return;
                }
            }

            RaycastToTarget(); //查看是否有遮挡
        }

        void RaycastToTarget()
        {
            RaycastHit hit;
            Vector3 origin = transform.position;
            origin.y += 0.5f;
            Vector3 dir = dirToTarget;
            dir.y += 0.5f;

            Debug.DrawRay(origin, dir, Color.red);
            if (Physics.Raycast(origin, dir, out hit, sight, estates.ignoreLayers))
            {
                PlayerState st = hit.transform.GetComponentInParent<PlayerState>();
                if (st != null) //如果不存在遮挡 那么开始接敌动作
                {
                    estates.rotateToTarget = true; 
                    aiState = AIState.inSight;
                }
            }
        }

        float DistanceFromTarget() //获取和玩家的距离
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
                Vector3 d = dirToTarget;
                a = Vector3.Angle(d, transform.forward);
            }

            return a;
        }
    }

    [System.Serializable]
    public class AIAttacks
    {
        public int weight;
        public float minDistance;
        public float minAngle;
        public float maxAngle;

        public float cooldown = 2;
        public float _cool;

        public string targetAnim;

        public bool isDefaultDamageCollider;
        public GameObject[] damageCollider;
    }
}