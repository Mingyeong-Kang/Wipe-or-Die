using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Unity.XR.CoreUtils;

public static class SceneSetupTool
{
    [MenuItem("WipeOrDie/0. Check Building & Player Positions")]
    static void CheckPositions()
    {
        string[] buildingNames = { "L1 (Lv1)", "L2 (Lv2)", "L3 (Lv3)" };
        string[] objNames      = { "L1", "L2", "L3" };
        int[]    windowCounts  = { 20, 30, 40 };
        int      cols          = 5;
        float    spacingY      = 2.0f;

        Debug.Log("=== 건물/창문/플레이어 좌표 체크 ===");

        for (int i = 0; i < 3; i++)
        {
            var bld = GameObject.Find(objNames[i]);
            if (bld == null)
            {
                Debug.LogWarning($"[{buildingNames[i]}] ❌ '{objNames[i]}' 오브젝트 없음 — 이름 확인 필요");
                continue;
            }

            Vector3 bPos = bld.transform.position;
            int rows = Mathf.CeilToInt((float)windowCounts[i] / cols);
            float windowBaseY = bPos.y + 1.6f;
            float windowTopY  = bPos.y + 1.6f + (rows - 1) * spacingY;
            float windowZ     = bPos.z + 3f;
            float playerZ     = bPos.z + 5f;

            Debug.Log($"[{buildingNames[i]}] 건물:{bPos}  |  창문Z:{windowZ:F1}  창문Y:{windowBaseY:F1}~{windowTopY:F1}({rows}행)  |  플레이어스폰:({bPos.x:F1}, 1.7, {playerZ:F1})");
        }

        var xrOrigin = Object.FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
        if (xrOrigin != null)
            Debug.Log($"XR Origin 현재위치: {xrOrigin.transform.position}");

        var lm = Object.FindFirstObjectByType<LevelManager>();
        if (lm != null)
        {
            var lmSO = new SerializedObject(lm);
            var spawnProp = lmSO.FindProperty("playerSpawnPositions");
            if (spawnProp != null && spawnProp.arraySize > 0)
            {
                for (int i = 0; i < spawnProp.arraySize; i++)
                    Debug.Log($"LevelManager 스폰 Lv{i + 1}: {spawnProp.GetArrayElementAtIndex(i).vector3Value}");
            }
            else
                Debug.LogWarning("LevelManager 스폰 위치 미설정 → 'Fix Player Position' 실행 필요");
        }
        else
            Debug.LogWarning("LevelManager 없음 → 'Setup MainScene' 실행 필요");
    }

    [MenuItem("WipeOrDie/1. Setup MainScene")]
    static void SetupMainScene()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (!scene.name.Equals("MainScene"))
        {
            Debug.LogError("[WipeOrDie] MainScene을 먼저 열어주세요! (현재: " + scene.name + ")");
            return;
        }

        XROrigin xrOrigin = Object.FindFirstObjectByType<XROrigin>();
        if (xrOrigin == null) { Debug.LogError("[WipeOrDie] XR Origin을 찾을 수 없어요!"); return; }

        // XR Origin에 컴포넌트 추가
        if (xrOrigin.GetComponent<RopeMovement>() == null)
            xrOrigin.gameObject.AddComponent<RopeMovement>();

        PlayerHealth playerHealth = xrOrigin.GetComponent<PlayerHealth>();
        if (playerHealth == null)
            playerHealth = xrOrigin.gameObject.AddComponent<PlayerHealth>();

        // 매니저 생성
        ScoreManager scoreManager       = EnsureManager<ScoreManager>("ScoreManager");
        CleaningManager cleaningManager = EnsureManager<CleaningManager>("CleaningManager");

        GameManager gameManager = EnsureManager<GameManager>("GameManager");
        var gmSO = new SerializedObject(gameManager);
        gmSO.FindProperty("cleaningManager").objectReferenceValue = cleaningManager;
        gmSO.FindProperty("playerHealth").objectReferenceValue    = playerHealth;
        gmSO.ApplyModifiedProperties();

