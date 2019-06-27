using System.IO;
using System.Linq;

using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

using UnityEngine;

public class SetupUnityForGit : EditorWindow
{
    private struct ToggleOption
    {
        public bool IsOn;
        public readonly string Name;

        public ToggleOption(bool isOn, string name)
        {
            IsOn = isOn;
            Name = name;
        }
    }

    private const string kProgressTitle = "Setup Unity: ";

    private bool m_IsSetupGitFiles = true;
    private bool m_IsRemovingPackages = true;
    private bool hasRemoveAllPackages = true;

    private ToggleOption[] m_PackagesToRemove = new ToggleOption[] { };

    [MenuItem("Tools/Setup Unity For Git")]
    public static void SetupWindow()
    {
        SetupUnityForGit window = GetWindow<SetupUnityForGit>(true, "Setup Unity For Git", true);
        window.minSize = new Vector2(340, 360);
        window.maxSize = new Vector2(340, 1024);

        window.BuildPackageList();
    }

    public void BuildPackageList()
    {
        ListRequest packageList = Client.List();
        while (!packageList.IsCompleted)
        {
        }

        m_PackagesToRemove = packageList.Result
            .Where(package => !package.name.Contains("package-manager-ui") && !package.name.Contains("com.unity.modules"))
            .Select((package) => { return new ToggleOption(true, package.name); })
            .ToArray();

        if (m_PackagesToRemove.Length > 0)
        {
            m_IsRemovingPackages = true;
        }
        else
        {
            m_IsRemovingPackages = false;
        }
    }

    private void DisplaySetting(ref bool setting, string settingsName, string description = "")
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);
        setting = EditorGUILayout.Toggle(settingsName, setting);
        if (setting && !string.IsNullOrEmpty(description))
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField(description, EditorStyles.wordWrappedLabel);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
    }

    private void SetupGitFiles()
    {
        // Set Git .gitignore & .gitattributes
        string projectFolder = Path.GetFullPath(Path.Combine(Application.dataPath, "../"));
        string sourceGitignore = Path.Combine(projectFolder, "Assets/UnityGitSetup/gitignore.txt");
        string sourceGitattributes = Path.Combine(projectFolder, "Assets/UnityGitSetup/gitattributes.txt");

        File.Copy(sourceGitignore, Path.Combine(projectFolder, ".gitignore"), true);
        File.Copy(sourceGitattributes, Path.Combine(projectFolder, ".gitattributes"), true);

        Debug.LogWarning("Replaced: .gitignore");
        Debug.LogWarning("Replaced: .gitattributes, Remember to initialize Git-LFS");

        // Set; Version Control - Visible Meta Files & Asset Serialization - force text

        EditorSettings.externalVersionControl = "Visible Meta Files";
        EditorSettings.serializationMode = SerializationMode.ForceText;

        Debug.LogWarning("Edit -> Project Settings -> Version Control: Changed to Visible Meta Files");
        Debug.LogWarning("Edit -> Project Settings -> Asset Serialization: Change to Force Text");
    }

    Vector2 folderOptionsScrollPosition;

    private void DisplayRemovablePackages()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);

        m_IsRemovingPackages = EditorGUILayout.Toggle("Remove packages", m_IsRemovingPackages);

        if (m_IsRemovingPackages)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Removes selected packages, added by default by Unity", EditorStyles.wordWrappedLabel);

            hasRemoveAllPackages = EditorGUILayout.Toggle("All", hasRemoveAllPackages);

            if (!hasRemoveAllPackages)
            {
                folderOptionsScrollPosition = EditorGUILayout.BeginScrollView(folderOptionsScrollPosition);

                for (int i = 0; i < m_PackagesToRemove.Length; i++)
                {
                    m_PackagesToRemove[i].IsOn = EditorGUILayout.Toggle(m_PackagesToRemove[i].Name, m_PackagesToRemove[i].IsOn);
                }
                EditorGUILayout.EndScrollView();
            }

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
    }

    private void RemovePackages()
    {
        RemoveRequest removePackage;

        foreach (var package in m_PackagesToRemove)
        {
            if (hasRemoveAllPackages || package.IsOn)
            {
                removePackage = Client.Remove(package.Name);
                while (!removePackage.IsCompleted)
                {
                }
            }
        }
    }

    private void CleanUp()
    {
        SetupUnityForGit window = GetWindow<SetupUnityForGit>(true, "Setup Unity For Git", true);
        window.Close();
        Debug.LogWarning("Removing Unity Git Setup Tool");
        AssetDatabase.MoveAssetToTrash("Assets/UnityGitSetup");
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);
        DisplaySetting(ref m_IsSetupGitFiles, "Setup git files", "Remember to initialize Git-LFS in repo after setup. \n\nNote: If the file that should be LFS is already on repro it will not be converted there is tools for that.");

        if (m_PackagesToRemove.Length > 0)
        {
            DisplayRemovablePackages();
        }

        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Start Setup"))
        {
            if (m_IsSetupGitFiles)
            {
                EditorUtility.DisplayProgressBar(kProgressTitle, "Setup Git", 0.2f);
                if (EditorUtility.DisplayDialog("Warning Destructive operation!", "This will override the current .gitignore & .gitattributes and replace them completely", "Destroy Them!!"))
                {
                    SetupGitFiles();
                    // Ensure any changes get picked up by the editor.
                    AssetDatabase.SaveAssets();
                }
            }

            if (m_IsRemovingPackages)
            {
                EditorUtility.DisplayProgressBar(kProgressTitle, "Removing packages", 0.4f);
                if (EditorUtility.DisplayDialog("Warning Destructive operation!", "This will remove the selected packages completely", "Destroy It!!"))
                {
                    RemovePackages();
                }
            }

            AssetDatabase.ForceReserializeAssets();

            EditorUtility.ClearProgressBar();
        }
        if (GUILayout.Button("Remove Setup Tool"))
        {
            if (EditorUtility.DisplayDialog("Warning Destructive operation!", "This will remove all trace of the SetupUnityForGit tool", "Destroy It!!", "Keep it around"))
            {
                CleanUp();
                // Ensure any changes get picked up by the editor.
                AssetDatabase.Refresh();
            }
        }
    }

}

