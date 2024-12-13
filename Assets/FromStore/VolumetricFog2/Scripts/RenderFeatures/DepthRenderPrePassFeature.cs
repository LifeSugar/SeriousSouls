using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

namespace VolumetricFogAndMist2 {
    public class DepthRenderPrePassFeature : ScriptableRendererFeature {

        public class DepthRenderPass : ScriptableRenderPass {

            const string m_ProfilerTag = "CustomDepthPrePass";
            const string SKW_DEPTH_PREPASS = "VF2_DEPTH_PREPASS";

            public static int layerMask;
            FilteringSettings m_FilteringSettings;

            List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

            RenderTargetHandle m_Depth;
            Material depthOnlyMaterial;

            public DepthRenderPass() {
                m_Depth.Init("_CustomDepthTexture");
                m_ShaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
                m_ShaderTagIdList.Add(new ShaderTagId("UniversalForward"));
                m_ShaderTagIdList.Add(new ShaderTagId("LightweightForward"));
                m_FilteringSettings = new FilteringSettings(RenderQueueRange.transparent, 0);
                Shader depthOnly = Shader.Find("Universal Render Pipeline/Unlit");
                if (depthOnly != null) {
                    depthOnlyMaterial = new Material(depthOnly);
                }
                SetupKeywords();
            }

            void SetupKeywords() {
                if (layerMask != 0) {
                    Shader.EnableKeyword(SKW_DEPTH_PREPASS);
                } else {
                    Shader.DisableKeyword(SKW_DEPTH_PREPASS);
                }
            }

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
                if (layerMask != m_FilteringSettings.layerMask) {
                    m_FilteringSettings = new FilteringSettings(RenderQueueRange.transparent, layerMask);
                    SetupKeywords();
                }
                RenderTextureDescriptor depthDesc = cameraTextureDescriptor;
                depthDesc.colorFormat = RenderTextureFormat.Depth;
                depthDesc.depthBufferBits = 32;
                depthDesc.msaaSamples = 1;

                cmd.GetTemporaryRT(m_Depth.id, depthDesc, FilterMode.Point);
                cmd.SetGlobalTexture("_CustomDepthTexture", m_Depth.Identifier());
                ConfigureTarget(m_Depth.Identifier());
                ConfigureClear(ClearFlag.All, Color.black);

            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
                if (layerMask == 0) return;
                CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                SortingCriteria sortingCriteria = SortingCriteria.CommonTransparent;
                var drawSettings = CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, sortingCriteria);
                drawSettings.perObjectData = PerObjectData.None;
                drawSettings.overrideMaterial = depthOnlyMaterial;

                ref CameraData cameraData = ref renderingData.cameraData;
                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref m_FilteringSettings);

                context.ExecuteCommandBuffer(cmd);

                CommandBufferPool.Release(cmd);
            }

            /// Cleanup any allocated resources that were created during the execution of this render pass.
            public override void FrameCleanup(CommandBuffer cmd) {
                if (cmd == null) return;
                cmd.ReleaseTemporaryRT(m_Depth.id);
            }
        }

        DepthRenderPass m_ScriptablePass;
        public static bool installed;

        public override void Create() {
            m_ScriptablePass = new DepthRenderPass() {
                // Configures where the render pass should be injected.
                renderPassEvent = RenderPassEvent.AfterRenderingOpaques
            };
        }

        void OnDestroy() {
            installed = false;
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
            installed = true;
            renderer.EnqueuePass(m_ScriptablePass);
        }

    }



}