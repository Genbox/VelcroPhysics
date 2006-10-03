//Input variables
float4x4 worldViewProjection;

struct VS_INPUT
{
	float4 ObjectPos: POSITION;
	float4 VertexColor: COLOR;
};

struct VS_OUTPUT 
{
   float4 ScreenPos:   POSITION;
   float4 VertexColor: COLOR;
};


VS_OUTPUT SimpleVS(VS_INPUT In)
{
   VS_OUTPUT Out;

	//Move to screen space
	Out.ScreenPos = mul(In.ObjectPos, worldViewProjection);
	Out.VertexColor = In.VertexColor;

    return Out;
}

//--------------------------------------------------------------//
// Technique Section for Simple screen transform
//--------------------------------------------------------------//
technique Simple
{
   pass Single_Pass
   {
		VertexShader = compile vs_1_1 SimpleVS();
		PixelShader = null;
   }

}
