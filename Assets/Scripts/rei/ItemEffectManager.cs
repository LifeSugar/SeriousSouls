using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rei
{
    public class ItemEffectManager : MonoBehaviour
    {
        Dictionary<string, int> effects = new Dictionary<string, int>();

        public void CastEffect(string effectId, PlayerState playerStates) {
            int i = GetIntFromId(effectId);
            if (i < 0)
                return;

            switch (i)
            {
                case 0: //bestus
                    AddHealth(playerStates);
                    break;
                case 1: //focus
                    AddFocus(playerStates);
                    break;
                case 2: //souls
                    AddSouls(playerStates);
                    break;
            }
        }

        #region Effects Actual
        void AddHealth(PlayerState playerStates) {
            playerStates.characterStats._health += playerStates.characterStats._healthRecoverValue;
        }

        void AddFocus(PlayerState playerStates) {
            playerStates.characterStats._focus += playerStates.characterStats._focusRecoverValue;
        }

        void AddSouls(PlayerState playerStates)
        {
            playerStates.characterStats._souls += 100;
        }
        #endregion

        int GetIntFromId(string id) {
            int index = -1;
            if (effects.TryGetValue(id, out index)){
                return index;
            }

            return index;
        }

        void InitEffectsId() {
            effects.Add("AddHealth", 0);
            effects.Add("focus", 1);
            effects.Add("souls", 2);
        }

        public static ItemEffectManager singleton;
        private void Awake()
        {
            InitEffectsId();
            singleton = this;
        }
    }
}