        // ObstacleSpawner (프리팹)
        ObstacleSpawner spawner = Object.FindFirstObjectByType<ObstacleSpawner>();
        if (spawner == null)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/ObstacleSpawner.prefab");
            var go = prefab != null
                ? (GameObject)PrefabUtility.InstantiatePrefab(prefab)
                : new GameObject("ObstacleSpawner");
            go.name = "ObstacleSpawner";
            spawner = go.GetComponent<ObstacleSpawner>() ?? go.AddComponent<ObstacleSpawner>();
        }
        spawner.target = xrOrigin.transform;
        EditorUtility.SetDirty(spawner);

        // 창문 머티리얼 생성
        Material[] windowMats = EnsureWindowMaterials();

        // 레벨별 창문 그룹 생성 (레벨 1: 2개, 레벨 2: 3개, 레벨 3: 4개)
        Window[][] levelWindows = CreateLevelWindows(windowMats);

        // CleaningManager 초기 창문 = 레벨 1
        var cmSO = new SerializedObject(cleaningManager);
        var winProp = cmSO.FindProperty("windows");
        winProp.arraySize = levelWindows[0].Length;
        for (int i = 0; i < levelWindows[0].Length; i++)
            winProp.GetArrayElementAtIndex(i).objectReferenceValue = levelWindows[0][i];
        cmSO.ApplyModifiedProperties();

        // LevelManager 생성 및 연결
        LevelManager levelManager = EnsureManager<LevelManager>("LevelManager");
        var lmSO = new SerializedObject(levelManager);
        lmSO.FindProperty("cleaningManager").objectReferenceValue = cleaningManager;
        lmSO.FindProperty("obstacleSpawner").objectReferenceValue = spawner;
        lmSO.FindProperty("playerTransform").objectReferenceValue = xrOrigin.transform;

        var levelsProp = lmSO.FindProperty("levels");
        levelsProp.arraySize = 3;
        float[] timeLimits    = { 240f, 360f, 480f };   // 4분, 6분, 8분
        float[] speedMults    = { 1f,   1.3f,  1.7f };
        float[] intervalMults = { 1f,   0.75f, 0.55f };
        for (int lvl = 0; lvl < 3; lvl++)
        {
            var levelElem = levelsProp.GetArrayElementAtIndex(lvl);
            levelElem.FindPropertyRelative("timeLimitSeconds").floatValue           = timeLimits[lvl];
            levelElem.FindPropertyRelative("obstacleSpeedMultiplier").floatValue    = speedMults[lvl];
            levelElem.FindPropertyRelative("obstacleIntervalMultiplier").floatValue = intervalMults[lvl];
            var windowsArr = levelElem.FindPropertyRelative("windows");
            windowsArr.arraySize = levelWindows[lvl].Length;
            for (int w = 0; w < levelWindows[lvl].Length; w++)
                windowsArr.GetArrayElementAtIndex(w).objectReferenceValue = levelWindows[lvl][w];
        }

        // 레벨별 플레이어 스폰 위치: L1(Lv1) → L2(Lv2) → L3(Lv3) 순서
        string[] spawnBuildingOrder = { "L1", "L2", "L3" };
        var spawnPosProp = lmSO.FindProperty("playerSpawnPositions");
        spawnPosProp.arraySize = 3;
        Vector3 firstSpawnPos = new Vector3(0f, 1.6f, 5f);
        for (int i = 0; i < 3; i++)
        {
            var bld = GameObject.Find(spawnBuildingOrder[i]);
            Vector3 bPos = bld != null ? bld.transform.position : new Vector3((2 - i) * 8f, 0f, 0f);
            // 창문(building +3f) 보다 2m 더 앞 = +5f, 눈 높이 1.7f
            var spawnPos = new Vector3(bPos.x, 1.7f, bPos.z + 5f);
            spawnPosProp.GetArrayElementAtIndex(i).vector3Value = spawnPos;
            if (i == 0) firstSpawnPos = spawnPos;
        }
        lmSO.ApplyModifiedProperties();

        // XR Origin을 Lv1 시작 위치(L3 앞)로 이동
        xrOrigin.transform.position = firstSpawnPos;
        Debug.Log($"[WipeOrDie] XR Origin 초기 위치: {firstSpawnPos}");

        // 청소 도구 배치
        SpawnToolPrefabs(xrOrigin.transform.position);

        // GameUI
        if (Object.FindFirstObjectByType<GameUI>() == null)
            CreateGameUI(xrOrigin);

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("[WipeOrDie] ✅ MainScene 세팅 완료! (Ctrl+S로 저장하세요)");
    }

    [MenuItem("WipeOrDie/2. Fix Player Position (MainScene)")]
    static void FixPlayerPosition()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (!scene.name.Equals("MainScene"))
        {
            Debug.LogError("[WipeOrDie] MainScene을 먼저 열어주세요! (현재: " + scene.name + ")");
            return;
        }

        XROrigin xrOrigin = Object.FindFirstObjectByType<XROrigin>();
        if (xrOrigin == null) { Debug.LogError("[WipeOrDie] XR Origin을 찾을 수 없어요!"); return; }

        LevelManager levelManager = Object.FindFirstObjectByType<LevelManager>();
        if (levelManager == null) { Debug.LogError("[WipeOrDie] LevelManager가 없어요. 먼저 Setup MainScene을 실행하세요."); return; }

        string[] spawnBuildingOrder = { "L1", "L2", "L3" };
        var lmSO = new SerializedObject(levelManager);
        lmSO.FindProperty("playerTransform").objectReferenceValue = xrOrigin.transform;

        var spawnPosProp = lmSO.FindProperty("playerSpawnPositions");
        spawnPosProp.arraySize = 3;
        Vector3 firstSpawnPos = xrOrigin.transform.position;
        for (int i = 0; i < 3; i++)
        {
            var bld = GameObject.Find(spawnBuildingOrder[i]);
            if (bld == null) { Debug.LogWarning($"[WipeOrDie] '{spawnBuildingOrder[i]}' 건물을 찾을 수 없어요."); continue; }
            Vector3 bPos = bld.transform.position;
            var spawnPos = new Vector3(bPos.x, 1.7f, bPos.z + 5f);
            spawnPosProp.GetArrayElementAtIndex(i).vector3Value = spawnPos;
            if (i == 0) firstSpawnPos = spawnPos;
        }
        // 제한시간 업데이트 (4분, 6분, 8분)
        float[] timeLimits = { 240f, 360f, 480f };
        var levelsProp2 = lmSO.FindProperty("levels");
        for (int i = 0; i < Mathf.Min(timeLimits.Length, levelsProp2.arraySize); i++)
            levelsProp2.GetArrayElementAtIndex(i).FindPropertyRelative("timeLimitSeconds").floatValue = timeLimits[i];

        lmSO.ApplyModifiedProperties();

        // XR Origin을 Lv1 시작 위치(L1 앞)로 이동
        xrOrigin.transform.position = firstSpawnPos;

        // 창문 레이어 설정
        int windowLayer = EnsureLayer("Window");
        foreach (var win in Object.FindObjectsByType<Window>(FindObjectsSortMode.None))
            win.gameObject.layer = windowLayer;

        // SprayBottle / WaterSpray 프리팹 windowLayer 설정
        SetPrefabWindowLayer<SprayBottle>("Assets/_Project/Prefabs/SprayBottle.prefab", windowLayer);
        SetPrefabWindowLayer<WaterSpray>("Assets/_Project/Prefabs/WaterSpray.prefab",   windowLayer);

        // ObstacleSpawner 장애물 프리팹 자동 연결
        ObstacleSpawner spawner = Object.FindFirstObjectByType<ObstacleSpawner>();
        if (spawner != null)
        {
            string[] obstaclePaths = {
                "Assets/_Project/Prefabs/BirdObstacle.prefab",
                "Assets/_Project/Prefabs/DroneObstacle.prefab",
                "Assets/_Project/Prefabs/FallingObstacle.prefab",
                "Assets/_Project/Prefabs/FallingPot.prefab",
            };
            int[] obstacleTypes = { 1, 1, 0, 0 }; // 0=Falling, 1=Flying

            var spawnerSO = new SerializedObject(spawner);
            var obstsProp = spawnerSO.FindProperty("obstacles");
            obstsProp.arraySize = obstaclePaths.Length;
            for (int i = 0; i < obstaclePaths.Length; i++)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(obstaclePaths[i]);
                var elem = obstsProp.GetArrayElementAtIndex(i);
                elem.FindPropertyRelative("obstaclePrefab").objectReferenceValue = prefab;
                elem.FindPropertyRelative("obstacleType").enumValueIndex = obstacleTypes[i];
            }
            spawnerSO.ApplyModifiedProperties();
            EditorUtility.SetDirty(spawner);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log($"[WipeOrDie] ✅ 플레이어 위치·레이어·장애물 연결 완료! (Ctrl+S로 저장하세요)");
    }

    [MenuItem("WipeOrDie/3. Fix TUTORIAL Scene")]
    static void FixTutorialScene()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (!scene.name.Equals("TUTORIAL"))
        {
            Debug.LogError("[WipeOrDie] TUTORIAL 씬을 먼저 열어주세요! (현재: " + scene.name + ")");
            return;
        }

        TutorialManager tm = Object.FindFirstObjectByType<TutorialManager>();
        if (tm == null) { Debug.LogError("[WipeOrDie] TutorialManager를 찾을 수 없어요!"); return; }

        Window window = Object.FindFirstObjectByType<Window>();
        if (window == null) { Debug.LogWarning("[WipeOrDie] Window 오브젝트가 없어요. 수동으로 연결해주세요."); return; }

        var so = new SerializedObject(tm);
        // tutorialWindow가 Window 타입으로 변경됨
        so.FindProperty("tutorialWindow").objectReferenceValue = window;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("[WipeOrDie] ✅ TUTORIAL 씬 수정 완료! (Ctrl+S로 저장하세요)");
    }

    // ── 헬퍼 ─────────────────────────────────────────────────────────

    static void SetPrefabWindowLayer<T>(string path, int layerIdx) where T : Component
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) return;
        var comp = prefab.GetComponent<T>();
        if (comp == null) return;
        var so = new SerializedObject(comp);
        so.FindProperty("windowLayer").intValue = 1 << layerIdx;
        so.ApplyModifiedProperties();
        PrefabUtility.SavePrefabAsset(prefab);
    }

    static int EnsureLayer(string layerName)
    {
        int idx = LayerMask.NameToLayer(layerName);
        if (idx >= 0) return idx;

        var tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layers = tagManager.FindProperty("layers");
        for (int i = 8; i < layers.arraySize; i++)
        {
            var elem = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(elem.stringValue))
            {
                elem.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                Debug.Log($"[WipeOrDie] 레이어 '{layerName}' 추가됨 (슬롯 {i})");
                return i;
            }
        }
        Debug.LogWarning($"[WipeOrDie] 레이어 슬롯 부족 — '{layerName}' 추가 실패, Default 레이어 사용");
        return 0;
    }

    static T EnsureManager<T>(string goName) where T : Component
    {
        T found = Object.FindFirstObjectByType<T>();
        if (found != null) return found;
        return new GameObject(goName).AddComponent<T>();
    }

    // 레벨별 창문 생성
    // L1=Lv1(20창문), L2=Lv2(30창문), L3=Lv3(40창문)
    static Window[][] CreateLevelWindows(Material[] mats)
    {
        string[] buildingNames = { "L1", "L2", "L3" };  // Lv1, Lv2, Lv3 순서
        int[]    counts        = { 20, 30, 40 };
        var result = new Window[3][];

        // 기존 Window 오브젝트 제거
        foreach (var old in Object.FindObjectsByType<Window>(FindObjectsSortMode.None))
            Object.DestroyImmediate(old.gameObject);

        int   cols     = 5;
        float spacingX = 1.6f;
        float spacingY = 2.0f;

        for (int lvl = 0; lvl < 3; lvl++)
        {
            var building = GameObject.Find(buildingNames[lvl]);
            Vector3 bPos = building != null ? building.transform.position : new Vector3((2 - lvl) * 8f, 0f, 0f);
            if (building == null)
                Debug.LogWarning($"[WipeOrDie] '{buildingNames[lvl]}' 오브젝트를 찾을 수 없어요. 기본 위치 사용.");

            float baseZ = bPos.z + 3f;

            result[lvl] = new Window[counts[lvl]];
            for (int w = 0; w < counts[lvl]; w++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                go.name = $"Window_L{lvl + 1}_{w + 1}";

                int   col     = w % cols;
                int   row     = w / cols;
                float xOffset = (col - (cols - 1) * 0.5f) * spacingX;
                float yOffset = row * spacingY;
                go.transform.position   = new Vector3(bPos.x + xOffset, 1.6f + yOffset, baseZ);
                go.transform.localScale = new Vector3(1.2f, 1.8f, 1f);
                go.SetActive(lvl == 0);

                if (go.GetComponent<Collider>() == null)
                    go.AddComponent<BoxCollider>();

                // Renderer에 Dirty 머티리얼 바로 적용 (에디터에서도 보이게)
                var renderer = go.GetComponent<Renderer>();
                renderer.sharedMaterial = mats[0]; // Dirty

                var window = go.AddComponent<Window>();
                ApplyWindowMaterials(window, mats);

                var wSO = new SerializedObject(window);
                wSO.FindProperty("windowRenderer").objectReferenceValue = renderer;
                wSO.ApplyModifiedProperties();

                result[lvl][w] = window;
            }
        }
        return result;
    }

    static Material[] EnsureWindowMaterials()
    {
        string[] names  = { "Window_Dirty", "Window_Sprayed", "Window_Sponged", "Window_Rinsed", "Window_Clean" };
        Color[]  colors = {
            new Color(0.35f, 0.28f, 0.18f, 1f),   // Dirty  - 갈색 불투명
            new Color(0.7f,  0.85f, 0.9f,  0.8f),  // Sprayed - 비누칠
            new Color(0.6f,  0.75f, 0.8f,  0.7f),  // Sponged - 거품 닦임
            new Color(0.5f,  0.7f,  0.85f, 0.5f),  // Rinsed  - 물 흘림
            new Color(0.85f, 0.95f, 1f,    0.25f),  // Clean   - 깨끗한 유리
        };

        string folder = "Assets/_Project/Materials";
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/_Project", "Materials");

        var mats = new Material[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            string path = $"{folder}/{names[i]}.mat";

            // 항상 새로 만들어서 설정 오류 방지
            var existingAsset = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existingAsset != null)
                AssetDatabase.DeleteAsset(path);

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            var mat = new Material(shader) { name = names[i] };

            // 양면 렌더링 (Quad 뒷면도 보이게)
            mat.SetFloat("_Cull", 0f);

            bool isTransparent = colors[i].a < 0.99f;
            if (isTransparent)
            {
                mat.SetFloat("_Surface", 1f);
                mat.SetFloat("_Blend",   0f);
                mat.SetFloat("_AlphaClip", 0f);
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = 3000;
            }
            else
            {
                mat.SetFloat("_Surface", 0f);
                mat.SetOverrideTag("RenderType", "Opaque");
                mat.renderQueue = 2000;
            }

            mat.SetColor("_BaseColor", colors[i]);
            mat.SetColor("_Color",     colors[i]);

            AssetDatabase.CreateAsset(mat, path);
            mats[i] = mat;
        }
        AssetDatabase.SaveAssets();
        return mats;
    }

    static Window[] EnsureWindows(int count, Material[] windowMats)
    {
        var existing = Object.FindObjectsByType<Window>(FindObjectsSortMode.None);

        // 기존 Window에도 머티리얼 연결
        foreach (var w in existing)
            ApplyWindowMaterials(w, windowMats);

        if (existing.Length >= count) return existing;

        var list = new System.Collections.Generic.List<Window>(existing);
        int toCreate = count - existing.Length;
        for (int i = 0; i < toCreate; i++)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = $"Window_{list.Count + 1}";
            go.transform.position = new Vector3(-2f + list.Count * 2f, 1.6f, 3f);
            go.transform.localScale = new Vector3(1.2f, 1.8f, 1f);
            if (go.GetComponent<Collider>() == null)
                go.AddComponent<BoxCollider>();

            var window = go.AddComponent<Window>();
            ApplyWindowMaterials(window, windowMats);

            // windowRenderer 연결
            var so = new SerializedObject(window);
            so.FindProperty("windowRenderer").objectReferenceValue = go.GetComponent<Renderer>();
            so.ApplyModifiedProperties();

            list.Add(window);
        }
        return list.ToArray();
    }

    static void ApplyWindowMaterials(Window window, Material[] mats)
    {
        var so = new SerializedObject(window);
        var matsProp = so.FindProperty("stateMaterials");
        matsProp.arraySize = mats.Length;
        for (int i = 0; i < mats.Length; i++)
            matsProp.GetArrayElementAtIndex(i).objectReferenceValue = mats[i];
        so.ApplyModifiedProperties();
    }

    static void SpawnToolPrefabs(Vector3 playerPos)
    {
        string[] toolPrefabPaths = {
            "Assets/_Project/Prefabs/SprayBottle.prefab",
            "Assets/_Project/Prefabs/Sponge.prefab",
            "Assets/_Project/Prefabs/DryCloth.prefab",
            "Assets/_Project/Prefabs/WaterSpray.prefab",
        };

        // 플레이어 손 닿는 위치에 일렬로 배치
        Vector3[] offsets = {
            new Vector3(-0.6f, 1.0f, 1.2f),
            new Vector3(-0.2f, 1.0f, 1.2f),
            new Vector3( 0.2f, 1.0f, 1.2f),
            new Vector3( 0.6f, 1.0f, 1.2f),
        };

        for (int i = 0; i < toolPrefabPaths.Length; i++)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(toolPrefabPaths[i]);
            if (prefab == null) { Debug.LogWarning($"[WipeOrDie] 프리팹 없음: {toolPrefabPaths[i]}"); continue; }

            // 이미 씬에 있으면 스킵
            string toolName = prefab.name;
            if (GameObject.Find(toolName) != null) continue;

            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.transform.position = playerPos + offsets[i];
        }
    }

    static void CreateGameUI(XROrigin xrOrigin)
    {
        var canvasGO = new GameObject("GameUI");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            canvasGO.transform.position = mainCam.transform.position + mainCam.transform.forward * 2f + Vector3.up * 0.3f;
            canvasGO.transform.rotation = mainCam.transform.rotation;
            var follow = canvasGO.AddComponent<FollowCamera>();
            follow.cam = mainCam.transform;
        }

        canvasGO.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 300);
        canvasGO.transform.localScale = Vector3.one * 0.002f;

        var gameUI = canvasGO.AddComponent<GameUI>();

        var scoreTMP = MakeText(canvasGO, "ScoreText",          "Score: 0",  new Vector2(-250, 120), 40);
        var comboTMP = MakeText(canvasGO, "ComboText",          "",          new Vector2( 250, 120), 40);
        var progTMP  = MakeText(canvasGO, "WindowProgressText", "창문 0/0", new Vector2(   0,  70), 32);
        var timerTMP = MakeText(canvasGO, "TimerText",          "02:00",     new Vector2(   0, 120), 48);
        var levelTMP = MakeText(canvasGO, "LevelText",          "LEVEL 1",   new Vector2(   0, -50), 36);

        var gameOverPanel = MakePanel(canvasGO, "GameOverPanel", new Color(0f, 0f, 0f, 0.85f));
        MakeText(gameOverPanel, "GOTitle", "GAME OVER", Vector2.zero, 72);
        gameOverPanel.SetActive(false);

        var levelClearPanel = MakePanel(canvasGO, "LevelClearPanel", new Color(0f, 0.4f, 0f, 0.85f));
        var finalTMP = MakeText(levelClearPanel, "FinalScoreText", "Final Score: 0", Vector2.zero, 48);
        levelClearPanel.SetActive(false);

        // 레벨 시작 패널
        var levelStartPanel = MakePanel(canvasGO, "LevelStartPanel", new Color(0f, 0f, 0.5f, 0.88f));
        var startTitleTMP = MakeText(levelStartPanel, "LevelStartTitle", "LEVEL 1",          new Vector2(0,  40), 80);
        var startInfoTMP  = MakeText(levelStartPanel, "LevelStartInfo",  "창문 20개  |  02:00", new Vector2(0, -30), 36);
        levelStartPanel.SetActive(false);

        var so = new SerializedObject(gameUI);
        so.FindProperty("scoreText").objectReferenceValue          = scoreTMP;
        so.FindProperty("comboText").objectReferenceValue          = comboTMP;
        so.FindProperty("windowProgressText").objectReferenceValue = progTMP;
        so.FindProperty("timerText").objectReferenceValue          = timerTMP;
        so.FindProperty("levelText").objectReferenceValue          = levelTMP;
        so.FindProperty("gameOverPanel").objectReferenceValue      = gameOverPanel;
        so.FindProperty("levelClearPanel").objectReferenceValue    = levelClearPanel;
        so.FindProperty("finalScoreText").objectReferenceValue     = finalTMP;
        so.FindProperty("levelStartPanel").objectReferenceValue      = levelStartPanel;
        so.FindProperty("levelStartTitleText").objectReferenceValue  = startTitleTMP;
        so.FindProperty("levelStartInfoText").objectReferenceValue   = startInfoTMP;
        so.ApplyModifiedProperties();
    }

    static TextMeshProUGUI MakeText(GameObject parent, string name, string text, Vector2 pos, int size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size;
        tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        var r = go.GetComponent<RectTransform>();
        r.anchoredPosition = pos; r.sizeDelta = new Vector2(400, 70);
        return tmp;
    }

    static GameObject MakePanel(GameObject parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<Image>().color = color;
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
        return go;
    }
}
