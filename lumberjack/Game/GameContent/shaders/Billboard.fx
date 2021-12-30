float4x4 World;
float4x4 View;
float4x4 Projection;

float3 Camera_Position;
float3 Rotation_Direction = float3(0,1,0);

// -- BillBoard Particles --
//--------------------------
Texture BillboardTex;
sampler texSamp0 = sampler_state
{
	texture		= <BillboardTex>;
	magfilter	= LINEAR;
	minfilter	= LINEAR;	
	mipfilter	= LINEAR;
	AddressU	= CLAMP;
	AddressV	= CLAMP;
};

struct VS_OUTPUT
{
	float4 Position : POSITION;
	float2 TexCoord : TEXCOORD0;
	float2 AlphaBlend : TEXCOORD1;
};

VS_OUTPUT vs_main(	float3 inPos : POSITION0, 
					float4 inTexCoordScale : TEXCOORD0,
					float inBlend : BLENDWEIGHT0 )
{
	VS_OUTPUT output = (VS_OUTPUT)0;
	
	output.AlphaBlend = float2(inBlend, inBlend);
	
	float4 positionDot4 = float4(inPos, 1);

	float4 center = mul(positionDot4, World);

	float3 eyeV = center - Camera_Position;
	float3 upV = normalize(Rotation_Direction);
	float3 sideV = normalize(cross(eyeV, upV));

	float3 finalPos = center;

	finalPos += (inTexCoordScale.x - .5f) * sideV * inTexCoordScale.z;
	finalPos += (1.5f - inTexCoordScale.y * 1.5f) * upV * (inTexCoordScale.w*.75);

	output.Position = mul(float4(finalPos, center.w), mul(View, Projection));
	output.TexCoord = float2(inTexCoordScale.x, inTexCoordScale.y);

	return output;
}


float4 ps_main(	float4 inPosition : POSITION,
				float2 inTexCoord : TEXCOORD0,
				float2 inAlpha : TEXCOORD1 ) : COLOR
{
	float4 color = tex2D(texSamp0, inTexCoord);
	//if(color.a > 0)
	//	color.a *= inAlpha.x;

	return color;//float4(color.rgb, inAlpha.x);
}

technique BillboardParticle
{
	pass Billboard
	{
		AlphaBlendEnable = true;
		SrcBlend = One;
		DestBlend = InvSrcAlpha;

		ZENABLE = false;
		ZWRITEENABLE = false;

		VertexShader = compile vs_2_0 vs_main();
		PixelShader = compile ps_2_0 ps_main();
	}
}






//-------------------------------------------------------
//
//				-- Billboard Sprites -- 
//
//-------------------------------------------------------


struct VS_OUTPUT_SPR
{
	float4 Position : POSITION;
	float2 TexCoord : TEXCOORD0;
};


VS_OUTPUT_SPR vs_main_spr(	float3 inPos : POSITION0, 
					float4 inTexCoord : TEXCOORD0,
					float4 inSizeScale : TEXCOORD1 )
{
	VS_OUTPUT_SPR output = (VS_OUTPUT_SPR)0;

	float4 center = mul(float4(inPos, 1), World);

	float3 eyeV = center - Camera_Position;
	float3 upV = normalize(Rotation_Direction);
	float3 sideV = normalize(cross(eyeV, upV));

	float3 finalPos = center;
	if(inTexCoord.x == 0)
		finalPos += ( ( -(inSizeScale.x/2) * sideV ) /10 ) * inSizeScale.z;
	else
		finalPos += ( ( (inSizeScale.x/2) * sideV ) /10 ) * inSizeScale.z;
	
	if(inTexCoord.y == 0)
		finalPos.xyz += ( ( inSizeScale.y  * upV ) /10 ) * inSizeScale.w;
	else
		finalPos.xyz += ( ( finalPos * upV ) /10 ) * inSizeScale.w;

	output.Position = mul(float4(finalPos, center.w), mul(View, Projection));
	output.TexCoord.x = inTexCoord.z;
	output.TexCoord.y = inTexCoord.w;

	return output;
}


float4 ps_main_spr(	float4 inPosition : POSITION,
				float2 inTexCoord : TEXCOORD0 ) : COLOR
{
	return tex2D(texSamp0, inTexCoord);
}

technique BillboardSprite
{
	pass Billboard
	{
		AlphaBlendEnable = true;
		SrcBlend = ONE;
		DestBlend = InvSrcAlpha;

		CullMode = None;
		ZENABLE = false;
		ZWRITEENABLE = false;

		VertexShader = compile vs_2_0 vs_main_spr();
		PixelShader = compile ps_2_0 ps_main_spr();
	}

}