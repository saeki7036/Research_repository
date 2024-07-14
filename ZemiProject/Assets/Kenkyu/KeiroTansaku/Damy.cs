using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damy : MonoBehaviour
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
        output = this.gameObject.GetComponent<OutPut>();//盤面の取得
        Stage_Index = output.Stage_Index;//インデックス取得
        agents = new Agent[mapData.preset[Stage_Index].Agents.Length];//人の数取得
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
    /* public void Agents_Move()
    {
        if (agents[0] != null)
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
        for (int i = 0; i < mapData.preset[Stage_Index].Agents.Length; i++)
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

                agents[i].Nabi[agents[i].NabiCount - Nabigate.Count] = useInfo.Pos;

                Nabigate.Remove(useInfo);
            }
            agents[i].NabiCount = 0;
        }
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
                    //   continue;

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



    // ノードクラス。各ノードはエージェントのパスとそのパスでの衝突情報を持つ
    public class Node
    {
        public Dictionary<int, List<(int, int)>> Paths; // 各エージェントのパスを格納
        public List<(int agent1, int agent2, int time)> Conflicts; // ノード内のすべての衝突のリスト
        public Node Parent; // 親ノードへの参照

        // コンストラクタ。初期パスと親ノードを受け取る
        public Node(Dictionary<int, List<(int, int)>> paths, Node parent = null)
        {
            Paths = paths;
            Conflicts = new List<(int agent1, int agent2, int time)>();
            Parent = parent;
        }
    }

    // 衝突クラス。エージェント間の衝突を表す
    public class Conflict
    {
        public int Agent1; // 衝突したエージェント1
        public int Agent2; // 衝突したエージェント2
        public int Time; // 衝突が発生した時刻

        // コンストラクタ。衝突の詳細を受け取る
        public Conflict(int agent1, int agent2, int time)
        {
            Agent1 = agent1;
            Agent2 = agent2;
            Time = time;
        }
    }

    // メインのCBSアルゴリズム。すべてのエージェントの経路を計算する
    public Dictionary<int, List<(int, int)>> FindPaths(Dictionary<int, (int, int)> startPositions, Dictionary<int, (int, int)> goalPositions, int[,] grid)
    {
        var openList = new List<Node>(); // オープンリスト。探索中のノードを管理
        var initialPaths = new Dictionary<int, List<(int, int)>>(); // 初期経路

        // 各エージェントの初期経路をA*で計算
        foreach (var agent in startPositions.Keys)
        {
            initialPaths[agent] = AStar(startPositions[agent], goalPositions[agent], grid);
        }

        var root = new Node(initialPaths); // 初期ノードを作成
        openList.Add(root); // オープンリストに初期ノードを追加

        // オープンリストが空になるまで探索を続ける
        while (openList.Count > 0)
        {
            var current = openList[0]; // 現在のノード
            openList.RemoveAt(0); // オープンリストから削除

            var conflict = FindFirstConflict(current.Paths); // 衝突を検出
            if (conflict == null)
            {
                // 衝突がない場合、解が見つかった
                return current.Paths;
            }

            var (agent1, agent2, time) = conflict.Value;

            // 衝突を解決するために新しい制約を追加し、ノードを生成
            var newPaths1 = new Dictionary<int, List<(int, int)>>(current.Paths);
            newPaths1[agent1] = AStarWithConstraint(startPositions[agent1], goalPositions[agent1], grid, (agent1, time));

            var newNode1 = new Node(newPaths1, current);
            newNode1.Conflicts.AddRange(current.Conflicts);
            newNode1.Conflicts.Add((agent1, agent2, time));
            openList.Add(newNode1);

            var newPaths2 = new Dictionary<int, List<(int, int)>>(current.Paths);
            newPaths2[agent2] = AStarWithConstraint(startPositions[agent2], goalPositions[agent2], grid, (agent2, time));

            var newNode2 = new Node(newPaths2, current);
            newNode2.Conflicts.AddRange(current.Conflicts);
            newNode2.Conflicts.Add((agent1, agent2, time));
            openList.Add(newNode2);
        }

        // 解が見つからなかった場合nullを返す
        return null;
    }

    // A*アルゴリズム。指定された開始位置から目標位置までの最短経路を返す
    private List<(int, int)> AStar((int, int) start, (int, int) goal, int[,] grid)
    {
        // A*アルゴリズムの実装
        // TODO: この関数は実装が必要
        return new List<(int, int)>();
    }

    // 制約を考慮したA*アルゴリズム。指定された制約を守りつつ経路を計算
    private List<(int, int)> AStarWithConstraint((int, int) start, (int, int) goal, int[,] grid, (int agent, int time) constraint)
    {
        // 優先度付きキューを使用してオープンリストを管理
        var openList = new PriorityQueue<(int x, int y, int g, int h, int f, (int x, int y)?, int t)>();
        var closedList = new HashSet<(int x, int y, int t)>();
        var cameFrom = new Dictionary<(int x, int y), (int x, int y)?>();

        // マンハッタン距離を使用してh値を計算
        int Heuristic((int x, int y) a, (int x, int y) b)
        {
            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
        }

        // 開始位置をオープンリストに追加
        openList.Enqueue((start.Item1, start.Item2, 0, Heuristic(start, goal), Heuristic(start, goal), null, 0));

        while (openList.Count > 0)
        {
            // 最小f値のノードを取り出す
            var current = openList.Dequeue();
            var (x, y, g, h, f, parent, t) = current;

            // 目標位置に到達した場合、経路を再構築
            if ((x, y) == goal)
            {
                var path = new List<(int, int)>();
                var node = (x, y);
                while (node != null)
                {
                    path.Add(node);
                    node = cameFrom[node];
                }
                path.Reverse();
                return path;
            }

            // クローズドリストに追加
            closedList.Add((x, y, t));

            // 近傍ノードを探索
            var neighbors = new (int dx, int dy)[] { (-1, 0), (1, 0), (0, -1), (0, 1) };
            foreach (var (dx, dy) in neighbors)
            {
                var neighbor = (x + dx, y + dy);
                var newT = t + 1;

                // グリッドの範囲外または通行不可能な場合はスキップ
                if (neighbor.x < 0 || neighbor.y < 0 || neighbor.x >= grid.GetLength(0) || neighbor.y >= grid.GetLength(1) || grid[neighbor.x, neighbor.y] == 1)
                {
                    continue;
                }

                // 制約を満たさない場合はスキップ
                if (constraint.agent == agent && newT == constraint.time && neighbor == start)
                {
                    continue;
                }

                // クローズドリストにある場合はスキップ
                if (closedList.Contains((neighbor.x, neighbor.y, newT)))
                {
                    continue;
                }

                // g, h, f値を計算
                var newG = g + 1;
                var newH = Heuristic(neighbor, goal);
                var newF = newG + newH;

                // オープンリストに追加
                openList.Enqueue((neighbor.x, neighbor.y, newG, newH, newF, (x, y), newT));

                // 経路情報を記録
                if (!cameFrom.ContainsKey(neighbor))
                {
                    cameFrom[neighbor] = (x, y);
                }
            }
        }

        // 経路が見つからなかった場合、空のリストを返す
        return new List<(int, int)>();
    }


    // エージェントの経路間で最初の衝突を見つける
    private (int, int, int)? FindFirstConflict(Dictionary<int, List<(int, int)>> paths)
    {
        // エージェントの経路をチェックして最初の衝突を探す
        foreach (var agent1 in paths.Keys)
        {
            for (int t = 0; t < paths[agent1].Count; t++)
            {
                foreach (var agent2 in paths.Keys)
                {
                    if (agent1 != agent2 && t < paths[agent2].Count && paths[agent1][t] == paths[agent2][t])
                    {
                        // 衝突が見つかった場合、その情報を返す
                        return (agent1, agent2, t);
                    }
                }
            }
        }
        // 衝突が見つからなかった場合nullを返す
        return null;
    }*/

}
