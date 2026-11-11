#ifndef VIRTUAL_RUNNER_WORLD_CURVATURE_LIT_SHADOW_INCLUDED
#define VIRTUAL_RUNNER_WORLD_CURVATURE_LIT_SHADOW_INCLUDED

float3 _LightDirection;
            float3 _LightPosition;

            struct CurvedShadowVaryings
            {
                #if defined(_ALPHATEST_ON)
                    float2 uv : TEXCOORD0;
                    half alpha : TEXCOORD1;
                #endif
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CurvedShadowVaryings VRWorldCurveShadowVertex(CurvedAttributes input)
            {
                CurvedShadowVaryings output = (CurvedShadowVaryings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                #if defined(_ALPHATEST_ON)
                    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                    output.alpha = input.color.a;
                #endif

                float3 originalPositionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 originalNormalWS = TransformObjectToWorldNormal(input.normalOS);
                float3 positionWS = VRApplyWorldCurve(originalPositionWS);
                float3 normalWS = VRApplyWorldCurveNormal(originalPositionWS, originalNormalWS);
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
                output.positionCS = ApplyShadowClamping(positionCS);
                return output;
            }

            half4 VRWorldCurveShadowFragment(CurvedShadowVaryings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                #if defined(_CAST_SHADOWS_OFF)
                    discard;
                #endif
                #if defined(_ALPHATEST_ON)
                    Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a * input.alpha, _BaseColor, _Cutoff);
                #endif
                return 0;
            }

#endif
