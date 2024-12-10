using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rei
{
    public class DamageCollider : MonoBehaviour //这个类类似AnimatorHook，由Player和Enemy共享
    {
        PlayerState _playerStates;
        EnemyStates eStates;

        public void InitPlayer(PlayerState st)
        {
            _playerStates = st;
            gameObject.layer = 9;
            gameObject.SetActive(false);
        }

        public void InitEnemy(EnemyStates st)
        {
            eStates = st;
            gameObject.layer = 9;
            gameObject.SetActive(false);
        }

        void OnTriggerEnter(Collider other)//超绝简单的伤害判断方法
        {
            if (_playerStates)
            {
                EnemyStates es = other.transform.GetComponentInParent<EnemyStates>();//找到有没有砍到敌人

                if (es != null)
                {
                    es.DoDamage();
                    this.gameObject.SetActive(false);
                }

                
            }

            if (eStates)
            {
                ParryCollider pc = other.transform.GetComponentInChildren<ParryCollider>();
                BlockCollider bc = other.transform.GetComponentInChildren<BlockCollider>();
                if (pc != null)
                {
                    // Debug.Log("this attack is parried by " + pc.GetComponentInParent<PlayerState>().name);
                    this.gameObject.SetActive(false);
                    PlayerState st = other.transform.GetComponentInParent<PlayerState>();
                    eStates.CheckForParry(st.transform, st);
                    
                }
                else if (bc != null)
                {
                    // Debug.Log("this attack is blocked");
                    this.gameObject.SetActive(false);
                    eStates.HandleBlocked();
                }
                else
                {
                    PlayerState st = other.transform.GetComponentInParent<PlayerState>();//找到有没有砍到玩家

                    if (st != null)
                    {
                        st.DoDamage(eStates.GetCurrentAttack());
                    }

                    this.gameObject.SetActive(false);
                }
                
            }
        }
    }
}