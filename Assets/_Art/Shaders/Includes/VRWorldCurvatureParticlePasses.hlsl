#ifndef VIRTUAL_RUNNER_PARTICLE_CURVATURE_PASSES_INCLUDED
#define VIRTUAL_RUNNER_PARTICLE_CURVATURE_PASSES_INCLUDED

#include "VRWorldCurvature.hlsl"

// Keep URP's stock particle vertex functions for streams, flipbook and instancing;
// replace only the outputs derived from the particle's world-space position.

#if defined(VR_PARTICLE_FORWARD_PASS)
VaryingsParticle VRParticleCurvedForwardVertex(AttributesParticle input)
{
    VaryingsParticle output = vertParticleUnlit(input);
    UNITY_SETUP_INSTANCE_ID(input);

    float3 originalPositionWS = GetVertexPositionInputs(input.positionOS.xyz).positionWS;
    float3 curvedPositionWS = VRApplyWorldCurve(originalPositionWS);

    output.positionWS.xyz = curvedPositionWS;
    output.clipPos = TransformWorldToHClip(curvedPositionWS);

    #if !defined(_FOG_FRAGMENT)
    output.positionWS.w = ComputeFogFactor(output.clipPos.z);
    #endif

    #if defined(_SOFTPARTICLES_ON) || defined(_FADING_ON) || defined(_DISTORTION_ON)
    output.projectedPosition = ComputeScreenPos(output.clipPos);
    #endif

    #if defined(_NORMALMAP)
    output.normalWS.xyz = VRApplyWorldCurveNormal(originalPositionWS, output.normalWS.xyz);
    output.tangentWS.xyz = VRApplyWorldCurveTangent(originalPositionWS, output.tangentWS.xyz);
    output.bitangentWS.xyz = VRApplyWorldCurveTangent(originalPositionWS, output.bitangentWS.xyz);

    half3 viewDirectionWS = GetWorldSpaceNormalizeViewDir(curvedPositionWS);
    output.normalWS.w = viewDirectionWS.x;
    output.tangentWS.w = viewDirectionWS.y;
    output.bitangentWS.w = viewDirectionWS.z;
    #else
    output.normalWS = VRApplyWorldCurveNormal(originalPositionWS, output.normalWS);
    output.viewDirWS = GetWorldSpaceNormalizeViewDir(curvedPositionWS);
    #endif

    return output;
}
#endif

#if defined(VR_PARTICLE_DEPTH_PASS)
VaryingsDepthOnlyParticle VRParticleCurvedDepthVertex(AttributesDepthOnlyParticle input)
{
    VaryingsDepthOnlyParticle output = DepthOnlyVertex(input);
    UNITY_SETUP_INSTANCE_ID(input);

    float3 originalPositionWS = GetVertexPositionInputs(input.vertex.xyz).positionWS;
    output.clipPos = TransformWorldToHClip(VRApplyWorldCurve(originalPositionWS));
    return output;
}
#endif

#if defined(VR_PARTICLE_DEPTH_NORMALS_PASS)
VaryingsDepthNormalsParticle VRParticleCurvedDepthNormalsVertex(AttributesDepthNormalsParticle input)
{
    VaryingsDepthNormalsParticle output = DepthNormalsVertex(input);
    UNITY_SETUP_INSTANCE_ID(input);

    float3 originalPositionWS = GetVertexPositionInputs(input.vertex.xyz).positionWS;
    float3 curvedPositionWS = VRApplyWorldCurve(originalPositionWS);
    output.clipPos = TransformWorldToHClip(curvedPositionWS);

    #if defined(_NORMALMAP)
    output.normalWS.xyz = VRApplyWorldCurveNormal(originalPositionWS, output.normalWS.xyz);
    output.tangentWS.xyz = VRApplyWorldCurveTangent(originalPositionWS, output.tangentWS.xyz);
    output.bitangentWS.xyz = VRApplyWorldCurveTangent(originalPositionWS, output.bitangentWS.xyz);

    half3 viewDirectionWS = GetWorldSpaceNormalizeViewDir(curvedPositionWS);
    output.normalWS.w = viewDirectionWS.x;
    output.tangentWS.w = viewDirectionWS.y;
    output.bitangentWS.w = viewDirectionWS.z;
    #else
    output.normalWS = VRApplyWorldCurveNormal(originalPositionWS, output.normalWS);
    output.viewDirWS = GetWorldSpaceNormalizeViewDir(curvedPositionWS);
    #endif

    return output;
}
#endif

#if defined(VR_PARTICLE_EDITOR_PASS)
VaryingsParticle VRParticleCurvedEditorVertex(AttributesParticle input)
{
    VaryingsParticle output = vertParticleEditor(input);
    UNITY_SETUP_INSTANCE_ID(input);

    float3 originalPositionWS = GetVertexPositionInputs(input.positionOS.xyz).positionWS;
    output.clipPos = TransformWorldToHClip(VRApplyWorldCurve(originalPositionWS));
    return output;
}
#endif

#endif
