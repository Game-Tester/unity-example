using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public enum GameTesterMode { Production, Sandbox }
public enum GameTesterPlayerAuthenticationMode { ConnectToken, Pin }

public static class GameTester
{
    // ------------------------------------------------------------------------------------------------------ //
    // Constructor
    // ------------------------------------------------------------------------------------------------------ //
    static GameTester()
    {
        Initialized = false;
        Mode = GameTesterMode.Sandbox;
        PlayerAuthenticated = false;
        PlayerAuthenticationMode = GameTesterPlayerAuthenticationMode.Pin;
    }

    // ------------------------------------------------------------------------------------------------------ //
    // Static Data
    // ------------------------------------------------------------------------------------------------------ //
    private static Dictionary<GameTesterMode, string> serverUrls = new Dictionary<GameTesterMode, string>
    {
        //{ GameTesterMode.Production, "https://server.gametester.gg/dev-api/v1" },
        { GameTesterMode.Sandbox, "https://server.gametester.gg/dev-api/v1/sandbox" }

        //{ GameTesterMode.Production, "http://localhost:5001/dev-api/v1" },
        //{ GameTesterMode.Sandbox, "http://localhost:5001/dev-api/v1/sandbox" }
    };
    private static string serverUrl { get { return serverUrls[Mode]; } }

    // ------------------------------------------------------------------------------------------------------ //
    // Properties
    // ------------------------------------------------------------------------------------------------------ //
    public static bool Initialized { get; private set; }
    public static GameTesterMode Mode { get; private set; }

    public static bool PlayerAuthenticated { get; private set; }
    public static GameTesterPlayerAuthenticationMode PlayerAuthenticationMode { get; private set; }
    public static string PlayerName { get; private set; }

    // ------------------------------------------------------------------------------------------------------ //
    // Private Fields
    // ------------------------------------------------------------------------------------------------------ //
    private static string developerToken;
    private static string connectTokenOrPin;
    private static string playerToken;

    // ------------------------------------------------------------------------------------------------------ //
    // Initialize
    // ------------------------------------------------------------------------------------------------------ //
    public static void Initialize(GameTesterMode mode, string developerToken)
    {
        Mode = mode;
        GameTester.developerToken = developerToken;
        Initialized = true;
    }

    // ------------------------------------------------------------------------------------------------------ //
    // Private Helper Methods
    // ------------------------------------------------------------------------------------------------------ //
    private static Dictionary<string, object> createApiObject()
    {
        var obj = new Dictionary<string, object>();

        obj.Add("developerToken", developerToken);
        obj.Add("playerToken", playerToken);

        return obj;
    }

    private static IEnumerator doPost<T>(string subUrl, Dictionary<string, object> body, Action<T> callback, Func<UnityWebRequest, T> parser)
    {
        using (var request = new UnityWebRequest(String.Format("{0}{1}", serverUrl, subUrl), "POST"))
        {
            var sb = new StringBuilder();
            sb.Append('{');
            int index = 0;
            foreach (var prop in body)
            {
                sb.Append('"');
                sb.Append(prop.Key);
                sb.Append('"');

                sb.Append(':');
                if (prop.Value is string)
                {
                    sb.Append('"');
                    sb.Append(prop.Value);
                    sb.Append('"');
                }
                else
                {
                    sb.Append(prop.Value);
                }

                if (index < body.Count - 1)
                {
                    sb.Append(',');
                }
                index++;
            }
            sb.Append('}');

            var json = sb.ToString();
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            var response = parser(request);
            callback(response);
        }
    }

    // ------------------------------------------------------------------------------------------------------ //
    // Public Helper Methods
    // ------------------------------------------------------------------------------------------------------ //
    public static void SetPlayerPin(string pin)
    {
        connectTokenOrPin = pin;
        PlayerAuthenticationMode = GameTesterPlayerAuthenticationMode.Pin;
    }

    public static void SetPlayerToken(string token)
    {
        connectTokenOrPin = token;
        PlayerAuthenticationMode = GameTesterPlayerAuthenticationMode.ConnectToken;
    }

    // ------------------------------------------------------------------------------------------------------ //
    // Api
    // ------------------------------------------------------------------------------------------------------ //
    public static class Api
    {
        public static IEnumerator Auth(Action<GameTesterAuthResponse> callback)
        {
            var obj = new Dictionary<string, object>();

            obj.Add("developerToken", developerToken);

            if (PlayerAuthenticationMode == GameTesterPlayerAuthenticationMode.Pin)
                obj.Add("playerPin", connectTokenOrPin);
            else
                obj.Add("connectToken", connectTokenOrPin);

            return doPost("/auth", obj, o => {
                if (o.Code == GameTesterResponseCode.Success)
                {
                    playerToken = o.PlayerToken;
                    PlayerName = o.PlayerName;
                    PlayerAuthenticated = true;
                }
                callback(o);
            }, GameTesterAuthResponse.Parse);
        }

