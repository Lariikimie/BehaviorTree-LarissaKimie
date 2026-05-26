using UnityEngine;
using UnityEngine.AI;

public class MouseBehavior : MonoBehaviour
{
    public Transform cheese;
    public Transform home;

    public Transform topDoor;

    public Transform bottomDoor;
    public float hunger = 70f;

    public Animator animator;

    NavMeshAgent agent;
    BehaviorTree tree;

    enum ActionState {IDLE, WORKING};
    ActionState currentState = ActionState.IDLE;
    void Start()
    {
        tree = new BehaviorTree();
        agent = GetComponent<NavMeshAgent>();
        Sequencer getFood = new Sequencer("Get Food");
        Leaf isHungry = new Leaf("Is Hungry", IsHungry);
        Leaf goToKitchen = new Leaf("Go To Kitchen", GoToKitchen);
        Leaf goHome = new Leaf("Go Home", GoHome);
        Leaf goToTopDoor = new Leaf("Go To Top Door", GoToTopDoor);
        Leaf goToBottomDoor = new Leaf("Go To Bottom Door", GoToBottomDoor);
        
        BTSelector openDoor = new BTSelector("Open Door");
        openDoor.AddChild(goToTopDoor);
        openDoor.AddChild(goToBottomDoor);

        getFood.AddChild(isHungry);
        getFood.AddChild(openDoor);
        getFood.AddChild(goToKitchen);
        getFood.AddChild(goHome);
        tree.AddChild(getFood);

        tree.PrintTree();
    }

    private BTNode.BTStatus treeStatus = BTNode.BTStatus.Running;
    void Update()
    {
        if (treeStatus != BTNode.BTStatus.Success)
        {
            treeStatus = tree.Process();
        }

        bool isRunning = agent.velocity.magnitude > 0.1f;
        animator.SetBool("isRunning", isRunning);
        animator.SetFloat("runningSpeed", agent.velocity.magnitude / agent.speed);
    }

    private BTNode.BTStatus GoToKitchen()
    {
        return GetObject(cheese);
    }

    private BTNode.BTStatus GoHome()
    {
        BTNode.BTStatus result = GoToLocation(home.position); 
        if(result != BTNode.BTStatus.Success)
        {
            return result;
        }

        Transform child = gameObject.transform.GetChild(1);
        child.SetParent(null);
        child.gameObject.SetActive(false);
        hunger = 100f;

        return result;
    }

    private BTNode.BTStatus GoToLocation(Vector3 Target)
    {
        if(currentState == ActionState.IDLE)
        {
            agent.SetDestination(Target);
            currentState = ActionState.WORKING;
        }
        if (agent.pathPending)
        {
            return BTNode.BTStatus.Running;
        }
        if (agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            currentState = ActionState.IDLE;
            agent.ResetPath();
            return BTNode.BTStatus.Failure;
        }
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            currentState = ActionState.IDLE;
            return BTNode.BTStatus.Success;
        }
        return BTNode.BTStatus.Running;
        
    }

    private BTNode.BTStatus GoToTopDoor()
    {
        return GoToDoor(topDoor);
    }

    private BTNode.BTStatus GoToBottomDoor()
    {
        return GoToDoor(bottomDoor);
    }

    private BTNode.BTStatus GoToDoor(Transform door) 
    {
        BTNode.BTStatus result = GoToLocation(door.position);
        if(result != BTNode.BTStatus.Success)
        {
            return result;
        }

        DoorLock doorLock = door.GetComponent<DoorLock>();
        if (doorLock != null && doorLock.Locked)
        {
            return BTNode.BTStatus.Failure;
        }

        if (door.childCount > 0)
        {
            door.GetChild(0).gameObject.SetActive(false);
        }

        return BTNode.BTStatus.Success;
    }

    private BTNode.BTStatus GetObject(Transform objectToTake)
    {
        BTNode.BTStatus result = GoToLocation(objectToTake.position);
        if (result != BTNode.BTStatus.Success)
        {
            return result;
        }
        
        objectToTake.SetParent(transform);

        return BTNode.BTStatus.Success;
    }

    private BTNode.BTStatus IsHungry()
    {
        if(hunger > 50f)
        {
            return BTNode.BTStatus.Success;
        }
        return BTNode.BTStatus.Failure;
    }
}
