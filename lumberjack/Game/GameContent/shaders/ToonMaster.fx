//-----------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation. All rights reserved.	(Depth Filter Base Code)
// Copyright (C) 2004 Anirudh S Shastry.						(Water Shader Base Code)
//
//	All Other Code...
//		by Dan Cuccia 10/25/11
//-----------------------------------------------------------------------------

//==========================================================================
//						GLOBAL VARIABLES
//==========================================================================

float4x4	World		: WORLD;
float4x4	View		: VIEW;
float4x4	ViewInverse : VIEWINVERSE;
float4x4	Projection	: PROJECTION;

float		Time		: TIME;

float3		CameraPosition;
float3		LightDirection		= normalize(float3(1, 1, -1));


float4		ModelColor;					//per model


//==========================================================================
//						TEXTURES AND SAMPLERS
//==========================================================================

bool TextureEnabled;				//per model

texture Texture	: register(t0);		//per model
sampler objectSampler : register(s0) = sampler_state
{
    Texture = (Texture);
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = WRAP;
    AddressV = WRAP;
};

texture Noise_Tex : register(t1);		//wood
sampler noise_volume : register(s1) = sampler_state
{
   Texture = (Noise_Tex);
   MAGFILTER = LINEAR;
   MINFILTER = LINEAR;
   MIPFILTER = LINEAR;
   ADDRESSU = WRAP;
   ADDRESSV = WRAP;
   ADDRESSW = WRAP;
};

texture pulse_train_Tex : register(t2);	//wood
sampler pulse_train  : register(s2)  = sampler_state
{
   Texture = (pulse_train_Tex);
   MAGFILTER = LINEAR;
   MINFILTER = LINEAR;
   MIPFILTER = LINEAR;
   ADDRESSU = WRAP;
};

texture lowTex : register(t3);			//terrain
sampler terrainSamp0  : register(s3)   = sampler_state
{
	Texture = (lowTex);
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
	MAGFILTER = LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
};

texture lowMidTex : register(t4);		//terrain
sampler terrainSamp1 : register(s4)  = sampler_state
{
	Texture = (lowMidTex);
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
	MAGFILTER = LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
};

texture highMidTex : register(t5);		//terrain
sampler terrainSamp2 : register(s5)  = sampler_state
{
	Texture = (highMidTex);
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
	MAGFILTER = LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
};

texture SkyBoxTexture : register(t6);	//skybox
samplerCUBE SkyBoxSampler : register(s6) = sampler_state 
{ 
   texture = <SkyBoxTexture>; 
   magfilter = LINEAR; 
   minfilter = LINEAR; 
   mipfilter = LINEAR; 
   AddressU = MIRROR; 
   AddressV = MIRROR; 
};

texture WaterNormalMap : register(t7);	//water normal mapping
sampler WaterNormalSampler : register(s7) = sampler_state
{
	texture = <WaterNormalMap>;
	MipFilter 	= LINEAR;
	MinFilter 	= LINEAR;
	MagFilter 	= LINEAR;
};


//=========================================================================
//						SHADER-SPECIFIC HARD CODES
//=========================================================================

// toon controllers
//-----------------
float	ToonThresholds[2]				= { 0.8, 0.4 };
float	ToonBrightnessLevels[3]			= { 1.3, 0.9, 0.5 };
float	TerrainToonThresholds[3]		= { 0.8, 0.6, 0.4 };
float	TerrainToonBrightnessLevels[4]	= { 1.3, 1.0, 0.7, 0.5 };

// wood controllers
//------------------
float	ring_freq				= float( 13.57 );
float	noise_amplitude			= float( 0.08 );
float4	light_wood_color		= float4( 1.00, 0.70, 0.15, 1.00 );
float4	dark_wood_color			= float4( 0.55, 0.30, 0.07, 1.00 );
float4	dark_light_wood_color	= float4( 0.50196, 0.274509, 0.101960, 1.00 );
float4	dark_dark_wood_color	= float4( 0.341176, 0.152941, 0.011764, 1.00 );

float4x4 texture_matrix0 = float4x4( 
	0.04, 0.00, 0.00, 0.00, 
	0.00, 0.00, 0.04, 0.00, 
	0.00, 0.04, 0.00, 0.00, 
	0.00, 0.00, 0.00, 1.00 );

float4x4 texture_matrix2 = float4x4( 
	0.04, 0.00, 0.00, 0.00, 
	0.00, -0.04, 0.00, 0.00, 
	0.00, 0.00, -0.04, 0.00, 
	0.00, 0.00, 0.00, 1.00 );

