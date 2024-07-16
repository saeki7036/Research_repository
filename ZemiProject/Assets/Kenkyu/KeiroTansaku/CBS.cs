using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;

public class CBS : MonoBehaviour
{
    //盤面の変数
    [SerializeField]
    MapDataBase mapData;
    OutPut output;
    const int BOARD_WIDTH = 30;
    const int BOARD_HEIGHT = 20;
    int Stage_Index = 0;

    List<Vector2Int>[] AgentsNabi;
    int[] Nabicount;
    // ノードクラス。各ノードはエージェントのパスとそのパスでの衝突情報を持つ
    public class Node
    {
        public Dictionary<int, List<Vector2Int>> Paths; // 各エージェントのパスを格納
        public Dictionary<int , List<Conflict>> Conflicts; // ノード内のすべての衝突のリスト
        public Node Parent; // 親ノードへの参照
        public int Score;
        // コンストラクタ。初期パスと親ノードを受け取る
        public Node(Dictionary<int, List<Vector2Int>> paths, Dictionary<int, List<Conflict>> conflicts,int score ,Node parent = null)
        {
            Paths = paths;
            Conflicts = conflicts;
            Parent = parent;
            Score = score;
        }
    }

    public class Conflict
    {
        public Vector2Int Pos; // 衝突した場所
        public int Agent; // 衝突が発生した時刻
        // コンストラクタ。衝突の詳細を受け取る
        public Conflict(Vector2Int pos, int agent)
        {
            Pos = pos;
            Agent = agent;
        }
    }
    void Start()
    {
        output = this.gameObject.GetComponent<OutPut>();//盤面の取得
        Stage_Index = output.Stage_Index;//インデックス取得

        //mapData.preset[Stage_Index].Agents.Length;//人の数取得
    }

    public void Agents_Move()
    {
        if (AgentsNabi != null)
            for (int i = 0; i < mapData.preset[Stage_Index].Agents.Length; i++)
            {
                if (AgentsNabi[i].Count > Nabicount[i])
                {
                    output.Agents[i].transform.position = new Vector3
                    (AgentsNabi[i][Nabicount[i]].x, 0, AgentsNabi[i][Nabicount[i]].y);
                    Nabicount[i]++;
                }
            }
    }
    public void Agents_Saerch()
    {
        var openList = new List<Node>(); // オープンリスト。探索中のノードを管理
        var initialPaths = new Dictionary<int, List<Vector2Int>>(); // 初期経路
        int AgentsMember = mapData.preset[Stage_Index].Agents.Length;

        // 各エージェントの初期経路をA*で計算
        for (int agent = 0; agent < AgentsMember; agent++)
        {
            initialPaths[agent] = AStar(Stage_Index, agent);
            if (initialPaths[agent] == null)
            {
                Debug.LogWarning("エージェント:" + agent + "経路なし");
                return;
            }
        }

        var root = new Node(initialPaths, new Dictionary<int, List<Conflict>>(), GetScore(initialPaths , AgentsMember)); // 初期ノードを作成
       
        openList.Add(root); // オープンリストに初期ノードを追加

        int c = 0;
        int before1 = -1, before2= -1, beforeT = -1;

        // オープンリストが空になるまで探索を続ける
        while (openList.Count > 0)
        {
            int nodeNum = GetNodeNum(openList);
            var current = openList[nodeNum]; // 現在のノード
            openList.RemoveAt(nodeNum); // オープンリストから削除
            
            var conflict = FindFirstConflict(current.Paths); // 衝突を検出
            //Debug.Log(conflict);
            if (conflict == null)
            {
                AgentsNabi = new List<Vector2Int>[AgentsMember];
                Nabicount = new int[AgentsMember];
                // 衝突がない場合、解が見つかった
                for (int agent = 0; agent < AgentsMember; agent++)
                {
                    Nabicount[agent] = 0;
                    AgentsNabi[agent] = new List<Vector2Int>();
                    AgentsNabi[agent] = current.Paths[agent];
                    Debug.Log("Agent:" + agent + "Count" + AgentsNabi[agent].Count);
                }
                return;
            }
            else if (conflict == (-1,-1,-1))
                continue;
            
            var (agent1, agent2, time) = conflict.Value;
            bool SameBefore = !(agent1 == before1 && agent2 == before2 && time == beforeT);
            // 衝突を解決するために新しい制約を追加し、ノードを生成
            if (true)
            {
                var newConflict1 = current.Conflicts;

                var newPaths1 = new Dictionary<int, List<Vector2Int>>(current.Paths);
                var constraint1 = new Conflict(newPaths1[agent1][time], agent1);

                if (!newConflict1.ContainsKey(time))
                    newConflict1.Add(time, new List<Conflict>());
                newConflict1[time].Add(constraint1);

                newPaths1[agent1] = AStarWithConstraint(Stage_Index, agent1, newConflict1);

                var newNode1 = new Node(newPaths1, newConflict1, GetScore(newPaths1, AgentsMember), current);

                //newNode1.Conflicts.AddRange(current.Conflicts);
                //newNode1.Conflicts.Add((agent1, agent2, time));

                openList.Add(newNode1);

                var newConflict2 = current.Conflicts;

                var newPaths2 = new Dictionary<int, List<Vector2Int>>(current.Paths);
                var constraint2 = new Conflict(newPaths2[agent2][time], agent2);

                if (!newConflict2.ContainsKey(time))
                    newConflict2[time] = new List<Conflict>();
                newConflict2[time].Add(constraint2);

                newPaths2[agent2] = AStarWithConstraint(Stage_Index, agent2, newConflict2);

                var newNode2 = new Node(newPaths2, newConflict2, GetScore(newPaths2, AgentsMember), current);

                //newNode2.Conflicts.AddRange(current.Conflicts);
                //newNode2.Conflicts.Add((agent1, agent2, time));

                openList.Add(newNode2);
            }

            before1 = agent1; before2 = agent2; beforeT = time;

            c++;
            if (c == 1000)
            {
                Debug.Log("探索失敗＼（＾o＾）／");
                return;
            }
                
        }
        // 解が見つからなかった場合nullを返す
        Debug.Log("探索失敗＼（＾o＾）／");
        return;
    }

