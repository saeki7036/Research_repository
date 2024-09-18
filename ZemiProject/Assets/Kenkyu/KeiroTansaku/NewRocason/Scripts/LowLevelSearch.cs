using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LowLevelSearch
{
    public class LowLevelPath
    {
        public AgentData[] agentData;
        public struct AgentData
        {
            public List<Vector2Int> Path;
            public Vector2Int GoalPos;
        }

        const int BOARD_WIDTH = 30;//x
        const int BOARD_HEIGHT = 20;//y
        (int, int)[] Move = { (0, 1), (0, -1), (1, 0), (-1, 0) };//x,y

        bool OverCheck(Vector2Int enter)
        {
            if (enter.x < 0 || enter.y < 0) return true;
            if (enter.x >= BOARD_WIDTH || enter.y >= BOARD_HEIGHT) return true;
            return false;
        }

        public LowLevelPath ? GetPathAndGoal(MapDataObject.Preset data)
        {
            if(data == null) return null;
            LowLevelPath lowLevelPath = new LowLevelPath();
            lowLevelPath.agentData = new AgentData[data.GetAgentMenber()];
            for (int i = 0; i < data.GetAgentMenber(); i++)
            {
                var goalAndPath = GetGoalAndPath(i,data);
                if(goalAndPath.Item1 == null)return null;

                lowLevelPath.agentData[i] = new()
                {
                    Path = goalAndPath.Item1,
                    GoalPos = goalAndPath.Item2
                };
            }
            
            return lowLevelPath;
        }

        public class SearchInfo
        {
            public SearchInfo Parent;
            public Vector2Int Pos;

            public SearchInfo(Vector2Int pos,SearchInfo parent = null)
            {
                Parent = parent;
                Pos = pos;              
            }
        }

        public void GetParentPos(List<Vector2Int> list, SearchInfo info)
        {
            if (info.Parent == null) list.Add(info.Pos);
            else
            {
                GetParentPos(list, info.Parent);
                list.Add(info.Pos);
            }
        }
        private (List<Vector2Int>, Vector2Int) GetGoalAndPath(int agent, MapDataObject.Preset data)
        {

            bool[] visit = Enumerable.Repeat(false, 
                BOARD_HEIGHT * BOARD_WIDTH).ToArray();// y * BOARD_HEIGHT + x;
            
            Queue<SearchInfo> openList = new();
            openList.Enqueue(new(data.GetStartPos(agent)));
            visit[openList.Peek().Pos.y * BOARD_HEIGHT + openList.Peek().Pos.x] = true;

            while (openList.Count > 0) 
            {
                SearchInfo info = openList.Dequeue();
                Vector2Int dequeue = info.Pos;

                foreach(var move in Move)
                {
                    Vector2Int enter = new Vector2Int(dequeue.x + move.Item1, dequeue.y + move.Item2);

                    if (OverCheck(enter)) continue;

                    if(visit[enter.y * BOARD_HEIGHT + enter.x]) continue;
                    visit[enter.y * BOARD_HEIGHT + enter.x] = true;

                    var mapObject = data.GetMapData(enter.y, enter.x);
                    if (mapObject == Map_Object.Wall) continue;
                    if (mapObject == Map_Object.Goal)
                    {
                        List<Vector2Int> path = new List<Vector2Int>();
                        GetParentPos(path,info);
                        path.Add(enter);
                        return (path, enter);
                    }
                    if (mapObject == Map_Object.Spase)
                    {
                        openList.Enqueue(new(enter,info));
                        continue;
                    }                
                }
            }
            return (null,new());
        }
    }    
}
