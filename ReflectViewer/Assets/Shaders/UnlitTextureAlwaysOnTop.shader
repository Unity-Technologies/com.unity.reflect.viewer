Shader "Custom/UnlitTextureAlwaysOnTop"
{
    Properties
    {
        _Texture ("Texture", 2D) = "white" { }
        _Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
    }
    SubShader
    {
        Tags {
            "RenderType"="Transparent"
            "Queue"="Transparent+110"
            }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Back
            ZTest Always
            ZWrite Off

            CGPROGRAM
            #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _Color;
            sampler2D _Texture;
            float4 _Texture_ST;

            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.position);
                o.uv = TRANSFORM_TEX(v.uv, _Texture);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_Texture, i.uv);
                fixed4 col = _Color * tex;

                return col;
            }
            ENDCG
        }
    }
}
