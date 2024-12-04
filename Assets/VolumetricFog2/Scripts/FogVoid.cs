using UnityEngine;

namespace VolumetricFogAndMist2 {

    [ExecuteInEditMode]
    public class FogVoid : MonoBehaviour {

        public float radius = 10f;
        [Range(0,1)] public float falloff = 0;

        private void OnEnable() {
            VolumetricFogManager.fogVoidManager.Refresh();
        }

        void OnDrawGizmosSelected() {
            Gizmos.color = new Color(1, 1, 0, 0.75F);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}