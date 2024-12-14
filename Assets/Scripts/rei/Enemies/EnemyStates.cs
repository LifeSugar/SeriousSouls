using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Serialization;


namespace rei
{
    public class EnemyStates : MonoBehaviour
    {
        public bool isBoss;
        [Header("States")] 
        public int health; //当前生命
        public int maxHealth; //生命上限
        public CharacterStats characterStats;//属性
        
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private int initialHealth;

        [Header("Value")] 
        public float delta;
        public float vertical;
        public float horizontal;
        public float poiseDegrade = 1f; //韧性每秒回复

        [Header("States")] 
        public bool canBeParried = true; //是否能被格挡
        public bool parryIsOn = true;
        public bool isInvincible; //无敌
        public bool dontDoAnything; //什么都不做
        public bool canMove; //可以移动
        public bool isDead; //鼠了捏
        //寻找目标相关
        public bool hasDestination; 
        public Vector3 targetDestionation;
        public Vector3 dirToTarget;
        public bool rotateToTarget;
        //大剑类or直剑类敌人，之后可以改成枚举类来判断敌人种类（但可能没精力搞这么多种类了
        public bool isGreatSword;
        public bool damaged = false;//是否受到伤害

        public bool applyRootTransform;

        //与其他组件的dependence
        public Animator anim;
        public Rigidbody rigid;
        EnemyTarget enTarget;
        AnimatorHook a_hook;
        public PlayerState parriedBy;
        public LayerMask obscaleLayerMask; //索敌时
        public NavMeshAgent agent;
        public PlayerState player;
        public GameObject lockOnGameObject;//被锁定的标记
        public GameObject enemyCanvas;
        public GameObject dropGameObject;
        public AudioSource audioSource;

        //攻击
        AIAttacks curAttack;
        public void SetCurrentAttack(AIAttacks a)
        {
            curAttack = a;
        }
        public AIAttacks GetCurrentAttack()
        {
            return curAttack;
        }

        public GameObject[] defaultDamageCollider; //默认伤害的的判定盒,和动作无关（一般是武器的判断盒）


        List<Rigidbody> ragdollRigids = new List<Rigidbody>(); //参与布娃娃的rb
        List<Collider> ragdollColliders = new List<Collider>(); //参与布娃娃的collider

        float timer;
        Image healthBar; //血条

        public delegate void SpellEffect_Loop(); //声明一个委托类型（嵌套类）

        public SpellEffect_Loop spellEffect_loop; //申明一个委托类型的字段，储存所有法术施加的方法

        public void Init()
        {
            anim = GetComponentInChildren<Animator>(); //获得Animator
            audioSource = this.gameObject.AddComponent<AudioSource>(); //添加AudioSource
            audioSource.maxDistance = 3.5f;
            enTarget = GetComponent<EnemyTarget>();//获取EnemyTarget
            enTarget.Init(this);//初始化EnemyTarget
            
            rigid = GetComponent<Rigidbody>();//获取rb
            agent = GetComponent<NavMeshAgent>();
            rigid.isKinematic = true; //设置为运动学物体，完全由脚本控制物体的运动和旋转
            player = InputHandler.instance.gameObject.GetComponent<PlayerState>();
            
            // //获取血条UI
            // enemyCanvas = GetComponentInChildren<Canvas>();

            //添加（获取）AnimatorHook 用于调整角色根运动
            a_hook = anim.GetComponent<AnimatorHook>();
            if (a_hook == null)
                a_hook = anim.gameObject.AddComponent<AnimatorHook>();
            a_hook.Init(null, this);
            
            //初始化布娃娃效果
            InitRagDoll();
            
            parryIsOn = false;

            //添加交互的蒙版
            this.gameObject.layer = 8;
            // ignoreLayers = 1  << 28;

            lockOnGameObject.SetActive(false);
            
            //初始化血条
            healthBar = enemyCanvas.transform.Find("HealthBG").Find("Health").GetComponent<Image>();
            enemyCanvas.gameObject.SetActive(false);
            health = maxHealth;
            
            //保存初始状态
            SaveInitialState();
        }

        void InitRagDoll()
        {
            Rigidbody[] rigs = GetComponentsInChildren<Rigidbody>();
            for (int i = 0; i < rigs.Length; i++) //历遍骨骼上的所有的rb
            {
                if (rigs[i] == rigid)//排除自己
                    continue;

                ragdollRigids.Add(rigs[i]); //获取所有的布娃娃rb
                rigs[i].isKinematic = true;//设置这些rb只能被脚本和动画控制

                Collider col = rigs[i].gameObject.GetComponent<Collider>();//获取所有布娃娃碰撞体
                col.isTrigger = true; //设置为trigger
                ragdollColliders.Add(col);
            }
        }

