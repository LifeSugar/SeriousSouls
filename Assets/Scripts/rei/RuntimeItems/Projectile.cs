using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rei
{
    public class Projectile : MonoBehaviour
    {
        // Rigidbody 用于控制弹道的物理行为
        Rigidbody rigid;

        // 水平和垂直速度，控制弹道的速度和弧度
        public float hSpeed = 5f;
        public float vSpeed = 2f;

        // 弹道的目标
        public Transform target;

        // 爆炸效果预制件
        public GameObject explosionPrefab;

        // 初始化弹道运动
        public void Init()
        {
            // 获取 Rigidbody 组件
            rigid = GetComponent<Rigidbody>();

            // 添加力，使弹道具有初速度
            Vector3 targetForce = transform.forward * hSpeed; // 水平力，沿前方方向
            targetForce += transform.up * vSpeed; // 垂直力，向上
            rigid.AddForce(targetForce, ForceMode.Impulse); // 以瞬间力模式添加力
        }

        // 处理弹道的触发事件，当与其他碰撞体接触时调用
        void OnTriggerEnter(Collider other)
        {
            // // 尝试获取碰撞对象的 EnemyStates，判断是否为敌人
            // EnemyStates eStates = other.GetComponentInParent<EnemyStates>();
            //
            // if (eStates != null)
            // {
            //     // 对敌人施加伤害
            //     eStates.DoDamageSpell();
            //     // 使用 SpellEffectsManager 触发 "onFire" 特效
            //     SpellEffectsManager.singleton.UseSpellEffect("onFire", null, eStates);
            // }

            // 在碰撞位置创建爆炸效果
            GameObject g0 = Instantiate(explosionPrefab, transform.position, transform.rotation) as GameObject;

            // 销毁弹道物体
            Destroy(this.gameObject);
        }
    }
}