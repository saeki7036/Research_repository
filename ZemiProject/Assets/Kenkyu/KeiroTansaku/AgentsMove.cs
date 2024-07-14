using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentsMove : MonoBehaviour
{
    [SerializeField]
    MapDataBase mapData;
    OutPut output;
    const int BOARD_WIDTH = 30;
    const int BOARD_HEIGHT = 20;
    int Stage_Index = 0;

    List<Info> Nabigate;

    public Agent[] agents;
    public class Agent
    {
        public int NabiCount = 0;
        public Vector2Int[] Nabi;
    }
    
    private class Info
    {
        public Info Parent { get; }
        public Vector2Int Pos { get; }
        public float TotalMoveCost { get; }
        public float ForecastCost { get; }

        public Info(Info parent, Vector2Int vector2Int, float totalMoveCost, Vector2Int goal)
        {
            Parent = parent;
            Pos = vector2Int;
            TotalMoveCost = totalMoveCost;
            ForecastCost = GetDistanceVecInt(Pos, goal);
        }
        public float Score => TotalMoveCost + ForecastCost;
        public bool GetSameInfo(List<Info> infos, Vector2Int pos, out Info oldInfo)
        {
            oldInfo = null;
            foreach (var info in infos)
                if (info.Pos == pos)
                {
                    oldInfo = info;
                    return true;
                }
            return false;
        }
    }
    void Start()
    {
        output = this.gameObject.GetComponent<OutPut>();
        Stage_Index = output.Stage_Index;
        agents = new Agent[mapData.preset[Stage_Index].Agents.Length];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Agents_Move()
    {
        if(agents[0] != null)
            for (int i = 0; i < mapData.preset[Stage_Index].Agents.Length; i++)
            {
                if (agents[i].NabiCount < agents[i].Nabi.Length)
                {
                    output.Agents[i].transform.position = new Vector3
                    (agents[i].Nabi[agents[i].NabiCount].x, 0, agents[i].Nabi[agents[i].NabiCount].y);
                    agents[i].NabiCount++;
                }
            }
    }

    public void Agents_Saerch()
    {
        for(int i = 0; i < mapData.preset[Stage_Index].Agents.Length; i++)
        {
            int Menber_index = i;
            Nabigate = new List<Info>();
            Saerch(Stage_Index, Menber_index);
            agents[i] = new Agent();
            agents[i].Nabi = new Vector2Int[Nabigate.Count];
            agents[i].NabiCount = Nabigate.Count;
            while (Nabigate.Count > 0)
            {
                Info useInfo = Nabigate[0];

                agents[i].Nabi[agents[i].NabiCount - Nabigate.Count] =  useInfo.Pos;

                Nabigate.Remove(useInfo);
            }
            agents[i].NabiCount = 0;
        } 
    }

    private static float GetDistanceCost(float distanceX, float distanceY)
    {
        const float SQUARE2 = 1.414f;
        if (distanceX > distanceY)
            return distanceY * SQUARE2 + (distanceX - distanceY);
        else
            return distanceX * SQUARE2 + (distanceY - distanceX);
    }
    private static float GetDistanceVecInt(Vector2Int a, Vector2Int b)
    {
        float X_dis = a.x - b.x;
        float Y_dis = a.y - b.y;
        if (X_dis < 0) X_dis *= -1;
        if (Y_dis < 0) Y_dis *= -1;
        return GetDistanceCost(X_dis, Y_dis);
    }

    private void Saerch(int stege, int menber)
    {
        Vector2Int GoalPos = mapData.GetGoalPos(stege, menber);
        Info StartInfo = new Info(null, mapData.GetStartPos(stege, menber), 0, GoalPos);
        List<Info> OpenList = new List<Info>();
        List<Info> CloseList = new List<Info>();
        OpenList.Add(StartInfo);
        while (OpenList.Count > 0)
        {
            Info useInfo = OpenList[0];

            for (int i = 0; i < OpenList.Count; i++)
            {
                if (OpenList[i].Score < useInfo.Score ||
                    OpenList[i].Score == useInfo.Score &&
                    OpenList[i].TotalMoveCost < useInfo.TotalMoveCost)
                {
                    useInfo = OpenList[i];
                }
            }
            OpenList.Remove(useInfo);
            CloseList.Add(useInfo);

            //Debug.Log(useInfo.Pos);

            if (useInfo.Pos == GoalPos)
            {
                Nabigate = RetracePath(StartInfo, useInfo);

                break;
            }

            NodeSearch(useInfo, stege, OpenList, CloseList, GoalPos);
        }
    }

    private static List<Info> RetracePath(Info start, Info end)
    {
        List<Info> path = new List<Info>();
        Info currentInfo = end;

        while (currentInfo != null)
        {
            path.Add(currentInfo);
            currentInfo = currentInfo.Parent;
        }
        path.Reverse();
        return path;
    }

    private void NodeSearch(Info info, int stege, List<Info> openList, List<Info> closeList, Vector2Int goalPos)
    {
        for (int X = -1; X <= 1; X++)
            for (int Y = -1; Y <= 1; Y++)
            {
                if (X == 0 && Y == 0)
                    continue;

                //斜め除外
                if (X + Y != 1 && X + Y != -1)
                    continue;

                Vector2Int checkPos = new Vector2Int(info.Pos.x + X, info.Pos.y + Y);

                bool OverCheck_x = (checkPos.x >= 0 && checkPos.x < BOARD_WIDTH);
                bool OverCheck_y = (checkPos.y >= 0 && checkPos.y < BOARD_HEIGHT);

                if (OverCheck_x && OverCheck_y)
                {
                    Map_Object movePos = mapData.GetMapData(stege, checkPos.y, checkPos.x);

                    //壁除外
                    if (movePos == Map_Object.Wall)
                        continue;
                    //他エージェント除外
                    //if (movePos == Map_Object.Agent)
                    //    continue;

                    float totalMoveCost = info.TotalMoveCost + 1.0f;

                    // 既に調査済みである
                    if (info.GetSameInfo(closeList, checkPos, out var close) == true)
                    {
                        // トータル移動コストが既存以上なら差し替え不要
                        if (totalMoveCost >= close.TotalMoveCost)
                            continue;

                        closeList.Remove(close);
                    }

                    // 現在調査中である
                    if (info.GetSameInfo(openList, checkPos, out var open) == true)
                    {
                        // トータル移動コストが既存以上なら差し替え不要
                        if (totalMoveCost >= open.TotalMoveCost)
                            continue;

                        openList.Remove(open);
                    }

                    Info neighborInfo = new Info(info, checkPos, totalMoveCost, goalPos);
                    openList.Add(neighborInfo);
                }
            }
    }
}
