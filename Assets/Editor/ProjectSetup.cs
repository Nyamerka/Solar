#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

[InitializeOnLoad]
public static partial class ProjectSetup
{
    static Material matFloor, matWall, matPlayer, matEnemy, matPickup, matDoor, matAltar;
    static Material matPatrol, matSentry, matWraith, matLurker;

    static ProjectSetup()
    {
        EditorApplication.delayCall += () =>
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes")
                || AssetDatabase.LoadAssetAtPath<Object>("Assets/Scenes/Level01.unity") == null)
            {
                if (EditorUtility.DisplayDialog("Solar",
                    "Проект не настроен. Запустить автосборку?",
                    "Да", "Позже (Solar → меню)"))
                {
                    RunFullSetup();
                }
            }
        };
    }

    [MenuItem("Solar/Setup Entire Project (One Click)")]
    public static void RunFullSetup()
    {
        if (!EditorUtility.DisplayDialog("Solar",
            "Будут созданы:\n• URP\n• Материалы\n• Префабы\n• Сцены (MainMenu, Level01, Level02, Level03)\n• NavMesh\n• Build Settings\n\nПродолжить?",
            "Да", "Отмена"))
            return;

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        EnsureDirectories();
        SetupURP();
        CreateMaterials();
        CreatePrefabs();
        CreateMainMenuScene();
        CreateLevel01Scene();
        CreateLevel02Scene();
        CreateLevel03Scene();
        SetupBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorSceneManager.OpenScene("Assets/Scenes/Level01.unity");

        EditorUtility.DisplayDialog("Solar",
            "Готово! Нажми Play для запуска Level01.\n\n" +
            "Если Unity попросит переключить Input System — нажми Yes.", "OK");
    }

    static void EnsureDirectories()
    {
        string[] dirs = {
            "Assets/Settings", "Assets/Materials", "Assets/Prefabs",
            "Assets/Scenes", "Assets/Resources"
        };
        foreach (var d in dirs)
            if (!AssetDatabase.IsValidFolder(d))
                Directory.CreateDirectory(d);
        AssetDatabase.Refresh();
    }

    static void SetupURP()
    {
        EditorUtility.DisplayProgressBar("Solar Setup", "URP...", 0.05f);

        UniversalRenderPipelineAsset pipelineAsset = null;
        var guids = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
        if (guids.Length > 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            pipelineAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
        }

        if (pipelineAsset == null)
        {
            var rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(rendererData, "Assets/Settings/URP_Renderer.asset");

            var createMethod = typeof(UniversalRenderPipelineAsset).GetMethod("Create",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public,
                null, new[] { typeof(ScriptableRendererData) }, null);

            if (createMethod != null)
            {
                pipelineAsset = createMethod.Invoke(null, new object[] { rendererData })
                    as UniversalRenderPipelineAsset;
            }
            else
            {
                pipelineAsset = ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>();
                var so = new SerializedObject(pipelineAsset);
                var rendererList = so.FindProperty("m_RendererDataList");
                if (rendererList != null)
                {
                    rendererList.arraySize = 1;
                    rendererList.GetArrayElementAtIndex(0).objectReferenceValue = rendererData;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            if (pipelineAsset != null)
            {
                pipelineAsset.shadowDistance = 30f;
                AssetDatabase.CreateAsset(pipelineAsset, "Assets/Settings/URPSettings.asset");
            }
        }

        if (pipelineAsset == null)
        {
            Debug.LogError("[Solar] Could not find or create URP pipeline asset.");
            EditorUtility.ClearProgressBar();
            return;
        }

        GraphicsSettings.defaultRenderPipeline = pipelineAsset;
        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            QualitySettings.SetQualityLevel(i, false);
            QualitySettings.renderPipeline = pipelineAsset;
        }

        EditorUtility.ClearProgressBar();
    }

    static void SetupBuildSettings()
    {
        EditorUtility.DisplayProgressBar("Solar Setup", "Build Settings...", 0.95f);

        var scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Level01.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Level02.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Level03.unity", true),
        };
        EditorBuildSettings.scenes = scenes;
        EditorUtility.ClearProgressBar();
    }
}
#endif
