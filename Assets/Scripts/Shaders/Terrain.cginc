#define USE_HASH

float _BlendRatio;

fixed4 hash4(fixed2 p)
{
	return frac(sin(fixed4(1.0 + dot(p,fixed2(37.0, 17.0)),
						  2.0 + dot(p,fixed2(11.0, 47.0)),
						  3.0 + dot(p,fixed2(41.0, 29.0)),
						  4.0 + dot(p,fixed2(23.0, 31.0))))*103.0);
}

fixed4 texNoTileTech1(sampler2D tex, float2 uv) {
	float2 iuv = floor(uv);
	float2 fuv = frac(uv);

	// Generate per-tile transformation
	#if defined (USE_HASH)
		float4 ofa = hash4(iuv + float2(0, 0));
		float4 ofb = hash4(iuv + float2(1, 0));
		float4 ofc = hash4(iuv + float2(0, 1));
		float4 ofd = hash4(iuv + float2(1, 1));
	#else
		float4 ofa = tex2D(_NoiseTex, (iuv + float2(0.5, 0.5))/256.0);
		float4 ofb = tex2D(_NoiseTex, (iuv + float2(1.5, 0.5))/256.0);
		float4 ofc = tex2D(_NoiseTex, (iuv + float2(0.5, 1.5))/256.0);
		float4 ofd = tex2D(_NoiseTex, (iuv + float2(1.5, 1.5))/256.0);
	#endif

	// Compute the correct derivatives
	float2 dx = ddx(uv);
	float2 dy = ddy(uv);

	// Mirror per-tile uvs
	ofa.zw = sign(ofa.zw - 0.5);
	ofb.zw = sign(ofb.zw - 0.5);
	ofc.zw = sign(ofc.zw - 0.5);
	ofd.zw = sign(ofd.zw - 0.5);

	float2 uva = uv * ofa.zw + ofa.xy, dxa = dx * ofa.zw, dya = dy * ofa.zw;
	float2 uvb = uv * ofb.zw + ofb.xy, dxb = dx * ofb.zw, dyb = dy * ofb.zw;
	float2 uvc = uv * ofc.zw + ofc.xy, dxc = dx * ofc.zw, dyc = dy * ofc.zw;
	float2 uvd = uv * ofd.zw + ofd.xy, dxd = dx * ofd.zw, dyd = dy * ofd.zw;

	// Fetch and blend
	float2 b = smoothstep(_BlendRatio, 1.0 - _BlendRatio, fuv);

	return lerp(	lerp(tex2D(tex, uva, dxa, dya), tex2D(tex, uvb, dxb, dyb), b.x),
					lerp(tex2D(tex, uvc, dxc, dyc), tex2D(tex, uvd, dxd, dyd), b.x), b.y);
}

fixed4 texNoTileTech2(sampler2D tex, float2 uv) {
	float2 iuv = floor(uv);
	float2 fuv = frac(uv);

	// Compute the correct derivatives for mipmapping
	float2 dx = ddx(uv);
	float2 dy = ddy(uv);

	// Voronoi contribution
	float4 va = 0.0;
	float wt = 0.0;
	float blur = -(_BlendRatio + 0.5) * 30.0;
	for (int j = -1; j <= 1; j++) {
		for (int i = -1; i <= 1; i++) {
			float2 g = float2((float)i, (float)j);
			#if defined (USE_HASH)
				float4 o = hash4(iuv + g);
			#else
				float4 o = tex2D(_NoiseTex, (iuv + g + float2(0.5, 0.5))/256.0);
			#endif
		    // Compute the blending weight proportional to a gaussian fallof
			float2 r = g - fuv + o.xy;
			float d = dot(r, r);
			float w = exp(blur * d);
			float4 c = tex2D(tex, uv + o.zw, dx, dy);
			va += w * c;
			wt += w;
		}
	}

	// Normalization
	return va/wt;
}

fixed4 texNoTileTech3(sampler2D tex, float2 uv){
	return tex2D(tex, uv);
}