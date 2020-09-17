using System.Collections;
using UnityEngine;
using agora_gaming_rtc;

public class RemoteDrawer : MonoBehaviour
{
    // Start is called before the first frame update
    IRtcEngine rtcEngine;

    Camera arCam;     // the AR Camera
    Camera renderCam; // the Renderer Camera, space of 3D objects
    Camera viewCam; // the viewer of the projected quad, acting camera since AR Camera projects into a RenderTexture

    [SerializeField] Transform referenceObject = null;

    [SerializeField] GameObject DrawPrefab = null;

    public float DotScale = 0.15f;

    private GameObject anchorGO;
    private Color DrawColor = Color.black;

    void Start()
    {
        rtcEngine = IRtcEngine.QueryEngine();
        if (rtcEngine != null)
        {
            rtcEngine.OnStreamMessage += OnStreamMessageHandler;
        }

        CamStart();
    }


    /// <summary>
    ///    The delegate function to handle message sent from Audience side
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="streamId"></param>
    /// <param name="data"></param>
    /// <param name="length"></param>
    void OnStreamMessageHandler(uint userId, int streamId, byte[] buffer, int length)
    {
        string data = System.Text.Encoding.UTF8.GetString(buffer, 0, length);
        if (data.Contains("color"))
        {
            StartCoroutine(CoProcessDrawingData(data));
        }
        else if (data.Contains("clear"))
        {
            Destroy(anchorGO);
        }

        Debug.Log("Main Camera pos = " + Camera.main.transform.position);
    }

    /// <summary>
    ///  Do the drawing async
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    IEnumerator CoProcessDrawingData(string data)
    {
        try
        {
            DrawmarkModel dm = JsonUtility.FromJson<DrawmarkModel>(data);
            DrawColor = dm.color;
            foreach (Vector2 pos in dm.points)
            {
                DrawDot(pos);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
        yield return null;
    }


    int dotCount = 0;
    /// <summary>
    ///   
    /// </summary>
    /// <param name="pos">Screen Position</param>
    void DrawDot(Vector2 pos)
    {
        if (anchorGO == null)
        {
            anchorGO = new GameObject();
            anchorGO.transform.SetParent(referenceObject.transform.parent);
            anchorGO.transform.position = Vector3.zero;
            anchorGO.transform.localScale = Vector3.one;
            anchorGO.name = "DrawAnchor";
        }


        // DeNormalize the position and adjust to passed camera
        Vector3 location = DeNormalizedPosition(pos, renderCam);


        GameObject go = GameObject.Instantiate(DrawPrefab, location, Quaternion.identity);
        go.transform.SetParent(anchorGO.transform);
        go.transform.localScale = DotScale * Vector3.one;
        go.layer = (int)CameraLayer.IGNORE_RAYCAST;
        go.name = "dot " + dotCount;
        dotCount++;
        Debug.LogFormat("{0} pos:{1} => : {2} ", go.name, pos, location);
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = renderer.material;
            if (mat != null)
            {
                mat.color = DrawColor;
            }
        }
    }

    /// <summary>
    ///    Provide a ViewPort Position (0,0) = bottom left and (1,1) top right
    /// return world position for the current camera
    /// </summary>
    /// <param name="vector2"></param>
    /// <param name="camera"></param>
    /// <returns></returns>
    Vector3 DeNormalizedPosition(Vector2 vector2, Camera camera)
    {

        Vector3 pos = new Vector3(vector2.x, vector2.y);

        pos = camera.ViewportToScreenPoint(pos);

        // Consider using the referenceObject for position calculation
        // Vector3 deltaPos = camera.transform.position - referenceObject.position;

        pos = new Vector3(pos.x, pos.y, 8);

        return camera.ScreenToWorldPoint(pos);
    }

    // Use this for initialization
    void CamStart()
    {
        renderCam = GameObject.Find("RenderCamera").GetComponent<Camera>();
        arCam = GameObject.Find("AR Camera").GetComponent<Camera>();
        viewCam = GameObject.Find("ViewCamera").GetComponent<Camera>();
    }
}
