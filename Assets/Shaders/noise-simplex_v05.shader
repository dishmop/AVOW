Shader "Custom noise/Noise-Simplex05" {
Properties {
	_Freq ("Frequency", Float) = 20
	_StaticSpeed ("StaticSpeed", Float) = 0.3
	_Color0 ("Color0", Color) = (1.0, 1.0, 1.0)
	_Color1 ("Color1", Color) = (1.0, 1.0, 1.0)
	_MidPoint ("MidPoint", Float) = 0

}

SubShader {
//       ZTest Greater
       Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
	Pass {
		Tags { "RenderType" = "Transparent" }
		CGPROGRAM
		
		#pragma target 3.0
		
		#pragma vertex vert
		#pragma fragment frag
		
		#include "noiseSimplex.cginc"
		 #include "UnityCG.cginc"
		
		struct v2f {
			float4 pos : SV_POSITION;
			float4 uv : TEXCOORD0;
//			float3 srcPos1 : TEXCOORD1;
		};
		
		uniform float
			_Freq,
			_StaticSpeed
			
//			_DetailScale
			;
				

			
		uniform float4 _Color0;
		uniform float4 _Color1;
		uniform float _MidPoint;
		float _TextureScale;
		

		
		v2f vert(appdata_base v)
		{
			v2f o;

			o.pos =	mul(UNITY_MATRIX_MVP, v.vertex);
			
			//o.uv = v.texcoord;
		    float4 worldPos = mul ((float4x4)_Object2World, v.vertex );
		    o.uv = worldPos;// / _TextureScale;  //setting and scaling UVs 		
		    
		  
			
			return o;
		}
		

		
		float4 frag(v2f i) : COLOR
		{
	

			

			float staticSpeedParam = _StaticSpeed;
	
		
			
			// Make them all animate in the same way
			//noisePos.y += _Time.y * _Speed;
			float4 noisePos =  float4(i.uv.x, i.uv.y, i.uv.z, 19 + _Time.y * staticSpeedParam);
			
			noisePos *= _Freq;
			
			
			float nsThis = snoise(noisePos) / 2 + 0.5f;

			
			float4 col =  lerp(_Color0, _Color1, nsThis);
			float alpha = 1-(abs(i.uv.y - _MidPoint) / 150);
			col.a = alpha * col.a;
			return col;
		}
		
		ENDCG
	}
}

}