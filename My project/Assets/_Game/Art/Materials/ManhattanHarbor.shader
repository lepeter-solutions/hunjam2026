Shader "HunJam/Manhattan Harbor"
{
    Properties { _WaterColor("Water", Color) = (0.055,0.115,0.15,1) }
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
                half4 _WaterColor;
            CBUFFER_END
            struct A { float4 positionOS:POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct V { float4 positionCS:SV_POSITION; float3 world:TEXCOORD0; UNITY_VERTEX_OUTPUT_STEREO };
            V Vert(A v)
            {
                V o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.world = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.world);
                return o;
            }
            half4 Frag(V i):SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float2 p = i.world.xz;
                float t = _Time.y;
                float d = distance(GetCameraPositionWS(),i.world);
                float ripple = exp(-d/600.0);
                half3 n = normalize(half3(
                    0.04*cos(dot(p,float2(.15,.09))+t*.7)+.035*ripple*cos(p.x*1.7+t),
                    1,
                    .055*cos(dot(p,float2(-.08,.21))+t*.55)));
                half3 v = normalize(GetCameraPositionWS()-i.world);
                half fresnel = pow(1-saturate(dot(n,v)),4);
                Light sun = GetMainLight();
                half specular = pow(saturate(dot(n,normalize(v+sun.direction))),96);
                half3 color = lerp(_WaterColor.rgb,half3(.38,.46,.55),fresnel*.8);
                color += sun.color * specular * .45;
                color = lerp(color,half3(.49,.53,.58),1-exp(-d/3500));
                return half4(color,1);
            }
            ENDHLSL
        }
    }
}
