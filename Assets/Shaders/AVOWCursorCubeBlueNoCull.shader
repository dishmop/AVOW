Shader "Custom/AVOWCursorCubeBlueNoCull" {
	Properties {
		 _Intensity ("Intensity", Float) = 1

	}
	SubShader {
			Cull Off
			ZTest Always
	    	Blend SrcAlpha One // additive blending
			Tags {"Queue"="Transparent"}
			
	    Pass {
			    CGPROGRAM
		
		        #pragma vertex vert
		        #pragma fragment frag
	
		        #include "UnityCG.cginc"
		        
		
		        float  _Intensity;
		        		   

		    
		
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
		        	return 1*(1/(x+offset)-1/(1+offset))/(1/offset-1/(1+offset));
	
		        }	
		        
		       	float4 CalcBlueCol(float x){		       		
	       			return float4(CalcCurve(x, 0.01), CalcCurve(x, 0.01), CalcCurve(x, 0.2), CalcCurve(x, 0.1));
		       	}
		       	
		   
		        float4 frag(v2f i) : COLOR
		        {
		        	float left = abs(i.uv[0]);
		        	float right = abs(1 - i.uv[0]);
		        	float top = abs(i.uv[1]);
		        	float bottom = abs(1 - i.uv[1]);
		        			        	
		        	float4 colLeft = 	CalcBlueCol(left);
		        	float4 colRight = 	CalcBlueCol(right);
		        	float4 colTop =		CalcBlueCol(top);
		        	float4 colBottom = 	CalcBlueCol(bottom);
		        	
		        	return _Intensity * (colLeft + colRight +  colTop + colBottom);
		        	
		        
	//		        	return lerp(_Color0, _Color1, val);
	
		        }
		        ENDCG
	
	    }
	}
	Fallback "VertexLit"
}
