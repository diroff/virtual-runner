#ifndef VIRTUAL_RUNNER_WORLD_CURVATURE_INCLUDED
#define VIRTUAL_RUNNER_WORLD_CURVATURE_INCLUDED

float4 _VRWorldCurveOriginWS;
float4 _VRWorldCurveForwardWS;
float4 _VRWorldCurveRightWS;

float _VRWorldCurveStrength;
float _VRWorldCurveLateralStrength;
float _VRWorldCurveBackwardStrength;

float3 VRWorldCurveForward()
{
    float3 forwardWS = _VRWorldCurveForwardWS.xyz;
    float lengthSquared = dot(forwardWS, forwardWS);

    return lengthSquared > 0.000001
        ? forwardWS * rsqrt(lengthSquared)
        : float3(0.0, 0.0, 1.0);
}

float3 VRWorldCurveRight()
{
    float3 rightWS = _VRWorldCurveRightWS.xyz;
    float lengthSquared = dot(rightWS, rightWS);

    return lengthSquared > 0.000001
        ? rightWS * rsqrt(lengthSquared)
        : float3(1.0, 0.0, 0.0);
}

float VRWorldCurveDepth(float3 positionWS)
{
    float3 forwardWS = VRWorldCurveForward();

    return max(
        0.0,
        dot(positionWS - _VRWorldCurveOriginWS.xyz, forwardWS));
}

float VRWorldCurveBackwardDepth(float3 positionWS)
{
    float3 forwardWS = VRWorldCurveForward();

    return max(
        0.0,
        -dot(positionWS - _VRWorldCurveOriginWS.xyz, forwardWS));
}

float VRWorldCurveLateral(float3 positionWS)
{
    float3 rightWS = VRWorldCurveRight();

    return dot(positionWS - _VRWorldCurveOriginWS.xyz, rightWS);
}

float3 VRApplyWorldCurve(float3 positionWS)
{
    float forwardDepth = VRWorldCurveDepth(positionWS);
    float backwardDepth = VRWorldCurveBackwardDepth(positionWS);
    float lateral = VRWorldCurveLateral(positionWS);

    float forwardOffset =
        _VRWorldCurveStrength * forwardDepth * forwardDepth;

    float backwardOffset =
        _VRWorldCurveBackwardStrength * backwardDepth * backwardDepth;

    float lateralOffset =
        _VRWorldCurveLateralStrength * lateral * lateral;

    positionWS.y -= forwardOffset + backwardOffset + lateralOffset;

    return positionWS;
}

float3 VRApplyWorldCurveNormal(float3 originalPositionWS, float3 normalWS)
{
    float forwardDepth = VRWorldCurveDepth(originalPositionWS);
    float backwardDepth = VRWorldCurveBackwardDepth(originalPositionWS);
    float lateral = VRWorldCurveLateral(originalPositionWS);

    float3 forwardWS = VRWorldCurveForward();
    float3 rightWS = VRWorldCurveRight();

    normalWS += forwardWS *
        (2.0 * _VRWorldCurveStrength * forwardDepth * normalWS.y);

    normalWS -= forwardWS *
        (2.0 * _VRWorldCurveBackwardStrength * backwardDepth * normalWS.y);

    normalWS += rightWS *
        (2.0 * _VRWorldCurveLateralStrength * lateral * normalWS.y);

    return normalize(normalWS);
}

float3 VRApplyWorldCurveTangent(float3 originalPositionWS, float3 tangentWS)
{
    float forwardDepth = VRWorldCurveDepth(originalPositionWS);
    float backwardDepth = VRWorldCurveBackwardDepth(originalPositionWS);
    float lateral = VRWorldCurveLateral(originalPositionWS);

    float3 forwardWS = VRWorldCurveForward();
    float3 rightWS = VRWorldCurveRight();

    float forwardCorrection =
        2.0 * _VRWorldCurveStrength * forwardDepth
        * dot(forwardWS, tangentWS);

    float backwardCorrection =
        2.0 * _VRWorldCurveBackwardStrength * backwardDepth
        * dot(forwardWS, tangentWS);

    float lateralCorrection =
        2.0 * _VRWorldCurveLateralStrength * lateral
        * dot(rightWS, tangentWS);

    tangentWS -= float3(
        0.0,
        1.0,
        0.0) * (forwardCorrection - backwardCorrection + lateralCorrection);

    return normalize(tangentWS);
}

#endif
