using UnityEngine;
using UnityEditor;

namespace VolumetricFogAndMist2 {

    public class VolumetricFog2EditorIntegration : MonoBehaviour {

        [MenuItem("GameObject//Effects/Volumetric Fog 2/Manager", false, 100)]
        public static void CreateManager(MenuCommand menuCommand) {
            VolumetricFogManager fog2 = Tools.CheckMainManager();
            Selection.activeObject = fog2.gameObject;
        }


        [MenuItem("GameObject/Effects/Volumetric Fog 2/Fog Volume", false, 120)]
        public static void CreateFogVolume(MenuCommand menuCommand) {
            GameObject go = VolumetricFogManager.CreateFogVolume("Fog Volume");
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create Fog Volume");
            Selection.activeObject = go;
        }

        [MenuItem("GameObject//Effects/Volumetric Fog 2/Fog Void", false, 121)]
        public static void CreateFogVoid(MenuCommand menuCommand) {
            GameObject go = VolumetricFogManager.CreateFogVoid("Fog Void");
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create Fog Void");
            Selection.activeObject = go;
        }
    }

}