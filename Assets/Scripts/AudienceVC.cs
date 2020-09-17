using UnityEngine;
using System.Collections;
using agora_gaming_rtc;

/// <summary>
///   AudienceVC acts as an audience to view the AR Caster's feed.  The viewer
/// can draw the area of interest on the AR Caster's screen remotely.
///   This controller relies on two other Monobehavior object:
///      - ColorController: allows user to pick a color for drawing
///      - TouchWatcher: handles screen touch for drawing.
/// </summary>
public class AudienceVC : PlayerViewControllerBase
{
    ColorButtonController colorButtonController;
    AudienceTouchWatcher touchWatcher;
    MonoBehaviour monoProxy;

    IRtcEngine rtcEngine;
    int dataStreamId = 0;

    protected override string RemoteStreamTargetImage
    {
        get
        {
            return MainVideoName;
        }
    }

    public override void OnSceneLoaded()
    {
        base.OnSceneLoaded();
        GameObject gameObject = GameObject.Find(SelfVideoName);
        if (gameObject != null)
        {
            gameObject.AddComponent<VideoSurface>();
        }

        gameObject = GameObject.Find("ColorController");
        if (gameObject != null)
        {
            colorButtonController = gameObject.GetComponent<ColorButtonController>();
            monoProxy = colorButtonController.GetComponent<MonoBehaviour>();
        }

        gameObject = GameObject.Find("TouchWatcher");
        if (gameObject != null)
        {
            touchWatcher = gameObject.GetComponent<AudienceTouchWatcher>();
            touchWatcher.DrawColor = colorButtonController.SelectedColor;
            touchWatcher.ProcessDrawing += ProcessDrawing;
            touchWatcher.NotifyClearDrawings += delegate ()
            {
                monoProxy.StartCoroutine(CoClearDrawing());

            };

            colorButtonController.OnColorChange += delegate (Color color)
            {
                touchWatcher.DrawColor = color;
            };
        }

        rtcEngine = IRtcEngine.QueryEngine();
        dataStreamId = rtcEngine.CreateDataStream(reliable: true, ordered: true);
    }

    public override void Join(string channel)
    {
        Debug.Log("Aud calling join (channel = " + channel + ")");

        if (mRtcEngine == null)
            return;

        // set callbacks (optional)
        mRtcEngine.OnJoinChannelSuccess = OnJoinChannelSuccess;
        mRtcEngine.OnUserJoined = OnUserJoined;
        mRtcEngine.OnUserOffline = OnUserOffline;

        // enable video
        mRtcEngine.EnableVideo();
        // allow camera output callback
        mRtcEngine.EnableVideoObserver();
        mRtcEngine.EnableLocalAudio(false);
        mRtcEngine.MuteLocalAudioStream(true);

        //mRtcEngine.SetChannelProfile(CHANNEL_PROFILE.CHANNEL_PROFILE_LIVE_BROADCASTING);
        //mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_AUDIENCE);

        // join channel
        mRtcEngine.JoinChannel(channel, null, 0);

        Debug.Log("initializeEngine done");
    }
    void ProcessDrawing(DrawmarkModel dm)
    {
        monoProxy.StartCoroutine(CoProcessDrawing(dm));
    }

    IEnumerator CoProcessDrawing(DrawmarkModel dm)
    {
        string json = JsonUtility.ToJson(dm);
        byte[] data = System.Text.Encoding.UTF8.GetBytes(json);
        if (dataStreamId > 0)
        {
            rtcEngine.SendStreamMessage(dataStreamId, data);
        }

        yield return null;
    }

    IEnumerator CoClearDrawing()
    {
        string json = "{\"clear\": true}";
        byte[] data = System.Text.Encoding.UTF8.GetBytes(json);
        if (dataStreamId > 0)
        {
            rtcEngine.SendStreamMessage(dataStreamId, data);
        }

        yield return null;
    }
}
