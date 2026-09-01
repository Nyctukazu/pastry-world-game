matrix MatrixTransform;

Texture2D SpriteTexture;
sampler TextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

void SpriteVertexShader(inout float4 color : COLOR0,
                         inout float2 texCoord : TEXCOORD0,
                         inout float4 position : SV_Position)
{
    position = mul(position, MatrixTransform);
}

float4 ScreenPS(float2 texCoord : TEXCOORD0, float4 color : COLOR0) : SV_Target0
{
    float4 tex = SpriteTexture.Sample(TextureSampler, texCoord) * color;
    float3 inv = float3(1.0f, 1.0f, 1.0f) - tex.rgb;
    float3 result = float3(1.0f, 1.0f, 1.0f) - (inv * inv);
    return float4(result, tex.a);
}

float4 SubtractPS(float2 texCoord : TEXCOORD0, float4 color : COLOR0) : SV_Target0
{
    float4 tex = SpriteTexture.Sample(TextureSampler, texCoord) * color;
    float3 result = saturate(tex.rgb - float3(0.3f, 0.3f, 0.3f));
    return float4(result, tex.a);
}

technique Screen
{
    pass P0
    {
        VertexShader = compile vs_3_0 SpriteVertexShader();
        PixelShader = compile ps_3_0 ScreenPS();
    }
}

technique Subtract
{
    pass P0
    {
        VertexShader = compile vs_3_0 SpriteVertexShader();
        PixelShader = compile ps_3_0 SubtractPS();
    }
}