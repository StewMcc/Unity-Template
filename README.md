# Unity Project Template
[Github template repository](https://help.github.com/en/articles/creating-a-template-repository) for [Unity](https://unity.com/)

There is a nice little tool (**Unity Template -> Setup Unity for Git**) that can be used to quickly clean up the repository if using different versions of Unity.

It double checks editor is set to force text, and visible meta files, it can also remove any of the packages added by Unity's upgrade process that you don't want (**some of these are added to help you**), then forces a re-serialization of all the meta files. The tool can then be used to remove itself.

# Tested on
 - 2020.3 
    - It will invoke the updater for packages, let it update, you can still use the tool to remove them after the update is finished. However, it will create a bunch of new files, and add packages that cover features removed from the base install. (VSCode/VisualStudio/Rider editor support etc.)
    - You must run the `Setup editor for Git` part at least once as git version control settings have moved, and they don't get copied over properly.
 - 2019.4 