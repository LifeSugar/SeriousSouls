using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rei
{
    public class ResourceManager : MonoBehaviour
    {
        
        //Dictionary<键，值>
        Dictionary<string, int> weapon_ids = new Dictionary<string, int>();
        Dictionary<string, int> spell_ids = new Dictionary<string, int>();
        Dictionary<string, int> consum_ids = new Dictionary<string, int>();
        Dictionary<string, int> item_ids = new Dictionary<string, int>();
        Dictionary<string, int> interaction_ids = new Dictionary<string, int>();
        Dictionary<string, int> npc_ids = new Dictionary<string, int>();
        Dictionary<string, int> audio_ids = new Dictionary<string, int>();

        public static ResourceManager instance;

        private void Awake()
        {
            instance = this;
            
            LoadWeaponIds();
            LoadSpellIds();
            LoadConsumables();
            LoadItems();
            LoadInteractions();
            LoadNpcs();
            LoadAudios();
        }

        //将 WeaponScriptableObject 中所有武器项的名称和对应的索引加载到 weapon_ids 字典中，以便后续根据名称快速检索武器。
        void LoadWeaponIds()
        {
            // 从 Resources 文件夹加载名为 "rei.WeaponScriptableObject" 的 WeaponScriptableObject 资源
            WeaponScriptableObject obj = Resources.Load("rei.WeaponScriptableObject") as WeaponScriptableObject;

            // 检查加载是否成功
            if (obj == null)
            {
                Debug.Log("rei.WeaponScriptableObject could not be loaded!"); // 输出错误信息
                return; // 如果加载失败，直接返回
            }
            else
            {
                Debug.Log("rei.Weapon loaded!");
                Debug.Log(obj.weapons_all[0].itemName);
            }

            // 遍历 weapons_all 列表，将每个武器的名称和索引存储到字典 weapon_ids 中
            for (int i = 0; i < obj.weapons_all.Count; i++)
            {
                // 检查字典中是否已包含同名武器，避免重复
                if (weapon_ids.ContainsKey(obj.weapons_all[i].itemName))
                {
                    Debug.Log("Weapon item is a duplicate"); // 输出重复项信息
                }
                else
                {
                    // 将武器名称和其在列表中的索引添加到字典中
                    weapon_ids.Add(obj.weapons_all[i].itemName, i);
                }
            }
        }

        //用于通过武器名称检索 Weapon 对象。此方法适用于快速定位特定武器
        public Weapon GetWeapon(string name)
        {
            // 加载 WeaponScriptableObject 资源以获取所有武器数据
            WeaponScriptableObject obj = Resources.Load("rei.WeaponScriptableObject") as WeaponScriptableObject;

            // 检查是否成功加载 WeaponScriptableObject 资源
            if (obj == null)
            {
                Debug.Log("rei.WeaponScriptableObject could not be loaded!");
                return null;
            }

            // 使用武器名称通过 GetWeaponIdfromString 方法获取对应的索引
            int index = GetWeaponIdfromString(name);
            if (index == -1)
            {
                Debug.Log("get no weapon");
                return null;
            }
                

            // 根据索引返回武器对象
            return obj.weapons_all[index];
        }

        int GetWeaponIdfromString(string name)
        {
            int index = -1;


            if (weapon_ids.TryGetValue(name, out index))
            {
                return index;
            }

            return -1;
        }
        
        
        //之后的东西同理
        
        void LoadSpellIds()
        {
            SpellItemScriptableObject obj = Resources.Load("rei.SpellItemScriptableObject") as SpellItemScriptableObject;

            if (obj == null)
            {
                Debug.Log("rei.SpellItemScriptableObject could not be loaded!");
                return;
            }

            for (int i = 0; i < obj.spell_items.Count; i++)
            {
                if (spell_ids.ContainsKey(obj.spell_items[i].itemName))
                {
                    Debug.Log("Spell item is a duplicate");
                }
                else
                {
                    spell_ids.Add(obj.spell_items[i].itemName, i);
                }
            }
        }

        int GetSpellIdFromString(string name) {
            int index = -1;

            if (spell_ids.TryGetValue(name, out index))
            {
                return index;
            }

            return -1;
            
        }

        public Spell GetSpell(string name) {
            SpellItemScriptableObject obj = Resources.Load("rei.SpellItemScriptableObject") as SpellItemScriptableObject;

            if (obj == null)
            {
                Debug.Log("rei.SpellItemScriptableObject could not be loaded!");
                return null;
            }

            int index = GetSpellIdFromString(name);
            if (index == -1)
                return null;

            return obj.spell_items[index];
        }

        // -------------------------Consumables--------------------------

        void LoadConsumables() {
            ConsumablesScriptableObject obj = Resources.Load("rei.ConsumablesScriptableObject") as ConsumablesScriptableObject;

            if (obj == null)
            {
                Debug.Log("rei.ConsumablesScriptableObject could not be loaded!");
                return;
            }

            for (int i = 0; i < obj.consumables.Count; i++)
            {
                if (consum_ids.ContainsKey(obj.consumables[i].itemName))
                {
                    Debug.Log("Consumable item is a duplicate");
                }
                else
                {
                    consum_ids.Add(obj.consumables[i].itemName, i);
                }
            }
        }

        int GetConsumableIdFromString(string name)
        {
            int index = -1;

            if (consum_ids.TryGetValue(name, out index))
            {
                return index;
            }

            return -1;

        }

        public Consumable GetConsumable(string name)
        {
            ConsumablesScriptableObject obj = Resources.Load("rei.ConsumablesScriptableObject") as ConsumablesScriptableObject;

            if (obj == null)
            {
                Debug.Log("rei.ConsumablesScriptableObject could not be loaded!");
                return null;
            }

            int index = GetConsumableIdFromString(name);
            if (index == -1)
                return null;

            return obj.consumables[index];
        }

        //--------------------------Items---------------------------------

        void LoadItems()
        {
            ItemScriptableObject obj = Resources.Load("rei.ItemScriptableObject") as ItemScriptableObject;

            if (obj == null)
            {
                Debug.Log("rei.ItemScriptableObject could not be loaded!");
                return;
            }

            for (int i = 0; i < obj.items.Count; i++)
            {
                if (item_ids.ContainsKey(obj.items[i].itemName))
                {
                    Debug.Log("Item is a duplicate");
                }
                else
                {
                    item_ids.Add(obj.items[i].itemName, i);
                }
            }
        }

        int GetItemIdFromString(string name)
        {
            int index = -1;

            if (item_ids.TryGetValue(name, out index))
            {
                return index;
            }

            return -1;

        }

        public Item GetItem(string name)
        {
            ItemScriptableObject obj = Resources.Load("rei.ItemScriptableObject") as ItemScriptableObject;

            if (obj == null)
            {
                Debug.Log("rei.ItemScriptableObject could not be loaded!");
                return null;
            }

            int index = GetItemIdFromString(name);
            if (index == -1)
                return null;

            return obj.items[index];
        }

        //------------------------Interactions-----------------------------

        void LoadInteractions()
        {
            InteractionScriptableObject obj = Resources.Load("rei.InteractionScriptableObject") as InteractionScriptableObject;

            if (obj == null)
            {
                Debug.Log("rei.InteractionScriptableObject could not be loaded!");
                return;
            }

            for (int i = 0; i < obj.interactions.Count; i++)
            {
                if (interaction_ids.ContainsKey(obj.interactions[i].interactionId))
                {
                    Debug.Log("Interaction is a duplicate");
                }
                else
                {
                    interaction_ids.Add(obj.interactions[i].interactionId, i);
                }
            }
        }

        int GetInteractionIdFromString(string name)
        {
            int index = -1;

            if (interaction_ids.TryGetValue(name, out index))
            {
                return index;
            }

            return -1;

        }

        public Interactions GetInteraction(string name)
        {
            InteractionScriptableObject obj = Resources.Load("rei.InteractionScriptableObject") as InteractionScriptableObject;

            if (obj == null)
            {
                Debug.Log("rei.InteractionScriptableObject could not be loaded!");
                return null;
            }

            int index = GetInteractionIdFromString(name);
            if (index == -1)
                return null;

            return obj.interactions[index];
        }

        //------------------------NPCs-----------------------------

        void LoadNpcs()
        {
            NPCScriptableObject obj = Resources.Load("rei.NPCScriptableObject") as NPCScriptableObject;

            if (obj == null)
            {
                Debug.Log("rei.NPCScriptableObject could not be loaded!");
                return;
            }

            for (int i = 0; i < obj.npcs.Length; i++)
            {
                if (npc_ids.ContainsKey(obj.npcs[i].npc_id))
                {
                    Debug.Log("NPC is a duplicate");
                }
                else
                {
                    npc_ids.Add(obj.npcs[i].npc_id, i);
                }
            }
        }

        int GetNPCfromString(string name)
        {
            int index = -1;

            if (npc_ids.TryGetValue(name, out index))
            {
                return index;
            }

            return -1;

        }

        public NPCDialogue GetNPCDialogue(string name)
        {
            NPCScriptableObject obj = Resources.Load("rei.NPCScriptableObject") as NPCScriptableObject;

            if (obj == null)
            {
                Debug.Log("rei.NPCScriptableObject could not be loaded!");
                return null;
            }

            int index = GetNPCfromString(name);
            if (index == -1)
                return null;

            return obj.npcs[index];
        }

        //------------------------Audios-----------------------------

        void LoadAudios()
        {
            AudioScriptableObject obj = Resources.Load("rei.AudioScriptableObject") as AudioScriptableObject;

            if (obj == null)
            {
                Debug.Log("rei.AudioScriptableObject could not be loaded!");
                return;
            }

            for (int i = 0; i < obj.audio_list.Count; i++)
            {
                if (audio_ids.ContainsKey(obj.audio_list[i].id))
                {
                    Debug.Log("Audio is a duplicate");
                }
                else
                {
                    audio_ids.Add(obj.audio_list[i].id, i);
                }
            }
        }

        int GetAudioFromString(string name)
        {
            int index = -1;

            if (audio_ids.TryGetValue(name, out index))
            {
                return index;
            }

            return -1;

        }

        public Audio GetAudio(string name)
        {
            AudioScriptableObject obj = Resources.Load("rei.AudioScriptableObject") as AudioScriptableObject;

            if (obj == null)
            {
                Debug.Log("rei.AudioScriptableObject could not be loaded!");
                return null;
            }

            int index = GetAudioFromString(name);
            if (index == -1)
                return null;

            return obj.audio_list[index];
        }
    }
    
    
}