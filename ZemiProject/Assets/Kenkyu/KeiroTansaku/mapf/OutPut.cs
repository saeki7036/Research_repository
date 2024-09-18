using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class OutPut : MonoBehaviour
{
    const int BOARD_WIDTH = 30;
    const int BOARD_HEIGHT = 20;

    [SerializeField]
    MapDataBase mapData;

    [SerializeField]
    GameObject _Wall, _Spase;

    [SerializeField]
    public int Stage_Index = 0;

    [System.NonSerialized]
    public GameObject[] Agents; 
    // Start is called before the first frame update
    void Start()
    {
        Agents = new GameObject[mapData.preset[Stage_Index].Agents.Length];
        for (int X = 0; X < BOARD_WIDTH; X++)
        {
            for (int Y = BOARD_HEIGHT - 1; Y >= 0; Y--)
            {
                Map_Object map_Object = mapData.preset[Stage_Index].Height[Y].Width[X];
                switch (map_Object)
                {
                    case Map_Object.Wall:
                        Instantiate(_Wall, new Vector3(X, 0, Y), Quaternion.identity);
                        break;
                    case Map_Object.Spase:
                        Instantiate(_Spase, new Vector3(X, 0, Y), Quaternion.identity);
                        break;
                    case Map_Object.Agent:
                         for (int i = 0; i < mapData.preset[Stage_Index].Agents.Length; i++)
                        {
                            if(X == mapData.preset[Stage_Index].Agents[i].StartPos.x && Y == mapData.preset[Stage_Index].Agents[i].StartPos.y)
                            {
                                Agents[i] = Instantiate(mapData.preset[Stage_Index].Agents[i].agentObj, new Vector3(X, 0, Y), Quaternion.identity);
                                Instantiate(_Spase, new Vector3(X, 0, Y), Quaternion.identity);
                                break;
                            }  
                        }
                        break;
                    case Map_Object.Goal:
                        for (int i = 0; i < mapData.preset[Stage_Index].Agents.Length; i++)
                        {
                            if (X == mapData.preset[Stage_Index].Agents[i].GoalPos.x && Y == mapData.preset[Stage_Index].Agents[i].GoalPos.y)
                            {
                                Instantiate(mapData.preset[Stage_Index].Agents[i].goalObj, new Vector3(X, 0, Y), Quaternion.identity);
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

    public void ResetObj()
    {      
        foreach (Transform child in this.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    public void ResetPos()
    {
        for(int i = 0; i < mapData.preset[Stage_Index].Agents.Length; i++)
        {
            Agents[i].transform.position = new Vector3
                (mapData.preset[Stage_Index].Agents[i].StartPos.x,0,mapData.preset[Stage_Index].Agents[i].StartPos.y);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
