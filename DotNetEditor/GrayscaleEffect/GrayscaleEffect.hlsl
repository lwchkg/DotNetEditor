// Grayscale shader 

sampler2D implicitInput : register(s0);

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float4 color = tex2D(implicitInput, uv);
    float gray = color.r * 0.3 + color.g * 0.59 + color.b * 0.11;

    float4 result;
    result.r = gray;
    result.g = gray;
    result.b = gray;
    result.a = color.a;

    return result;
}
