using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HitMarkerManager
{
    public string ownerTag;
    public LayerMask hitLayer;
    public QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

    public void ConfigureMarkers(HitMarker[] hitMarkers)
    {
        foreach (var marker in hitMarkers)
        {
            marker.hitLayer = hitLayer;
            marker.triggerInteraction = triggerInteraction;
            marker.ownerTag = ownerTag;
        }
    }

}