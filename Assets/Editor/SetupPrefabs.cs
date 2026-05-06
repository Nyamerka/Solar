#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

public static partial class ProjectSetup
{
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
        go.AddComponent<EnemyStunBar>();

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

        var glowObj = new GameObject("Glow");
        glowObj.transform.SetParent(go.transform);
        glowObj.transform.localPosition = Vector3.zero;
        var light = glowObj.AddComponent<Light>();
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

        var glowObj = new GameObject("Glow");
        glowObj.transform.SetParent(go.transform);
        glowObj.transform.localPosition = Vector3.zero;
        var light = glowObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 2f;
        light.intensity = 0.8f;
        light.color = new Color(0.5f, 1f, 0.8f);

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }
}
#endif
