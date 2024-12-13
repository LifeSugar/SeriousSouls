using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VolumetricFogAndMist2 {

    public static class Tools {

        public static Color ColorBlack = Color.black;
        public static void CheckCamera(ref Camera cam) {
            if (cam != null) return;
            cam = Camera.main;
            if (cam == null) {
                Camera[] cameras = Object.FindObjectsOfType<Camera>();
                for (int k = 0; k < cameras.Length; k++) {
                    if (cameras[k].isActiveAndEnabled && cameras[k].gameObject.activeInHierarchy) {
                        cam = cameras[k];
                        return;
                    }

                }
            }
        }

        public static VolumetricFogManager CheckMainManager() {
            VolumetricFogManager fog2 = Object.FindObjectOfType<VolumetricFogManager>();
            if (fog2 == null) {
                GameObject go = new GameObject();
                fog2 = go.AddComponent<VolumetricFogManager>();
                go.name = fog2.managerName;
            }
            return fog2;
        }


        public static void CheckManager<T>(ref T manager) where T : Component {
            if (manager == null) {
                manager = Object.FindObjectOfType<T>();
                if (manager == null) {
                    VolumetricFogManager root = CheckMainManager();
                    if (root != null) {
                        manager = Object.FindObjectOfType<T>();
                    }
                    if (manager == null) {
                        GameObject o = new GameObject();
                        o.transform.SetParent(root.transform, false);
                        manager = o.AddComponent<T>();
                        o.name = ((IVolumetricFogManager)manager).managerName;
                    }
                }
            }
        }
    }

}