float4x4 texture_matrix1 = float4x4( 
	-0.04, 0.00, 0.00, 0.00, 
	0.00, 0.04, 0.00, 0.00, 
	0.00, 0.00, 0.04, 0.00, 
	0.00, 0.00, 0.00, 1.00 );


// water controllers
//-------------------
float	BumpHeight			= 0.5f;
float2	TextureScale		= { 4.0f, 4.0f };
float2	BumpSpeed			= { 0.0f, 0.01f };
float	FresnelBias			= 0.025f;
float	FresnelPower		= 1.0f;
float	HDRMultiplier		= 1.0f;
float4	DeepColor			= { 0.0f, 0.40f, 0.50f, 1.0f };
float4	ShallowColor		= { 0.55f, 0.75f, 0.75f, 1.0f };
float4	ReflectionColor		= { 1.0f, 1.0f, 1.0f, 1.0f };
float	ReflectionAmount	= 0.5f;
float	WaterAmount			= 0.5f;






//======================================================================
//								TOON SHADING
//======================================================================

struct LightingVertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TextureCoordinate : TEXCOORD0;
    float LightAmount : TEXCOORD1;
};

LightingVertexShaderOutput LightingVertexShader(float4 inPos : POSITION0,
													float3 inNormal : NORMAL0,
													float2 inTexCoord : TEXCOORD0 )
{
    LightingVertexShaderOutput output;

    output.Position = mul(mul(mul(inPos, World), View), Projection);
    output.TextureCoordinate = inTexCoord;

    float3 worldNormal = normalize(mul(inNormal,  World));
    output.LightAmount = dot(worldNormal, LightDirection);
    
    return output;
}


float4 ToonPixelShader(float2 inTexcoord : TEXCOORD0,
						float inLightAmount : TEXCOORD1 ) : COLOR0
{
    float4 color = TextureEnabled ? tex2D(objectSampler, inTexcoord) : ModelColor;
    
    float light;

    if (inLightAmount > ToonThresholds[0])
        light = ToonBrightnessLevels[0];
    else if (inLightAmount > ToonThresholds[1])
        light = ToonBrightnessLevels[1];
    else
        light = ToonBrightnessLevels[2];
                
    color.rgb *= light;
    
    return color;
}





//====================================================================
//						NORMAL, DEPTH SHADING
//====================================================================

struct NormalDepth_VS_OUTPUT
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

NormalDepth_VS_OUTPUT NormalDepthVertexShader( float4 inPosition : POSITION0,
												float3 inNormal : NORMAL0,
												float2 inTexcoord : TEXCOORD0 )
{
    NormalDepth_VS_OUTPUT output;

    output.Position = mul(mul(mul(inPosition, World), View), Projection);
    
    float3 worldNormal = normalize(mul(inNormal, World));

    // The output color holds the normal, scaled to fit into a 0 to 1 range.
    output.Color.rgb = (worldNormal + 1) / 2;

    // The output alpha holds the depth, scaled between 50000 (as a temp far plane distance).
    output.Color.a = output.Position.z / 50000;
    
    return output;    
}


float4 NormalDepthPixelShader(float4 color : COLOR0) : COLOR0
{
    return color;
}






//==============================================================
//							WOOD-TOON SHADING
//==============================================================

struct WoodToon_VS_OUTPUT{
   float4 Pos     : POSITION;
   float3 TCoord0 : TEXCOORD0;
   float3 TCoord1 : TEXCOORD1;
   float3 TCoord2 : TEXCOORD2;
   float LightAmount : TEXCOORD3;
};

WoodToon_VS_OUTPUT WoodToon_vs_main(float4 vPosition: POSITION0,
									float3 inNormal : NORMAL0 )
{
   WoodToon_VS_OUTPUT output = (WoodToon_VS_OUTPUT) 0; 

   output.Pos = mul(mul(mul(vPosition, World), View), Projection);

   // Transform Pshade (using texture matrices)
   output.TCoord0 = mul (texture_matrix0, vPosition);
   output.TCoord1 = mul (texture_matrix1, vPosition);
   output.TCoord2 = mul (texture_matrix2, vPosition);

   float3 worldNormal = normalize(mul(inNormal, World));
   output.LightAmount = dot(worldNormal, LightDirection);

   return output;
}

