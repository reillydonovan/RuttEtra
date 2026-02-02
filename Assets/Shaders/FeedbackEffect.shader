Shader "Hidden/RuttEtra/Feedback"
{
    Properties
    {
        _MainTex ("Current Frame", 2D) = "white" {}
        _FeedbackTex ("Feedback Buffer", 2D) = "black" {}
        _FeedbackAmount ("Feedback Amount", Range(0, 1)) = 0.85
        _Zoom ("Zoom", Range(0.9, 1.1)) = 1.01
        _Rotation ("Rotation", Float) = 0
        _Offset ("Offset", Vector) = (0, 0, 0, 0)
        _HueShift ("Hue Shift", Range(0.9, 1.1)) = 1
        _Saturation ("Saturation", Range(0.9, 1.1)) = 1
        _Brightness ("Brightness", Range(0.9, 1.1)) = 0.98
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_FeedbackTex);
            SAMPLER(sampler_FeedbackTex);
            
            float _FeedbackAmount;
            float _Zoom;
            float _Rotation;
            float4 _Offset;
            float _HueShift;
            float _Saturation;
            float _Brightness;
            
            float3 rgb2hsv(float3 c)
            {
                float4 K = float4(0.0, -1.0/3.0, 2.0/3.0, -1.0);
                float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
                float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }
            
            float3 hsv2rgb(float3 c)
            {
                float4 K = float4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }
            
            float4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                
                // Sample current frame
                float4 current = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                
                // Transform UV for feedback sampling
                float2 center = float2(0.5, 0.5);
                float2 feedbackUV = uv - center;
                
                // Apply rotation
                float c = cos(_Rotation);
                float s = sin(_Rotation);
                feedbackUV = float2(
                    feedbackUV.x * c - feedbackUV.y * s,
                    feedbackUV.x * s + feedbackUV.y * c
                );
                
                // Apply zoom
                feedbackUV /= _Zoom;
                
                // Apply offset
                feedbackUV += center + _Offset.xy;
                
                // Sample feedback with transformed UV
                float4 feedback = SAMPLE_TEXTURE2D(_FeedbackTex, sampler_FeedbackTex, feedbackUV);
                
                // Apply color modifications to feedback
                float3 hsv = rgb2hsv(feedback.rgb);
                hsv.x = frac(hsv.x * _HueShift);
                hsv.y *= _Saturation;
                hsv.z *= _Brightness;
                feedback.rgb = hsv2rgb(hsv);
                
                // Blend current with feedback
                float4 result = lerp(current, feedback, _FeedbackAmount);
                
                // Add current frame on top (always visible)
                float currentBrightness = dot(current.rgb, float3(0.299, 0.587, 0.114));
                result = lerp(result, current, currentBrightness * 0.5);
                
                return result;
            }
            ENDHLSL
        }
    }
    
    Fallback Off
}
