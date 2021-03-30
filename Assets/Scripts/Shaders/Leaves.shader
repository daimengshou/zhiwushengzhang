Shader "Custom/Leaves" {
	Properties {
	//细胞纹理，用于模拟虫洞
		[Toggle]_SimWormhole("Simulation Wormhole", Float) = 0
		_CelluarTex("Celluar Texture", 2D) = "white" {}
		_Threshold("Color Threshold", Color) = (0,0,0,1)

		[Toggle]_IsIllness("Simulation Illness", Float) = 0
		_IllnessColor("Illness Color", Color) = (1, 1, 1, 1)
		_inBlack("Input Black", Range(0,255)) = 0
		_inGamma("Input Gamma", Range(0,2)) = 1.61
		_inWhite("Input White", Range(0,255)) = 255

		_OutWhite("Out White", Range(0,255)) = 255
		_OutBlack("Out Black", Range(0,255)) = 0
	//叶片正面参数
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
		[HideInInspector]_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
		[HideInInspector]_BumpMap ("Normalmap", 2D) = "bump" {}
		[HideInInspector]_GlossMap ("Gloss (A)", 2D) = "black" {}
		[HideInInspector]_TranslucencyMap ("Translucency (A)", 2D) = "white" {}
		[HideInInspector]_ShadowOffset ("Shadow Offset (A)", 2D) = "black" {}
	//叶片背面参数
		[HideInInspector]_BackShininess ("Shininess", Range (0.01, 1)) = 0.078125
		[HideInInspector]_BackBumpMap ("Normalmap", 2D) = "bump" {}
		[HideInInspector]_BackGlossMap ("Gloss (A)", 2D) = "black" {}
		[HideInInspector]_BackTranslucencyMap ("Translucency (A)", 2D) = "white" {}
		[HideInInspector]_BackShadowOffset ("Shadow Offset (A)", 2D) = "black" {}

		// These are here only to provide default values
		_Cutoff ("Alpha cutoff", Range(0,1)) = 0.3
		[HideInInspector] _TreeInstanceColor ("TreeInstanceColor", Vector) = (1,1,1,1)
		[HideInInspector] _TreeInstanceScale ("TreeInstanceScale", Vector) = (1,1,1,1)
		[HideInInspector] _SquashAmount ("Squash", Float) = 1
	}

	SubShader { 
		Tags { "IgnoreProjector"="True" "RenderType"="TreeLeaf" }
		LOD 200
		Cull off
		//开始渲染正面
		
	CGPROGRAM
	#pragma surface surf TreeLeaf alphatest:_Cutoff vertex:TreeVertLeaf addshadow nolightmap noforwardadd
	#include "UnityBuiltin3xTreeLibrary.cginc"

	float _SimWormhole;
	sampler2D _CelluarTex;
	fixed4 _Threshold;

	float _IsIllness;
	fixed4 _IllnessColor;
	float _inBlack;
	float _inGamma;
	float _inWhite;
	float _OutWhite;
	float _OutBlack;

	sampler2D _MainTex;
	sampler2D _BumpMap;
	sampler2D _GlossMap;
	sampler2D _TranslucencyMap;
	half _Shininess;

	struct Input {
		float2 uv_MainTex;
		fixed4 color : COLOR; // color.a = AO
	};

	bool LessThanThreshold(fixed4 color)
	{
		if		(color.r < _Threshold.r)
			return true;
		else if (color.r > _Threshold.r)
			return false;
		else if (color.g < _Threshold.g)
			return true;
		else if (color.g > _Threshold.g)
			return false;
		else if (color.b < _Threshold.b)
			return true;
		else
			return false;
	}

	float GetPixelLevel(float pixelColor)
	{
		float pixelResult;
		pixelResult = max(0.0, pixelColor * 255.0 - _inBlack);
		pixelResult = saturate(pow(pixelResult / (_inWhite - _inBlack), _inGamma)); //除以(_inWhite - _inBlack)会比直接除以_inWhite更亮。
		pixelResult = (pixelResult * (_OutWhite - _OutBlack) + _OutBlack) / 255; //使用_OutWhite和_OutBlack修改像素值，以便可以从全局上控制最大像素值和最小像素值
		return pixelResult;
	}

	void surf (Input IN, inout LeafSurfaceOutput o) {
		fixed4 c = tex2D(_MainTex, IN.uv_MainTex);

		if (_SimWormhole)
		{
			fixed4 c_CelluarTex = tex2D(_CelluarTex, IN.uv_MainTex);
			if (c.a != 0 && c_CelluarTex.a != 0 &&
				LessThanThreshold(c_CelluarTex))
				c.a = 0;
		}

		if (_IsIllness)
		{
			fixed3 tempColor = c.rgb * _IllnessColor.rgb * _IllnessColor.a;
			fixed outR = GetPixelLevel(tempColor.r);
			fixed outG = GetPixelLevel(tempColor.g);
			fixed outB = GetPixelLevel(tempColor.b);

			o.Albedo = fixed3(outR, outG, outB);
		}
		else
		{
			o.Albedo = c.rgb * IN.color.rgb * _Color.a;
		}

		o.Translucency = tex2D(_TranslucencyMap, IN.uv_MainTex).rgb;
		o.Gloss = tex2D(_GlossMap, IN.uv_MainTex).a;
		o.Alpha = c.a;
		o.Specular = _Shininess;
		o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
	}
	ENDCG
	

	Cull front
	//开始渲染反面
	CGPROGRAM
	#pragma surface surf TreeLeaf alphatest:_Cutoff vertex:TreeVertLeaf addshadow nolightmap noforwardadd
	#include "UnityBuiltin3xTreeLibrary.cginc"

	float _SimWormhole;
	sampler2D _CelluarTex;
	fixed4 _Threshold;

	sampler2D _MainTex;
	sampler2D _BackBumpMap;
	sampler2D _BackGlossMap;
	sampler2D _BackTranslucencyMap;
	half _BackShininess;

	struct Input {
		float2 uv_BackMainTex;
		fixed4 BackColor : COLOR; // color.a = AO
	};

	bool LessThanThreshold(fixed4 color)
	{
		if		(color.r < _Threshold.r)
			return true;
		else if (color.r > _Threshold.r)
			return false;
		else if (color.g < _Threshold.g)
			return true;
		else if (color.g > _Threshold.g)
			return false;
		else if (color.b < _Threshold.b)
			return true;
		else
			return false;
	}

	void surf (Input IN, inout LeafSurfaceOutput o) {
		fixed4 c = tex2D(_MainTex, IN.uv_BackMainTex);

		if (_SimWormhole)
		{
			fixed4 c_CelluarTex = tex2D(_CelluarTex, IN.uv_BackMainTex);

			if (c.a != 0 && c_CelluarTex.a != 0 &&
				LessThanThreshold(c_CelluarTex))
				c.a = 0;
		}

		o.Albedo = c.rgb * IN.BackColor.rgb * IN.BackColor.a;
		o.Translucency = tex2D(_BackTranslucencyMap, IN.uv_BackMainTex).rgb;
		o.Gloss = tex2D(_BackGlossMap, IN.uv_BackMainTex).a;
		o.Alpha = c.a;
		o.Specular = _BackShininess;
		o.Normal = UnpackNormal(tex2D(_BackBumpMap, IN.uv_BackMainTex));
	}
	ENDCG
	}

	Dependency "OptimizedShader" = "Hidden/Nature/Tree Creator Leaves Optimized"
	FallBack "Diffuse"
}