float4 WoodToon_ps_main(uniform bool isDarkWood,
						float3 Pshade0 : TEXCOORD0, 
						float3 Pshade1 : TEXCOORD1, 
						float3 Pshade2 : TEXCOORD2,
						float inLightAmount : TEXCOORD3 ) : COLOR
{
    float3 coloredNoise;

    // Construct colored noise from three samples
    coloredNoise.x = tex3D (noise_volume, Pshade0).r;
    coloredNoise.y = tex3D (noise_volume, Pshade1).r;
    coloredNoise.z = tex3D (noise_volume, Pshade2).r;

    coloredNoise = coloredNoise * 2.0f - 1.0f;

    // Scale noise and add to Pshade
    float3 noisyPshade = Pshade0 + coloredNoise * noise_amplitude;

    float scaledDistFromZAxis = sqrt(dot(noisyPshade.xy, noisyPshade.xy)) * ring_freq;

    float4 blendFactor = tex1D (pulse_train, scaledDistFromZAxis);
    
	float4 woodColor;
	if(isDarkWood)
		woodColor =  lerp (dark_dark_wood_color, dark_light_wood_color, blendFactor.x);
	else
		woodColor =  lerp (dark_wood_color, light_wood_color, blendFactor.x);

	float light;

    if (inLightAmount > ToonThresholds[0])
        light = ToonBrightnessLevels[0];
    else if (inLightAmount > ToonThresholds[1])
        light = ToonBrightnessLevels[1];
    else
        light = ToonBrightnessLevels[2];
                
    woodColor.rgb *= light;

	return woodColor;
}







//==============================================================
//				TOON - GENERATED TERRAIN
//==============================================================


struct Terrain_VS_OUTPUT{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float3 TexWeight : TEXCOORD1;
	float LightAmount : TEXCOORD2;
	//float4 Color : COLOR0;
};

Terrain_VS_OUTPUT ToonTerrain_vs_main(  float4 inPosition	: POSITION0,
										float3 inNormal		: NORMAL0,
										float2 inTexcoord	: TEXCOORD0,
										float3 inTexWeight	: TEXCOORD1 )
{
	Terrain_VS_OUTPUT output = (Terrain_VS_OUTPUT)0;

	output.Position = mul(mul(mul(inPosition, World), View), Projection);
	output.TexCoord = inTexcoord;
	output.TexWeight = inTexWeight;

	float3 worldNormal = mul(inNormal, World);
    output.LightAmount = dot(worldNormal, LightDirection);

	return output;
}

float4 ToonTerrain_ps_main( float2 inTexcoord : TEXCOORD0,
							float4 inTexWeight : TEXCOORD1,
							float inLightAmount : TEXCOORD2 ) : COLOR0
{
	float4 output;
	output = tex2D(terrainSamp0, inTexcoord) * inTexWeight.y;
	output += tex2D(terrainSamp1, inTexcoord) * inTexWeight.x;
	output += tex2D(terrainSamp2, inTexcoord) * inTexWeight.z;

	float light;

    if (inLightAmount > TerrainToonThresholds[0])
        light = TerrainToonBrightnessLevels[0];
    else if (inLightAmount > TerrainToonThresholds[1])
        light = TerrainToonBrightnessLevels[1];
    else if (inLightAmount > TerrainToonThresholds[2])
        light = TerrainToonBrightnessLevels[2];
	else 
		light = TerrainToonBrightnessLevels[3];

	output.rgb *= light;
	return output;
}




//==============================================================
//					TOON TERRAIN VERTEX COLORING
//==============================================================


struct ColorTerrain_VS_OUTPUT
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
	float LightAmount : TEXCOORD0;
};

ColorTerrain_VS_OUTPUT ColoredTerrain_vs_main(	float4 inPosition : POSITION0,
												float3 inNormal : NORMAL0,
												float4 inColor : TEXCOORD0 )
{

	ColorTerrain_VS_OUTPUT output = (ColorTerrain_VS_OUTPUT)0;

	output.Position = mul(mul(mul(inPosition, World), View), Projection);
	output.Color = inColor;

	float3 worldNormal = mul(inNormal, World);
    output.LightAmount = dot(worldNormal, LightDirection);

	return output;
}


