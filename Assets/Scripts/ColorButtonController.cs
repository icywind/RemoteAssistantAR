using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorButtonController : MonoBehaviour
{
    [SerializeField]
    private Button mainButton = null;

    [SerializeField]
    private GameObject buttonGroup = null;

    /// <summary>
    ///   The group of button color
    /// </summary>
    [SerializeField]
    private List<Color> ButtonColors = new List<Color>();

    [SerializeField]
    private GameObject buttonColorSample = null;  // this is the first color

    public Color SelectedColor { get; private set; }

    public event System.Action<Color> OnColorChange;

    private void Awake()
    {
        mainButton.onClick.AddListener(OnMainButtonTap);
        SetupColorChoices();
        OnColorSelect(0);
    }

    private void SetupColorChoices()
    {
        GameObject go;
        Vector3 beginPos = buttonColorSample.transform.localPosition;
        for (int i = 0; i < ButtonColors.Count; i++)
        {
            if (i == 0) { go = buttonColorSample.gameObject; }
            else
            {
                go = GameObject.Instantiate(buttonColorSample.gameObject);
                go.transform.SetParent(buttonColorSample.transform.parent);
                go.transform.localPosition = beginPos + i * 85 * Vector3.up;
                go.transform.localScale = Vector3.one;
            }

            Image image = go.GetComponent<Image>();
            if (image != null)
            {
                image.color = ButtonColors[i];
            }
            Button button = go.GetComponent<Button>();
            if (button != null)
            {
                int colorIndex = i;
                button.onClick.AddListener(() => { OnColorSelect(colorIndex); });
            }
        }
    }

    private void OnMainButtonTap()
    {
        buttonGroup.SetActive(!buttonGroup.activeInHierarchy);
    }

    private void OnColorSelect(int num)
    {
        SelectedColor = ButtonColors[num];
        mainButton.GetComponent<Image>().color = SelectedColor;
        OnMainButtonTap();
        if (OnColorChange != null)
        {
            OnColorChange(SelectedColor);
        }
    }
}
