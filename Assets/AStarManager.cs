using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class Node
{
    public Node(bool _isWall, int _x, int _y) { isWall = _isWall; x = _x; y = _y; }

    public bool isWall;
    public Node ParentNode;

    // G : 시작으로부터 이동했던 거리, H : |가로|+|세로| 장애물 무시하여 목표까지의 거리, F : G + H
    public int x, y, G, H;
    public int F { get { return G + H; } }
}


public class AStarManager : MonoBehaviour
{
    public Vector2Int bottomLeft, topRight, startPos, targetPos;
    public List<Node> FinalNodeList;
    public bool allowDiagonal, dontCrossCorner;

    int sizeX, sizeY;
    Node[,] NodeArray;
    Node StartNode, TargetNode, CurNode;
    List<Node> OpenList, ClosedList;

    public Transform groundParent;
    public Ground baseGround;
    List<Ground> grounds = new List<Ground>();
    public int mode = 0;
    Ground startGround;
    Ground endGround;

    int[,] map;

    private void SettingMap()
    {
        map = new int[sizeX, sizeY];

        // 맵 그리기
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                Ground ground = Instantiate(baseGround, groundParent);
                ground.Set(this, x, y, map[x, y] == 1);
                grounds.Add(ground);
            }
        }
    }

    private void DrawMap()
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                map[x, y] = 0;
                if (x == 0 || x == sizeX - 1 || y == 0 || y == sizeY - 1) map[x, y] = 1; // 테두리 만들기
                else if (x % 2 == 0 || y % 2 == 0) map[x, y] = 1; // 격자 만들기
            }
        }

        // 미로 만들기
        for (int x = 0; x < sizeX; x++)
        {
            int count = 0;
            for (int y = 0; y < sizeY; y++)
            {
                if ((x > 0 && x < sizeX - 1 && y > 0 && y < sizeY - 1) && (x % 2 == 1 && y % 2 == 1))
                {
                    if (UnityEngine.Random.value < 0.5f)
                    {
                        map[x, y + 1] = 0;
                        count++;
                    }
                    else
                    {
                        int index = (UnityEngine.Random.Range(0, count) * 2);
                        map[x + 1, y - index] = 0;
                        count = 0;
                    }
                }
                else if (x == 0 || x == sizeX - 1 || y == 0 || y == sizeY - 1)
                {
                    map[x, y] = 1;
                }
            }
        }

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                for (int i = 0; i < grounds.Count; i++)
                {
                    if (grounds[i].pos.x == x && grounds[i].pos.y == y)
                    {
                        grounds[i].Set(this, x, y, map[x, y] == 1);
                        break;
                    }
                }
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PathFind();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            DrawMap();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1)) mode = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) mode = 1;
    }

    private void Start()
    {
        sizeX = topRight.x - bottomLeft.x + 1;
        sizeY = topRight.y - bottomLeft.y + 1;

        SettingMap();
        DrawMap();
    }
    public void PathFind()
    {
        StopAllCoroutines();
        // NodeArray의 크기 정해주고, isWall, x, y 대입

        NodeArray = new Node[sizeX, sizeY];

        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                bool isWall = false;
                // foreach (Collider2D col in Physics2D.OverlapCircleAll(new Vector2(i + bottomLeft.x, j + bottomLeft.y), 0.4f))
                //     if (col.gameObject.layer == LayerMask.NameToLayer("Wall")) isWall = true;

                NodeArray[i, j] = new Node(map[i, j] == 1, i + bottomLeft.x, j + bottomLeft.y);
            }
        }


        // 시작과 끝 노드, 열린리스트와 닫힌리스트, 마지막리스트 초기화
        StartNode = NodeArray[startPos.x - bottomLeft.x, startPos.y - bottomLeft.y];
        TargetNode = NodeArray[targetPos.x - bottomLeft.x, targetPos.y - bottomLeft.y];

        OpenList = new List<Node>() { StartNode };
        ClosedList = new List<Node>();
        FinalNodeList = new List<Node>();


        while (OpenList.Count > 0)
        {
            // 열린리스트 중 가장 F가 작고 F가 같다면 H가 작은 걸 현재노드로 하고 열린리스트에서 닫힌리스트로 옮기기
            CurNode = OpenList[0];
            for (int i = 1; i < OpenList.Count; i++)
                if (OpenList[i].F <= CurNode.F && OpenList[i].H < CurNode.H) CurNode = OpenList[i];

            OpenList.Remove(CurNode);
            ClosedList.Add(CurNode);


            // 마지막
            if (CurNode == TargetNode)
            {
                Node TargetCurNode = TargetNode;
                while (TargetCurNode != StartNode)
                {
                    FinalNodeList.Add(TargetCurNode);
                    TargetCurNode = TargetCurNode.ParentNode;
                }
                FinalNodeList.Add(StartNode);
                FinalNodeList.Reverse();

                for (int i = 0; i < FinalNodeList.Count; i++) print(i + "번째는 " + FinalNodeList[i].x + ", " + FinalNodeList[i].y);
                break;
            }


            // ↗↖↙↘
            if (allowDiagonal)
            {
                OpenListAdd(CurNode.x + 1, CurNode.y + 1);
                OpenListAdd(CurNode.x - 1, CurNode.y + 1);
                OpenListAdd(CurNode.x - 1, CurNode.y - 1);
                OpenListAdd(CurNode.x + 1, CurNode.y - 1);
            }

            // ↑ → ↓ ←
            OpenListAdd(CurNode.x, CurNode.y + 1);
            OpenListAdd(CurNode.x + 1, CurNode.y);
            OpenListAdd(CurNode.x, CurNode.y - 1);
            OpenListAdd(CurNode.x - 1, CurNode.y);
        }

        if (FinalNodeList.Count > 0)
            StartCoroutine(Animation());
    }

    void OpenListAdd(int checkX, int checkY)
    {
        // 상하좌우 범위를 벗어나지 않고, 벽이 아니면서, 닫힌리스트에 없다면
        if (checkX >= bottomLeft.x && checkX < topRight.x + 1 && checkY >= bottomLeft.y && checkY < topRight.y + 1 && !NodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y].isWall && !ClosedList.Contains(NodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y]))
        {
            // 대각선 허용시, 벽 사이로 통과 안됨
            if (allowDiagonal) if (NodeArray[CurNode.x - bottomLeft.x, checkY - bottomLeft.y].isWall && NodeArray[checkX - bottomLeft.x, CurNode.y - bottomLeft.y].isWall) return;

            // 코너를 가로질러 가지 않을시, 이동 중에 수직수평 장애물이 있으면 안됨
            if (dontCrossCorner) if (NodeArray[CurNode.x - bottomLeft.x, checkY - bottomLeft.y].isWall || NodeArray[checkX - bottomLeft.x, CurNode.y - bottomLeft.y].isWall) return;


            // 이웃노드에 넣고, 직선은 10, 대각선은 14비용
            Node NeighborNode = NodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y];
            int MoveCost = CurNode.G + (CurNode.x - checkX == 0 || CurNode.y - checkY == 0 ? 10 : 14);


            // 이동비용이 이웃노드G보다 작거나 또는 열린리스트에 이웃노드가 없다면 G, H, ParentNode를 설정 후 열린리스트에 추가
            if (MoveCost < NeighborNode.G || !OpenList.Contains(NeighborNode))
            {
                NeighborNode.G = MoveCost;
                NeighborNode.H = (Mathf.Abs(NeighborNode.x - TargetNode.x) + Mathf.Abs(NeighborNode.y - TargetNode.y)) * 10;
                NeighborNode.ParentNode = CurNode;

                OpenList.Add(NeighborNode);
            }
        }
    }

    public Transform hero;
    public Transform hero2;
    private IEnumerator Animation()
    {
        int index = 0;
        float time = 0f;
        Vector2 startPos = new Vector2(FinalNodeList[0].x, FinalNodeList[0].y);
        Vector2 targetPos = new Vector2(FinalNodeList[1].x, FinalNodeList[1].y);
        hero.position = startPos;
        SetAllColor();
        while (index < FinalNodeList.Count - 1)
        {
            time += Time.deltaTime * 10f;
            hero.position = Vector2.Lerp(startPos, targetPos, time);
            if (time >= 1f)
            {
                if (index + 1 >= FinalNodeList.Count - 1) break;
                time = 0f;
                GetNode(new Vector2Int(FinalNodeList[index].x, FinalNodeList[index].y)).SetColor(Color.red);
                index++;
                startPos = new Vector2(FinalNodeList[index].x, FinalNodeList[index].y);
                targetPos = new Vector2(FinalNodeList[index + 1].x, FinalNodeList[index + 1].y);
            }
            yield return null;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (FinalNodeList.Count != 0) for (int i = 0; i < FinalNodeList.Count - 1; i++)
                Gizmos.DrawLine(new Vector2(FinalNodeList[i].x, FinalNodeList[i].y), new Vector2(FinalNodeList[i + 1].x, FinalNodeList[i + 1].y));
    }

    private Ground GetNode(Vector2Int pos)
    {
        for (int i = 0; i < grounds.Count; i++)
        {
            if (grounds[i].pos == pos) return grounds[i];
        }
        return null;
    }

    private void SetAllColor()
    {
        for (int i = 0; i < grounds.Count; i++)
        {
            if (grounds[i] == endGround) continue;
            grounds[i].SetColor();
        }
    }

    public void ClickNode(Ground ground, Vector2Int pos)
    {
        if (ground.isWall) return;

        if (mode == 0)
        {
            startPos = pos;
            if (startGround != null)
                startGround.SetColor(Color.white);
            startGround = ground;
            startGround.SetColor(Color.red);
        }
        else if (mode == 1)
        {
            targetPos = pos;
            if (endGround != null)
                endGround.SetColor(Color.white);
            endGround = ground;
            endGround.SetColor(Color.blue);
        }

    }
}
