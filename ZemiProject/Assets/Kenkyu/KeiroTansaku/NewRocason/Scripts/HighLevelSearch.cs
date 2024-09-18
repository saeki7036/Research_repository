using System.Collections.Generic;
using UnityEngine;

public class HighLevelSearch
{
    const int BOARD_WIDTH = 30;//x
    const int BOARD_HEIGHT = 20;//y

    (int, int)[] Move = { (0, 1), (0, -1), (1, 0), (-1, 0) };//x,y

    // ノードクラス。各ノードはエージェントのパスとそのパスでの衝突情報を持つ
    public class Node
    {
        public List<Vector2Int>[] Paths; // 各エージェントのパスを格納
        public Dictionary<int, List<Conflict>> Constraints; // ノード内のすべての衝突のリスト||time,Conflict
        public Node Parent; // 親ノードへの参照
        public int Score;
        // コンストラクタ。初期パスと親ノードを受け取る
        public Node(List<Vector2Int>[] paths, Dictionary<int, List<Conflict>> constraint, int score, Node parent = null)
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
        public Node() { }

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
    public class HighLevelPath
    {

        public struct FindConflict
        {
            public bool Found;
            public Vector2Int Pos;
            public int Time;
            public List<int> Menber;
        }

        FindConflict FindConflictList(List<Vector2Int>[] paths, Vector2Int[] goalPos)
        {
            FindConflict findConflict = new FindConflict();

            int time = 0;
            int moveMenber = paths.Length;
            while (moveMenber > 0)
            {
                HashSet<Vector2Int> pos = new();
                for (int menber = 0; menber < paths.Length; menber++)
                {
                    int settime = time;
                    if (paths[menber].Count <= time)
                    {
                        if (paths[menber].Count == time)
                            moveMenber--;
                        settime = paths[menber].Count - 1;
                    }
                    // コンフリクト検出
                    if (pos.Contains(paths[menber][settime]))
                    {
                        findConflict.Found = false;
                        findConflict.Pos = paths[menber][settime];
                        findConflict.Time = settime;
                        findConflict.Menber = new();

                        // 衝突したエージェントを検出
                        for (int other = 0; other < paths.Length; other++)
                        {
                            if (paths[other].Count > settime && findConflict.Pos == paths[other][settime])
                            {
                                findConflict.Menber.Add(other);
                            }
                        }
                        return findConflict;
                    }
                   
                    if (goalPos[menber] != paths[menber][settime])
                        pos.Add(paths[menber][settime]);
                }
                time++;
            }
            findConflict.Found = true;
            return findConflict;
        }

        bool NullPathCheck(List<Vector2Int>[] list)
        {
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] == null)
                    return true;
            }
            return false;
        }

        public List<Vector2Int>[] GetPath(MapDataObject.Preset data, List<Vector2Int>[] farstPath, Vector2Int[] goalPos)
        {
            SolutionSearch solutionSearch = new SolutionSearch();

            var openList = new List<Node>(); // オープンリスト。探索中のノードを管理
            var root = new Node(farstPath, new Dictionary<int, List<Conflict>>(), CalcScore(farstPath));// 初期ノード
            openList.Add(root); // オープンリストに初期ノードを追加

            int c = 0;

            while (openList.Count > 0)// オープンリストが空になるまで探索を続ける
            {
                Node currentNode = GetNodeLowest(openList); // 現在のノード取得、リスト内削除
                if (NullPathCheck(currentNode.Paths)) continue;
                FindConflict conflict = FindConflictList(currentNode.Paths, goalPos);// 衝突を検出までパス内を検索
                if (conflict.Found) // 衝突がない場合
                {
                    return currentNode.Paths;
                }

                // 衝突を解決するために新しい制約を追加し、ノードを生成
                foreach (int agent in conflict.Menber)
                {
                    Node newNode = new Node();
                    newNode.Parent = currentNode;
                    newNode.Constraints = new Dictionary<int, List<Conflict>>(GetNewConstraint(currentNode, agent, conflict.Time));
                    newNode.Paths = solutionSearch.GetSolutionPaths(data,goalPos, newNode.Constraints);
                    newNode.Score = CalcScore(newNode.Paths);
                    openList.Add(newNode);
                }

                //この場所にいなければいけない制約と、この場所にいてはいけない制約を両立する。                  
                //ノードの生成する場所の長さが長い
                c++;
                if (c > 100) break;
            }

            return null;
        }

        public int CalcScore(List<Vector2Int>[] paths)
        {
            int Maxscore = 0;
            for (int agent = 0; agent < paths.Length; agent++)
            {
                if (paths[agent] == null)
                    return 999999999;
                Maxscore += paths[agent].Count;
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
        private Dictionary<int, List<Conflict>> GetNewConstraint(Node currentNode, int agent, int time)
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
    }
}
