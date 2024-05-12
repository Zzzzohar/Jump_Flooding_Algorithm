using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(JumpFloodingAlgorithm))]
public class JumpFloodingAlgorithmEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        JumpFloodingAlgorithm JFA = (JumpFloodingAlgorithm)target;
        
        //根据选择文件夹修改myScript.savePath
        if(GUILayout.Button("Select Save Path"))
        {
            JFA.savePath = EditorUtility.OpenFolderPanel("Select Save Path", JFA.savePath, "");
            //svaepath 只显示asset之后的
            JFA.savePath = JFA.savePath.Substring(JFA.savePath.IndexOf("Assets"));
        }
        if(GUILayout.Button("Save Distance Field To PNG"))
        {
            JFA.SaveDistanceField();
        }
    }
}