        //开启布娃娃方法，同时在这一帧结束时关闭动画，和此组件，即角色死亡
        public void EnableRagdoll()
        {
            
            //关闭主碰撞和rb的物理交互
            Collider controllerCollider = rigid.gameObject.GetComponent<Collider>(); 
            controllerCollider.enabled = false;
            rigid.isKinematic = true;
            rigid.velocity = Vector3.zero;
            
            agent.isStopped = true;
            agent.enabled = false;
            StartCoroutine(CloseAnimator());
            for (int i = 0; i < ragdollRigids.Count; i++)//历遍所有布娃娃组件，开启他们的碰撞和物理交互
            {
                ragdollRigids[i].velocity = Vector3.zero;
                ragdollRigids[i].isKinematic = false;
                ragdollColliders[i].isTrigger = false;
                ragdollRigids[i].detectCollisions = true;
            }
            
            
            //执行“shut down”

        }
        
        //关掉这个组件和动画
        IEnumerator CloseAnimator()
        {
            this.GetComponentInChildren<AnimatorHook>().enabled = false;
            anim.enabled = false;
            var ai = this.GetComponent<AIHandler>();
            ai.enabled = false;
            
            yield return new WaitForEndOfFrame();
            // this.enabled = false;
        }

        public void Tick(float d)
        {
            applyRootTransform = anim.applyRootMotion;
            if (isGreatSword) //大剑哥
                anim.Play("gs_oh_idle_r");
            else //直剑哥
                anim.Play("oh_idle_r");
            delta = d;
            // delta = Time.deltaTime;
            canMove = anim.GetBool("OnEmpty");

            if (enTarget != null)
            {
                //监测自生是否被锁定
                if (player.lockOnTarget == null)
                    enTarget.isLockOn = false;

                //如果被锁定，打开被锁定的指示物
                if (enTarget.isLockOn)
                {
                    lockOnGameObject.SetActive(true);
                }
                else
                {
                    lockOnGameObject.SetActive(false);
                }
            }
            
            

            //被砍了就打开血条
            if (damaged)
                enemyCanvas.gameObject.SetActive(true);

            //实时更新血条
            UpdateEnemyHealthUI(health, maxHealth);

            //法术再说吧 估计没有
            if (spellEffect_loop != null)
                spellEffect_loop();

            //锁住这个敌人
            if (dontDoAnything)
            {
                dontDoAnything = !canMove;
                return;
            }

            
            //转向敌人
            if (rotateToTarget)
            {
                LookTowardsTarget();
            }

            if (health <= 0)
            {
                // Debug.Log("dfdfdf");
                anim.SetBool("44", true);
                //鼠了捏
                if (!isDead)
                {
                    isDead = true;
                    
                    lockOnGameObject.SetActive(false);
                    InputHandler.instance._playerStates.characterStats._souls += 1000;
                    
                    enemyCanvas.gameObject.SetActive(false);//关血条
                    // audioSource.PlayOneShot(ResourceManager.instance.GetAudio("die").audio_clip);
                    EnableRagdoll(); //开启布娃娃效果
                    this.GetComponent<AIHandler>().enabled = false;
                    StartCoroutine(StartSinking());//沉入地下
                }
            }

            //处理无敌帧，能动了说明你无敌帧过了
            if (isInvincible)
            {
                isInvincible = !canMove;
            }

            //被弹反了，但被弹反的过程结束了
            if (parriedBy != null && parryIsOn == false)
            {
                parriedBy = null;
            }

            if (canMove)
            {
                parryIsOn = false; //被弹反的过程结束了
                anim.applyRootMotion = false; //能动的时候不要应用根运动

                MovementAnimation();
            }
            else
            {
                if (anim.applyRootMotion == false)
                    anim.applyRootMotion = true; //不能动的时候应用根运动
            }

            characterStats.poise -= delta * poiseDegrade; //每秒回复韧性
            if (characterStats.poise < 0)
                characterStats.poise = 0;
            damaged = false; //更新受伤状态
        }

        public void MovementAnimation()
        {
            float square = agent.desiredVelocity.sqrMagnitude; // 获取代理的期望速度的平方
            float v = Mathf.Clamp(square, 0, 0.5f); // 将速度值限制在 0 到 0.5 之间
            anim.SetFloat("vertical", v, 0.2f, delta); // 平滑设置动画参数 "vertical"
        }

