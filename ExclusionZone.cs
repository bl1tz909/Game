using UnityEngine;

[ExecuteAlways]
public class CampfireExclusionZoneDrawer : MonoBehaviour
{
    public float exclusionRadius = 10f;
    public Color gizmoColor = new Color(1f, 0.5f, 0f, 0.25f); // orange-ish

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, exclusionRadius);
    }
}