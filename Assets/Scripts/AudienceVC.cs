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

    void ProcessDrawing(DrawmarkModel dm)
    {
        monoProxy.StartCoroutine(CoProcessDrawing(dm));
    }

    IEnumerator CoProcessDrawing(DrawmarkModel dm)
    {
        string json = JsonUtility.ToJson(dm);
        if (dataStreamId > 0)
        {
            rtcEngine.SendStreamMessage(dataStreamId, json);
        }

        yield return null;
    }

    IEnumerator CoClearDrawing()
    {
        string json = "{\"clear\": true}";
        if (dataStreamId > 0)
        {
            rtcEngine.SendStreamMessage(dataStreamId, json);
        }

        yield return null;
    }
}
