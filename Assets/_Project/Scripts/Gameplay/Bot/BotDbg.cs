using UnityEngine;

public static class BotDbg
{
    public static bool Enabled = true;

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Log(Transform who, string fmt, params object[] args)
    {
        if (!Enabled || who == null) return;
        Debug.Log($"[BOT:{who.name}] " + string.Format(fmt, args));
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Draw(Vector3 a, Vector3 b, Color c, float t = 0f)
    {
        if (!Enabled) return;
        Debug.DrawLine(a, b, c, t);
    }
}