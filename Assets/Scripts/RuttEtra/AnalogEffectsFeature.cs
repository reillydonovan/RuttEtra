using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

/// <summary>
/// URP Renderer Feature for analog effects post-processing.
/// Compatible with Unity 6's RenderGraph API.
/// </summary>
public class AnalogEffectsFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class AnalogEffectsSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }
    
    public AnalogEffectsSettings settings = new AnalogEffectsSettings();
    private AnalogEffectsPass _pass;
    
    public override void Create()
    {
        _pass = new AnalogEffectsPass(settings);
    }
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Only add pass for game/scene cameras
        if (renderingData.cameraData.cameraType == CameraType.Game || 
            renderingData.cameraData.cameraType == CameraType.SceneView)
        {
            renderer.EnqueuePass(_pass);
        }
    }
    
    protected override void Dispose(bool disposing)
    {
        _pass?.Dispose();
    }
    
    class AnalogEffectsPass : ScriptableRenderPass
    {
        private AnalogEffectsSettings _settings;
        private Material _material;
        
        private static readonly int ScanlineIntensity = Shader.PropertyToID("_ScanlineIntensity");
        private static readonly int ScanlineCount = Shader.PropertyToID("_ScanlineCount");
        private static readonly int PhosphorGlow = Shader.PropertyToID("_PhosphorGlow");
        private static readonly int ScreenCurvature = Shader.PropertyToID("_ScreenCurvature");
        private static readonly int Vignette = Shader.PropertyToID("_Vignette");
        private static readonly int TrackingNoise = Shader.PropertyToID("_TrackingNoise");
        private static readonly int ColorBleed = Shader.PropertyToID("_ColorBleed");
        private static readonly int TapeNoise = Shader.PropertyToID("_TapeNoise");
        private static readonly int HorizontalJitter = Shader.PropertyToID("_HorizontalJitter");
        private static readonly int ChromaticAmount = Shader.PropertyToID("_ChromaticAmount");
        private static readonly int HorizontalHold = Shader.PropertyToID("_HorizontalHold");
        private static readonly int VerticalHold = Shader.PropertyToID("_VerticalHold");
        private static readonly int DriftSpeed = Shader.PropertyToID("_DriftSpeed");
        private static readonly int StaticNoise = Shader.PropertyToID("_StaticNoise");
        private static readonly int SnowAmount = Shader.PropertyToID("_SnowAmount");
        private static readonly int Saturation = Shader.PropertyToID("_Saturation");
        private static readonly int HueShift = Shader.PropertyToID("_HueShift");
        private static readonly int Gamma = Shader.PropertyToID("_Gamma");
        private static readonly int EnableCRT = Shader.PropertyToID("_EnableCRT");
        private static readonly int EnableVHS = Shader.PropertyToID("_EnableVHS");
        private static readonly int EnableChromatic = Shader.PropertyToID("_EnableChromatic");
        private static readonly int EnableHoldDrift = Shader.PropertyToID("_EnableHoldDrift");
        private static readonly int EnableSignalNoise = Shader.PropertyToID("_EnableSignalNoise");
        private static readonly int EffectTime = Shader.PropertyToID("_EffectTime");
        private static readonly int BlitTexture = Shader.PropertyToID("_BlitTexture");
        
        public AnalogEffectsPass(AnalogEffectsSettings settings)
        {
            _settings = settings;
            renderPassEvent = settings.renderPassEvent;
            profilingSampler = new ProfilingSampler("AnalogEffects");
            requiresIntermediateTexture = true;
            
            // Create material
            var shader = Shader.Find("RuttEtra/AnalogEffects");
            if (shader != null)
            {
                _material = CoreUtils.CreateEngineMaterial(shader);
            }
        }
        
        private class PassData
        {
            public TextureHandle source;
            public TextureHandle destination;
            public Material material;
        }
        
        // New RenderGraph API for Unity 6
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // Find the AnalogEffects component
            var effects = Object.FindFirstObjectByType<AnalogEffects>();
            if (effects == null || !effects.enableEffects) return;
            
            // Check if any effect is enabled
            if (!effects.enableCRT && !effects.enableVHS && !effects.enableChromatic && 
                !effects.enableHoldDrift && !effects.enableSignalNoise)
            {
                return;
            }
            
            if (_material == null) return;
            
            // Update material properties from component
            UpdateMaterialFromComponent(effects);
            
            // Get resource data
            var resourceData = frameData.Get<UniversalResourceData>();
            
            // Skip if rendering to back buffer directly
            if (resourceData.isActiveTargetBackBuffer)
                return;
            
            var source = resourceData.activeColorTexture;
            if (!source.IsValid())
                return;
            
            var destinationDesc = renderGraph.GetTextureDesc(source);
            destinationDesc.name = "_AnalogEffectsTempRT";
            destinationDesc.clearBuffer = false;
            
            TextureHandle destination = renderGraph.CreateTexture(destinationDesc);
            
            // Use unsafe pass for blitting
            using (var builder = renderGraph.AddUnsafePass<PassData>("AnalogEffects", out var passData))
            {
                passData.source = source;
                passData.destination = destination;
                passData.material = _material;
                
                builder.UseTexture(source, AccessFlags.ReadWrite);
                builder.UseTexture(destination, AccessFlags.ReadWrite);
                
                builder.SetRenderFunc((PassData data, UnsafeGraphContext ctx) =>
                {
                    // Get the native command buffer from the unsafe command buffer
                    CommandBuffer cmd = CommandBufferHelpers.GetNativeCommandBuffer(ctx.cmd);
                    
                    // Set source texture on material
                    data.material.SetTexture(BlitTexture, data.source);
                    
                    // Blit source through effect material to temp
                    Blitter.BlitCameraTexture(cmd, data.source, data.destination, data.material, 0);
                    // Blit temp back to source (copy)
                    Blitter.BlitCameraTexture(cmd, data.destination, data.source);
                });
            }
        }
        
        private void UpdateMaterialFromComponent(AnalogEffects effects)
        {
            if (_material == null || effects == null) return;
            
            // Enable flags
            _material.SetFloat(EnableCRT, effects.enableCRT ? 1 : 0);
            _material.SetFloat(EnableVHS, effects.enableVHS ? 1 : 0);
            _material.SetFloat(EnableChromatic, effects.enableChromatic ? 1 : 0);
            _material.SetFloat(EnableHoldDrift, effects.enableHoldDrift ? 1 : 0);
            _material.SetFloat(EnableSignalNoise, effects.enableSignalNoise ? 1 : 0);
            
            // Time
            _material.SetFloat(EffectTime, Time.time);
            
            // CRT
            _material.SetFloat(ScanlineIntensity, effects.scanlineIntensity);
            _material.SetFloat(ScanlineCount, effects.scanlineCount);
            _material.SetFloat(PhosphorGlow, effects.phosphorGlow);
            _material.SetFloat(ScreenCurvature, effects.screenCurvature);
            _material.SetFloat(Vignette, effects.vignette);
            
            // VHS
            _material.SetFloat(TrackingNoise, effects.trackingNoise);
            _material.SetFloat(ColorBleed, effects.colorBleed);
            _material.SetFloat(TapeNoise, effects.tapeNoise);
            _material.SetFloat(HorizontalJitter, effects.horizontalJitter);
            
            // Chromatic
            _material.SetFloat(ChromaticAmount, effects.chromaticAmount);
            
            // Hold drift
            _material.SetFloat(HorizontalHold, effects.horizontalHold);
            _material.SetFloat(VerticalHold, effects.verticalHold);
            _material.SetFloat(DriftSpeed, effects.driftSpeed);
            
            // Noise
            _material.SetFloat(StaticNoise, effects.staticNoise);
            _material.SetFloat(SnowAmount, effects.snowAmount);
            
            // Color
            _material.SetFloat(Saturation, effects.saturation);
            _material.SetFloat(HueShift, effects.hueShift);
            _material.SetFloat(Gamma, effects.gamma);
        }
        
        public void Dispose()
        {
            if (_material != null)
            {
                CoreUtils.Destroy(_material);
            }
        }
    }
}
