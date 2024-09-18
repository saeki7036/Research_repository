using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
public class AgentsPathSearch
{
    public List<Vector2Int>[] ? GetPath(MapDataObject.Preset data)
    {
        if (data == null) return null;

        LowLevelSearch.LowLevelPath LowLevel = new();
        HighLevelSearch.HighLevelPath HighLevel = new();

        var goalandPaths = LowLevel.GetPathAndGoal(data); // èâä˙åoòH
        if (goalandPaths == null) return null;

        int agentsMenber = data.GetAgentMenber();

        Vector2Int[] goalPos = new Vector2Int[agentsMenber];
        List<Vector2Int>[] farstPaths = new List<Vector2Int>[agentsMenber];
        for (int i = 0; i < agentsMenber; i++)
        {
            goalPos[i] = goalandPaths.agentData[i].GoalPos;
            farstPaths[i] = new List<Vector2Int>(goalandPaths.agentData[i].Path);
        }

        List<Vector2Int>[] HighLevelPaths = HighLevel.GetPath(data, farstPaths, goalPos);

        if (HighLevelPaths == null) return null;
        else return HighLevelPaths;
    }
}
