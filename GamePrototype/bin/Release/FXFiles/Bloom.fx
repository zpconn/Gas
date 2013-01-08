float4x4 worldViewProj;

float brightPassThreshold;
float blur;
float bloomScale;

texture brightPassTex;

texture bloomTex;

texture additiveBlendTex1;
texture additiveBlendTex2;

sampler brightPassSampler = sampler_state
{
	texture = (brightPassTex);
	AddressU = Clamp;
	AddressV = Clamp;
};

sampler bloomSampler = sampler_state
{
	texture = (bloomTex);
	AddressU = Clamp;
	AddressV = Clamp;
};

sampler additiveBlendTex1Sampler = sampler_state
{
	texture = (additiveBlendTex1);
	AddressU = Clamp;
	AddressV = Clamp;
};

sampler additiveBlendTex2Sampler = sampler_state
{
	texture = (additiveBlendTex2);
	AddressU = Clamp;
	AddressV = Clamp;
};

struct VSOut
{
	float4 pos : POSITION;
	float2 texCoord0 : TEXCOORD0;
};

void vertexShader(in float4 inPos : POSITION,
	              in float2 texCoord0 : TEXCOORD0,
	              out VSOut OUT)
{
	OUT.pos = mul(inPos, worldViewProj);
	OUT.texCoord0 = texCoord0;
}

float4 brightPassPS(float2 texCoord0 : TEXCOORD0) : COLOR0
{
	float4 color = tex2D(brightPassSampler, texCoord0);
	float luminance = 0.33f * (color.r + color.g + color.b);
	//float luminance = clamp(0, 1, color.r + color.g + color.b);
	
	if (luminance > brightPassThreshold)
	{
		luminance = luminance - brightPassThreshold;
		return luminance * color;
	}	
	else
		return float4(0, 0, 0, color.a);
}

float4 bloomXPS(float2 texCoord0 : TEXCOORD0) : COLOR0
{
	float define = blur * 100;
	
	float2 TexelKernel[13] =
	{
		{ -6, 0 },
		{ -5, 0 },
		{ -4, 0 },
		{ -3, 0 },
		{ -2, 0 },
		{ -1, 0 },
		{  0, 0 },
		{  1, 0 },
		{  2, 0 },
		{  3, 0 },
		{  4, 0 },
		{  5, 0 },
		{  6, 0 },
	};
	
	const float BlurWeights[13] =
	{
		0.002216,
		0.008764,
		0.026995,
		0.064759,
		0.120985,
		0.176033,
		0.199471,
		0.176033,
		0.120985,
		0.064759,
		0.026995,
		0.008764,
		0.002216,
	};
	
	float4 color = 0;
	for (int i = 0; i < 13; i++)
	{
		color += tex2D(bloomSampler, texCoord0 + TexelKernel[i].xy / define) * BlurWeights[i];
	}
	
	return color * bloomScale;
}

float4 bloomYPS(float2 texCoord0 : TEXCOORD0) : COLOR0
{
	float define = blur * 100;
	
	float2 TexelKernel[13] =
	{
		{ -6, 0 },
		{ -5, 0 },
		{ -4, 0 },
		{ -3, 0 },
		{ -2, 0 },
		{ -1, 0 },
		{  0, 0 },
		{  1, 0 },
		{  2, 0 },
		{  3, 0 },
		{  4, 0 },
		{  5, 0 },
		{  6, 0 },
	};
	
	const float BlurWeights[13] =
	{
		0.002216,
		0.008764,
		0.026995,
		0.064759,
		0.120985,
		0.176033,
		0.199471,
		0.176033,
		0.120985,
		0.064759,
		0.026995,
		0.008764,
		0.002216,
	};
	
	float4 color = 0;
	for (int i = 0; i < 13; i++)
	{
		color += tex2D(bloomSampler, texCoord0 + TexelKernel[i].yx / define) * BlurWeights[i];
	}
	
	return color * bloomScale;
}

float4 additiveBlendPS(float2 texCoord0 : TEXCOORD0) : COLOR0
{
	return tex2D(additiveBlendTex1Sampler, float2(texCoord0.x, texCoord0.y)) + tex2D(additiveBlendTex2Sampler, texCoord0);
}

technique DefaultTechnique
{
	pass P0 // Bright pass
	{
		AlphaBlendEnable = true;
		Clipping = true;
		CullMode = None;
		DestBlend = InvSrcAlpha;
		DitherEnable = false;
		FogEnable = false;
		Lighting = false;
		MinFilter [0] = Linear;
		MagFilter [0] = Linear;
		MinFilter [1] = Linear;
		MagFilter [1] = Linear;
		ShadeMode = Gouraud;
		SpecularEnable = false;
		SrcBlend = SrcAlpha;
		ZEnable = false;
		ZWriteEnable = false;
		FillMode = Solid;
		
		VertexShader = compile vs_1_1 vertexShader();
		PixelShader = compile ps_2_0 brightPassPS();
	}
	
	pass P1 // Horizontal Gaussian blur
	{
		AlphaBlendEnable = true;
		Clipping = true;
		CullMode = None;
		DestBlend = InvSrcAlpha;
		DitherEnable = false;
		FogEnable = false;
		Lighting = false;
		MinFilter [0] = Linear;
		MagFilter [0] = Linear;
		MinFilter [1] = Linear;
		MagFilter [1] = Linear;
		ShadeMode = Gouraud;
		SpecularEnable = false;
		SrcBlend = SrcAlpha;
		ZEnable = false;
		ZWriteEnable = false;
		FillMode = Solid;
		
		VertexShader = compile vs_1_1 vertexShader();
		PixelShader = compile ps_2_0 bloomXPS();
	}
	
	pass P2 // Vertical Gaussian blur
	{
		AlphaBlendEnable = true;
		Clipping = true;
		CullMode = None;
		DestBlend = InvSrcAlpha;
		DitherEnable = false;
		FogEnable = false;
		Lighting = false;
		MinFilter [0] = Linear;
		MagFilter [0] = Linear;
		MinFilter [1] = Linear;
		MagFilter [1] = Linear;
		ShadeMode = Gouraud;
		SpecularEnable = false;
		SrcBlend = SrcAlpha;
		ZEnable = false;
		ZWriteEnable = false;
		FillMode = Solid;
		
		VertexShader = compile vs_1_1 vertexShader();
		PixelShader = compile ps_2_0 bloomYPS();
	}
	
	pass P3 // Additive composition
	{
		AlphaBlendEnable = true;
		Clipping = true;
		CullMode = None;
		DestBlend = InvSrcAlpha;
		DitherEnable = false;
		FogEnable = false;
		Lighting = false;
		MinFilter [0] = Linear;
		MagFilter [0] = Linear;
		MinFilter [1] = Linear;
		MagFilter [1] = Linear;
		ShadeMode = Gouraud;
		SpecularEnable = false;
		SrcBlend = SrcAlpha;
		ZEnable = false;
		ZWriteEnable = false;
		FillMode = Solid;
		
		VertexShader = compile vs_1_1 vertexShader();
		PixelShader = compile ps_2_0 additiveBlendPS();
	}
}