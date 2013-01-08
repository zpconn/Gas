float4x4 world : WORLD;
float4x4 worldRot;
float4x4 worldViewProj : WORLDVIEWPROJECTION;
float4 lightPos[4];
float4 shadowCasterCenter;
float4 shadowDir;
float4 eyePos;
float4 lightColor[4];
float range[4];
int numActiveLights;
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

float4 noLighting(float2 texCoord0 : TEXCOORD0) : COLOR0
{
	return tex2D(diffuseSampler, texCoord0);
}

float4 textureMappingPS(float2 texCoord0 : TEXCOORD0,
                        float2 worldPos : TEXCOORD1) : COLOR0
{
	float4 tmpColor = float4(0,0,0,0);

    for (int i = 0; i < numActiveLights; i++)
    {
    	float2 deltaLightPos = float2(lightPos[i].xy) - float2(worldPos.xy);
		float3 lightDir = float3(deltaLightPos.xy, 250.0f);
		float lenSq = dot(lightDir, lightDir);
	
		float attn = min((range[i] * range[i]) / lenSq, 1.0f);
		
		tmpColor.xyz += lightColor[i] * attn;
    }
	
	float4 color = tex2D(diffuseSampler, texCoord0);
	float4 fixedA = float4(color.rgb, 1.0f) * tmpColor;
	
	return float4(fixedA.rgb, color.a);
}

float4 shadowPS(float2 texCoord0 : TEXCOORD0,
                float2 worldPos : TEXCOORD1) : COLOR0
{
	float4 tmpColor = float4(0,0,0,0);
    
    for (int i = 0; i < numActiveLights; i++)
    {
    	float2 deltaLightPos = float2(lightPos[i].xy) - float2(worldPos.xy);
		float3 lightDir = float3(deltaLightPos.xy, 250.0f);
		float lenSq = dot(lightDir, lightDir);
		
		float attn = min((range[0] * range[0]) / lenSq, 1.0f);
		
		tmpColor.xyz += lightColor[i] * attn;
    }
    
    float brightness = saturate(tmpColor.r + tmpColor.g + tmpColor.b);
    return float4(0, 0, 0, 1 - brightness);
    
    //float2 deltaPos = float2(worldPos.xy) - float2(shadowCasterCenter.xy);
	//float lenSq = dot(deltaPos, deltaPos);
	
	// We have a parametric ray defined by 
	//
	// p(t) = p_0 + t*d
	//
	// where
	//
	// p_0 = shadowCasterCenter and d = shadowDir
	//
	// We also have a point worldPos. We wish to find the distance between worldPos and p(t)
	// along the line normal to the ray. We find this by translating shadowCasterCenter by
	// -p_0, then calculating the perp-dot product of the translated point with shadowDir.
	//
	// The perp-dot product of A and B is perp-dot(A,B) = |A| * |B| * sin(theta)
	// 
	// where
	//
	// theta = arcTan( dot(A,B) / (|A| * |B|) )
	//
	// In our case, A = raySpacePos (in the below code) and B = shadowDir. Further, we know
	// that |shadowDir| = |B| = 1. Thus |A| * |B| = |A| = |raySpacePos|.
	
	//float2 raySpacePos = worldPos.xy - shadowCasterCenter.xy;
	//float magProd = length(raySpacePos);
	//float theta = atan( dot(raySpacePos, shadowDir.xy) / magProd );
	//float lengthFromRay = magProd * sin(theta);
	
	//float shadowLength = 150.0f;
	
	// We know have the distance of worldPos from shadowCasterCenter and from the shadow direction.
	// We want lenSq to make the shadow darker, while we want lengthFromRay to make it more transparent.
	
	//float distanceAvg = lengthFromRay * 130;
	//float attn = min((shadowLength * shadowLength) / distanceAvg, 1.0f);
	
	//return float4(0, 0, 0, attn);
}

//float4 shadowPS(float2 texCoord0 : TEXCOORD0,
//                float2 worldPos : TEXCOORD1,
//                float4 color : COLOR0) : COLOR0
//{
//	float2 deltaLightPos = float2(lightPos[0].xy) - float2(worldPos.xy);
//	float3 lightDir = float3(deltaLightPos.xy, 250.0f);
//	float lenSq = dot(lightDir, lightDir);
//
//	float attn = min((range[0] * range[0]) / lenSq, 1.0f);
//	
//	return float4(0,0,0,attn);
//}

float4 lightingPS(float2 texCoord0 : TEXCOORD0,
                  float2 worldPos : TEXCOORD1) : COLOR0
{
	float4 diffuseColor = tex2D(diffuseSampler, texCoord0);
	float4 normalColor = tex2D(normalSampler, texCoord0);
	
	float3 normal = float3(normalColor.xyz) * 2 - 1;
	normal = mul(normal, (float3x3)world);
	
	float4 tmpColor = float4(0,0,0,0);
	
	for (int i = 0; i < numActiveLights; i++)
	{
	    float2 deltaLightPos = float2(lightPos[i].xy) - float2(worldPos.xy);
		float3 lightDir = float3(deltaLightPos.xy, 250.0f);
		float lenSq = dot(lightDir, lightDir);
		lightDir = normalize(lightDir);
	
		float attn = min((range[i] * range[i]) / lenSq, 1.0f);
		
		float NdotL = saturate(dot(lightDir, normal));
		
		tmpColor += attn * NdotL * lightColor[i];
	}
	
	float4 fixedA = float4(diffuseColor.rgb, 1.0f) * tmpColor;
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
		ZEnable = true;
		ZWriteEnable = true;
		FillMode = Solid;
		
		VertexShader = compile vs_1_1 transformVS();
		PixelShader = compile ps_2_0 textureMappingPS();
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
		ZEnable = true;
		ZWriteEnable = true;
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
		ZEnable = true;
		ZWriteEnable = true;
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
		ZEnable = true;
		ZWriteEnable = true;
		FillMode = Solid;
		
		VertexShader = compile vs_1_1 transformVS();
		PixelShader = compile ps_2_0 textureMappingPS();
	}
	
	pass P4 // Shadows
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
		PixelShader = compile ps_2_0 shadowPS();
	}
	
	pass P5
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
		PixelShader = compile ps_2_0 noLighting();
	}
}