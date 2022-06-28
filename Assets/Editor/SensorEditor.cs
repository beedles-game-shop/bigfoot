using System;
using UnityEngine;

// https://github.com/SebLague/Field-of-View
using UnityEditor;
[CustomEditor(typeof(GameObject))]
public class SensorEditor : Editor {

    void OnSceneGUI()
    {
        var sensors = FindObjectsOfType<Sensor>();
        foreach (var sensor in sensors)
        {
            DrawHearing(sensor);
            DrawVision(sensor);
        }
    }

    private void DrawVision(Sensor sensor)
    {
         Handles.color = Color.white;
         Vector3 viewAngleA = sensor.DirFromAngle (-sensor.viewAngle / 2, false);
         Vector3 viewAngleB = sensor.DirFromAngle (sensor.viewAngle / 2, false);
         Handles.DrawWireArc (sensor.transform.position, Vector3.up, viewAngleA, sensor.viewAngle, sensor.viewRadius);
 
         Handles.DrawLine (sensor.transform.position, sensor.transform.position + viewAngleA * sensor.viewRadius);
         Handles.DrawLine (sensor.transform.position, sensor.transform.position + viewAngleB * sensor.viewRadius);
 
         Handles.color = Color.red;
         foreach (var visibleTarget in sensor.visibleTargets) {
             Handles.DrawLine (sensor.transform.position, visibleTarget);
         }           
    }

    private void DrawHearing(Sensor sensor)
    {
        Handles.color = Color.blue;
        Handles.DrawWireArc(sensor.transform.position, Vector3.up, Vector3.forward, 360, sensor.audibleRadius);

        Handles.color = Color.yellow;
        foreach (var target in sensor.audibleTargets)
        {
            Handles.DrawLine(sensor.transform.position, target.TargetPosition);
        }       
    }

}
