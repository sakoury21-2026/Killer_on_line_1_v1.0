using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Read-only scene audit tool for Killer on Line 1.
///
/// Put this file inside Assets/Editor. The tool never saves or changes the scene.
/// Reports are written outside Assets so Unity will not import them.
/// </summary>
public static class SceneAuditExporter
{
    private const string MenuRoot = "Tools/Killer on Line 1/Scene Audit/";
    private const long LargeSceneWarningBytes = 500L * 1024L * 1024L;
    private const int NamePathSampleLimit = 8;

    private static readonly string[] ImportantComponentNames =
    {
        "PlayerMovement",
        "PlayerLook",
        "PlayerInteraction",
        "PlayerInventory",
        "PlayerInput",
        "CharacterController",
        "DrawerInteractable",
        "ExitKeyPickup",
        "NoiseSystem",
        "NoiseListenerTest",
        "TestInteractable",
        "FollowTarget",
        "NavMeshAgent",
        "NavMeshSurface",
        "Terrain",
        "TerrainCollider",
        "AudioListener",
        "EventSystem"
    };

    [MenuItem(MenuRoot + "Export Quick Report")]
    private static void ExportQuickReport()
    {
        ExportActiveSceneAudit(false);
    }

    [MenuItem(MenuRoot + "Export Full Report")]
    private static void ExportFullReport()
    {
        bool shouldStart = EditorUtility.DisplayDialog(
            "Full Scene Audit",
            "The full audit also checks serialized object references and scene dependencies. " +
            "On a very large scene this can take several minutes. The scene will not be changed.",
            "Start Full Audit",
            "Cancel");

        if (shouldStart)
        {
            ExportActiveSceneAudit(true);
        }
    }

    [MenuItem(MenuRoot + "Export Quick Report", true)]
    private static bool CanExportQuickReport()
    {
        return CanExportReport();
    }

    [MenuItem(MenuRoot + "Export Full Report", true)]
    private static bool CanExportFullReport()
    {
        return CanExportReport();
    }

