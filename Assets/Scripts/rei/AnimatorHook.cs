using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rei
{
    public class AnimatorHook : MonoBehaviour
    {
        //rei test

        Animator anim; // 用于角色动画控制的Animator对象

        PlayerState _playerStates; // 管理角色状态的StateManager

        EnemyStates eStates;           // 管理敌人状态的EnemyStates
        Rigidbody rigid; // 用于物理控制的Rigidbody

        public float rm_multi; // 动画移动速度的倍数，用于调整动画根运动
        bool rolling; // 标记是否处于翻滚状态
        float roll_t; // 用于翻滚动画的时间计数器
        float delta; // 帧间隔时间
        AnimationCurve roll_curve; // 定义翻滚动画曲线的AnimationCurve
        
        

        HandleIK ik_handler; // 处理逆向运动学（IK）的HandleIK组件
        public bool useIK; // 是否启用IK
        public AvatarIKGoal currentHand; // 当前使用IK的手（左手或右手）

        public bool killDelta; // 用于立即停止动画根运动的标志

        //-----------------------------------------------------------------------

        // 初始化方法，传入StateManager和EnemyStates
        public void Init(PlayerState st , EnemyStates eSt)
        {
            _playerStates = st;
            eStates = eSt;

            // player/enemy二选一
            if (st != null)
            {
                anim = st.anim;
                rigid = st.rigid;
                roll_curve = _playerStates.roll_curve;
                delta = _playerStates.delta;
            }
            
            if (eSt != null) {
                anim = eSt.anim;
                rigid = eSt.rigid;
                delta = eSt.delta;
            }

            // 初始化IK处理器
            ik_handler = gameObject.GetComponent<HandleIK>();
            if (ik_handler != null)
                ik_handler.Init(anim);
        }

        // 初始化翻滚状态
        public void InitForRoll()
        {
            rolling = true;
            roll_t = 0;
        }

        // 关闭翻滚状态
        public void CloseRoll()
        {
            if (!rolling)
                return;

            rm_multi = 1;
            rolling = false;
        }

        // 在Animator每帧更新时处理动画根运动
        void OnAnimatorMove()
        {
            if (ik_handler != null)
            {
                // 在翻滚和攻击时调整IK权重
                ik_handler.OnAnimatorMoveTick((currentHand == AvatarIKGoal.LeftHand));
            }

            // 没有角色(或敌人)状态时直接返回
            if (_playerStates == null && eStates == null)
                return;

            if (rigid == null)//总体思路还是靠控制rigidbody来进行运动（所以使用animation physic方法）
                return;

            // 检查动作状态（onEmpty等），如果满足条件则退出
            if (_playerStates != null)
            {
                if (_playerStates.onEmpty)
                    return;
                delta = _playerStates.delta; //animator采用的updateMode是Animate Physics,这里的delta是fixeddeltatime
            }
            if (eStates != null) 
            {
                if (eStates.canMove)
                    return;
                delta = eStates.delta;
            }

            rigid.drag = 0; 

            // 检查rm_multi是否初始化
            if (rm_multi == 0)
                rm_multi = 1;

            // 根据不同状态处理动画根运动
            if (!rolling)
            {
                
                Vector3 deltaPos = anim.deltaPosition;
                if (killDelta) //立刻停止根运动
                {
                    killDelta = false;
                    deltaPos = Vector3.zero;
                }

                Vector3 v = (deltaPos * rm_multi) / delta; //deltaPos/delta = velocity换句话说这里v = rm_multi * rb.rm_multi
                v.y = rigid.velocity.y;//这里保留了垂直高度的速度

                if (eStates)
                    eStates.agent.velocity = v;
                else
                    rigid.velocity = v; //相当于给xz方向的速度做了个加速，y方向的保留，当然速度快了距离也自然更远了
            }
            else
            {
                roll_t += delta / _playerStates.rollDuration; //每帧增加的时间进度
                if (roll_t > 1)
                {
                    roll_t = 1;
                }

                if (_playerStates == null)
                    return;

                float zValue = _playerStates.roll_curve.Evaluate(roll_t);
                Vector3 v1 = (_playerStates.lockOn) ? _playerStates.moveDir : Vector3.forward;
                Vector3 relative = (_playerStates.lockOn) ? _playerStates.moveDir * zValue : transform.TransformDirection(v1 * zValue);
                Vector3 v2 = (relative * rm_multi);
                v2.y = rigid.velocity.y;
                rigid.constraints |= RigidbodyConstraints.FreezePositionY;
                rigid.velocity =(_playerStates.lockOn) ? anim.deltaPosition * 0.8f * zValue / delta : v2 * 3.2f; //自行理解
            }
        }

        // 在Animator中处理IK
        void OnAnimatorIK()
        {
            if (ik_handler == null)
                return;

            if (!useIK)
            {
                if (ik_handler.weight > 0)
                {
                    ik_handler.IKTick(currentHand, 0);
                }
                else
                {
                    ik_handler.weight = 0;
                }
            }
            else
            {
                ik_handler.IKTick(currentHand, 1);
            }
        }

        // 在LateUpdate中调用IK处理的延迟更新
        void LateUpdate()
        {
            if (ik_handler != null)
                ik_handler.LateTick();
        }

        // 开启攻击状态
        public void OpenAttack()
        {
            if (_playerStates)
                _playerStates.canAttack = true;
        }

        // 开启移动状态
        public void OpenCanMove()
        {
            if (_playerStates)
            {
                _playerStates.canMove = true;
            }
        }

        // 开启伤害碰撞体
        public void OpenDamageColliders()
        {
            if (_playerStates)
                _playerStates.inventoryManager.OpenAllDamageColliders();
            if (eStates)
                eStates.OpenDamageCollier();

            OpenParryFlag();
        }

        // 关闭伤害碰撞体
        public void CloseDamageColliders()
        {
            if (_playerStates)
                _playerStates.inventoryManager.CloseAllDamageColliders();
            if (eStates)
                eStates.CloseDamageCollider();

            CloseParryFlag();
        }

        // 开启格挡标志
        public void OpenParryFlag()
        {
            if (_playerStates)
            {
                _playerStates.parryIsOn = true;
            }

            if (eStates) 
            {
                eStates.parryIsOn = true;
            }
        }

        // 关闭格挡标志
        public void CloseParryFlag()
        {
            if (_playerStates)
            {
                _playerStates.parryIsOn = false;
            }

            if (eStates) 
            {
                eStates.parryIsOn = false;
            }
        }

        // 开启格挡碰撞体
        public void OpenParryCollider()
        {
            if (_playerStates == null)
                return;

            _playerStates.inventoryManager.OpenParryCollider();
        }

        // 关闭格挡碰撞体
        public void CloseParryCollider()
        {
            if (_playerStates == null)
                return;

            _playerStates.inventoryManager.CloseParryCollider();
        }

        // 关闭法术粒子效果
        public void CloseParticle()
        {
            if (_playerStates)
            {
                if (_playerStates.inventoryManager.currentSpell.currentParticle != null)
                    _playerStates.inventoryManager.currentSpell.currentParticle.SetActive(false);
            }
        }

        // 发射法术弹道
        public void InitiateThrowForProjectile()
        {
            if (_playerStates)
            {
                _playerStates.ThrowProjectile();
            }
        }

        // 初始化盾牌的IK
        public void InitIKForShield(bool isLeft)
        {
            ik_handler.UpdateIKTargets((isLeft) ? IKSnapShotType.shield_l : IKSnapShotType.shield_r, isLeft);
        }

        // 初始化呼吸法术的IK
        public void InitIKForBreathSpell(bool isLeft)
        {
            ik_handler.UpdateIKTargets((isLeft) ? IKSnapShotType.breath_l : IKSnapShotType.breath_r, isLeft);
        }

        // 开启旋转控制
        public void OpenRotationControl()
        {
            if (_playerStates)
                _playerStates.canRotate = true;
            if (eStates)
                eStates.rotateToTarget = true;
        }

        // 关闭旋转控制
        public void CloseRotationControl()
        {
            if (_playerStates)
                _playerStates.canRotate = false;
            if (eStates)
                eStates.rotateToTarget = false;
        }

        // 消耗当前物品
        public void ConsumeCurrentItem()
        {
            if (_playerStates && _playerStates.inventoryManager.curConsumable)
            {
                _playerStates.inventoryManager.curConsumable.itemCount--;
                ItemEffectManager.singleton.CastEffect(_playerStates.inventoryManager.curConsumable.instance.consumableEffect,
                    _playerStates);
            }
        }

        // 播放音效
        public void PlaySoundEffect()
        {
            if (_playerStates)
            {
                _playerStates.audio_source.PlayOneShot(_playerStates.audio_clip);
            }
        }

        public void HideRightHandWeapon()
        {
            if (_playerStates.inventoryManager.hasRightHandWeapon && _playerStates.inventoryManager.hasLeftHandWeapon)
            {
                Debug.Log("skill both handed");
                _playerStates.inSkill = true;
                _playerStates.inventoryManager.rightHandWeapon.weaponModel.SetActive(false);
            }
        }

        public void UnhideRightHandWeapons()
        {
            if (_playerStates.inventoryManager.hasRightHandWeapon && _playerStates.inventoryManager.hasLeftHandWeapon)
            {
                Debug.Log("skill both handed close");
                _playerStates.inSkill = false;
                _playerStates.inventoryManager.rightHandWeapon.weaponModel.SetActive(true);
            }
        }
    }
}