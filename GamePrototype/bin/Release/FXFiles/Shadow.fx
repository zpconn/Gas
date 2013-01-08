float4x4 worldViewProj;
float4x4 world;
float4x4 viewProj;

float blur;

int numActiveLights;
float4 lightPos[4];
float4 lightColor[4];
float range[4];

texture shadowTex;

sampler shadowTexSampler = sampler_state
{
	texture = (shadowTex);
	AddressU = Clamp;
	AddressV = Clamp;
};

struct VSOut
{
	float4 pos : POSITION;
	float2 texCoord0 : TEXCOORD0;
	float2 worldPos : TEXCOORD1;
	float4 color : COLOR0;
};

void vertexShader(in float4 inPos : POSITION,
	              in float2 texCoord0 : TEXCOORD0,
	              in float4 color : COLOR0,
	              out VSOut OUT)
{
	OUT.pos = mul(inPos, worldViewProj);
	OUT.worldPos = mul(inPos, world);
	OUT.color = color;
	OUT.texCoord0 = texCoord0;
}

float4 shadowPS(float4 pos : POSITION) : COLOR0
{
	return float4(0, 0, 0, 1);
}

float4 blurXPS(float2 texCoord0 : TEXCOORD0) : COLOR0
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
		color += tex2D(shadowTexSampler, texCoord0 + TexelKernel[i].xy / define) * BlurWeights[i];
	}
	
	return color;
}

float4 blurYPS(float2 texCoord0 : TEXCOORD0) : COLOR0
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
		color += tex2D(shadowTexSampler, texCoord0 + TexelKernel[i].yx / define) * BlurWeights[i];
	}
	
	return color;
}

float4 lightingPS(float2 texCoord0 : TEXCOORD0,
                  float2 worldPos : TEXCOORD1) : COLOR0
{
	float4 tmpColor = float4(0,0,0,0);
    
    for (int i = 0; i < numActiveLights; i++)
    {
    	float2 lightPosScreen = mul(lightPos[i], viewProj);
    	float2 deltaLightPos = float2(lightPosScreen.xy) - float2(worldPos.xy);
		float3 lightDir = float3(deltaLightPos.xy, 250.0f);
		float lenSq = dot(lightDir, lightDir);
		
		float attn = min((range[i] * range[i]) / lenSq, 1.0f);
		
		tmpColor.xyz += lightColor[i] * attn;
    }
    
    float brightness = saturate(tmpColor.r + tmpColor.g + tmpColor.b);
    //float brightness = 0.3f;

	float4 color = tex2D(shadowTexSampler, texCoord0);
	
	if (color.a == 0)
	    color = float4(0, 0, 0, 0);
	else
	    color.a = saturate(color.a - brightness);
		
	return color;
}

float4 finalizePS(float2 texCoord0 : TEXCOORD0) : COLOR0
{
	return tex2D(shadowTexSampler, texCoord0);
}

technique DefaultTechnique
{
	pass P0
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
		ZEnable = true;
		ZWriteEnable = true;
		FillMode = Solid;
		
		VertexShader = compile vs_1_1 vertexShader();
		PixelShader = compile ps_1_1 shadowPS();
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
		PixelShader = compile ps_2_0 blurXPS();
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
		PixelShader = compile ps_2_0 blurYPS();
	}
	
	pass P3 // Lighting
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
		PixelShader = compile ps_2_0 lightingPS();
	}
	
	pass P4 // Finalize
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
		ZEnable = true;
		ZWriteEnable = true;
		FillMode = Solid;
		
		VertexShader = compile vs_1_1 vertexShader();
		PixelShader = compile ps_2_0 finalizePS();
	}
}