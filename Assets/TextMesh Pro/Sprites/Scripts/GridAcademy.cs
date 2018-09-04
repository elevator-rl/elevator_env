using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading;
using MLAgents;
public class GridAcademy : Academy
{
    [HideInInspector]
    public List<GameObject> actorObjs;
    [HideInInspector]
    public int[] players;


    //[HideInInspector]
    public GameObject[] trueAgent;

    public int gridSize;

    public GameObject camObject;
    Camera cam;
    Camera agentCam;

    public GameObject agentPref;
    public GameObject goalPref;
    public GameObject pitPref;


    public GameObject goalObject;

    public UnityEngine.UI.Text debugText;

    GameObject[] objects;

    GameObject plane;
    GameObject sN;
    GameObject sS;
    GameObject sE;
    GameObject sW;

    CsvFileWriter csvWriter;

    int nGameNo =1;

    bool bSuccess = false;

    public override void InitializeAcademy()
    {
        gridSize = (int)resetParameters["gridSize"];
        cam = camObject.GetComponent<Camera>();

        objects = new GameObject[3] {agentPref, goalPref, pitPref};

        agentCam = GameObject.Find("agentCam").GetComponent<Camera>();

        actorObjs = new List<GameObject>();

        plane = GameObject.Find("Plane");
        sN = GameObject.Find("sN");
        sS = GameObject.Find("sS");
        sW = GameObject.Find("sW");
        sE = GameObject.Find("sE");


        int AgenCount = (int)resetParameters["chaserAgent"];

        trueAgent = new GameObject[AgenCount];



        var brain = transform.Find("GridWorldBrain").GetComponent<Brain>();
        int pitNo = Random.Range(0, trueAgent.Length);

       
        for (int i = 0; i < AgenCount; ++i)
        {
            trueAgent[i] = Instantiate(agentPref);
            trueAgent[i].GetComponent<GridAgent>().agentParameters.agentCameras[0] =
                GameObject.Find("agentCam").GetComponent<Camera>();

            trueAgent[i].GetComponent<GridAgent>().brain = brain;
            trueAgent[i].GetComponent<GridAgent>().OnEnable();


            //if (pitNo == i)
            //{
            //    trueAgent[i].GetComponent<GridAgent>().SetPitMode(6);
            //    trueAgent[i].GetComponent<GridAgent>().SetSight(1);
            //}

        }

        goalObject = GameObject.Find("goal");


   
    }

    private void Start()
    {


        var brain = transform.Find("GridWorldBrain").GetComponent<Brain>();

      
        if (brain.brainType == BrainType.Internal)
        {
            CoreBrainInternal coreBrainInternal = (CoreBrainInternal)brain.coreBrain;

            string name =   "summary/"+coreBrainInternal.graphModel.name+".csv";

            csvWriter = new CsvFileWriter(name);

            List<string> columns = new List<string>() { "No","Reward","Step"};// making Index Row
            csvWriter.WriteRow(columns);

        }


    }

    public void SetEnvironment()
    {
        cam.transform.position = new Vector3(-((int)resetParameters["gridSize"] - 1) / 2f, 
                                             (int)resetParameters["gridSize"] * 1.25f, 
                                             -((int)resetParameters["gridSize"] - 1) / 2f);
        cam.orthographicSize = ((int)resetParameters["gridSize"] + 5f) / 2f;

        List<int> playersList = new List<int>();

        for (int i = 0; i < (int)resetParameters["numObstacles"]; i++)
        {
            playersList.Add(2);
        }

        for (int i = 0; i < (int)resetParameters["numGoals"]; i++)
        {
            playersList.Add(1);
        }

      

        players = playersList.ToArray();

        plane.transform.localScale = new Vector3(gridSize / 10.0f, 1f, gridSize / 10.0f);
        plane.transform.position = new Vector3((gridSize - 1) / 2f, -0.5f, (gridSize - 1) / 2f);
        sN.transform.localScale = new Vector3(1, 1, gridSize + 2);
        sS.transform.localScale = new Vector3(1, 1, gridSize + 2);
        sN.transform.position = new Vector3((gridSize - 1) / 2f, 0.0f, gridSize);
        sS.transform.position = new Vector3((gridSize - 1) / 2f, 0.0f, -1);
        sE.transform.localScale = new Vector3(1, 1, gridSize + 2);
        sW.transform.localScale = new Vector3(1, 1, gridSize + 2);
        sE.transform.position = new Vector3(gridSize, 0.0f, (gridSize - 1) / 2f);
        sW.transform.position = new Vector3(-1, 0.0f, (gridSize - 1) / 2f);

        agentCam.orthographicSize = (gridSize) / 2f;
        agentCam.transform.position = new Vector3((gridSize - 1) / 2f, gridSize + 1f, (gridSize - 1) / 2f);

#if UNITY_EDITOR
        if (resetParameters["timeScale"] >0f)
            Time.timeScale = resetParameters["timeScale"];
#endif

    }

