using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rei {
    public class NPCScriptableObject : ScriptableObject
    {
        public NPCDialogue[] npcs;
    }

    [System.Serializable]
    public class NPCDialogue {
        public string npc_id;
        public Dialogue[] dialogue; //这里是为了处理外来可能存在的分支动画

    }

    [System.Serializable]
    public class Dialogue {
        public string[] dialogueText;
        public bool increaseIndex;
    }

}

