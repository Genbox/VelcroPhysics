float4x4 Projection;

struct Vertex
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;
};

Vertex VertexShaderFunction(Vertex input)
{
    input.Position = mul(input.Position, Projection);
    return input;
}

float4 PixelShaderFunction(Vertex input) : COLOR0
{
    return input.Color;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_1_1 VertexShaderFunction();
        PixelShader = compile ps_1_1 PixelShaderFunction();
    }
}
