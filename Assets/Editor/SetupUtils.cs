#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static partial class ProjectSetup
{
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

    static void CreateLevelBounds(Transform parent, float minX, float maxX,
        float minZ, float maxZ, float height)
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
