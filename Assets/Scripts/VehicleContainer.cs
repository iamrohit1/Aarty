using System.Collections;
using UnityEngine;

public class VehicleContainer : MonoBehaviour
{
    private double speed;
    private string id;

    public void setSpeed(double speed)
    {
        this.speed = speed;
    }

    public double getSpeed()
    {
        return this.speed;
    }
    public string getId()
    {
        return this.id;
    }
    public void setId(string id)
    {
        this.id = id;
    }

}