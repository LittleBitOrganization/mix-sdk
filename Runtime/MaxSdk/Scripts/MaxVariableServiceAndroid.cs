using UnityEngine;
namespace AppLovinMax
{

public class MaxVariableServiceAndroid
{
    private static readonly AndroidJavaClass _maxUnityPluginClass = new AndroidJavaClass("com.applovin.mediation.unity.MaxUnityPlugin");
    private static readonly MaxVariableServiceAndroid _instance = new MaxVariableServiceAndroid();

    public static MaxVariableServiceAndroid Instance
    {
        get { return _instance; }
    }

    /// <summary>
    /// Explicitly retrieve the latest variables from the server.
    /// Please make sure to implement the callback <see cref="MaxSdkCallbacks.OnVariablesUpdatedEvent"/>.
    /// </summary>
    public void LoadVariables()
    {
        _maxUnityPluginClass.CallStatic("loadVariables");
    }

    /// <summary>
    /// Returns the variable value associated with the given key, or false if no mapping of the desired type exists for the given key.
    /// </summary>
    /// <param name="key">The variable name to retrieve the value for.</param>
    public bool GetBoolean(string key, bool defaultValue = false)
    {
        return _maxUnityPluginClass.CallStatic<bool>("getBoolean", key, defaultValue);
    }

    /// <summary>
    /// Returns the variable value associated with the given key, or an empty string if no mapping of the desired type exists for the given key.
    /// </summary>
    /// <param name="key">The variable name to retrieve the value for.</param>
    public string GetString(string key, string defaultValue = "")
    {
        return _maxUnityPluginClass.CallStatic<string>("getString", key, defaultValue);
    }
}
}
