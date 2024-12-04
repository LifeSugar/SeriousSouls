//------------------------------------------------------------------------------------------------------------------
// Volumetric Fog & Mist 2
// Created by Kronnect
//------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace VolumetricFogAndMist2 {

    public enum VolumetricFogShape {
        Box,
        Sphere
    }

    [ExecuteInEditMode]
    public partial class VolumetricFog : MonoBehaviour {

        public VolumetricFogProfile profile;

        public bool enablePointLights;
        public bool enableVoids;

        const string SKW_SHAPE_BOX = "V2F_SHAPE_BOX";
        const string SKW_SHAPE_SPHERE = "V2F_SHAPE_SPHERE";
        const string SKW_POINT_LIGHTS = "VF2_POINT_LIGHTS";
        const string SKW_VOIDS = "VF2_VOIDS";
        const string SKW_FOW = "VF2_FOW";
        const string SKW_RECEIVE_SHADOWS = "VF2_RECEIVE_SHADOWS";
        const string SKW_DISTANCE = "VF2_DISTANCE";
        const string SKW_DETAIL_NOISE = "V2F_DETAIL_NOISE";

        Renderer r;
        Material fogMat, noiseMat, turbulenceMat;
        Material fogMat2D, noiseMat2D, turbulenceMat2D;
        RenderTexture rtNoise, rtTurbulence;
        float turbAcum;
        Vector3 windDirectionAcum;
        Vector3 sunDir;
        float dayLight;
        List<string> shaderKeywords;
        Texture3D detailTex, refDetailTex;

        void OnEnable() {
            VolumetricFogManager manager = Tools.CheckMainManager();
            gameObject.layer = manager.fogLayer;
            FogOfWarInit();
            UpdateMaterialProperties();
        }

        private void OnDisable() {
            if (profile != null) {
                profile.onSettingsChanged -= UpdateMaterialProperties;
            }
        }

        private void OnValidate() {
            UpdateMaterialProperties();
        }

        private void OnDestroy() {
            if (rtNoise != null) {
                rtNoise.Release();
            }
            if (rtTurbulence != null) {
                rtTurbulence.Release();
            }
            if (fogMat != null) {
                DestroyImmediate(fogMat);
                fogMat = null;
            }
            FogOfWarDestroy();
        }

        void OnDrawGizmosSelected() {
            Gizmos.color = new Color(1, 1, 0, 0.75F);
            Gizmos.DrawWireCube(transform.position, transform.lossyScale);
        }

        void LateUpdate() {
            if (fogMat == null || r == null || profile == null) return;
            Bounds bounds = r.bounds;
            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;

            if (profile.shape == VolumetricFogShape.Sphere) {
                Vector3 scale = transform.localScale;
                if (scale.z != scale.x || scale.y != scale.x) {
                    scale.z = scale.y = scale.x;
                    transform.localScale = scale;
                    extents = r.bounds.extents;
                }
                extents.x *= extents.x;
            }

            Vector4 border = new Vector4(extents.x * profile.border + 0.0001f, extents.x * (1f - profile.border), extents.z * profile.border + 0.0001f, extents.z * (1f - profile.border));
            fogMat.SetVector("_BoundsCenter", center);
            fogMat.SetVector("_BoundsExtents", extents);
            fogMat.SetVector("_BoundsBorder", border);
            fogMat.SetFloat("_BoundsVerticalOffset", profile.verticalOffset);

            VolumetricFogManager globalManager = VolumetricFogManager.instance;
            Light sun = globalManager.sun;
            if (sun != null) {
                sunDir = -sun.transform.forward;
                fogMat.SetVector("_SunDir", sunDir);
                dayLight = 1f + sunDir.y * 2f;
                if (dayLight < 0) dayLight = 0; else if (dayLight > 1f) dayLight = 1f;
                float brightness;
                float alpha;
                if (profile != null) {
                    brightness = profile.brightness;
                    alpha = profile.albedo.a;
                } else {
                    brightness = 1f;
                    alpha = 1f;
                }
                Color lightColor = sun.color * (sun.intensity * brightness * dayLight * 2f);
                lightColor.a = alpha;
                fogMat.SetVector("_LightColor", lightColor);
            }

            windDirectionAcum += profile.windDirection * Time.deltaTime;
            fogMat.SetVector("_WindDirection", windDirectionAcum);

            transform.rotation = Quaternion.identity;

            UpdateNoise();

            if (enableFogOfWar) {
                UpdateFogOfWar();
            }
        }


        void UpdateNoise() {
            if (profile == null) return;
            Texture noiseTex = profile.noiseTexture as Texture2D;
            if (noiseTex == null) return;

            if (rtTurbulence == null || rtTurbulence.width != noiseTex.width) {
                RenderTextureDescriptor desc = new RenderTextureDescriptor(noiseTex.width, noiseTex.height, RenderTextureFormat.ARGB32, 0);
                rtTurbulence = new RenderTexture(desc);
                rtTurbulence.wrapMode = TextureWrapMode.Repeat;
            }
            turbAcum += Time.deltaTime * profile.turbulence;
            turbulenceMat.SetFloat("_Amount", turbAcum);
            turbulenceMat.SetFloat("_NoiseStrength", profile.noiseStrength);
            turbulenceMat.SetFloat("_NoiseFinalMultiplier", profile.noiseFinalMultiplier);
            Graphics.Blit(noiseTex, rtTurbulence, turbulenceMat);

            if (rtNoise == null || rtNoise.width != noiseTex.width) {
                RenderTextureDescriptor desc = new RenderTextureDescriptor(noiseTex.width, noiseTex.height, RenderTextureFormat.ARGB32, 0);
                rtNoise = new RenderTexture(desc);
                rtNoise.wrapMode = TextureWrapMode.Repeat;
            }
            noiseMat.SetColor("_SpecularColor", profile.specularColor);
            noiseMat.SetFloat("_SpecularIntensity", profile.specularIntensity);

            float spec = 1.0001f - profile.specularThreshold;
            float nlighty = sunDir.y > 0 ? (1.0f - sunDir.y) : (1.0f + sunDir.y);
            float nyspec = nlighty / spec;

            noiseMat.SetFloat("_SpecularThreshold", nyspec);
            noiseMat.SetVector("_SunDir", sunDir);

            Color ambientColor = RenderSettings.ambientLight;
            float ambientIntensity = RenderSettings.ambientIntensity;
            Color ambientMultiplied = ambientColor * ambientIntensity;
            float fogIntensity = 1.15f;
            fogIntensity *= dayLight;
            Color textureBaseColor = Color.Lerp(ambientMultiplied, profile.albedo * fogIntensity, fogIntensity);

            noiseMat.SetColor("_Color", textureBaseColor);
            Graphics.Blit(rtTurbulence, rtNoise, noiseMat);

            fogMat.SetTexture("_MainTex", rtNoise);
        }

        public void UpdateMaterialProperties() {

            if (!gameObject.activeInHierarchy) return;

            r = GetComponent<Renderer>();

            if (profile == null) {
                if (fogMat == null && r != null) {
                    fogMat = new Material(Shader.Find("VolumetricFog2/Empty"));
                    fogMat.hideFlags = HideFlags.DontSave;
                    r.sharedMaterial = fogMat;
                }
                return;
            }
            profile.onSettingsChanged -= UpdateMaterialProperties;
            profile.onSettingsChanged += UpdateMaterialProperties;

            if (fogMat2D == null) {
                fogMat2D = new Material(Shader.Find("VolumetricFog2/VolumetricFog2DURP"));
                fogMat2D.hideFlags = HideFlags.DontSave;
            }
            fogMat = fogMat2D;
            if (turbulenceMat2D == null) {
                turbulenceMat2D = new Material(Shader.Find("VolumetricFog2/Turbulence2D"));
            }
            turbulenceMat = turbulenceMat2D;
            if (noiseMat2D == null) {
                noiseMat2D = new Material(Shader.Find("VolumetricFog2/Noise2DGen"));
            }
            noiseMat = noiseMat2D;

            if (r != null) {
                r.sharedMaterial = fogMat;
            }

            if (fogMat == null || profile == null) return;

            r.sortingLayerID = profile.sortingLayerID;
            r.sortingOrder = profile.sortingOrder;
            fogMat.renderQueue = profile.renderQueue;
            float noiseScale = 0.1f / profile.noiseScale;
            fogMat.SetFloat("_NoiseScale", noiseScale);
            fogMat.SetFloat("_DeepObscurance", profile.deepObscurance);
            fogMat.SetFloat("_LightDiffusionPower", profile.lightDiffusionPower);
            fogMat.SetFloat("_LightDiffusionIntensity", profile.lightDiffusionIntensity);
            fogMat.SetFloat("_ShadowIntensity", profile.shadowIntensity);
            fogMat.SetFloat("_Density", profile.density);
            fogMat.SetFloat("_FogStepping", profile.raymarchQuality);
            fogMat.SetFloat("_DitherStrength", profile.dithering * 0.01f);
            fogMat.SetFloat("_JitterStrength", profile.jittering);

            if (profile.useDetailNoise) {
                fogMat.SetFloat("_DetailStrength", profile.detailStrength);
                fogMat.SetFloat("_DetailScale", (1f / profile.detailScale) * noiseScale);
                if ((detailTex == null || refDetailTex != profile.detailTexture) && profile.detailTexture != null) {
                    refDetailTex = profile.detailTexture;
                    Texture3D tex = new Texture3D(profile.detailTexture.width, profile.detailTexture.height, profile.detailTexture.depth, TextureFormat.Alpha8, false);
                    tex.filterMode = FilterMode.Bilinear;
                    Color32[] colors = profile.detailTexture.GetPixels32();
                    for (int k=0;k<colors.Length;k++) { colors[k].a = colors[k].r; }
                    tex.SetPixels32(colors);
                    tex.Apply();
                    detailTex = tex;
                }
                fogMat.SetTexture("_DetailTex", detailTex);
            }

            if (shaderKeywords == null) {
                shaderKeywords = new List<string>();
            } else {
                shaderKeywords.Clear();
            }

            if (profile.distance > 0) {
                fogMat.SetVector("_DistanceData", new Vector4(0, 10f * (1f - profile.distanceFallOff), 0, 1f / (0.0001f + profile.distance * profile.distance)));
                shaderKeywords.Add(SKW_DISTANCE);
            }
            if (profile.shape == VolumetricFogShape.Box) shaderKeywords.Add(SKW_SHAPE_BOX); else shaderKeywords.Add(SKW_SHAPE_SPHERE);
            if (enablePointLights) shaderKeywords.Add(SKW_POINT_LIGHTS);
            if (enableVoids) shaderKeywords.Add(SKW_VOIDS);
            if (profile.receiveShadows) shaderKeywords.Add(SKW_RECEIVE_SHADOWS);
            if (enableFogOfWar) {
                fogMat.SetTexture("_FogOfWar", fogOfWarTexture);
                fogMat.SetVector("_FogOfWarCenter", fogOfWarCenter);
                fogMat.SetVector("_FogOfWarSize", fogOfWarSize);
                Vector3 ca = fogOfWarCenter - 0.5f * fogOfWarSize;
                fogMat.SetVector("_FogOfWarCenterAdjusted", new Vector3(ca.x / fogOfWarSize.x, 1f, ca.z / (fogOfWarSize.z + 0.0001f)));
                shaderKeywords.Add(SKW_FOW);
            }
            if (profile.useDetailNoise) shaderKeywords.Add(SKW_DETAIL_NOISE);
            fogMat.shaderKeywords = shaderKeywords.ToArray();
        }


    }


}
