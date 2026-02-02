Shader "RuttEtra/AnalogEffects"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HideInInspector] _BlitTexture ("Blit Texture", 2D) = "white" {}
        
        // Enable flags
        _EnableCRT ("Enable CRT", Float) = 0
        _EnableVHS ("Enable VHS", Float) = 0
        _EnableChromatic ("Enable Chromatic", Float) = 0
        _EnableHoldDrift ("Enable Hold Drift", Float) = 0
        _EnableSignalNoise ("Enable Signal Noise", Float) = 0
        
        // CRT
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.3
        _ScanlineCount ("Scanline Count", Range(100, 1000)) = 300
        _PhosphorGlow ("Phosphor Glow", Range(0, 1)) = 0.2
        _ScreenCurvature ("Screen Curvature", Range(0, 0.5)) = 0.1
        _Vignette ("Vignette", Range(0, 0.5)) = 0.05
        
        // VHS
        _TrackingNoise ("Tracking Noise", Range(0, 1)) = 0.1
        _ColorBleed ("Color Bleed", Range(0, 1)) = 0.2
        _TapeNoise ("Tape Noise", Range(0, 1)) = 0.1
        _HorizontalJitter ("Horizontal Jitter", Range(0, 0.1)) = 0.02
        
        // Chromatic
        _ChromaticAmount ("Chromatic Amount", Range(0, 0.1)) = 0.01
        
        // Hold drift
        _HorizontalHold ("Horizontal Hold", Range(0, 1)) = 0
        _VerticalHold ("Vertical Hold", Range(0, 1)) = 0
        _DriftSpeed ("Drift Speed", Range(0, 5)) = 1
        
        // Noise
        _StaticNoise ("Static Noise", Range(0, 1)) = 0.1
        _SnowAmount ("Snow Amount", Range(0, 1)) = 0
        
        // Color
        _Saturation ("Saturation", Range(0, 2)) = 1
        _HueShift ("Hue Shift", Range(-1, 1)) = 0
        _Gamma ("Gamma", Range(0.5, 2)) = 1
        
        // Time
        _EffectTime ("Effect Time", Float) = 0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        ZWrite Off ZTest Always Cull Off
        
        Pass
        {
            Name "AnalogEffects"
            
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            
            // Additional textures for fallback
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            // Properties
            float _EnableCRT, _EnableVHS, _EnableChromatic, _EnableHoldDrift, _EnableSignalNoise;
            float _ScanlineIntensity, _ScanlineCount, _PhosphorGlow, _ScreenCurvature, _Vignette;
            float _TrackingNoise, _ColorBleed, _TapeNoise, _HorizontalJitter;
            float _ChromaticAmount;
            float _HorizontalHold, _VerticalHold, _DriftSpeed;
            float _StaticNoise, _SnowAmount;
            float _Saturation, _HueShift, _Gamma;
            float _EffectTime;
            
            // Noise functions
            float hash(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }
            
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(hash(i), hash(i + float2(1, 0)), f.x),
                           lerp(hash(i + float2(0, 1)), hash(i + float2(1, 1)), f.x), f.y);
            }
            
            // Color conversion
            float3 rgb2hsv(float3 c)
            {
                float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
                float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }
            
            float3 hsv2rgb(float3 c)
            {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }
            
            // CRT curvature
            float2 curveUV(float2 uv, float amount)
            {
                uv = uv * 2.0 - 1.0;
                float2 offset = abs(uv.yx) / float2(6.0, 4.0) * amount;
                uv = uv + uv * offset * offset;
                uv = uv * 0.5 + 0.5;
                return uv;
            }
            
            float4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float time = _EffectTime > 0 ? _EffectTime : _Time.y;
                
                // Hold drift
                if (_EnableHoldDrift > 0.5)
                {
                    uv.x += _HorizontalHold * sin(time * _DriftSpeed * 2.0 + uv.y * 10.0) * 0.1;
                    uv.y += _VerticalHold * time * _DriftSpeed * 0.1;
                    uv.y = frac(uv.y);
                }
                
                // CRT curvature
                if (_EnableCRT > 0.5 && _ScreenCurvature > 0)
                {
                    uv = curveUV(uv, _ScreenCurvature);
                }
                
                // VHS horizontal jitter
                if (_EnableVHS > 0.5 && _HorizontalJitter > 0)
                {
                    float jitter = (noise(float2(time * 100.0, uv.y * 50.0)) - 0.5) * _HorizontalJitter;
                    uv.x += jitter;
                }
                
                // VHS tracking noise
                if (_EnableVHS > 0.5 && _TrackingNoise > 0)
                {
                    float tracking = step(0.99 - _TrackingNoise * 0.1, noise(float2(time * 5.0, uv.y * 3.0)));
                    uv.x += tracking * (noise(float2(time * 50.0, uv.y * 100.0)) - 0.5) * 0.2;
                }
                
                // Sample with chromatic aberration
                float3 col;
                if (_EnableChromatic > 0.5 && _ChromaticAmount > 0)
                {
                    float2 dir = (uv - 0.5) * _ChromaticAmount;
                    col.r = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + dir).r;
                    col.g = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).g;
                    col.b = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv - dir).b;
                }
                else
                {
                    col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).rgb;
                }
                
                // VHS color bleed
                if (_EnableVHS > 0.5 && _ColorBleed > 0)
                {
                    float bleed = _ColorBleed * 0.01;
                    float3 bleedCol = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(bleed, 0)).rgb;
                    col = lerp(col, float3(col.r, bleedCol.g, bleedCol.b), _ColorBleed * 0.5);
                }
                
                // CRT scanlines
                if (_EnableCRT > 0.5 && _ScanlineIntensity > 0)
                {
                    float scanline = sin(uv.y * _ScanlineCount * 3.14159) * 0.5 + 0.5;
                    scanline = pow(scanline, 1.5);
                    col *= 1.0 - scanline * _ScanlineIntensity;
                }
                
                // CRT phosphor glow
                if (_EnableCRT > 0.5 && _PhosphorGlow > 0)
                {
                    float glow = (col.r + col.g + col.b) / 3.0;
                    col += col * glow * _PhosphorGlow;
                }
                
                // VHS tape noise
                if (_EnableVHS > 0.5 && _TapeNoise > 0)
                {
                    float tapeNoise = noise(float2(uv.x * 100.0, time * 10.0 + uv.y * 50.0));
                    col += (tapeNoise - 0.5) * _TapeNoise * 0.3;
                }
                
                // Signal static noise
                if (_EnableSignalNoise > 0.5 && _StaticNoise > 0)
                {
                    float staticN = hash(uv + time) * _StaticNoise;
                    col += (staticN - _StaticNoise * 0.5);
                }
                
                // Snow
                if (_EnableSignalNoise > 0.5 && _SnowAmount > 0)
                {
                    float snow = hash(uv * 1000.0 + time * 100.0);
                    col = lerp(col, float3(snow, snow, snow), _SnowAmount);
                }
                
                // Color adjustments
                // Saturation
                float luma = dot(col, float3(0.299, 0.587, 0.114));
                col = lerp(float3(luma, luma, luma), col, _Saturation);
                
                // Hue shift
                if (abs(_HueShift) > 0.001)
                {
                    float3 hsv = rgb2hsv(col);
                    hsv.x = frac(hsv.x + _HueShift);
                    col = hsv2rgb(hsv);
                }
                
                // Gamma
                col = pow(max(col, 0), _Gamma);
                
                // CRT vignette
                if (_EnableCRT > 0.5 && _Vignette > 0)
                {
                    float2 vigUV = uv * (1.0 - uv.xy);
                    float vig = vigUV.x * vigUV.y * 15.0;
                    vig = pow(vig, _Vignette * 2.0);
                    col *= vig;
                }
                
                // Out of bounds check (for curvature)
                if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
                    col = float3(0, 0, 0);
                
                return float4(col, 1.0);
            }
            ENDHLSL
        }
    }
    
    Fallback Off
}
