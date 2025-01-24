# BlenderImportFixer
Allows for **all** animations contained in a `.blend` file to be correctly imported by Unity,\
instead of just the most recent one being imported under the generic name 'Scene'.

Replaces all occurences of `bake_anim_use_all_actions=False` in\
`<CurrentUnityEditorVersion>/Editor/Data/Tools/Unity-BlenderToFBX.py`\
with `bake_anim_use_all_actions=True`.

based on a more [manual script version](https://discussions.unity.com/t/blender-2-8-animations-not-importing-in-beta-2019-3-0b4/759171/46) proposed by [fuser](https://discussions.unity.com/u/fuser/summary) on the Unity forums.