        //逐渐向目标方向旋转
        void LookTowardsTarget()
        {
            // 获取指向目标的方向向量（dirToTarget在AIhandler里计算）
            Vector3 dir = dirToTarget;

            // 忽略 y 轴的差异，只在水平面上计算旋转方向
            dir.y = 0;

            // 使用 LookRotation 创建一个目标旋转四元数，该四元数表示物体应该面向 dir 的方向
            Quaternion targetRotation = Quaternion.LookRotation(dir);

            // 使用 Slerp（球面线性插值）方法逐渐将当前物体的旋转（transform.rotation）平滑地过渡到目标旋转
            // delta 是时间步长，`delta * 5` 表示调整旋转速度
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, delta * 5);
        }

        public void SetDestination(Vector3 d)
        {
            if (agent) // 检查 agent 是否存在
            {
                if (!hasDestination) // 如果当前没有目标位置
                {
                    hasDestination = true; // 设置标志位，表示已有目标
                    agent.isStopped = false; // 让 NavMeshAgent 继续移动
                    agent.SetDestination(d); // 设置 NavMeshAgent 的目标位置
                    targetDestionation = d; // 记录目标位置到成员变量 targetDestionation
                }
            }
        }

        public void DoDamage()//造成伤害
        {
            if (!InputHandler.instance._playerStates.powered && isBoss)
                return;
            if (isInvincible) //如果无敌那么无视发生
                return;
            damaged = true;
            rotateToTarget = true;
            //int damage = StatsCalculations.CalculateBaseDamage(curWeapon.weaponStats, characterStats); 一些复杂的伤害计算方法还没写
            int damage = 60; //凑合用先
            health -= damage;
            // audioSource.PlayOneShot(ResourceManager.instance.GetAudio("slash_impact").audio_clip);//被砍音效
            if (canMove) //在没动作的情况下随机播放受伤动画
            {
                int ran = Random.Range(0, 100);
                string tA = (ran > 50) ? "damage1" : "damage2";
                anim.Play(tA);
            }

            isInvincible = true; //开启无敌帧
            anim.applyRootMotion = true; //开启根动画控制
            anim.SetBool("canMove", false);
        }

        public void DoDamageSpell() //受到法术伤害
        {
            if (isInvincible)
                return;
            health -= 50;
            audioSource.PlayOneShot(ResourceManager.instance.GetAudio("damage_3").audio_clip);
            damaged = true;
            rotateToTarget = true;
            anim.Play("damage_3");
        }

        public void HandleBlocked() //被格挡了
        {
            // audioSource.PlayOneShot(ResourceManager.instance.GetAudio("shield_impact").audio_clip);//乓的一声
            anim.Play("attack_interrupt"); //攻击被阻挡的动画
            anim.SetFloat("interruptSpeed", 3f);
            player.characterStats._stamina -= 60;
            Vector3 targetDir = transform.position - player.transform.position;
            player.SnapToRotation(targetDir);
            CloseDamageCollider();
        }

        public void CheckForParry(Transform target, PlayerState playerStates) //检查是否被弹反到了，如果被弹反到了，那么就被处决
        {
            // Debug.Log("Checking for parry");
            if (canBeParried == false || parryIsOn == false || isInvincible)
                return;

            Vector3 dir = transform.position - target.position;
            dir.Normalize();
            float dot = Vector3.Dot(target.forward, dir);
            if (dot < 0)
                return;


            isInvincible = true;
            anim.Play("attack_interrupt");
            anim.SetFloat("interruptSpeed", 1.2f);
            anim.applyRootMotion = true;
            anim.SetBool("canMove", false);
            			// states.parryTarget = this;
            parriedBy = playerStates;
            return;
        }

        public void IsGettingParried(Action a, Weapon curWeapon) //被处决
        {
            // if (isBoss && !InputHandler.instance._playerStates.powered)
            //     return;
            damaged = true;
            //float damage = StatsCalculations.CalculateBaseDamage(curWeapon.weaponStats, characterStats, a.parryMultiplier);计算处决伤害
            float damage = 180;
            if (health < damage)
                anim.SetBool("44" , true);

            StartCoroutine(DoParriedDamage(damage));
            agent.isStopped = true;
            dontDoAnything = true;
            anim.SetBool("canMove", false);
            anim.applyRootMotion = true;
            anim.Play("getting_parried");
            parriedBy = null;
        }

        IEnumerator DoParriedDamage(float damage)
        {
            yield return new WaitForSeconds(0.8f);
            health -= Mathf.RoundToInt(damage);
        }

