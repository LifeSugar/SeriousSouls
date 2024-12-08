using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace rei
{
    /// <summary>
    /// 用于控制玩家的核心脚本
    /// </summary>
    public class PlayerState : MonoBehaviour
    {
        #region 必要参数

        [Header("Init")] public GameObject activeModel; // 当前使用的玩家模型（带动画）
        public Image damageImage; // 受伤时的屏幕特效 UI

        [Header("Character Stats")] public Attributes attributes; // 玩家装备相关属性
        public CharacterStats characterStats; // 玩家当前属性（如生命、体力）
        public WeaponStats weaponStats; // 当前武器的属性（用于攻击）

        [Header("Inputs")] public float vertical; // 垂直方向输入值（W/S 或摇杆）
        public float horizontal; // 水平方向输入值（A/D 或摇杆）
        public float moveAmount; // 移动量（结合水平和垂直输入计算）
        public Vector3 moveDir; // 移动方向向量
        public bool rt, rb, lt, lb; // 按键输入（RT/RB/左手攻击等）
        public bool rollInput; // 翻滚输入
        public bool itemInput; // 使用物品输入
        public Vector3 rollDir; // 翻滚方向（用于调试）

        [Header("Stats")] public float moveSpeed = 3f; // 角色移动速度
        public float rotateSpeed = 5f; // 旋转速度
        public float toGround = 0.5f; // 地面检测距离
        public float rollSpeed = 1; // 翻滚速度
        public float rollCost = 50f; // 翻滚消耗的体力值
        public float parryOffset = 1f; // 招架动作位置偏移
        public float backstabOffset = 1f; // 背刺动作位置偏移

        [Header("States")]
        // 角色各种状态标记，用于控制行为和逻辑
        public bool run; // 是否在跑步

        public bool onGround; // 是否在地面上
        public bool lockOn; // 是否锁定目标
        public bool inAction; // 是否正在执行动作
        public bool canMove; // 是否可以移动
        public bool canRotate; // 是否可以旋转
        public bool canAttack; // 是否可以攻击
        public bool isSpellCasting; // 是否正在施法
        public bool unfreezeY; //这个值可以通过动作的animation event来解锁，实现一些根运动上天的动作
        public bool enableIK; // 是否启用 IK（逆向动力学）
        public bool isTwoHanded; // 是否使用双手模式
        public bool usingItem; // 是否正在使用道具
        public bool isBlocking; // 是否处于格挡状态
        public bool isLeftHand; // 是否使用左手攻击
        public bool canBeParried; // 是否可以被招架
        public bool parryIsOn; // 是否正在进行招架反击
        public bool onEmpty; // 是否处于空闲状态
        public bool hurt; // 是否受到伤害
        public bool isInvincible; // 是否无敌（如翻滚时）
        public bool damaged; // 是否被击中，用于触发 UI 特效
        public bool isDead = false; // 是否死亡

        [Header("Others")] public EnemyTarget lockOnTarget; // 当前锁定的敌人目标
        public Transform lockOnTransform; // 锁定目标的位置

        [Header("Rolls")] public AnimationCurve roll_curve; // 翻滚动作的曲线控制
        public float rollDuration = 0.6f; // 翻滚持续时间

        [Header("Debug")] public float rbvz; // 调试用变量（刚体速度 z 分量）
        public float rbvx; // 调试用变量（刚体速度 x 分量）

        [HideInInspector]
        // 通过代码动态获取的组件和管理器
        public Animator anim; // 动画控制器

        public Rigidbody rigid; // 刚体，用于物理模拟
        public AnimatorHook a_hook; // 动画钩子，用于处理动画事件
        public ActionManager actionManager; // 动作管理器
        public InventoryManager inventoryManager; // 背包管理器
        public PickableItemsManager pickManager; // 可拾取物品管理器
        public AudioSource audio_source; // 音频源组件
        public AudioClip audio_clip; // 当前音频剪辑
        // public SceneManager sceneController; // 场景管理器（注释掉）

        public float delta; // 每帧时间间隔（`FixedUpdate` 用）
        [HideInInspector] public LayerMask ignoreLayers; // 用于射线检测时忽略的层级

        [HideInInspector] public Action currentAction; // 当前正在执行的动作

        public ActionInput storeActionInput; // 缓存的动作输入
        public ActionInput storePreviousAction; // 缓存的前一个动作输入

        public float _actionDelay; // 动作延迟计时器
        public float flashSpeed = 2f; // 受伤特效的淡出速度
        public Color flashColour = new Color(1f, 0f, 0f, 0.1f); // 受伤 UI 特效颜色

        [HideInInspector] public bool inSkill;

        #endregion

        #region Initialization

        /// <summary>
        /// Init在InputHandler中被调用，本脚本以及所有的关联脚本：inventory manager，action manager， pick manager， animation hook， UIManager， Dialogue Manager初始化
        /// 并设置自身的layer，设置layermask到敌人的layer
        /// /// </summary>
        public void Init()
        {
            //初始化关联组件和脚本
            Debug.Log("StateManager init");
            SetUpAnimator();
            rigid = GetComponent<Rigidbody>();
            rigid.angularDrag = 9999;
            rigid.drag = 4;
            rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rigid.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            inventoryManager = GetComponent<InventoryManager>();
            if (inventoryManager == null)
                Debug.Log("No inventory manager");
            inventoryManager.Init(this);

            actionManager = GetComponent<ActionManager>();
            if (actionManager == null)
                Debug.Log("No action manager");
            actionManager.Init(this);

            pickManager = GetComponent<PickableItemsManager>();
            if (pickManager == null)
                Debug.Log("No pick manager");

            a_hook = activeModel.GetComponent<AnimatorHook>();
            if (a_hook == null)
                a_hook = activeModel.AddComponent<AnimatorHook>();

            a_hook.Init(this, null); //Es: null

            audio_source = activeModel.GetComponent<AudioSource>();

            //敌人的layer是9，地形的layer是28
            gameObject.layer = 8;
            ignoreLayers = ~(1 << 9);

            //初始状态设定在地面上
            anim.SetBool("onGround", true);

            characterStats.InitCurrent();

            //初始化UI
            UIManager ui = UIManager.instance;
            ui.InitBars(characterStats.hp, characterStats.fp, characterStats.stamina); //初始化红蓝绿条
            ui.InitSouls(characterStats._souls); //初始化魂

            //初始化对话
            DialogueManager.instance.Init(this.transform);
        }

        //初始化动画状态机
        void SetUpAnimator()
        {
            if (activeModel == null)
            {
                anim = GetComponentInChildren<Animator>(); //在子物体中找到状态机
                if (anim == null)
                    Debug.Log("No model found");
                else
                    activeModel = anim.gameObject;
            }

            if (anim == null)
                anim = activeModel.GetComponent<Animator>(); //双重保障

            anim.applyRootMotion = false; //关闭根运动
        }

        #endregion

        #region 生命周期方法

        /// <summary>
        /// 每帧物理更新的核心方法，处理角色的物理模拟和状态更新。
        /// 本方法在 FixedUpdate 中调用，涵盖了包括地面检测、动画状态更新、动作执行等逻辑。
        /// </summary>
        /// <param name="d">时间间隔（`FixedDeltaTime`）</param>
        public void FixedTick(float d)
        {
            // 调试用：记录刚体速度的 x 和 z 分量
            rbvz = rigid.velocity.z;
            rbvx = rigid.velocity.x;

            // 1. 地面检测，更新 `onGround` 状态，并同步到动画参数
            onGround = OnGround(); // 调用 OnGround() 方法判断是否在地面上
            anim.SetBool("onGround", onGround); // 将结果传递给 Animator 参数

            // 2. 更新帧时间
            delta = d;

            // 重置一些状态
            isBlocking = false; // 重置格挡状态
            if (onGround && !unfreezeY)
            {
                // 在地面上且没有特殊要求时，冻结 Y 轴位置
                rigid.constraints |= RigidbodyConstraints.FreezePositionY;
            }
            else
            {
                // ，解除 Y 轴冻结
                rigid.constraints &= ~RigidbodyConstraints.FreezePositionY;
            }
            

            // 3. 重置连续攻击的段数（延迟超过一定时间会重置）
            ResetActionIndex(d);

            // 4. 如果玩家在地面上，更新相关状态
            if (onGround == true)
            {
                // 使用道具状态
                usingItem = anim.GetBool("usingItem");
                anim.SetBool("spellcasting", isSpellCasting); // 更新施法状态

                // 根据当前持有的武器或道具更新显示状态
                if (inventoryManager.rightHandWeapon != null)
                    inventoryManager.rightHandWeapon.weaponModel.SetActive(!usingItem && !inSkill && !anim.GetBool("interacting")); // 如果正在使用道具，交互，使用某些左手lt 隐藏武器
                if (inventoryManager.curConsumable != null && inventoryManager.curConsumable.itemModel != null)
                    inventoryManager.curConsumable.itemModel.SetActive(usingItem); // 如果有当前道具，显示道具模型

                // 更新 IK 和其他动画相关状态
                if (!isBlocking && !isSpellCasting)
                    enableIK = false; // 非格挡、非施法状态禁用 IK
                if (inAction)
                {
                    anim.applyRootMotion = true; // 启用根运动（如翻滚等动作依赖）
                    _actionDelay += delta; // 累加动作延迟计时器
                    if (_actionDelay > 0.3f) // 如果超过 0.3 秒
                    {
                        inAction = false; // 重置动作状态
                        _actionDelay = 0; // 重置计时器
                    }
                    else
                    {
                        return; // 如果还在动作中，退出方法，避免其他逻辑执行
                    }
                }
            }

            // 5. 处理空闲状态和相关能力
            onEmpty = anim.GetBool("OnEmpty") && !anim.GetBool("interacting"); // 检查是否处于空闲状态
            if (onEmpty)
            {
                canMove = true; // 空闲时允许移动
                canAttack = true; // 空闲时允许攻击
            }
            else
            {
                canMove = false; // 非空闲时禁止移动
                canAttack = false; // 非空闲时禁止攻击
            }

            // 6. 如果允许旋转，调用旋转处理方法
            if (canRotate)
                HandleRotation();

            // 如果既不能移动也不能攻击，直接退出方法
            if (!onEmpty && !canMove && !canAttack)
                return;

            // 7. 如果移动输入大于 0 且当前不在空闲状态，则触发动画
            if (canMove && !onEmpty)
            {
                if (moveAmount > 0)
                {
                    anim.CrossFade("Empty Override", 0.1f); // 播放过渡到空闲状态的动画
                    onEmpty = true; // 更新空闲状态
                }
            }

            // 重置玩家输入
            ResetInput();

            // 8. 检测攻击或使用道具的输入
            if (canAttack)
                DetectAction(); // 检测攻击输入
            if (canMove)
                DetectItemAction(); // 检测使用道具输入

            // 动画根运动处理：在动作播放完成后关闭根运动
            anim.applyRootMotion = false;

            // 如果当前处于格挡状态，设置无敌状态
            if (inventoryManager.blockCollider.gameObject.activeSelf)
            {
                isInvincible = true; // 设置无敌
            }

            // 9. 根据状态调整刚体阻力
            if (moveAmount > 0 || !onGround)
            {
                rigid.drag = 0; // 移动或不在地面时降低阻力
            }
            else
            {
                rigid.drag = 4; // 静止时增加阻力
            }

            // 10. 处理移动速度和动画
            if (usingItem || isSpellCasting)
            {
                run = false; // 使用道具或施法时不能跑步
                moveAmount = Mathf.Clamp(moveAmount, 0, 0.5f); // 限制移动速度
            }

            if (onGround && canMove)
                rigid.velocity = moveDir * (moveSpeed * moveAmount); // 更新刚体速度

            if (!onGround)
                run = false; // 不在地面时无法跑步

            if (run)
            {
                moveSpeed = 5.5f; // 跑步速度
                lockOn = false; // 跑步时取消锁定
            }
            else
            {
                moveSpeed = 3f; // 默认移动速度
            }

            // 调用旋转处理逻辑
            HandleRotation();

            // 11. 更新动画状态
            anim.SetBool("lockon", lockOn); // 更新锁定状态到动画
            if (!lockOn)
                HandleMovementAnimation(); // 普通移动动画
            else
                HandleLockOnAnimations(moveDir); // 锁定目标时的移动动画

            anim.SetBool("isLeft", isLeftHand); // 设置是否左手持武

            // 更新 IK 状态
            a_hook.useIK = enableIK;

            // 12. 处理格挡、施法及翻滚逻辑
            HandleBlocking();
            if (isSpellCasting)
            {
                HandleSpellCasting();
                return; // 如果施法中，退出方法
            }

            // 翻滚逻辑
            a_hook.CloseRoll(); // 关闭翻滚状态
            if (onGround)
                HandleRolls(); // 处理翻滚逻辑

            // 13. 更新锁定目标
            if (!lockOn)
                lockOnTarget = null; // 没有锁定目标
            if (lockOnTarget != null)
                lockOnTarget.isLockOn = true; // 更新锁定目标的状态
        }

        float i_timer;

        /// <summary>
        /// 每帧更新方法（在 Update 中调用），主要负责处理非物理相关的逻辑：
        /// - 处理道具拾取逻辑
        /// - 无敌状态计时
        /// - 受伤特效 UI 的显示和淡出
        /// - 重置 `damaged` 状态
        /// </summary>
        /// <param name="d">帧时间间隔（`Time.deltaTime`）</param>
        public void Tick(float d)
        {
            // 更新 delta 时间（每帧的时间间隔）
            delta = d;

            // 1. 调用拾取物品管理器的逻辑
            pickManager.Tick();

            // 2. 如果角色处于无敌状态，开始计时
            if (isInvincible)
            {
                i_timer += delta; // 累加计时器
                if (i_timer > 0.5f) // 如果无敌时间超过 0.5 秒
                {
                    i_timer = 0; // 重置计时器
                    isInvincible = false; // 解除无敌状态
                }
            }

            // 3. 如果角色受到伤害（`damaged` 为 true），触发受伤特效
            if (damaged)
            {
                damageImage.color = flashColour; // 将受伤特效 UI 颜色设置为 `flashColour`
            }
            else
            {
                // 如果未受伤，逐渐将 UI 颜色淡化到透明
                damageImage.color = Color.Lerp(damageImage.color, Color.clear, flashSpeed * Time.deltaTime);
            }

            // 4. 重置 `damaged` 状态
            damaged = false; // 每帧结束时，将 `damaged` 状态重置为 false，准备下一次伤害检测
            
        }

        #endregion

        /// <summary>
        /// 检测玩家输入状态，用于判断当前是否有输入操作。
        /// 返回值为 `true` 表示玩家正在进行输入操作（如攻击、翻滚）。
        /// </summary>
        /// <returns>
        /// 如果玩家按下任意一个关键输入（RT、RB、LT、LB 或翻滚键），返回 `true`；否则返回 `false`。
        /// </returns>
        public bool IsInput()
        {
            // 检查以下按键输入是否为真
            if (rt || rb || lt || lb || rollInput)
                return true; // 如果任意一个输入为真，返回 true，表示有输入
            return false; // 如果所有输入为假，返回 false，表示没有输入
        }


        /// <summary>
        /// 处理角色的旋转逻辑，根据输入方向或锁定目标调整角色面向方向。
        /// </summary>
        void HandleRotation()
        {
            // 1. 确定目标方向（Target Direction）
            Vector3 targetDir = (lockOn == false) 
                ? moveDir // 如果没有锁定目标，则使用移动方向
                : (lockOnTransform != null) 
                    ? lockOnTransform.position - transform.position // 如果锁定目标存在，则指向目标位置
                    : moveDir; // 如果没有目标，但处于锁定状态，则保持移动方向
            targetDir.y = 0; // 忽略 Y 轴（仅在水平平面内旋转）

            // 如果目标方向为零（未输入任何方向），保持当前朝向
            if (targetDir == Vector3.zero)
                targetDir = transform.forward;

            // 2. 计算目标旋转（Target Rotation）
            Quaternion targetRot = Quaternion.LookRotation(targetDir); // 根据目标方向计算目标旋转

            // 3. 插值旋转（Slerp Rotation）
            transform.rotation = Quaternion.Slerp(
                transform.rotation,  // 当前旋转
                targetRot,           // 目标旋转
                delta * moveAmount * rotateSpeed // 根据输入量和旋转速度进行平滑插值
            );
        }

        //************Item**********************
        /// <summary>
        /// 检测玩家使用物品的输入并处理道具使用逻辑。
        /// 如果玩家满足使用条件，则触发道具使用动画和效果。
        /// </summary>
        public void DetectItemAction()
        {
            // 1. 检查是否满足使用条件（状态检查）
            if (onEmpty == false || usingItem || isBlocking) 
                return; // 如果角色不在空闲状态、正在使用物品或正在格挡，则直接返回

            // 2. 检查是否有使用物品的输入
            if (itemInput == false)
                return; // 如果没有触发使用物品的输入，则直接返回

            // 3. 检查是否有当前选中的消耗品
            if (inventoryManager.curConsumable == null)
                return; // 如果当前没有选中的消耗品，则直接返回

            // 4. 检查道具是否还有库存
            if (inventoryManager.curConsumable.itemCount < 1 && 
                inventoryManager.curConsumable.unlimitedCount == false)
                return; // 如果道具库存为零且不是无限道具，则直接返回

            // 5. 获取当前消耗品槽的信息
            RuntimeConsumable slot = inventoryManager.curConsumable;

            // 6. 检查道具是否有指定的使用动画
            string targetAnim = slot.instance.targetAnim; // 获取消耗品的目标动画名称
            if (string.IsNullOrEmpty(targetAnim)) 
                return; // 如果没有指定动画，则直接返回

            // 7. 设置状态为正在使用物品
            usingItem = true;

            // 8. 播放使用物品的动画
            anim.Play(targetAnim); // 使用 Animator 播放目标动画
        }

        /// <summary>
        /// 处理玩家的交互逻辑，包括与 NPC 对话和触发场景交互事件。
        /// </summary>
        public void InteractLogic()
        {
            // 1. 检查交互目标是否是 NPC（对话类型）
            if (pickManager.interactionCandidate.actionType == UIActionType.talk)
            {
                // 播放对话音效（如果有对应资源）
                // audio_source.PlayOneShot(ResourceManager.instance.GetAudio("hello").audio_clip);

                // 执行实际的对话逻辑
                pickManager.interactionCandidate.InteractActual();
                return; // 完成对话逻辑后直接返回
            }

            // 2. 获取交互目标的交互信息
            Interactions interaction = ResourceManager.instance.GetInteraction(pickManager.interactionCandidate.interactionId);

            // 3. 如果交互类型是一次性交互（如开门等）
            if (interaction.oneShot)
            {
                // 如果当前交互目标已在交互列表中，移除它
                if (pickManager.interactions.Contains(pickManager.interactionCandidate))
                {
                    pickManager.interactions.Remove(pickManager.interactionCandidate);
                }
            }

            // 4. 将玩家朝向交互目标
            Vector3 targetDir = pickManager.interactionCandidate.transform.position - transform.position; // 计算方向向量
            SnapToRotation(targetDir); // 调用方法，立即将玩家旋转到目标方向

            

            // 6. 播放交互动画
            PlayAnimation(interaction.anim);
            
            // 5. 执行交互目标的实际交互逻辑
            pickManager.interactionCandidate.InteractActual();

            // 7. 清除当前交互候选对象
            pickManager.interactionCandidate = null;
        }

        /// <summary>
        /// 立即将角色旋转到指定方向，忽略插值和平滑效果。
        /// 用于需要快速调整角色朝向的场景（如交互或背刺）。
        /// </summary>
        /// <param name="dir">目标方向向量（通常是交互目标的位置减去角色当前位置）。</param>
        public void SnapToRotation(Vector3 dir)
        {
            // 1. 归一化目标方向向量
            dir.Normalize(); // 将方向向量标准化，确保长度为 1
            dir.y = 0; // 忽略 Y 轴方向，仅处理水平旋转

            // 2. 如果目标方向为零向量，保持当前朝向
            if (dir == Vector3.zero)
                dir = transform.forward; // 使用角色当前的前向向量作为默认方向

            // 3. 根据目标方向计算目标旋转
            Quaternion t = Quaternion.LookRotation(dir); // 创建一个面向目标方向的四元数

            // 4. 立即将角色的旋转设置为目标旋转
            transform.rotation = t; // 直接修改角色的 `rotation` 属性
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


        [Header("ActionsTimer")] public float actionTimer = 0f; // 计时器
        public bool isTiming = false; // 是否正在计时
        public float resetTime = 1.2f; // 1.1秒后重置时间

        /// <summary>
        /// 检测玩家的攻击输入，并根据当前输入决定执行何种动作（攻击、格挡、施法等）。
        /// 负责调用与动作相关的具体逻辑（如攻击或技能释放）。
        /// 我这里写的并不好
        /// </summary>
        public void DetectAction()
        {
            // 1. 如果当前状态不允许攻击，直接返回
            if (canAttack == false && (onEmpty == false || usingItem || isSpellCasting))
                return; // 如果不能攻击或当前非空闲状态、正在使用物品或施法，直接退出

            // 2. 检查是否有任何攻击输入
            if (rb == false && rt == false && lt == false && lb == false)
                return; // 如果所有攻击相关的输入按键都未按下，直接退出

            // 3. 检查体力是否足够
            // if (characterStats._stamina <=
            //     actionManager.GetActionFromInput(actionManager.GetActionInput(this)).staminaCost ||
            //     characterStats._focus <=
            //     actionManager.GetActionFromInput(actionManager.GetActionInput(this)).staminaCost)
            // {
            //     return; // 如果当前体力不足以执行动作，直接退出
            // }
                

            // 4. 获取当前动作输入并保存
            ActionInput targetInput = actionManager.GetActionInput(this); // 获取动作输入
            storeActionInput = targetInput; // 保存当前输入，用于后续逻辑

            // 5. 如果正在连续动作中，尝试延续前一个动作
            if (onEmpty == false)
            {
                a_hook.killDelta = true; // 动画钩子设置为立即触发下一个动作
                targetInput = storePreviousAction; // 延续前一个动作
            }

            // 6. 如果体力不足以延续前一个动作，退出
            if ((characterStats._stamina <= 
                actionManager.GetActionFromInput(storePreviousAction).staminaCost || characterStats._focus <= actionManager.GetActionFromInput(storePreviousAction).fpCost) && actionManager.GetActionFromInput(targetInput).staminaCost > characterStats._stamina )
            {
                Debug.Log("actionquit"); // 输出调试信息
                return; // 直接退出动作检测
            }

            // 7. 保存当前动作为前一个动作
            storePreviousAction = targetInput;

            // 8. 获取与输入对应的动作信息
            Action slot = actionManager.GetActionFromInput(targetInput); 
            if (slot == null)
                return; // 如果找不到对应的动作，直接退出

            // 9. 根据动作类型执行不同逻辑
            switch (slot.type)
            {
                case ActionType.attack:
                    AttackAction(slot); // 执行攻击逻辑
                    break;
                case ActionType.block:
                    BlockAction(slot); // 执行格挡逻辑
                    break;
                case ActionType.spells:
                    SpellAction(slot); // 执行施法逻辑
                    break;
                case ActionType.parry:
                    ParryAction(slot); // 执行招架逻辑
                    break;
            }

            // 10. 开始动作计时，用于重置连续攻击段数
            isTiming = true; // 设置计时状态为开启
            actionTimer = 0f; // 重置动作计时器
        }

        /// <summary>
        /// 执行玩家的攻击逻辑，根据当前动作信息触发对应的攻击动画和效果。
        /// 包括检查体力、触发特殊攻击（招架、背刺）、设置动画状态等。
        /// </summary>
        /// <param name="slot">当前攻击动作的输入信息。</param>
        void AttackAction(Action slot)
        {
            // 1. 检查体力是否足够进行攻击
            if (characterStats._stamina < slot.staminaCost || characterStats._focus < slot.fpCost)
                return; // 如果体力不足，直接退出

            // 2. 检查专注值是否足够（某些特殊攻击可能需要消耗专注值）
            if (characterStats._focus < slot.fpCost)
                return; // 如果专注值不足，直接退出

            // 3. 检查是否可以触发招架（Parry）
            if (CheckForParry(slot))
                return; // 如果成功触发招架逻辑，直接返回

            // 4. 检查是否可以触发背刺（BackStab）
            if (CheckForBackStab(slot))
                return; // 如果成功触发背刺逻辑，直接返回

            // 5. 初始化目标动画
            string targetAnim = null;

            // 6. 获取当前动作的动画分支
            ActionAnim branch = slot.GetActionStep(ref actionManager.actionIndex)
                .GetBranch(storeActionInput); // 根据输入决定动作的分支动画

            // 从分支中获取目标动画名称
            targetAnim = branch.targetAnim;

            // 获取动画对应的音频资源
            if (audio_clip != null)
            {
                audio_clip = ResourceManager.instance.GetAudio(branch.audio_ids).audio_clip;
            }

            // 7. 如果未找到目标动画，直接返回
            if (string.IsNullOrEmpty(targetAnim))
                return;

            // 8. 设置当前动作为正在执行的动作
            currentAction = slot; // 将当前动作信息保存

            // 9. 更新动作状态
            canBeParried = slot.canBeParried; // 设置当前攻击是否可以被弹反

            canAttack = false; // 禁止再次攻击
            onEmpty = false; // 更新为空闲状态
            canMove = false; // 禁止移动
            inAction = true; // 标记角色正在执行动作

            // 10. 设置动画速度
            float targetSpeed = 1;
            if (slot.changeSpeed) // 如果配置允许修改动画速度
            {
                targetSpeed = slot.animSpeed; // 使用配置的动画速度
                if (targetSpeed == 0)
                    targetSpeed = 1; // 如果速度为零，重置为默认速度 1
            }

            anim.SetFloat("animSpeed", targetSpeed); // 更新 Animator 的动画速度

            // 11. 设置是否镜像播放动画
            anim.SetBool("mirror", slot.mirror); // 根据配置决定是否镜像动画

            // 12. 播放攻击动画
            anim.CrossFade(targetAnim, 0.2f); // 平滑切换到目标动画

            // 13. 消耗体力和专注值
            characterStats._stamina -= slot.staminaCost; // 减少当前攻击的体力消耗
            characterStats._focus -= slot.fpCost; // 减少专注值（如果需要）

            // 14. 重置输入状态
            rb = false;
            rt = false;
            lb = false;
            lt = false;
        }


        /// <summary>
        /// 检查是否可以触发招架（Parry）动作，并执行招架逻辑。
        /// 如果成功触发招架，返回 true；否则返回 false。
        /// </summary>
        /// <param name="slot">当前攻击动作的配置信息。</param>
        /// <returns>如果成功触发招架，返回 true；否则返回 false。</returns>
        bool CheckForParry(Action slot)
        {
            // 1. 如果当前动作不允许招架，直接返回 false
            if (slot.canParry == false)
                return false;

            // 2. 初始化一个用于存储检测到的敌人状态的变量
            EnemyStates parryTarget = null;

            // 3. 计算射线起点位置（角色当前的位置略微向上抬高）
            Vector3 origin = transform.position;
            origin.y += 1; // 设置射线的起点高度，与敌人的中心位置对齐

            // 4. 定义射线的方向（角色正前方）
            Vector3 rayDir = transform.forward;

            LayerMask enemyLayer = 1 << 8;

            // 5. 使用射线检测前方的敌人
            RaycastHit hit;
            if (Physics.Raycast(origin, rayDir, out hit, 2, enemyLayer))
            {
                // 如果检测到敌人，获取其 EnemyStates 组件
                parryTarget = hit.transform.GetComponentInParent<EnemyStates>();
            }

            // 6. 如果没有检测到敌人，或敌人无法被招架，返回 false
            if (parryTarget == null || parryTarget.parriedBy == null)
                return false;

            // 7. 计算角色与敌人之间的方向向量
            Vector3 dir = parryTarget.transform.position - transform.position;
            dir.Normalize(); // 标准化方向向量
            dir.y = 0; // 忽略垂直方向

            // 8. 检查角色面向与敌人方向的夹角是否在可招架范围内
            float angle = Vector3.Angle(transform.forward, dir);
            if (angle < 60) // 如果夹角小于 60 度，允许招架
            {
                // 9. 计算招架时玩家的位置调整
                Vector3 targetPosition = -dir * parryOffset; // 根据偏移量调整玩家位置
                targetPosition += parryTarget.transform.position; // 以敌人位置为基准计算玩家的新位置
                transform.position = targetPosition; // 设置玩家的新位置

                // 10. 调整玩家和敌人的朝向
                if (dir == Vector3.zero)
                    dir = -parryTarget.transform.forward; // 如果方向为零，设置为敌人朝向的反方向

                Quaternion eRotation = Quaternion.LookRotation(-dir); // 敌人朝向玩家
                Quaternion ourRot = Quaternion.LookRotation(dir); // 玩家朝向敌人

                parryTarget.transform.rotation = eRotation; // 设置敌人的旋转
                transform.rotation = ourRot; // 设置玩家的旋转

                // 11. 执行敌人被招架的逻辑
                parryTarget.IsGettingParried(slot, inventoryManager.GetCurrentWeapon(slot.mirror));

                // 12. 更新玩家的动作状态
                canAttack = false; // 禁止再次攻击
                onEmpty = false; // 更新为空闲状态
                canMove = false; // 禁止移动
                inAction = true; // 标记角色正在执行动作

                // 13. 设置镜像动画（如果当前动作需要）
                anim.SetBool("mirror", slot.mirror);
                anim.SetFloat("parrySpeed", 1); // 设置招架动作的速度
                anim.CrossFade("parry_attack", 0.2f); // 播放招架攻击动画

                // 14. 清除锁定目标
                lockOnTarget = null;

                // 15. 返回 true，表示成功触发招架
                return true;
            }

            // 如果未满足招架条件，返回 false
            return false;
        }

        /// <summary>
        /// 检查是否可以触发背刺（BackStab）动作，并执行背刺逻辑。
        /// 如果成功触发背刺，返回 true；否则返回 false。
        /// </summary>
        /// <param name="slot">当前攻击动作的配置信息。</param>
        /// <returns>如果成功触发背刺，返回 true；否则返回 false。</returns>
        bool CheckForBackStab(Action slot)
        {
            // 1. 如果当前动作不允许背刺，直接返回 false
            if (slot.canBackStab == false)
                return false;

            // 2. 初始化一个用于存储检测到的敌人状态的变量
            EnemyStates backstab = null;

            // 3. 计算射线起点位置（角色当前的位置略微向上抬高）
            Vector3 origin = transform.position;
            origin.y += 1; // 设置射线的起点高度，与敌人的中心位置对齐

            // 4. 定义射线的方向（角色正前方）
            Vector3 rayDir = transform.forward;

            // 5. 使用射线检测前方的敌人
            LayerMask enemyLayer = 1 << 8;
            RaycastHit hit;
            if (Physics.Raycast(origin, rayDir, out hit, 1, enemyLayer))
            {
                // 如果检测到敌人，获取其 EnemyStates 组件
                backstab = hit.transform.GetComponentInParent<EnemyStates>();
            }

            // 6. 如果没有检测到敌人，返回 false
            if (backstab == null)
                return false;

            // 7. 计算玩家与敌人之间的方向向量
            Vector3 dir = transform.position - backstab.transform.position;
            dir.Normalize(); // 标准化方向向量
            dir.y = 0; // 忽略垂直方向

            // 8. 检查玩家是否位于敌人的后方
            float angle = Vector3.Angle(backstab.transform.forward, dir);
            if (angle > 150) // 如果角度大于 150 度，认为玩家在敌人后方
            {
                // 9. 调整玩家的位置到背刺动作的合适位置
                Vector3 targetPosition = dir * backstabOffset; // 根据偏移量计算新位置
                targetPosition += backstab.transform.position; // 以敌人位置为基准
                transform.position = targetPosition; // 设置玩家的新位置

                // 10. 调整敌人朝向与玩家一致
                backstab.transform.rotation = transform.rotation;

                // 11. 执行敌人被背刺的逻辑
                backstab.IsGettingBackStabbed(slot, inventoryManager.GetCurrentWeapon(slot.mirror));

                // 12. 更新玩家状态
                canAttack = false; // 禁止再次攻击
                onEmpty = false; // 更新为空闲状态
                canMove = false; // 禁止移动
                inAction = true; // 标记角色正在执行动作

                // 13. 设置镜像动画（如果当前动作需要）
                anim.SetBool("mirror", slot.mirror);
                anim.CrossFade("parry_attack", 0.2f); // 播放背刺动画（可能与招架复用动画）

                // 14. 禁用右手输入，避免影响背刺流程
                rb = false;

                // 15. 返回 true，表示成功触发背刺
                return true;
            }

            // 如果未满足背刺条件，返回 false
            return false;
        }

        //**************Blocking******************
        bool blockAnim;
        string blockIdleAnim;

        /// <summary>
        /// 处理玩家的格挡逻辑，根据输入状态控制格挡动作的开始和结束。
        /// </summary>
        void HandleBlocking()
        {
            // 如果玩家未处于格挡状态
            if (isBlocking == false)
            {
                // 如果当前格挡动画正在播放
                if (blockAnim)
                {
                    inventoryManager.CloseBlockCollider(); // 关闭格挡碰撞器（可能用于防御机制）
                    anim.CrossFade(blockIdleAnim, 0.1f); // 切换到格挡结束后的空闲动画
                    blockAnim = false; // 标记格挡动画已结束
                }
            }
        }

        /// <summary>
        /// 执行玩家的格挡动作，包括开启碰撞检测和播放格挡动画。
        /// </summary>
        /// <param name="slot">当前动作的配置信息。</param>
        void BlockAction(Action slot)
        {
            isBlocking = true; // 标记玩家进入格挡状态
            enableIK = true; // 启用 IK（用于控制武器或盾牌位置）
            isLeftHand = slot.mirror; // 根据动作配置，判断是否使用左手格挡

            // 初始化 IK 的位置（左手或右手）
            a_hook.currentHand = (slot.mirror) ? AvatarIKGoal.LeftHand : AvatarIKGoal.RightHand;
            a_hook.InitIKForShield(slot.mirror); // 初始化 IK 数据

            // 打开格挡碰撞器
            inventoryManager.OpenBlockCollider();

            // 如果格挡动画尚未播放
            if (blockAnim == false)
            {
                // 根据是否双持设置格挡空闲动画
                blockIdleAnim = (isTwoHanded == false)
                    ? inventoryManager.GetCurrentWeapon(isLeftHand).oh_idle
                    : inventoryManager.GetCurrentWeapon(isLeftHand).th_idle;

                // 根据左手或右手，调整动画名称后缀
                blockIdleAnim += (isLeftHand) ? "_l" : "_r";

                // 获取并播放格挡动作动画
                string targetAnim = slot.targetAnim;
                targetAnim += (isLeftHand) ? "_l" : "_r";
                anim.CrossFade(targetAnim, 0.1f);

                // 标记格挡动画正在播放
                blockAnim = true;
            }
        }

        /// <summary>
        /// 执行玩家的招架动作，包括播放对应动画和设置角色状态。
        /// </summary>
        /// <param name="slot">当前动作的配置信息。</param>
        void ParryAction(Action slot)
        {
            string targetAnim = null;

            // 获取对应的动作分支动画
            targetAnim = slot.GetActionStep(ref actionManager.actionIndex)
                .GetBranch(slot.input).targetAnim;

            // 如果未找到对应动画，直接返回
            if (string.IsNullOrEmpty(targetAnim))
                return;

            // 设置动画速度
            float targetSpeed = 1;
            if (slot.changeSpeed)
            {
                targetSpeed = slot.animSpeed;
                if (targetSpeed == 0)
                    targetSpeed = 1; // 如果速度为 0，设置为默认速度 1
            }
            anim.SetFloat("animSpeed", targetSpeed);

            // 更新角色状态
            canBeParried = slot.canBeParried; // 当前攻击是否可以被弹反
            canAttack = false; // 禁止再次攻击
            onEmpty = false; // 标记当前状态为非空闲
            canMove = false; // 禁止移动
            inAction = true; // 标记正在执行动作

            // 设置镜像动画
            anim.SetBool("mirror", slot.mirror);

            // 播放招架动画
            anim.CrossFade(targetAnim, 0.2f);
        }

        //法术 再说吧类了

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
        {
            // slot is from the ActionManager

            //没写22
        }

        void HandleSpellCasting()
        {
            //没写
        }

        public void ThrowProjectile() //发射法术
        {
            if (projectileCandidate == null)
                return;

            GameObject g0 = Instantiate(projectileCandidate) as GameObject;
            Transform p = anim.GetBoneTransform((spellIsMirrored) ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
            g0.transform.position = p.position;


            if (lockOnTransform && lockOn)
            {
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

        /// <summary>
        /// 处理玩家的翻滚逻辑，根据玩家输入和状态触发翻滚动画和相关逻辑。
        /// 具有消耗体力的代价。
        /// </summary>
        void HandleRolls()
        {
            // 1. 检查是否满足翻滚条件
            if (!rollInput || usingItem || characterStats._stamina < rollCost)
                return; // 如果没有翻滚输入，正在使用道具，或体力不足，直接返回

            // 2. 初始化翻滚的垂直和水平输入
            float v = vertical; // 垂直方向输入
            float h = horizontal; // 水平方向输入

            // 3. 处理非锁定状态下的翻滚
            if (!lockOn)
            {
                v = (moveAmount > 0.3f) ? 1 : 0; // 如果有移动输入，设置垂直翻滚量为 1，否则为 0
                h = 0; // 非锁定状态下忽略水平输入

                // 如果有翻滚输入方向
                if (v != 0)
                {
                    if (moveDir == Vector3.zero) // 如果移动方向尚未设置
                        moveDir = transform.forward; // 默认翻滚方向为角色当前朝向

                    transform.rotation = Quaternion.LookRotation(moveDir); // 更新角色旋转方向为翻滚方向

                    a_hook.InitForRoll(); // 初始化翻滚的动画钩子
                    a_hook.rm_multi = rollSpeed; // 设置翻滚速度倍数
                }
                else
                {
                    a_hook.rm_multi = 1.3f; // 默认翻滚速度倍数
                }
            }
            else // 4. 处理锁定状态下的翻滚
            {
                v = (moveAmount > 0.3f) ? v : 0; // 如果移动量足够，保留垂直输入，否则为 0
                h = (moveAmount > 0.3f) ? h : 0; // 如果移动量足够，保留水平输入，否则为 0

                // 处理方向修正
                if (v > 0) v = 1;
                if (h > 0) h = 1;
                if (v < 0) v = -1;
                if (h < 0) h = -1;

                // 如果有翻滚输入方向
                if (v != 0 || h != 0)
                {
                    if (moveDir == Vector3.zero)
                        moveDir = transform.forward;

                    a_hook.InitForRoll(); // 初始化翻滚的动画钩子
                    a_hook.rm_multi = rollSpeed; // 设置翻滚速度倍数
                }
                else
                {
                    a_hook.rm_multi = 1.3f; // 默认翻滚速度倍数
                }
            }

            // 5. 设置动画输入参数
            anim.SetFloat("vertical", v); // 设置垂直翻滚量
            anim.SetFloat("horizontal", h); // 设置水平翻滚量

            // 6. 更新翻滚状态
            canAttack = false; // 禁止攻击
            onEmpty = false; // 标记非空闲状态
            canMove = false; // 禁止移动
            inAction = true; // 标记为执行动作中
            anim.CrossFade("Rolls", 0.2f); // 播放翻滚动画
            isInvincible = true; // 设置翻滚期间无敌状态
            isBlocking = false; // 禁止格挡
            characterStats._stamina -= rollCost; // 消耗体力值
        }

        /// <summary>
        /// 处理角色的基础移动动画逻辑，根据角色的移动状态和输入调整动画参数。
        /// </summary>
        void HandleMovementAnimation()
        {
            // 设置是否运行的动画参数
            anim.SetBool("run", run); 

            // 设置角色垂直方向的移动动画参数
            // 使用平滑插值（damping）确保动画过渡流畅
            anim.SetFloat("vertical", moveAmount, 0.4f, delta); 

            // 设置是否在地面的动画参数
            anim.SetBool("onGround", onGround); 
        }

        /// <summary>
        /// 处理角色在锁定状态下的移动动画，根据相对方向调整动画参数。
        /// </summary>
        /// <param name="moveDir">移动方向向量。</param>
        void HandleLockOnAnimations(Vector3 moveDir)
        {
            // 1. 将世界坐标的移动方向转换为相对于角色的本地坐标
            Vector3 relativeDir = transform.InverseTransformDirection(moveDir); 
            float h = relativeDir.x; // 水平方向输入（左右）
            float v = relativeDir.z; // 垂直方向输入（前后）

            // 2. 如果正在使用道具或施法，限制移动动画的范围
            if (usingItem || isSpellCasting)
            {
                run = false; // 使用道具或施法时无法跑步
                v = Mathf.Clamp(v, -0.7f, 0.6f); // 限制垂直移动动画范围
                h = Mathf.Clamp(h, -0.6f, 0.6f); // 限制水平移动动画范围
            }

            // 3. 设置垂直方向和水平方向的动画参数
            anim.SetFloat("vertical", v, 0.2f, delta); // 使用平滑插值调整动画
            anim.SetFloat("horizontal", h, 0.2f, delta); // 水平方向动画参数
        }

        /// <summary>
        /// 检测玩家是否在地面上，通过多个射线向下检测来判断。
        /// 如果任意射线检测到地面，则返回 true，同时调整玩家的 Y 坐标以匹配地面高度。
        /// </summary>
        /// <returns>如果检测到地面，返回 true；否则返回 false。</returns>
        public bool OnGround()
        {
            // 默认返回值：假设玩家未在地面上
            bool r = false;

            // 1. 定义射线的起点数组
            Vector3[] rayOrigins = new Vector3[]
            {
                transform.position + (Vector3.up * toGround), // 中心
                transform.position + (Vector3.up * toGround) + (Vector3.forward * 0.1f), // 前
                transform.position + (Vector3.up * toGround) + (Vector3.back * 0.1f), // 后
                transform.position + (Vector3.up * toGround) + (Vector3.left * 0.1f), // 左
                transform.position + (Vector3.up * toGround) + (Vector3.right * 0.1f), // 右

                // 斜方向
                transform.position + (Vector3.up * toGround) + (Vector3.forward * 0.1f) + (Vector3.left * 0.1f), // 左前
                transform.position + (Vector3.up * toGround) + (Vector3.forward * 0.1f) + (Vector3.right * 0.1f), // 右前
                transform.position + (Vector3.up * toGround) + (Vector3.back * 0.1f) + (Vector3.left * 0.1f), // 左后
                transform.position + (Vector3.up * toGround) + (Vector3.back * 0.1f) + (Vector3.right * 0.1f) // 右后
            };

            // 2. 遍历所有射线
            foreach (var origin in rayOrigins)
            {
                // 射线方向向下
                Vector3 dir = -Vector3.up;
                float dis = toGround + 0.2f; // 射线长度，略大于 `toGround` 值

                RaycastHit hit;

                // 在 Scene 中显示射线（用于调试）
                Debug.DrawRay(origin, dir * dis, Color.cyan);

                LayerMask grondLayer = 1 << 28;

                // 3. 检测射线是否命中地面
                if (Physics.Raycast(origin, dir, out hit, dis, grondLayer))
                {
                    // 如果检测到地面，设置标志为 true
                    r = true;

                    // 4. 使用命中点的 Y 坐标更新玩家位置
                    transform.position = new Vector3(
                        transform.position.x, // 保持 X 坐标不变
                        hit.point.y, // 使用命中点的 Y 坐标
                        transform.position.z // 保持 Z 坐标不变
                    );

                    // 找到第一个命中点后，立即跳出循环
                    break;
                }
            }

            // 5. 返回地面检测结果
            return r;
        }

        //***********TwoHanded********************

        /// <summary>
        /// 处理玩家的双手武器切换逻辑，支持从单手切换到双手模式，或从双手切换回单手模式。
        /// </summary>
        public void HandleTwoHanded()
        {
            bool isRight = true; // 标记是否使用右手武器

            // 1. 检查右手是否装备武器
            if (inventoryManager.rightHandWeapon == null)
                return;

            // 获取右手武器实例
            Weapon w = inventoryManager.rightHandWeapon.instance;

            // 如果右手武器未找到，尝试使用左手武器
            if (w == null)
            {
                w = inventoryManager.leftHandWeapon.instance;
                isRight = false; // 标记为使用左手武器
            }

            // 如果仍未找到武器，退出方法
            if (w == null)
                return;

            // 2. 如果切换到双手模式
            if (isTwoHanded)
            {
                anim.CrossFade(w.th_idle, 0.2f); // 播放双手持武器的空闲动画
                actionManager.UpdateActionsTwoHanded(); // 更新动作管理器以支持双手动作

                // 如果当前武器在右手，禁用左手武器模型
                if (isRight)
                {
                    if (inventoryManager.leftHandWeapon)
                        inventoryManager.leftHandWeapon.weaponModel.SetActive(false);
                }
                else // 如果当前武器在左手，禁用右手武器模型
                {
                    if (inventoryManager.rightHandWeapon)
                        inventoryManager.rightHandWeapon.weaponModel.SetActive(false);
                }
            }
            // 3. 如果切换回单手模式
            else
            {
                anim.Play("Equip Weapon"); // 播放切换回单手的动画
                actionManager.UpdateActionsOneHanded(); // 更新动作管理器以支持单手动作

                // 如果当前武器在右手，启用左手武器模型
                if (isRight)
                {
                    if (inventoryManager.leftHandWeapon)
                        inventoryManager.leftHandWeapon.weaponModel.SetActive(true);
                }
                else // 如果当前武器在左手，启用右手武器模型
                {
                    if (inventoryManager.rightHandWeapon)
                        inventoryManager.rightHandWeapon.weaponModel.SetActive(true);
                }
            }
        }

        //状态

        public void AddHealth()
        {
            characterStats.fp++;
        }

        public void Recover()
        {
            characterStats._health = characterStats.hp;
            characterStats._focus = characterStats.fp;
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

        /// <summary>
        /// 执行玩家受到伤害的逻辑，包括扣减生命值、播放受伤动画、设置无敌时间等。
        /// </summary>
        /// <param name="a">包含攻击信息的对象（如攻击来源或攻击类型）。</param>
        public void DoDamage(AIAttacks a)
        {
            // 1. 如果玩家当前处于无敌状态，直接返回
            if (isInvincible)
                return;

            // 2. 设置玩家为受伤状态，用于触发受伤效果
            damaged = true;

            // 3. 定义伤害值（可扩展为从攻击信息 `a` 中读取具体伤害值）
            int damage = 20;

            // 4. 扣减玩家生命值
            characterStats._health -= damage;

            // 5. 如果玩家没有进行翻滚，则播放受伤动画
            if (!rollInput)
            {
                int ran = Random.Range(0, 100); // 随机选择一个受伤动画
                string tA = (ran > 50) ? "damage1" : "damage2"; // 动画名称
                anim.Play(tA); // 播放受伤动画
            }

            // 6. 更新玩家状态
            anim.SetBool("OnEmpty", false); // 更新动画状态机，标记非空闲状态
            onEmpty = false; // 设置玩家为非空闲状态
            isInvincible = true; // 设置玩家为无敌状态（用于短时间内避免连续受到伤害）
            anim.applyRootMotion = true; // 启用根运动，确保受伤动画的位移正确
            anim.SetBool("canMove", false); // 禁止玩家移动

            // 7. 检查玩家是否死亡
            if (characterStats._health <= 0 && !isDead)
            {
                Die(); // 调用死亡逻辑
            }
        }

        public void Die()
        {
            isDead = true;
            isInvincible = true;
            // StartCoroutine(sceneController.HandleGameOver());
        }

        public void ResetInput()
        {
            if (characterStats._stamina <
                actionManager.GetActionFromInput(actionManager.GetActionInput(this)).staminaCost || characterStats._focus < actionManager.GetActionFromInput(actionManager.GetActionInput(this)).fpCost)
            {
                rb = false;
                rt = false;
                lb = false;
                lt = false;
            }
        }

        private void ResetActionIndex(float d)
        {
            if (isTiming)
            {
                actionTimer += d; // 累加计时器

                // 如果超过（）秒未调用 DetectAction，则重置 actionIndex
                if (actionTimer >= resetTime)
                {
                    actionManager.actionIndex = 0; // 重置动作索引
                    isTiming = false; // 停止计时
                }
            }
        }
    }
}
