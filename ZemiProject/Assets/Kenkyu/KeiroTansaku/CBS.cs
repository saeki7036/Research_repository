using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SocialPlatforms.Impl;
using static CBS;

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
        public Dictionary<int , List<Conflict>> Constraints; // ノード内のすべての衝突のリスト
        public Node Parent; // 親ノードへの参照
        public int Score;
        // コンストラクタ。初期パスと親ノードを受け取る
        public Node(Dictionary<int, List<Vector2Int>> paths, Dictionary<int, List<Conflict>> constraint, int score ,Node parent = null)
        {
            Paths = paths;
            Constraints = constraint;
            Parent = parent;
            Score = score;
        }
        public Node(Node node)
        {
            Paths = node.Paths; Constraints = node.Constraints; Parent = node; Score = node.Score;
        }
        public Node(){}
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

    public Dictionary<int, List<Vector2Int>> GetSolutionPaths(int menber, Dictionary<int, List<Conflict>> constriants)
    {
        var Paths = new Dictionary<int, List<Vector2Int>>();
        // 各エージェントの初期経路をA*で計算
        for (int agent = 0; agent < menber; agent++)
        {
            Paths[agent] = AStarWithConstraint(Stage_Index, agent, constriants);
            if (Paths[agent] == null)
            {
                Debug.LogWarning("エージェント:" + agent + "経路なし");
                return null; 
            }
        }
        return Paths;
    }

    public void Agents_Found(Node node, int menber)
    {
        AgentsNabi = new List<Vector2Int>[menber];
        Nabicount = new int[menber];
        // 衝突がない場合、解が見つかった
        for (int agent = 0; agent < menber; agent++)
        {
            Nabicount[agent] = 0;
            AgentsNabi[agent] = new List<Vector2Int>();
            AgentsNabi[agent] = node.Paths[agent];
            //Debug.Log("Agent:" + agent + "Count" + AgentsNabi[agent].Count);
        }
    }

    public Dictionary<int, List<Conflict>> GetNewConstraint(Node currentNode, int agent,int time)
    {
        var NewConstraint = new Dictionary<int, List<Conflict>>(currentNode.Constraints);

        Conflict newConflict = new Conflict(currentNode.Paths[agent][time], agent);

        if (!currentNode.Constraints.ContainsKey(time))
            NewConstraint.Add(time, new List<Conflict>());
        else
            NewConstraint[time] = new List<Conflict>(currentNode.Constraints[time]);

        NewConstraint[time].Add(newConflict);
        //Debug.Log(NewConstraint == currentNode.Constraints);
        return new Dictionary<int, List<Conflict>>(NewConstraint);
    }

    public void Debug_Constraints(Dictionary<int, List<Conflict>> constraint)
    {
        List<int> keys = new List<int>(constraint.Keys);
        int count = 0;
        foreach (int key in keys)
            count += constraint[key].Count;
        Debug.Log("コンフリクト数："+ count);  
        
    }

    List<int> CoustraintLog(List<Node> open)
    {
        List<int> list = new List<int>();
        for(int i = 0; i <open.Count;i++)
        {
            int value = 0;
            List<int> keys = new List<int>(open[i].Constraints.Keys);
            foreach (int key in keys)
                value += open[i].Constraints[key].Count;
            list.Add(value);
        }
            return list;
    }

    bool SameCheck(List<int> a, List<int> b)
    {
        for (int i = 0; i < a.Count; i++)
        {
            if (a[i] != b[i])return false;
        }
        return true;
    }



    public void Agents_Saerch()
    {
        var openList = new List<Node>(); // オープンリスト。探索中のノードを管理
        int AgentsMember = mapData.preset[Stage_Index].Agents.Length;
        var initialPaths = GetSolutionPaths(AgentsMember, null); // 初期経路
        if (initialPaths == null) return;//経路が存在できないなら返す

        // 初期ノードを作成
        var root = new Node(initialPaths, new Dictionary<int, List<Conflict>>(), GetScore(initialPaths , AgentsMember)); 
        openList.Add(root); // オープンリストに初期ノードを追加

        //Debug//
        int c = 0;
        //int before1 = -1, before2= -1, beforeT = -1;
        //Debug//

        // オープンリストが空になるまで探索を続ける
        while (openList.Count > 0)
        {
            Node currentNode = GetNodeLowest(openList); // 現在のノード
            //var conflict = FindFirstConflict(currentNode.Paths); // 衝突を検出までパス内を検索
            var conflict = FindConflictList(currentNode.Paths);
            if (conflict == null)// 衝突がない場合
            {
                Agents_Found(currentNode, AgentsMember); return;
            }
            else if(conflict == (null, -1))
            //else if (conflict == (-1,-1,-1))// 探索できていない経路が取得された場合
                continue;

            //var (agent1, agent2, time) = conflict.Value;
            var (menbersList, time) = conflict.Value;

            //Debug//
            //bool SameBefore = !(agent1 == before1 && agent2 == before2 && time == beforeT);
            //List<int> Before = new List<int>();
            //List<int> After = new List<int>();
            //Debug//

            // 衝突を解決するために新しい制約を追加し、ノードを生成
            if (true)
            {             
                foreach (int agent in menbersList)
                {
                    Node newNode = new Node();
                    newNode.Parent = currentNode;
                    newNode.Constraints = new Dictionary<int, List<Conflict>>(GetNewConstraint(currentNode, agent, time));
                    newNode.Paths = GetSolutionPaths(AgentsMember, newNode.Constraints);
                    newNode.Score = GetScore(newNode.Paths, AgentsMember);
                    openList.Add(newNode);
                    //Debug_Constraints(newNode.Constraints);
                    //Before = CoustraintLog(openList);
                    //After = CoustraintLog(openList);
                    //Debug.Log(SameCheck(Before, After));
                }
                /*
                var newConstraint1 = new Dictionary<int, List<Conflict>>(currentNode.Constraints);
                var newPaths1 = new Dictionary<int, List<Vector2Int>>(currentNode.Paths);
                var newConflict1 = new Conflict(newPaths1[agent1][time], agent1);
                //この場所にいなければいけない制約と、この場所にいてはいけない制約を両立する。

                if (!newConstraint1.ContainsKey(time))
                    newConstraint1.Add(time, new List<Conflict>());
                newConstraint1[time].Add(newConflict1);
                newPaths1[agent1] = AStarWithConstraint(Stage_Index, agent1, newConstraint1);
                Node newNode1 = new Node(newPaths1, newConstraint1, GetScore(newPaths1, AgentsMember), currentNode);
                //ノードの生成する場所の長さが長い
                //newNode1.Conflicts.AddRange(current.Conflicts);
                //newNode1.Conflicts.Add((agent1, agent2, time));
                openList.Add(newNode1);
                var newConstraint2 = new Dictionary<int, List<Conflict>>(currentNode.Constraints);
                var newPaths2 = new Dictionary<int, List<Vector2Int>>(currentNode.Paths);
                var newConflict2 = new Conflict(newPaths2[agent2][time], agent2);
                if (!newConstraint2.ContainsKey(time))
                    newConstraint2[time] = new List<Conflict>();
                newConstraint2[time].Add(newConflict2);
                newPaths2[agent2] = AStarWithConstraint(Stage_Index, agent2, newConstraint2);
                var newNode2 = new Node(newPaths2, newConstraint2, GetScore(newPaths2, AgentsMember), currentNode);               
                //newNode2.Conflicts.AddRange(current.Conflicts);
                //newNode2.Conflicts.Add((agent1, agent2, time));
                openList.Add(newNode2);
              */
            }
            //before1 = agent1; before2 = agent2; beforeT = time;
            c++;
            if (c == 500)
            {
                Debug.Log("");
            }
            if (c == 1000)
            {
                Debug.Log("探索失敗＼（＾o＾）／チョウカシティ");
                return;
            }              
        }
        // 解が見つからなかった場合nullを返す
        Debug.Log("探索失敗＼（＾o＾）／ カイナシティ");
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

    private Node GetNodeLowest(List<Node> nodelist)
    {
        if (nodelist.Count == 1)
        {
            Node lastNode = new Node(nodelist[0]);
            nodelist.RemoveAt(0);  // オープンリストから削除
            return lastNode;
        }
      
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

        Node returnNode = new Node(nodelist[number]);
        nodelist.RemoveAt(number);  // オープンリストから削除
        return returnNode;
    }

    private (List<int>, int)? FindConflictList(Dictionary<int, List<Vector2Int>> paths)
    {
        foreach (var agent1 in paths.Keys)
        {
            if (paths[agent1] == null)
            {

                return (null, -1);
            }
                
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
                        List<int> menberslist = new List<int>();
                        Vector2Int Pos = paths[agent1][t];
                        for (int i = 0; i < paths.Count; i++)
                        {

                            if (paths[i].Count <= t)
                                continue;
                            if (Pos == paths[i][t])
                                menberslist.Add(i);
                        }
                        //return (menberslist, t);
                        return (menberslist, t);
                    }
                    else if (t + 1 < paths[agent2].Count &&t + 1 < paths[agent1].Count)
                    {
                        int next = t + 1;
                        if (paths[agent1][t] == paths[agent2][next] && paths[agent1][next] == paths[agent2][t])
                        {
                            Debug.Log("agentA:" + agent1 + " agentB:" + agent2 + " Time:" + next +
                                " A_PosX:" + paths[agent1][next].x + " A_PosY" + paths[agent1][next].y +
                                " B_PosX:" + paths[agent2][next].x + " B_PosY" + paths[agent2][next].y);

                            List<int> menberslist = new List<int>();
                            menberslist.Add(agent1);
                            menberslist.Add(agent2);
                            // 衝突が見つかった場合、その情報を返す
                            return (menberslist, next);
                        }
                    }
                }
            }
        }
        // 衝突が見つからなかった場合nullを返す
        return null;
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
                        List<int> menberslist = new List<int>();
                        Vector2Int Pos = paths[agent1][t];
                        for (int i = 0; i < paths.Count; i++)
                        {

                            if (paths[i].Count <= t)
                                continue;
                            if(Pos == paths[i][t])                         
                                menberslist.Add(i);              
                        }
                        //return (menberslist, t);
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

    enum CloseSaerch
    {
        Unconfirmed,
        ReEntry,
        Visited,
    }


    private List<Vector2Int> SaerchWithConstraint(int stege, int menber, Dictionary<int, List<Conflict>> conflicts)
    {
        Vector2Int GoalPos = mapData.GetGoalPos(stege, menber);
        Vector2Int StartPos = mapData.GetStartPos(stege, menber);

        Info StartInfo = new Info(null, StartPos, 0, GoalPos);

        List<Info> OpenList = new List<Info>();

        CloseSaerch[] CloseSaerched = new CloseSaerch[BOARD_WIDTH * BOARD_HEIGHT];
        for(int i = 0; i < BOARD_WIDTH * BOARD_HEIGHT; i++)
            CloseSaerched[i] = CloseSaerch.Unconfirmed;

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

            if (CloseSaerched[useInfo.Pos.x + useInfo.Pos.y * BOARD_HEIGHT] == CloseSaerch.ReEntry)
            {

            }
            else if (CloseSaerched[useInfo.Pos.x + useInfo.Pos.y * BOARD_HEIGHT] == CloseSaerch.Unconfirmed)
            {
                CloseSaerched[useInfo.Pos.x + useInfo.Pos.y * BOARD_HEIGHT] = CloseSaerch.Visited;
            }
            
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

                //場外除外
                bool OverCheck_x = (checkPos.x >= 0 && checkPos.x < BOARD_WIDTH);
                bool OverCheck_y = (checkPos.y >= 0 && checkPos.y < BOARD_HEIGHT);
                if (!(OverCheck_x && OverCheck_y))
                    continue;

                //壁除外
                Map_Object movePos = mapData.GetMapData(stege, checkPos.y, checkPos.x);
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

                        }*/
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
                */

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
    }
}
