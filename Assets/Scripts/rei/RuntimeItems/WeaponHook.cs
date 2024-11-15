using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rei
{
    public class WeaponHook : MonoBehaviour
    {
        
        //这里还是先用熟悉的Collier做
        public GameObject[] damageCollider;
        
        public void OpenDamageColliders(){
            for (int i = 0; i < damageCollider.Length; i++) {
                damageCollider [i].SetActive (true);
            }

        }

        public void CloseDamageColliders(){
            for (int i = 0; i < damageCollider.Length; i++) {
                damageCollider [i].SetActive (false);
            }

        }

        public void InitDamageColliders(StateManager states) {
            for (int i = 0; i < damageCollider.Length; i++)
            {
                damageCollider[i].GetComponent<DamageCollider>().InitPlayer(states);
            }
        }
    }
}
