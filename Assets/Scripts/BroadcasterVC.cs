using System.Collections;
using agora_gaming_rtc;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;

using static agora_gaming_rtc.ExternalVideoFrame;

/// <summary>
///    Broadcast View Controller controls the client that uses the AR Camera to
/// Show the real world surrounding to the Audience client.  It receives the 
/// message about Audience client's drawing and draw in the Unity world space,
/// and such AR object is also included in the video sharing frames to show
/// to the Audience.
/// </summary>
public class BroadcasterVC : PlayerViewControllerBase
{
    public static TextureFormat ConvertFormat = TextureFormat.BGRA32;
    public static VIDEO_PIXEL_FORMAT PixelFormat = VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_BGRA;

    public static int ShareCameraMode = 1;  // 0 = unsafe buffer pointer, 1 = renderer iamge
    ARCameraManager cameraManager;

    MonoBehaviour monoProxy;
    int i = 0; // monotonic timestamp counter
    Camera renderCamera = null;

    Texture2D BufferTexture;

    public override void Join(string channel)
    {
        Debug.Log("calling join (channel = " + channel + ")");

        if (mRtcEngine == null)
            return;

        // set callbacks (optional)
        mRtcEngine.OnJoinChannelSuccess = OnJoinChannelSuccess;
        mRtcEngine.OnUserJoined = OnUserJoined;
        mRtcEngine.OnUserOffline = OnUserOffline;

        int s = mRtcEngine.SetVideoEncoderConfiguration(new VideoEncoderConfiguration
        {
            dimensions = new VideoDimensions()
            {
                width = 1080,
                height = 1920
            },
            orientationMode = ORIENTATION_MODE.ORIENTATION_MODE_ADAPTIVE
        });
#if !UNITY_EDITOR
        Debug.Assert(s == 0, "RTC set video encoder configuration failed.");
#endif

        // enable video
        mRtcEngine.EnableVideo();
        // allow camera output callback
        mRtcEngine.EnableVideoObserver();
        //mRtcEngine.EnableLocalVideo(false);
        CameraCapturerConfiguration config = new CameraCapturerConfiguration();
        config.preference = CAPTURER_OUTPUT_PREFERENCE.CAPTURER_OUTPUT_PREFERENCE_AUTO;
        config.cameraDirection = CAMERA_DIRECTION.CAMERA_REAR;
        mRtcEngine.SetCameraCapturerConfiguration(config);

        mRtcEngine.SetVideoQualityParameters(true);
        mRtcEngine.SetExternalVideoSource(true, false);

        // join channel
        mRtcEngine.JoinChannel(channel, null, 0);

        // Optional: if a data stream is required, here is a good place to create it
        int streamID = mRtcEngine.CreateDataStream(true, true);
        Debug.Log("initializeEngine done, data stream id = " + streamID);
    }

    public override void OnSceneLoaded()
    {
        base.OnSceneLoaded();
        GameObject go = GameObject.Find("ButtonColor");
        if (go != null)
        {
            // the button is only available for AudienceVC
            go.SetActive(false);
        }

        go = GameObject.Find("AR Camera");
        if (go != null)
        {
            monoProxy = go.GetComponent<MonoBehaviour>();
            cameraManager = go.GetComponent<ARCameraManager>();
        }


        go = GameObject.Find("sphere");
        if (go != null)
        {
            var sphere = go;
            // hide this before AR Camera start capturing
            sphere.SetActive(false);
            monoProxy.StartCoroutine(DelayAction(.5f,
                () =>
                {
                    sphere.SetActive(true);
                }));
        }

        go = GameObject.Find("RenderCamera");
        renderCamera = go.GetComponent<Camera>();
    }

    // When a remote user joined, this delegate will be called. Typically
    // create a GameObject to render video on it
    protected override void OnUserJoined(uint uid, int elapsed)
    {
        base.OnUserJoined(uid, elapsed);
        OnEnable();
    }

    void OnEnable()
    {
        cameraManager.frameReceived += OnCameraFrameReceived;
        RenderTexture renderTexture = Camera.main.targetTexture;
        if (renderTexture != null)
        {
            BufferTexture = new Texture2D(renderTexture.width, renderTexture.height, ConvertFormat, false);

            // Editor only, where onFrameReceived won't invoke
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
            {
                Debug.LogWarning(">>> Testing in Editor, start coroutine to capture Render data");
                monoProxy.StartCoroutine(CoShareRenderData());
            }
        }
    }

    void OnDisable()
    {
        cameraManager.frameReceived -= OnCameraFrameReceived;

        BufferTexture = null;
    }

    IEnumerator DelayAction(float delay, System.Action doAction)
    {
        yield return new WaitForSeconds(delay);
        doAction();
    }

