using UnityEngine;

namespace rei
{
    public class WaterTrigger : MonoBehaviour
    {
        void OnTriggerEnter(Collider collision)
        {
            PlayerState ps = collision.GetComponent<PlayerState>();
            if (ps != null)
            {
                InputHandler.instance._playerStates.characterStats._health = 0;
                InputHandler.instance._playerStates.Die();
            }
        }
    }
}