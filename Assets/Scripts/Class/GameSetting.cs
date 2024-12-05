using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GameSetting : MonoBehaviour
{
    public HostName currentHostName = HostName.dev;
    public string currentURL;
    public GameSetup gameSetup;
    public APIManager apiManager;
    public delegate void ParameterHandler(string value);
    protected private Dictionary<string, ParameterHandler> customHandlers = new Dictionary<string, ParameterHandler>();
    public string unitKey = string.Empty;
    public string testURL = string.Empty;

    protected virtual void GetParseURLParams()
    {
        this.CurrentURL = string.IsNullOrEmpty(Application.absoluteURL) ? this.testURL : Application.absoluteURL;
        string[] urlParts = this.CurrentURL.Split('?');
        if (urlParts.Length > 1)
        {
            string queryString = urlParts[1];
            string[] parameters = queryString.Split('&');

            foreach (string parameter in parameters)
            {
                string[] keyValue = parameter.Split('=');
                if (keyValue.Length == 2)
                {
                    string key = keyValue[0];
                    string value = keyValue[1];
                    LogController.Instance?.debug($"Parameter Key: {key}, Value: {value}");

                    if (!string.IsNullOrEmpty(value))
                    {

                        switch (key)
                        {
                            case "jwt":
                                this.apiManager.jwt = value;
                                LogController.Instance?.debug("Current jwt: " + this.apiManager.jwt);
                                break;
                            case "id":
                                this.apiManager.appId = value;
                                LogController.Instance?.debug("Current app/book id: " + this.apiManager.appId);
                                break;
                            case "unit":
                                this.unitKey = value;
                                LogController.Instance?.debug("Current Game Unit: " + this.unitKey);
                                break;
                            case "gameTime":
                                this.GameTime = float.Parse(value);
                                LogController.Instance?.debug("Game Time: " + this.GameTime);
                                this.ShowFPS = true;
                                break;
                            case "playerNumbers":
                                this.PlayerNumbers = int.Parse(value);
                                LogController.Instance?.debug("player Numbers: " + this.PlayerNumbers);
                                break;
                            default:
                                if (this.customHandlers.TryGetValue(key, out ParameterHandler handler))
                                {
                                    handler(value);
                                }
                                break;
                        }
                    }
                }
            }
        }
    }

    public void RegisterCustomHandler(string key, ParameterHandler handler)
    {
        if (!this.customHandlers.ContainsKey(key))
        {
            this.customHandlers[key] = handler;
        }
    }

    protected virtual void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        Application.runInBackground = true;
        DontDestroyOnLoad(this);
    }

    protected virtual void Start()
    {
        this.apiManager.Init();
    }

    protected virtual void Update()
    {
        this.apiManager.controlDebugLayer();
    }

    public void InitialGameImages(Action onCompleted = null)
    {
        if (this.apiManager.IsLogined)
        {
            this.initialGameImagesByAPI(onCompleted);
        }
        else
        {
            this.initialGameImagesByLocal(onCompleted);
        }
    }

    private void initialGameImagesByLocal(Action onCompleted = null)
    {
        //Download game background image from local streaming assets
        this.gameSetup.loadImageMethod = LoadImageMethod.StreamingAssets;
        StartCoroutine(this.gameSetup.Load("GameUI", "bg", _bgTexture =>
        {
            LogController.Instance?.debug($"Downloaded bg Image!!");
            ExternalCaller.UpdateLoadBarStatus("Loading Bg");
            if(_bgTexture != null) this.gameSetup.bgTexture = _bgTexture;

            StartCoroutine(this.gameSetup.Load("GameUI", "preview", _previewTexture =>
            {
                LogController.Instance?.debug($"Downloaded preview Image!!");
                ExternalCaller.UpdateLoadBarStatus("Loading Instruction");
                if(_previewTexture != null) this.gameSetup.previewTexture = _previewTexture;
                onCompleted?.Invoke();
            }));
        }));
    }

    private void initialGameImagesByAPI(Action onCompleted = null)
    {
        //Download game background image from api
        this.gameSetup.loadImageMethod = LoadImageMethod.Url;
        var imageUrls = new List<string>
        {
            this.apiManager.settings.backgroundImageUrl,
            this.apiManager.settings.previewGameImageUrl,
            this.apiManager.settings.frameImageUrl_P1,
            this.apiManager.settings.frameImageUrl_P2,
            this.apiManager.settings.grid_image
        };
        imageUrls = imageUrls.Where(url => !string.IsNullOrEmpty(url)).ToList();

        if (imageUrls.Count > 0)
        {
            StartCoroutine(LoadImages(imageUrls, onCompleted));
        }
        else
        {
            LogController.Instance?.debug($"No valid image URLs found!!");
            onCompleted?.Invoke();
        }
    }

    private IEnumerator LoadImages(List<string> imageUrls, Action onCompleted)
    {
        foreach (var url in imageUrls)
        {
            Texture texture = null;
            // Load each image
            yield return StartCoroutine(this.gameSetup.Load("", url, _texture =>
            {
                texture = _texture;
                LogController.Instance?.debug($"Downloaded image from: {url}");
                ExternalCaller.UpdateLoadBarStatus($"Loading SetupUI");
            }));

            // Assign textures based on their URL
            if (url == this.apiManager.settings.backgroundImageUrl)
            {
                this.gameSetup.bgTexture = texture != null ? texture : null;
            }
            else if (url == this.apiManager.settings.previewGameImageUrl)
            {
                this.gameSetup.previewTexture = texture != null ? texture : null;
            }
            else if (url == this.apiManager.settings.frameImageUrl_P1)
            {
                this.gameSetup.frameTexture_p1 = texture != null ? texture : null;
            }
            else if (url == this.apiManager.settings.frameImageUrl_P2)
            {
                this.gameSetup.frameTexture_p2 = texture != null ? texture : null;
            }
            else if (url == this.apiManager.settings.grid_image)
            {
                this.gameSetup.gridTexture = texture != null ? texture : null;
            }
        }

        onCompleted?.Invoke();
    }

    public void InitialGameSetup()
    {
        this.gameSetup.setBackground();
        this.gameSetup.setInstruction(this.apiManager.settings.instructionContent);
    }
    public string CurrentURL
    {
        set { this.currentURL = value; }
        get { return this.currentURL; }
    }

    public float GameTime
    {
        get { return this.gameSetup.gameTime; }
        set { this.gameSetup.gameTime = value; }
    }

    public bool ShowFPS
    {
        get { return this.gameSetup.showFPS; }
        set { this.gameSetup.showFPS = value; }
    }

    public int PlayerNumbers
    {
        get { return this.gameSetup.playerNumber; }
        set { this.gameSetup.playerNumber = value; }
    }

    public string CurrentHostName
    {
        get
        {
            return currentHostName switch
            {
                HostName.dev => "https://dev.openknowledge.hk",
                HostName.prod => "https://www.rainbowone.app/",
                _ => throw new NotImplementedException()
            };
        }
    }

    public void Reload()
    {
        ExternalCaller.ReLoadCurrentPage();
    }
}

