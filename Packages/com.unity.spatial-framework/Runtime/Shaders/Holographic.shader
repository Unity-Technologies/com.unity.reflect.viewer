Shader "Hololens Shader Pack/Holographic"
{
	Properties
	{
		_Color("Color", Color) = (0.26,0.19,0.16,0.0)
		_Offset("Offset", Range(0.0,1.0)) = 0.0
		_Scale("Scale", Range(0.0,10.0)) = 1.0
		_RimPower("Rim Power", Range(0.1,8.0)) = 3.0

		[Header(Wire)]
		_WireColor("Wire Color", Color) = (0.26,0.19,0.16,0.0)
		[PowerSlider(2.0)]  _Amount("Wire Thickness", Range(0.001, 0.1)) = 0.01
	}
	
	SubShader{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent-1" }
		Blend OneMinusDstColor One

		Pass
		{
			Cull Back

			CGPROGRAM
			#include "HoloCP.cginc"

			#pragma vertex vert
			#pragma fragment frag

			fixed4 _Color;
			fixed _Offset;
			fixed _Scale;
			fixed _RimPower;

			struct v2f
			{
				fixed4 viewPos : SV_POSITION;
				fixed3 normal: NORMAL;
				fixed3 worldSpaceViewDir: TEXCOORD0;
				fixed4 world : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata_base v)
			{
				UNITY_SETUP_INSTANCE_ID(v);
				v2f o;
				o.viewPos = UnityObjectToClipPos(v.vertex);
				o.worldSpaceViewDir = WorldSpaceViewDir(v.vertex);
				o.normal = mul(unity_ObjectToWorld, fixed4(v.normal, 0.0)).xyz;
				o.world = mul(unity_ObjectToWorld, v.vertex);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				fixed4 o = 0;
				half rim = 1.0 - saturate(dot(normalize(i.worldSpaceViewDir), normalize(i.normal)));
				o.rgb = _Color.rgb * (_Offset + _Scale * pow(rim, _RimPower));
				return o;
			}
			ENDCG
		}

		Pass
		{
			Cull Front

			CGPROGRAM
			#include "HoloCP.cginc"

			#pragma vertex vert
			#pragma fragment frag

			fixed4 _WireColor;
			fixed _Amount;

			struct v2f
			{
				fixed4 viewPos : SV_POSITION;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata_base v)
			{
				UNITY_SETUP_INSTANCE_ID(v);
				v2f o;
				fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
				o.viewPos = UnityObjectToClipPos(v.vertex + v.normal * _Amount);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				return  _WireColor;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
