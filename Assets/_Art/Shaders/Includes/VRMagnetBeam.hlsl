#ifndef VIRTUAL_RUNNER_MAGNET_BEAM_INCLUDED
#define VIRTUAL_RUNNER_MAGNET_BEAM_INCLUDED

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
    half4 _EmissionColor;
    half _EmissionIntensity;
    half _Opacity;
    half _MappingMode;
    half _FlowSpeed;
    half _FlowFrequency;
    half _FlowSharpness;
    half _FlowStrength;
    half _StreamCount;
    half _StreamTwist;
    half _SwirlSpeed;
    half _StreamSharpness;
    half _StreamStrength;
    half _RimPower;
    half _RimStrength;
    half _SparkleDensity;
    half _SparkleSpeed;
    half _SparkleStrength;
    half _NearFade;
    half _Premultiply;
CBUFFER_END

struct VRMagnetBeamAttributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VRMagnetBeamVaryings
{
    float4 positionCS : SV_POSITION;
    float3 positionOS : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    half3 normalWS : TEXCOORD2;
    float2 uv : TEXCOORD3;
    half fogFactor : TEXCOORD4;
    UNITY_VERTEX_OUTPUT_STEREO
};

VRMagnetBeamVaryings VRMagnetBeamVertex(VRMagnetBeamAttributes input)
{
    VRMagnetBeamVaryings output = (VRMagnetBeamVaryings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    float3 originalPositionWS = TransformObjectToWorld(input.positionOS.xyz);
    float3 originalNormalWS = TransformObjectToWorldNormal(input.normalOS);
    float3 curvedPositionWS = VRApplyWorldCurve(originalPositionWS);

    output.positionOS = input.positionOS.xyz;
    output.positionWS = curvedPositionWS;
    output.normalWS = VRApplyWorldCurveNormal(originalPositionWS, originalNormalWS);
    output.positionCS = TransformWorldToHClip(curvedPositionWS);
    output.uv = input.uv;
    output.fogFactor = ComputeFogFactor(output.positionCS.z);
    return output;
}

half VRMagnetBeamSparkle(float3 positionOS, float time)
{
    float3 coordinate = positionOS * _SparkleDensity + float3(0.0, 0.0, time * _SparkleSpeed);
    float3 cell = floor(coordinate);
    float3 offset = frac(coordinate) - 0.5;
    float randomValue = frac(sin(dot(cell, float3(12.9898, 78.233, 37.719))) * 43758.5453);
    float selectedCell = step(0.96, randomValue);
    float sparkleShape = 1.0 - smoothstep(0.06, 0.3, length(offset));
    return (half)(selectedCell * sparkleShape);
}

half4 VRMagnetBeamFragment(VRMagnetBeamVaryings input) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    float3 localPosition = input.positionOS;
    float2 mappedUV = lerp(localPosition.xz, input.uv, saturate(_MappingMode));
    mappedUV = mappedUV * _BaseMap_ST.xy + _BaseMap_ST.zw;
    half4 textureColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, mappedUV);

    float beamDepth = max(0.0, localPosition.z);
    float angle = atan2(localPosition.y, localPosition.x);
    float time = _Time.y;

    half lane = pow(saturate(0.5 + 0.5 * sin(angle * round(_StreamCount)
        + beamDepth * _StreamTwist + time * _SwirlSpeed)), _StreamSharpness);
    half wave = pow(saturate(0.5 + 0.5 * sin(beamDepth * _FlowFrequency
        + time * _FlowSpeed)), _FlowSharpness);

    half3 viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
    half3 normalWS = normalize(input.normalWS);
    half rim = pow(1.0h - saturate(abs(dot(normalWS, viewDirectionWS))), _RimPower);
    half sparkle = VRMagnetBeamSparkle(localPosition, time);

    half energy = wave * _FlowStrength + lane * _StreamStrength + rim * _RimStrength
        + sparkle * _SparkleStrength;
    half fade = smoothstep(0.0h, max(_NearFade, 0.01h), length(localPosition));
    half alpha = saturate(textureColor.a * _BaseColor.a * _Opacity * fade * (0.45h + energy));

    half3 baseColor = textureColor.rgb * _BaseColor.rgb;
    half3 emission = _EmissionColor.rgb * _EmissionIntensity * energy;
    half3 color = MixFog(baseColor + emission, input.fogFactor);

    color = lerp(color, color * alpha, saturate(_Premultiply));

    return half4(color, alpha);
}

#endif
