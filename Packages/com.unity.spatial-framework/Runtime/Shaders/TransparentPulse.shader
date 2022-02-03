Shader "SpatialFramework/TransparentPulse"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}

		_PulseSpeed ("Pulse Speed", Float) = 30.0
		_PulseMaxAlpha ("Max Pulse Alpha", Range (0, 1)) = 0.5
		_PulseMinAlpha ("Min Pulse Alpha", Range (0, 1)) = 0

		_WaveSpeed ("Shine Speed", Float) = 30.0
        _WaveSize ("Shine Frequency", Range (0.1, 5)) = .5

		_WaveMaxAlpha ("Max Shine Alpha", Range (0, 1)) = 0.5
		_WaveMinAlpha ("Min Shine Alpha", Range (0, 1)) = 0

		_MaxDistance ("Max Distance", Range (0.1, 100)) = 1
        _DistanceFadeFactor ("Distance Fade Start", Range (0, 1)) = .5
	}

	SubShader
	{
		Tags {"Queue"="Transparent" "RenderType"="Transparent"}
		LOD 100
		ZWrite On
		Offset -1,-1
		Blend One OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 realWorldCameraDistance : COLOR;
			};

			sampler2D _MainTex;

            fixed _WorldScale;

			float4 _MainTex_ST;
			fixed4 _Color;
			float _PulseSpeed;
			float _PulseMinAlpha;
			float _PulseMaxAlpha;

            float _WaveSpeed;
			float _WaveMinAlpha;
			float _WaveMaxAlpha;
			fixed _WaveSize;

			float _MaxDistance;
			float _DistanceFadeFactor;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.realWorldCameraDistance = ( mul(unity_ObjectToWorld, v.vertex).xyz - _WorldSpaceCameraPos.xyz )/ _WorldScale;
                o.realWorldCameraDistance.y = 0;

				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv) * _Color;
				float pulsePhase = 0.5 * (sin(_Time * _PulseSpeed) + 1.0); // Map time to sin wave from 0 - 1
				float pulseAlpha = _PulseMinAlpha + (pulsePhase)*(_PulseMaxAlpha - _PulseMinAlpha); // Remap wave to min/max alpha range

				float distance = length(i.realWorldCameraDistance);
                float wavePhase = _Time*_WaveSpeed + distance*-_WaveSize;
				float waveAlpha = clamp( sin(wavePhase), 0.0, 1.0)*(_WaveMaxAlpha - _WaveMinAlpha) + _WaveMinAlpha;
				float distanceFade = 1 - _DistanceFadeFactor * pow(saturate(distance/_MaxDistance), 1.5);
				col *= max(pulseAlpha, waveAlpha * distanceFade);
				return col;
			}
			ENDCG
		}


	}
}
