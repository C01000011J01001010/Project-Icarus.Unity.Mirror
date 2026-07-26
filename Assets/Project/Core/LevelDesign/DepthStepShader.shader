Shader "Map/DepthStepShader"
{
    Properties
    {
        _BakeColor ("Bake Color", Color) = (1,1,1,1)
        _Steps ("Steps", Float) = 8.0
    }

    // ========================================================
    // [SubShader 1] URP (Universal Render Pipeline)
    // ========================================================
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100

        Pass
        {
            Name "MapBakePass_URP"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float depth : TEXCOORD0; };

            CBUFFER_START(UnityPerMaterial)
                float4 _BakeColor;
                float _Steps;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;

                float3 positionVS = TransformWorldToView(vertexInput.positionWS);
                output.depth = -positionVS.z / _ProjectionParams.z; // 0~1 정규화
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Steps가 0 이하(None)인 경우: Depth 명도 양자화 스킵 (Pure Color)
                if (_Steps <= 0.5)
                {
                    return _BakeColor;
                }

                float clampedDepth = saturate(1.0 - input.depth);
                float steps = max(2.0, _Steps);
                float steppedDepth = floor(clampedDepth * steps) / (steps - 1.0);

                return _BakeColor * steppedDepth;
            }
            ENDHLSL
        }
    }

    // ========================================================
    // [SubShader 2] Built-In Render Pipeline (Fallback)
    // ========================================================
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            Name "MapBakePass_BuiltIn"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 vertex : SV_POSITION; float depth : TEXCOORD0; };

            float4 _BakeColor;
            float _Steps;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.depth = -(mul(UNITY_MATRIX_MV, v.vertex).z) * _ProjectionParams.w;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                if (_Steps <= 0.5)
                {
                    return _BakeColor;
                }

                float clampedDepth = saturate(1.0 - i.depth);
                float steps = max(2.0, _Steps);
                float steppedDepth = floor(clampedDepth * steps) / (steps - 1.0);

                return _BakeColor * steppedDepth;
            }
            ENDCG
        }
    }
}