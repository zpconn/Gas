float4x4 world : WORLD;
float4x4 worldRot;
float4x4 worldViewProj : WORLDVIEWPROJECTION;
float4 lightPos;
float4 eyePos;
float range;
float shininess;
float parallaxAmount;
float ambient;
float4 scrollVec;
float heightMapScale;

texture diffuseMap;
texture normalMap;

sampler diffuseSampler = sampler_state
{
	texture = (diffuseMap);
};

sampler normalSampler = sampler_state
{
	texture = (normalMap);
};

// The vertex shader output structure
struct transformVSOutput
{
	float4 pos : POSITION;
	float4 color : COLOR0;
	float2 texCoord0 : TEXCOORD0;
	float2 worldPos : TEXCOORD1;
};

// This shader computes standard vertex transforms
transformVSOutput transformVS(float4 inPos : POSITION,
                              float4 inColor : COLOR0,
                              float2 inTexCoord0 : TEXCOORD0)
{
	transformVSOutput o;
	
	// Transform the vertex from model space into homogeneous projection space
	o.pos = mul(inPos, worldViewProj);
	
	// Compute the world position of this vertex
	o.worldPos = mul(inPos, world);
	
	// Propagate the color and texture coordinates through
	o.color = inColor;
	o.texCoord0 = inTexCoord0;
	
	return o;
}

float4 textureMappingPS(float2 texCoord0 : TEXCOORD0,
                        float2 worldPos : TEXCOORD1) : COLOR0
{
	float2 deltaLightPos = float2(lightPos.xy) - float2(worldPos.xy);
	float3 lightDir = float3(deltaLightPos.xy, 250.0f);
	float lenSq = dot(lightDir, lightDir);
	
	float attn = min((range * range) / lenSq, 1.0f);

	float4 color = tex2D(diffuseSampler, texCoord0);
	float4 fixedAlphaColor = float4(color.rgb, 1.0f);
	fixedAlphaColor = (ambient + attn) * fixedAlphaColor;
	
	return float4(fixedAlphaColor.rgb, color.a);
}

float4 vertexColoringLightingPS(float2 texCoord0 : TEXCOORD0,
                 float2 worldPos : TEXCOORD1,
                 float4 color : COLOR0) : COLOR0
{
	float2 deltaLightPos = float2(lightPos.xy) - float2(worldPos.xy);
	float3 lightDir = float3(deltaLightPos.xy, 250.0f);
	float lenSq = dot(lightDir, lightDir);
	
	float attn = min((range * range) / lenSq, 1.0f);
	
	float4 fixedAlphaColor = float4(color.rgb, 1.0f);
	fixedAlphaColor = (ambient + attn) * fixedAlphaColor;
	
	return float4(fixedAlphaColor.rgb, color.a);
}

// This shader computes N dot L lighting with parallax mapping
float4 lightingPS(float2 texCoord0 : TEXCOORD0,
                  float2 worldPos : TEXCOORD1) : COLOR0
{
	float heightBias = 0.01f;

	// Compute the light direction
	float2 deltaLightPos = float2(lightPos.xy) - float2(worldPos.xy);
	float3 lightDir = float3(deltaLightPos.xy, 250.0f);
	float lenSq = dot(lightDir, lightDir);
	lightDir = normalize(lightDir);
    
    // Calculate the eye direction in world space
    float3 eyeDir = normalize(eyePos.xyz - float3(worldPos.xy, 1.0f));
 
 	// Apply the texture coordinate transformation
 	float2 texSample = texCoord0 + scrollVec.xy;

	// Sample the texture maps
	float4 diffuseColor = tex2D(diffuseSampler, texSample);
	float4 normalColor = tex2D(normalSampler, texSample);
	
	// Uncompress the per-pixel normal ([0,1] -> [-1,1])
	float3 normal = float3(normalColor.xyz) * 2 - 1;
	
	// Transform from tangent space to world space -- this consists of just a rotation.
	//normal = mul(normal, worldRot);
	normal = mul(normal, (float3x3)world);
	
	// Compute the attenuation
	float attn = min((range * range) / lenSq, 1.0f);
	
	// Compute the diffuse coefficient
	float NdotL = attn * saturate(dot(lightDir, normal));
	
	float4 fixedA = float4(diffuseColor.rgb, 1.0f);
	fixedA = (ambient + NdotL) * fixedA;	
	return float4(fixedA.rgb, diffuseColor.a);
}

technique DefaultTechnique
{
	pass P0 // Texture mapping
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
		
		VertexShader = compile vs_1_1 transformVS();
		PixelShader = compile ps_2_0 vertexColoringLightingPS();
	}
	
	pass P1 // Normal mapping
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
		
		VertexShader = compile vs_1_1 transformVS();
		PixelShader = compile ps_2_0 lightingPS();
	}
	
	pass P2 // Plain old vertex-based coloring
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
		
		VertexShader = compile vs_1_1 transformVS();
		PixelShader = NULL;
	}
	
	pass P3 // Texture mapping
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
		
		VertexShader = compile vs_1_1 transformVS();
		PixelShader = compile ps_2_0 textureMappingPS();
	}
}