Shader "Custom/AVOWLightening" {
       Properties {
       		_Alpha ("Alpha", Float) = 1
        }
        
        SubShader {
        		ZTest Off
            	Blend SrcAlpha One // additive blending
            	Cull Off
			Tags {"Queue"="Transparent"}
			
            Pass {
			    CGPROGRAM
		
		        #pragma vertex vert
		        #pragma fragment frag

		        #include "UnityCG.cginc"
		        
		
		        float4 _Color0;
		        float4 _Color1;
		        
		        uniform float _Alpha;
		    
		
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
		        	float xx = i.uv[0] - 0.5;
		        	float yy = i.uv[1] - 0.5;
		        	
		        	if (yy <= 0){
			        	float x = abs(xx);
			        	
			        	float4 col = float4(CalcCurve(x, 0.01), CalcCurve(x, 0.01), CalcCurve(x, 0.2), _Alpha * CalcCurve(x, 0.1));
			        	return col;
			        }
			        else{
			        	float x = sqrt(xx * xx + yy * yy);
			        	
			        	float4 col = float4(CalcCurve(x, 0.01), CalcCurve(x, 0.01), CalcCurve(x, 0.2), _Alpha * CalcCurve(x, 0.1));
			        	return col;//float4(1, 0, 0, 1);
			        }
		        	
		        
//		        	return lerp(_Color0, _Color1, val);

		        }
		        ENDCG

            }
        }
        Fallback "VertexLit"
    }