    private int GetScore(Dictionary<int, List<Vector2Int>> dictionary, int member)
    {
        int Maxscore = 0;
        for (int agent = 0; agent < member; agent++)
        {
            if (dictionary[agent] == null)
                return 999999;
            Maxscore += dictionary[agent].Count;
        }
        return Maxscore;
    }

    private int GetNodeNum(List<Node> nodelist)
    {
        if (nodelist.Count == 1) return 0;

        int Minscore = nodelist[0].Score;
        int number = 0;
        for (int i = 1; i < nodelist.Count; i++)
        {
            if (nodelist[i].Score < Minscore)
            {
                Minscore = nodelist[i].Score;
                number = i;
            }
        }
        return number;
    }


    // エージェントの経路間で最初の衝突を見つける
    private (int, int, int)? FindFirstConflict(Dictionary<int, List<Vector2Int>> paths)
    {
        foreach (var agent1 in paths.Keys)
        {
            if (paths[agent1] == null)
                return (-1, -1, -1);
        }
            // エージェントの経路をチェックして最初の衝突を探す
            foreach (var agent1 in paths.Keys)
        {
            for (int t = 0; t < paths[agent1].Count; t++)
            {
                foreach (var agent2 in paths.Keys)
                {
                    bool sameAgentsCheck = agent1 == agent2;
                    if (sameAgentsCheck)
                        continue;

                    bool overTimeCheck = t < paths[agent2].Count;
                    if (!overTimeCheck)
                        continue;

                    if (paths[agent1][t] == paths[agent2][t])
                    {
                        Debug.Log("agentA:" + agent1 + " agentB:" + agent2 + " Time:" + t + 
                            " PosX:" + paths[agent1][t].x + " PosY" + paths[agent1][t].y);
                        // 衝突が見つかった場合、その情報を返す
                        return (agent1, agent2, t);
                    }
                    else if(t + 1 < paths[agent2].Count)
                    {
                        int next = t + 1;
                        if (paths[agent1][t] == paths[agent2][next] && paths[agent1][next] == paths[agent2][t])
                        {
                            Debug.Log("agentA:" + agent1 + " agentB:" + agent2 + " Time:" + next +
                                " A_PosX:" + paths[agent1][next].x + " A_PosY" + paths[agent1][next].y +
                                " B_PosX:" + paths[agent2][next].x + " B_PosY" + paths[agent2][next].y);
                            // 衝突が見つかった場合、その情報を返す
                            return (agent1, agent2, next);
                        }
                    }
                }
            }
        }
        // 衝突が見つからなかった場合nullを返す
        return null;
    }

