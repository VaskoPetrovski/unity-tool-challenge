using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class StoreImport : EditorWindow
{
    private static string modelsPath = "Assets/1_Graphics/Models";
    private static string iconsPath = "Assets/1_Graphics/Store";

    [MenuItem("Tools/Store import")]
    public static void ShowWindow()
    {
        GetWindow(typeof(StoreImport));
    }

    private string direcotryPath = "";

    private GameObject fbxGameObject;
    private Material characterMaterial;
    private Sprite charaterIcon;
    private string characterName = "New Character";
    private string externalAssetsName = "AssetName";
    private int characterPrice = 100;
    private bool isPrefabVariant = true;

    public string fbxPath = "";
    public string iconPath = "";
    public int tab = 0;

    private List<GameObject> unusedModels = new List<GameObject>();
    private Dictionary<int, bool> selectedModels = new Dictionary<int, bool>();


    // private string sheet_id = "";
    // private string sheet_guid = "";


    /// <summary>
    /// Renders an Editor Window with a toolbar for choosing the import method
    /// Project: Import a new character from files present in the project
    /// Import external: Import a new character from external files, present in the computer
    /// Forgotten assets: Lists all fbx files that aren't used in the store, that may be forgotten when importing new characters
    /// </summary>
    private void OnGUI()
    {
        tab = GUILayout.Toolbar(tab, new string[] { "Project", "Import external", "Forgotten assets" });
        switch (tab)
        {
            case 0:
                RenderNewModelImportTab();
                break;
            case 1:
                RenderExternalImportTab();
                break;
            case 2:
                RenderUnusedAssetsTab();
                break;
        }
    }

    private void RenderUnusedAssetsTab()
    {
        GUILayout.Label("Forgotten assets", EditorStyles.boldLabel);
        if (GUILayout.Button("Search Forgotten Assets"))
        {
            unusedModels = TestPrefabVariant();
        }

        RenderUnusedFBXModels();
        var optionsEnabled = selectedModels.Count(pair => pair.Value) > 0;
        if (!optionsEnabled) GUILayout.Label("Please select the FBX file and the icon before continuing");
        GUI.enabled = optionsEnabled;
        GUILayout.Label("Prefab Variant Settings", EditorStyles.boldLabel);
        characterPrice = EditorGUILayout.IntField("Price", characterPrice);
        characterName = EditorGUILayout.TextField("Name", characterName);
        charaterIcon = (Sprite)EditorGUILayout.ObjectField("Icon", charaterIcon, typeof(Sprite), false);
        characterMaterial ??=
            (Material)AssetDatabase.LoadAssetAtPath("Assets/1_Graphics/Materials/Blue.mat", typeof(Material));
        isPrefabVariant = GUILayout.Toggle(isPrefabVariant, "Create Prefab As Variant");
        if (selectedModels.Count(pair => pair.Value) > 1)
        {
            GUILayout.Label("<color=red><size=20>NOTE: All of the selected prefabs will have the same icon name and cost</size></color>",new GUIStyle());
        }
        if (GUILayout.Button("Create Prefab Variants"))
        {
            AddSelectedUnusedFBXModels();
        }
        GUI.enabled = true;
        

    }

    private void RenderExternalImportTab()
    {
        GUILayout.Label("Asset Name", EditorStyles.boldLabel);
        externalAssetsName = GUILayout.TextField(externalAssetsName);
        GUILayout.Label("Select the FBX file");
        GUILayout.BeginHorizontal();
        GUI.enabled = false;
        GUILayout.TextField(fbxPath);
        GUI.enabled = true;
        if (GUILayout.Button("Select FBX")) OpenFBXFileExplorer();
        GUILayout.EndHorizontal();
        GUILayout.Label("Select the Icon file");
        GUILayout.BeginHorizontal();
        GUI.enabled = false;
        GUILayout.TextField(iconPath);
        GUI.enabled = true;
        if (GUILayout.Button("Select Icon")) OpenIconFileExplorer();
        GUILayout.EndHorizontal();
        GUILayout.Label($"Files will be copied to:");
        GUILayout.Label($"Models: {Path.Combine(modelsPath, $"{externalAssetsName}.FBX")}");
        GUILayout.Label($"Icons: {Path.Combine(iconsPath, $"{externalAssetsName}.png")}");
        if (GUILayout.Button("Copy Assets"))
        {
            CreatePrefabFromExternal();
        }
        
    }

    private void RenderNewModelImportTab()
    {
        GUILayout.Label("Project", EditorStyles.boldLabel);
        GUILayout.Label("Import New Store Items", EditorStyles.boldLabel);

        GUILayout.Label("Select the FBX file");
        fbxGameObject = (GameObject)EditorGUILayout.ObjectField("FBX file", fbxGameObject, typeof(GameObject), false);
        charaterIcon = (Sprite)EditorGUILayout.ObjectField("Icon", charaterIcon, typeof(Sprite), false);

        var optionsEnabled = fbxGameObject != null && charaterIcon != null;
        if (!optionsEnabled) GUILayout.Label("Please select the FBX file and the icon before continuing");
        GUI.enabled = optionsEnabled;
        characterPrice = EditorGUILayout.IntField("Price", characterPrice);
        characterName = EditorGUILayout.TextField("Name", characterName);
        // ??=  null coalescing assignment operator
        characterMaterial ??=
            (Material)AssetDatabase.LoadAssetAtPath("Assets/1_Graphics/Materials/Blue.mat", typeof(Material));
        GUILayout.Label("Select the Character Material (Blue material is selected by default)");
        characterMaterial =
            (Material)EditorGUILayout.ObjectField("Material", characterMaterial, typeof(Material), false);
        // fbxGameObject = EditorGUILayout.ObjectField("label")
        isPrefabVariant = GUILayout.Toggle(isPrefabVariant, "Create Prefab As Variant");
        if (GUILayout.Button("Create Prefab"))
        {
            CreatePrefab(fbxGameObject, charaterIcon, characterName, characterPrice);
        }

        GUI.enabled = true;
    }

    /// <summary>
    /// Renders A list of unused FBX models with a toggle to select them
    /// </summary>
    private void RenderUnusedFBXModels()
    {
        for (var i = 0; i < unusedModels.Count; i++)
        {
            GUILayout.BeginHorizontal();
            GUI.enabled = false;
            var _ = (GameObject)EditorGUILayout.ObjectField("Gameobject", unusedModels[i], typeof(GameObject), false);
            GUI.enabled = true;
            if (!selectedModels.ContainsKey(i))
                selectedModels.Add(i, false);
            selectedModels[i] = GUILayout.Toggle(selectedModels[i], "Import To Store");
            GUILayout.EndHorizontal();
        }
    }

    /// <summary>
    /// Adds the selected unused FBX models to the store as prefabs
    /// </summary>
    private void AddSelectedUnusedFBXModels()
    {
        foreach (var keyValuePair in selectedModels.Where(keyValuePair => keyValuePair.Value))
        {
            CreatePrefab(unusedModels[keyValuePair.Key], charaterIcon, characterName, characterPrice);
        }

        characterName = "New Character";
        characterPrice = 100;
        charaterIcon = null;
        selectedModels.Clear();
        unusedModels = TestPrefabVariant();
    }


    /// <summary>
    /// Adds a new entry to the StoreItems list in the Store Singleton GameObject present in the scene
    /// </summary>
    private void AddItemToStore(string name, int price, GameObject prefab, Sprite icon)
    {
        Store store = FindObjectOfType(typeof(Store)) as Store;
        if (store == null)
        {
            Debug.LogError("No store found");
        }
        else
        {
            var newStoreItem = new StoreItem
            {
                Name = name,
                // This does not guarantee that the id is unique, i will set this
                // id just for testing purposes Ideally the id should be a string
                // set by the GUID of the prefab instance which will guarantee that the id is unique
                Id = store.StoreItems.Count > 0 ? store.StoreItems[store.StoreItems.Count - 1].Id + 1 : 0,
                Price = price,
                Icon = icon,
                Prefab = prefab
            };

            store.GetComponent<Store>().StoreItems.Add(newStoreItem);
        }
    }

    /// <summary>
    /// Opens a file explorer to select the FBX file
    /// </summary>
    private void OpenFBXFileExplorer()
    {
        fbxPath = EditorUtility.OpenFilePanel("Select Model (FBX)", "", "fbx");
    }

    /// <summary>
    /// Opens a file explorer to select the png file used as an icon
    /// </summary>
    private void OpenIconFileExplorer()
    {
        iconPath = EditorUtility.OpenFilePanel("Select Icon (PNG)", "", "png");
    }


 

    /// <summary>
    /// When searching for fbx files to import, we want to ignore files that have "@" in their name assuming that they are animations.
    /// or files that are in the Animations folder. This is the case only for this particular project. other projects may not follow this rule,
    /// and this will probably need to be changed.A better solution would be to have a naming convention for the fbx models (E.g FBXCharacter_NAME.FBX)
    /// because it will allow for easier targeting on fbx character models.
    /// </summary>
    private static bool TestFBXFilePath(string file)
    {
        // Assuming animations will have @ in their name
        return file.EndsWith(".FBX") &&
               !Path.GetFileName(file).Contains("@") &&
               !Path.GetFileName(file).Contains("Animations");
    }


    /// <summary>
    /// Returns a list of paths for all of the FBX files in the project
    /// </summary>
    private static List<string> GetAllFbxFilePaths(string path)
    {
        List<string> files = new List<string>();
        foreach (var file in Directory.GetFiles(path))
        {
            if (TestFBXFilePath(file))
            {
                files.Add(AssetsRelativePath(file));
            }
        }

        // List<string> dirs = new List<string>();
        foreach (var directory in Directory.GetDirectories(path))
        {
            files = files.Concat(GetAllFbxFilePaths(directory)).ToList();
            GetAllFbxFilePaths(directory);
        }

        return files;
    }

    /// <summary>
    /// Return a  list of all FBX files in the project which do not have a prefab variant files that are not used in the Store -> StoreItems list
    /// </summary>
    private List<GameObject> TestPrefabVariant()
    {
        
        Dictionary<GameObject,bool> used = new Dictionary<GameObject, bool>();

        GetAllFbxFilePaths(Application.dataPath).ForEach((path) =>
        {
            // originalObjects.Add(AssetDatabase.LoadAssetAtPath<GameObject>(path));
            used.Add(AssetDatabase.LoadAssetAtPath<GameObject>(path), false);
        });
        
        Store store = FindObjectOfType(typeof(Store)) as Store;
        store.StoreItems.ForEach((item) =>
        {
            if (item.Prefab != null)
            {
                var original = PrefabUtility.GetCorrespondingObjectFromSource(item.Prefab);
                if (original != null && used.ContainsKey(original)) {
                    used[original] = true;
                }
             
            }
        });
        return used.Where(keyValuePair => !keyValuePair.Value).Select(keyValuePair => keyValuePair.Key).ToList();
    }
    

    /// <summary>
    /// Creates a prefab from the selected FBX file, icon, characterName and characterPrice then
    /// adds it to the StoreItems list in the Store Singleton GameObject.
    /// By default we add a capsule collider and an animator controller to the prefab with custom default presets
    /// Also we set the prefab to be a variant if the isPrefabVariant bool is true (this comes in handy when we want to find unused models)
    /// </summary>
    private void CreatePrefab(GameObject fbxObject, Sprite icon, string characterName, int characterPrice)
    {
        string prefabName = $"{characterName}";
        string localPath = "Assets/2_Prefabs/" + prefabName + ".prefab";
        GameObject prefab = PrefabUtility.InstantiatePrefab(fbxObject) as GameObject;
        // Debug.Log(prefab == null);

        // Animator
        var animator = prefab.GetComponent<Animator>();
        AnimatorController animatorController =
            (AnimatorController)AssetDatabase.LoadAssetAtPath(
                "Assets/1_Graphics/AnimatorControllers/Controller.controller", typeof(AnimatorController));
        animator.runtimeAnimatorController = animatorController;

        // Capsule Collider
        var capsuleCollider = prefab.AddComponent<CapsuleCollider>();
        capsuleCollider.center = new Vector3(0, 0.55f, 0);
        capsuleCollider.radius = 0.2f;
        capsuleCollider.height = 1.1f;
        capsuleCollider.direction = 1; // X=0, Y=1, Z=2

        // Material
        var skinnedMeshRenderer = prefab.GetComponentInChildren<SkinnedMeshRenderer>();
        Material material =
            (Material)AssetDatabase.LoadAssetAtPath("Assets/1_Graphics/Materials/Blue.mat", typeof(Material));
        skinnedMeshRenderer.materials = new Material[] { material, material }; // 1 - haed 2- body
        if (!isPrefabVariant)
            PrefabUtility.UnpackPrefabInstance(prefab, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
        var savedPrefab = PrefabUtility.SaveAsPrefabAsset(prefab, localPath);
        AddItemToStore(characterName, characterPrice, savedPrefab, icon);
        DestroyImmediate(prefab);
        EditorUtility.DisplayDialog("Success", "The new character has been added to the store", "OK");
    }

    /// <summary>
    /// Copies fbx and icon from external assets files and adding them to the project
    /// for further use in the CreatePrefab method
    ///
    /// In its current implementation, this is more of a POC(Proof of concept) because it requires manual reimport of the project in order work properly
    /// i did a quick google search and it seems like it requires quite an effort to make it work properly directly from the editor
    /// </summary>
    private void CreatePrefabFromExternal()
    {
        // var folderPath = Path.Combine("/Temp/StoreExternalImports");
        var copiedFbxPath = Path.Combine(modelsPath, $"{externalAssetsName}.FBX");
        var copiedIconPath = Path.Combine(iconsPath, $"{externalAssetsName}.png");
        if (File.Exists(copiedFbxPath)) File.Delete(copiedFbxPath);
        if (File.Exists(copiedIconPath)) File.Delete(copiedIconPath);
        FileUtil.CopyFileOrDirectory(fbxPath, copiedFbxPath);
        FileUtil.CopyFileOrDirectory(iconPath, copiedIconPath);
        AssetDatabase.Refresh();
        AssetDatabase.ImportAsset(copiedFbxPath, ImportAssetOptions.Default);
        TextureImporter textureImporter = AssetImporter.GetAtPath(copiedIconPath) as TextureImporter;
        textureImporter.textureType = TextureImporterType.Sprite;
        EditorUtility.SetDirty(textureImporter);
        textureImporter.SaveAndReimport();

        fbxGameObject = AssetDatabase.LoadAssetAtPath<GameObject>(copiedFbxPath);
        charaterIcon = AssetDatabase.LoadAssetAtPath<Sprite>(copiedIconPath);
        tab = 0;
        EditorUtility.DisplayDialog("Success", "FBX model and Icon copied to the project and pre-selected for creating new character", "OK");
    }

    /// <summary>
    /// Converts an absolute path to a path relative to the Project's Assets folder.
    /// </summary>
    private static string AssetsRelativePath(string absolutePath)
    {
        if (absolutePath.StartsWith(Application.dataPath))
        {
            return "Assets" + absolutePath.Substring(Application.dataPath.Length);
        }
        else
        {
            throw new System.ArgumentException("Full path does not contain the current project's Assets folder",
                "absolutePath");
        }
    }
}