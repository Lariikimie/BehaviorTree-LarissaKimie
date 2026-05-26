using System.Collections.Generic;
using UnityEngine;
public class BTNode
{
    public enum BTStatus { Success, Failure, Running };
    public BTStatus status; 
    public List<BTNode> children = new List<BTNode>();
    public int currentChild = 0;
    public string name;

    public BTNode(string name)
    {
        this.name = name;
    }

    public void AddChild(BTNode child)
    {
        children.Add(child);
    }

    public virtual BTStatus Process()
    {
        return children.Count > 0 ? children[currentChild].Process() : BTStatus.Success;
    }
}
    
