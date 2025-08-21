#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"

struct Data
{
    half3 albedo;
    half3 emission;
    half  alpha;
};

half4 UniversalFragmentUnlit(half3 color, half alpha)
{
    Data surfaceData;

    surfaceData.albedo = color;
    surfaceData.alpha = alpha;
    surfaceData.emission = 0;

    half3 albedo = surfaceData.albedo;
    half4 finalColor = half4(albedo + surfaceData.emission, surfaceData.alpha);

    return finalColor;
}


PackedVaryings vert(Attributes input)
{
    Varyings output = (Varyings)0;
    output = BuildVaryings(input);
    PackedVaryings packedOutput = PackVaryings(output);
    return packedOutput;
}

void frag(PackedVaryings packedInput, out half4 outColor : SV_Target0)
{
    Varyings unpacked = UnpackVaryings(packedInput);
    UNITY_SETUP_INSTANCE_ID(unpacked);
    SurfaceDescription surfaceDescription = BuildSurfaceDescription(unpacked);

#if defined(_SURFACE_TYPE_TRANSPARENT)
    bool isTransparent = true;
#else
    bool isTransparent = false;
#endif

#if defined(_ALPHATEST_ON)
    half alpha = AlphaDiscard(surfaceDescription.Alpha, surfaceDescription.AlphaClipThreshold);
#elif defined(_SURFACE_TYPE_TRANSPARENT)
    half alpha = surfaceDescription.Alpha;
#else
    half alpha = half(1.0);
#endif


#if defined(_ALPHAMODULATE_ON)
    surfaceDescription.BaseColor = AlphaModulate(surfaceDescription.BaseColor, alpha);
#endif
    
    Data inputData = (Data)0;
    half4 finalColor = UniversalFragmentUnlit(surfaceDescription.BaseColor, alpha);
    finalColor.a = OutputAlpha(finalColor.a, isTransparent);

  
    outColor = finalColor;
}
