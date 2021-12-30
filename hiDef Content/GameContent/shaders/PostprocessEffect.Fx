//-----------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation. All rights reserved.
// - modified by Dan Cuccia 10/25/11
//-----------------------------------------------------------------------------




// Settings controlling the edge detection filter.
float EdgeWidth = 1;
float EdgeIntensity = 2;

float NormalThreshold = 0.5;
float DepthThreshold = 0.1;

float NormalSensitivity = 1;
float DepthSensitivity = 10;

float2 ScreenResolution;
float2 ScreenHalfPixel;

texture SceneTexture;
sampler SceneSampler : register(s0) = sampler_state
{
    Texture = (SceneTexture);
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture NormalDepthTexture;
sampler NormalDepthSampler : register(s1) = sampler_state
{
    Texture = (NormalDepthTexture);
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};



//--------------------------------------------------
//a default vertex shader that doesn't need normals
//--------------------------------------------------
struct VS_OUTPUT
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

VS_OUTPUT default_vs_main( float3 inPos:POSITION0, 
							float2 inTexCoord:TEXCOORD0 )
{
	VS_OUTPUT output = (VS_OUTPUT)0;

	output.Position = float4(inPos,1);
	output.TexCoord = inTexCoord - ScreenHalfPixel;
	
	return output;
}



float4 EdgeDetect_ps_main(float2 texCoord : TEXCOORD0,
							uniform bool outputLines) : COLOR0
{
    float3 scene = tex2D(SceneSampler, texCoord);
    
    float2 edgeOffset = EdgeWidth / ScreenResolution;
        
    float4 n1 = tex2D(NormalDepthSampler, texCoord + float2(-1, -1) * edgeOffset);
    float4 n2 = tex2D(NormalDepthSampler, texCoord + float2( 1,  1) * edgeOffset);
    float4 n3 = tex2D(NormalDepthSampler, texCoord + float2(-1,  1) * edgeOffset);
    float4 n4 = tex2D(NormalDepthSampler, texCoord + float2( 1, -1) * edgeOffset);

    float4 diagonalDelta = abs(n1 - n2) + abs(n3 - n4);

    float normalDelta = dot(diagonalDelta.xyz, 1);
    float depthDelta = diagonalDelta.w;
        
    normalDelta = saturate((normalDelta - NormalThreshold) * NormalSensitivity);
    depthDelta = saturate((depthDelta - DepthThreshold) * DepthSensitivity);

    float edgeAmount = saturate(normalDelta + depthDelta) * EdgeIntensity;

	if(outputLines)
	{
		edgeAmount = 1-edgeAmount;
		return edgeAmount;
	}
        
    scene *= (1 - edgeAmount);

    return float4(scene, 1);
}



float4 debug_ps_main(float2 texCoord : TEXCOORD0, 
						uniform bool showNormals,
						uniform bool showDiffuse,
						uniform bool showDepth ) : COLOR0
{
	if(showDiffuse)
		return tex2D( SceneSampler, texCoord );
	else if(showNormals)
		return tex2D( NormalDepthSampler, texCoord);
	else if(showDepth)
	{
		float4 output = tex2D( NormalDepthSampler, texCoord );
		float depth = 1 - (output.a / (output.r * output.g * output.b));
		return depth;
	}
}


//=========================================================================
//							TECHNIQUES
//=========================================================================

technique DebugShowNormals
{
	pass P0
	{
		VertexShader = compile vs_2_0 default_vs_main();
		PixelShader = compile ps_2_0 debug_ps_main(true, false, false);
	}
}

technique DebugShowDiffuse
{
	pass P0
	{
		VertexShader = compile vs_2_0 default_vs_main();
		PixelShader = compile ps_2_0 debug_ps_main(false, true, false);
	}
}

technique DebugShowDepth
{
	pass P0
	{
		VertexShader = compile vs_2_0 default_vs_main();
		PixelShader = compile ps_2_0 debug_ps_main(false, false, true);
	}
}


technique DebugShowSilhouette
{
	pass P0
	{
		VertexShader = compile vs_2_0 default_vs_main();
		PixelShader = compile ps_2_0 EdgeDetect_ps_main(true);
	}
}


technique EdgeDetect
{
    pass P0
    {
		VertexShader = compile vs_2_0 default_vs_main();
        PixelShader = compile ps_2_0 EdgeDetect_ps_main(false);
    }
}