float4 ColoredTerrain_ps_main(	float4 inColor : COLOR0,
								float inLightAmount : TEXCOORD0 ) : COLOR0
{
	float4 color = inColor;

	float light;
    if (inLightAmount > TerrainToonThresholds[0])
        light = TerrainToonBrightnessLevels[0];
    else if (inLightAmount > TerrainToonThresholds[1])
        light = TerrainToonBrightnessLevels[1];
    else if( inLightAmount > TerrainToonThresholds[2])
        light = TerrainToonBrightnessLevels[2];
	else
		light = TerrainToonBrightnessLevels[3];

	color.rgb *= light;
	return color;
}





//==============================================================
//					TOON VERTEX COLORING
//==============================================================

struct VERTEXCOLOR_VS_OUTPUT
{
	float4 Position : POSITION0;
	float LightAmount : TEXCOORD0;
	float4 Color : COLOR0;
};

VERTEXCOLOR_VS_OUTPUT ToonVertexColor_vs_main( float4 inPosition : POSITION0, 
												float3 inNormal : NORMAL0 )
{
	VERTEXCOLOR_VS_OUTPUT output = (VERTEXCOLOR_VS_OUTPUT)0;

	output.Position = mul(mul(mul(inPosition, World), View), Projection);

	output.Color = ModelColor;

	float3 worldNormal = normalize(mul(inNormal, World));
    output.LightAmount = dot(worldNormal, LightDirection);

	return output;
}

float4 ToonVertexColor_ps_main( float4 inColor : COLOR0,
								float inLightAmount : TEXCOORD0 ) : COLOR0
{
	float4 output = float4(inColor.rgb,1);

	float light;

    if (inLightAmount > ToonThresholds[0])
        light = ToonBrightnessLevels[0];
    else if (inLightAmount > ToonThresholds[1])
        light = ToonBrightnessLevels[1];
    else
        light = ToonBrightnessLevels[2];

	output.rgb *= light;

	return output;
}





//==============================================================
//						TOON - WATER
//==============================================================

struct ToonWater_VS_OUTPUT
{
	float4 Position	: POSITION;
	float2 TexCoord	: TEXCOORD0;
	float3 TanToCube[3]	: TEXCOORD1;
	float2 Bump0 : TEXCOORD4;
	float2 Bump1 : TEXCOORD5;
	float2 Bump2 : TEXCOORD6;
	float3 View	: TEXCOORD7;
};

ToonWater_VS_OUTPUT ToonWater_vs_main( float4 inPosition : POSITION0,
										float3 inNormal : NORMAL0,
										float3 inTangent : TANGENT,
										float3 inBinormal : BINORMAL,
										float2 inTexCoord : TEXCOORD0 )
{
	ToonWater_VS_OUTPUT output = (ToonWater_VS_OUTPUT)0;

	output.Position = mul(mul(mul(inPosition, World), View), Projection);
	output.TexCoord = inTexCoord * TextureScale;

	float time = fmod( Time, 100.0 );
	output.Bump0 = inTexCoord * TextureScale + time * BumpSpeed;
	output.Bump1 = inTexCoord * TextureScale * 2.0f + time * BumpSpeed * 4.0;
	output.Bump2 = inTexCoord * TextureScale * 4.0f + time * BumpSpeed * 8.0;

	float3x3 tangentSpace;
	tangentSpace[0] = BumpHeight * normalize(inTangent);
	tangentSpace[1] = BumpHeight * normalize(inBinormal);
	tangentSpace[2] = normalize(mul(inNormal, World));

	output.TanToCube[0] = mul( tangentSpace, World[0].xyz );
	output.TanToCube[1] = mul( tangentSpace, World[1].xyz );
	output.TanToCube[2] = mul( tangentSpace, World[2].xyz );

	float4 worldPos = mul( inPosition, World );
	output.View = ViewInverse[3].xyz - worldPos;

	return output;
}


float3 Refract( float3 inverse, float3 normal, float refractionIndex, out bool fail )
{
	float dotNorm = dot( inverse, normal );
	float k = 1 - refractionIndex * refractionIndex * ( 1 - dotNorm * dotNorm );
	fail = k < 0;
	return refractionIndex * inverse - ( refractionIndex * dotNorm + sqrt(k) )* normal;
}

