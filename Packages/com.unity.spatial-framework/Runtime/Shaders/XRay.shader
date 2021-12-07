Shader "Hololens Shader Pack/XRay"
{
	Properties
	{
		_Color("Color", Color) = (0.26,0.19,0.16,0.0)
		_Offset("Offset", Range(0.0,1.0)) = 0.0
		_Scale("Scale", Range(0.0,10.0)) = 1.0
		_RimPower("Rim Power", Range(0.1,8.0)) = 3.0
	}
	SubShader{
		Cull Back
		ZWrite Off
		Blend OneMinusDstColor One // Soft Additive

		Tags { "RenderType" = "Transparent"  "Queue" = "Transparent" }

		CGPROGRAM
		#include "HoloCP.cginc"

		#pragma surface surf Lambert

		struct Input {
			fixed3 viewDir;
			fixed3 worldPos;
		};

		fixed4 _Color;
		fixed _Offset;
		fixed _Scale;
		fixed _RimPower;
		void surf(Input IN, inout SurfaceOutput o) {
			half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
			o.Emission = saturate(_Color.rgb * (_Offset + _Scale * pow(rim, _RimPower)));
		}
		ENDCG
	}
	Fallback "Diffuse"
}