    /// <summary>
    ///   Delegate callback handles every frame generated by the AR Camera.
    /// </summary>
    /// <param name="eventArgs"></param>
    private void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        // There are two ways doing the capture. 
        if (ShareCameraMode == 0)
        {
            // See function header for what this function is
            // CaptureARBuffer();
        }
        else
        {
            ShareRenderTexture();
        }
    }

    // When remote user is offline, this delegate will be called. Typically
    // delete the GameObject for this user
    protected override void OnUserOffline(uint uid, USER_OFFLINE_REASON reason)
    {
        // remove video stream
        Debug.Log("onUserOffline: uid = " + uid + " reason = " + reason);
        // this is called in main thread
        GameObject go = GameObject.Find(uid.ToString());
        if (!ReferenceEquals(go, null))
        {
            UnityEngine.Object.Destroy(go);
        }
        OnDisable();
    }

    // Uncomment the follow function to try out XRCameraImage method to 
    // get the image of the AR Camera. Requires unsafe code compilation option
    // in Settings.
    /*
    private unsafe void CaptureARBuffer()
    {
        // Get the image in the ARSubsystemManager.cameraFrameReceived callback

        XRCameraImage image;
        if (!cameraManager.TryGetLatestImage(out image))
        {
            Debug.LogWarning("Capture AR Buffer returns nothing!!!!!!");
            return;
        }

        var conversionParams = new XRCameraImageConversionParams
        {
            // Get the full image
            inputRect = new RectInt(0, 0, image.width, image.height),

            // Downsample by 2
            outputDimensions = new Vector2Int(image.width, image.height),

            // Color image format
            outputFormat = ConvertFormat,

            // Flip across the x axis
            transformation = CameraImageTransformation.MirrorX

            // Call ProcessImage when the async operation completes
        };
        // See how many bytes we need to store the final image.
        int size = image.GetConvertedDataSize(conversionParams);

        // Allocate a buffer to store the image
        var buffer = new NativeArray<byte>(size, Allocator.Temp);

        // Extract the image data
        image.Convert(conversionParams, new System.IntPtr(buffer.GetUnsafePtr()), buffer.Length);

        // The image was converted to RGBA32 format and written into the provided buffer
        // so we can dispose of the CameraImage. We must do this or it will leak resources.

        byte[] bytes = buffer.ToArray();
        monoProxy.StartCoroutine(PushFrame(bytes, image.width, image.height,
                 () => { image.Dispose(); buffer.Dispose(); }));
    }
    */

    /// <summary>
    ///   Get the image from renderTexture.  (AR Camera must assign a RenderTexture prefab in
    /// its renderTexture field.)
    /// </summary>
    private void ShareRenderTexture()
    {
        if (BufferTexture == null) // offlined
        {
            return;
        }
        Camera targetCamera = Camera.main; // AR Camera
        RenderTexture.active = targetCamera.targetTexture; // the targetTexture holds render texture
        Rect rect = new Rect(0, 0, targetCamera.targetTexture.width, targetCamera.targetTexture.height);
        BufferTexture.ReadPixels(rect, 0, 0);
        BufferTexture.Apply();

        byte[] bytes = BufferTexture.GetRawTextureData();

        // sends the Raw data contained in bytes
        monoProxy.StartCoroutine(PushFrame(bytes, (int)rect.width, (int)rect.height,
         () =>
         {
             bytes = null;
         }));
        RenderTexture.active = null;
    }

    /// <summary>
    ///    For use in Editor testing only.
    /// </summary>
    /// <returns></returns>
    IEnumerator CoShareRenderData()
    {
        while (ShareCameraMode == 1)
        {
            yield return new WaitForEndOfFrame();
            OnCameraFrameReceived(default);
        }
        yield return null;
    }

    /// <summary>
    /// Push frame to the remote client.  This is the same code that does ScreenSharing.
    /// </summary>
    /// <param name="bytes">raw video image data</param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="onFinish">callback upon finish of the function</param>
    /// <returns></returns>
    IEnumerator PushFrame(byte[] bytes, int width, int height, System.Action onFinish)
    {
        if (bytes == null || bytes.Length == 0)
        {
            Debug.LogError("Zero bytes found!!!!");
            yield break;
        }

        IRtcEngine rtc = IRtcEngine.QueryEngine();
        //if the engine is present
        if (rtc != null)
        {
            //Create a new external video frame
            ExternalVideoFrame externalVideoFrame = new ExternalVideoFrame();
            //Set the buffer type of the video frame
            externalVideoFrame.type = ExternalVideoFrame.VIDEO_BUFFER_TYPE.VIDEO_BUFFER_RAW_DATA;
            // Set the video pixel format
            externalVideoFrame.format = PixelFormat; // VIDEO_PIXEL_BGRA for now
            //apply raw data you are pulling from the rectangle you created earlier to the video frame
            externalVideoFrame.buffer = bytes;
            //Set the width of the video frame (in pixels)
            externalVideoFrame.stride = width;
            //Set the height of the video frame
            externalVideoFrame.height = height;
            //Remove pixels from the sides of the frame
            externalVideoFrame.cropLeft = 10;
            externalVideoFrame.cropTop = 10;
            externalVideoFrame.cropRight = 10;
            externalVideoFrame.cropBottom = 10;
            //Rotate the video frame (0, 90, 180, or 270)
            //externalVideoFrame.rotation = 90;
            externalVideoFrame.rotation = 180;
            // increment i with the video timestamp
            externalVideoFrame.timestamp = i++;
            //Push the external video frame with the frame we just created
            // int a = 
            rtc.PushVideoFrame(externalVideoFrame);
            // Debug.Log(" pushVideoFrame(" + i + ") size:" + bytes.Length + " => " + a);

        }
        yield return null;
        onFinish();
    }
}
