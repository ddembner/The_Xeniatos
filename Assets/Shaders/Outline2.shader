﻿Shader "Unlit/Outline2"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", color) = (1, 1, 1, 1)
		_OutlineColor("Outline Color", color) = (1, 1, 1, 1)
		[PerRendererData]_OutlineWidth("Outline Width", Range(1, 5)) = 1
	}
	
    SubShader
    {
		Tags { "RenderType" = "Transparent" }
		LOD 100
        pass
        {
			zwrite off
		
            CGPROGRAM
            #pragma vertex vert
			#pragma fragment frag

            #include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 pos : POSITION;
				float3 normal : NORMAL;
			};

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _OutlineColor;
			float _OutlineWidth;

            v2f vert (appdata v)
            {
				v.vertex.xyz *= _OutlineWidth;

				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o; 
            }

			fixed4 frag(v2f i) : SV_Target
			{
				return _OutlineColor;
			}
            ENDCG
        }

		pass //Object
		{
			zwrite on
			Lighting on
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
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float4 col = tex2D(_MainTex, i.uv) * _Color;
				return col;
			}
			ENDCG
		}
    }

	Fallback "Diffuse"
}
