using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using it.polito.appeal.traci;
using de.tudresden.sumo.cmd;
using de.tudresden.ws.container;
using System;

public class SumoBehavior : MonoBehaviour
{
    public GameObject carPrefab;
    public GameObject lanePrefab;
    public float LaneHeight;
    public float CarHeight;


    private string sumo_bin = "C:\\Program Files (x86)\\Eclipse\\Sumo\\bin\\sumo-gui.exe";
    private string config_file = "C:\\Users\\Joey\\Desktop\\Aarti\\traci-demo-master\\demo.sumocfg";
    private SumoTraciConnection conn;


    private List<GameObject> vehicles;
    private List<GameObject> lanes;

    // To keep a clean hierarchy, generated game objects will be stowed as childs of these objects
    private GameObject vehicleParent;
    private GameObject laneParent;

    // keep track of vehicle creation
    private bool vehiclesCreationSuccess = false;


    private int createdVehiclesCount = 0;

    // Use this for initialization
    void Start()
    {
        Application.targetFrameRate = 30;
        try
        {
            Initialize();
        }
        catch (Exception r) {
            Debug.Log(" r = " + r.Message + " " + r.StackTrace);
        }
        
        ReadAndPrintLanes();
        vehiclesCreationSuccess = ReadVehicles();
        
    }

    // FixedUpdate is called once per frame at requested framerate
    void FixedUpdate()
    {
          if (vehiclesCreationSuccess)
              UpdateVehicles();
    }

    /*
     * Initialize 
     * 
     * Pseudo :
     *     
        conn = new SumoTraciConnection(sumo_dir,config_file) 
        conn.runServer() 
            launchSumoInRemotePort()  
            tcp_connectToSUMO() 
        conn.do_timestep() 
        readLanes() 
            printLanes()

    */
    void Initialize()
    {
        conn = new SumoTraciConnection(sumo_bin, config_file);
        //set some options
        conn.addOption("step-length", "0.3333"); //timestep 33.33ms - 30 frames a second
        conn.addOption("start", null);
        conn.runServer();

        for (int i = 0; i < 100; i++)
        {
            conn.do_timestep();
        }

        lanes = new List<GameObject>();
        vehicles = new List<GameObject>();

        laneParent = new GameObject();
        laneParent.name = "lanesCreatedOnRuntime";
        vehicleParent = new GameObject();
        vehicleParent.name = "vehiclesCreatedOnRuntime";

        vehiclesCreationSuccess = false;
    }


    void RemoveVehicles() {
        foreach (GameObject g in vehicles) {
            Destroy(g);
        }
        vehicles.Clear();
    }

    bool ReadVehicles()
    {

        try
        {
            RemoveVehicles();

            SumoStringList carIds = (SumoStringList)conn.do_job_get(de.tudresden.sumo.cmd.Vehicle.getIDList());

            foreach (string id in carIds)
            {
                conn.do_timestep();
                // create new car
                GameObject newVehicle = Instantiate(carPrefab);
                // assign id
                newVehicle.GetComponent<VehicleContainer>().setId(id);
                // get position
                SumoPosition2D position = (SumoPosition2D)conn.do_job_get(de.tudresden.sumo.cmd.Vehicle.getPosition(id));
                // assign position
                newVehicle.transform.position = new Vector3((float)position.x,CarHeight, (float)position.y);
                // get speed
                double speed = double.Parse(conn.do_job_get(de.tudresden.sumo.cmd.Vehicle.getSpeed(id)).ToString());
                // assign speed
                newVehicle.GetComponent<VehicleContainer>().setSpeed(speed);

                if (id.Contains("ev")) {
                    // assign red color to EV
                    newVehicle.GetComponent<Renderer>().material.color = Color.red;
                }

                newVehicle.transform.parent = vehicleParent.transform;
                // store vehicle
                vehicles.Add(newVehicle);
            }
            
        }
        catch (Exception e)
        {
            Debug.Log("Read Vehicles Exception : " + e.Message + " -- " + e.StackTrace);
            return false;
        }

        return true;
    }

