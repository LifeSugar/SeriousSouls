using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace rei
{
    // 定义一个委托，用于传递篝火名称并定义传送事件
    public delegate void TeleportHandler(string targetCampFireName);

    public class CampFireUIHandler : MonoBehaviour
    {
        // 退出按钮
        public Button exitButton;

        // 传送按钮（通用）
        public Button teleportButton;

        // 显示当前篝火名称的 UI 文本
        public Text currentCampFire;

        // 用于生成传送选择按钮的 UI 模板
        public GameObject campFireTelepotSelectionUI;

        // 容纳所有传送按钮的父对象
        public GameObject parentObject;

        // 传送事件，允许外部订阅
        public event TeleportHandler OnTeleport;
        
        /// <summary>
        /// 更新当前篝火的 UI 信息，显示当前坐下的篝火名称。
        /// </summary>
        public void UpdateCampFireUI()
        {
            // 设置当前篝火的名称
            currentCampFire.text = CampFireManager.instance.GetSittingCampFire().name;

            // 清除传送按钮区域中的所有子物体
            ClearChildren();
        }
        
        /// <summary>
        /// 清空父对象下的所有子物体
        /// </summary>
        public void ClearChildren()
        {
            // 从最后一个子物体开始删除，避免索引问题
            for (int i = parentObject.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = parentObject.transform.GetChild(i);
                Destroy(child.gameObject); // 销毁子物体
            }

            Debug.Log("All child objects have been cleared.");
        }

        /// <summary>
        /// 初始化传送选择界面，为每个篝火生成一个对应的按钮。
        /// </summary>
        public void InitCampFireSelectionUI()
        {
            // 清除旧的传送按钮
            ClearChildren();

            // 获取所有激活的篝火
            List<CampFire> campFires = CampFireManager.instance.GetActiveCampFire();

            // 遍历每个篝火，为其生成一个传送按钮
            foreach (CampFire campFire in campFires)
            {
                // 实例化一个新的传送按钮 UI
                GameObject newObject = Instantiate(campFireTelepotSelectionUI, parentObject.transform);

                // 设置父对象
                newObject.transform.SetParent(parentObject.transform);

                // 设置按钮文本为篝火名称
                newObject.GetComponentInChildren<Text>().text = campFire.name;

                // 获取按钮文本内容（传递给事件）
                string buttonText = newObject.GetComponentInChildren<Text>().text;

                // 如果是当前坐下的篝火，显示 "Current" 标识
                if (campFire.sitting)
                {
                    newObject.transform.Find("Current").gameObject.SetActive(true);
                }
                else
                {
                    // 为按钮绑定点击事件，点击时传递按钮对应的篝火名称
                    newObject.GetComponent<Button>().onClick.AddListener(() => OnTeleportClicked(buttonText));
                }
            }
        }

        /// <summary>
        /// 点击传送按钮时触发，调用传送事件。
        /// </summary>
        /// <param name="campFireName">目标篝火的名称。</param>
        void OnTeleportClicked(String campFireName)
        {
            // 调用传送事件，将目标篝火名称传递给订阅者(所有篝火）
            OnTeleport?.Invoke(campFireName);
        }
        
        // 单例模式实例，用于全局访问
        public static CampFireUIHandler instance;

        /// <summary>
        /// 初始化单例模式实例。
        /// </summary>
        void Awake()
        {
            instance = this;
        }
    }
}