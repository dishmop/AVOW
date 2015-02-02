Shader "Custom/AVOWNodeGlow" {
       Properties {
             _Intensity ("Intensity", Range(0, 1)) = 0
        }
        SubShader {
        	//ZTest Always
            	Blend SrcAlpha One // additive blending
			Tags {"Queue"="Transparent"}
			
            Pass {
			    CGPROGRAM
		
		        #pragma vertex vert
		        #pragma fragment frag

		        #include "UnityCG.cginc"
		        
		
		        uniform float _Intensity;
		    
		
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
		        
		        
		        float CalcCurve(float x, float offset)
		        {
		        	//float offset = 0.5;
		        	return (1/(x+offset)-1/(1+offset))/(1/offset-1/(1+offset));

		        }		        

		        float4 frag(v2f i) : COLOR
		        {
		        	float xVal = i.uv[0] - 0.5;
		        	float xMul = cos(3.14159 * 3.5 * xVal* xVal * xVal);
		        	float y =  abs(i.uv[1] - 0.5);
		        	float4 col0 = float4(xMul *  CalcCurve(y, 0.01), xMul * CalcCurve(y, 0.02),  xMul * CalcCurve(y, 0.2),  xMul * CalcCurve(y, 0.02));
		        	float4 col1 = float4(xMul *  CalcCurve(y, 0.01), xMul * CalcCurve(y, 0.02),  xMul * CalcCurve(y, 0.2),  xMul * CalcCurve(y, 0.2));


		        	return   lerp(col0, col1, _Intensity);
		        	
		        
//		        	return lerp(_Color0, _Color1, val);

		        }
		        ENDCG

            }
        }
        Fallback "VertexLit"
    }
