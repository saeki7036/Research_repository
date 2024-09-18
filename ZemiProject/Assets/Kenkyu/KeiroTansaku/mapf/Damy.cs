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
        output = this.gameObject.GetComponent<OutPut>();//�Ֆʂ̎擾
        Stage_Index = output.Stage_Index;//�C���f�b�N�X�擾
        agents = new Agent[mapData.preset[Stage_Index].Agents.Length];//�l�̐��擾
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

                //�΂ߏ��O
                if (X + Y != 1 && X + Y != -1)
                    continue;

                Vector2Int checkPos = new Vector2Int(info.Pos.x + X, info.Pos.y + Y);

                bool OverCheck_x = (checkPos.x >= 0 && checkPos.x < BOARD_WIDTH);
                bool OverCheck_y = (checkPos.y >= 0 && checkPos.y < BOARD_HEIGHT);

                if (OverCheck_x && OverCheck_y)
                {
                    Map_Object movePos = mapData.GetMapData(stege, checkPos.y, checkPos.x);

                    //�Ǐ��O
                    if (movePos == Map_Object.Wall)
                        continue;
                    //���G�[�W�F���g���O
                    //if (movePos == Map_Object.Agent)
                    //   continue;

                    float totalMoveCost = info.TotalMoveCost + 1.0f;

                    // ���ɒ����ς݂ł���
                    if (info.GetSameInfo(closeList, checkPos, out var close) == true)
                    {
                        // �g�[�^���ړ��R�X�g�������ȏ�Ȃ獷���ւ��s�v
                        if (totalMoveCost >= close.TotalMoveCost)
                            continue;

                        closeList.Remove(close);
                    }

                    // ���ݒ������ł���
                    if (info.GetSameInfo(openList, checkPos, out var open) == true)
                    {
                        // �g�[�^���ړ��R�X�g�������ȏ�Ȃ獷���ւ��s�v
                        if (totalMoveCost >= open.TotalMoveCost)
                            continue;

                        openList.Remove(open);
                    }

                    Info neighborInfo = new Info(info, checkPos, totalMoveCost, goalPos);
                    openList.Add(neighborInfo);
                }
            }
    }



    // �m�[�h�N���X�B�e�m�[�h�̓G�[�W�F���g�̃p�X�Ƃ��̃p�X�ł̏Փˏ�������
    public class Node
    {
        public Dictionary<int, List<(int, int)>> Paths; // �e�G�[�W�F���g�̃p�X���i�[
        public List<(int agent1, int agent2, int time)> Conflicts; // �m�[�h���̂��ׂĂ̏Փ˂̃��X�g
        public Node Parent; // �e�m�[�h�ւ̎Q��

        // �R���X�g���N�^�B�����p�X�Ɛe�m�[�h���󂯎��
        public Node(Dictionary<int, List<(int, int)>> paths, Node parent = null)
        {
            Paths = paths;
            Conflicts = new List<(int agent1, int agent2, int time)>();
            Parent = parent;
        }
    }

    // �Փ˃N���X�B�G�[�W�F���g�Ԃ̏Փ˂�\��
    public class Conflict
    {
        public int Agent1; // �Փ˂����G�[�W�F���g1
        public int Agent2; // �Փ˂����G�[�W�F���g2
        public int Time; // �Փ˂�������������

        // �R���X�g���N�^�B�Փ˂̏ڍׂ��󂯎��
        public Conflict(int agent1, int agent2, int time)
        {
            Agent1 = agent1;
            Agent2 = agent2;
            Time = time;
        }
    }

    // ���C����CBS�A���S���Y���B���ׂẴG�[�W�F���g�̌o�H���v�Z����
    public Dictionary<int, List<(int, int)>> FindPaths(Dictionary<int, (int, int)> startPositions, Dictionary<int, (int, int)> goalPositions, int[,] grid)
    {
        var openList = new List<Node>(); // �I�[�v�����X�g�B�T�����̃m�[�h���Ǘ�
        var initialPaths = new Dictionary<int, List<(int, int)>>(); // �����o�H

        // �e�G�[�W�F���g�̏����o�H��A*�Ōv�Z
        foreach (var agent in startPositions.Keys)
        {
            initialPaths[agent] = AStar(startPositions[agent], goalPositions[agent], grid);
        }

        var root = new Node(initialPaths); // �����m�[�h���쐬
        openList.Add(root); // �I�[�v�����X�g�ɏ����m�[�h��ǉ�

        // �I�[�v�����X�g����ɂȂ�܂ŒT���𑱂���
        while (openList.Count > 0)
        {
            var current = openList[0]; // ���݂̃m�[�h
            openList.RemoveAt(0); // �I�[�v�����X�g����폜

            var conflict = FindFirstConflict(current.Paths); // �Փ˂����o
            if (conflict == null)
            {
                // �Փ˂��Ȃ��ꍇ�A������������
                return current.Paths;
            }

            var (agent1, agent2, time) = conflict.Value;

            // �Փ˂��������邽�߂ɐV���������ǉ����A�m�[�h�𐶐�
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

        // ����������Ȃ������ꍇnull��Ԃ�
        return null;
    }

    // A*�A���S���Y���B�w�肳�ꂽ�J�n�ʒu����ڕW�ʒu�܂ł̍ŒZ�o�H��Ԃ�
    private List<(int, int)> AStar((int, int) start, (int, int) goal, int[,] grid)
    {
        // A*�A���S���Y���̎���
        // TODO: ���̊֐��͎������K�v
        return new List<(int, int)>();
    }

    // ������l������A*�A���S���Y���B�w�肳�ꂽ��������o�H���v�Z
    private List<(int, int)> AStarWithConstraint((int, int) start, (int, int) goal, int[,] grid, (int agent, int time) constraint)
    {
        // �D��x�t���L���[���g�p���ăI�[�v�����X�g���Ǘ�
        var openList = new PriorityQueue<(int x, int y, int g, int h, int f, (int x, int y)?, int t)>();
        var closedList = new HashSet<(int x, int y, int t)>();
        var cameFrom = new Dictionary<(int x, int y), (int x, int y)?>();

        // �}���n�b�^���������g�p����h�l���v�Z
        int Heuristic((int x, int y) a, (int x, int y) b)
        {
            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
        }

        // �J�n�ʒu���I�[�v�����X�g�ɒǉ�
        openList.Enqueue((start.Item1, start.Item2, 0, Heuristic(start, goal), Heuristic(start, goal), null, 0));

        while (openList.Count > 0)
        {
            // �ŏ�f�l�̃m�[�h�����o��
            var current = openList.Dequeue();
            var (x, y, g, h, f, parent, t) = current;

            // �ڕW�ʒu�ɓ��B�����ꍇ�A�o�H���č\�z
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

            // �N���[�Y�h���X�g�ɒǉ�
            closedList.Add((x, y, t));

            // �ߖT�m�[�h��T��
            var neighbors = new (int dx, int dy)[] { (-1, 0), (1, 0), (0, -1), (0, 1) };
            foreach (var (dx, dy) in neighbors)
            {
                var neighbor = (x + dx, y + dy);
                var newT = t + 1;

                // �O���b�h�͈̔͊O�܂��͒ʍs�s�\�ȏꍇ�̓X�L�b�v
                if (neighbor.x < 0 || neighbor.y < 0 || neighbor.x >= grid.GetLength(0) || neighbor.y >= grid.GetLength(1) || grid[neighbor.x, neighbor.y] == 1)
                {
                    continue;
                }

                // ����𖞂����Ȃ��ꍇ�̓X�L�b�v
                if (constraint.agent == agent && newT == constraint.time && neighbor == start)
                {
                    continue;
                }

                // �N���[�Y�h���X�g�ɂ���ꍇ�̓X�L�b�v
                if (closedList.Contains((neighbor.x, neighbor.y, newT)))
                {
                    continue;
                }

                // g, h, f�l���v�Z
                var newG = g + 1;
                var newH = Heuristic(neighbor, goal);
                var newF = newG + newH;

                // �I�[�v�����X�g�ɒǉ�
                openList.Enqueue((neighbor.x, neighbor.y, newG, newH, newF, (x, y), newT));

                // �o�H�����L�^
                if (!cameFrom.ContainsKey(neighbor))
                {
                    cameFrom[neighbor] = (x, y);
                }
            }
        }

        // �o�H��������Ȃ������ꍇ�A��̃��X�g��Ԃ�
        return new List<(int, int)>();
    }


    // �G�[�W�F���g�̌o�H�Ԃōŏ��̏Փ˂�������
    private (int, int, int)? FindFirstConflict(Dictionary<int, List<(int, int)>> paths)
    {
        // �G�[�W�F���g�̌o�H���`�F�b�N���čŏ��̏Փ˂�T��
        foreach (var agent1 in paths.Keys)
        {
            for (int t = 0; t < paths[agent1].Count; t++)
            {
                foreach (var agent2 in paths.Keys)
                {
                    if (agent1 != agent2 && t < paths[agent2].Count && paths[agent1][t] == paths[agent2][t])
                    {
                        // �Փ˂����������ꍇ�A���̏���Ԃ�
                        return (agent1, agent2, t);
                    }
                }
            }
        }
        // �Փ˂�������Ȃ������ꍇnull��Ԃ�
        return null;
    }*/

}
