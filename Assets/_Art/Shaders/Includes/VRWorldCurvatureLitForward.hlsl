#ifndef VIRTUAL_RUNNER_WORLD_CURVATURE_LIT_FORWARD_INCLUDED
#define VIRTUAL_RUNNER_WORLD_CURVATURE_LIT_FORWARD_INCLUDED

struct CurvedVaryings
        {
            half4 color : COLOR;
            float2 uv : TEXCOORD0;
            float3 positionWS : TEXCOORD1;
            half3 normalWS : TEXCOORD2;
            half4 tangentWS : TEXCOORD3;
            half4 fogFactorAndVertexLight : TEXCOORD5;
            DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 8);
            #ifdef DYNAMICLIGHTMAP_ON
                float2 dynamicLightmapUV : TEXCOORD9;
            #endif
            float4 positionCS : SV_POSITION;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };

        CurvedVaryings VRWorldCurveVertex(CurvedAttributes input)
        {
            CurvedVaryings output = (CurvedVaryings)0;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_TRANSFER_INSTANCE_ID(input, output);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
            VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);

            float3 originalPositionWS = positionInputs.positionWS;
            float3 positionWS = VRApplyWorldCurve(originalPositionWS);
            float3 normalWS = VRApplyWorldCurveNormal(originalPositionWS, normalInputs.normalWS);

            output.positionWS = positionWS;
            output.positionCS = TransformWorldToHClip(positionWS);
            output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
            output.color = input.color;
            output.normalWS = normalWS;
            #if defined(_NORMALMAP)
                float3 tangentWS = VRApplyWorldCurveTangent(originalPositionWS, normalInputs.tangentWS);
                output.tangentWS = half4(tangentWS, input.tangentOS.w * GetOddNegativeScale());
            #else
                output.tangentWS = half4(1.0h, 0.0h, 0.0h, input.tangentOS.w * GetOddNegativeScale());
            #endif

            half3 vertexLight = 0.0h;
            #if defined(_ADDITIONAL_LIGHTS_VERTEX)
                vertexLight = VertexLighting(positionWS, normalWS);
            #endif
            half fogFactor = ComputeFogFactor(output.positionCS.z);
            output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

            OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
            #ifdef DYNAMICLIGHTMAP_ON
                output.dynamicLightmapUV = input.dynamicLightmapUV * unity_DynamiclightmapST.xy + unity_DynamiclightmapST.zw;
            #endif
            OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

            return output;
        }

        void VRInitializeInputData(CurvedVaryings input, half3 normalTS, out InputData inputData)
        {
            inputData = (InputData)0;
            inputData.positionWS = input.positionWS;
            inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);

            #if defined(_NORMALMAP)
                half3 tangentWS = normalize(input.tangentWS.xyz);
                half3 bitangentWS = input.tangentWS.w * cross(input.normalWS, tangentWS);
                half3x3 tangentToWorld = half3x3(tangentWS, bitangentWS, input.normalWS);
                inputData.normalWS = TransformTangentToWorld(normalTS, tangentToWorld);
            #else
                inputData.normalWS = input.normalWS;
            #endif

            inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
            inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
            inputData.fogCoord = input.fogFactorAndVertexLight.x;
            inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
            inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);

            #if defined(DYNAMICLIGHTMAP_ON)
                inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.dynamicLightmapUV, input.vertexSH, inputData.normalWS);
            #else
                inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
            #endif
            inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);
        }

        half4 VRWorldCurveFragment(CurvedVaryings input) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

            half4 albedoAlpha = SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
            half4 blendedColor = VRMixVertexColor(albedoAlpha * _BaseColor, input.color);
            SurfaceData surfaceData = (SurfaceData)0;
            surfaceData.alpha = Alpha(blendedColor.a, half4(1.0h, 1.0h, 1.0h, 1.0h), _Cutoff);
            surfaceData.albedo = blendedColor.rgb;
            surfaceData.specular = half3(0.0, 0.0, 0.0);

            #if defined(_METALLICSPECGLOSSMAP)
                half4 metallicGloss = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, input.uv);
                surfaceData.metallic = metallicGloss.r;
                surfaceData.smoothness = metallicGloss.a * _Smoothness;
            #else
                surfaceData.metallic = _Metallic;
                surfaceData.smoothness = _Smoothness;
            #endif

            surfaceData.normalTS = SampleNormal(input.uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);

            #if defined(_OCCLUSIONMAP)
                half occlusion = SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, input.uv).g;
                surfaceData.occlusion = lerp(1.0h, occlusion, _OcclusionStrength);
            #else
                surfaceData.occlusion = 1.0h;
            #endif

            surfaceData.emission = SampleEmission(input.uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));

            #if defined(_PRESERVE_SPECULAR_LIGHTING)
                surfaceData.albedo *= surfaceData.alpha;
                surfaceData.emission *= surfaceData.alpha;
            #endif

            #if defined(_ALPHAMODULATE_ON)
                surfaceData.albedo = lerp(half3(1.0h, 1.0h, 1.0h), surfaceData.albedo, surfaceData.alpha);
            #endif

            surfaceData.clearCoatMask = 0.0h;
            surfaceData.clearCoatSmoothness = 0.0h;

            InputData inputData;
            VRInitializeInputData(input, surfaceData.normalTS, inputData);

            half4 color = UniversalFragmentPBR(inputData, surfaceData);
            color.rgb = MixFog(color.rgb, inputData.fogCoord);

            #if defined(_ALPHAPREMULTIPLY_ON)
                color.rgb *= color.a;
            #endif

            color.a = OutputAlpha(color.a, _Surface > 0.5h);
            return color;
        }

#endif