float4 ToonWater_ps_main( float2 inTexCoord : TEXCOORD0,
							float3 inTanToCube[3] : TEXCOORD1,
							float2 inBump0 : TEXCOORD4,
							float2 inBump1 : TEXCOORD5,
							float2 inBump2 : TEXCOORD6,
							float3 inView : TEXCOORD7 ) : COLOR0
{
	float4 t0 = tex2D( WaterNormalSampler, inBump0 ) * 2.0f - 1.0f;
    float4 t1 = tex2D( WaterNormalSampler, inBump1 ) * 2.0f - 1.0f;
    float4 t2 = tex2D( WaterNormalSampler, inBump2 ) * 2.0f - 1.0f;

	float3 vN = t0.xyz + t1.xyz + t2.xyz;

	float3x3 tanToWorld;
    tanToWorld[0] = inTanToCube[0];
    tanToWorld[1] = inTanToCube[1];
    tanToWorld[2] = inTanToCube[2];

	float3 worldNormal = mul( tanToWorld, vN );
    worldNormal = normalize( worldNormal );

	inView = normalize( inView );
    float3 vR = reflect( -inView, worldNormal );

	float4 reflect = texCUBE( SkyBoxSampler, vR.zyx );    
    reflect = texCUBE( SkyBoxSampler, vR );
	reflect.rgb *= ( 1.0 + reflect.a * HDRMultiplier );

	float facing  = 1.0 - max( dot( inView, worldNormal ), 0 );
    float fresnel = FresnelBias + ( 1.0 - FresnelBias ) * pow( facing, FresnelPower);

	float4 waterColor = lerp( DeepColor, ShallowColor, facing );

	return waterColor * WaterAmount + reflect * ReflectionColor * ReflectionAmount * fresnel;
}


struct SkyBox_VS_OUTPUT
{
	float4 Position : POSITION0;
	float3 TexCoord : TEXCOORD0;
};

SkyBox_VS_OUTPUT SkyBox_vs_main( float4 inPosition : POSITION0 )
{
	SkyBox_VS_OUTPUT output = (SkyBox_VS_OUTPUT)0;

	output.Position = mul(mul(mul(inPosition, World), View), Projection);

	float4 VertexPosition = mul(inPosition, World);
	output.TexCoord = VertexPosition - CameraPosition;

    return output;
}

float4 SkyBox_ps_main( float3 inTexCoord : TEXCOORD0 ) : COLOR0
{
	return texCUBE(SkyBoxSampler, normalize(inTexCoord));
}





//==============================================================
//					TECHNIQUES
//==============================================================

// typical skybox technique						//SKYBOX
technique SkyBox
{
	pass P0
	{
		VertexShader = compile vs_2_0 SkyBox_vs_main();
		PixelShader = compile ps_2_0 SkyBox_ps_main();
	}
}

// normal and depth values.						//DEPTH 
technique NormalDepth
{
    pass P0
    {
        VertexShader = compile vs_2_0 NormalDepthVertexShader();
        PixelShader = compile ps_2_0 NormalDepthPixelShader();
    }
}



// banded cartoon shading.						//DIFFUSE 
technique Toon
{
    pass P0
    {
        VertexShader = compile vs_2_0 LightingVertexShader();
        PixelShader = compile ps_2_0 ToonPixelShader();
    }
}


// auto-wood shading in toon lighting			//WOOD_TOON
technique WoodToon
{
	pass P0
	{
		VertexShader = compile vs_2_0 WoodToon_vs_main();
		PixelShader = compile ps_2_0 WoodToon_ps_main(false);
	}
}

// dark-wood shading in toon lighting			// DARK_WOOD_TOON
technique DarkWood
{
	pass P0
	{
		VertexShader = compile vs_2_0 WoodToon_vs_main();
		PixelShader = compile ps_2_0 WoodToon_ps_main(true);
	}
}


// toon-lit generated terrain					//TERRAIN_TOON
technique TerrainToon
{
	pass P0
	{
		VertexShader = compile vs_2_0 ToonTerrain_vs_main();
		PixelShader = compile ps_2_0 ToonTerrain_ps_main();
	}
}


//toon-lit vertex colored terrain				//TERRAIN_TOON_COLORED
technique TerrainColoredToon
{
	pass P0
	{
		VertexShader = compile vs_2_0 ColoredTerrain_vs_main();
		PixelShader = compile ps_2_0 ColoredTerrain_ps_main();
	}
}


// toon-lit vertex coloring
technique VertexColor							//VERTEX COLORING
{
	pass P0
	{
		VertexShader = compile vs_2_0 ToonVertexColor_vs_main();
		PixelShader = compile ps_2_0 ToonVertexColor_ps_main();
	}
}


// toon-lit water								//WATER
technique WaterToon
{
	pass P0
	{
		VertexShader = compile vs_2_0 ToonWater_vs_main();
		PixelShader = compile ps_2_0 ToonWater_ps_main();
	}
}