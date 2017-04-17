﻿Shader "Unlit/Transparent HSB"
{
    Properties
    {
        _MainTex ("Base (RGB), Alpha (A)", 2D) = "black" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
         _OtherColor ("Other Color", Color) = (1,1,1,1)
    }
 
    SubShader
    {
        LOD 100
 
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }
     
        Cull Off
        Lighting Off
        ZWrite Off
        Fog { Mode Off }
        Offset -1, -1
        Blend SrcAlpha OneMinusSrcAlpha
 
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
             
            #include "UnityCG.cginc"
            #include "HSB.cginc"
 float4 _BaseColor;
  float4 _OtherColor;
            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                fixed4 color : COLOR;
            };
 
            struct v2f
            {
                float4 vertex : SV_POSITION;
                half2 texcoord : TEXCOORD0;
                fixed4 color : COLOR;
            };
 
            sampler2D _MainTex;
         
            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color;
                return o;
            }
             
            fixed4 frag (v2f i) : COLOR
            {
                float4 startColor = tex2D(_MainTex, i.texcoord);
                float4 hsbColor = applyHSBEffect(startColor, _BaseColor);
                return hsbColor ;// _BaseColor;
            }
            ENDCG
        }
    }
 
    SubShader
    {
        LOD 100
 
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }
     
        Pass
        {
            Cull Off
            Lighting Off
            ZWrite Off
            Fog { Mode Off }
            Offset -1, -1
            ColorMask RGB
            //AlphaTest Greater .01
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMaterial AmbientAndDiffuse
         
            SetTexture [_MainTex]
            {
                Combine Texture * Primary
            }
        }
    }
}