using System;
using UnityEditor;
using UnityEngine;

public class CreateGOHelper : MonoBehaviour
{
    [MenuItem("GameObject/Systems/VFX System")]
    private static void CreateVFXSystem()
    {
        CreateSystemGameObject(typeof(VFXSystem));
    }

    private static void CreateSystemGameObject(Type system)
    {
        GameObject go = new GameObject(system.Name);

        go.AddComponent(system);
        
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        
        Selection.activeObject = go;
    }
}