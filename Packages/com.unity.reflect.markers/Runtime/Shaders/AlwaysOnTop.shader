﻿Shader "Markers/AlwaysOnTop_Unlit" {
	Properties {
        _MainTex ("Base", 2D) = "white" {}
        _Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
	    [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Int) = 0
    }
   
    CGINCLUDE
 
        #include "UnityCG.cginc"
 
        sampler2D _MainTex;
        fixed4 _Color;
       
        half4 _MainTex_ST;
                       
        struct v2f {
            half4 pos : SV_POSITION;
            half2 uv : TEXCOORD0;
            fixed4 vertexColor : COLOR;
        };
 
        v2f vert(appdata_full v) {
            v2f o;
           
            o.pos = UnityObjectToClipPos (v.vertex);  
            o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
            o.vertexColor = v.color * _Color;
                   
            return o;
        }
       
        fixed4 frag( v2f i ) : COLOR { 
            return tex2D (_MainTex, i.uv.xy) * i.vertexColor;
        }
   
    ENDCG
   
    SubShader {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent+100"}
        Cull [_Cull]
        Lighting Off
        ZWrite Off
        ZTest Always
        Fog { Mode Off }
        Blend SrcAlpha OneMinusSrcAlpha
       
    Pass {
   
        CGPROGRAM
       
        #pragma vertex vert
        #pragma fragment frag
        #pragma fragmentoption ARB_precision_hint_fastest
       
        ENDCG
         
        }
               
    }
    FallBack Off
}
