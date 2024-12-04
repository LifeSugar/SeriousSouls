using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VolumetricFogAndMist2 {

    public delegate void OnSettingsChanged();

    [CreateAssetMenu(menuName = "Volumetric Fog \x8B& Mist/Fog Profile", fileName = "VolumetricFogProfile", order = 1001)]
    public class VolumetricFogProfile : ScriptableObject {

        [Header("Rendering")]
        [Range(1, 16)] public int raymarchQuality = 6;
        [Range(0, 2)] public float dithering = 1f;
        [Range(0, 2)] public float jittering = 0.25f;
        [Tooltip("The render queue for this renderer. By default, all transparent objects use a render queue of 3000. Use a lower value to render before all transparent objects.")]
        public int renderQueue = 3100;
        [Tooltip("Optional sorting layer Id (number) for this renderer. By default 0. Usually used to control the order with other transparent renderers, like Sprite Renderer.")]
        public int sortingLayerID;
        [Tooltip("Optional sorting order for this renderer. Used to control the order with other transparent renderers, like Sprite Renderer.")]
        public int sortingOrder;

        [Header("Density")]
        public Texture2D noiseTexture;
        [Range(0, 3)] public float noiseStrength = 1f;
        public float noiseScale = 15f;
        public float noiseFinalMultiplier = 1f;

        public bool useDetailNoise;
        public Texture3D detailTexture;
        public float detailScale = 0.35f;
        [Range(0,1f)] public float detailStrength = 0.5f;

        public float density = 1f;

        [Header("Boundary")]
        public VolumetricFogShape shape = VolumetricFogShape.Box;
        [Range(0, 1f)] public float border = 0.05f;
        public float verticalOffset;
        [Tooltip("When enabled, makes fog appear at certain distance from a camera")]
        public float distance;
        [Range(0, 1)] public float distanceFallOff;

        [Header("Colors")]
        public Color albedo = new Color32(227, 227, 227, 255);
        public float brightness = 1f;
        [Range(0, 2)] public float deepObscurance = 1f;
        public Color specularColor = new Color(1, 1, 0.8f, 1);
        [Range(0, 1f)] public float specularThreshold = 0.637f;
        [Range(0, 1f)] public float specularIntensity = 0.428f;

        [Header("Animation")]
        public float turbulence = 0.73f;
        public Vector3 windDirection = new Vector3(0.02f, 0, 0);

        [Header("Directional Light")]
        [Range(0, 64)] public float lightDiffusionPower = 32;
        [Range(0, 1)] public float lightDiffusionIntensity = 0.4f;
        public bool receiveShadows;
        [Range(0, 1)] public float shadowIntensity = 0.5f;

        public event OnSettingsChanged onSettingsChanged;

        private void OnEnable() {
            if (noiseTexture == null) {
                noiseTexture = Resources.Load<Texture2D>("Textures/NoiseTex256");
            }
            if (detailTexture == null) {
                detailTexture = Resources.Load<Texture3D>("Textures/NoiseTex3D");
            }
        }

        private void OnValidate() {
            distance = Mathf.Max(0, distance);
            density = Mathf.Max(0, density);
            noiseScale = Mathf.Max(0.1f, noiseScale);
            detailScale = Mathf.Max(0.01f, detailScale);
            if (onSettingsChanged != null) {
                onSettingsChanged();
            }
        }

    }
}