    private static bool CanExportReport()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        return !EditorApplication.isPlaying && activeScene.IsValid() && activeScene.isLoaded;
    }

    private static void ExportActiveSceneAudit(bool fullAudit)
    {
        Scene scene = SceneManager.GetActiveScene();

        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Scene Audit", "Exit Play Mode before exporting the report.", "OK");
            return;
        }

        if (!scene.IsValid() || !scene.isLoaded)
        {
            EditorUtility.DisplayDialog("Scene Audit", "Open the scene that you want to inspect first.", "OK");
            return;
        }

        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);
        string modeName = fullAudit ? "Full" : "Quick";
        string safeSceneName = MakeSafeFileName(string.IsNullOrWhiteSpace(scene.name) ? "UnsavedScene" : scene.name);
        string reportFolder = Path.Combine(
            projectRoot,
            "SceneAuditReports",
            safeSceneName + "_" + timestamp + "_" + modeName);

        Directory.CreateDirectory(reportFolder);

        try
        {
            AuditScene(scene, reportFolder, fullAudit);
            EditorUtility.RevealInFinder(reportFolder);
            Debug.Log("Scene audit completed. Report folder: " + reportFolder);
            EditorUtility.DisplayDialog(
                "Scene Audit Complete",
                "The report was created successfully.\n\n" + reportFolder +
                "\n\nCompress this folder to ZIP and send it for review.",
                "OK");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorUtility.DisplayDialog(
                "Scene Audit Failed",
                "The audit stopped because of an error. Copy the red error from CONSOLE and send it for review.",
                "OK");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private static void AuditScene(Scene scene, string reportFolder, bool fullAudit)
    {
        AuditState state = new AuditState();
        string hierarchyPath = Path.Combine(reportFolder, "01_HIERARCHY.tsv");
        string problemsPath = Path.Combine(reportFolder, "02_PROBLEMS.tsv");
        string prefabsPath = Path.Combine(reportFolder, "03_PREFAB_INSTANCES.tsv");
        string importantObjectsPath = Path.Combine(reportFolder, "04_IMPORTANT_OBJECTS.tsv");

        using (StreamWriter hierarchyWriter = CreateWriter(hierarchyPath))
        using (StreamWriter problemsWriter = CreateWriter(problemsPath))
        using (StreamWriter prefabsWriter = CreateWriter(prefabsPath))
        using (StreamWriter importantWriter = CreateWriter(importantObjectsPath))
        {
            hierarchyWriter.WriteLine(
                "Index\tDepth\tPath\tActiveSelf\tActiveInHierarchy\tLayer\tTag\tStatic\t" +
                "LocalPosition\tLocalRotationEuler\tLocalScale\tComponents\tPrefabStatus\tPrefabSource");
            problemsWriter.WriteLine("Severity\tCategory\tObjectPath\tComponent\tDetails");
            prefabsWriter.WriteLine("InstanceRootPath\tStatus\tSourcePrefab");
            importantWriter.WriteLine("ObjectPath\tMatchedBy\tComponents");

            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                GameObject root = roots[rootIndex];
                int beforeRootCount = state.GameObjectCount;
                ScanRoot(
                    root,
                    rootIndex,
                    roots.Length,
                    fullAudit,
                    state,
                    hierarchyWriter,
                    problemsWriter,
                    prefabsWriter,
                    importantWriter);
                state.RootObjectCounts[BuildPathPart(root.transform)] = state.GameObjectCount - beforeRootCount;
            }

            AddSceneLevelFindings(scene, state, problemsWriter);
        }

        WriteSummary(scene, reportFolder, fullAudit, state);
        WriteComponentCounts(reportFolder, state.ComponentCounts);
        WriteRepeatedNames(reportFolder, state.NameStats);
        WritePrefabCounts(reportFolder, state.PrefabSourceCounts);
        WriteRootCounts(reportFolder, state.RootObjectCounts);
        WriteScriptInventory(reportFolder, state);

        if (fullAudit)
        {
            WriteDependencies(scene, reportFolder, state);
        }

        WriteReadMe(reportFolder, fullAudit);
    }

    private static void ScanRoot(
        GameObject root,
        int rootIndex,
        int rootCount,
        bool fullAudit,
        AuditState state,
        StreamWriter hierarchyWriter,
        StreamWriter problemsWriter,
        StreamWriter prefabsWriter,
        StreamWriter importantWriter)
    {
        Stack<HierarchyNode> stack = new Stack<HierarchyNode>();
        stack.Push(new HierarchyNode(root.transform, BuildPathPart(root.transform), 0));

        while (stack.Count > 0)
        {
            HierarchyNode node = stack.Pop();
            Transform transform = node.Transform;
            GameObject gameObject = transform.gameObject;
            state.GameObjectCount++;

            if (gameObject.activeSelf)
            {
                state.ActiveSelfCount++;
            }
            else
            {
                state.InactiveSelfCount++;
            }

            RecordName(state, gameObject.name, node.Path);

            Component[] components = gameObject.GetComponents<Component>();
            List<string> componentNames = new List<string>(components.Length);
            bool isImportant = NameLooksImportant(gameObject.name);
            string importantReason = isImportant ? "GameObject name" : string.Empty;

            int nullComponentCount = 0;
            foreach (Component component in components)
            {
                if (component == null)
                {
                    nullComponentCount++;
                    continue;
                }

                Type componentType = component.GetType();
                string componentName = componentType.Name;
                string fullComponentName = componentType.FullName ?? componentName;
                componentNames.Add(fullComponentName);
                Increment(state.ComponentCounts, componentName);
                state.ComponentCount++;

                if (ImportantComponentNames.Contains(componentName, StringComparer.OrdinalIgnoreCase))
                {
                    isImportant = true;
                    importantReason = AppendReason(importantReason, componentName);
                }

                CheckKnownComponentProblems(
                    component,
                    componentName,
                    node.Path,
                    state,
                    problemsWriter);

                if (fullAudit)
                {
                    FindMissingSerializedReferences(component, node.Path, problemsWriter, state);
                }
            }

            int missingScriptCount = Math.Max(
                nullComponentCount,
                GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject));

            if (missingScriptCount > 0)
            {
                state.MissingScriptCount += missingScriptCount;
                WriteProblem(
                    problemsWriter,
                    "ERROR",
                    "Missing Script",
                    node.Path,
                    "MonoBehaviour",
                    missingScriptCount + " missing script component(s)");
            }

            PrefabInstanceStatus prefabStatus = PrefabUtility.GetPrefabInstanceStatus(gameObject);
            string prefabSource = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);

            if (PrefabUtility.GetNearestPrefabInstanceRoot(gameObject) == gameObject)
            {
                state.PrefabInstanceCount++;
                string sourceForReport = string.IsNullOrEmpty(prefabSource) ? "<missing or unavailable>" : prefabSource;
                prefabsWriter.WriteLine(
                    EscapeTsv(node.Path) + "\t" + prefabStatus + "\t" + EscapeTsv(sourceForReport));
                Increment(state.PrefabSourceCounts, sourceForReport);

                if (prefabStatus == PrefabInstanceStatus.MissingAsset)
                {
                    state.MissingPrefabCount++;
                    WriteProblem(
                        problemsWriter,
                        "ERROR",
                        "Missing Prefab Asset",
                        node.Path,
                        "Prefab Instance",
                        "The source prefab cannot be found");
                }
            }

            if (isImportant)
            {
                importantWriter.WriteLine(
                    EscapeTsv(node.Path) + "\t" +
                    EscapeTsv(importantReason) + "\t" +
                    EscapeTsv(string.Join(", ", componentNames)));
            }

            hierarchyWriter.WriteLine(
                state.GameObjectCount + "\t" +
                node.Depth + "\t" +
                EscapeTsv(node.Path) + "\t" +
                gameObject.activeSelf + "\t" +
                gameObject.activeInHierarchy + "\t" +
                gameObject.layer + "\t" +
                EscapeTsv(GetSafeTag(gameObject)) + "\t" +
                gameObject.isStatic + "\t" +
                FormatVector3(transform.localPosition) + "\t" +
                FormatVector3(transform.localEulerAngles) + "\t" +
                FormatVector3(transform.localScale) + "\t" +
                EscapeTsv(string.Join(", ", componentNames)) + "\t" +
                prefabStatus + "\t" +
                EscapeTsv(prefabSource));

            if (state.GameObjectCount % 250 == 0)
            {
                float progress = rootCount == 0 ? 0f : (rootIndex + 0.5f) / rootCount;
                EditorUtility.DisplayProgressBar(
                    "Scene Audit",
                    "Scanning object " + state.GameObjectCount + ": " + gameObject.name,
                    Mathf.Clamp01(progress));
            }

            for (int childIndex = transform.childCount - 1; childIndex >= 0; childIndex--)
            {
                Transform child = transform.GetChild(childIndex);
                string childPath = node.Path + "/" + BuildPathPart(child);
                stack.Push(new HierarchyNode(child, childPath, node.Depth + 1));
            }
        }
    }

    private static void CheckKnownComponentProblems(
        Component component,
        string componentName,
        string objectPath,
        AuditState state,
        StreamWriter problemsWriter)
    {
        if (component is Terrain terrain)
        {
            state.TerrainCount++;

            if (terrain.terrainData == null)
            {
                WriteProblem(problemsWriter, "ERROR", "Terrain Data", objectPath, componentName, "TerrainData is missing");
            }

            if (component.GetComponent<TerrainCollider>() == null)
            {
                state.TerrainWithoutColliderCount++;
                WriteProblem(
                    problemsWriter,
                    "ERROR",
                    "Terrain Collider",
                    objectPath,
                    componentName,
                    "Terrain has no TerrainCollider on the same GameObject");
            }
        }

        if (component is TerrainCollider terrainCollider && terrainCollider.terrainData == null)
        {
            WriteProblem(
                problemsWriter,
                "ERROR",
                "Terrain Collider",
                objectPath,
                componentName,
                "TerrainCollider has no TerrainData");
        }

        if (component is MeshFilter meshFilter)
        {
            CheckEmbeddedMesh(meshFilter.sharedMesh, objectPath, componentName, state, problemsWriter);
        }
        else if (component is SkinnedMeshRenderer skinnedMeshRenderer)
        {
            CheckEmbeddedMesh(skinnedMeshRenderer.sharedMesh, objectPath, componentName, state, problemsWriter);
        }
        else if (component is MeshCollider meshCollider)
        {
            CheckEmbeddedMesh(meshCollider.sharedMesh, objectPath, componentName, state, problemsWriter);
        }
    }

    private static void CheckEmbeddedMesh(
        Mesh mesh,
        string objectPath,
        string componentName,
        AuditState state,
        StreamWriter problemsWriter)
    {
        if (mesh == null || !state.CheckedMeshInstanceIds.Add(mesh.GetInstanceID()))
        {
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(mesh);
        if (string.IsNullOrEmpty(assetPath) && mesh.vertexCount >= 1000)
        {
            state.EmbeddedLargeMeshCount++;
            state.EmbeddedLargeMeshVertexCount += mesh.vertexCount;
            WriteProblem(
                problemsWriter,
                "REVIEW",
                "Embedded Mesh",
                objectPath,
                componentName,
                "Mesh '" + EscapeTsv(mesh.name) + "' has " + mesh.vertexCount +
                " vertices and no external asset path. Many embedded meshes can make a Scene very large.");
        }
    }

    private static void FindMissingSerializedReferences(
        Component component,
        string objectPath,
        StreamWriter problemsWriter,
        AuditState state)
    {
        if (component is Transform)
        {
            return;
        }

        try
        {
            SerializedObject serializedObject = new SerializedObject(component);
            SerializedProperty property = serializedObject.GetIterator();
            while (property.NextVisible(true))
            {
                if (property.propertyPath == "m_Script")
                {
                    continue;
                }

                if (property.propertyType == SerializedPropertyType.ObjectReference &&
                    property.objectReferenceValue == null &&
                    property.objectReferenceInstanceIDValue != 0)
                {
                    state.MissingReferenceCount++;
                    WriteProblem(
                        problemsWriter,
                        "ERROR",
                        "Missing Object Reference",
                        objectPath,
                        component.GetType().Name,
                        "Serialized field: " + property.propertyPath);
                }
            }
        }
        catch (Exception exception)
        {
            state.SerializedScanFailureCount++;
            WriteProblem(
                problemsWriter,
                "REVIEW",
                "Serialized Scan Failure",
                objectPath,
                component.GetType().Name,
                exception.GetType().Name + ": " + exception.Message);
        }
    }

    private static void AddSceneLevelFindings(Scene scene, AuditState state, StreamWriter problemsWriter)
    {
        long sceneFileSize = GetProjectAssetFileSize(scene.path);
        state.SceneFileSizeBytes = sceneFileSize;

        if (sceneFileSize >= LargeSceneWarningBytes)
        {
            WriteProblem(
                problemsWriter,
                "WARNING",
                "Very Large Scene File",
                scene.path,
                "Scene",
                "Scene size is " + FormatBytes(sceneFileSize) +
                ". Check embedded meshes and very large root hierarchies.");
        }

        int noiseSystemCount = GetCount(state.ComponentCounts, "NoiseSystem");
        if (noiseSystemCount != 1)
        {
            WriteProblem(
                problemsWriter,
                "ERROR",
                "NoiseSystem Count",
                scene.name,
                "NoiseSystem",
                "Expected exactly 1 in the gameplay scene, found " + noiseSystemCount);
        }

        int playerMovementCount = GetCount(state.ComponentCounts, "PlayerMovement");
        if (playerMovementCount != 1)
        {
            WriteProblem(
                problemsWriter,
                "WARNING",
                "Player Count",
                scene.name,
                "PlayerMovement",
                "Expected 1 player movement component, found " + playerMovementCount);
        }

        int testComponentCount =
            GetCount(state.ComponentCounts, "NoiseListenerTest") +
            GetCount(state.ComponentCounts, "TestInteractable");
        if (testComponentCount > 0)
        {
            WriteProblem(
                problemsWriter,
                "REVIEW",
                "Test Components",
                scene.name,
                "Scene",
                "Found " + testComponentCount + " test component(s). Confirm they do not belong in the presentation scene.");
        }
    }

    private static void WriteSummary(Scene scene, string reportFolder, bool fullAudit, AuditState state)
    {
        string summaryPath = Path.Combine(reportFolder, "00_SUMMARY.txt");
        using (StreamWriter writer = CreateWriter(summaryPath))
        {
            writer.WriteLine("KILLER ON LINE 1 - SCENE AUDIT SUMMARY");
            writer.WriteLine("=======================================");
            writer.WriteLine("Read-only audit: the tool did not save or modify the Scene.");
            writer.WriteLine();
            writer.WriteLine("Audit mode: " + (fullAudit ? "Full" : "Quick"));
            writer.WriteLine("Unity version: " + Application.unityVersion);
            writer.WriteLine("Scene name: " + scene.name);
            writer.WriteLine("Scene path: " + (string.IsNullOrEmpty(scene.path) ? "<unsaved>" : scene.path));
            writer.WriteLine("Scene dirty flag after audit: " + scene.isDirty);
            writer.WriteLine("Scene file size: " + FormatBytes(state.SceneFileSizeBytes));
            writer.WriteLine("GameObjects: " + state.GameObjectCount);
            writer.WriteLine("ActiveSelf GameObjects: " + state.ActiveSelfCount);
            writer.WriteLine("InactiveSelf GameObjects: " + state.InactiveSelfCount);
            writer.WriteLine("Components: " + state.ComponentCount);
            writer.WriteLine("Prefab instance roots: " + state.PrefabInstanceCount);
            writer.WriteLine("Missing prefab assets: " + state.MissingPrefabCount);
            writer.WriteLine("Missing scripts: " + state.MissingScriptCount);
            writer.WriteLine("Missing serialized references: " +
                             (fullAudit ? state.MissingReferenceCount.ToString() : "not checked in Quick mode"));
            writer.WriteLine("Serialized components that could not be scanned: " +
                             (fullAudit ? state.SerializedScanFailureCount.ToString() : "not checked in Quick mode"));
            writer.WriteLine("Terrains: " + state.TerrainCount);
            writer.WriteLine("Terrains without TerrainCollider: " + state.TerrainWithoutColliderCount);
            writer.WriteLine("Large embedded meshes: " + state.EmbeddedLargeMeshCount);
            writer.WriteLine("Vertices in large embedded meshes: " + state.EmbeddedLargeMeshVertexCount);
            writer.WriteLine();
            writer.WriteLine("IMPORTANT COMPONENT COUNTS");
            writer.WriteLine("--------------------------");

            foreach (string componentName in ImportantComponentNames)
            {
                writer.WriteLine(componentName + ": " + GetCount(state.ComponentCounts, componentName));
            }

            writer.WriteLine();
            writer.WriteLine("FILES IN THIS REPORT");
            writer.WriteLine("--------------------");
            writer.WriteLine("00_SUMMARY.txt - overview and important counts");
            writer.WriteLine("01_HIERARCHY.tsv - every GameObject and its components");
            writer.WriteLine("02_PROBLEMS.tsv - missing items and review warnings");
            writer.WriteLine("03_PREFAB_INSTANCES.tsv - prefab instances and source assets");
            writer.WriteLine("04_IMPORTANT_OBJECTS.tsv - Player, Lauren, noise, navigation and gameplay objects");
            writer.WriteLine("05_COMPONENT_COUNTS.tsv - component totals");
            writer.WriteLine("06_REPEATED_NAMES.tsv - repeated names; repetition is not automatically an error");
            writer.WriteLine("07_PREFAB_SOURCE_COUNTS.tsv - number of instances per source prefab");
            writer.WriteLine("08_ROOT_OBJECT_COUNTS.tsv - hierarchy size under each root object");
            writer.WriteLine("09_SCRIPT_INVENTORY.tsv - first-party scripts found under Assets");
            writer.WriteLine("10_SCRIPT_PROBLEMS.tsv - duplicate script names/classes and Script/Scripts folder warning");

            if (fullAudit)
            {
                writer.WriteLine("11_DEPENDENCIES.tsv - assets referenced by the active Scene");
            }
        }
    }

    private static void WriteComponentCounts(string reportFolder, Dictionary<string, int> counts)
    {
        using (StreamWriter writer = CreateWriter(Path.Combine(reportFolder, "05_COMPONENT_COUNTS.tsv")))
        {
            writer.WriteLine("ComponentType\tCount");
            foreach (KeyValuePair<string, int> item in counts.OrderByDescending(item => item.Value).ThenBy(item => item.Key))
            {
                writer.WriteLine(EscapeTsv(item.Key) + "\t" + item.Value);
            }
        }
    }

    private static void WriteRepeatedNames(string reportFolder, Dictionary<string, NameStat> nameStats)
    {
        using (StreamWriter writer = CreateWriter(Path.Combine(reportFolder, "06_REPEATED_NAMES.tsv")))
        {
            writer.WriteLine("GameObjectName\tCount\tSamplePaths\tNote");
            foreach (KeyValuePair<string, NameStat> item in nameStats
                         .Where(item => item.Value.Count > 1)
                         .OrderByDescending(item => item.Value.Count)
                         .ThenBy(item => item.Key))
            {
                writer.WriteLine(
                    EscapeTsv(item.Key) + "\t" +
                    item.Value.Count + "\t" +
                    EscapeTsv(string.Join(" | ", item.Value.SamplePaths)) + "\t" +
                    "Repeated names are review candidates, not automatic deletion targets");
            }
        }
    }

    private static void WritePrefabCounts(string reportFolder, Dictionary<string, int> counts)
    {
        using (StreamWriter writer = CreateWriter(Path.Combine(reportFolder, "07_PREFAB_SOURCE_COUNTS.tsv")))
        {
            writer.WriteLine("SourcePrefab\tInstanceCount");
            foreach (KeyValuePair<string, int> item in counts.OrderByDescending(item => item.Value).ThenBy(item => item.Key))
            {
                writer.WriteLine(EscapeTsv(item.Key) + "\t" + item.Value);
            }
        }
    }

    private static void WriteRootCounts(string reportFolder, Dictionary<string, int> counts)
    {
        using (StreamWriter writer = CreateWriter(Path.Combine(reportFolder, "08_ROOT_OBJECT_COUNTS.tsv")))
        {
            writer.WriteLine("RootGameObject\tObjectsInSubtree");
            foreach (KeyValuePair<string, int> item in counts.OrderByDescending(item => item.Value).ThenBy(item => item.Key))
            {
                writer.WriteLine(EscapeTsv(item.Key) + "\t" + item.Value);
            }
        }
    }

    private static void WriteScriptInventory(string reportFolder, AuditState state)
    {
        string[] scriptGuids = AssetDatabase.FindAssets("t:MonoScript", new[] { "Assets" });
        Dictionary<string, List<string>> pathsByFileName = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, List<string>> pathsByClassName = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        bool hasScriptFolder = false;
        bool hasScriptsFolder = false;

        using (StreamWriter inventoryWriter = CreateWriter(Path.Combine(reportFolder, "09_SCRIPT_INVENTORY.tsv")))
        using (StreamWriter problemsWriter = CreateWriter(Path.Combine(reportFolder, "10_SCRIPT_PROBLEMS.tsv")))
        {
            inventoryWriter.WriteLine("AssetPath\tFileName\tCompiledClass");
            problemsWriter.WriteLine("Severity\tCategory\tDetails\tPaths");

            foreach (string guid in scriptGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Path.GetFileName(path);
                string className = string.Empty;

                if (path.IndexOf("/Script/", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    hasScriptFolder = true;
                }

                if (path.IndexOf("/Scripts/", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    hasScriptsFolder = true;
                }

                try
                {
                    MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                    Type compiledClass = monoScript == null ? null : monoScript.GetClass();
                    className = compiledClass == null ? "<not compiled or no class>" : compiledClass.FullName;
                }
                catch (Exception exception)
                {
                    className = "<error: " + exception.GetType().Name + ">";
                }

                inventoryWriter.WriteLine(
                    EscapeTsv(path) + "\t" + EscapeTsv(fileName) + "\t" + EscapeTsv(className));
                AddPath(pathsByFileName, fileName, path);

                if (!string.IsNullOrEmpty(className) && !className.StartsWith("<", StringComparison.Ordinal))
                {
                    AddPath(pathsByClassName, className, path);
                }
            }

            if (hasScriptFolder && hasScriptsFolder)
            {
                state.HasScriptAndScriptsFolders = true;
                problemsWriter.WriteLine(
                    "REVIEW\tScript Folder Naming\tBoth folders named 'Script' and 'Scripts' exist under Assets\t" +
                    "Search 09_SCRIPT_INVENTORY.tsv before moving anything");
            }

            foreach (KeyValuePair<string, List<string>> item in pathsByFileName.Where(item => item.Value.Count > 1))
            {
                problemsWriter.WriteLine(
                    "WARNING\tDuplicate Script File Name\t" + EscapeTsv(item.Key) + "\t" +
                    EscapeTsv(string.Join(" | ", item.Value)));
            }

            foreach (KeyValuePair<string, List<string>> item in pathsByClassName.Where(item => item.Value.Count > 1))
            {
                problemsWriter.WriteLine(
                    "ERROR\tDuplicate Compiled Class\t" + EscapeTsv(item.Key) + "\t" +
                    EscapeTsv(string.Join(" | ", item.Value)));
            }
        }
    }

    private static void WriteDependencies(Scene scene, string reportFolder, AuditState state)
    {
        using (StreamWriter writer = CreateWriter(Path.Combine(reportFolder, "11_DEPENDENCIES.tsv")))
        {
            writer.WriteLine("AssetPath\tFileSizeBytes\tReadableSize\tExtension");

            if (string.IsNullOrEmpty(scene.path))
            {
                writer.WriteLine("<scene is unsaved>\t0\t0 B\t");
                return;
            }

            string[] dependencies = AssetDatabase.GetDependencies(scene.path, true);
            List<DependencyInfo> dependencyInfos = new List<DependencyInfo>(dependencies.Length);

            foreach (string dependency in dependencies)
            {
                long size = GetProjectAssetFileSize(dependency);
                dependencyInfos.Add(new DependencyInfo(dependency, size));
                state.DependencyCount++;
                state.DependencySizeBytes += size;
            }

            foreach (DependencyInfo dependency in dependencyInfos
                         .OrderByDescending(item => item.SizeBytes)
                         .ThenBy(item => item.AssetPath))
            {
                writer.WriteLine(
                    EscapeTsv(dependency.AssetPath) + "\t" +
                    dependency.SizeBytes + "\t" +
                    FormatBytes(dependency.SizeBytes) + "\t" +
                    EscapeTsv(Path.GetExtension(dependency.AssetPath)));
            }
        }
    }

    private static void WriteReadMe(string reportFolder, bool fullAudit)
    {
        using (StreamWriter writer = CreateWriter(Path.Combine(reportFolder, "README_SEND_ME.txt")))
        {
            writer.WriteLine("1. Do not edit the report files.");
            writer.WriteLine("2. Close them if they are open in Excel or another program.");
            writer.WriteLine("3. Compress this entire folder to one ZIP file.");
            writer.WriteLine("4. Upload the ZIP in the same ChatGPT conversation.");
            writer.WriteLine("5. Also send a screenshot of Unity CONSOLE if it contains red errors.");
            writer.WriteLine();
            writer.WriteLine("Audit mode used: " + (fullAudit ? "Full" : "Quick"));
            writer.WriteLine("The exporter is read-only and did not save the active Scene.");
        }
    }

    private static StreamWriter CreateWriter(string path)
    {
        return new StreamWriter(path, false, new UTF8Encoding(true));
    }

    private static void RecordName(AuditState state, string objectName, string objectPath)
    {
        if (!state.NameStats.TryGetValue(objectName, out NameStat stat))
        {
            stat = new NameStat();
            state.NameStats.Add(objectName, stat);
        }

        stat.Count++;
        if (stat.SamplePaths.Count < NamePathSampleLimit)
        {
            stat.SamplePaths.Add(objectPath);
        }
    }

    private static void AddPath(Dictionary<string, List<string>> dictionary, string key, string path)
    {
        if (!dictionary.TryGetValue(key, out List<string> paths))
        {
            paths = new List<string>();
            dictionary.Add(key, paths);
        }

        paths.Add(path);
    }

    private static void Increment(Dictionary<string, int> dictionary, string key)
    {
        dictionary[key] = GetCount(dictionary, key) + 1;
    }

    private static int GetCount(Dictionary<string, int> dictionary, string key)
    {
        return dictionary.TryGetValue(key, out int count) ? count : 0;
    }

    private static bool NameLooksImportant(string objectName)
    {
        string lowerName = objectName.ToLowerInvariant();
        return lowerName.Contains("player") ||
               lowerName.Contains("lauren") ||
               lowerName.Contains("noise") ||
               lowerName.Contains("drawer") ||
               lowerName.Contains("terrain") ||
               lowerName.Contains("navmesh") ||
               lowerName.Contains("exit") ||
               lowerName.Contains("key") ||
               lowerName.Contains("hide");
    }

    private static string AppendReason(string current, string next)
    {
        return string.IsNullOrEmpty(current) ? next : current + ", " + next;
    }

    private static string BuildPathPart(Transform transform)
    {
        return EscapeTsv(transform.name) + "[" + transform.GetSiblingIndex() + "]";
    }

    private static string GetSafeTag(GameObject gameObject)
    {
        try
        {
            return gameObject.tag;
        }
        catch
        {
            return "<invalid tag>";
        }
    }

    private static string FormatVector3(Vector3 value)
    {
        return value.x.ToString("0.###", CultureInfo.InvariantCulture) + "," +
               value.y.ToString("0.###", CultureInfo.InvariantCulture) + "," +
               value.z.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static string EscapeTsv(string value)
    {
        return string.IsNullOrEmpty(value)
            ? string.Empty
            : value.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
    }

    private static string MakeSafeFileName(string value)
    {
        foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(invalidCharacter, '_');
        }

        return value;
    }

    private static long GetProjectAssetFileSize(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath))
        {
            return 0;
        }

        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string fullPath = Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(fullPath) ? new FileInfo(fullPath).Length : 0;
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024)
        {
            return bytes + " B";
        }

        double kilobytes = bytes / 1024d;
        if (kilobytes < 1024d)
        {
            return kilobytes.ToString("0.##", CultureInfo.InvariantCulture) + " KB";
        }

        double megabytes = kilobytes / 1024d;
        if (megabytes < 1024d)
        {
            return megabytes.ToString("0.##", CultureInfo.InvariantCulture) + " MB";
        }

        double gigabytes = megabytes / 1024d;
        return gigabytes.ToString("0.##", CultureInfo.InvariantCulture) + " GB";
    }

    private static void WriteProblem(
        StreamWriter writer,
        string severity,
        string category,
        string objectPath,
        string component,
        string details)
    {
        writer.WriteLine(
            EscapeTsv(severity) + "\t" +
            EscapeTsv(category) + "\t" +
            EscapeTsv(objectPath) + "\t" +
            EscapeTsv(component) + "\t" +
            EscapeTsv(details));
    }

    private readonly struct HierarchyNode
    {
        public HierarchyNode(Transform transform, string path, int depth)
        {
            Transform = transform;
            Path = path;
            Depth = depth;
        }

        public Transform Transform { get; }
        public string Path { get; }
        public int Depth { get; }
    }

    private readonly struct DependencyInfo
    {
        public DependencyInfo(string assetPath, long sizeBytes)
        {
            AssetPath = assetPath;
            SizeBytes = sizeBytes;
        }

        public string AssetPath { get; }
        public long SizeBytes { get; }
    }

    private sealed class NameStat
    {
        public int Count;
        public readonly List<string> SamplePaths = new List<string>();
    }

    private sealed class AuditState
    {
        public int GameObjectCount;
        public int ActiveSelfCount;
        public int InactiveSelfCount;
        public int ComponentCount;
        public int PrefabInstanceCount;
        public int MissingPrefabCount;
        public int MissingScriptCount;
        public int MissingReferenceCount;
        public int SerializedScanFailureCount;
        public int TerrainCount;
        public int TerrainWithoutColliderCount;
        public int EmbeddedLargeMeshCount;
        public long EmbeddedLargeMeshVertexCount;
        public int DependencyCount;
        public long DependencySizeBytes;
        public long SceneFileSizeBytes;
        public bool HasScriptAndScriptsFolders;

        public readonly Dictionary<string, int> ComponentCounts =
            new Dictionary<string, int>(StringComparer.Ordinal);

        public readonly Dictionary<string, NameStat> NameStats =
            new Dictionary<string, NameStat>(StringComparer.Ordinal);

        public readonly Dictionary<string, int> PrefabSourceCounts =
            new Dictionary<string, int>(StringComparer.Ordinal);

        public readonly Dictionary<string, int> RootObjectCounts =
            new Dictionary<string, int>(StringComparer.Ordinal);

        public readonly HashSet<int> CheckedMeshInstanceIds = new HashSet<int>();
    }
}
