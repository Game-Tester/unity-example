using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    [SerializeField]
    private UnityEngine.UI.Text ScoreDisplay;
    [SerializeField]
    private UnityEngine.UI.Image LivesDisplay;
    [SerializeField]
    private GameObject StartDisplay;
    [SerializeField]
    private GameObject LoginUI;
    [SerializeField]
    private GameObject UnlockSplash;
    [SerializeField]
    private GameObject PlayerPrefab;
    [SerializeField]
    private Sprite[] LiveImages;

    [SerializeField]
    private UnityEngine.UI.InputField UserPin;
    [SerializeField]
    private UnityEngine.UI.Button TestAuthButton;

    // private
    private int score = 0;
    private int pickups = 0;
    private SpawnManager _spawner;
    private SpawnManager spawner
    {
        get
        {
            if (_spawner == null)
                _spawner = FindObjectOfType<SpawnManager>();
            return _spawner;
        }
    }

    // events
    void Update()
    {
        if (Input.GetAxis("Exit") > 0)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    // public
    public void StartGame()
    {
        if (!spawner.Running)
        {
            pickups = 0;
            score = 0;
            UpdateScore(0);
            Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity);
            spawner.Running = true;

            StartDisplay.SetActive(false);
            LoginUI.SetActive(false);
            UnlockSplash.SetActive(false);

            InitializeGameTester();

            UserPin.gameObject.SetActive(false);
            TestAuthButton.gameObject.SetActive(false);
        }
    }

    public void UpdateLives(int current)
    {
        LivesDisplay.sprite = LiveImages[current];   
    }

    public void UpdateScore(int addition)
    {
        score += addition;
        ScoreDisplay.text = $"Score: {score}";

        // Call the /unlock endpoint once all criteria for test completion has been met.
        if (score == 3)
            StartCoroutine(GameTester.Api.UnlockTest(o => UnlockTest()));
    }

    public void UnlockTest() {
        UnlockSplash.SetActive(true);
        Time.timeScale = 0;
    }

    public void UpdatePickups(int addition)
    {
        pickups += addition;

        // Call the / endpoint and pass the data point id to log the data point
        if (pickups == 1)
            StartCoroutine(GameTester.Api.Datapoint(1, o => UnityEngine.Debug.Log(o)));
    }

    public void ShowStart()
    {
        StartDisplay.SetActive(true);
    }

    public void TestAuth()
    {
        // Plugin requires a bit of initialization. See the GameTester.cs file for more info
        InitializeGameTester();

        var playerPin = UserPin.text ?? string.Empty;

        if (playerPin.Length > 0)
            GameTester.SetPlayerPin(playerPin);

        // Call the /auth endpoint and pass the player pin or connectToken. If successful a player token will be returned and is used for data point and unlock calls
        StartCoroutine(GameTester.Api.Auth(o => 
        {
            if (o.Code == GameTesterResponseCode.Success)
            {
                StartGame();
            }
        }));
    }

    // private
    private void InitializeGameTester()
    {
        GameTesterMode mode = GameTesterMode.Sandbox;

        GameTester.Initialize(mode, "validToken");
    }

}
