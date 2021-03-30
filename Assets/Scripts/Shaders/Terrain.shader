
Shader "Custom/Terrain" {
    Properties {
      _MainTex ("Texture", 2D) = "white" {}
	  _Glossiness("Smoothness", Range(0, 1)) = 0.5
	  _Metallic("Metallic", Range(0, 1)) = 0.0
	  _BlendRatio("Blend Ratio", Range(0, 1)) = 0.5
    }

    SubShader {
      Tags { "RenderType" = "Opaque" }

      CGPROGRAM
      #pragma surface surf Standard fullforwardshadows

	  #include "Terrain.cginc"

      struct Input {
          float2 uv_MainTex : TEXCOORD0;
		  UNITY_FOG_COORDS(1)
      };

      sampler2D _MainTex;
	  half _Glossiness;
	  half _Metallic;

      void surf (Input IN, inout SurfaceOutputStandard o) {
		  fixed4 c = texNoTileTech1(_MainTex, IN.uv_MainTex);
		  //fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
		  UNITY_APPLY_FOG_COLOR(IN.fogCoord, c, fixed4(0, 0, 0, 0));

          o.Albedo = c.rgb;
		  o.Metallic = _Metallic;
		  o.Smoothness = _Glossiness;
		  o.Alpha = c.a;
      }
      ENDCG

    } 
    Fallback "Diffuse"
}