Shader "Unlit/TerrainLines"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }


            fixed Remap(float In, float2 InMinMax, float2 OutMinMax)
            {
                return OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }


            float sample_remap(sampler2D tex, float2 uv)
            {
            	float col = tex2D(tex, uv).a;
	            return Remap(col, float2(0.43f, 0.68f), float2(0.0f, 1.0f));
            }

            float sample_remap_round(sampler2D tex, float2 uv, float r)
            {
            	float col = tex2D(tex, uv).a;
	            col = Remap(col, float2(0.43f, 0.68f), float2(0.0f, 1.0f));
            	return round(col * r) / r;
            }
            
			float sobel (sampler2D tex, float2 uv, float d) {
				float2 delta = float2(d, d);
				
				float4 hr = float4(0, 0, 0, 0);
				float4 vt = float4(0, 0, 0, 0);
				
				hr += sample_remap(tex, (uv + float2(-1.0, -1.0) * delta)) *  1.0;
				hr += sample_remap(tex, (uv + float2( 0.0, -1.0) * delta)) *  0.0;
				hr += sample_remap(tex, (uv + float2( 1.0, -1.0) * delta)) * -1.0;
				hr += sample_remap(tex, (uv + float2(-1.0,  0.0) * delta)) *  2.0;
				hr += sample_remap(tex, (uv + float2( 0.0,  0.0) * delta)) *  0.0;
				hr += sample_remap(tex, (uv + float2( 1.0,  0.0) * delta)) * -2.0;
				hr += sample_remap(tex, (uv + float2(-1.0,  1.0) * delta)) *  1.0;
				hr += sample_remap(tex, (uv + float2( 0.0,  1.0) * delta)) *  0.0;
				hr += sample_remap(tex, (uv + float2( 1.0,  1.0) * delta)) * -1.0;
				
				vt += sample_remap(tex, (uv + float2(-1.0, -1.0) * delta)) *  1.0;
				vt += sample_remap(tex, (uv + float2( 0.0, -1.0) * delta)) *  2.0;
				vt += sample_remap(tex, (uv + float2( 1.0, -1.0) * delta)) *  1.0;
				vt += sample_remap(tex, (uv + float2(-1.0,  0.0) * delta)) *  0.0;
				vt += sample_remap(tex, (uv + float2( 0.0,  0.0) * delta)) *  0.0;
				vt += sample_remap(tex, (uv + float2( 1.0,  0.0) * delta)) *  0.0;
				vt += sample_remap(tex, (uv + float2(-1.0,  1.0) * delta)) * -1.0;
				vt += sample_remap(tex, (uv + float2( 0.0,  1.0) * delta)) * -2.0;
				vt += sample_remap(tex, (uv + float2( 1.0,  1.0) * delta)) * -1.0;
				
				return sqrt(hr * hr + vt * vt);
			}

            float sobel_round (sampler2D tex, float2 uv, float d, float r) {
				float2 delta = float2(d, d);
				
				float4 hr = float4(0, 0, 0, 0);
				float4 vt = float4(0, 0, 0, 0);
				
				hr += sample_remap_round(tex, (uv + float2(-1.0, -1.0) * delta), r) *  1.0;
				hr += sample_remap_round(tex, (uv + float2( 0.0, -1.0) * delta), r) *  0.0;
				hr += sample_remap_round(tex, (uv + float2( 1.0, -1.0) * delta), r) * -1.0;
				hr += sample_remap_round(tex, (uv + float2(-1.0,  0.0) * delta), r) *  2.0;
				hr += sample_remap_round(tex, (uv + float2( 0.0,  0.0) * delta), r) *  0.0;
				hr += sample_remap_round(tex, (uv + float2( 1.0,  0.0) * delta), r) * -2.0;
				hr += sample_remap_round(tex, (uv + float2(-1.0,  1.0) * delta), r) *  1.0;
				hr += sample_remap_round(tex, (uv + float2( 0.0,  1.0) * delta), r) *  0.0;
				hr += sample_remap_round(tex, (uv + float2( 1.0,  1.0) * delta), r) * -1.0;
				
				vt += sample_remap_round(tex, (uv + float2(-1.0, -1.0) * delta), r) *  1.0;
				vt += sample_remap_round(tex, (uv + float2( 0.0, -1.0) * delta), r) *  2.0;
				vt += sample_remap_round(tex, (uv + float2( 1.0, -1.0) * delta), r) *  1.0;
				vt += sample_remap_round(tex, (uv + float2(-1.0,  0.0) * delta), r) *  0.0;
				vt += sample_remap_round(tex, (uv + float2( 0.0,  0.0) * delta), r) *  0.0;
				vt += sample_remap_round(tex, (uv + float2( 1.0,  0.0) * delta), r) *  0.0;
				vt += sample_remap_round(tex, (uv + float2(-1.0,  1.0) * delta), r) * -1.0;
				vt += sample_remap_round(tex, (uv + float2( 0.0,  1.0) * delta), r) * -2.0;
				vt += sample_remap_round(tex, (uv + float2( 1.0,  1.0) * delta), r) * -1.0;
				
				return sqrt(hr * hr + vt * vt);
			}
            
            fixed4 frag (v2f i) : SV_Target
            {
                float col = sample_remap(_MainTex, i.uv);
            	float wide_line =  sobel_round(_MainTex, i.uv, 0.0005f, 10);

                // col = Remap(col, float2(0.43f, 0.68f), float2(0.0f, 1.0f));

            	col = step(0.01f, sobel(_MainTex, i.uv, 0.00022f)) * col; 
                
                return col + wide_line;
            }
            ENDCG
        }
    }
}
