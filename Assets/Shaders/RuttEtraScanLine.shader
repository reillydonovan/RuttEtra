Shader "RuttEtra/ScanLine"
{
    Properties
    {
        _LineWidth ("Line Width", Range(0.001, 0.1)) = 0.01
        _GlowIntensity ("Glow Intensity", Range(0, 3)) = 0.5
        _NoiseAmount ("Noise Amount", Range(0, 1)) = 0
        _BaseColor ("Base Color", Color) = (0, 1, 0.5, 1)
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        Blend SrcAlpha One
        ZWrite Off
        Cull Off
        
        Pass
        {
            Name "RuttEtraScanLine"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float3 worldPos : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            CBUFFER_START(UnityPerMaterial)
                float _LineWidth;
                float _GlowIntensity;
                float _NoiseAmount;
                float4 _BaseColor;
            CBUFFER_END
            
            // Simple noise function
            float hash(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.worldPos = TransformObjectToWorld(input.positionOS.xyz);
                output.color = input.color;
                
                return output;
            }
            
            float4 frag(Varyings input) : SV_Target
            {
                float4 col = input.color;
                
                // Add noise jitter
                if (_NoiseAmount > 0)
                {
                    float noise = hash(input.worldPos.xy + _Time.y) * 2.0 - 1.0;
                    col.rgb += noise * _NoiseAmount;
                }
                
                // Apply glow (additive blend handles the bloom effect)
                col.rgb *= (1.0 + _GlowIntensity);
                
                // Ensure minimum alpha for visibility
                col.a = saturate(col.a * 0.8 + 0.2);
                
                return col;
            }
            ENDHLSL
        }
    }
    
    Fallback "Sprites/Default"
}




