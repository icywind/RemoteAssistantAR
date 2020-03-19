using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
#if(UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
using UnityEngine.Android;
#endif
using agora_gaming_rtc;

public enum PlayerMode
{
    BroadCaster,
    Audience
};

/// <summary>
///    TestHome serves a game controller object for this application.
/// </summary>
public class GameController : MonoBehaviour
{

    // Use this for initialization
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
    private ArrayList permissionList = new ArrayList();
#endif
    static IVideoChatClient app = null;

    private string HomeSceneName = "MainScene";

    private string AudienceSceneName = "AudPlayScene";
    private string BroadcastSceneName = "CastARScene";

    // PLEASE KEEP THIS App ID IN SAFE PLACE
    // Get your own App ID at https://dashboard.agora.io/
    [SerializeField]
    private string AppID = "your_appid";

    public PlayerMode PlayerMode { get; private set; }
    private IRtcEngine mRtcEngine;

    void Awake()
    {
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
		permissionList.Add(Permission.Microphone);         
		permissionList.Add(Permission.Camera);               
#endif

        // keep this alive across scenes
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        if (CheckAppId())
        {
            ShowVersion();
        }
        LoadLastChannel();
    }

    void Update()
    {
        CheckPermissions();
    }

    /// <summary>
    ///   Check if a AppID exist
    /// </summary>
    /// <returns>yes => a string of at least 10 characters long ...</returns>
    private bool CheckAppId()
    {
        Debug.Assert(AppID.Length > 10, "Please fill in your AppId first on Game Controller object.");
        return AppID.Length > 10;
    }

    /// <summary>
    ///   Checks for platform dependent permissions.
    /// </summary>
    private void CheckPermissions()
    {
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
        foreach(string permission in permissionList)
        {
            if (!Permission.HasUserAuthorizedPermission(permission))
            {                 
				Permission.RequestUserPermission(permission);
			}
        }
#endif
    }

    private void LoadLastChannel()
    {
        string channel = PlayerPrefs.GetString("ChannelName");
        if (!string.IsNullOrEmpty(channel))
        {
            GameObject go = GameObject.Find("ChannelName");
            InputField field = go.GetComponent<InputField>();

            field.text = channel;
        }
    }

    private void ShowVersion()
    {
        // init engine
        mRtcEngine = IRtcEngine.GetEngine(AppID);

        // enable log
        mRtcEngine.SetLogFilter(LOG_FILTER.DEBUG | LOG_FILTER.INFO | LOG_FILTER.WARNING | LOG_FILTER.ERROR | LOG_FILTER.CRITICAL);

        GameObject textVersionGameObject = GameObject.Find("VersionText");
        textVersionGameObject.GetComponent<Text>().text = "SDK Version : " + IRtcEngine.GetSdkVersion();
    }

    public void OnJoinButtonClicked(int mode)
    {
        // get parameters (channel name, channel profile, etc.)
        GameObject go = GameObject.Find("ChannelName");
        InputField field = go.GetComponent<InputField>();

        if (string.IsNullOrEmpty(field.text))
        {
            Debug.LogWarning("Channel name is empty!");
            return;
        }

        PlayerMode = mode == 0 ? PlayerMode.BroadCaster : PlayerMode.Audience;

        // create app if nonexistent
        if (PlayerMode == PlayerMode.BroadCaster)
        {
            app = new BroadcasterVC();
        }
        else
        {
            app = new AudienceVC();
        }

        app.LoadEngine(AppID);
        Debug.LogWarning("Joining with mode = " + PlayerMode);
        // join channel and jump to next scene
        app.Join(field.text);


        // Save the channel name so we don't need to type it when coming back
        PlayerPrefs.SetString("ChannelName", field.text);
        PlayerPrefs.Save();

        SceneManager.sceneLoaded += OnLevelFinishedLoading; // configure GameObject after scene is loaded
        SceneManager.LoadScene(PlayerMode == PlayerMode.Audience ? AudienceSceneName : BroadcastSceneName, LoadSceneMode.Single);
    }

    private void OnLeaveButtonClicked()
    {
        if (!ReferenceEquals(app, null))
        {
            app.Leave(); // leave channel
            app.UnloadEngine(); // delete engine
            app = null; // delete app
            SceneManager.LoadScene(HomeSceneName, LoadSceneMode.Single);
        }
        Destroy(gameObject);
    }


    private void SetupToggleMic()
    {

        GameObject go = GameObject.Find("ToggleButton");
        if (go != null)
        {
            ToggleButton toggle = go.GetComponent<ToggleButton>();
            if (toggle != null)
            {

                toggle.button1.onClick.AddListener(() =>
                {
                    toggle.Tap();
                    mRtcEngine.EnableLocalAudio(false);
                    mRtcEngine.MuteLocalAudioStream(true);
                });
                toggle.button2.onClick.AddListener(() =>
                {
                    toggle.Tap();
                    mRtcEngine.EnableLocalAudio(true);
                    mRtcEngine.MuteLocalAudioStream(false);
                });
            }
        }
    }

    public void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == AudienceSceneName || scene.name == BroadcastSceneName)
        {
            if (!ReferenceEquals(app, null))
            {
                app.OnSceneLoaded(); // call this after scene is loaded
            }

            GameObject go = GameObject.Find("ButtonExit");
            if (go != null)
            {
                Button button = go.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(OnLeaveButtonClicked);
                }
            }
            SetupToggleMic();
            SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        }
    }

    void OnApplicationPause(bool paused)
    {
        if (!ReferenceEquals(app, null))
        {
            app.EnableVideo(paused);
        }
    }

    void OnApplicationQuit()
    {
        IRtcEngine.Destroy();
    }
}