        public static IEnumerator Datapoint(int datapointId, Action<GameTesterResponse> callback)
        {
            var body = createApiObject();
            body.Add("datapointId", datapointId);
            return doPost(string.Empty, body, callback, GameTesterResponse.Parse);
        }

        public static IEnumerator UnlockTest(Action<GameTesterResponse> callback)
        {
            var body = createApiObject();
            return doPost("/unlock", body, callback, GameTesterResponse.Parse);
        }
    }

}

// ------------------------------------------------------------------------------------------------------ //
// Response
// ------------------------------------------------------------------------------------------------------ //

public enum GameTesterResponseCode : int
{
    HttpError = -10,
    ResponseParseError = -11,

    Success = -1,

    GeneralError = 0,

    MissingDeveloperToken = 1,
    MissingPlayerAuthentication = 2,
    InvalidDeveloperToken = 3,
    InvalidPlayerConnectToken = 4,
    InvalidPlayerPin = 5,
    MissingDatapoint = 6,
    DataPointDoesNotExist = 7,
    TestNotRunning = 8,
    InvalidPlayerForTest = 9,
    TestAlreadyUnlocked = 10,
    TestNotInSetupState = 11,
    MissingPlayerToken = 12,
    InvalidPlayerToken = 13,
}

public struct GameTesterResponse
{
    public GameTesterResponseCode Code { get; private set; }
    public string Message { get; private set; }

    public static GameTesterResponse Parse(UnityWebRequest request)
    {
        if (request.isNetworkError || request.isHttpError)
            return GameTesterResponse.HttpError(request.error);
        else
            return GameTesterResponse.ParseResponse(request.downloadHandler.text);
    }

    public static GameTesterResponse ParseResponse(string webResult)
    {
        try
        {
            var response = JsonUtility.FromJson<ResponseJson>(webResult);
            return new GameTesterResponse
            {
                Code = (GameTesterResponseCode)response.code,
                Message = response.message
            };
        }
        catch (Exception e)
        {
            return new GameTesterResponse
            {
                Code = GameTesterResponseCode.ResponseParseError,
                Message = e.Message
            };
        }
    }

    public static GameTesterResponse HttpError(string error)
    {
        return new GameTesterResponse
        {
            Code = GameTesterResponseCode.HttpError,
            Message = error
        };
    }

    [System.Serializable]
    public class ResponseJson
    {
        public int code;
        public string message;
    }

    public override string ToString() { return String.Format("[({0}){1}] {2}", (int)Code, Enum.GetName(typeof(GameTesterResponseCode), Code), Message); }
}

public struct GameTesterAuthResponse
{
    public GameTesterResponseCode Code { get; private set; }
    public string Message { get; private set; }
    public string PlayerToken { get; private set; }
    public string PlayerName { get; private set; }

    public static GameTesterAuthResponse Parse(UnityWebRequest request)
    {
        if (request.isNetworkError || request.isHttpError)
            return GameTesterAuthResponse.HttpError(request.error);
        else
            return GameTesterAuthResponse.ParseResponse(request.downloadHandler.text);
    }

    public static GameTesterAuthResponse ParseResponse(string webResult)
    {
        try
        {
            var response = JsonUtility.FromJson<ResponseJson>(webResult);
            return new GameTesterAuthResponse
            {
                Code = (GameTesterResponseCode)response.code,
                Message = response.message,
                PlayerToken = response.playerToken,
                PlayerName = response.playerName,
            };
        }
        catch (Exception e)
        {
            return new GameTesterAuthResponse
            {
                Code = GameTesterResponseCode.ResponseParseError,
                Message = e.Message,
                PlayerToken = null,
                PlayerName = null
            };
        }
    }

    public static GameTesterAuthResponse HttpError(string error)
    {
        return new GameTesterAuthResponse
        {
            Code = GameTesterResponseCode.HttpError,
            Message = error
        };
    }

    [System.Serializable]
    public class ResponseJson
    {
        public int code;
        public string message;
        public string playerToken;
        public string playerName;
    }

    public override string ToString() { return String.Format("[({0}){1}] PlayerName: {2}, {3}", (int)Code, Enum.GetName(typeof(GameTesterResponseCode), Code), PlayerName, Message); }
}
