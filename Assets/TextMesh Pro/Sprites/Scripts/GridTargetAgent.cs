using System.Linq;
using UnityEngine;
using MLAgents;
public class GridTargetAgent : Agent
{
    [Header("Specific to GridWorld")]
    private GridAcademy academy;
    public float timeBetweenDecisionsAtInference;
    private float timeSinceDecision;

    float actionSucc = 1f;


    public static Vector3[] actionPos;

    public void Start()
    {
        
    }

    public override void InitializeAgent()
    {
        if(academy==null)
            academy = FindObjectOfType(typeof(GridAcademy)) as GridAcademy;

        if (actionPos == null)
        {
            actionPos = new Vector3[9];


            actionPos[0].Set(0f, 0, 1f);
            actionPos[1].Set(0f, 0, -1f);
            actionPos[2].Set(-1f, 0, 0f);
            actionPos[3].Set(1f, 0, 0f);

            actionPos[4].Set(1f, 0, -1f);
            actionPos[5].Set(-1f, 0, 1f);
            actionPos[6].Set(1f, 0, 1f);
            actionPos[7].Set(-1f, 0, -1f);
            actionPos[8].Set(0f, 0, 0f);

        }

     
    }

    public override void CollectObservations()
    {
        AddVectorObs(transform.position.x);
        AddVectorObs(transform.position.z);
        AddVectorObs(actionSucc);

        for (int i = 0; i < 9; ++i)
        {
            Vector3 targetPos = transform.position + actionPos[i];

            Collider[] blockTest = Physics.OverlapBox(targetPos, new Vector3(0.3f, 0.3f, 0.3f));

            if (blockTest.Where(col => col.gameObject.tag == "wall"
            || col.gameObject.tag == "pit" || col.gameObject.tag == "agent").ToArray().Length > 0)
            {
                AddVectorObs(0);
            }
            else
            {
                AddVectorObs(1);
            }
        }

      
    }

    // to be implemented by the developer
    public override void AgentAction(float[] vectorAction, string textAction)
    {

      
        int action = Mathf.FloorToInt(vectorAction[0]);



        if (GetStepCount() >= agentParameters.maxStep)
        {
            SetReward(10.0f);
            Done();
            return;
        }


        // 0 - Forward, 1 - Backward, 2 - Left, 3 - Right
        Vector3 targetPos = transform.position;

        targetPos = transform.position + actionPos[action];

        actionSucc = 0;

        Collider[] blockTest = Physics.OverlapBox(transform.position, new Vector3(0.3f, 0.3f, 0.3f));

        if (blockTest.Where(col => col.gameObject.tag=="agent").ToArray().Length >0)
        {
            SetReward(-1f+( (float)GetStepCount()/(float)agentParameters.maxStep) );
            Done();
            return;
        }

        blockTest = Physics.OverlapBox(targetPos, new Vector3(0.3f, 0.3f, 0.3f));
        if (blockTest.Where(col => col.gameObject.tag == "wall").ToArray().Length == 0)
        {
            if (blockTest.Where(col => col.gameObject.tag == "pit").ToArray().Length >= 1)
            {
                
            }
            else if (blockTest.Where(col => col.gameObject.tag == "agent").ToArray().Length >= 1)
            {
             
            }
            else
            {
                transform.position = targetPos;
                actionSucc = 1f;
            }


        }


        int nBlock = 0;

        for (int i = 0; i < 9; ++i)
        {
            targetPos = transform.position + actionPos[i];

            Collider[] block = Physics.OverlapBox(targetPos, new Vector3(0.3f, 0.3f, 0.3f));

            if (block.Where(col => col.gameObject.tag == "wall"
            || col.gameObject.tag == "pit" || col.gameObject.tag == "agent").ToArray().Length > 0)
            {
                nBlock++;
            }

        }

        if (nBlock >= 3)
        {
            AddReward(-0.02f * nBlock);
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