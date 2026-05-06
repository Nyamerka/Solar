#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.AI.Navigation;

public static partial class ProjectSetup
{
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

        var lurkerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Lurker.prefab");

        PlacePrefab(sentryPrefab, new Vector3(0, 0.1f, 10), "Sentry_2");

        var patrol3 = PlacePrefab(patrolPrefab, new Vector3(0, 0.1f, 47), "Patrol_3");
        var wp5 = new GameObject("WP5"); wp5.transform.position = new Vector3(-1, 0, 45);
        var wp6 = new GameObject("WP6"); wp6.transform.position = new Vector3(1, 0, 49);
        WirePatrolWaypoints(patrol3, new[] { wp5.transform, wp6.transform });

        var lurker1 = PlacePrefab(lurkerPrefab, new Vector3(0, 0.1f, 52), "Lurker_1");
        var lw1 = new GameObject("L1_WP1"); lw1.transform.position = new Vector3(-2, 0, 50);
        var lw2 = new GameObject("L1_WP2"); lw2.transform.position = new Vector3(2, 0, 54);
        WirePatrolWaypoints(lurker1, new[] { lw1.transform, lw2.transform });

        PlacePrefab(wraithPrefab, new Vector3(-2, 0.1f, 33), "Wraith_2");

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

        PlacePrefab(sentryPrefab, new Vector3(3, 0.1f, 0), "Sentry_2");
        PlacePrefab(sentryPrefab, new Vector3(0, 0.1f, 13), "Sentry_3");

        var p3 = PlacePrefab(patrolPrefab, new Vector3(0, 0.1f, 7), "Patrol_3");
        var p3wp1 = new GameObject("P3_WP1"); p3wp1.transform.position = new Vector3(-1, 0, 5.5f);
        var p3wp2 = new GameObject("P3_WP2"); p3wp2.transform.position = new Vector3(1, 0, 8.5f);
        WirePatrolWaypoints(p3, new[] { p3wp1.transform, p3wp2.transform });

        PlacePrefab(wraithPrefab, new Vector3(-8, 0.1f, 1), "Wraith_2");

        var lurker2 = PlacePrefab(lurkerPrefab, new Vector3(0, 0.1f, 24), "Lurker_2");
        var l2wp1 = new GameObject("L2_WP1"); l2wp1.transform.position = new Vector3(-2, 0, 22);
        var l2wp2 = new GameObject("L2_WP2"); l2wp2.transform.position = new Vector3(2, 0, 26);
        WirePatrolWaypoints(lurker2, new[] { l2wp1.transform, l2wp2.transform });

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

        var p4 = PlacePrefab(patrolPrefab, new Vector3(0, 0.1f, 20), "Patrol_4");
        var p4w1 = new GameObject("P4W1"); p4w1.transform.position = new Vector3(-1, 0, 18);
        var p4w2 = new GameObject("P4W2"); p4w2.transform.position = new Vector3(1, 0, 22);
        WirePatrolWaypoints(p4, new[] { p4w1.transform, p4w2.transform });

        var p5 = PlacePrefab(patrolPrefab, new Vector3(0, 0.1f, 33), "Patrol_5");
        var p5w1 = new GameObject("P5W1"); p5w1.transform.position = new Vector3(-1, 0, 31);
        var p5w2 = new GameObject("P5W2"); p5w2.transform.position = new Vector3(1, 0, 35);
        WirePatrolWaypoints(p5, new[] { p5w1.transform, p5w2.transform });

        PlacePrefab(sentryPrefab, new Vector3(-10, 0.1f, 22.5f), "Sentry_4");
        PlacePrefab(sentryPrefab, new Vector3(10, 0.1f, 22.5f), "Sentry_5");

        PlacePrefab(wraithPrefab, new Vector3(0, 0.1f, 27), "Wraith_3");

        var l3 = PlacePrefab(lurkerPrefab, new Vector3(-4, 0.1f, 41), "Lurker_3");
        var lw5 = new GameObject("LW5"); lw5.transform.position = new Vector3(-6, 0, 38);
        var lw6 = new GameObject("LW6"); lw6.transform.position = new Vector3(-2, 0, 44);
        WirePatrolWaypoints(l3, new[] { lw5.transform, lw6.transform });

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
}
#endif
