using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneContainer : MonoBehaviour
{
    private Vector3 V1;
    private Vector3 V2;
    private List<string> allowedVehicles;
    private double width;
    private double length;
    private string id;



    public void  SetLaneProperties(string id, Vector3 V1, Vector3 V2, double length, double width, List<string> allowedVehicles)
    {
        this.id = id;
        this.V1 = V1;
        this.V2 = V2;
        this.length = length;
        this.width = width;
        this.allowedVehicles = allowedVehicles;
    }

}