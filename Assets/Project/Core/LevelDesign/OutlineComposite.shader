Shader "Map/OutlineComposite"
{
    Properties
    {
        _MainTex ("Base Map", 2D) = "white" {}
        _MaskTex ("Mask Map", 2D) = "black" {}
        _CoverTex ("Cover Map (Front)", 2D) = "black" {} // 🌟 추가됨
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineThickness ("Outline Thickness", Float) = 2.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "MapOutlinePass_URP"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_MaskTex); SAMPLER(sampler_MaskTex);
            TEXTURE2D(_CoverTex); SAMPLER(sampler_CoverTex); // 🌟 추가됨

            CBUFFER_START(UnityPerMaterial)
                float4 _PixelSize;
                float4 _OutlineColor;
                float _OutlineThickness;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                float centerMask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, input.uv).r;
                float coverMask = SAMPLE_TEXTURE2D(_CoverTex, sampler_CoverTex, input.uv).r; // 🌟 커버 픽셀 확인

                float2 offset = _PixelSize.xy * _OutlineThickness;
                float edge = 0;
                edge += SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, input.uv + float2(offset.x, 0)).r;
                edge += SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, input.uv - float2(offset.x, 0)).r;
                edge += SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, input.uv + float2(0, offset.y)).r;
                edge += SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, input.uv - float2(0, offset.y)).r;
                edge += SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, input.uv + float2(offset.x, offset.y)).r;
                edge += SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, input.uv - float2(offset.x, -offset.y)).r;
                edge += SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, input.uv + float2(-offset.x, offset.y)).r;
                edge += SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, input.uv - float2(-offset.x, -offset.y)).r;

                // 🌟 핵심: 외곽선 판정이 났더라도, coverMask가 존재(1)하면 외곽선을 0으로 날려버림!
                float isOutline = step(0.1, edge) * step(centerMask, 0.5) * (1.0 - step(0.5, coverMask));

                return lerp(baseColor, _OutlineColor, isOutline * _OutlineColor.a);
            }
            ENDHLSL
        }
    }
}