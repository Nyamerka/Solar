#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static partial class ProjectSetup
{
    static Material MakeMat(Shader shader, string name, Color color)
    {
        string path = $"Assets/Materials/{name}.mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;

        var mat = new Material(shader) { color = color };
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    static Material MakeEmissiveMat(Shader shader, string name, Color color, Color emission)
    {
        string path = $"Assets/Materials/{name}.mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;

        var mat = new Material(shader) { color = color };
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", emission);
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    static void CreateMaterials()
    {
        EditorUtility.DisplayProgressBar("Solar Setup", "Материалы...", 0.10f);
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        matFloor  = MakeMat(shader, "Floor",  new Color(0.2f, 0.2f, 0.25f));
        matWall   = MakeMat(shader, "Wall",   new Color(0.35f, 0.3f, 0.28f));
        matPlayer = MakeEmissiveMat(shader, "Player", new Color(0.4f, 0.6f, 1f), new Color(0.1f, 0.15f, 0.3f));
        matEnemy  = MakeEmissiveMat(shader, "Enemy",  new Color(0.9f, 0.3f, 0.2f), new Color(0.2f, 0.05f, 0.03f));
        matPickup = MakeMat(shader, "Pickup", new Color(1f, 0.85f, 0.2f));
        matDoor   = MakeMat(shader, "Door",   new Color(0.5f, 0.35f, 0.2f));
        matAltar  = MakeEmissiveMat(shader, "Altar",  new Color(0.3f, 0.5f, 1f), new Color(0.05f, 0.1f, 0.3f));

        matPatrol = MakeEmissiveMat(shader, "Enemy_Patrol", new Color(0.85f, 0.4f, 0.15f), new Color(0.15f, 0.05f, 0f));
        matSentry = MakeEmissiveMat(shader, "Enemy_Sentry", new Color(0.6f, 0.15f, 0.15f), new Color(0.2f, 0.03f, 0.03f));
        matWraith = MakeEmissiveMat(shader, "Enemy_Wraith", new Color(0.6f, 0.7f, 0.9f), new Color(0.1f, 0.12f, 0.2f));
        matLurker = MakeEmissiveMat(shader, "Enemy_Lurker", new Color(0.3f, 0.7f, 0.2f), new Color(0.05f, 0.15f, 0.03f));

        EditorUtility.ClearProgressBar();
    }
}
#endif
