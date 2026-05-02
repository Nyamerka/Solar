#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.AI.Navigation;
using TMPro;
using System.IO;

[InitializeOnLoad]
public static class ProjectSetup
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
            "Будут созданы:\n• URP\n• Материалы\n• Префабы\n• Сцены (MainMenu, Level01, Level02)\n• NavMesh\n• Build Settings\n\nПродолжить?",
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

    static void CreatePrefabs()
    {
        EditorUtility.DisplayProgressBar("Solar Setup", "Префабы...", 0.20f);

        CreatePlayerPrefab();
        CreateEnemyPrefab("Enemy_Patrol",  EnemyType.Patrol,  1.8f, 3.5f, 60f, 6f, 6f, 1);
        CreateEnemyPrefab("Enemy_Sentry",  EnemyType.Sentry,  1.8f, 2.5f, 90f, 8f, 8f, 1);
        CreateEnemyPrefab("Enemy_Wraith",  EnemyType.Wraith,  1.5f, 2.0f, 0f,  0f, 10f, 2);
        CreateEnemyPrefab("Enemy_Lurker",  EnemyType.Lurker,  2.0f, 4.5f, 30f, 10f, 4f, 1);
        CreatePickupPrefab<ArtifactPickup>("Pickup_Artifact",  new Color(1f, 0.9f, 0.3f));
        CreatePickupPrefab<EnergyPickup>("Pickup_Energy",      new Color(0.3f, 1f, 0.5f));
        CreatePickupPrefab<KeyPickup>("Pickup_Key",            new Color(1f, 0.7f, 0f));
        CreatePickupPrefab<UpgradePickup>("Pickup_Upgrade",    new Color(0.6f, 0.3f, 1f));
        CreateLightEssencePrefab();

        EditorUtility.ClearProgressBar();
    }

    static void CreatePlayerPrefab()
    {
        string path = "Assets/Prefabs/Player.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

        var player = new GameObject("Player") { tag = "Player" };

        var torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        torso.name = "Torso";
        torso.transform.SetParent(player.transform);
        torso.transform.localPosition = new Vector3(0, 0.9f, 0);
        torso.transform.localScale = new Vector3(0.6f, 0.7f, 0.5f);
        torso.GetComponent<Renderer>().sharedMaterial = matPlayer;
        Object.DestroyImmediate(torso.GetComponent<Collider>());

        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(player.transform);
        head.transform.localPosition = new Vector3(0, 1.65f, 0);
        head.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        head.GetComponent<Renderer>().sharedMaterial = matPlayer;
        Object.DestroyImmediate(head.GetComponent<Collider>());

        var lantern = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lantern.name = "Lantern";
        lantern.transform.SetParent(player.transform);
        lantern.transform.localPosition = new Vector3(0.35f, 1.1f, 0.2f);
        lantern.transform.localScale = new Vector3(0.15f, 0.25f, 0.15f);
        var lanternMat = new Material(matPlayer.shader);
        lanternMat.color = new Color(1f, 0.9f, 0.5f);
        lanternMat.EnableKeyword("_EMISSION");
        lanternMat.SetColor("_EmissionColor", new Color(0.4f, 0.35f, 0.1f));
        AssetDatabase.CreateAsset(lanternMat, "Assets/Materials/Lantern.mat");
        lantern.GetComponent<Renderer>().sharedMaterial = lanternMat;
        Object.DestroyImmediate(lantern.GetComponent<Collider>());

        var rb = player.AddComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        var col = player.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0, 1f, 0);
        col.height = 2f;
        col.radius = 0.4f;

        player.AddComponent<PlayerController>();
        player.AddComponent<EnergyComponent>();
        player.AddComponent<PlayerHealth>();
        player.AddComponent<PlayerNoise>();
        player.AddComponent<PlayerInventory>();

        var haloGO = new GameObject("PlayerLight");
        haloGO.transform.SetParent(player.transform);
        haloGO.transform.localPosition = new Vector3(0, 0.5f, 0);
        var haloLight = haloGO.AddComponent<Light>();
        haloLight.type = LightType.Point;
        haloLight.range = 4f;
        haloLight.intensity = 2.5f;
        haloLight.color = new Color(1f, 0.9f, 0.7f);

        var flashGO = new GameObject("Flashlight");
        flashGO.transform.SetParent(player.transform);
        flashGO.transform.localPosition = new Vector3(0, 1f, 0.5f);
        var spotLight = flashGO.AddComponent<Light>();
        spotLight.type = LightType.Spot;
        spotLight.range = 8f;
        spotLight.spotAngle = 45f;
        spotLight.intensity = 3f;
        spotLight.enabled = false;
        flashGO.AddComponent<Flashlight>();

        var burstGO = new GameObject("BurstLight");
        burstGO.transform.SetParent(player.transform);
        burstGO.transform.localPosition = new Vector3(0, 1f, 0);
        var burstLight = burstGO.AddComponent<Light>();
        burstLight.type = LightType.Point;
        burstLight.range = 6f;
        burstLight.intensity = 5f;
        burstLight.enabled = false;

        var prefab = PrefabUtility.SaveAsPrefabAsset(player, path);
        Object.DestroyImmediate(player);

        var so = new SerializedObject(prefab.GetComponentInChildren<Flashlight>());
        SetRef(so, "spotLight", prefab.transform.Find("Flashlight").GetComponent<Light>());
        SetRef(so, "burstLight", prefab.transform.Find("BurstLight").GetComponent<Light>());
        SetRef(so, "energy", prefab.GetComponent<EnergyComponent>());
        SetLayer(so, "enemyLayer", 8);
        SetLayer(so, "wallLayer", 9);
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(prefab);
    }

    static void CreateEnemyPrefab(string name, EnemyType type,
        float patrolSpeed, float chaseSpeed,
        float viewAngle, float viewDist, float lightHP, int contactDamage)
    {
        string path = $"Assets/Prefabs/{name}.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

        var go = new GameObject(name);
        go.layer = 8;

        BuildEnemyVisual(go, type);

        var col = go.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0, 1f, 0);
        col.height = 2f;
        col.radius = 0.4f;

        var agent = go.AddComponent<NavMeshAgent>();
        agent.speed = chaseSpeed;
        agent.stoppingDistance = 1f;

        go.AddComponent<EnemyAI>();
        go.AddComponent<EnemyLightSensor>();

        if (type != EnemyType.Wraith)
            go.AddComponent<EnemyPatrol>();

        if (type != EnemyType.Wraith && viewAngle > 0)
            go.AddComponent<EnemyVision>();

        if (type == EnemyType.Wraith)
        {
            go.AddComponent<EnemyHearing>();
            go.AddComponent<EnemyLightVisibility>();
        }

        go.AddComponent<EnemyLightHealth>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);

        var aiSO = new SerializedObject(prefab.GetComponent<EnemyAI>());
        aiSO.FindProperty("enemyType").enumValueIndex = (int)type;
        aiSO.FindProperty("patrolSpeed").floatValue = patrolSpeed;
        aiSO.FindProperty("chaseSpeed").floatValue = chaseSpeed;
        aiSO.FindProperty("contactDamage").intValue = contactDamage;

        var essencePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pickup_LightEssence.prefab");
        if (essencePrefab == null) essencePrefab = CreateLightEssencePrefab();
        if (essencePrefab != null)
            SetRef(aiSO, "lightEssencePrefab", essencePrefab);

        aiSO.ApplyModifiedPropertiesWithoutUndo();

        if (viewAngle > 0)
        {
            var vis = prefab.GetComponent<EnemyVision>();
            if (vis != null)
            {
                var visSO = new SerializedObject(vis);
                visSO.FindProperty("viewAngle").floatValue = viewAngle;
                visSO.FindProperty("viewDistance").floatValue = viewDist;
                SetLayer(visSO, "wallMask", 9);
                visSO.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        var healthSO = new SerializedObject(prefab.GetComponent<EnemyLightHealth>());
        healthSO.FindProperty("lightHP").floatValue = lightHP;
        healthSO.ApplyModifiedPropertiesWithoutUndo();

        if (type == EnemyType.Wraith)
        {
            var hearSO = new SerializedObject(prefab.GetComponent<EnemyHearing>());
            hearSO.FindProperty("hearingRange").floatValue = 10f;
            hearSO.FindProperty("noiseThreshold").floatValue = 0.3f;
            hearSO.ApplyModifiedPropertiesWithoutUndo();
        }

        EditorUtility.SetDirty(prefab);
    }

    static void BuildEnemyVisual(GameObject root, EnemyType type)
    {
        Material mat;
        switch (type)
        {
            case EnemyType.Sentry: mat = matSentry; break;
            case EnemyType.Wraith: mat = matWraith; break;
            case EnemyType.Lurker: mat = matLurker; break;
            default: mat = matPatrol; break;
        }

        if (mat == null) mat = matEnemy;

        switch (type)
        {
            case EnemyType.Patrol:
            {
                var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = "Body"; body.transform.SetParent(root.transform);
                body.transform.localPosition = new Vector3(0, 0.85f, 0);
                body.transform.localScale = new Vector3(0.5f, 0.7f, 0.5f);
                body.GetComponent<Renderer>().sharedMaterial = mat;
                Object.DestroyImmediate(body.GetComponent<Collider>());

                var skull = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                skull.name = "Skull"; skull.transform.SetParent(root.transform);
                skull.transform.localPosition = new Vector3(0, 1.6f, 0);
                skull.transform.localScale = new Vector3(0.35f, 0.4f, 0.35f);
                skull.GetComponent<Renderer>().sharedMaterial = mat;
                Object.DestroyImmediate(skull.GetComponent<Collider>());

                var lEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                lEye.name = "EyeL"; lEye.transform.SetParent(skull.transform);
                lEye.transform.localPosition = new Vector3(-0.25f, 0.1f, 0.4f);
                lEye.transform.localScale = Vector3.one * 0.2f;
                var eyeMat = new Material(mat.shader) { color = Color.red };
                eyeMat.EnableKeyword("_EMISSION");
                eyeMat.SetColor("_EmissionColor", new Color(0.8f, 0, 0));
                AssetDatabase.CreateAsset(eyeMat, "Assets/Materials/EnemyEye.mat");
                lEye.GetComponent<Renderer>().sharedMaterial = eyeMat;
                Object.DestroyImmediate(lEye.GetComponent<Collider>());

                var rEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rEye.name = "EyeR"; rEye.transform.SetParent(skull.transform);
                rEye.transform.localPosition = new Vector3(0.25f, 0.1f, 0.4f);
                rEye.transform.localScale = Vector3.one * 0.2f;
                rEye.GetComponent<Renderer>().sharedMaterial = eyeMat;
                Object.DestroyImmediate(rEye.GetComponent<Collider>());
                break;
            }

            case EnemyType.Sentry:
            {
                var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                body.name = "Body"; body.transform.SetParent(root.transform);
                body.transform.localPosition = new Vector3(0, 0.8f, 0);
                body.transform.localScale = new Vector3(0.7f, 1.2f, 0.5f);
                body.GetComponent<Renderer>().sharedMaterial = mat;
                Object.DestroyImmediate(body.GetComponent<Collider>());

                var head = GameObject.CreatePrimitive(PrimitiveType.Cube);
                head.name = "Head"; head.transform.SetParent(root.transform);
                head.transform.localPosition = new Vector3(0, 1.6f, 0);
                head.transform.localScale = new Vector3(0.5f, 0.4f, 0.45f);
                head.GetComponent<Renderer>().sharedMaterial = mat;
                Object.DestroyImmediate(head.GetComponent<Collider>());

                var visor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                visor.name = "Visor"; visor.transform.SetParent(head.transform);
                visor.transform.localPosition = new Vector3(0, -0.1f, 0.45f);
                visor.transform.localScale = new Vector3(0.7f, 0.25f, 0.15f);
                var visorMat = new Material(mat.shader) { color = new Color(1f, 0.3f, 0.1f) };
                visorMat.EnableKeyword("_EMISSION");
                visorMat.SetColor("_EmissionColor", new Color(0.5f, 0.1f, 0f));
                AssetDatabase.CreateAsset(visorMat, "Assets/Materials/SentryVisor.mat");
                visor.GetComponent<Renderer>().sharedMaterial = visorMat;
                Object.DestroyImmediate(visor.GetComponent<Collider>());
                break;
            }

            case EnemyType.Wraith:
            {
                var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = "Body"; body.transform.SetParent(root.transform);
                body.transform.localPosition = new Vector3(0, 1.1f, 0);
                body.transform.localScale = new Vector3(0.4f, 0.9f, 0.4f);
                body.GetComponent<Renderer>().sharedMaterial = mat;
                Object.DestroyImmediate(body.GetComponent<Collider>());

                var hood = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                hood.name = "Hood"; hood.transform.SetParent(root.transform);
                hood.transform.localPosition = new Vector3(0, 1.8f, -0.05f);
                hood.transform.localScale = new Vector3(0.45f, 0.5f, 0.45f);
                hood.GetComponent<Renderer>().sharedMaterial = mat;
                Object.DestroyImmediate(hood.GetComponent<Collider>());
                break;
            }

            case EnemyType.Lurker:
            {
                var body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                body.name = "Body"; body.transform.SetParent(root.transform);
                body.transform.localPosition = new Vector3(0, 0.6f, 0);
                body.transform.localScale = new Vector3(0.8f, 0.55f, 0.9f);
                body.GetComponent<Renderer>().sharedMaterial = mat;
                Object.DestroyImmediate(body.GetComponent<Collider>());

                var lLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                lLeg.name = "LegL"; lLeg.transform.SetParent(root.transform);
                lLeg.transform.localPosition = new Vector3(-0.25f, 0.3f, 0.2f);
                lLeg.transform.localScale = new Vector3(0.15f, 0.35f, 0.15f);
                lLeg.GetComponent<Renderer>().sharedMaterial = mat;
                Object.DestroyImmediate(lLeg.GetComponent<Collider>());

                var rLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                rLeg.name = "LegR"; rLeg.transform.SetParent(root.transform);
                rLeg.transform.localPosition = new Vector3(0.25f, 0.3f, 0.2f);
                rLeg.transform.localScale = new Vector3(0.15f, 0.35f, 0.15f);
                rLeg.GetComponent<Renderer>().sharedMaterial = mat;
                Object.DestroyImmediate(rLeg.GetComponent<Collider>());

                var glow = new GameObject("LurkerGlow");
                glow.transform.SetParent(root.transform);
                glow.transform.localPosition = new Vector3(0, 0.5f, 0);
                var gl = glow.AddComponent<Light>();
                gl.type = LightType.Point; gl.range = 2f; gl.intensity = 0.6f;
                gl.color = new Color(0.3f, 0.9f, 0.2f);
                break;
            }
        }
    }

    static void CreatePickupPrefab<T>(string name, Color color) where T : Pickup
    {
        string path = $"Assets/Prefabs/{name}.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = name;
        go.layer = 10;
        go.transform.localScale = Vector3.one * 0.5f;

        var renderer = go.GetComponent<Renderer>();
        var mat = new Material(matPickup) { color = color };
        AssetDatabase.CreateAsset(mat, $"Assets/Materials/{name}.mat");
        renderer.sharedMaterial = mat;

        var col = go.GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 2f;

        go.AddComponent<T>();

        var glow = new GameObject("Glow");
        glow.transform.SetParent(go.transform);
        glow.transform.localPosition = Vector3.zero;
        var light = glow.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 2f;
        light.intensity = 0.5f;
        light.color = color;

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
    }

    static GameObject CreateLightEssencePrefab()
    {
        string path = "Assets/Prefabs/Pickup_LightEssence.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "Pickup_LightEssence";
        go.layer = 10;
        go.transform.localScale = Vector3.one * 0.3f;

        var renderer = go.GetComponent<Renderer>();
        var mat = new Material(matPickup) { color = new Color(0.5f, 1f, 0.8f) };
        AssetDatabase.CreateAsset(mat, "Assets/Materials/LightEssence.mat");
        renderer.sharedMaterial = mat;

        var col = go.GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 3f;

        go.AddComponent<LightEssence>();

        var glow = new GameObject("Glow");
        glow.transform.SetParent(go.transform);
        glow.transform.localPosition = Vector3.zero;
        var light = glow.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 2f;
        light.intensity = 0.8f;
        light.color = new Color(0.5f, 1f, 0.8f);

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    static void CreateMainMenuScene()
    {
        EditorUtility.DisplayProgressBar("Solar Setup", "MainMenu...", 0.40f);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGO = new GameObject("Main Camera") { tag = "MainCamera" };
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.05f, 0.1f);
        camGO.transform.position = new Vector3(0, 0, -10);

        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        CreateTMPText("TitleText", "SOLAR", canvasGO.transform,
            new Vector2(0, 200), 72, Color.white, TextAlignmentOptions.Center);

        CreateTMPText("Subtitle", "Огни в забытых глубинах", canvasGO.transform,
            new Vector2(0, 120), 24, new Color(0.6f, 0.7f, 1f), TextAlignmentOptions.Center);

        var btn1 = CreateButton("Уровень 1", canvasGO.transform, new Vector2(0, 20));
        var btn2 = CreateButton("Уровень 2", canvasGO.transform, new Vector2(0, -50));
        var btn3 = CreateButton("Уровень 3", canvasGO.transform, new Vector2(0, -120));
        var btn4 = CreateButton("Выход",     canvasGO.transform, new Vector2(0, -190));

        var mcGO = new GameObject("MenuController");
        var mc = mcGO.AddComponent<MenuController>();
        var mcSO = new SerializedObject(mc);
        SetRef(mcSO, "level1Button", btn1.GetComponent<Button>());
        SetRef(mcSO, "level2Button", btn2.GetComponent<Button>());
        SetRef(mcSO, "level3Button", btn3.GetComponent<Button>());
        SetRef(mcSO, "quitButton",   btn4.GetComponent<Button>());
        mcSO.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
        EditorUtility.ClearProgressBar();
    }

    static void CreateLevel01Scene()
    {
        EditorUtility.DisplayProgressBar("Solar Setup", "Level01...", 0.55f);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.03f, 0.03f, 0.06f);
        RenderSettings.fog = false;

        var camGO = new GameObject("Main Camera") { tag = "MainCamera" };
        camGO.AddComponent<Camera>();
        camGO.AddComponent<TopDownCamera>();
        camGO.transform.position = new Vector3(0, 15, -8);
        camGO.transform.rotation = Quaternion.Euler(60, 0, 0);

        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        var levelRoot = new GameObject("Level");
        levelRoot.isStatic = true;

        float wh = 3.5f;
        float wt = 0.5f;

        CreateRoom(levelRoot.transform, "StartRoom",  Vector3.zero, 6, 6, wh, wt, openN: true);
        CreateRoom(levelRoot.transform, "Corridor1", new Vector3(0,0,5), 3, 4, wh, wt, openN: true, openS: true);
        CreateRoom(levelRoot.transform, "RoomA", new Vector3(0,0,10), 6, 6, wh, wt, openS: true, openN: true, openE: true);
        CreateRoom(levelRoot.transform, "SideRoom", new Vector3(5.5f,0,10), 5, 5, wh, wt, openW: true);
        CreateRoom(levelRoot.transform, "Corridor2", new Vector3(0,0,15), 3, 4, wh, wt, openN: true, openS: true);
        CreateRoom(levelRoot.transform, "RoomB", new Vector3(0,0,21), 8, 8, wh, wt, openS: true, openN: true);
        CreateRoom(levelRoot.transform, "Corridor3", new Vector3(0,0,27), 3, 4, wh, wt, openN: true, openS: true);
        CreateRoom(levelRoot.transform, "RoomC", new Vector3(0,0,32), 8, 6, wh, wt, openS: true, openN: true);
        CreateRoom(levelRoot.transform, "Corridor4", new Vector3(0,0,37), 3, 4, wh, wt, openN: true, openS: true);
        CreateRoom(levelRoot.transform, "RoomD", new Vector3(0,0,42), 6, 6, wh, wt, openS: true, openN: true);
        CreateRoom(levelRoot.transform, "Corridor5", new Vector3(0,0,47), 3, 4, wh, wt, openN: true, openS: true);
        CreateRoom(levelRoot.transform, "ExitHall", new Vector3(0,0,52), 6, 6, wh, wt, openS: true);

        CreateLevelBounds(levelRoot.transform, -5f, 9f, -4f, 56f, 4f);

        var navSurface = levelRoot.AddComponent<NavMeshSurface>();
        navSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        navSurface.BuildNavMesh();

        var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
        var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
        player.transform.position = new Vector3(0, 0.1f, 0);

        var artifactPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pickup_Artifact.prefab");
        PlacePrefab(artifactPrefab, new Vector3(0, 0.5f, 10), "Artifact_1");
        PlacePrefab(artifactPrefab, new Vector3(5.5f, 0.5f, 10), "Artifact_2");
        PlacePrefab(artifactPrefab, new Vector3(2, 0.5f, 33), "Artifact_3");
        PlacePrefab(artifactPrefab, new Vector3(0, 0.5f, 43), "Artifact_4");
        PlacePrefab(artifactPrefab, new Vector3(0, 0.5f, 53), "Artifact_5");

        var energyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pickup_Energy.prefab");
        PlacePrefab(energyPrefab, new Vector3(-2, 0.5f, 21), "Energy_1");
        PlacePrefab(energyPrefab, new Vector3(0, 0.5f, 37), "Energy_2");

        var upgradePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pickup_Upgrade.prefab");
        PlacePrefab(upgradePrefab, new Vector3(2, 0.5f, 21), "Upgrade_1");

        var sentryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Sentry.prefab");
        PlacePrefab(sentryPrefab, new Vector3(0, 0.1f, 21), "Sentry_1");

        var patrolPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Patrol.prefab");
        var patrol1 = PlacePrefab(patrolPrefab, new Vector3(-1, 0.1f, 32), "Patrol_1");
        var wp1 = new GameObject("WP1"); wp1.transform.position = new Vector3(-3, 0, 30);
        var wp2 = new GameObject("WP2"); wp2.transform.position = new Vector3(3, 0, 34);
        WirePatrolWaypoints(patrol1, new[] { wp1.transform, wp2.transform });

        var patrol2 = PlacePrefab(patrolPrefab, new Vector3(0, 0.1f, 42), "Patrol_2");
        var wp3 = new GameObject("WP3"); wp3.transform.position = new Vector3(-2, 0, 40);
        var wp4 = new GameObject("WP4"); wp4.transform.position = new Vector3(2, 0, 44);
        WirePatrolWaypoints(patrol2, new[] { wp3.transform, wp4.transform });

        var wraithPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Wraith.prefab");
        PlacePrefab(wraithPrefab, new Vector3(5.5f, 0.1f, 11), "Wraith_1");

        CreateAltarInstance(new Vector3(-3, 0, 19), "LightAltar_1");
        CreateAltarInstance(new Vector3(0, 0, 50), "LightAltar_2");

        var goalGO = new GameObject("LevelGoal");
        goalGO.transform.position = new Vector3(0, 1, 54.5f);
        var goalCol = goalGO.AddComponent<BoxCollider>();
        goalCol.isTrigger = true;
        goalCol.size = new Vector3(4, 3, 1);
        var goal = goalGO.AddComponent<LevelGoal>();
        var goalSO = new SerializedObject(goal);
        goalSO.FindProperty("requirement").enumValueIndex = (int)DoorRequirement.AllArtifacts;
        goalSO.FindProperty("requireKey").boolValue = false;
        goalSO.ApplyModifiedPropertiesWithoutUndo();

        var invSO = new SerializedObject(player.GetComponent<PlayerInventory>());
        invSO.FindProperty("artifactsRequired").intValue = 5;
        invSO.ApplyModifiedPropertiesWithoutUndo();

        CreateHUDCanvas();

        var pauseGO = new GameObject("PauseManager");
        var pausePanel = CreatePausePanel(null);
        var pauseCtrl = pauseGO.AddComponent<PauseController>();
        var pauseSO = new SerializedObject(pauseCtrl);
        SetRef(pauseSO, "pausePanel", pausePanel);
        pauseSO.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Level01.unity");
        EditorUtility.ClearProgressBar();
    }

    static void CreateLevel02Scene()
    {
        EditorUtility.DisplayProgressBar("Solar Setup", "Level02...", 0.75f);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.03f, 0.03f, 0.06f);

        var camGO = new GameObject("Main Camera") { tag = "MainCamera" };
        camGO.AddComponent<Camera>();
        camGO.AddComponent<TopDownCamera>();
        camGO.transform.position = new Vector3(0, 15, -8);

        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        var levelRoot = new GameObject("Level");
        levelRoot.isStatic = true;

        float wh = 3.5f, wt = 0.5f;

        CreateRoom(levelRoot.transform, "CentralHall", Vector3.zero, 10, 10, wh, wt,
            openN: true, openE: true, openW: true, openS: true);
        CreateRoom(levelRoot.transform, "Entrance", new Vector3(0,0,-7), 3, 4, wh, wt, openN: true);
        CreateRoom(levelRoot.transform, "WestRoom", new Vector3(-8,0,0), 6, 6, wh, wt, openE: true);
        CreateRoom(levelRoot.transform, "EastRoom", new Vector3(8,0,0), 6, 6, wh, wt, openW: true);
        CreateRoom(levelRoot.transform, "NorthCorridor", new Vector3(0,0,7), 3, 4, wh, wt,
            openN: true, openS: true);
        CreateRoom(levelRoot.transform, "ThroneRoom", new Vector3(0,0,13), 8, 8, wh, wt,
            openS: true, openN: true, openW: true);
        CreateRoom(levelRoot.transform, "WestCrypt", new Vector3(-6.5f,0,13), 5, 5, wh, wt, openE: true);
        CreateRoom(levelRoot.transform, "FinalCorridor", new Vector3(0,0,19), 3, 4, wh, wt,
            openN: true, openS: true);
        CreateRoom(levelRoot.transform, "HallOfGods", new Vector3(0,0,24), 6, 6, wh, wt, openS: true);

        CreateLevelBounds(levelRoot.transform, -12f, 12f, -10f, 28f, 4f);

        var navSurface = levelRoot.AddComponent<NavMeshSurface>();
        navSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        navSurface.BuildNavMesh();

        var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
        var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
        player.transform.position = new Vector3(0, 0.1f, -7);

        var invSO = new SerializedObject(player.GetComponent<PlayerInventory>());
        invSO.FindProperty("artifactsRequired").intValue = 5;
        invSO.ApplyModifiedPropertiesWithoutUndo();

        var artPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pickup_Artifact.prefab");
        PlacePrefab(artPrefab, new Vector3(-8, 0.5f, 0), "Artifact_1");
        PlacePrefab(artPrefab, new Vector3(8, 0.5f, 0), "Artifact_2");
        PlacePrefab(artPrefab, new Vector3(3, 0.5f, 13), "Artifact_3");
        PlacePrefab(artPrefab, new Vector3(-6.5f, 0.5f, 13), "Artifact_4");
        PlacePrefab(artPrefab, new Vector3(0, 0.5f, 25), "Artifact_5");

        var keyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pickup_Key.prefab");
        PlacePrefab(keyPrefab, new Vector3(-2, 0.5f, 14), "Key_1");

        var energyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pickup_Energy.prefab");
        PlacePrefab(energyPrefab, new Vector3(-3, 0.5f, 0), "Energy_1");
        PlacePrefab(energyPrefab, new Vector3(7, 0.5f, 1), "Energy_2");

        var patrolPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Patrol.prefab");

        var p1 = PlacePrefab(patrolPrefab, new Vector3(2, 0.1f, 2), "Patrol_1");
        var p1wp1 = new GameObject("P1_WP1"); p1wp1.transform.position = new Vector3(3, 0, 3);
        var p1wp2 = new GameObject("P1_WP2"); p1wp2.transform.position = new Vector3(-3, 0, -3);
        WirePatrolWaypoints(p1, new[] { p1wp1.transform, p1wp2.transform });

        var p2 = PlacePrefab(patrolPrefab, new Vector3(-6.5f, 0.1f, 13), "Patrol_2");
        var p2wp1 = new GameObject("P2_WP1"); p2wp1.transform.position = new Vector3(-5.5f, 0, 12);
        var p2wp2 = new GameObject("P2_WP2"); p2wp2.transform.position = new Vector3(-7.5f, 0, 14);
        WirePatrolWaypoints(p2, new[] { p2wp1.transform, p2wp2.transform });

        var sentryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Sentry.prefab");
        PlacePrefab(sentryPrefab, new Vector3(-2, 0.1f, -2), "Sentry_1");

        var wraithPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Wraith.prefab");
        PlacePrefab(wraithPrefab, new Vector3(8, 0.1f, 1), "Wraith_1");

        var lurkerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Lurker.prefab");
        var lurker = PlacePrefab(lurkerPrefab, new Vector3(0, 0.1f, 13), "Lurker_1");
        var lwp1 = new GameObject("L_WP1"); lwp1.transform.position = new Vector3(-2, 0, 11);
        var lwp2 = new GameObject("L_WP2"); lwp2.transform.position = new Vector3(2, 0, 15);
        WirePatrolWaypoints(lurker, new[] { lwp1.transform, lwp2.transform });

        CreateAltarInstance(new Vector3(4, 0, -3), "LightAltar_1");
        CreateAltarInstance(new Vector3(-3, 0, 15), "LightAltar_2");

        CreatePitTrap(new Vector3(0, 0, 18), "PitTrap_1");
        CreatePitTrap(new Vector3(0, 0, 20), "PitTrap_2");

        var goalGO = new GameObject("LevelGoal");
        goalGO.transform.position = new Vector3(0, 1, 26.5f);
        var goalCol = goalGO.AddComponent<BoxCollider>();
        goalCol.isTrigger = true;
        goalCol.size = new Vector3(4, 3, 1);
        var goal = goalGO.AddComponent<LevelGoal>();
        var goalSO = new SerializedObject(goal);
        goalSO.FindProperty("requirement").enumValueIndex = (int)DoorRequirement.AllArtifacts;
        goalSO.FindProperty("requireKey").boolValue = true;
        goalSO.ApplyModifiedPropertiesWithoutUndo();

        CreateHUDCanvas();
        var pauseGO = new GameObject("PauseManager");
        var pausePanel = CreatePausePanel(null);
        var pauseCtrl = pauseGO.AddComponent<PauseController>();
        var pauseSO = new SerializedObject(pauseCtrl);
        SetRef(pauseSO, "pausePanel", pausePanel);
        pauseSO.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Level02.unity");
        EditorUtility.ClearProgressBar();
    }

    static void CreateLevel03Scene()
    {
        EditorUtility.DisplayProgressBar("Solar Setup", "Level03...", 0.88f);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.01f, 0.01f, 0.03f);

        var camGO = new GameObject("Main Camera") { tag = "MainCamera" };
        camGO.AddComponent<Camera>();
        camGO.AddComponent<TopDownCamera>();
        camGO.transform.position = new Vector3(0, 15, -8);

        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        var levelRoot = new GameObject("Level");
        levelRoot.isStatic = true;

        float wh = 4f, wt = 0.5f;

        CreateRoom(levelRoot.transform, "SpawnRoom", new Vector3(0,0,0), 5, 5, wh, wt, openN: true);
        CreateRoom(levelRoot.transform, "Corr_N", new Vector3(0,0,4.5f), 3, 4, wh, wt, openN: true, openS: true);

        CreateRoom(levelRoot.transform, "Nexus", new Vector3(0,0,11.5f), 12, 10, wh, wt,
            openN: true, openS: true, openE: true, openW: true);

        CreateRoom(levelRoot.transform, "WestWing", new Vector3(-10,0,11.5f), 8, 8, wh, wt, openE: true, openN: true);
        CreateRoom(levelRoot.transform, "WestTunnel", new Vector3(-10,0,17.5f), 3, 4, wh, wt, openN: true, openS: true);
        CreateRoom(levelRoot.transform, "WestVault", new Vector3(-10,0,22.5f), 6, 6, wh, wt, openS: true);

        CreateRoom(levelRoot.transform, "EastWing", new Vector3(10,0,11.5f), 8, 8, wh, wt, openW: true, openN: true);
        CreateRoom(levelRoot.transform, "EastTunnel", new Vector3(10,0,17.5f), 3, 4, wh, wt, openN: true, openS: true);
        CreateRoom(levelRoot.transform, "EastVault", new Vector3(10,0,22.5f), 6, 6, wh, wt, openS: true);

        CreateRoom(levelRoot.transform, "Corr_Throne", new Vector3(0,0,19.5f), 3, 6, wh, wt, openN: true, openS: true);
        CreateRoom(levelRoot.transform, "ThroneHall", new Vector3(0,0,26.5f), 10, 8, wh, wt, openS: true, openN: true);
        CreateRoom(levelRoot.transform, "FinalCorr", new Vector3(0,0,32.5f), 3, 4, wh, wt, openN: true, openS: true);
        CreateRoom(levelRoot.transform, "BossRoom", new Vector3(0,0,40.5f), 14, 12, wh, wt, openS: true);

        CreateLevelBounds(levelRoot.transform, -14f, 14f, -4f, 48f, 5f);

        var navSurface = levelRoot.AddComponent<NavMeshSurface>();
        navSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        navSurface.BuildNavMesh();

        var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
        var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
        player.transform.position = new Vector3(0, 0.1f, 0);

        var invSO = new SerializedObject(player.GetComponent<PlayerInventory>());
        invSO.FindProperty("artifactsRequired").intValue = 7;
        invSO.ApplyModifiedPropertiesWithoutUndo();

        var artPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pickup_Artifact.prefab");
        PlacePrefab(artPrefab, new Vector3(-10, 0.5f, 11.5f), "Artifact_1");
        PlacePrefab(artPrefab, new Vector3(10, 0.5f, 11.5f), "Artifact_2");
        PlacePrefab(artPrefab, new Vector3(-10, 0.5f, 23.5f), "Artifact_3");
        PlacePrefab(artPrefab, new Vector3(10, 0.5f, 23.5f), "Artifact_4");
        PlacePrefab(artPrefab, new Vector3(3, 0.5f, 27.5f), "Artifact_5");
        PlacePrefab(artPrefab, new Vector3(-3, 0.5f, 27.5f), "Artifact_6");
        PlacePrefab(artPrefab, new Vector3(0, 0.5f, 42), "Artifact_7");

        var keyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pickup_Key.prefab");
        PlacePrefab(keyPrefab, new Vector3(0, 0.5f, 27), "Key_1");

        var energyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pickup_Energy.prefab");
        PlacePrefab(energyPrefab, new Vector3(-4, 0.5f, 11.5f), "Energy_1");
        PlacePrefab(energyPrefab, new Vector3(4, 0.5f, 11.5f), "Energy_2");
        PlacePrefab(energyPrefab, new Vector3(0, 0.5f, 32.5f), "Energy_3");

        var upgradePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pickup_Upgrade.prefab");
        PlacePrefab(upgradePrefab, new Vector3(-10, 0.5f, 17.5f), "Upgrade_1");

        var patrolPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Patrol.prefab");
        var sentryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Sentry.prefab");
        var wraithPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Wraith.prefab");
        var lurkerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Lurker.prefab");

        var p1 = PlacePrefab(patrolPrefab, new Vector3(-3, 0.1f, 11.5f), "Patrol_1");
        var p1w1 = new GameObject("P1W1"); p1w1.transform.position = new Vector3(-4, 0, 8.5f);
        var p1w2 = new GameObject("P1W2"); p1w2.transform.position = new Vector3(4, 0, 14.5f);
        WirePatrolWaypoints(p1, new[] { p1w1.transform, p1w2.transform });

        var p2 = PlacePrefab(patrolPrefab, new Vector3(-10, 0.1f, 13), "Patrol_2");
        var p2w1 = new GameObject("P2W1"); p2w1.transform.position = new Vector3(-12, 0, 10);
        var p2w2 = new GameObject("P2W2"); p2w2.transform.position = new Vector3(-8, 0, 14);
        WirePatrolWaypoints(p2, new[] { p2w1.transform, p2w2.transform });

        var p3 = PlacePrefab(patrolPrefab, new Vector3(10, 0.1f, 13), "Patrol_3");
        var p3w1 = new GameObject("P3W1"); p3w1.transform.position = new Vector3(8, 0, 10);
        var p3w2 = new GameObject("P3W2"); p3w2.transform.position = new Vector3(12, 0, 14);
        WirePatrolWaypoints(p3, new[] { p3w1.transform, p3w2.transform });

        PlacePrefab(sentryPrefab, new Vector3(0, 0.1f, 11.5f), "Sentry_1");
        PlacePrefab(sentryPrefab, new Vector3(-3, 0.1f, 27), "Sentry_2");
        PlacePrefab(sentryPrefab, new Vector3(3, 0.1f, 27), "Sentry_3");

        PlacePrefab(wraithPrefab, new Vector3(-10, 0.1f, 23), "Wraith_1");
        PlacePrefab(wraithPrefab, new Vector3(10, 0.1f, 23), "Wraith_2");

        var l1 = PlacePrefab(lurkerPrefab, new Vector3(0, 0.1f, 40), "Lurker_1");
        var lw1 = new GameObject("LW1"); lw1.transform.position = new Vector3(-5, 0, 37);
        var lw2 = new GameObject("LW2"); lw2.transform.position = new Vector3(5, 0, 44);
        WirePatrolWaypoints(l1, new[] { lw1.transform, lw2.transform });

        var l2 = PlacePrefab(lurkerPrefab, new Vector3(4, 0.1f, 41), "Lurker_2");
        var lw3 = new GameObject("LW3"); lw3.transform.position = new Vector3(6, 0, 38);
        var lw4 = new GameObject("LW4"); lw4.transform.position = new Vector3(2, 0, 44);
        WirePatrolWaypoints(l2, new[] { lw3.transform, lw4.transform });

        CreatePitTrap(new Vector3(0, 0, 18), "PitTrap_1");
        CreatePitTrap(new Vector3(0, 0, 21), "PitTrap_2");
        CreatePitTrap(new Vector3(0, 0, 32), "PitTrap_3");

        CreateAltarInstance(new Vector3(-4, 0, 8.5f), "LightAltar_1");
        CreateAltarInstance(new Vector3(10, 0, 17.5f), "LightAltar_2");
        CreateAltarInstance(new Vector3(0, 0, 31), "LightAltar_3");

        var goalGO = new GameObject("LevelGoal");
        goalGO.transform.position = new Vector3(0, 1, 46f);
        var goalCol = goalGO.AddComponent<BoxCollider>();
        goalCol.isTrigger = true;
        goalCol.size = new Vector3(4, 3, 1);
        var goal = goalGO.AddComponent<LevelGoal>();
        var goalSO = new SerializedObject(goal);
        goalSO.FindProperty("requirement").enumValueIndex = (int)DoorRequirement.AllArtifacts;
        goalSO.FindProperty("requireKey").boolValue = true;
        goalSO.ApplyModifiedPropertiesWithoutUndo();

        CreateHUDCanvas();
        var pauseGO = new GameObject("PauseManager");
        var pausePanel = CreatePausePanel(null);
        var pauseCtrl = pauseGO.AddComponent<PauseController>();
        var pauseSO = new SerializedObject(pauseCtrl);
        SetRef(pauseSO, "pausePanel", pausePanel);
        pauseSO.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Level03.unity");
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

    static void CreateRoom(Transform parent, string name, Vector3 center,
        float width, float depth, float wallHeight, float wallThickness,
        bool openN = false, bool openS = false, bool openE = false, bool openW = false)
    {
        var room = new GameObject(name);
        room.transform.SetParent(parent);
        room.transform.position = center;
        room.isStatic = true;

        MakeBox("Floor", room.transform, Vector3.zero,
            new Vector3(width, 0.1f, depth), matFloor, true);

        float hw = width / 2f;
        float hd = depth / 2f;
        float hy = wallHeight / 2f;
        float gap = 1.5f;

        if (!openN)
            MakeBox("WallN", room.transform, new Vector3(0, hy, hd),
                new Vector3(width, wallHeight, wallThickness), matWall, true);
        else
            MakeWallWithGap(room.transform, "WallN", new Vector3(0, hy, hd),
                width, wallHeight, wallThickness, gap, true);

        if (!openS)
            MakeBox("WallS", room.transform, new Vector3(0, hy, -hd),
                new Vector3(width, wallHeight, wallThickness), matWall, true);
        else
            MakeWallWithGap(room.transform, "WallS", new Vector3(0, hy, -hd),
                width, wallHeight, wallThickness, gap, true);

        if (!openE)
            MakeBox("WallE", room.transform, new Vector3(hw, hy, 0),
                new Vector3(wallThickness, wallHeight, depth), matWall, true);
        else
            MakeWallWithGap(room.transform, "WallE", new Vector3(hw, hy, 0),
                depth, wallHeight, wallThickness, gap, false);

        if (!openW)
            MakeBox("WallW", room.transform, new Vector3(-hw, hy, 0),
                new Vector3(wallThickness, wallHeight, depth), matWall, true);
        else
            MakeWallWithGap(room.transform, "WallW", new Vector3(-hw, hy, 0),
                depth, wallHeight, wallThickness, gap, false);
    }

    static void MakeWallWithGap(Transform parent, string name, Vector3 center,
        float totalLength, float height, float thickness, float gapSize, bool horizontal)
    {
        float segLen = (totalLength - gapSize) / 2f;
        float offset = (segLen + gapSize) / 2f;

        if (horizontal)
        {
            MakeBox(name + "_L", parent, center + new Vector3(-offset, 0, 0),
                new Vector3(segLen, height, thickness), matWall, true);
            MakeBox(name + "_R", parent, center + new Vector3(offset, 0, 0),
                new Vector3(segLen, height, thickness), matWall, true);
        }
        else
        {
            MakeBox(name + "_L", parent, center + new Vector3(0, 0, -offset),
                new Vector3(thickness, height, segLen), matWall, true);
            MakeBox(name + "_R", parent, center + new Vector3(0, 0, offset),
                new Vector3(thickness, height, segLen), matWall, true);
        }
    }

    static GameObject MakeBox(string name, Transform parent, Vector3 localPos,
        Vector3 scale, Material mat, bool isStatic)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = scale;
        go.isStatic = isStatic;
        go.layer = name.StartsWith("Wall") ? 9 : 0;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    static void CreateLevelBounds(Transform parent, float minX, float maxX, float minZ, float maxZ, float height)
    {
        float cx = (minX + maxX) / 2f;
        float cz = (minZ + maxZ) / 2f;
        float w = maxX - minX;
        float d = maxZ - minZ;
        float t = 1f;
        float y = height / 2f;

        MakeInvisibleWall("BoundN", parent, new Vector3(cx, y, maxZ + t/2), new Vector3(w + 2*t, height, t));
        MakeInvisibleWall("BoundS", parent, new Vector3(cx, y, minZ - t/2), new Vector3(w + 2*t, height, t));
        MakeInvisibleWall("BoundE", parent, new Vector3(maxX + t/2, y, cz), new Vector3(t, height, d + 2*t));
        MakeInvisibleWall("BoundW", parent, new Vector3(minX - t/2, y, cz), new Vector3(t, height, d + 2*t));
    }

    static void MakeInvisibleWall(string name, Transform parent, Vector3 pos, Vector3 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.layer = 9;
        go.isStatic = true;
        var col = go.AddComponent<BoxCollider>();
        col.size = size;
    }

    static void CreateHUDCanvas()
    {
        var canvasGO = new GameObject("HUD Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        var heartsGO = CreateTMPText("HeartsText",
            "<color=#FF4444>\u2665</color> <color=#FF4444>\u2665</color> <color=#FF4444>\u2665</color>",
            canvasGO.transform, new Vector2(20, -15), 28, Color.white, TextAlignmentOptions.TopLeft);
        var hRT = heartsGO.GetComponent<RectTransform>();
        hRT.anchorMin = new Vector2(0, 1);
        hRT.anchorMax = new Vector2(0, 1);
        hRT.pivot = new Vector2(0, 1);
        hRT.sizeDelta = new Vector2(200, 40);
        heartsGO.GetComponent<TMP_Text>().richText = true;

        var sliderGO = new GameObject("EnergyBar");
        sliderGO.transform.SetParent(canvasGO.transform, false);
        var sliderRT = sliderGO.AddComponent<RectTransform>();
        sliderRT.anchorMin = new Vector2(0, 1);
        sliderRT.anchorMax = new Vector2(0, 1);
        sliderRT.pivot = new Vector2(0, 1);
        sliderRT.anchoredPosition = new Vector2(20, -50);
        sliderRT.sizeDelta = new Vector2(150, 12);

        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(sliderGO.transform, false);
        var bgImage = bgGO.AddComponent<Image>();
        bgImage.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);
        bgGO.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        bgGO.GetComponent<RectTransform>().anchorMax = Vector2.one;
        bgGO.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        var fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(sliderGO.transform, false);
        var fillRT = fillAreaGO.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.sizeDelta = Vector2.zero;

        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        var fillImage = fillGO.AddComponent<Image>();
        fillImage.color = new Color(0.3f, 0.85f, 1f);
        fillGO.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        fillGO.GetComponent<RectTransform>().anchorMax = Vector2.one;
        fillGO.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        var slider = sliderGO.AddComponent<Slider>();
        slider.fillRect = fillGO.GetComponent<RectTransform>();
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 1;
        slider.interactable = false;

        var burstGO = new GameObject("BurstCooldown");
        burstGO.transform.SetParent(canvasGO.transform, false);
        var burstRT = burstGO.AddComponent<RectTransform>();
        burstRT.anchorMin = new Vector2(0, 1);
        burstRT.anchorMax = new Vector2(0, 1);
        burstRT.pivot = new Vector2(0, 1);
        burstRT.anchoredPosition = new Vector2(180, -42);
        burstRT.sizeDelta = new Vector2(24, 24);
        var burstImg = burstGO.AddComponent<Image>();
        burstImg.color = new Color(1f, 0.8f, 0.2f, 0.7f);
        burstImg.type = Image.Type.Filled;
        burstImg.fillMethod = Image.FillMethod.Radial360;
        burstImg.fillAmount = 0;

        var artGO = CreateTMPText("ArtifactText", "0 / 3", canvasGO.transform,
            new Vector2(-20, -20), 24, Color.white, TextAlignmentOptions.TopRight);
        var artRT = artGO.GetComponent<RectTransform>();
        artRT.anchorMin = new Vector2(1, 1);
        artRT.anchorMax = new Vector2(1, 1);
        artRT.pivot = new Vector2(1, 1);

        var winPanel = CreateOverlayPanel(canvasGO.transform, "WinPanel",
            "Уровень пройден!", "Следующий уровень", "В меню");
        winPanel.SetActive(false);

        var losePanel = CreateOverlayPanel(canvasGO.transform, "LosePanel",
            "Тебя поглотила тьма", "Заново", "В меню");
        losePanel.SetActive(false);

        var uiCtrl = canvasGO.AddComponent<UIController>();
        var uiSO = new SerializedObject(uiCtrl);

        SetRef(uiSO, "heartsText", heartsGO.GetComponent<TMP_Text>());

        SetRef(uiSO, "energyBar", slider);
        SetRef(uiSO, "energyFill", fillImage);
        SetRef(uiSO, "artifactText", artGO.GetComponent<TMP_Text>());
        SetRef(uiSO, "burstCooldownIcon", burstImg);
        SetRef(uiSO, "winPanel", winPanel);
        SetRef(uiSO, "losePanel", losePanel);

        var winBtns = winPanel.GetComponentsInChildren<Button>(true);
        if (winBtns.Length >= 2)
        {
            SetRef(uiSO, "winNextLevelButton", winBtns[0]);
            SetRef(uiSO, "winMenuButton", winBtns[1]);
        }

        var loseBtns = losePanel.GetComponentsInChildren<Button>(true);
        if (loseBtns.Length >= 2)
        {
            SetRef(uiSO, "loseRestartButton", loseBtns[0]);
            SetRef(uiSO, "loseMenuButton", loseBtns[1]);
        }

        uiSO.ApplyModifiedPropertiesWithoutUndo();
    }

    static GameObject CreateOverlayPanel(Transform parent, string name,
        string titleText, string btn1Text, string btn2Text)
    {
        var panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        var panelRT = panel.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.sizeDelta = Vector2.zero;
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.8f);

        CreateTMPText("Title", titleText, panel.transform,
            new Vector2(0, 60), 48, Color.white, TextAlignmentOptions.Center);

        CreateButton(btn1Text, panel.transform, new Vector2(0, -20));
        CreateButton(btn2Text, panel.transform, new Vector2(0, -90));

        return panel;
    }

    static GameObject CreatePausePanel(Transform parent)
    {
        var panel = new GameObject("PausePanel");
        if (parent != null) panel.transform.SetParent(parent, false);
        var panelRT = panel.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.sizeDelta = Vector2.zero;

        var canvas = panel.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        panel.AddComponent<GraphicRaycaster>();

        var bg = new GameObject("Background");
        bg.transform.SetParent(panel.transform, false);
        var bgRT = bg.AddComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.sizeDelta = Vector2.zero;
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0.7f);

        CreateTMPText("PauseTitle", "ПАУЗА", panel.transform,
            new Vector2(0, 60), 48, Color.white, TextAlignmentOptions.Center);

        CreateButton("Продолжить", panel.transform, new Vector2(0, -20));
        CreateButton("В меню", panel.transform, new Vector2(0, -90));

        panel.SetActive(false);
        return panel;
    }

    static GameObject CreateButton(string text, Transform parent, Vector2 pos)
    {
        var btnGO = new GameObject(text + "_Button");
        btnGO.transform.SetParent(parent, false);
        var rt = btnGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(280, 50);

        var img = btnGO.AddComponent<Image>();
        img.color = new Color(0.2f, 0.3f, 0.5f);
        btnGO.AddComponent<Button>();

        CreateTMPText("Text", text, btnGO.transform,
            Vector2.zero, 24, Color.white, TextAlignmentOptions.Center);
        var txtRT = btnGO.transform.GetChild(0).GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.sizeDelta = Vector2.zero;

        return btnGO;
    }

    static GameObject CreateTMPText(string name, string text, Transform parent,
        Vector2 pos, float fontSize, Color color, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(600, 80);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;

        return go;
    }

    static void CreatePitTrap(Vector3 position, string name)
    {
        var pit = new GameObject(name);
        pit.transform.position = position;

        var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "PitVisual";
        visual.transform.SetParent(pit.transform, false);
        visual.transform.localPosition = new Vector3(0, 0.03f, 0);
        visual.transform.localScale = new Vector3(1.0f, 0.06f, 1.0f);
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var pitMat = new Material(shader);
        pitMat.color = new Color(0.15f, 0.05f, 0.05f);
        pitMat.EnableKeyword("_EMISSION");
        pitMat.SetColor("_EmissionColor", new Color(0.15f, 0.02f, 0.02f));
        visual.GetComponent<Renderer>().sharedMaterial = pitMat;
        Object.DestroyImmediate(visual.GetComponent<Collider>());

        var triggerCol = pit.AddComponent<BoxCollider>();
        triggerCol.isTrigger = true;
        triggerCol.center = new Vector3(0, 0.5f, 0);
        triggerCol.size = new Vector3(1.0f, 2f, 1.0f);
        pit.AddComponent<PitTrap>();
    }

    static GameObject CreateAltarInstance(Vector3 position, string name)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.position = position;
        go.transform.localScale = new Vector3(1f, 0.5f, 1f);
        go.GetComponent<Renderer>().sharedMaterial = matAltar;

        var triggerCol = go.AddComponent<SphereCollider>();
        triggerCol.isTrigger = true;
        triggerCol.radius = 2f;

        var lightGO = new GameObject("AltarLight");
        lightGO.transform.SetParent(go.transform);
        lightGO.transform.localPosition = new Vector3(0, 1f, 0);
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 4f;
        light.intensity = 2f;
        light.color = new Color(0.3f, 0.5f, 1f);

        var altar = go.AddComponent<LightAltar>();
        var altarSO = new SerializedObject(altar);
        SetRef(altarSO, "altarLight", light);
        altarSO.ApplyModifiedPropertiesWithoutUndo();

        return go;
    }

    static GameObject PlacePrefab(GameObject prefab, Vector3 position, string name)
    {
        if (prefab == null) return null;
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = name;
        instance.transform.position = position;
        return instance;
    }

    static void WirePatrolWaypoints(GameObject enemyInstance, Transform[] waypoints)
    {
        if (enemyInstance == null) return;
        var patrol = enemyInstance.GetComponent<EnemyPatrol>();
        if (patrol == null) return;

        var so = new SerializedObject(patrol);
        var wpProp = so.FindProperty("waypoints");
        wpProp.arraySize = waypoints.Length;
        for (int i = 0; i < waypoints.Length; i++)
            wpProp.GetArrayElementAtIndex(i).objectReferenceValue = waypoints[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetRef(SerializedObject so, string propName, Object value)
    {
        var prop = so.FindProperty(propName);
        if (prop != null)
            prop.objectReferenceValue = value;
    }

    static void SetLayer(SerializedObject so, string propName, int layer)
    {
        var prop = so.FindProperty(propName);
        if (prop != null)
            prop.intValue = 1 << layer;
    }
}
#endif
