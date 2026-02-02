using UnityEngine;
using UnityEngine.Video;
using System;

/// <summary>
/// Video file input for processing pre-recorded video through the Rutt/Etra effect.
/// Alternative to webcam input.
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class VideoFileInput : MonoBehaviour
{
    [Header("Video Source")]
    public VideoClip videoClip;
    public string videoURL;
    public VideoSource sourceType = VideoSource.VideoClip;
    
    [Header("Playback")]
    public bool playOnStart = true;
    public bool loop = true;
    public float playbackSpeed = 1f;
    
    [Header("Frame Output")]
    public RenderTexture outputTexture;
    public int outputWidth = 640;
    public int outputHeight = 480;
    
    // Events
    public event Action<Texture> OnFrameReady;
    public event Action OnVideoStarted;
    public event Action OnVideoEnded;
    public event Action OnVideoLooped;
    
    private VideoPlayer _videoPlayer;
    private RenderTexture _renderTexture;
    private bool _isReady;
    private bool _wasPlaying;
    
    public enum VideoSource { VideoClip, URL }
    
    public bool IsPlaying => _videoPlayer != null && _videoPlayer.isPlaying;
    public bool IsReady => _isReady;
    public double CurrentTime => _videoPlayer?.time ?? 0;
    public double Duration => _videoPlayer?.length ?? 0;
    public Texture CurrentFrame => _renderTexture;
    
    private void Awake()
    {
        _videoPlayer = GetComponent<VideoPlayer>();
        SetupVideoPlayer();
    }
    
    private void Start()
    {
        CreateRenderTexture();
        
        if (playOnStart && (videoClip != null || !string.IsNullOrEmpty(videoURL)))
        {
            Play();
        }
    }
    
    private void SetupVideoPlayer()
    {
        _videoPlayer.playOnAwake = false;
        _videoPlayer.isLooping = loop;
        _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        _videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        
        // Add audio source if needed
        if (GetComponent<AudioSource>() == null)
        {
            var audioSource = gameObject.AddComponent<AudioSource>();
            _videoPlayer.SetTargetAudioSource(0, audioSource);
        }
        
        _videoPlayer.prepareCompleted += OnPrepareCompleted;
        _videoPlayer.started += OnStarted;
        _videoPlayer.loopPointReached += OnLoopPoint;
    }
    
    private void CreateRenderTexture()
    {
        if (_renderTexture != null)
        {
            _renderTexture.Release();
            Destroy(_renderTexture);
        }
        
        _renderTexture = new RenderTexture(outputWidth, outputHeight, 0, RenderTextureFormat.ARGB32);
        _renderTexture.filterMode = FilterMode.Bilinear;
        _renderTexture.Create();
        
        _videoPlayer.targetTexture = _renderTexture;
        
        if (outputTexture == null)
            outputTexture = _renderTexture;
    }
    
    private void Update()
    {
        // Update playback speed
        if (_videoPlayer != null)
            _videoPlayer.playbackSpeed = playbackSpeed;
        
        // Send frame updates
        if (_isReady && _videoPlayer.isPlaying && _renderTexture != null)
        {
            OnFrameReady?.Invoke(_renderTexture);
        }
        
        // Check for video end (non-looping)
        if (_wasPlaying && !_videoPlayer.isPlaying && !loop)
        {
            OnVideoEnded?.Invoke();
        }
        _wasPlaying = _videoPlayer.isPlaying;
    }
    
    public void Play()
    {
        if (sourceType == VideoSource.VideoClip && videoClip != null)
        {
            _videoPlayer.source = UnityEngine.Video.VideoSource.VideoClip;
            _videoPlayer.clip = videoClip;
        }
        else if (sourceType == VideoSource.URL && !string.IsNullOrEmpty(videoURL))
        {
            _videoPlayer.source = UnityEngine.Video.VideoSource.Url;
            _videoPlayer.url = videoURL;
        }
        else
        {
            Debug.LogWarning("VideoFileInput: No video source specified");
            return;
        }
        
        _videoPlayer.Prepare();
    }
    
    public void PlayURL(string url)
    {
        videoURL = url;
        sourceType = VideoSource.URL;
        Play();
    }
    
    public void PlayClip(VideoClip clip)
    {
        videoClip = clip;
        sourceType = VideoSource.VideoClip;
        Play();
    }
    
    public void Pause()
    {
        _videoPlayer?.Pause();
    }
    
    public void Resume()
    {
        _videoPlayer?.Play();
    }
    
    public void Stop()
    {
        _videoPlayer?.Stop();
        _isReady = false;
    }
    
    public void Seek(double time)
    {
        if (_videoPlayer != null)
            _videoPlayer.time = time;
    }
    
    public void SeekNormalized(float t)
    {
        if (_videoPlayer != null)
            _videoPlayer.time = t * _videoPlayer.length;
    }
    
    public void SetLoop(bool shouldLoop)
    {
        loop = shouldLoop;
        if (_videoPlayer != null)
            _videoPlayer.isLooping = shouldLoop;
    }
    
    public void SetPlaybackSpeed(float speed)
    {
        playbackSpeed = Mathf.Clamp(speed, 0.1f, 10f);
    }
    
    private void OnPrepareCompleted(VideoPlayer source)
    {
        _isReady = true;
        
        // Update render texture size to match video
        if (source.width > 0 && source.height > 0)
        {
            // Optionally resize to video native resolution
            // outputWidth = (int)source.width;
            // outputHeight = (int)source.height;
            // CreateRenderTexture();
        }
        
        source.Play();
    }
    
    private void OnStarted(VideoPlayer source)
    {
        OnVideoStarted?.Invoke();
    }
    
    private void OnLoopPoint(VideoPlayer source)
    {
        OnVideoLooped?.Invoke();
    }
    
    /// <summary>
    /// Connect this video input to the mesh generator instead of webcam
    /// </summary>
    public void ConnectToMeshGenerator()
    {
        var meshGen = FindFirstObjectByType<RuttEtraMeshGenerator>();
        if (meshGen != null)
        {
            // Disconnect webcam
            var webcam = FindFirstObjectByType<WebcamCapture>();
            if (webcam != null)
                webcam.enabled = false;
            
            // The mesh generator will need to be modified to accept this input
            // For now, we provide the texture via event
            OnFrameReady += (tex) => meshGen.SendMessage("OnVideoFrame", tex, SendMessageOptions.DontRequireReceiver);
            
            Debug.Log("VideoFileInput connected to mesh generator");
        }
    }
    
    private void OnDestroy()
    {
        if (_videoPlayer != null)
        {
            _videoPlayer.prepareCompleted -= OnPrepareCompleted;
            _videoPlayer.started -= OnStarted;
            _videoPlayer.loopPointReached -= OnLoopPoint;
        }
        
        if (_renderTexture != null)
        {
            _renderTexture.Release();
            Destroy(_renderTexture);
        }
    }
}
