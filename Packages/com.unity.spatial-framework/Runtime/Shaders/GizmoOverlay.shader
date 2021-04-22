Shader "Unity/Handles/GizmoOverlay"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Power ("Power", float) = 1.9
        _EdgeShading ("Edge Shading", float) = 0.4
        _EdgeSize ("Edge Size", float) = 0.4

    }

    SubShader
    {
        Tags
        {
            "IgnoreProjector"="True" "RenderType"="Transparent"
        }
        Lighting Off

        Pass {
            ZWrite Off
            ZTest Always

            //Blendop Max
            Blend SrcAlpha One

          CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;

                // https://www.opengl.org/discussion_boards/showthread.php/166719-Clean-Wireframe-Over-Solid-Mesh
                o.pos = float4(UnityObjectToViewPos(v.vertex.xyz), 1);
                o.pos.xyz *= .99;
                o.pos = mul(UNITY_MATRIX_P, o.pos);
                return o;
            }


            half4 frag (v2f i) : COLOR
            {
                float4 color = _Color;
                color.a *= 0.5;
                color.rgb *= 1.3f;
                return color;
            }
            ENDCG
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _Color;
            float _Power;
            float _EdgeShading;
            float _EdgeSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 WorldSpaceNormal : TEXCOORD0;
                float3 WorldSpaceViewDirection : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;

                // https://www.opengl.org/discussion_boards/showthread.php/166719-Clean-Wireframe-Over-Solid-Mesh
                o.pos = float4(UnityObjectToViewPos(v.vertex.xyz), 1);
                o.pos.xyz *= .99;
                o.pos = mul(UNITY_MATRIX_P, o.pos);

                float3 WorldSpaceNormal =UnityObjectToWorldNormal(v.normal);
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                float3 WorldSpaceViewDirection = normalize(UnityWorldSpaceViewDir(worldPos));
                o.WorldSpaceNormal = WorldSpaceNormal;
                o.WorldSpaceViewDirection = WorldSpaceViewDirection;
                return o;
            }

            void Unity_FresnelEffect_float(float3 Normal, float3 ViewDir, float Power, out float Out)
            {
                Out = pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
            }

            void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
            {
                Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }

            half4 frag (v2f i) : COLOR
            {
                float fresnel;
                Unity_FresnelEffect_float(i.WorldSpaceNormal, i.WorldSpaceViewDirection, _Power, fresnel);
                float remap;
                Unity_Remap_float(abs(1-fresnel), float2(0,1), float2(-_EdgeSize, 1), remap);
                float4 edgeColor = _Color * float4((1-_EdgeShading).xxx, 1.0);
                float4 color = max(edgeColor, remap.xxxx * _Color);
                return color;
            }

            ENDCG
        }
    }
}
