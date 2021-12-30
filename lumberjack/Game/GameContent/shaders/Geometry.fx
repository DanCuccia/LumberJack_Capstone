float4x4 World;
float4x4 View;
float4x4 Projection;

float Opacity
<
	string UIName = "Opacity";
	string UIWidget = "Slider";
	float  UIMin 	= 0.0f;
	float  UIMax 	= 1.0f;
	float  UIStep 	= 0.001f;
> = 0.3f;


struct VS_OUTPUT
{
    float4 Position : POSITION0;
	float4 Color: COLOR0;
};

VS_OUTPUT vs_main(float4 inPos : POSITION0,
				float4 inColor	: COLOR0)
{
    VS_OUTPUT output = (VS_OUTPUT)0;

    output.Position = mul(mul(mul(inPos, World), View), Projection);
	output.Color = inColor;

    return output;
}


float4 ps_main(float4 inPos : POSITION0,
				float4 inColor	: COLOR0) : COLOR0
{
    return float4(inColor.rgb, Opacity);
}


technique Geometry
{
    pass Geometry
    {
		CullMode = NONE;

        VertexShader = compile vs_2_0 vs_main();

		ZEnable = true;
		ZWriteEnable = true;
		AlphaBlendEnable = true;
		SrcBlend = SRCALPHA;
		DestBlend = INVSRCALPHA;

        PixelShader = compile ps_2_0 ps_main();
    }
}
