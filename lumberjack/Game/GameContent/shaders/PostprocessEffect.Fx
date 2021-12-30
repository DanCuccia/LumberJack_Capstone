//-----------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation. All rights reserved.
// - modified by Dan Cuccia 10/25/11
//-----------------------------------------------------------------------------




// Settings controlling the edge detection filter.
float EdgeWidth = 1;
float EdgeIntensity = 2;

// How sensitive should the edge detection be to tiny variations in the input data?
// Smaller settings will make it pick up more subtle edges, while larger values get
// rid of unwanted noise.
float NormalThreshold = 0.5;
float DepthThreshold = 0.1;

// How dark should the edges get in response to changes in the input data?
float NormalSensitivity = 1;
float DepthSensitivity = 10;

// Pass in the current screen resolution.
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
    // Look up the original color from the main scene.
    float3 scene = tex2D(SceneSampler, texCoord);
    

    // Look up four values from the normal/depth texture, offset along the
    // four diagonals from the pixel we are currently shading.
    float2 edgeOffset = EdgeWidth / ScreenResolution;
        
    float4 n1 = tex2D(NormalDepthSampler, texCoord + float2(-1, -1) * edgeOffset);
    float4 n2 = tex2D(NormalDepthSampler, texCoord + float2( 1,  1) * edgeOffset);
    float4 n3 = tex2D(NormalDepthSampler, texCoord + float2(-1,  1) * edgeOffset);
    float4 n4 = tex2D(NormalDepthSampler, texCoord + float2( 1, -1) * edgeOffset);

    // Work out how much the normal and depth values are changing.
    float4 diagonalDelta = abs(n1 - n2) + abs(n3 - n4);

    float normalDelta = dot(diagonalDelta.xyz, 1);
    float depthDelta = diagonalDelta.w;
        
    // Filter out very small changes, in order to produce nice clean results.
    normalDelta = saturate((normalDelta - NormalThreshold) * NormalSensitivity);
    depthDelta = saturate((depthDelta - DepthThreshold) * DepthSensitivity);

    // Does this pixel lie on an edge?
    float edgeAmount = saturate(normalDelta + depthDelta) * EdgeIntensity;

	if(outputLines)
	{
		edgeAmount = 1-edgeAmount;
		return edgeAmount;
	}
        
    // Apply the edge detection result to the main scene color.
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


