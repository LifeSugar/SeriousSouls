using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace rei
{
    public class EnemyManager : MonoBehaviour
    {
        public List<EnemyTarget> enemyTargets = new List<EnemyTarget>();//所有的Enemy Target列表
        public float detectRange; //监测距离

        public List<EnemyInfo> enemyInfos = new List<EnemyInfo>();

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
        
        /// <summary>
        /// 保存所有敌人的初始状态
        /// </summary>
        private void SaveInitialEnemyStates()
        {
            foreach (var enemyInfo in enemyInfos)
            {
                if (enemyInfo.instance != null)
                {
                    enemyInfo.position = enemyInfo.instance.transform.position;
                    enemyInfo.rotation = enemyInfo.instance.transform.rotation;
                }
            }
        }
        
        
        /// <summary>
        /// 重置所有敌人
        /// </summary>
        public void ResetAllEnemies()
        {
            foreach (var enemyInfo in enemyInfos)
            {
                if (enemyInfo.instance == null) // 如果敌人实例已被销毁，则重新实例化
                {
                    GameObject newEnemy = Instantiate(enemyInfo.prefab, enemyInfo.position, enemyInfo.rotation);
                    EnemyStates enemyStates = newEnemy.GetComponent<EnemyStates>();
                    enemyStates.GetComponent<EnemyAIHandler>().Init();
                    enemyInfo.instance = enemyStates;

                    // 初始化新实例
                    enemyStates.SaveInitialState();
                }
                else // 如果实例还存在，则重置状态
                {
                    enemyInfo.instance.transform.position = enemyInfo.position;
                    enemyInfo.instance.transform.rotation = enemyInfo.rotation;
                    enemyInfo.instance.ResetState();
                }
            }
        }

        public static EnemyManager instance;

        public void Awake()
        {
            instance = this;
        }
    }

    [System.Serializable]
    public class EnemyInfo
    {
        public GameObject prefab;
        public Vector3 position;
        public Quaternion rotation;
        public EnemyStates instance;
    }
}