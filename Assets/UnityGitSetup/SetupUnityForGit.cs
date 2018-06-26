using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SetupUnityForGit : EditorWindow {

	private const string progressTitle = "Setup Unity: ";

	private bool isSetupGitFiles = true;
	private bool isReplacingManifest = true;
	private bool isSetupDefaultFolders = true;
	private bool hasAllDefaultFolders = true;

	[MenuItem("Tools/Setup Unity For Git")]
	public static void SetupWindow() {
		SetupUnityForGit window = GetWindow<SetupUnityForGit>(true, "Setup Unity For Git", true);
		window.minSize = new Vector2(340, 360);
		window.maxSize = new Vector2(340, 1024);
	}

	private void DisplaySetting(ref bool setting, string settingsName, string description = "") {

		EditorGUILayout.BeginVertical(GUI.skin.box);
		setting = EditorGUILayout.Toggle(settingsName, setting);
		if (setting && !string.IsNullOrEmpty(description)) {
			EditorGUI.indentLevel++;
			EditorGUILayout.LabelField(description, EditorStyles.wordWrappedLabel);
			EditorGUI.indentLevel--;
		}
		EditorGUILayout.EndVertical();
	}

	private struct FolderOption {
		public bool createFolder;
		public readonly string folderName;

		public FolderOption(bool create,string name){
			createFolder = create;
			folderName = name;
		}
	}

	private readonly FolderOption[] folderOptions = {
		new FolderOption (true,"3rdParty"),
		new FolderOption (false,"Animation"),
		new FolderOption (false,"Audio"),
		new FolderOption (false,"Fonts"),
		new FolderOption (false,"Meshes"),
		new FolderOption (false,"Materials"),
		new FolderOption (true,"Prefabs"),
		new FolderOption (false,"Presets"),
		new FolderOption (true,"Scenes"),
		new FolderOption (true,"Scripts"),
		new FolderOption (true,"Settings"),
		new FolderOption (false,"Shaders"),
		new FolderOption (false,"Textures")
	};

	Vector2 folderOptionsScrollPosition;

	private void DisplayDefaultFolderOptions() {
		EditorGUILayout.BeginVertical(GUI.skin.box);

		isSetupDefaultFolders = EditorGUILayout.Toggle("Setup default folders", isSetupDefaultFolders);

		if (isSetupDefaultFolders) {
			EditorGUI.indentLevel++;
			EditorGUILayout.LabelField("Creates default folders with .keep within them to force them to be added to Git", EditorStyles.wordWrappedLabel);

			hasAllDefaultFolders = EditorGUILayout.Toggle("All", hasAllDefaultFolders);

			if (!hasAllDefaultFolders) {
				folderOptionsScrollPosition = EditorGUILayout.BeginScrollView(folderOptionsScrollPosition);

				for (int i = 0; i < folderOptions.Length; i++) {
					folderOptions[i].createFolder = EditorGUILayout.Toggle(folderOptions[i].folderName, folderOptions[i].createFolder);
				}
				EditorGUILayout.EndScrollView();
			}

			EditorGUI.indentLevel--;
		}
		EditorGUILayout.EndVertical();
	}

	private void SetupGitFiles() {
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

	private void ReplaceManifest() {

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

	private void SetupDefaultFolders() {
		string assetsFolder = Path.GetFullPath(Application.dataPath);

		foreach (var folderOption in folderOptions) {
			if (folderOption.createFolder || hasAllDefaultFolders) {
				CreateFolderWithGitKeep(Path.Combine(assetsFolder, folderOption.folderName));
			}
		}

		Debug.LogWarning("Created default Folders with git .keep files");
	}

	private void CreateFolderWithGitKeep(string folder) {
		Directory.CreateDirectory(folder);
		FileStream file = File.Create(Path.Combine(folder, ".keep"));
		file.Close();
	}

	private void CleanUp() {
		SetupUnityForGit window = GetWindow<SetupUnityForGit>(true, "Setup Unity For Git", true);
		window.Close();
		Debug.LogWarning("Removing Unity Git Setup Tool");
		AssetDatabase.MoveAssetToTrash("Assets/UnityGitSetup");
	}

	private void OnGUI() {
		EditorGUILayout.BeginVertical(GUI.skin.box);
		DisplaySetting(ref isSetupGitFiles, "Setup git files", "Remember to initialize Git-LFS in repo after setup. \n\nNote: If the file that should be LFS is already on repro it will not be converted there is tools for that.");
#if UNITY_2017_2_OR_NEWER
		DisplaySetting(ref isReplacingManifest, "Replace packages", "Replaces the Package Manifest to exclude default packages");
#endif
		DisplayDefaultFolderOptions();

		EditorGUILayout.EndVertical();

		if (GUILayout.Button("Start Setup")) {
			if (isSetupGitFiles) {
				EditorUtility.DisplayProgressBar(progressTitle, "Setup Git", 0.2f);
				if (EditorUtility.DisplayDialog("Warning Destructive operation!", "This will override the current .gitignore & .gitattributes and replace them completely", "Destroy Them!!"))
					SetupGitFiles();
			}
#if UNITY_2017_2_OR_NEWER
			if (isReplacingManifest) {
				EditorUtility.DisplayProgressBar(progressTitle, "Replacing packages manifest", 0.4f);
				if (EditorUtility.DisplayDialog("Warning Destructive operation!", "This will override the current package manager manifest and replace it completely", "Destroy It!!"))
					ReplaceManifest();
			}
#endif
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

