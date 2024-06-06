using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ground : MonoBehaviour
{
    private AStarManager manager;
    public SpriteRenderer spriteRenderer;


    public Vector2Int pos;
    public bool isWall;

    public void Set(AStarManager m, int x, int y, bool isWall)
    {
        manager = m;
        pos.x = x;
        pos.y = y;
        spriteRenderer.color = isWall ? Color.black : Color.white;
        transform.position = new Vector3(x, y, 0);
        this.isWall = isWall;
    }

    private void OnMouseDown()
    {
        manager.ClickNode(this, pos);
    }

    public void SetColor(Color color)
    {
        spriteRenderer.color = color;
    }

    public void SetColor()
    {
        spriteRenderer.color = isWall ? Color.black : Color.white;
    }
}
