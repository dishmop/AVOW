Shader "Custom/AVOWCursorCubeGreen" {
	Properties {
	}
	SubShader {
		ZTest Always
	    	Blend SrcAlpha One // additive blending
			Tags {"Queue"="Transparent"}
			
	    Pass {
			    CGPROGRAM
		
		        #pragma vertex vert
		        #pragma fragment frag
	
		        #include "UnityCG.cginc"
		        
		
		        float4 _Color0;
		        float4 _Color1;
		        		   
		        		        
		        uniform float _Greenness;
		    
		
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
		        
		       	float4 CalcGreenCol(float x){		       		
	       			return float4(CalcCurve(x, 0.01), CalcCurve(x, 0.2), CalcCurve(x, 0.01), CalcCurve(x, 0.1));
		       	}
		       	
		   
		        float4 frag(v2f i) : COLOR
		        {
		        	float left = abs(i.uv[0]);
		        	float right = abs(1 - i.uv[0]);
		        	float top = abs(i.uv[1]);
		        	float bottom = abs(1 - i.uv[1]);
		        			        	
		        	float4 colLeft = 	CalcGreenCol(left);
		        	float4 colRight = 	CalcGreenCol(right);
		        	float4 colTop =		CalcGreenCol(top);
		        	float4 colBottom = 	CalcGreenCol(bottom);
		        	
		        	return colLeft + colRight +  colTop + colBottom;
		        	
		        
	//		        	return lerp(_Color0, _Color1, val);
	
		        }
		        ENDCG
	
	    }
	}
	Fallback "VertexLit"
}
