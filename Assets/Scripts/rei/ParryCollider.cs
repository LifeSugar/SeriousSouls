using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rei
{
    public class ParryCollider : MonoBehaviour
    {
        PlayerState playerState;
        EnemyStates eState;
        
        public float maxTimer = 0.6f;
        float timer;
        
        public void InitPlayer(PlayerState st){
            playerState = st;
        }
			
        public void InitEnemy(EnemyStates eSt){
            eState = eSt;
        }

        void Update(){
            if (playerState) {
                timer += playerState.delta;

                if (timer > maxTimer) {
                    timer = 0;
                    gameObject.SetActive (false);
                }
            }
        }

        void OnTriggerEnter (Collider other)
        {
            

            // if (playerState) 
            // {
            //     EnemyStates e_st = other.transform.GetComponentInParent<EnemyStates> ();
            //
            //     if (e_st != null) {
            //         e_st.CheckForParry (transform.root, playerState);
            //     }
            // }
            //
            // if (eState) 
            // {
				        //
            // }

        }
    }

}