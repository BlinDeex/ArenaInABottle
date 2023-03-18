sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
texture uImage2;
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uSaturation;
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
float2 uTargetPosition;
float4 uLegacyArmorSourceRect;
float2 uLegacyArmorSheetSize;
float4 uShaderSpecificData;

texture sampleTexture;
float noiseScalar;
float2 screenSize;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float4 position : SV_POSITION, float4 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    coords.y += uTime / 40;
    float4 noise = tex2D(samplerTex, coords / noiseScalar);
    noise.rgb = clamp(noise.rgb, 0.4, 1);
    return color * noise * sampleColor * color.a;
}

technique Technique1
{
    pass NoisePass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}