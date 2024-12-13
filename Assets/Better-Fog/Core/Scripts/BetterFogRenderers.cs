using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace INab.BetterFog.Core
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [ImageEffectAllowedInSceneView] // We need this flag
    public class BetterFogRenderers : MonoBehaviour
    {
        public List<CustomRenderer> depthRenderers = new List<CustomRenderer>();
        public List<CustomRenderer> fogOffsetRenderers = new List<CustomRenderer>();
    }


    [Serializable]
    public class CustomRenderer
    {
        public bool render = true;
        public bool alwaysRender = true;
        public Renderer renderer;
        public bool drawAllSubmeshes = false;
        public Material material;
    }
}