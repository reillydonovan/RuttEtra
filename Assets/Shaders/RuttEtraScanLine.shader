Shader "RuttEtra/ScanLine"
{
    Properties
    {
        _LineWidth ("Line Width", Range(0.001, 0.1)) = 0.01
        _GlowIntensity ("Glow Intensity", Range(0, 3)) = 0.5
        _NoiseAmount ("Noise Amount", Range(0, 1)) = 0
        _LineTaper ("Line Taper", Range(0, 1)) = 0
        _BaseColor ("Base Color", Color) = (0, 1, 0.5, 1)
        _BackgroundColor ("Background Color", Color) = (0, 0, 0, 1)
        _Bloom ("Bloom", Range(0, 1)) = 0
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
            #pragma geometry geom
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma target 4.0
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2g
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float depth : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float3 worldPos : TEXCOORD0;
                float depth : TEXCOORD1;
                float2 uv : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            CBUFFER_START(UnityPerMaterial)
                float _LineWidth;
                float _GlowIntensity;
                float _NoiseAmount;
                float _LineTaper;
                float4 _BaseColor;
                float4 _BackgroundColor;
                float _Bloom;
            CBUFFER_END
            
            float hash(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }
            
            v2g vert(Attributes input)
            {
                v2g output;
                UNITY_SETUP_INSTANCE_ID(input);
                output.positionOS = input.positionOS;
                output.color = input.color;
                output.depth = input.positionOS.z;
                return output;
            }
            
            [maxvertexcount(4)]
            void geom(line v2g input[2], inout TriangleStream<Varyings> outputStream)
            {
                float4 p0 = input[0].positionOS;
                float4 p1 = input[1].positionOS;
                
                float4 clip0 = TransformObjectToHClip(p0.xyz);
                float4 clip1 = TransformObjectToHClip(p1.xyz);
                
                float2 screen0 = clip0.xy / clip0.w;
                float2 screen1 = clip1.xy / clip1.w;
                
                float2 dir = normalize(screen1 - screen0);
                float2 normal = float2(-dir.y, dir.x);
                
                float width = _LineWidth * 0.5;
                
                // Depth-based width variation
                float avgDepth = (input[0].depth + input[1].depth) * 0.5;
                float depthWidth = lerp(width, width * 0.3, saturate(avgDepth * 0.2) * _LineTaper);
                
                float2 offset = normal * depthWidth;
                
                Varyings v;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(v);
                
                // Vertex 0 - bottom left
                v.positionCS = float4((screen0 - offset) * clip0.w, clip0.z, clip0.w);
                v.worldPos = TransformObjectToWorld(p0.xyz);
                v.color = input[0].color;
                v.depth = input[0].depth;
                v.uv = float2(0, 0);
                outputStream.Append(v);
                
                // Vertex 1 - top left
                v.positionCS = float4((screen0 + offset) * clip0.w, clip0.z, clip0.w);
                v.uv = float2(0, 1);
                outputStream.Append(v);
                
                // Vertex 2 - bottom right
                v.positionCS = float4((screen1 - offset) * clip1.w, clip1.z, clip1.w);
                v.worldPos = TransformObjectToWorld(p1.xyz);
                v.color = input[1].color;
                v.depth = input[1].depth;
                v.uv = float2(1, 0);
                outputStream.Append(v);
                
                // Vertex 3 - top right
                v.positionCS = float4((screen1 + offset) * clip1.w, clip1.z, clip1.w);
                v.uv = float2(1, 1);
                outputStream.Append(v);
                
                outputStream.RestartStrip();
            }
            
            float4 frag(Varyings input) : SV_Target
            {
                float4 col = input.color;
                
                // Soft edge falloff
                float edge = abs(input.uv.y - 0.5) * 2.0;
                float softness = 1.0 - smoothstep(0.5, 1.0, edge);
                
                // Depth-based intensity
                float depthFactor = saturate(1.0 - input.depth * 0.1);
                
                // Noise
                if (_NoiseAmount > 0)
                {
                    float noise = hash(input.worldPos.xy + _Time.y * 10.0) * 2.0 - 1.0;
                    col.rgb += noise * _NoiseAmount;
                }
                
                // Glow
                float glow = (1.0 + _GlowIntensity * depthFactor);
                col.rgb *= glow;
                
                // Bloom effect
                if (_Bloom > 0)
                {
                    float bloom = pow(softness, 0.5) * _Bloom;
                    col.rgb += col.rgb * bloom;
                }
                
                // Taper
                if (_LineTaper > 0)
                {
                    col.rgb *= lerp(1.0, depthFactor, _LineTaper);
                }
                
                col.a *= softness;
                col.a = saturate(col.a * 0.9 + 0.1);
                
                return col;
            }
            ENDHLSL
        }
    }
    
    Fallback "Sprites/Default"
}
