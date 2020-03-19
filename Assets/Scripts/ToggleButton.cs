using UnityEngine;
using UnityEngine.UI;

public class ToggleButton : MonoBehaviour
{
    public Button button1;

    public Button button2;

    private void Start()
    {
        Toggle(false);
    }

    void Toggle(bool enable)
    {
        button1.gameObject.SetActive(enable);
        button2.gameObject.SetActive(!enable);
    }

    private bool tapped = false;
    public void Tap()
    {
        Toggle(tapped);
        tapped = !tapped;
    }
}
