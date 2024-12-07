using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

namespace rei
{
    public class ScriptableObjectManager : MonoBehaviour
    {
        // 泛型静态方法，用于创建指定类型的 ScriptableObject 资源
        public static void CreateAsset<T>() where T : ScriptableObject
        {
            // 创建一个指定类型 T 的 ScriptableObject 实例
            T asset = ScriptableObject.CreateInstance<T>();

            // 检查 Resources 文件夹中是否已有同类型的资源
            if (Resources.Load(typeof(T).ToString()) == null)
            {
                // 如果未找到同类型资源，则生成资源的唯一路径并创建资源
                string assetPath =
                    AssetDatabase.GenerateUniqueAssetPath("Assets/Resources/" + typeof(T).ToString() + ".asset");

                AssetDatabase.CreateAsset(asset, assetPath); // 在指定路径创建 ScriptableObject 资源
                AssetDatabase.SaveAssets(); // 保存资源
                AssetDatabase.Refresh(); // 刷新编辑器以显示新资源
                EditorUtility.FocusProjectWindow(); // 聚焦到项目窗口
                Selection.activeObject = asset; // 选中新创建的资源
            }
            else
            {
                // 如果资源已存在，输出一条日志信息
                Debug.Log(typeof(T).ToString() + " already created!");
            }
        }

        // 菜单项方法，在编辑器菜单 "Assets/Inventory" 下创建一个 ConsumablesScriptableObject
        [MenuItem("Assets/Inventory/Create Consumables List")]
        public static void CreateConsumables()
        {
            ScriptableObjectManager.CreateAsset<ConsumablesScriptableObject>();
        }

        // 创建一个 WeaponScriptableObject
        [MenuItem("Assets/Inventory/CreateWeaponList")]
        public static void CreateWeaponList()
        {
            ScriptableObjectManager.CreateAsset<WeaponScriptableObject>();
        }

        // 创建一个 SpellItemScriptableObject
        [MenuItem("Assets/Inventory/CreateSpellItemList")]
        public static void CreateSpellItemList()
        {
            ScriptableObjectManager.CreateAsset<SpellItemScriptableObject>();
        }

        // 创建一个 ItemScriptableObject
        [MenuItem("Assets/Inventory/CreateItemList")]
        public static void CreateItemList()
        {
            ScriptableObjectManager.CreateAsset<ItemScriptableObject>();
        }

        // 创建一个 InteractionScriptableObject
        [MenuItem("Assets/Inventory/CreateInteractionList")]
        public static void CreateInteractionList()
        {
            ScriptableObjectManager.CreateAsset<InteractionScriptableObject>();
        }

        // 创建一个 NPCScriptableObject
        [MenuItem("Assets/Inventory/CreateNPCList")]
        public static void CreateNPCList()
        {
            ScriptableObjectManager.CreateAsset<NPCScriptableObject>();
        }

        // 创建一个 AudioScriptableObject
        [MenuItem("Assets/Inventory/CreateAudioList")]
        public static void CreateAudioList()
        {
            ScriptableObjectManager.CreateAsset<AudioScriptableObject>();
        }
        
        [MenuItem("Assets/Inventory/CreateKeyList")]
        public static void CreateKeyList()
        {
            ScriptableObjectManager.CreateAsset<KeyScriptableObject>();
        }
    }
}