[Serializable]
public class GameSetup : LoadImage
{
    [Tooltip("Default Game Background Texture")]
    public Texture bgTexture;
    [Tooltip("Default Game Preview Texture")]
    public Texture previewTexture;
    [Tooltip("Default P1 Frame Texture")]
    public Texture frameTexture_p1;
    [Tooltip("Default P2 Frame Texture")]
    public Texture frameTexture_p2;
    [Tooltip("Default grid Texture")]
    public Texture gridTexture;
    [Tooltip("Find Tag name of GameBackground in different scene")]
    public RawImage gameBackground;
    [Tooltip("Instruction Preview Image")]
    public RawImage gamePreview;
    public InstructionText instructions;
    public float gameTime;
    public bool showFPS = false;
    public int playerNumber = 1;
    public GridWordFormat gridWordFormat = GridWordFormat.AllUpper;
    public Color32 gridNormalColor = default;
    public Color32 gridPressedColor = default;

    public void setBackground()
    {
        if (this.gameBackground == null)
        {
            var tex = GameObject.FindGameObjectWithTag("GameBackground");
            this.gameBackground = tex.GetComponent<RawImage>();
        }

        if (this.gameBackground != null)
        {
            this.gameBackground.texture = this.bgTexture;
        }
    }

    public void setInstruction(string content = "")
    {
        if (!string.IsNullOrEmpty(content) && this.instructions == null)
        {
            var instructionText = GameObject.FindGameObjectWithTag("Instruction");
            this.instructions = instructionText != null ? instructionText.GetComponent<InstructionText>() : null;
            if (instructionText != null) this.instructions.setContent(content);
        }

        if (this.gamePreview == null)
        {
            var preview = GameObject.FindGameObjectWithTag("GamePreview");
            this.gamePreview = preview != null ? preview.GetComponent<RawImage>() : null;
            if (preview != null) this.gamePreview.texture = this.previewTexture;
        }
    }
}


public enum HostName
{
    dev,
    prod
}