        public void IsGettingBackStabbed(Action a, Weapon curWeapon) //被背刺，直接死
        {
            if (isBoss && !InputHandler.instance._playerStates.powered)
                return;
            dontDoAnything = true;
            anim.SetBool("canMove", false);
            anim.Play("getting_backstabbed");
            StartCoroutine(PlaySlashImpact());
            StartCoroutine(SetHealth());
        }

        IEnumerator SetHealth()//两秒之后直接死
        {
            yield return new WaitForSeconds(2.1f);
            health = 0;
        }

        IEnumerator PlaySlashImpact()
        {
            yield return new WaitForSeconds(0.5f);
            // audioSource.PlayOneShot(ResourceManager.instance.GetAudio("slash_impact").audio_clip);
        }

        // public ParticleSystem fireParticle;
        // float _t;
        //
        // public void OnFire()
        // {
        //     if (fireParticle == null)
        //         return;
        //
        //     if (_t < 3)
        //     {
        //         _t += Time.deltaTime;
        //         fireParticle.Emit(1);
        //     }
        //     else
        //     {
        //         _t = 0;
        //         spellEffect_loop = null;
        //     }
        // }

        public void OpenDamageCollier()
        {
            if (curAttack == null)
                return;
        
            if (curAttack.isDefaultDamageCollider || curAttack.damageCollider.Length == 0)
            {
                ObjectListStatus(defaultDamageCollider, true);
            }
            else
            {
                ObjectListStatus(curAttack.damageCollider, true);
            }
        }
        
        public void CloseDamageCollider()
        {
            if (curAttack == null)
                return;
        
            if (curAttack.isDefaultDamageCollider || curAttack.damageCollider.Length == 0)
            {
                ObjectListStatus(defaultDamageCollider, false);
            }
            else
            {
                ObjectListStatus(curAttack.damageCollider, false);
            }
        }

        void ObjectListStatus(GameObject[] l, bool status)
        {
            for (int i = 0; i < l.Length; i++)
            {
                l[i].SetActive(status);
            }
        }

        IEnumerator StartSinking()
        {
            Debug.Log("Starting sinking");
            this.GetComponent<CapsuleCollider>().enabled = false;
            player.lockOnTarget = null;
            EnemyManager.instance.enemyTargets.Remove(enTarget);
            yield return new WaitForSeconds(2.8f);
            if (dropGameObject)
                HandleDropItem();
            foreach (var c in ragdollColliders)
            {
                c.isTrigger = true;
            }
            foreach (var r in ragdollRigids)
            {
                r.useGravity = false;
            }
            transform.DOMoveY(transform.position.y - 1, 2).SetEase(Ease.InOutQuad).OnComplete(() => Destroy(this.gameObject));
            
            
        }

        //掉落东西
        void HandleDropItem()
        {
            GameObject go = new GameObject();
            go = Instantiate(dropGameObject) as GameObject;
            go.transform.position = new Vector3(transform.position.x, transform.position.y + .5f, transform.position.z);
            player.pickManager.pick_items.Add(go.GetComponent<PickableItem>());
        }

        //更新血条
        void UpdateEnemyHealthUI(int curHealth, int maxHealth)
        {
            if (!isBoss)
                healthBar.rectTransform.sizeDelta = new Vector2((float)curHealth / (float)maxHealth , 0.05f);
            else
            {
                Vector2 targetSize;
                targetSize = new Vector2((float)curHealth / (float)maxHealth * 1000, 20f);

                // 平滑过渡到目标尺寸
                healthBar.rectTransform.sizeDelta = Vector2.Lerp(healthBar.rectTransform.sizeDelta, targetSize, Time.deltaTime * 5);
            }
                

        }
        
        
        /// <summary>
        /// 保存初始状态
        /// </summary>
        public void SaveInitialState()
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            initialHealth = maxHealth;
        }

        /// <summary>
        /// 重置敌人状态
        /// </summary>
        public void ResetState()
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            
            this.lockOnGameObject.SetActive(false);
            this.enemyCanvas.SetActive(false);

            health = initialHealth;
            isDead = false;
            damaged = false;

            // 重置动画
            if (anim != null)
            {
                anim.Rebind();
                anim.Update(0f);
            }

            // 重置导航代理
            if (agent != null)
            {
                agent.enabled = false; // 临时禁用
                agent.enabled = true;
            }

            // 重置布娃娃
            if (rigid != null)
            {
                rigid.isKinematic = true;
                rigid.velocity = Vector3.zero;
            }
        }
    }
}