    public void CreateNewCar_Runtime() {

        vehiclesCreationSuccess = false;

        try {
            // creation query
            // Args - ID, TYPE, ROUTE, TIME, DISTANCE, SPEED, idk wtf byte is.
            conn.do_job_set(de.tudresden.sumo.cmd.Vehicle.add("my_"+ createdVehiclesCount++.ToString(), "car", "", 0, 15, 5, new byte()));
        }
        catch (Exception e) {
            Debug.Log("Runtime Vehicle Create Exception - " + e.Message);
            return;
        }

        Debug.Log("Successfully Created Vehicle" );

        // refresh vehicles
        vehiclesCreationSuccess = ReadVehicles();

        return;
    }

    void UpdateVehicles()
    {
        
        try {
            conn.do_timestep();
            foreach (GameObject v in vehicles)
            {
                // get new position of the car
                SumoPosition2D newPosition = (SumoPosition2D)conn.do_job_get(de.tudresden.sumo.cmd.Vehicle.getPosition(v.GetComponent<VehicleContainer>().getId()));
                // convert into vector3
                Vector3 newPositonV3 = new Vector3((float)newPosition.x, CarHeight, (float)newPosition.y);
                // update the position of car
                v.transform.position = newPositonV3;
            }
        } catch (Exception e) {
            Debug.Log(" Exception updating vehicles - " + e.Message + " " + e.StackTrace);
            if (conn.isClosed()) 
            {
                conn.close();
                vehiclesCreationSuccess = false;
                Debug.Log(" Application quit as connection to SUMO was terminated! ");
                Application.Quit();
                return;
            }
        }
        
    }


    /*
 * ReadLanes
 * 
 * Pseudo:
 * 
    conn.do_job_get(LaneIDList)
    linkList = new List<Link>()  
    foreach(id in IDlist)   
        vertices = conn.do_job_get(laneID.Vertices)
        width = conn.do_job_get(laneID.Width)   
        allowed = conn.do_job_get(laneID.AllowedVehicles)    
            type = switch(allowed)   
        link = new Link(vertices,width,id,type)   
        linkList.add(link)  
    printLanes() 

 * */

    void ReadAndPrintLanes()
    {

        try
        {
            lanes.Clear();
            SumoStringList laneIDs = (SumoStringList)conn.do_job_get(Lane.getIDList());
            foreach (string id in laneIDs)
            {
                // Get lane details
                var laneShape = conn.do_job_get(Lane.getShape(id));
                List<Vector3> laneShapeList = HelperMethods.GetPoisitonsListFromSumoStringList(laneShape.ToString(), 0);

                double length = double.Parse(conn.do_job_get(Lane.getLength(id)).ToString());
                double width = double.Parse(conn.do_job_get(Lane.getWidth(id)).ToString());

                var allowedVehicles = (SumoStringList)conn.do_job_get(Lane.getAllowed(id));
                List<string> allowedVehicleList = new List<string>();
                if (allowedVehicles.size() > 0) {
                    // do allowed vehicle parse here 
                    // future use -- method not implemented
                    allowedVehicleList = HelperMethods.GetAllowedVehiclesListFromSumoStringList(allowedVehicles);
                }

                GameObject lane = DrawLane(laneShapeList[0], laneShapeList[1], (float)width);
                lane.AddComponent<LaneContainer>();
                lane.GetComponent<LaneContainer>().SetLaneProperties(id, laneShapeList[0], laneShapeList[1], length, width, allowedVehicleList);
                lane.transform.parent = laneParent.transform;
                lanes.Add(lane);
                
            }

        }
        catch (Exception e) {

            Debug.Log("Read Lanes Exception : " + e.Message + " -- " + e.StackTrace);
            return;
        }


        
    }

    // Creates a unity cube and streches it in desired length and width
    GameObject DrawLane(Vector3 start, Vector3 end, float width = 0.1f )
    {
        GameObject myLane = Instantiate(lanePrefab);
        // Assuming this is run on a unit cube.
        Vector3 between = end - start;
        float distance = between.magnitude;
        Vector3 ls = myLane.transform.localScale;
        ls.z = distance;
        ls.x = width;
        myLane.transform.localScale = ls;
        Vector3 mid = Vector3.Lerp(start, end, 0.5f);
        mid.y = LaneHeight;
        myLane.transform.position = mid;
        myLane.transform.LookAt(end);
        return myLane;

    }

}
