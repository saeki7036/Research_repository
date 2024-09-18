using System.Collections.Generic;
using UnityEngine;

public class HighLevelSearch
{
    const int BOARD_WIDTH = 30;//x
    const int BOARD_HEIGHT = 20;//y

    (int, int)[] Move = { (0, 1), (0, -1), (1, 0), (-1, 0) };//x,y

    // �m�[�h�N���X�B�e�m�[�h�̓G�[�W�F���g�̃p�X�Ƃ��̃p�X�ł̏Փˏ�������
    public class Node
    {
        public List<Vector2Int>[] Paths; // �e�G�[�W�F���g�̃p�X���i�[
        public Dictionary<int, List<Conflict>> Constraints; // �m�[�h���̂��ׂĂ̏Փ˂̃��X�g||time,Conflict
        public Node Parent; // �e�m�[�h�ւ̎Q��
        public int Score;
        // �R���X�g���N�^�B�����p�X�Ɛe�m�[�h���󂯎��
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
        public Vector2Int Pos; // �Փ˂����ꏊ
        public int Agent; // �Փ˂�������������
        // �R���X�g���N�^�B�Փ˂̏ڍׂ��󂯎��
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
                    // �R���t���N�g���o
                    if (pos.Contains(paths[menber][settime]))
                    {
                        findConflict.Found = false;
                        findConflict.Pos = paths[menber][settime];
                        findConflict.Time = settime;
                        findConflict.Menber = new();

                        // �Փ˂����G�[�W�F���g�����o
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

            var openList = new List<Node>(); // �I�[�v�����X�g�B�T�����̃m�[�h���Ǘ�
            var root = new Node(farstPath, new Dictionary<int, List<Conflict>>(), CalcScore(farstPath));// �����m�[�h
            openList.Add(root); // �I�[�v�����X�g�ɏ����m�[�h��ǉ�

            int c = 0;

            while (openList.Count > 0)// �I�[�v�����X�g����ɂȂ�܂ŒT���𑱂���
            {
                Node currentNode = GetNodeLowest(openList); // ���݂̃m�[�h�擾�A���X�g���폜
                if (NullPathCheck(currentNode.Paths)) continue;
                FindConflict conflict = FindConflictList(currentNode.Paths, goalPos);// �Փ˂����o�܂Ńp�X��������
                if (conflict.Found) // �Փ˂��Ȃ��ꍇ
                {
                    return currentNode.Paths;
                }

                // �Փ˂��������邽�߂ɐV���������ǉ����A�m�[�h�𐶐�
                foreach (int agent in conflict.Menber)
                {
                    Node newNode = new Node();
                    newNode.Parent = currentNode;
                    newNode.Constraints = new Dictionary<int, List<Conflict>>(GetNewConstraint(currentNode, agent, conflict.Time));
                    newNode.Paths = solutionSearch.GetSolutionPaths(data,goalPos, newNode.Constraints);
                    newNode.Score = CalcScore(newNode.Paths);
                    openList.Add(newNode);
                }

                //���̏ꏊ�ɂ��Ȃ���΂����Ȃ�����ƁA���̏ꏊ�ɂ��Ă͂����Ȃ�����𗼗�����B                  
                //�m�[�h�̐�������ꏊ�̒���������
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
                nodelist.RemoveAt(0);  // �I�[�v�����X�g����폜
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
            nodelist.RemoveAt(number);  // �I�[�v�����X�g����폜
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
