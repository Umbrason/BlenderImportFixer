using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;

/*
modified version of a script by 'fuser'
https://discussions.unity.com/t/blender-2-8-animations-not-importing-in-beta-2019-3-0b4/759171/46 
*/
public class BlenderImportFixer
{
    private const string importerScriptName = "Unity-BlenderToFBX.py";
    private static readonly string relativePathFromAppToImporterScript = Path.Combine("Data", "Tools", importerScriptName);
    private static readonly string bakeAllAnimsRegex = @"bake_anim_use_all_actions=(True|False)";
    private static readonly string bakeSpaceTransformRegex = @"bake_space_transform=(True|False)";

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

        var foundBakeAllAnimsLine = false;
        var foundFalseBakeAllAnimsLine = false;
        var foundBakeSpaceTransformRegex = false;
        var foundFalseSpaceTransformRegex = false;

        var tempFilePath = Path.GetTempFileName();
        var lines = File.ReadAllLines(pathToImporterScript);
        for (int i = 0; i < lines.Length; i++)
        {
            var match = Regex.Match(lines[i], bakeAllAnimsRegex);
            if (!match.Success) continue;
            foundBakeAllAnimsLine = true;
            var containsFalse = lines[i].Contains("False");
            foundFalseBakeAllAnimsLine |= containsFalse;
            lines[i] = containsFalse ? lines[i].Replace("False", "True") : lines[i];
        }

        for (int i = 0; i < lines.Length; i++)
        {
            var match = Regex.Match(lines[i], bakeSpaceTransformRegex);
            if (!match.Success) continue;
            foundBakeSpaceTransformRegex = true;
            var containsFalse = lines[i].Contains("False");
            foundFalseSpaceTransformRegex |= containsFalse;
            lines[i] = containsFalse ? lines[i].Replace("False", "True") : lines[i];
        }
        if (!foundBakeSpaceTransformRegex)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if(!lines[i].Contains("bpy.ops.export_scene.fbx(filepath=outfile,")) continue;
                lines[i] = "bpy.ops.export_scene.fbx(filepath=outfile,\nbake_space_transform=True,";
                break;
            }
        }
        /*
        axis_forward='-Z',
        axis_up='Y',
        bake_space_transform=True,
        */
        if (!foundBakeAllAnimsLine)
        {
            EditorUtility.DisplayDialog("Could not find any lines to fix", @$"
Could not find any relevant line in the importer script, so the script cannot be fixed.
Either the file is corrupt or the issue has been fixed by the Unity team.
In the latter case you can safely remove this package."
            , "OK");
            return;
        }
        if (!foundFalseBakeAllAnimsLine) return; //nothing to write back
        File.WriteAllLines(pathToImporterScript, lines);
    }
}