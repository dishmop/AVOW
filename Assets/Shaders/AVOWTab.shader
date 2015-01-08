Shader "Custom/AVOWTab" {
       Properties {
             _Color ("Color", Color) = (1,1,1,1)

        }
        SubShader {
        	ZTest Always
			Blend SrcAlpha OneMinusSrcAlpha
			Tags {"Queue"="Transparent"}
			
            Pass {
			    CGPROGRAM
		
		        #pragma vertex vert
		        #pragma fragment frag

		        #include "UnityCG.cginc"
		        
		
		        float4 _Color;
		    
		
		        struct v2f {
		            float4 pos : SV_POSITION;
		            float2 uv : TEXCOORD0;
		        };
		
		
		        v2f vert (appdata_base v)
		        {
		            v2f o;
		            o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
		            o.uv = v.texcoord;
		            return o;
		        }
		        
		        float CalcCurve(float x)
		        {
		        	float offset = 1;
		        	return (1/(x+offset)-1/(1+offset))/(1/offset-1/(1+offset));

		        }
		
		        float4 frag(v2f i) : COLOR
		        {
		        	float alpha = CalcCurve(i.uv[1]);
		        	//float alpha = 1-i.uv[1];
		        	float4 col = _Color;
		        	col[3] = alpha;
		        	return col;
		        }
		        ENDCG

            }
        }
        Fallback "VertexLit"
    }
