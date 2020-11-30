Shader "Custom/UnlitGradientAlwaysOnTop"
{
    Properties
    {
        _ColorBottom ("Color Bottom", Color) = (1.0, 1.0, 1.0, 1.0)
        _ColorTop ("Color Top", Color) = (1.0, 1.0, 1.0, 0.0)
        [Toggle] _EaseOut("Ease Out", Float) = 1
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
                float height : Output;
            };

            float4 _ColorBottom;
            float4 _ColorTop;
            float _EaseOut;

            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.position);;
                o.height = (v.position.y + 1)*0.5;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float objectHeight = i.height;
                if(_EaseOut == 1)
                {
                    objectHeight = 1 - (1-objectHeight)*(1-objectHeight);
                }
                fixed4 col = lerp(_ColorBottom, _ColorTop, objectHeight);

                return col;
            }
            ENDCG
        }
    }
}
