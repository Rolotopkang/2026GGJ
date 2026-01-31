Shader "PostProcess/CRTEffect"
{
    Properties
    {
        [Header(Scanlines)]
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.25
        _ScanlineCount ("Scanline Count", Float) = 350
        [Header(Vignette)]
        _VignetteStrength ("Vignette Strength", Range(0, 1)) = 0.4
        _VignetteSoftness ("Vignette Softness", Range(0.01, 1)) = 0.6
        [Header(Curvature)]
        _Curvature ("Screen Curvature", Range(0, 0.02)) = 0.008
        [Header(Chromatic)]
        _ChromaticOffset ("Chromatic Aberration", Range(0, 0.005)) = 0.001
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZTest Always
        ZWrite Off
        Cull Off

        Pass
        {
            Name "CRT"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GlobalSamplers.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D_X(_BlitTexture);
            float4 _BlitScaleBias;

            float _ScanlineIntensity;
            float _ScanlineCount;
            float _VignetteStrength;
            float _VignetteSoftness;
            float _Curvature;
            float _ChromaticOffset;

            struct Attributes
            {
                uint vertexID : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
                float2 uv = GetFullScreenTriangleTexCoord(input.vertexID);
                output.positionCS = pos;
                output.texcoord = uv * _BlitScaleBias.xy + _BlitScaleBias.zw;
                return output;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;

                // 屏幕弯曲（可选）
                if (_Curvature > 0)
                {
                    float2 cc = uv - 0.5;
                    float dist = dot(cc, cc);
                    uv = uv + cc * (dist * _Curvature);
                }

                // 色差（红/蓝轻微偏移）
                float4 col;
                if (_ChromaticOffset > 0)
                {
                    float2 off = (uv - 0.5) * _ChromaticOffset;
                    col.r = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + off, 0).r;
                    col.g = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, 0).g;
                    col.b = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv - off, 0).b;
                    col.a = 1;
                }
                else
                {
                    col = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, 0);
                }

                // 扫描线
                float scanline = sin(uv.y * _ScanlineCount * 6.28318) * 0.5 + 0.5;
                scanline = 1.0 - scanline * _ScanlineIntensity;
                col.rgb *= scanline;

                // 暗角
                float2 vuv = uv * 2.0 - 1.0;
                float vignette = 1.0 - pow(saturate(length(vuv) * _VignetteSoftness), 2.0) * _VignetteStrength;
                col.rgb *= vignette;

                return col;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
