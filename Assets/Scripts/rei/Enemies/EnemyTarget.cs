using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rei
{
    public class EnemyTarget : MonoBehaviour
    {
        public int index;
        public bool isLockOn;
        public List<Transform> targets = new List<Transform>();
        public List<HumanBodyBones> h_bones = new List<HumanBodyBones> ();

        public EnemyStates eStates;

        Animator anim;
    }
}