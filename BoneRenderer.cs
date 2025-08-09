using UnityEngine;
using UnityEngine.Animations.Rigging;
using System.Collections.Generic;

[ExecuteInEditMode]
public class AutoAssignBoneRenderer : MonoBehaviour
{
    void Start()
    {
        var boneRenderer = GetComponent<BoneRenderer>();
        if (boneRenderer != null)
        {
            List<Transform> bones = new List<Transform>();
            GetChildBones(transform, bones);
            boneRenderer.transforms = bones.ToArray();
            Debug.Log($"Assigned {bones.Count} bones to BoneRenderer.");
        }
    }

    void GetChildBones(Transform current, List<Transform> bones)
    {
        foreach (Transform child in current)
        {
            bones.Add(child);
            GetChildBones(child, bones);
        }
    }
}
