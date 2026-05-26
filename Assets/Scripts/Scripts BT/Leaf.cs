using UnityEngine;

public class Leaf : BTNode
{
    public delegate BTStatus Tick();
    private Tick ProcessMethod;

    public Leaf(string name, Tick processMethod)
        : base(name)
    {
        this.ProcessMethod = processMethod;
    }

    public override BTStatus Process()
    {
        if(ProcessMethod != null)
        {
            return ProcessMethod();
        }
        return BTStatus.Failure;
    }
}
