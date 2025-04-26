using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Block : BoardObject
{
    public Vector2 Position => transform.position;
    public Node Node;

    public void InitializeBlock(Node node)
    {
        Node = node;
        SpawnAnimation();
    }
    private void SpawnAnimation()
    {
        var baseScale = transform.localScale;
        transform.localScale = Vector3.zero;
        transform.DOScale(baseScale, 0.4f)
            .SetEase(Ease.OutBack); // you can tweak the ease for nicer feel
    }
}
