Shader "Custom/Billboard"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        
        Pass
        {
            // 不写入深度，不进行深度测试
            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // 属性
            sampler2D _MainTex;
            float4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // 摄像机位置全局变量，由Unity自动传入
            // float3 _WorldSpaceCameraPos;

            v2f vert (appdata v)
            {
                v2f o;

                // 将顶点位置从对象空间变换到世界空间
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                // Billboard逻辑：
                // 将面朝向摄像机，需要计算当前物体的位置与摄像机位置的方向。
                // 假设物体的中心点位于 worldPos，实际中可能需要将世界位置移到物体中心再旋转。
                // 为简化，这里假定物体位置为worldPos本身就是中心。

                float3 camPos = _WorldSpaceCameraPos;
                float3 forward = normalize(worldPos - camPos);

                // Billboard通常需要让平面面向摄像机： 
                // forward指向摄像机到物体的方向，为了让物体面朝摄像机，我们需要将其法线指向 camera->object 反向
                // 不过这里 forward 是 (worldPos - camPos), 实际是由相机看向物体的方向
                // 如果想让正面朝向相机，应使用 (camPos - worldPos)
                forward = normalize(camPos - worldPos);

                // 定义一个默认的上方向（世界上方向）
                float3 up = float3(0,1,0);

                // 计算右方向
                float3 right = normalize(cross(up, forward));

                // 基于计算的 right 和 forward 来构建一个朝向摄像机的坐标系
                up = normalize(cross(forward, right));

                // 将顶点偏移 (相对于物体中心) 在局部转为世界，并最终输出到裁剪空间
                // 这里假设物体的原始顶点数据是以中心为原点的一个平面
                float3 localPos = v.vertex.xyz;

                // 将局部坐标(假设x,y为面板的平面坐标，z为0)映射到 "right" 和 "up" 上, 忽略原始的z
                float3 billboardPos = worldPos + localPos.x * right + localPos.y * up;

                // 转换到裁剪空间
                o.pos = UnityObjectToClipPos(float4(billboardPos, 1.0));
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv) * _Color;
                return c;
            }
            ENDCG
        }
    }
}