    private List<Vector2Int> AStar(int index, int agent)
    {
        return Saerch(index, agent);
    }

    private List<Vector2Int> AStarWithConstraint(int index, int agent, Dictionary<int , List<Conflict>> conflicts)
    {       
        return SaerchWithConstraint(index, agent, conflicts);
    }

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
    private List<Vector2Int> Saerch(int stege, int menber)
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
                    (OpenList[i].Score == useInfo.Score &&
                    OpenList[i].TotalMoveCost < useInfo.TotalMoveCost))
                {
                    useInfo = OpenList[i];
                }
            }
            OpenList.Remove(useInfo);
            CloseList.Add(useInfo);

            if (useInfo.Pos == GoalPos)
            {
                List<Info> Nabigate;
                Nabigate = RetracePath(StartInfo, useInfo);
                List <Vector2Int> Pos = new List<Vector2Int>();
                while (Nabigate.Count > 0)
                {                  
                    Pos.Add(Nabigate[0].Pos);
                    Nabigate.Remove(Nabigate[0]);
                }
                return Pos;
            }

            PosSearch(useInfo, stege, OpenList, CloseList, GoalPos);
        }
        return new List<Vector2Int>();
    }

    class CloseSaerch
    {
        public bool Saerch;
        public int Cost;
        public CloseSaerch()
        {
            Saerch = false;
            Cost = 10000;
        }
    }


    private List<Vector2Int> SaerchWithConstraint(int stege, int menber, Dictionary<int, List<Conflict>> conflicts)
    {
        Vector2Int GoalPos = mapData.GetGoalPos(stege, menber);
        Vector2Int StartPos = mapData.GetStartPos(stege, menber);

        Info StartInfo = new Info(null, StartPos, 0, GoalPos);

        List<Info> OpenList = new List<Info>();

        CloseSaerch[] CloseSaerched = new CloseSaerch[BOARD_WIDTH * BOARD_HEIGHT];
        for(int i = 0; i < BOARD_WIDTH * BOARD_HEIGHT; i++)
            CloseSaerched[i] = new CloseSaerch();

        List<Info> CloseList = new List<Info>();

        OpenList.Add(StartInfo);

        while (OpenList.Count > 0)
        {
            Info useInfo = OpenList[0];

            for (int i = 0; i < OpenList.Count; i++)
            {
                if (OpenList[i].Score < useInfo.Score ||
                    (OpenList[i].Score == useInfo.Score &&
                    OpenList[i].TotalMoveCost < useInfo.TotalMoveCost))
                {
                    useInfo = OpenList[i];
                }
            }

            if (useInfo.Pos.y == 3 || useInfo.Pos.y == 1)
                Debug.Log("インチ");

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

            OpenList.Remove(useInfo);

            PosSearchWithConstraint(useInfo, stege, menber, OpenList, CloseSaerched, GoalPos,conflicts);

            CloseSaerched[useInfo.Pos.x + useInfo.Pos.y * BOARD_HEIGHT].Saerch = true;
            if(CloseSaerched[useInfo.Pos.x + useInfo.Pos.y * BOARD_HEIGHT].Cost > useInfo.TotalMoveCost)
            CloseSaerched[useInfo.Pos.x + useInfo.Pos.y * BOARD_HEIGHT].Cost = useInfo.TotalMoveCost;

            CloseList.Add(useInfo);
        }
        Debug.Log("Agent:" + menber + "探索失敗");
        return null;
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

    private void PosSearch(Info info, int stege, List<Info> openList, List<Info> closeList, Vector2Int goalPos)
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
                    //   continue;

                    int totalMoveCost = info.TotalMoveCost + 1;
                   
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

    private bool ConstraintCheck(int nowagent,Vector2Int checkPos, List<Conflict> constraint)
    {
        for(int i = 0; i < constraint.Count; i++)   
            if (constraint[i].Agent == nowagent)
                if (constraint[i].Pos == checkPos)
                    return true;
        return false;
    }

    private void PosSearchWithConstraint(Info info, int stege, int menber,List<Info> openList,
        CloseSaerch[] closeList, Vector2Int goalPos, Dictionary<int, List<Conflict>> conflicts)
    {
         for (int X = -1; X <= 1; X++)
            for (int Y = -1; Y <= 1; Y++)
            {
                //if (X == 0 && Y == 0)
                //    continue;

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
                    //   continue;

                    int totalMoveCost = info.TotalMoveCost + 1;

                    if (conflicts.ContainsKey(totalMoveCost) == true)
                        if (ConstraintCheck(menber, checkPos, conflicts[totalMoveCost]))
                        {
                            Info stayInfo = new Info(info, info.Pos, totalMoveCost, goalPos);

                            if (ConstraintCheck(menber, info.Pos, conflicts[totalMoveCost]) == false)                                
                            {
                                openList.Add(stayInfo);
                                closeList[info.Pos.x + info.Pos.y * BOARD_HEIGHT].Cost = totalMoveCost;
                            }
                            else
                            {
                                Info returnInfo = new Info(info, info.Parent.Pos, totalMoveCost, goalPos);
                                Info ParentInfo = returnInfo;
                                
                                while (returnInfo.Pos == info.Pos)
                                {
                                    if(ParentInfo.Parent == null)
                                    {
                                        break;
                                    }
                                    ParentInfo = ParentInfo.Parent;
                                    returnInfo = new Info(info, ParentInfo.Pos, totalMoveCost, goalPos);
                                }

                                if (ParentInfo.Parent == null)
                                    continue;

                                openList.Add(returnInfo);

                                closeList[returnInfo.Pos.x + returnInfo.Pos.y * BOARD_HEIGHT].Cost = totalMoveCost + 1;
                                closeList[info.Pos.x + info.Pos.y * BOARD_HEIGHT].Cost = totalMoveCost + 2;

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
                                   
                                
                            }*/
                            continue;
                        }

                    // 既に調査済みである
                    if (closeList[checkPos.x + checkPos.y * BOARD_HEIGHT].Saerch)
                    {
                        if (totalMoveCost >= closeList[checkPos.x + checkPos.y * BOARD_HEIGHT].Cost)
                            continue;

                        closeList[checkPos.x + checkPos.y * BOARD_HEIGHT].Saerch = false;
                        closeList[checkPos.x + checkPos.y * BOARD_HEIGHT].Cost = 10000;
                    }

                    /*
                    if (info.GetSameInfo(closeList, checkPos, out var close) == true)
                    {
                        // トータル移動コストが既存以上なら差し替え不要
                        if (totalMoveCost >= close.TotalMoveCost)
                            continue;

                        closeList.Remove(close);
                    }
                    */

                    // 現在調査中である
                    if (info.GetSameInfo(openList, checkPos, out var open) == true)
                    {
                        // トータル移動コストが既存以上なら差し替え不要
                        if (totalMoveCost >= open.TotalMoveCost)
                            continue;

                        openList.Remove(open);
                    }

                    Info neighborInfo = new Info(info, checkPos, totalMoveCost, goalPos);

                    if (neighborInfo.Pos.y == 3 || neighborInfo.Pos.y == 1) 
                        Debug.Log("アンチ");

                    openList.Add(neighborInfo);
                }
            }
    }
}
