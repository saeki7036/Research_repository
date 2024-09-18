using UnityEngine;

[CreateAssetMenu(fileName = "MapDataObject", menuName = "OriginalScriptableObjects/MapDataObject")]

    public class MapDataObject : ScriptableObject
    {
        public Preset[] preset;

        [System.Serializable]
        public class Preset
        {
            //public Map_Object[][] presetData;
            public Agent[] agents;
            public HEIGHT[] height;
            public Goal[] goal;
            
            [System.Serializable]
            public class HEIGHT
            {
                public Map_Object[] width;
            }

            [System.Serializable]
            public class Agent
            {
                public Vector2Int startPos;
                public Color myColor;
                public GameObject agentObj;
                public GameObject symbol;
            }
            
            [System.Serializable]
            public class Goal
            {
                public Vector2Int goalPos;
                public Color myColor;
                public GameObject goalObj;
            }

        public int GetAgentMenber() => agents.Length;

        public Map_Object GetMapData(int Y_Pos, int X_Pos)
        {
            return height[Y_Pos].width[X_Pos];
        }

        public Vector2Int GetStartPos(int Point_Num)
        {
            return agents[Point_Num].startPos;
        }
    }
        
        public MapDataObject Clone()
        {
            return Instantiate(this);
        }
    }

