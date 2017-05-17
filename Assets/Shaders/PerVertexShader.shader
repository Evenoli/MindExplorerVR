Shader "Unlit/PerVertexShader"
{
	Properties
	{
		_Color("Main Color", Color) = (1,1,1,1)
	}

		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100
		Cull Off
		Lighting Off
		ZWrite On

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			
			struct appdata {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				fixed4 color : COLOR;
			};

			v2f vert(appdata v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{ 
				return i.color; 
			}
			ENDCG
		}
	}
}
