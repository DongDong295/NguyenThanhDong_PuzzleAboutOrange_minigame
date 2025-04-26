using DG.Tweening;
using UnityEngine;

public class Puzzle : BoardObject
{
    public Vector2 Position => transform.position;
    public Node Node;

    public void Initialize(Node node)
    {
        Node = node;
        SpawnAnimation();
    }

    public void SetBlock(Node node)
    {
        if (Node != null)
        {
            Node.OccupiedObject = null;
        }
        Node = node;
        Node.OccupiedObject = this;
    }

    private void SpawnAnimation()
    {
        var baseScale = transform.localScale;
        transform.localScale = Vector3.zero;
        transform.DOScale(baseScale, 0.4f)
            .SetEase(Ease.OutBack); // you can tweak the ease for nicer feel
    }
}
