using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rei
{
    public class EnemyTarget : MonoBehaviour
    {
        public int index; //此单位可锁定目标的索引
        public bool isLockOn;
        public List<Transform> targets = new List<Transform>();
        public List<HumanBodyBones> h_bones = new List<HumanBodyBones>();

        public EnemyStates eStates;

        Animator anim;
        //-----------------------------------------------------------------------

        public void Init(EnemyStates st)
        {
            eStates = st;
            anim = eStates.anim;
            if (anim.isHuman == false) //判断是否是人行敌人 如果不是人行敌人那就无法锁定
                return;

            // 这里是通过骨骼的位置来调整target的位置，之后也可以手动添加一些锁定位置到target
            for (int i = 0; i < h_bones.Count; i++)
            {
                targets.Add(anim.GetBoneTransform(h_bones[i]));
            }

            EnemyManager.instance.enemyTargets.Add(this);
        }

        void OnDestroy()
        {
            EnemyManager.instance.enemyTargets.Remove(this);
        }

        // needs  to be clarified. (4)
        public Transform GetTarget(bool negative = false)
        {
            // 若在初始化时没有指定，那么就用自身的位置
            if (targets.Count == 0)
                return transform;
            // 切换左右目标
            if (negative == false)
            {
                //when h > 0

                if (index < targets.Count - 1)
                {
                    index++;
                }
                else
                {
                    index = 0;
                }
            }
            //when (h < 0)
            else
            {
                if (index == 0)
                {
                    index = targets.Count - 1;
                }
                else
                    index--;
            }

            index = Mathf.Clamp(index, 0, targets.Count);

            return targets[index];
        }
    }
}