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
        public Dialogue[] dialogue;

    }

    [System.Serializable]
    public class Dialogue {
        public string[] dialogueText;
        public bool increaseIndex;
    }

}

