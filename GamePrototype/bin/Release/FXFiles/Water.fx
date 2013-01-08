float4x4 world : WORLD;
float4x4 worldViewProj : WORLDVIEWPROJECTION;
float4 lightPos;
float4 lightColor;
float4 texCoordScroll;
float range;
float timer;

texture water;
texture sand;

sampler waterSampler = sampler_state
{
	texture = (water);
};

sampler sandSampler = sampler_state
{
	texture = (sand);
};

struct transformVSOutput
{
	float4 pos : POSITION;
	float4 color : COLOR0;
	float2 texCoord0 : TEXCOORD0;
	float2 worldPos : TEXCOORD1;
};

transformVSOutput transformVS(float4 inPos : POSITION,
                              float4 inColor : COLOR0,
                              float2 inTexCoord0 : TEXCOORD0)
{
	transformVSOutput o;
	
	o.pos = mul(inPos, worldViewProj);
	o.worldPos = mul(inPos, world);
	
	o.color = inColor;
	o.texCoord0 = inTexCoord0;
	
	return o;
}

float4 waterPS(float2 texCoord0 : TEXCOORD0,
               float2 worldPos : TEXCOORD1) : COLOR0
{
	float2 waveTexCoords = texCoord0 + texCoordScroll;
	waveTexCoords.x = waveTexCoords.x + 0.008f * sin(waveTexCoords.y * 34.0f + timer);
	
	//float2 deltaLightPos = float2(lightPos.xy) - float2(worldPos.xy);
	//float3 lightDir = float3(deltaLightPos.xy, 250.0f);
	//float lenSq = dot(lightDir, lightDir);
	
	//float attn = min((range * range) / lenSq, 1.0f);
	
	float4 color = tex2D(sandSampler, texCoord0 + texCoordScroll) 
		+ tex2D(waterSampler, waveTexCoords);
	//float4 light = attn * lightColor;
	//float4 fixedA = float4(color.rgb, 1.0f);
	//fixedA = fixedA * light;
	
	//return float4(fixedA.rgb, color.a);
	
	return color;
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
		
		VertexShader = compile vs_1_1 transformVS();
		PixelShader = compile ps_2_0 waterPS();
	}
}