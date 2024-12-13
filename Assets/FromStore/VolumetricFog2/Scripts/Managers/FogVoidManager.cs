using UnityEngine;

namespace VolumetricFogAndMist2 {

    [ExecuteInEditMode]
    public class FogVoidManager : MonoBehaviour, IVolumetricFogManager {

        public string managerName {
            get {
                return "Fog Void Manager";
            }
        }

        public const int MAX_FOG_VOID = 8;

        [Header("Void Search Settings")]
        public Transform trackingCenter;
        public float newFogVoidCheckInterval = 3f;

        FogVoid[] fogVoids;
        Vector4[] fogVoidPositionAndSizes;
        float checkNewFogVoidLastTime;
        bool requireRefresh;


        private void OnEnable() {
            if (trackingCenter == null) {
                Camera cam = null;
                Tools.CheckCamera(ref cam);
                if (cam != null) {
                    trackingCenter = cam.transform;
                }
            }
            if (fogVoidPositionAndSizes == null || fogVoidPositionAndSizes.Length != MAX_FOG_VOID) {
                fogVoidPositionAndSizes = new Vector4[MAX_FOG_VOID];
            }
        }

        void SubmitFogVoidData() {

            int k = 0;
            for (int i = 0; k < MAX_FOG_VOID && i < fogVoids.Length; i++) {
                FogVoid fogVoid = fogVoids[i];
                if (fogVoid == null || !fogVoid.isActiveAndEnabled) continue;
                Vector3 pos = fogVoid.transform.position;
                fogVoidPositionAndSizes[k].x = pos.x;
                fogVoidPositionAndSizes[k].y = 10f * (1f - fogVoid.falloff);
                fogVoidPositionAndSizes[k].z = pos.z;
                fogVoidPositionAndSizes[k].w = 1f / (0.0001f + fogVoid.radius * fogVoid.radius);
                k++;
            }
            Shader.SetGlobalVectorArray("_VF2_FogVoidPositionAndSizes", fogVoidPositionAndSizes);
            Shader.SetGlobalInt("_VF2_FogVoidCount", k);
        }

        /// <summary>
        /// Look for nearest point lights
        /// </summary>
        public void TrackFogVoids(bool forceImmediateUpdate = false) {

            // Look for new lights?
            if (forceImmediateUpdate || fogVoids == null || !Application.isPlaying || (newFogVoidCheckInterval > 0 && Time.time - checkNewFogVoidLastTime > newFogVoidCheckInterval)) {
                checkNewFogVoidLastTime = Time.time;
                fogVoids = Object.FindObjectsOfType<FogVoid>();
                System.Array.Sort(fogVoids, fogVoidDistanceComparer);
            }
        }

        int fogVoidDistanceComparer(FogVoid v1, FogVoid v2) {
            float dist1 = (v1.transform.position - trackingCenter.position).sqrMagnitude;
            float dist2 = (v2.transform.position - trackingCenter.position).sqrMagnitude;
            if (dist1 < dist2) return -1;
            if (dist1 > dist2) return 1;
            return 0;
        }

        void LateUpdate() {
            if (requireRefresh) {
                requireRefresh = false;
                TrackFogVoids(true);
            } else {
                TrackFogVoids();
            }
            SubmitFogVoidData();
        }

        public void Refresh() {
            requireRefresh = true;
        }


    }

}