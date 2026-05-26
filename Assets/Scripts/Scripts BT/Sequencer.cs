using UnityEngine;

public class Sequencer : BTNode
{
    public Sequencer(string name) : base(name)
    {
    }

    public override BTStatus Process()
    {
        BTStatus childStatus = children[currentChild].Process();

        if (childStatus == BTStatus.Running || childStatus == BTStatus.Failure)
        {
            return childStatus;
        }

        currentChild++;

        if (currentChild >= children.Count)
        {
            currentChild = 0;
            return BTStatus.Success;
        }
        return BTStatus.Running;
    }

}
