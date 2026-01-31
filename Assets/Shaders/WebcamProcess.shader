Shader "Hidden/RuttEtra/WebcamProcess"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MirrorX ("Mirror X", Float) = 1
        _MirrorY ("Mirror Y", Float) = 0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        
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
            
            float _MirrorX;
            float _MirrorY;
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                
                float2 uv = input.uv;
                if (_MirrorX > 0.5) uv.x = 1.0 - uv.x;
                if (_MirrorY > 0.5) uv.y = 1.0 - uv.y;
                output.uv = uv;
                
                return output;
            }
            
            float4 frag(Varyings input) : SV_Target
            {
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
            }
            ENDHLSL
        }
    }
}




