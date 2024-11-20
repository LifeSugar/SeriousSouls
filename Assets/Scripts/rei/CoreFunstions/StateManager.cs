using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace rei
{
    public class StateManager : MonoBehaviour
    {
        [Header("Init")]
        public GameObject activeModel;
        public Image damageImage;

        [Header("Character Stats")]
        public Attributes attributes;
        public CharacterStats characterStats;
        public WeaponStats weaponStats;

        [Header("Inputs")]
        public float vertical;
        public float horizontal;
        public float moveAmount;
        public Vector3 moveDir;
        public bool rt, rb, lt, lb;
        public bool rollInput;
        public bool itemInput;


        [Header("Stats")]
        public float moveSpeed = 3f;
        public float rotateSpeed = 5f;
        public float toGround = 0.5f;
        public float rollSpeed = 1;
        public float parryOffset = 1.4f;
        public float backstabOffset = 1.4f;

        [Header("States")]
        public bool run;
        public bool onGround;
        public bool lockOn;
        public bool inAction;
        public bool canMove;
        public bool canRotate;
        public bool canAttack;
        public bool isSpellCasting;
        public bool enableIK;
        public bool isTwoHanded;
        public bool usingItem;
        public bool isBlocking;
        public bool isLeftHand;
        public bool canBeParried;
        public bool parryIsOn;
        public bool onEmpty;
        public bool isInvincible;
        public bool damaged;
        public bool isDead = false;


        [Header("Others")]
        public EnemyTarget lockOnTarget;
        public Transform lockOnTransform;
        public AnimationCurve roll_curve;

        [HideInInspector]
        public Animator anim;
        [HideInInspector]
        public Rigidbody rigid;
        [HideInInspector]
        public AnimatorHook a_hook;
        [HideInInspector]
        public ActionManager actionManager;
        [HideInInspector]
        public InventoryManager inventoryManager;
        [HideInInspector]
        public PickableItemsManager pickManager;
        [HideInInspector]
        public AudioSource audio_source;
        [HideInInspector]
        public AudioClip audio_clip;
        // public SceneManager sceneController;


        public float delta;
        [HideInInspector]
        public LayerMask ignoreLayers;

        [HideInInspector]
        public Action currentAction;


        [HideInInspector]
        public ActionInput storeActionInput;
        public ActionInput storePreviousAction;

        float _actionDelay;
        float flashSpeed = 5f;
        Color flashColour = new Color(1f, 0f, 0f, 0.1f);

         public void Init()
        {
            Debug.Log("StateManager init");
            SetUpAnimator();
            rigid = GetComponent<Rigidbody>();
            rigid.angularDrag = 999;
            rigid.drag = 4;
            rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            
            // Debug.Log("Start to set inventory");
            inventoryManager = GetComponent<InventoryManager>();
            if (inventoryManager == null)
                Debug.Log("No inventory manager");
            inventoryManager.Init(this);
            // Debug.Log("inventory manager initialized");

            actionManager = GetComponent<ActionManager>();
            if (actionManager == null)
                Debug.Log("No action manager");
            actionManager.Init(this);

            pickManager = GetComponent<PickableItemsManager>();
            if (pickManager == null)
                Debug.Log("No pick manager");

            a_hook = activeModel.GetComponent<AnimatorHook>();
            if (a_hook == null)
                // add AnimatorHook component to the active model.
                a_hook = activeModel.AddComponent<AnimatorHook>();

            a_hook.Init(this); //Es: null

            audio_source = activeModel.GetComponent<AudioSource>();

            // ignore the damage collider LayerMask when in contact.
            gameObject.layer = 8;
            ignoreLayers = ~(1 << 9);

            anim.SetBool("onGround", true);

            characterStats.InitCurrent();

            UIManager ui = UIManager.instance;
            ui.AffectAll(characterStats.hp, characterStats.fp, characterStats.stamina);
            ui.InitSouls(characterStats._souls);

            DialogueManager.instance.Init(this.transform);
        }

        void SetUpAnimator()
        {
            if (activeModel == null)
            {
                // get the animator of the model object under controller
                anim = GetComponentInChildren<Animator>();

                if (anim == null)
                    Debug.Log("No model found");
                // get the game object that contains the animator.
                else
                    activeModel = anim.gameObject;
            }

            if (anim == null)
                anim = activeModel.GetComponent<Animator>();

            anim.applyRootMotion = false;
            Debug.Log("Animator setup done");
        }


        //--------------------runner------------------------
        public void FixedTick(float d)
        {
            // 如果正在计时
            if (isTiming)
            {
                actionTimer += d; // 累加计时器

                // 如果超过（）秒未调用 DetectAction，则重置 actionIndex
                if (actionTimer >= resetTime)
                {
                    actionManager.actionIndex = 0; // 重置动作索引
                    isTiming = false;             // 停止计时
                }
            }
            
            // onGround = OnGround();
            
            delta = d;
            isBlocking = false;
            rigid.constraints &= ~RigidbodyConstraints.FreezePositionY;

            //-----------------Handle Actions and Interactions and States--------------------

            //_________Attack (inAction)_______ 
            if (onGround == true)
            {
                usingItem = anim.GetBool("interacting");
                anim.SetBool("spellcasting", isSpellCasting);
                if (inventoryManager.rightHandWeapon != null)
                    inventoryManager.rightHandWeapon.weaponModel.SetActive(!usingItem);
                if (inventoryManager.curConsumable != null)
                {
                    if (inventoryManager.curConsumable.itemModel != null)
                        inventoryManager.curConsumable.itemModel.SetActive(usingItem);
                }



                if (isBlocking == false && isSpellCasting == false)
                    enableIK = false;

                if (inAction)
                { //"inAction" evaluation.
                    anim.applyRootMotion = true;
                    _actionDelay += delta;
                    if (_actionDelay > 0.3f)
                    { // Make Room: if the action is more than 0.3s, reset it again to make room for another action to take place.
                        inAction = false;
                        _actionDelay = 0;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            //_____Start of States______
            onEmpty = anim.GetBool("OnEmpty");
            //canMove = anim.GetBool("canMove");

            if (onEmpty)
            {
                canMove = true;
                canAttack = true;
            }

            if (canRotate)
                HandleRotation();

            if (!onEmpty && !canMove && !canAttack) //Stop updating when all of the state managing variable is false (most likely the character is inAction)
                return;

            if (canMove && !onEmpty)
            {
                if (moveAmount > 0)
                {
                    anim.CrossFade("Empty Override", 0.1f);
                    onEmpty = true;
                }
            }

            if (canAttack)
                DetectAction();
            if (canMove)
                DetectItemAction();

            // turn off RootMotion after the animation is played.
            anim.applyRootMotion = false;

            if (inventoryManager.blockCollider.gameObject.activeSelf) {
                isInvincible = true;
            }

            //_____End of States_____

            // --------Handle movement and physics-------
            //physics
            if (moveAmount > 0 || !onGround)
            {
                rigid.drag = 0;
            }
            else
                rigid.drag = 4;

            //movement
            if (usingItem || isSpellCasting)
            {
                run = false;
                moveAmount = Mathf.Clamp(moveAmount, 0, 0.5f);
            }

            if (onGround && canMove)
                rigid.velocity = moveDir * (moveSpeed * moveAmount);
            

            if (run)
            {
                moveSpeed = 5.5f;
                lockOn = false;
            }
            else
            {
                moveSpeed = 3f;
            }

            HandleRotation();

            // ------Handle movement's animations------
            anim.SetBool("lockon", lockOn);
            if (lockOn == false)
                HandleMovementAnimation();
            else
                HandleLockOnAnimations(moveDir);

            //anim.SetBool("blocking", isBlocking);
            anim.SetBool("isLeft", isLeftHand);

            //________________________
            a_hook.useIK = enableIK;
            HandleBlocking();

            if (isSpellCasting)
            {
                HandleSpellCasting();
                return;
            }
            //_________________________

            //Rolls (inAction)
            a_hook.CloseRoll();
            if (onGround == true)
                HandleRolls();

            //_________________________
            if (lockOn == false)
                lockOnTarget = null;
            if (lockOnTarget != null)
            {
                lockOnTarget.isLockOn = true;
            }
        }

        float i_timer;

        public void Tick(float d)
        {
            delta = d;
            onGround = OnGround();
            anim.SetBool("onGround", onGround);
            pickManager.Tick();
            if (isInvincible)
            {
                i_timer += delta;
                if (i_timer > 0.5f)
                {
                    i_timer = 0;
                    isInvincible = false;
                }
            }

            if (damaged)
            {
                damageImage.color = flashColour;
            }
            else
            {
                damageImage.color = Color.Lerp(damageImage.color, Color.clear, flashSpeed * Time.deltaTime);
            }
            damaged = false;
        }

        public bool IsInput()
        {
            if (rt || rb || lt || lb || rollInput)
                return true;
            return false;
        }


        //-----------------definer------------------------
        void HandleRotation()
        {
            //Handle rotations. (base on directions) (5)
            Vector3 targetDir = (lockOn == false) ? moveDir
                : (lockOnTransform != null) ? lockOnTransform.position - transform.position
                : moveDir;
            targetDir.y = 0;
            if (targetDir == Vector3.zero)
                targetDir = transform.forward;

            Quaternion targetRot = Quaternion.LookRotation(targetDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, delta * moveAmount * rotateSpeed);
        }

        //************Item**********************
        public void DetectItemAction()
        {
            if (onEmpty == false || usingItem || isBlocking)
                return;

            if (itemInput == false)
                return;

            if (inventoryManager.curConsumable == null)
                return;

            if (inventoryManager.curConsumable.itemCount < 1 && inventoryManager.curConsumable.unlimitedCount == false)
                return;

            RuntimeConsumable slot = inventoryManager.curConsumable;

            string targetAnim = slot.instance.targetAnim;
            audio_clip = ResourceManager.instance.GetAudio(slot.instance.audio_id).audio_clip;
            if (string.IsNullOrEmpty(targetAnim))
                return;

            usingItem = true;
            anim.Play(targetAnim);

        }

        public void InteractLogic()
        {
            if (pickManager.interactionCandidate.actionType == UIActionType.talk)
            {
                audio_source.PlayOneShot(ResourceManager.instance.GetAudio("hello").audio_clip);
                pickManager.interactionCandidate.InteractActual();
                return;
            }


            Interactions interaction = ResourceManager.instance.GetInteraction(pickManager.interactionCandidate.interactionId);

            if (interaction.oneShot)
            {
                if (pickManager.interactions.Contains(pickManager.interactionCandidate))
                {
                    pickManager.interactions.Remove(pickManager.interactionCandidate);
                }
            }

            Vector3 targetDir = pickManager.interactionCandidate.transform.position - transform.position;
            SnapToRotation(targetDir);

            pickManager.interactionCandidate.InteractActual();

            PlayAnimation(interaction.anim);
            pickManager.interactionCandidate = null;
        }

        public void SnapToRotation(Vector3 dir)
        {
            dir.Normalize();
            dir.y = 0;
            if (dir == Vector3.zero)
                dir = transform.forward;
            Quaternion t = Quaternion.LookRotation(dir);
            transform.rotation = t;
        }

        public void PlayAnimation(string targetAnim)
        {

            onEmpty = false;
            canMove = false;
            canAttack = false;
            inAction = true;
            isBlocking = false;
            anim.CrossFade(targetAnim, 0.2f);
        }

        //**********Actions*********************
        
        private float actionTimer = 0f; // 计时器
        private bool isTiming = false; // 是否正在计时
        private float resetTime = 0.9f;  // 1.1秒后重置时间
        public void DetectAction()
        {
            // if cannot move, exit the function
            if (canAttack == false && (onEmpty == false || usingItem || isSpellCasting))
                return;

            if (rb == false && rt == false && lt == false && lb == false)
                return;

            if (characterStats._stamina <= 8)
                return;

            ActionInput targetInput = actionManager.GetActionInput(this);
            storeActionInput = targetInput;
            if (onEmpty == false)
            {
                a_hook.killDelta = true;
                targetInput = storePreviousAction;
            }


            storePreviousAction = targetInput;
            Action slot = actionManager.GetActionFromInput(targetInput);

            if (slot == null)
                return;
            switch (slot.type)
            {
                case ActionType.attack:
                    AttackAction(slot);
                    break;
                case ActionType.block:
                    BlockAction(slot);
                    break;
                case ActionType.spells:
                    SpellAction(slot);
                    break;
                case ActionType.parry:
                    ParryAction(slot);
                    break;
            }
            
            isTiming = true; // 开始计时
            actionTimer = 0f; // 重置计时器
        }

        void AttackAction(Action slot)
        {
            // 如果角色体力不足，则无法执行攻击，直接返回
            if (characterStats._stamina < slot.staminaCost)
                return;

            // 检查是否可以执行格挡反击（Parry），如果成功触发，则直接返回
            if (CheckForParry(slot))
                return;

            // 检查是否可以执行背刺（BackStab），如果成功触发，则直接返回
            if (CheckForBackStab(slot))
                return;

            // 初始化目标动画变量
            string targetAnim = null;

            // 获取当前 Action 的步骤（step），并根据输入动作（storeActionInput）获取对应分支动画信息，branch也可以处理冲刺攻击和蓄力攻击等情况，再说吧
            ActionAnim branch = slot.GetActionStep(ref actionManager.actionIndex).GetBranch(storeActionInput);

            // 从分支中获取目标动画名称
            targetAnim = branch.targetAnim;

            // 获取动画对应的音频资源并保存到 `audio_clip` 中
            if (audio_clip != null)
            {
                audio_clip = ResourceManager.instance.GetAudio(branch.audio_ids).audio_clip;
            }
            

            // 如果目标动画为空或未指定，则直接返回
            if (string.IsNullOrEmpty(targetAnim))
                return;

            // 设置当前的攻击 Action 信息
            currentAction = slot;

            // 是否可以被格挡反击
            canBeParried = slot.canBeParried;

            // 更新角色状态，禁用攻击、移动等其他行为
            canAttack = false; // 禁止再次攻击
            onEmpty = false;   // 表示角色处于“非空闲”状态
            canMove = false;   // 禁止移动
            inAction = true;   // 标记为当前正在执行动作

            // 设置目标动画速度，默认速度为 1
            float targetSpeed = 1;
            if (slot.changeSpeed)
            {
                targetSpeed = slot.animSpeed;
                
                if (targetSpeed == 0)
                    targetSpeed = 1; // 如果动画速度为 0，则将其恢复为默认值 1
            }

            // 将动画速度传递给 Animator，控制动画播放速度
            anim.SetFloat("animSpeed", targetSpeed);

            // 设置 Animator 的“镜像”状态，控制是否反转动画（例如左手或右手攻击）
            anim.SetBool("mirror", slot.mirror);

            // 切换到目标攻击动画，使用 0.2 秒的过渡时间
            anim.CrossFade(targetAnim, 0.2f);

            // 减少角色的体力值，消耗对应攻击的体力成本
            characterStats._stamina -= slot.staminaCost;
            //战技会耗蓝
            characterStats._focus -= slot.fpCost;
            
            //重置输入状态
            rb = false;
            rt = false;
            lb = false;
            lt = false;

            // 注释：没有直接将角色的速度清零（如 `rigid.velocity = Vector3.zero`），
            // 而是在 AnimatorHook 中通过根运动（Root Motion）处理角色的运动速度。
        }

        //格挡
        bool CheckForParry(Action slot)
        {

            //没写

            return false;
        }

        bool CheckForBackStab(Action slot)
        {
            // 检查当前动作是否允许进行背刺攻击
            if (slot.canBackStab == false)
                return false;

            // 初始化一个 EnemyStates 用于存储检测到的敌人对象
            EnemyStates backstab = null;

            // 计算射线起点，从玩家位置向前方发射一条射线
            Vector3 origin = transform.position;
            origin.y += 1; // 提高射线起点，使其处于敌人中心高度
            Vector3 rayDir = transform.forward;
            RaycastHit hit;

            // 发射射线，检查前方是否有敌人
            if (Physics.Raycast(origin, rayDir, out hit, 1, ignoreLayers))
            {
                // 获取射线命中的物体上的 EnemyStates 组件（即敌人）
                backstab = hit.transform.GetComponentInParent<EnemyStates>();
            }

            // 如果没有检测到敌人，返回 false，无法进行背刺
            if (backstab == null)
                return false;

            // 计算玩家到敌人的方向向量，用于检测玩家是否在敌人的背后
            Vector3 dir = transform.position - backstab.transform.position;
            dir.Normalize(); // 归一化方向
            dir.y = 0; // 忽略 Y 轴方向

            // 计算敌人前方向量与玩家方向的角度
            float angle = Vector3.Angle(backstab.transform.forward, dir);

            // 如果角度大于 150 度，表示玩家在敌人后方，可以进行背刺
            if (angle > 150)
            {
                //没写
            }

            // 背刺失败，返回 false
            return false;
        }

        //**************Blocking******************
        bool blockAnim;
        string blockIdleAnim;

        void HandleBlocking()
        {
            if (isBlocking == false)
            {
                
                if (blockAnim)
                {
                    inventoryManager.CloseBlockCollider();
                    anim.CrossFade(blockIdleAnim, 0.1f);
                    blockAnim = false;
                }
            }
        }

        void BlockAction(Action slot)
        {
            isBlocking = true;
            enableIK = true;
            isLeftHand = slot.mirror;
            a_hook.currentHand = (slot.mirror) ? AvatarIKGoal.LeftHand : AvatarIKGoal.RightHand;
            a_hook.InitIKForShield(slot.mirror);
            inventoryManager.OpenBlockCollider();
            
            if (blockAnim == false)
            {
                blockIdleAnim = (isTwoHanded == false) ? inventoryManager.GetCurrentWeapon(isLeftHand).oh_idle : inventoryManager.GetCurrentWeapon(isLeftHand).th_idle;
                blockIdleAnim += (isLeftHand) ? "_l" : "_r";
                string targetAnim = slot.targetAnim;
                targetAnim += (isLeftHand) ? "_l" : "_r";
                anim.CrossFade(targetAnim, 0.1f);
                blockAnim = true;
            }
        }

        void ParryAction(Action slot)
        {
            string targetAnim = null;
            targetAnim = slot.GetActionStep(ref actionManager.actionIndex).GetBranch(slot.input).targetAnim;
            if (string.IsNullOrEmpty(targetAnim))
                return;

            float targetSpeed = 1;
            if (slot.changeSpeed)
            {
                targetSpeed = slot.animSpeed;
                if (targetSpeed == 0)
                    targetSpeed = 1;
            }

            anim.SetFloat("animSpeed", targetSpeed);

            // exit the function after playing the animation.
            canBeParried = slot.canBeParried;

            canAttack = false;
            onEmpty = false;
            canMove = false; // canMove = false to use the RootMotion.
            inAction = true;
            anim.SetBool("mirror", slot.mirror);
            anim.CrossFade(targetAnim, 0.2f);
        }

        //*****************Spells*****************

        float cur_focusCost;
        float cur_staminaCost;
        float spellCastTime;
        float max_spellCastTime;
        string spellTargetAnim;
        bool spellIsMirrored;
        GameObject projectileCandidate;
        SpellType curSpellType;

        public delegate void SpellCast_Start();
        public delegate void SpellCast_Loop();
        public delegate void SpellCast_Stop();
        public SpellCast_Start spellCast_start;
        public SpellCast_Loop spellCast_loop;
        public SpellCast_Stop spellCast_stop;

        void SpellAction(Action slot)
        { // slot is from the ActionManager

            //没写22
        }

        void HandleSpellCasting()
        {
            //没写
        }

        public void ThrowProjectile()
        { // will be called in AnimatorHook.cs
            if (projectileCandidate == null)
                return;

            GameObject g0 = Instantiate(projectileCandidate) as GameObject;
            Transform p = anim.GetBoneTransform((spellIsMirrored) ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
            g0.transform.position = p.position;


            if (lockOnTransform && lockOn) {
                Vector3 v = lockOnTransform.position;
                v.y += 1f;
                g0.transform.LookAt(v);
            }     
            else
                g0.transform.rotation = transform.rotation;

            Projectile proj = g0.GetComponent<Projectile>();
            proj.Init();
            characterStats._stamina -= cur_staminaCost;
            characterStats._focus -= cur_focusCost;
        }


        //************Locomotions*****************

        void HandleRolls()
        {
            if (!rollInput || usingItem || characterStats._stamina < 10)
                return;
            // will roll instantly, no delay.
            float v = vertical;
            float h = horizontal;
            v = (moveAmount > 0.3f) ? 1 : 0;
            h = 0;

            // Direction.
            // rotate the target to the rolling direction to match the animation at the end
            //, thus when the animation is ended, the target is set to rotate to the matching direction.
            if (v != 0)
            {
                if (moveDir == Vector3.zero)
                    moveDir = transform.forward;

                transform.rotation = Quaternion.LookRotation(moveDir);

                a_hook.InitForRoll();
                a_hook.rm_multi = rollSpeed;
            }
            else
            {
                a_hook.rm_multi = 1.3f;
            }

            // setting input values.
            anim.SetFloat("vertical", v);
            anim.SetFloat("horizontal", h);

            //To run the inAction evaluation:
            canAttack = false;
            onEmpty = false;
            canMove = false;
            inAction = true;
            anim.CrossFade("Rolls", 0.2f);
            isInvincible = true;
            isBlocking = false;
            characterStats._stamina -= 25f;
        }

        void HandleMovementAnimation()
        {
            anim.SetBool("run", run);
            anim.SetFloat("vertical", moveAmount, 0.4f, delta);
            anim.SetBool("onGround", onGround);
        }

        void HandleLockOnAnimations(Vector3 moveDir)
        {
            Vector3 relativeDir = transform.InverseTransformDirection(moveDir);
            float h = relativeDir.x;
            float v = relativeDir.z;

            if (usingItem || isSpellCasting)
            {
                run = false;
                v = Mathf.Clamp(v, -0.7f, 0.6f);
                h = Mathf.Clamp(h, -0.6f, 0.6f);
            }

            anim.SetFloat("vertical", v, 0.2f, delta);
            //horizontal in this state can be set.
            anim.SetFloat("horizontal", h, 0.2f, delta);
        }

        public bool OnGround()
        {
            // 初始化返回值为 false，表示默认情况下不在地面上
            bool r = false;

            // 从对象位置向上移动一定距离（`toGround`）的起点
            Vector3 origin = transform.position + (Vector3.up * toGround);

            // 射线的方向为向下
            Vector3 dir = -Vector3.up;

            // 射线的长度为 `toGround` + 0.2f，多增加了 0.2f 是为了稍微超出地面范围
            float dis = toGround + 0.2f;

            // 用于存储射线检测的结果
            RaycastHit hit;

            // 在 Scene 视图中画出射线，用于调试（可视化检测）
            Debug.DrawRay(origin, dir * dis, Color.cyan);

            // 使用物理射线检测（从 origin 向 dir 方向发射射线，长度为 dis，忽略指定的层）
            if (Physics.Raycast(origin, dir, out hit, dis, ignoreLayers))
            {
                // 如果射线检测到碰撞，说明对象处于地面上
                r = true;

                // 将对象位置调整到射线碰撞点（即地面高度）
                Vector3 targetPosition = hit.point;
                transform.position = targetPosition;
            }

            // 返回是否在地面上的检测结果
            return r;
        }

        //***********TwoHanded********************

        public void HandleTwoHanded()
        {
            bool isRight = true;
            if (inventoryManager.rightHandWeapon == null)
                return;

            Weapon w = inventoryManager.rightHandWeapon.instance;
            if (w == null)
            {
                w = inventoryManager.leftHandWeapon.instance;
                isRight = false;
            }
            if (w == null)
                return;


            if (isTwoHanded)
            {
                anim.CrossFade(w.th_idle, 0.2f);
                actionManager.UpdateActionsTwoHanded();
                if (isRight)
                {
                    if (inventoryManager.leftHandWeapon)
                        inventoryManager.leftHandWeapon.weaponModel.SetActive(false);
                }
                else
                {
                    if (inventoryManager.rightHandWeapon)
                        inventoryManager.rightHandWeapon.weaponModel.SetActive(false);
                }
            }

            else
            {
                //string targetAnim = w.oh_idle;
                //targetAnim += (isRight) ? "_r" : "_l";
                //anim.CrossFade(targetAnim, 0.2f);
                anim.Play("Equip Weapon");
                actionManager.UpdateActionsOneHanded();

                if (isRight)
                {
                    if (inventoryManager.leftHandWeapon)
                        inventoryManager.leftHandWeapon.weaponModel.SetActive(true);
                }
                else
                {
                    if (inventoryManager.rightHandWeapon)
                        inventoryManager.rightHandWeapon.weaponModel.SetActive(true);
                }
            }

        }

        //**********StatsMonitor*****************

        public void AddHealth()
        {
            characterStats.fp++;
        }

        public void MonitorStats()
        {
            if (run & moveAmount > 0)
            {
                characterStats._stamina -= delta * 17;
            }
            else
            {
                characterStats._stamina += delta * 15;
            }

            characterStats._health = Mathf.Clamp(characterStats._health, 0, characterStats.hp);
            characterStats._focus = Mathf.Clamp(characterStats._focus, 0, characterStats.fp);
            characterStats._stamina = Mathf.Clamp(characterStats._stamina, 0, characterStats.stamina);
        }

        public void SubstractStaminaOverTime()
        {
            characterStats._stamina -= cur_staminaCost;
        }

        public void SubstractFocusOverTime()
        {
            characterStats._focus -= cur_focusCost;
        }

        public void EffectBlocking()
        {
            isBlocking = true;
        }

        public void StopEffectBlocking()
        {
            isBlocking = false;
        }

        // public void DoDamage(AIAttacks a)
        // {
        //     if (isInvincible)
        //         return;
        //     damaged = true;
        //
        //     int damage = 20;
        //
        //     characterStats._health -= damage;
        //     if (canMove)
        //     {
        //         int ran = Random.Range(0, 100);
        //         string tA = (ran > 50) ? "damage_1" : "damage_2";
        //         audio_clip = ResourceManager.instance.GetAudio("hurt").audio_clip;
        //         anim.Play(tA);
        //     }
        //     anim.SetBool("OnEmpty", false);
        //     onEmpty = false;
        //     isInvincible = true;
        //     anim.applyRootMotion = true;
        //     anim.SetBool("canMove", false);
        //     if (characterStats._health <= 0 && !isDead) {
        //         Die();
        //     }
        // }

        public void Die() {
            isDead = true;
            isInvincible = true;
            // StartCoroutine(sceneController.HandleGameOver());
        }
    }
    
}