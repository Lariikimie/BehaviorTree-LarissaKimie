using UnityEngine;
using System.Collections.Generic;
public class BehaviorTree : BTNode
{
    struct NodeLevel
    {
        public int level;
        public BTNode node;
    }
    public BehaviorTree() : base("Tree") { }
    public BehaviorTree(string name) : base(name) { }
    public override BTStatus Process()
    {
        return children[currentChild].Process();
    }

    public void PrintTree()
    {
        string treeStructure = "";
        Stack<NodeLevel> nodeStack = new Stack<NodeLevel>();
        BTNode currentNode = this;
        nodeStack.Push(new NodeLevel { level = 0, node = currentNode });

        while (nodeStack.Count > 0)
        {
            NodeLevel nextNode = nodeStack.Pop();
            treeStructure += new string(' ', nextNode.level * 2) + nextNode.node.name + "\n";
            for (int i = nextNode.node.children.Count - 1; i >= 0; i--)
            {
                nodeStack.Push(new NodeLevel { level = nextNode.level + 1, node = nextNode.node.children[i] });
            }
        }
        Debug.Log(treeStructure);
    }
}
