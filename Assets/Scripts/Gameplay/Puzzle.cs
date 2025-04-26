using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle : BoardObject
{
    public Vector2 Position => transform.position;
    public Node Node;
    public void SetBlock(Node node)
    {
        if (Node != null)
        {
            Node.OccupiedObject = null;
        }
        Node = node;
        Node.OccupiedObject = this;
    }
}
