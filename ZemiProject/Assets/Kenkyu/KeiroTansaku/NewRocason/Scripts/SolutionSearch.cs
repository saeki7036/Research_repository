using System.Collections.Generic;
using UnityEngine;

public class SolutionSearch
{
    const int BOARD_WIDTH = 30;//x
    const int BOARD_HEIGHT = 20;//y

    private class Info
    {
        public Info Parent { get; }
        public Vector2Int Pos { get; }
        public int TotalMoveCost { get; }
        public float ForecastCost { get; }

        public Info(Info parent, Vector2Int vector2Int, int totalMoveCost, Vector2Int goal)
        {
            Parent = parent;
            Pos = vector2Int;
            TotalMoveCost = totalMoveCost;
            ForecastCost = GetDistanceCost(Pos, goal);
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

        private float GetDistanceCost(Vector2Int a, Vector2Int b)
        {
            const float SQUARE2 = 1.414f;

            float X_dis = a.x - b.x;
            float Y_dis = a.y - b.y;
            if (X_dis < 0) X_dis *= -1;
            if (Y_dis < 0) Y_dis *= -1;

            if (X_dis > Y_dis)
                return Y_dis * SQUARE2 + (X_dis - Y_dis);
            else
                return X_dis * SQUARE2 + (Y_dis - X_dis);
        }
    }
    Info GetInfoWithList(List<Info> OpenList)
    {
        Info returnInfo = OpenList[0];

        for (int i = 0; i < OpenList.Count; i++)
        {
            if (OpenList[i].Score < returnInfo.Score ||
                (OpenList[i].Score == returnInfo.Score &&
                OpenList[i].TotalMoveCost < returnInfo.TotalMoveCost))
            {
                returnInfo = OpenList[i];
            }
        }
        return returnInfo;
    }
    public List<Vector2Int>[] GetSolutionPaths(MapDataObject.Preset data, Vector2Int[] goalPos, Dictionary<int, List<HighLevelSearch.Conflict>> conflicts)
    {
        List<Vector2Int>[] ReturnPath = new List<Vector2Int>[data.agents.Length];
        for (int i = 0; i < data.agents.Length; i++)
        {
            List<Vector2Int> vector2Ints = SearchWithConstraint(data, i, goalPos[i], conflicts);
            ReturnPath[i] = vector2Ints;
        }
        return ReturnPath;
    }

    enum CloseSearch
    {
        Unconfirmed,
        ReEntry,
        Visited,
    }

    private List<Vector2Int> SearchWithConstraint(MapDataObject.Preset data, int menber, Vector2Int goalpos, Dictionary<int, List<HighLevelSearch.Conflict>> conflicts)
    {
        Vector2Int GoalPos = goalpos;
        Vector2Int StartPos = data.GetStartPos(menber);

        Info StartInfo = new Info(null, StartPos, 0, GoalPos);

        List<Info> OpenList = new List<Info>();

        CloseSearch[] CloseSearched = new CloseSearch[BOARD_WIDTH * BOARD_HEIGHT];
        for (int i = 0; i < BOARD_WIDTH * BOARD_HEIGHT; i++)
            CloseSearched[i] = CloseSearch.Unconfirmed;

        List<Info> CloseList = new List<Info>();

        OpenList.Add(StartInfo);


        int c = 0;

        while (OpenList.Count > 0)
        {
            Info useInfo = GetInfoWithList(OpenList);
            OpenList.Remove(useInfo);
            Debug.Log(useInfo.Pos);
            if (useInfo.Pos == GoalPos)
            {
                List<Info> Nabigate;
                Nabigate = RetracePath(StartInfo, useInfo);
                List<Vector2Int> Pos = new List<Vector2Int>();
                while (Nabigate.Count > 0)
                {
                    Pos.Add(Nabigate[0].Pos);
                    Nabigate.Remove(Nabigate[0]);
                }
                return Pos;
            }

            PosSearchWithConstraint(useInfo, data, menber, OpenList, CloseSearched, GoalPos, conflicts);

            if (CloseSearched[useInfo.Pos.x + useInfo.Pos.y * BOARD_HEIGHT] == CloseSearch.ReEntry)
            {

            }
            else if (CloseSearched[useInfo.Pos.x + useInfo.Pos.y * BOARD_HEIGHT] == CloseSearch.Unconfirmed)
            {
                CloseSearched[useInfo.Pos.x + useInfo.Pos.y * BOARD_HEIGHT] = CloseSearch.Visited;
            }

            CloseList.Add(useInfo);

            c++;
            if (c == 300) break;
        }
        Debug.Log("Agent:" + menber + "探索失敗");
        return null;
    }
    private List<Info> RetracePath(Info start, Info end)
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
    bool OverCheck(Vector2Int enter)
    {
        if (enter.x < 0 || enter.y < 0) return true;
        if (enter.x >= BOARD_WIDTH || enter.y >= BOARD_HEIGHT) return true;
        return false;
    }

