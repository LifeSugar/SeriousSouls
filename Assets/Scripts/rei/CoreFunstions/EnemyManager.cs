using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace rei
{
    public class EnemyManager : MonoBehaviour
    {
        public List<EnemyTarget> enemyTargets = new List<EnemyTarget>();//所有的Enemy Target列表
        public float detectRange; //监测距离

        public EnemyTarget GetEnemy(Vector3 from)
        {
            EnemyTarget r = null;
            float minDist = detectRange;
            for (int i = 0; i < enemyTargets.Count; i++)
            {
                float tDist = Vector3.Distance(from, enemyTargets[i].GetTarget().position);
                if (tDist < minDist) //这里的判断条件可以再严格一些 之后再说
                {
                    minDist = tDist;
                    r = enemyTargets[i];
                }
            }

            return r;
        }

        public static EnemyManager instance;

        public void Awake()
        {
            instance = this;
        }
    }
}