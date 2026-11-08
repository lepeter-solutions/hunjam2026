Shader "HunJam/Manhattan Landscape"
{
    Properties
    {
        _HazeColor("Distant haze", Color) = (0.49,0.53,0.58,1)
        _HazeDistance("Haze distance", Float) = 2400
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _HazeColor;
                float _HazeDistance;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; half4 color:COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; half3 normalWS:TEXCOORD1; half4 color:COLOR; UNITY_VERTEX_OUTPUT_STEREO };
            Varyings Vert(Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.color = v.color;
                return o;
            }
            half4 Frag(Varyings i):SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                Light sun = GetMainLight();
                half diffuse = saturate(dot(normalize(i.normalWS), sun.direction));
                half3 color = i.color.rgb * (half3(0.48,0.52,0.59) + sun.color * diffuse * 0.6);
                // Distance atmosphere for background relief, independent of the dense gameplay fog.
                float d = distance(GetCameraPositionWS(), i.positionWS);
                half haze = 1.0 - exp(-d / max(1.0, _HazeDistance));
                color = lerp(color, _HazeColor.rgb, haze);
                return half4(color,1);
            }
            ENDHLSL
        }
    }
}
