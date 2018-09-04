using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MLAgents;
public class GridAgent : Agent
{
    [Header("Specific to GridWorld")]
    private GridAcademy academy;
    public float timeBetweenDecisionsAtInference;
    private float timeSinceDecision;

    float actionSucc = 1f;

    public GameObject gobjpit = null;
    public static Vector3[] actionPos;

    public static GameObject obc_res = null;

    Vector3 preRewordPos;
    Vector3 prePos;

    public int restObcCount,maxObcCount;

    public List<GameObject> listObc = new List<GameObject>();


    public void OnEnable()
    {
        if (brain == null)
            return;

        base.OnEnable();
    } 

    public override void InitializeAgent()
    {
        academy = FindObjectOfType(typeof(GridAcademy)) as GridAcademy;

        if (actionPos == null)
        {
            actionPos = new Vector3[5];


            actionPos[0].Set(0f, 0, 1f);
            actionPos[1].Set(0f, 0, -1f);
            actionPos[2].Set(-1f, 0, 0f);
            actionPos[3].Set(1f, 0, 0f);
            actionPos[4].Set(0, 0, 0f);

        }

        preRewordPos = transform.position;

        if(obc_res ==null)
        {
            obc_res = (GameObject)Resources.Load("pit_x");
        }
  
        Init();
    }

    public void Init()
    {
        preRewordPos = transform.position;

        foreach (var o in listObc)
        {
            DestroyImmediate(o);
        }

        listObc.Clear();

        
        restObcCount = maxObcCount;

        prePos = transform.position;
    }

    public void SetPitMode(int nObcCount)
    {
        Debug.LogWarning("AddPlt");

        if (gobjpit == null)
        {
            gobjpit = Instantiate(obc_res);
            gobjpit.transform.position = transform.position;
            gobjpit.transform.parent = this.transform;
        }
        else
            gobjpit.SetActive(true);

        maxObcCount = nObcCount;
        restObcCount = nObcCount;
    }

    public void SetSight(int Range)
    {
      
    }

    public bool AddObc(Vector3 pos)
    {
        if (restObcCount < 1)
            return false;

        gobjpit = Instantiate(academy.pitPref);
        gobjpit.transform.position = pos;

        restObcCount--;

        listObc.Add(gobjpit);
        return true;

    }

    public override void CollectObservations()
    {
        AddVectorObs(gameObject.transform.position.x);
        AddVectorObs(gameObject.transform.position.z);
        AddVectorObs(actionSucc);

        for(int i=0; i<4;++i)
        {
            Vector3 targetPos = transform.position+ actionPos[i];

            Collider[] blockTest = Physics.OverlapBox(targetPos, new Vector3(0.3f, 0.3f, 0.3f));

            int coltype = 0;

            foreach (var col in blockTest)
            {
                switch (col.tag)
                {
                    case "wall":
                        coltype = 1;
                        break;
                    case "agent":
                        coltype = 2; 
                        break;
                    case "pit":
                        coltype = 3;
                        break;
                    default:
                        coltype = 0;
                        break;
                }

                //if (coltype > 0)
                //    coltype = 1;
            }
            AddVectorObs(coltype);
        }


           
        AddVectorObs(academy.goalObject.transform.position.x);
        AddVectorObs(academy.goalObject.transform.position.z);   ///9개
      
     
        //10개
        if (maxObcCount>0)
            AddVectorObs(restObcCount/ maxObcCount); 
        else
            AddVectorObs(0);


     
        AddVectorObs(0);

        AddVectorObs(0);
        AddVectorObs(0);            //13개
        AddVectorObs(0);            //14개
        AddVectorObs(0);            //15개
        AddVectorObs(0);            //16개

    }

    // to be implemented by the developer
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        actionSucc = 0;
        AddReward(-0.001f);
        int action = Mathf.FloorToInt(vectorAction[0]);


        if(action>= actionPos.Length)
        {
            if (AddObc(transform.position + actionPos[action - actionPos.Length]))
            {
                actionSucc = 1;
            }
            else
            {
                AddReward(-0.002f);
            }

            
           // academy.UIDeBugText(string.Format("{0}:Action No:{1}", this.GetInstanceID(), action));
            return;
        }
   
        // 0 - Forward, 1 - Backward, 2 - Left, 3 - Right
        Vector3 targetPos = transform.position;

        targetPos = transform.position + actionPos[action];



        int moveable = 0;

