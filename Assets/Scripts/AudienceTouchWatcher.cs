using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
///   AudienceTouchWatcher is a handler that watches user's screen touch.
/// It will record the touch points in a buffer and send the buffer when it
/// is filled or at the end of the touch.
/// </summary>
public class AudienceTouchWatcher : MonoBehaviour
{
    // need to put 3d object under the parent canvas object to show
    [SerializeField] Transform parentCanvasObject = null;
    [SerializeField] GameObject DrawPrefab = null;
    [SerializeField] GraphicRaycaster graphicRaycaster = null;

    [SerializeField] bool showLocalDrawing = false;

    List<Vector2> Points;
    Vector3 lastPos = Vector3.positiveInfinity;
    GameObject hostingGO;

    const int LAYER_UI = 5;
    const int BufferLength = 10;

    public System.Action<DrawmarkModel> ProcessDrawing;
    public System.Action NotifyClearDrawings;
    public Color DrawColor = Color.black;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Points = new List<Vector2>();
        }

        if (Input.GetMouseButton(0))
        {
            if (RayHitUI())
            {
                return;
            }

            Vector3 pos = Input.mousePosition;
            if (DistanceToLastPoint(pos) > 0.1f)
            {
                if (showLocalDrawing)
                {
                    DrawDot(pos);
                }
                lastPos = pos;
                BufferSendPoints(NormalizePoint(pos));
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            SendDrawing();
        }
    }


    float DistanceToLastPoint(Vector3 point)
    {
        if (lastPos == Vector3.positiveInfinity) { return Mathf.Infinity; }
        return Vector3.Distance(lastPos, point);
    }

    /// <summary>
    ///    Draw the dots on the screen after converting to world position.
    ///  Also send the world position to remote client. 
    /// </summary>
    /// <param name="screenPos">Screen Position</param>
    void DrawDot(Vector3 screenPos)
    {
        if (hostingGO == null)
        {
            hostingGO = new GameObject();
            hostingGO.transform.position = Vector3.zero;
            hostingGO.transform.SetParent(parentCanvasObject);
            hostingGO.transform.localScale = Vector3.one;
        }

        Vector3 pos = Camera.main.ScreenToWorldPoint(screenPos);
        GameObject go = GameObject.Instantiate(DrawPrefab, pos, Quaternion.identity);
        go.transform.SetParent(hostingGO.transform);
        go.transform.localPosition = new Vector3(go.transform.localPosition.x,
            go.transform.localPosition.y, 0);

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
    ///    Put the position from the current screen to a format other device to use.
    /// </summary>
    /// <param name="screenPos"></param>
    /// <returns></returns>
    public static Vector2 NormalizePoint(Vector3 screenPos)
    {
        Vector3 vp = Camera.main.ScreenToViewportPoint(screenPos);
        return new Vector2(vp.x, vp.y);
    }

    public void ClearDrawings()
    {
        if (hostingGO != null)
        {
            Destroy(hostingGO);
            hostingGO = null;
        }
        lastPos = Vector3.positiveInfinity;

        if (NotifyClearDrawings != null)
        {
            NotifyClearDrawings();
        }
    }

    void BufferSendPoints(Vector3 pos)
    {
        Points.Add(new Vector2(pos.x, pos.y));

        if (Points.Count > BufferLength)
        {
            SendDrawing();
        }
    }

    void SendDrawing()
    {
        if (Points == null || Points.Count == 0) { return; }

        DrawmarkModel dm = new DrawmarkModel
        {
            color = DrawColor,
            points = Points
        };

        if (ProcessDrawing != null)
        {
            ProcessDrawing(dm);
        }

        Points = new List<Vector2>();
    }

    /// <summary>
    ///   Checking if the touch is on a UI component, which should be ignored.
    /// </summary>
    /// <returns></returns>
    bool RayHitUI()
    {
        //Create the PointerEventData with null for the EventSystem
        PointerEventData ped = new PointerEventData(null);
        //Set required parameters, in this case, mouse position
        ped.position = Input.mousePosition;
        //Create list to receive all results
        List<RaycastResult> results = new List<RaycastResult>();
        //Raycast it
        graphicRaycaster.Raycast(ped, results);

        if (results.Count > 0)
        {
            return results.Any(r => r.gameObject.layer == LAYER_UI);
        }
        return false;
    }
}
