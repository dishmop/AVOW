Shader "Custom/AVOWGlow" {
	Properties {
		 _Intensity ("Intensity", Float) = 1
		 _Texture ("Texture", 2D) = "defaulttexture" {}

	}
	SubShader {
			//Cull Off
			ZTest Always
	    	Blend SrcAlpha One // additive blending
			Tags {"Queue"="Transparent"}
			
	    Pass {
			    CGPROGRAM
		
		        #pragma vertex vert
		        #pragma fragment frag
	
		        #include "UnityCG.cginc"
		        
		
		        uniform float  _Intensity;	
		       	uniform sampler2D _Texture;     		   

		    
		
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
		        

		        float4 frag(v2f i) : COLOR
		        {
		        	return _Intensity * tex2D(_Texture, i.uv);
		        }
		        ENDCG
	
	    }
	}
	Fallback "VertexLit"
}
