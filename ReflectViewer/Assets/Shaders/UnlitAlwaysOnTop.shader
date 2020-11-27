Shader "Custom/UnlitAlwaysOnTop"
{
    Properties
    {
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
            ColorMask RGB

            CGPROGRAM
            #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 position : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.position);;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = _Color;

                return col;
            }
            ENDCG
        }
    }
}
