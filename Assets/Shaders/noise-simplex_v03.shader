Shader "Custom noise/Noise-Simplex03" {
Properties {
	_Freq ("Frequency", Float) = 20
	_StaticSpeed ("StaticSpeed", Float) = 0.3
	_Color0 ("Color0", Color) = (1.0, 1.0, 1.0)
	_Color1 ("Color1", Color) = (1.0, 1.0, 1.0)


}

SubShader {
//       ZTest Greater
//       Cull Front
		Blend SrcAlpha OneMinusSrcAlpha
	Pass {
	//	Tags { "RenderType" = "Transparent" }
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
		
		float2 CalcCentreXY(float2 xy, float2 corner, float2 up){
			const  float pi = 3.14159;
	
		 	float2 ret;
		 	float2 pos;
		 	float2 relPos = xy - corner;
		 	float relPosLen = sqrt(relPos.x * relPos.x + relPos.y * relPos.y);
			float2 relPosNorm = relPos * 1.0 / relPosLen;
		 	float angle = acos(relPosNorm.x * up.x + relPosNorm.y * up.y);
			// scale the angle
			angle = 0.1 * angle / pi;	
			ret.x = relPosLen;
			ret.y = angle;	 
			return ret;
		}
		
		float4 frag(v2f i) : COLOR
		{
	
			// Work out the main noise value
			float4 noisePos;

			float2 i2uv = i.uv.xy;
			

			float staticSpeedParam = _StaticSpeed;
	
		
			
			// Make them all animate in the same way
			//noisePos.y += _Time.y * _Speed;
			noisePos.x =  i2uv.x;
			noisePos.y =  i2uv.y;
			noisePos.z =  19 + _Time.y * staticSpeedParam;
			
			noisePos *= _Freq;
			
			
			float nsThis = snoise(noisePos) / 2 + 0.5f;

			
			return lerp(_Color0, _Color1, nsThis);
		}
		
		ENDCG
	}
}

}