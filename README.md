# Unity Project Template
Unity project, with git-lfs, `.gitignore` and `.gitattributes` setup.

- 2018.1 or newer delete UnityPackageManager folder
- 2017.4 or older, delete Packages folder

# Instead of forking
Most of the time, easiest just to copy the `.gitignore`, `.gitattributes`, and `UnityPackageManager`/`Packages` folders into your new project. Also in Edit->ProjectSettings->EditorSettings set `Asset Serialization` to `Force Text`, and set `Version Control` `Mode` to `Visible Meta Files`.


This is due to the ProjectSettings `.asset`'s usually including lots of old useless information that won't be removed until you change a setting in the relevant file.
