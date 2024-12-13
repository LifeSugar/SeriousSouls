using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace rei
{
    public class EnemyManager : MonoBehaviour
    {
        public List<EnemyTarget> enemyTargets = new List<EnemyTarget>();//所有的Enemy Target列表
        public float detectRange; //监测距离
        public EnemyStates prefab;

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

        void Start()
        {
            InitEnenmies();
        }

        void InitEnenmies()
        {
            // 清空当前的 enemyInfos 列表以确保初始化干净
            enemyInfos.Clear();

            // 遍历当前对象的所有子物体
            for (int i = 0; i < transform.childCount; i++)
            {
                // 获取子物体
                Transform child = transform.GetChild(i);

                // 尝试获取子物体上的 EnemyStates 脚本
                EnemyStates enemyStates = child.GetComponent<EnemyStates>();
                if (enemyStates != null)
                {
                    // 创建一个新的 EnemyInfo
                    EnemyInfo ei = new EnemyInfo();

                    // 获取与子物体相关的初始信息
                    ei.instance = enemyStates;
                    ei.position = child.position;
                    ei.rotation = child.rotation;

                    // 尝试从 EnemyStates 中获取 prefab，如果没有则默认使用子物体本身
                    // ei.prefab = enemyStates.GetPrefab() != null ? enemyStates.GetPrefab() : child.gameObject;
                    ei.prefab = prefab.gameObject;

                    // 将创建的 EnemyInfo 添加到列表中
                    enemyInfos.Add(ei);
                }
            }
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
        public IEnumerator ResetAllEnemies()
        {
            foreach (var enemyInfo in enemyInfos)
            {
                if (enemyInfo.instance == null)
                {
                    GameObject newEnemy = Instantiate(enemyInfo.prefab, enemyInfo.position, enemyInfo.rotation);
                    EnemyStates enemyStates = newEnemy.GetComponent<EnemyStates>();
                    enemyStates.GetComponent<AIHandler>().Init();
                    enemyInfo.instance = enemyStates;
                    newEnemy.transform.SetParent(this.transform);
                    newEnemy.gameObject.SetActive(true);

                    enemyStates.SaveInitialState();
                }
                else
                {
                    enemyInfo.instance.transform.position = enemyInfo.position;
                    enemyInfo.instance.transform.rotation = enemyInfo.rotation;
                    enemyInfo.instance.ResetState();
                }

                // 如果需要模拟异步操作，可在每个敌人重置后添加一点延迟
                yield return null; // 在每次循环后暂停一帧
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