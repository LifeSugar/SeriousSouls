Shader "VolumetricFog2/VolumetricFog2DURP"
{
	Properties
	{
		[HideInInspector] _MainTex("Noise Texture", 2D) = "white" {}
		[HideInInspector] _DetailTex("Detail Texture", 3D) = "white" {}
		[HideInInspector] _NoiseScale("Noise Scale", Range(0.001, 0.04)) = 0.025
		[HideInInspector] _Color("Color", Color) = (1,1,1)
		[HideInInspector] _Density("Density", Float) = 1.0
		[HideInInspector] _FogStepping("Raymarch Steps", Range(1, 32)) = 2
		[HideInInspector] _DeepObscurance("Deep Obscurance", Range(0, 2)) = 0.7
		[HideInInspector] _LightDiffusionPower("Sun Diffusion Power", Range(1, 64)) = 32
		[HideInInspector] _LightDiffusionIntensity("Sun Diffusion Intensity", Range(0, 1)) = 0.4
		[HideInInspector] _ShadowIntensity("Sun Shadow Intensity", Range(0, 1)) = 0.5
		[HideInInspector] _WindDirection("Wind Direction", Vector) = (1, 0, 0)
		[HideInInspector] _DitherStrength("Dither Strength", Range(0, 2)) = 1.0
		[HideInInspector] _JitterStrength("Jitter Strength", Range(0, 2)) = 1.0
		[HideInInspector] _SunDir("Sun Direction", Vector) = (1,0,0)
		[HideInInspector] _FogOfWar("Fog Of War", 2D) = "white" {}
		[HideInInspector] _BoundsCenter("Bounds Center", Vector) = (0,0,0)
		[HideInInspector] _BoundsExtents("Bounds Size", Vector) = (0,0,0)
		[HideInInspector] _BoundsBorder("Bounds Border", Vector) = (0,1,0)
		[HideInInspector] _DetailStrength("Detail Strength", Range(0, 2)) = 0.5
		[HideInInspector] _DetailScale("Detail Scale", Float) = 4
	}
		SubShader
		{
			Tags { "RenderType" = "Transparent" "Queue" = "Transparent+100" "DisableBatching" = "True" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" }
			Blend One OneMinusSrcAlpha
			ZTest Always
			Cull Front
			ZWrite Off

			Pass
			{
				Tags { "LightMode" = "UniversalForward" }
				HLSLPROGRAM
				#pragma prefer_hlslcc gles
				#pragma exclude_renderers d3d11_9x
				#pragma target 3.0
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
				#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
                #pragma multi_compile _ VF2_DEPTH_PREPASS
				#pragma multi_compile_local _ VF2_POINT_LIGHTS
				#pragma multi_compile_local _ VF2_VOIDS
				#pragma multi_compile_local _ VF2_FOW
				#pragma multi_compile_local _ VF2_RECEIVE_SHADOWS
				#pragma multi_compile_local _ VF2_DISTANCE
				#pragma multi_compile_local V2F_SHAPE_BOX V2F_SHAPE_SPHERE
				#pragma multi_compile_local _ V2F_DETAIL_NOISE

				#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
				#undef SAMPLE_TEXTURE2D
				#define SAMPLE_TEXTURE2D(textureName, samplerName, coord2) SAMPLE_TEXTURE2D_LOD(textureName, samplerName, coord2, 0)
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

				#include "CommonsURP.hlsl"
				#include "Primitives.cginc"
				#include "ShadowsURP.cginc"
				#include "Raymarch2D.cginc"
				#include "PointLights.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f
				{
					float4 pos     : SV_POSITION;
                    float3 wpos    : TEXCOORD0;
					float4 scrPos  : TEXCOORD1;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
				};

				v2f vert(appdata v)
				{
					v2f o;

					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

					o.pos = TransformObjectToHClip(v.vertex.xyz);
				    o.wpos = TransformObjectToWorld(v.vertex.xyz);
					o.scrPos = ComputeScreenPos(o.pos);

					#if defined(UNITY_REVERSED_Z)
						o.pos.z = o.pos.w * UNITY_NEAR_CLIP_VALUE; //  1.0e-9f;
					#else
						o.pos.z = o.pos.w - 1.0e-6f;
					#endif

					return o;
				}


				half4 frag(v2f i) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID(i);
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

					float3 rayStart = GetRayStart(i.wpos);
					float3 ray = i.wpos - rayStart;
                   	float t1 = length(ray);
					float3 rayDir = ray / t1;

					#if V2F_SHAPE_SPHERE
						float t0;
						SphereIntersection(rayStart, rayDir, t0, t1);
					#else
						float t0 = BoxIntersection(rayStart, rayDir);
					#endif

					CLAMP_RAY_DEPTH(rayStart, i.scrPos, t1);
                  	if (t0>=t1) return 0;

					SetJitter(i.scrPos);

					half4 fogColor = GetFogColor(rayStart, rayDir, t0, t1);
					#if VF2_POINT_LIGHTS
						AddPointLights(fogColor, rayDir, t0, t1 - t0);
					#endif
					return fogColor;
				}
				ENDHLSL
			}

		}
}
