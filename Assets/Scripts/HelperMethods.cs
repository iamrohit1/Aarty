using de.tudresden.ws.container;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelperMethods
{

    public static List<Vector3> GetPoisitonsListFromSumoStringList(string list, float height)
    {
       
        List<Vector3> finalList = new List<Vector3>();
        string[] v1v2 = list.Split(' ');
        finalList.Add(Vector2StringToVector3(v1v2[0],height));
        finalList.Add(Vector2StringToVector3(v1v2[1],height));
        return finalList;

    }


    public static List<string> GetAllowedVehiclesListFromSumoStringList(SumoStringList list)
    {

        List<string> finalList = new List<string>();

        
        return finalList;

    }

    private static Vector3 Vector2StringToVector3(string s, float y) {
        Vector3 r = new Vector3();
        r.x = float.Parse(s.Split(',')[0]);
        r.y = y;
        r.z = float.Parse(s.Split(',')[1]);
        return r;
    }



}
