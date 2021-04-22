Shader "Custom/SelectionOutline"
{
     Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _OutlineColor ("OutlineColor", Color) = (1.0, 0.4, 0.0, 0.0)
        _OutlineWidth ("OutlineWidth", Float) = 2
    }

    SubShader
    {
        CGINCLUDE
        #include "UnityCG.cginc"
        struct Input
        {
            float4 position : POSITION;
            float2 uv : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Varying
        {
            float4 position : SV_POSITION;
            float2 uv : TEXCOORD0;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        Varying vertex(Input input)
        {
            Varying output;

            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_INITIALIZE_OUTPUT(Varying, output);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            output.position = UnityObjectToClipPos(input.position);
            output.uv = UnityStereoTransformScreenSpaceTex(input.uv);
            return output;
        }
        ENDCG

        Tags { "RenderType"="Opaque" }

        // #0: Mask 1 in alpha, 1 in red
        Pass
        {
            Blend One Zero
            ZTest Always
            Cull Off
            ZWrite Off
            // push towards camera a bit, so that coord mismatch due to dynamic batching is not affecting us
            Offset -0.02, 0

            CGPROGRAM
            #pragma vertex vertex
            #pragma fragment fragment
            #pragma target 3.0
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            half4 fragment(Varying i) : SV_Target
            {
                return half4(1, 1, 1, 1);
            }
            ENDCG
        }

        // #1: final postprocessing pass
        Pass
        {
            ZTest Always
            Cull Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vertex
            #pragma fragment fragment
            #pragma target 3.0
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
            float4 _MainTex_TexelSize;
            half4 _OutlineColor;
            half _OutlineWidth;

            half4 fragment(Varying i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                half4 col = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv);

                bool isSelected = col.r == 1;
                float alpha = saturate(col.b * 10);
                if (isSelected)
                {
                    // outline color alpha controls how much tint the whole object gets
                    alpha = _OutlineColor.a;
                    if (any(i.uv - _MainTex_TexelSize.xy*_OutlineWidth < 0) || any(i.uv + _MainTex_TexelSize.xy*_OutlineWidth > 1))
                        alpha = 1;
                }

                half4 outlineColor = half4(_OutlineColor.rgb, alpha);
                return outlineColor;
            }
            ENDCG
        }

        // #2: separable blur pass, either horizontal or vertical
        Pass
        {
            ZTest Always
            Cull Off
            ZWrite Off

            CGPROGRAM
            #pragma vertex vertex
            #pragma fragment fragment
            #pragma target 3.0
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            float2 _BlurDirection;
            UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
            float4 _MainTex_TexelSize;
            half _OutlineWidth;

            // 9-tap Gaussian kernel, that blurs green & blue channels,
            // keeps red & alpha intact.
            static const half4 kCurveWeights[9] = {
                half4(0,0.0204001988,0.0204001988,0),
                half4(0,0.0577929595,0.0577929595,0),
                half4(0,0.1215916882,0.1215916882,0),
                half4(0,0.1899858519,0.1899858519,0),
                half4(1,0.2204586031,0.2204586031,1),
                half4(0,0.1899858519,0.1899858519,0),
                half4(0,0.1215916882,0.1215916882,0),
                half4(0,0.0577929595,0.0577929595,0),
                half4(0,0.0204001988,0.0204001988,0)
            };

            half4 fragment(Varying i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float2 step = _MainTex_TexelSize.xy * _BlurDirection * _OutlineWidth * 0.5;
                float2 uv = i.uv - step * 4;
                half4 col = 0;
                for (int tap = 0; tap < 9; ++tap)
                {
                    col += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, uv) * kCurveWeights[tap];
                    uv += step;
                }
                return col;
            }
            ENDCG
        }
    }
}
