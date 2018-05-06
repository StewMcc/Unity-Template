using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SetupUnityForGit : EditorWindow {

	private const string progressTitle = "Setup Unity: ";

	private bool isSetupGitFiles = true;
	private bool isReplacingManifest = true;
	private bool isSetupDefaultFolders = true;

	[MenuItem("Tools/Setup Unity For Git")]
	public static void SetupWindow() {
		GetWindow<SetupUnityForGit>(true, "Setup Unity For Git", true);
	}

	private static void SetupGitFiles() {
		// Set Git .gitignore & .gitattributes
		string projectFolder = Path.GetFullPath(Path.Combine(Application.dataPath, "../"));
		string sourceGitignore = Path.Combine(projectFolder, "Assets/UnityGitSetup/gitignore.txt");
		string sourceGitattributes = Path.Combine(projectFolder, "Assets/UnityGitSetup/gitattributes.txt");

		File.Copy(sourceGitignore, Path.Combine(projectFolder, ".gitignore"), true);
		File.Copy(sourceGitattributes, Path.Combine(projectFolder, ".gitattributes"), true);

		Debug.LogWarning("Replaced: .gitignore");
		Debug.LogWarning("Replaced: .gitattributes");

		// Set Version Control - Visible Meta Files & Asset Serialization - force text
	}

	private static void ReplaceManifest() {

		string packageFolder;
#if UNITY_2018_1_OR_NEWER
		packageFolder = "Packages";
#else
		packageFolder = "UnityPackageManager";
#endif

		string projectFolder = Path.GetFullPath(Path.Combine(Application.dataPath, "../"));

		packageFolder = Path.Combine(projectFolder, packageFolder);

		string sourceManifest = Path.Combine(projectFolder, "Assets/UnityGitSetup/manifest.txt");
		string destinationManifest = Path.Combine(packageFolder, "manifest.json");

		File.Copy(sourceManifest, destinationManifest, true);

		Debug.LogWarning("Replaced: " + destinationManifest);

	}

	private static void SetupDefaultFolders() {
		string assetsFolder = Path.GetFullPath(Application.dataPath);

		CreateFolderWithGitKeep(Path.Combine(assetsFolder, "Meshes"));
		CreateFolderWithGitKeep(Path.Combine(assetsFolder, "Fonts"));
		CreateFolderWithGitKeep(Path.Combine(assetsFolder, "Presets"));
		CreateFolderWithGitKeep(Path.Combine(assetsFolder, "Prefabs"));
		CreateFolderWithGitKeep(Path.Combine(assetsFolder, "Settings"));
		CreateFolderWithGitKeep(Path.Combine(assetsFolder, "Textures"));
		CreateFolderWithGitKeep(Path.Combine(assetsFolder, "Materials"));
		CreateFolderWithGitKeep(Path.Combine(assetsFolder, "Scenes"));
		CreateFolderWithGitKeep(Path.Combine(assetsFolder, "Packages"));
		CreateFolderWithGitKeep(Path.Combine(assetsFolder, "Scripts"));
		CreateFolderWithGitKeep(Path.Combine(assetsFolder, "Shaders"));
		CreateFolderWithGitKeep(Path.Combine(assetsFolder, "Sounds"));
		
		Debug.LogWarning("Created default Folders with git .keep files");
	}

	private static void CreateFolderWithGitKeep(string folder) {
		Directory.CreateDirectory(folder);
		FileStream file = File.Create(Path.Combine(folder, ".keep"));
		file.Close();
	}

	private static void CleanUp() {
		SetupUnityForGit window = GetWindow<SetupUnityForGit>(true, "Setup Unity For Git", true);
		window.Close();
		Debug.LogWarning("Removing Unity Git Setup Tool");
		AssetDatabase.MoveAssetToTrash("Assets/UnityGitSetup");
	}

	private void OnGUI() {
		EditorGUILayout.LabelField("");

		isSetupGitFiles = EditorGUILayout.Toggle("Setup git files", isSetupGitFiles);
		isReplacingManifest = EditorGUILayout.Toggle("Replace packages manifest", isReplacingManifest);
		isSetupDefaultFolders = EditorGUILayout.Toggle("Setup default folders", isSetupDefaultFolders);

		if (GUILayout.Button("Start Setup")) {
			if (isSetupGitFiles) {
				EditorUtility.DisplayProgressBar(progressTitle, "Setup Git", 0.2f);
				if (EditorUtility.DisplayDialog("Warning Destructive operation!", "This will override the current .gitignore & .gitattributes and replace them completely", "Destroy Them!!"))
					SetupGitFiles();
			}
			if (isReplacingManifest) {
				EditorUtility.DisplayProgressBar(progressTitle, "Replacing packages manifest", 0.4f);
				if (EditorUtility.DisplayDialog("Warning Destructive operation!", "This will override the current package manager manifest and replace it completely", "Destroy It!!"))
					ReplaceManifest();
			}
			if (isSetupDefaultFolders) {
				EditorUtility.DisplayProgressBar(progressTitle, "Setup default folders", 0.6f);
				SetupDefaultFolders();
			}
			// Ensure any changes get picked up by the editor.
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			EditorUtility.ClearProgressBar();
		}
		if (GUILayout.Button("Remove Setup Tool")) {
			if (EditorUtility.DisplayDialog("Warning Destructive operation!", "This will remove all trace of the SetupUnityForGit tool", "Destroy It!!", "Keep it around")) {
				CleanUp();
				// Ensure any changes get picked up by the editor.
				AssetDatabase.Refresh();
			}
		}
	}

}