        for (int i = 0; i < actionPos.Length; ++i)
        {
            Vector3 nearPos = transform.position;

            nearPos = transform.position + actionPos[i];

            Collider[] block = Physics.OverlapBox(nearPos, new Vector3(0.3f, 0.3f, 0.3f));
            if (block.Where(col => col.gameObject.tag == "wall" || col.gameObject.tag == "pit" ).ToArray().Length == 0)
            {
                moveable += 1;
            }
        }

        if(moveable==0)
        {
            foreach (var o in academy.trueAgent)
            {

                if(o.GetComponent<GridAgent>().maxObcCount==0)
                    o.GetComponent<GridAgent>().SetReward(-0.5f);
                else
                    o.GetComponent<GridAgent>().SetReward(-1f);
            }

            Done();
            return;
        }


        Collider[] blockTest = Physics.OverlapBox(targetPos, new Vector3(0.3f, 0.3f, 0.3f));

        if (blockTest.Where(col => col.gameObject.tag == "wall").ToArray().Length == 0)
        {
            if (blockTest.Where(col => col.gameObject.tag == "pit").ToArray().Length >= 1)
            {
                //SetReward(-0.2f);  //충돌로 못가는 그리드를 이동 할려고 했을 경우 패널티를 준다..
                //Done();
                //SetReward(-1f);
                AddReward(-0.002f);
            }
            else if (blockTest.Where(col => col.gameObject.tag == "agent"&& col.gameObject != this).ToArray().Length >= 1)
            {
                AddReward(-0.002f);
                //AddReward(-0.02f);

            }
            else if (blockTest.Where(col => col.gameObject.tag == "goal").ToArray().Length >= 1)
            {
                return;
            }
            else
            {

                float preDist = Vector3.SqrMagnitude(transform.position - academy.goalObject.transform.position);

                float nextDist = Vector3.SqrMagnitude(targetPos - academy.goalObject.transform.position);

                if (nextDist < preDist && preRewordPos != targetPos && this.restObcCount == 0)
                {
                    preRewordPos = targetPos;
                    AddReward(0.002f);
                }

                prePos = transform.position;
                transform.position = targetPos;

                if (prePos == transform.position)
                {
                    AddReward(-0.002f);
                }

                actionSucc = 1;
            
            }


/*
            if (blockTest.Where(col => col.gameObject.tag == "goal").ToArray().Length == 1)
            {

                int blockCnt = 0;

                for (int i = 0; i < 4; ++i)
                {

                    Vector3 nearPos = academy.goalObject.transform.position + Fugitive.actionPos[i];

                    Collider[] block = Physics.OverlapBox(targetPos, new Vector3(0.3f, 0.3f, 0.3f));

                    if (block.Where(col => col.gameObject.tag == "wall"
                    || col.gameObject.tag == "pit" || col.gameObject.tag == "agent").ToArray().Length > 0)
                    {
                        blockCnt++;
                    }
                   
                }


                float blockBouse = blockCnt + 0.1f;


                transform.position = targetPos;
                //Done();

                float bonus = (GetStepCount() -agentParameters.maxStep) / agentParameters.maxStep; 

                SetReward(10f*(1f+ bonus+ blockBouse));
                actionSucc = action;



                foreach (var o in academy.actorObjs)
                {
                    if (o.tag == "agent" && o != gameObject)
                    {

                        float nextDist = Vector3.SqrMagnitude(o.transform.position - academy.goalObject.transform.position);

                        if(nextDist>1.4)
                            SetReward(10f * 0.3f*(1f + bonus+ blockBouse));
                        else
                            SetReward(10f * 0.5f * (1f + bonus+ blockBouse));
                    }
                }

                academy.WriteSummary(GetStepCount());
                Done();
            }
*/

        }
        else
        {
            AddReward(-0.005f);
        }
      

     
    }

    // to be implemented by the developer
    public override void AgentReset()
    {
        academy.AcademyReset();

    }

    public void FixedUpdate()
    {
        WaitTimeInference();
    }

    private void WaitTimeInference()
    {

        //if (GetStepCount() + 1 >= agentParameters.maxStep)
        //{
        //    SetReward(-3f);
        //    academy.UIDeBugText(string.Format("Step Over: {0}:{1}", this.GetInstanceID(),GetCumulativeReward()));
           
        //    return;
        //}



        if (!academy.GetIsInference())
        {
            RequestDecision();
        }
        else
        {
            if (timeSinceDecision >= timeBetweenDecisionsAtInference)
            {
                timeSinceDecision = 0f;
                RequestDecision();
            }
            else
            {
                timeSinceDecision += Time.fixedDeltaTime;
            }
        }
    }
}
