using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

/*
modified version of a script by user 'fuser'
https://discussions.unity.com/t/blender-2-8-animations-not-importing-in-beta-2019-3-0b4/759171/46 
*/
public class BlenderImportFixer
{
    private const string importerScriptName = "Unity-BlenderToFBX.py";
    private static readonly string relativePathFromAppToImporterScript = Path.Combine("Data", "Tools", importerScriptName);

    private static readonly Regex bakeAllAnimsFalseFinder = new Regex(@"(.*\b)bake_anim_use_all_actions=False(\b.*)");
    private const string bakeAllAnimsTrueReplacement = "$1bake_anim_use_all_actions=True$2";

    [InitializeOnLoadMethod]
    static void FixBlenderAnimationImporter()
    {
        var pathToApp = EditorApplication.applicationPath;
        var pathToAppDirectory = Path.GetDirectoryName(pathToApp);
        var pathToImporterScript = Path.Combine(pathToAppDirectory, relativePathFromAppToImporterScript);

        if (!File.Exists(pathToImporterScript))
        {
            EditorUtility.DisplayDialog("Could not find importer script", @$"
Could not find the importer script, so the script cannot be fixed. We are looking for the file ""{importerScriptName}"".
If you find this file and it's still relevant please fix the reference in class {nameof(BlenderImportFixer)}."
            , "OK");
            return;
        }

        var foundFalseBakeAllAnims = false;

        var tempFilePath = Path.GetTempFileName();
        using (StreamWriter output = File.CreateText(tempFilePath))
        {
            using (StreamReader original = File.OpenText(pathToImporterScript))
            {
                string singleLine;
                while ((singleLine = original.ReadLine()) != null)
                {
                    var matches = bakeAllAnimsFalseFinder.Matches(singleLine);
                    if (matches.Count > 0)
                    {
                        Assert.AreEqual(matches.Count, 1, "The regex expression should be greedy, so there should only be a maximum of one match per line");
                        output.WriteLine(matches[0].Result(bakeAllAnimsTrueReplacement));
                        foundFalseBakeAllAnims = true;
                    }
                    else
                    {
                        output.WriteLine(singleLine);
                    }
                }
            }
        }

        if (!foundFalseBakeAllAnims)
        {
            EditorUtility.DisplayDialog("Could not find the line to fix", String.Format("We successfully found the script that needed to be fixed, but there was no line matching the \"{0}\" regex. This run of this script has not fixed the importer - it's possible that the importer has already been fixed.", bakeAllAnimsFalseFinder.ToString()), "OK");
            File.Delete(tempFilePath);
            return;
        }

        File.Delete(pathToImporterScript);
        File.Move(tempFilePath, pathToImporterScript);
    }
}