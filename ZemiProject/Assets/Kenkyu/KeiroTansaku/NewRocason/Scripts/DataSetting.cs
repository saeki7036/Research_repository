using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DataSetting;
using static UnityEditor.PlayerSettings;
using static UnityEditor.Rendering.CameraUI;

public class DataSetting : MonoBehaviour
{
    const int BOARD_WIDTH = 30;
    const int BOARD_HEIGHT = 20;

    [SerializeField]
    MapDataObject mapData;

    [SerializeField]
    GameObject _Wall, _Spase;

    [SerializeField]
    public int Stage_Index = 0;

    [System.NonSerialized]
    public GameObject[] Agents;

    public struct AgentsPath
    {
        public int count;
        public List<Vector2Int> path;
    }

    public AgentsPath[] agentsPaths;



    // Start is called before the first frame update
    void Start()
    {
        SetObj();
    }

    public void SetObj()
    {
        Agents = new GameObject[mapData.preset[Stage_Index].agents.Length];
        for (int X = 0; X < BOARD_WIDTH; X++)
        {
            for (int Y = BOARD_HEIGHT - 1; Y >= 0; Y--)
            {
                Map_Object map_Object = mapData.preset[Stage_Index].height[Y].width[X];
                switch (map_Object)
                {
                    case Map_Object.Wall:
                        Instantiate(_Wall, new Vector3(X, 0, Y), Quaternion.identity);
                        break;
                    case Map_Object.Spase:
                        Instantiate(_Spase, new Vector3(X, 0, Y), Quaternion.identity);
                        break;
                    case Map_Object.Agent:
                        for (int i = 0; i < mapData.preset[Stage_Index].agents.Length; i++)
                        {
                            if (X == mapData.preset[Stage_Index].agents[i].startPos.x && Y == mapData.preset[Stage_Index].agents[i].startPos.y)
                            {
                                Agents[i] = Instantiate(mapData.preset[Stage_Index].agents[i].agentObj, new Vector3(X, 0, Y), Quaternion.identity);
                                Instantiate(_Spase, new Vector3(X, 0, Y), Quaternion.identity);
                                break;
                            }
                        }
                        break;
                    case Map_Object.Goal:
                        for (int i = 0; i < mapData.preset[Stage_Index].goal.Length; i++)
                        {
                            if (X == mapData.preset[Stage_Index].goal[i].goalPos.x && Y == mapData.preset[Stage_Index].goal[i].goalPos.y)
                            {
                                Instantiate(mapData.preset[Stage_Index].goal[i].goalObj, new Vector3(X, 0, Y), Quaternion.identity);
                                Instantiate(_Spase, new Vector3(X, 0, Y), Quaternion.identity);
                                break;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }


    public void ResetPos()
    {
        MapDataObject.Preset preset = mapData.preset[Stage_Index];

        for (int i = 0; i < preset.agents.Length; i++)
        {
            Agents[i].transform.position = new Vector3
                (preset.GetStartPos(i).x,0, preset.GetStartPos(i).y);
            if(agentsPaths != null)
            agentsPaths[i].count = 0;
        }
    }

    public void AgentsMove()
    {
        if (agentsPaths != null)
            for (int i = 0; i < agentsPaths.Length; i++)
            {
                if (agentsPaths[i].count < agentsPaths[i].path.Count)
                {
                    Agents[i].transform.position = new Vector3
                    (agentsPaths[i].path[agentsPaths[i].count].x, 0, agentsPaths[i].path[agentsPaths[i].count].y);
                    agentsPaths[i].count++;
                }
            }
        else
            Debug.LogError("まずは探索しろ！！！！！！！！！！！！！！！！！！！");
    }

    public void DataGetCharnge() 
    {      
        AgentsPathSearch search = new AgentsPathSearch();
        var Paths = search.GetPath(mapData.preset[Stage_Index]);

        if (Paths == null)
        {
            Debug.LogError("失敗、、、探索失敗、、、！");
            return;
        }
          
        Debug.Log("探索成功");

        agentsPaths = new AgentsPath[Agents.Length];
        for (int i = 0; i < agentsPaths.Length; i++)
        {
            agentsPaths[i].path = new(Paths[i]);
            agentsPaths[i].count = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
