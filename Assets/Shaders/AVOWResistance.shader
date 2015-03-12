Shader "Custom/AVOWResistance" {
       Properties {
             _Color0 ("Color0", Color) = (1,1,1,1)
             _Color1 ("Color1", Color) = (1,1,1,1)
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
		        
		
		        float4 _Color0;
		        float4 _Color1;
		    
		
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
		        	float offset = 0.02;
		        	return (1/(x+offset)-1/(1+offset))/(1/offset-1/(1+offset));

		        }		        
		
		        float4 frag(v2f i) : COLOR
		        {
		        	float x = abs(i.uv[0] - i.uv[1]) * abs(1-i.uv[0] - i.uv[1]);
		        	float val = CalcCurve(x);
		        	
		        	float x0 = abs(i.uv[0] - i.uv[1]);
		        	float val0 = CalcCurve(x0);
		        	float x1 = abs(1-i.uv[0] - i.uv[1]);
		        	float val1 = CalcCurve(x1);
		        	return lerp(_Color0, _Color1, val0 + val1);
		        	/*
		        	//float alpha = 1- sqrt(i.uv[0] * i.uv[0] + i.uv[1] * i.uv[1]);
		        	float alpha  = 1;
		        	float4 col = _Color0;
		        	if (i.uv[0] > i.uv[1])
		        	{
		        		col = _Color1;
		        	}
		        	col[3] = alpha;
		        	return col;
		        	*/
		        }
		        ENDCG

            }
        }
        Fallback "VertexLit"
    }
