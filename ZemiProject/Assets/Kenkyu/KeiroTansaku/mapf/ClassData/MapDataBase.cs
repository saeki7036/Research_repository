
using UnityEngine;

public enum Map_Object
{
    Wall = 0,
    Spase = 1,
    Agent= 2,
    Goal= 3,
}
[CreateAssetMenu(fileName = "MapDataBase", menuName = "OriginalScriptableObjects/MapDataBase")]

[System.Serializable]

public class MapDataBase : ScriptableObject
{
    public Preset[] preset;

    [System.Serializable]
    public class Preset
    {
        //public Map_Object[][] presetData;
        public Agent[] Agents;
        public HEIGHT[] Height;

        [System.Serializable]
        public class HEIGHT
        {
            public Map_Object[] Width;
        }

        [System.Serializable]
        public class Agent
        {
            public Vector2Int StartPos;
            public Vector2Int GoalPos;
            public Color MyColor;
            public GameObject agentObj;
            public GameObject goalObj;
            public GameObject symbol;
            //public Material material;
        }
    }
    public MapDataBase Clone()
    {
        return Instantiate(this);
    }

    public Map_Object GetMapData(int Pre_Num, int Y_Pos, int X_Pos) 
    {
        //Debug.Log(preset[Pre_Num].Height[Y_Pos].Width[X_Pos]);
        return preset[Pre_Num].Height[Y_Pos].Width[X_Pos];
    }

    public Vector2Int GetStartPos(int Pre_Num, int Point_Num) 
    { 
        return preset[Pre_Num].Agents[Point_Num].StartPos; 
    }

    public Vector2Int GetGoalPos(int Pre_Num, int Point_Num)
    {
        return preset[Pre_Num].Agents[Point_Num].GoalPos;
    }
}