    private void PosSearchWithConstraint(Info info, MapDataObject.Preset data, int menber, List<Info> openList,
        CloseSearch[] closeList, Vector2Int goalPos, Dictionary<int, List<HighLevelSearch.Conflict>> conflicts)
    {
        (int, int)[] Move = { (0, 1), (0, -1), (1, 0), (-1, 0), (0,0)};//x,y
        foreach (var move in Move)
        {
            Vector2Int checkPos = new Vector2Int(info.Pos.x + move.Item1, info.Pos.y + move.Item2);
            if (OverCheck(checkPos)) continue;

            var mapObj = data.GetMapData(checkPos.y, checkPos.x);

            if (mapObj == Map_Object.Wall) continue;

            int totalMoveCost = info.TotalMoveCost + 1;

            if (conflicts != null && conflicts.ContainsKey(totalMoveCost) == true)
            {
                if (ConstraintCheck(menber, checkPos, conflicts[totalMoveCost]))
                {
                    continue;
                }
            }

            // 既に調査済みである
            if (closeList[checkPos.x + checkPos.y * BOARD_HEIGHT] == CloseSearch.Visited)
            {
                    continue;
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
        /*
         for (int X = -1; X <= 1; X++)
            for (int Y = -1; Y <= 1; Y++)
            {
                //if (X == 0 && Y == 0)
                //    continue;

                //斜め除外
                if (X + Y != 1 && X + Y != -1)
                    continue;

                Vector2Int checkPos = new Vector2Int(info.Pos.x + X, info.Pos.y + Y);

                //場外除外
                bool OverCheck_x = (checkPos.x >= 0 && checkPos.x < BOARD_WIDTH);
                bool OverCheck_y = (checkPos.y >= 0 && checkPos.y < BOARD_HEIGHT);
                if (!(OverCheck_x && OverCheck_y))
                    continue;

                //壁除外
                Map_Object movePos = data.GetMapData(checkPos.y, checkPos.x);
                if (movePos == Map_Object.Wall)
                    continue;

                //他エージェント除外
                //if (movePos == Map_Object.Agent)
                //   continue;

                int totalMoveCost = info.TotalMoveCost + 1;

                if (conflicts != null && conflicts.ContainsKey(totalMoveCost) == true)
                    if (ConstraintCheck(menber, checkPos, conflicts[totalMoveCost]))
                    {
                        Info stayInfo = new Info(info, info.Pos, totalMoveCost, goalPos);

                        if (ConstraintCheck(menber, info.Pos, conflicts[totalMoveCost]) == false)
                        {
                            openList.Add(stayInfo);
                            closeList[info.Pos.x + info.Pos.y * BOARD_HEIGHT] = CloseSaerch.ReEntry;
                        }
                        else
                        {
                            Info returnInfo = new Info(info, info.Parent.Pos, totalMoveCost, goalPos);
                            Info ParentInfo = returnInfo;

                            while (returnInfo.Pos == info.Pos)
                            {
                                ParentInfo = ParentInfo.Parent;
                                if (ParentInfo == null)
                                {
                                    break;
                                }
                                returnInfo = new Info(info, ParentInfo.Pos, totalMoveCost, goalPos);
                            }

                            if (ParentInfo == null)
                                continue;

                            openList.Add(returnInfo);

                            closeList[info.Pos.x + info.Pos.y * BOARD_HEIGHT] = CloseSaerch.ReEntry;
                            closeList[returnInfo.Pos.x + returnInfo.Pos.y * BOARD_HEIGHT] = CloseSaerch.ReEntry;
                        }
                        /*
                        int[] Move_X = { 0, -1, 1, 0 };
                        int[] Move_Y = { 1, 0, 0, -1 };
                        for (int i = 0; i < 4;i++)
                        {
                            Vector2Int movingPos = new Vector2Int(info.Pos.x + Move_X[i], info.Pos.y + Move_Y[i]);

                            OverCheck_x = (movingPos.x >= 0 && movingPos.x < BOARD_WIDTH);
                            OverCheck_y = (movingPos.y >= 0 && movingPos.y < BOARD_HEIGHT);
                            if (!OverCheck_x || !OverCheck_y)
                                continue;

                            Map_Object movingObj = mapData.GetMapData(stege, movingPos.y, movingPos.x);
                            //壁除外
                            if (movingObj == Map_Object.Wall)
                                continue;

                        }
        continue;
    }

                // 既に調査済みである
                if (closeList[checkPos.x + checkPos.y * BOARD_HEIGHT] == CloseSaerch.Visited)
                {
                    //if (totalMoveCost >= closeList[checkPos.x + checkPos.y * BOARD_HEIGHT].Cost)
                    continue;
                    //closeList[checkPos.x + checkPos.y * BOARD_HEIGHT].Saerch = false;
                    //closeList[checkPos.x + checkPos.y * BOARD_HEIGHT].Cost = 10000;
                }

                /*
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
                    if (closeList[checkPos.x + checkPos.y * BOARD_HEIGHT] != CloseSaerch.ReEntry)
                    {
                        // トータル移動コストが既存以上なら差し替え不要
                        if (totalMoveCost >= open.TotalMoveCost)
                            continue;

                        openList.Remove(open);
                    }
                }

                Info neighborInfo = new Info(info, checkPos, totalMoveCost, goalPos);
openList.Add(neighborInfo);

            }
         */

    }
    private bool ConstraintCheck(int nowAgent, Vector2Int checkPos, List<HighLevelSearch.Conflict> constraint)
    {
        for (int i = 0; i < constraint.Count; i++)
            if (constraint[i].Pos == checkPos)
            {
                if (constraint[i].Agent == nowAgent)
                    return false;
                else
                    return true;
            }

        foreach(var i in constraint)
        {
            if(i.Agent == nowAgent)
            {
                return true;
            }
        }
        return false;
    }
}