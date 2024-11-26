using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rei
{
    public class DamageCollider : MonoBehaviour //这个类类似AnimatorHook，由Player和Enemy共享
    {
        StateManager states;
        EnemyStates eStates;

        public void InitPlayer(StateManager st)
        {
            states = st;
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
            if (states)
            {
                EnemyStates es = other.transform.GetComponentInParent<EnemyStates>();//找到有没有砍到敌人

                if (es != null)
                {
                    es.DoDamage();
                }

                return;
            }

            if (eStates)
            {
                StateManager st = other.transform.GetComponentInParent<StateManager>();//找到有没有砍到玩家

                if (st != null)
                {
                    st.DoDamage(eStates.GetCurrentAttack());
                }

                return;
            }
        }
    }
}