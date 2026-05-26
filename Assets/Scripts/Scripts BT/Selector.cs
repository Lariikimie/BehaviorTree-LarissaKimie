using UnityEngine;

public class BTSelector : BTNode 
{ 
    public BTSelector(string name) : base(name) 
    { 
        this.name = name; 
    } 
 
    public override BTStatus Process() 
    { 
        BTStatus childStatus = children[currentChild].Process(); 
 
        if(childStatus == BTStatus.Running) return childStatus; 
 
        if(childStatus == BTStatus.Success) 
        { 
            currentChild = 0; 
            return childStatus; 
        } 
 
        currentChild++;
        if(currentChild >= children.Count) return BTStatus.Failure;

        return BTStatus.Running; 
    }
}        