    public override void AcademyReset()
    {

        var brain = transform.Find("GridWorldBrain").GetComponent<Brain>();

        if (brain.brainType == BrainType.Internal)
        {
            Thread.Sleep(200);
        }




#if UNITY_EDITOR

        if ((int)resetParameters["maxEpisode"] >0 && GetEpisodeCount()>= (int)resetParameters["maxEpisode"])
        {

            UnityEditor.EditorApplication.isPlaying = false;

        }

#endif


        foreach (GameObject actor in actorObjs)
        {
            DestroyImmediate(actor);
        }
        SetEnvironment();

        actorObjs.Clear();

        HashSet<int> numbers = new HashSet<int>();
        while (numbers.Count < players.Length + 1)
        {
            numbers.Add(Random.Range(0, gridSize * gridSize));
        }
        int[] numbersA = Enumerable.ToArray(numbers);


        HashSet<Vector3> hashPos = new HashSet<Vector3>();

        for (int i = 0; i < players.Length; i++)
        {
            int x = (numbersA[i]) / gridSize;
            int y = (numbersA[i]) % gridSize;
            GameObject actorObj = Instantiate(objects[players[i]]);

            Vector3 targetPos = new Vector3(x, -0.25f, y);

         
            actorObj.transform.position = targetPos;
            actorObjs.Add(actorObj);

            if (actorObj.tag == "goal")
                goalObject = actorObj;
           
      
            hashPos.Add(targetPos);
        }



      

        for (int i = 0; i < trueAgent.Length; ++i)
        {
            if (trueAgent[i] == null)
                continue;

            while (true)
            {
                int r = Random.Range(0, gridSize * gridSize);
                int x_a = r/ gridSize;
                int y_a = r % gridSize;

                Vector3 targetPos = new Vector3(x_a, -0.25f, y_a);

                if (hashPos.Add(targetPos))
                {
                    trueAgent[i].transform.position = targetPos;
                    break;
                }
            }

            trueAgent[i].GetComponent<GridAgent>().Init();
      

        }


        while ((int)resetParameters["numGoals"] ==0)
        {
            int r = Random.Range(0, gridSize * gridSize);
            int x_a = r / gridSize;
            int y_a = r % gridSize;

            Vector3 targetPos = new Vector3(x_a, -0.25f, x_a);

            if (!hashPos.Contains(targetPos))
            {
                goalObject.transform.position = targetPos;
                break;
            }
        }
    }

    public override void AcademyStep()
    {


    }

    public void WriteSummary(int step)
    {

        //return;

        if (csvWriter == null)
            return;

        {
            List<string> columns = new List<string>();

            columns.Add(nGameNo.ToString());

            float totalReward = 0;
            for (int i = 0; i < trueAgent.Length; ++i)
            {

                totalReward += trueAgent[i].GetComponent<GridAgent>().GetReward();

            }


            columns.Add(totalReward.ToString());

            columns.Add(step.ToString());

            csvWriter.WriteRow(columns);
            nGameNo++;
        }
    }

    public void SetDone()
    {
        if(this.GetStepCount()<10)
        {
            Done();
            return;
        }
        

        int blockCnt = 0;

        for (int i = 0; i < 4; ++i)
        {

            Vector3 nearPos = goalObject.transform.position + Fugitive.actionPos[i];

            Collider[] block = Physics.OverlapBox(nearPos, new Vector3(0.3f, 0.3f, 0.3f));

            if (block.Where(col => col.gameObject.tag == "pit").ToArray().Length > 0)
            {
                blockCnt++;
            }

        }


        float blockBouse = blockCnt + 0.1f;

        foreach (var o in trueAgent)
        {
            
            float nextDist = Vector3.SqrMagnitude(o.transform.position - goalObject.transform.position);

            if (o.GetComponent<GridAgent>().maxObcCount>0)
                o.GetComponent<GridAgent>().SetReward(4f+blockCnt*2f);
            else
                o.GetComponent<GridAgent>().SetReward(5f);
           
        }

        WriteSummary(GetStepCount());

     
        bSuccess = true;


        UIDeBugText(string.Format("Clear:{0}-{1}", GetEpisodeCount(), GetStepCount()));


        Done();

    }


    public void UIDeBugText(string str)
    {
        debugText.text = str;
    }


    public void SetFail()
    {

        if (IsDone())
            return;

        foreach (var o in trueAgent)
        {

            o.GetComponent<GridAgent>().SetReward(-5f);

        }

        WriteSummary(GetStepCount());
        Done();

    }
}
