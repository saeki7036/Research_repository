using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static MapDataBase.Preset;
using static UnityEditor.PlayerSettings;
public class MapEditer : EditorWindow
{
    [MenuItem("Window/OriginalPanels/EditorWindow", priority = 0)]
    static public void CreateWindow()
    {
        EditorWindow.GetWindow<MapEditer>();
    }

    [SerializeField]
    private MapDataBase BaseData;

    [SerializeField]
    private string BaseDataPath;

    [SerializeField]
    private GUISkin _skin;
    private void OnEnable()
    {
        var defaultData  = AssetDatabase.LoadAssetAtPath<MapDataBase>("Assets/Kenkyu/KeiroTansaku/ClassData/MapDataBase.asset");
        //var defaultData = Resources.Load<MapDataBase>("MapDataBase");
        this.BaseData = defaultData.Clone();
        this.BaseDataPath = AssetDatabase.GetAssetPath(defaultData);
    }
   
    const int BOARD_WIDTH = 30;
    const int BOARD_HEIGHT = 20;
    private Vector2 scrollPosition;
    private Vector2Int LDpos, RUpos;
    private int Slider_Value_Map = 0;

    public enum Map_Object_bata
    {
        Wall = 0,
        Spase = 1,
       
    }
    private Map_Object_bata SerectObj = Map_Object_bata.Spase;
    //private int Slider_Value_Floor = 0;
    // Update is called once per frame
    void ColorGUI(Map_Object map_Object, Vector2Int pos)
    {
        switch (map_Object)
        {
            case Map_Object.Wall:
                GUI.backgroundColor = Color.black;
                break;
            case Map_Object.Spase:
                GUI.backgroundColor = Color.white;
                break;
            case Map_Object.Agent:
                for (int i = 0; i < this.BaseData.preset[Slider_Value_Map].Agents.Length; i++)
                {
                    bool _agent = (BaseData.preset[Slider_Value_Map].Height[pos.y].Width[pos.x] == Map_Object.Agent);
                    bool _posint = (BaseData.preset[Slider_Value_Map].Agents[i].StartPos == pos);
                    if (_agent && _posint)
                    {
                        GUI.backgroundColor = BaseData.preset[Slider_Value_Map].Agents[i].MyColor; 
                        break;
                    }
                    if(i +1 == this.BaseData.preset[Slider_Value_Map].Agents.Length)
                        GUI.backgroundColor = Color.gray;
                }
                break;
            case Map_Object.Goal:
                for (int i = 0; i < this.BaseData.preset[Slider_Value_Map].Agents.Length; i++)
                {
                    bool _agent = (BaseData.preset[Slider_Value_Map].Height[pos.y].Width[pos.x] == Map_Object.Goal);
                    bool _posint = (BaseData.preset[Slider_Value_Map].Agents[i].GoalPos == pos);
                    if (_agent && _posint)
                    {
                        GUI.backgroundColor = BaseData.preset[Slider_Value_Map].Agents[i].MyColor;
                        break;
                    }
                    if (i + 1 == this.BaseData.preset[Slider_Value_Map].Agents.Length)
                        GUI.backgroundColor = Color.gray;
                }
                break;
            default:
                GUI.backgroundColor = Color.gray;
                break;
        }
    }
    private void OnGUI()
    {
        using (new EditorGUILayout.VerticalScope())
        {
            //Debug.Log(BaseData);
            if(BaseData == null) this.BaseData = AssetDatabase.LoadAssetAtPath<MapDataBase>(this.BaseDataPath).Clone();

            Undo.RecordObject(BaseData, "Modify MapDataBase");
     
            using (new EditorGUILayout.HorizontalScope())
            {
                Slider_Value_Map = (int)EditorGUILayout.Slider("Map_ID", Slider_Value_Map, 0, this.BaseData.preset.Length - 1, GUILayout.MaxWidth(300f));
                EditorGUILayout.LabelField("||Map:", GUILayout.MaxWidth(40f));
               
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("|Floor:", GUILayout.MaxWidth(40f));
                if (GUILayout.Button("  マップを追加", GUILayout.MaxWidth(120f), GUILayout.MaxHeight(20f)))
                {
                    Undo.RecordObject(BaseData, "Add Map");
                    int Map_Length = this.BaseData.preset.Length;
                    System.Array.Resize(ref this.BaseData.preset, Map_Length + 1);
                    this.BaseData.preset[Map_Length] = new MapDataBase.Preset()
                    {
                        Agents = new MapDataBase.Preset.Agent[1],
                        Height =  new HEIGHT[BOARD_HEIGHT],
                    };
                    for(int _Y = 0; BOARD_HEIGHT >_Y; _Y++)
                    {
                        this.BaseData.preset[Map_Length].Height[_Y] = new MapDataBase.Preset.HEIGHT()
                        {
                            Width = new Map_Object[BOARD_WIDTH],
                        };
                        //System.Array.Resize(ref this.BaseData.preset[Map_Length].Height[_Y].Width, BOARD_WIDTH);
                        for (int _X = 0; BOARD_WIDTH > _X; _X++)
                            this.BaseData.preset[Map_Length].Height[_Y].Width[_X] = Map_Object.Wall;
                    }
                    //SetData(Map_Length);
                }
                
                if (GUILayout.Button("Agentを追加", GUILayout.MaxWidth(120f), GUILayout.MaxHeight(20f)))
                {
                    Undo.RecordObject(BaseData, "Add Agent");
                    int Point_Length = this.BaseData.preset[Slider_Value_Map].Agents.Length;
                    System.Array.Resize(ref this.BaseData.preset[Slider_Value_Map].Agents, Point_Length + 1);
                    this.BaseData.preset[Slider_Value_Map].Agents[Point_Length]  = new MapDataBase.Preset.Agent()
                    {
                        StartPos = new Vector2Int(0, 0),
                        GoalPos = new Vector2Int(0, 0),
                        MyColor = new Color(0,0,0,1f),
                    };
                }
                if (GUILayout.Button("元に戻す", GUILayout.MaxWidth(60f), GUILayout.MaxHeight(20f)))
                {
                    this.BaseData = AssetDatabase.LoadAssetAtPath<MapDataBase>(this.BaseDataPath).Clone();
                    EditorGUIUtility.editingTextField = false;
                }

                if (GUILayout.Button("保存", GUILayout.MaxWidth(60f), GUILayout.MaxHeight(20f)))
                {
                    bool Chack = true;
                    for(int i = 0; i < this.BaseData.preset[Slider_Value_Map].Agents.Length; i++)
                    {
                        Vector2Int S_pos = new(BaseData.preset[Slider_Value_Map].Agents[i].StartPos.x, BaseData.preset[Slider_Value_Map].Agents[i].StartPos.y);

                        Vector2Int G_pos = new(BaseData.preset[Slider_Value_Map].Agents[i].GoalPos.x, BaseData.preset[Slider_Value_Map].Agents[i].GoalPos.y);

                        bool C_start = false;
                        bool C_goal = false;

                        if (S_pos.x >= 0 && S_pos.x < BOARD_WIDTH && S_pos.y >= 0 && S_pos.y < BOARD_HEIGHT)
                        {
                            C_start = (BaseData.preset[Slider_Value_Map].Height[S_pos.y].Width[S_pos.x] == Map_Object.Agent);

                        }
                        if (G_pos.x >= 0 && G_pos.x < BOARD_WIDTH && G_pos.y >= 0 && G_pos.y < BOARD_HEIGHT)
                        {
                            C_goal = (BaseData.preset[Slider_Value_Map].Height[G_pos.y].Width[G_pos.x] == Map_Object.Goal);

                        }
                        if(!C_start || !C_goal)
                        {
                            Chack = false;
                            break;
                        }
                    }
                    if (Chack)
                    {
                        var data = AssetDatabase.LoadAssetAtPath<MapDataBase>(this.BaseDataPath);
                        EditorUtility.CopySerialized(this.BaseData, data);
                        EditorUtility.SetDirty(data);
                        AssetDatabase.SaveAssets();
                    }
                    else
                    {
                        Debug.Log("Error");
                        Debug.Assert(false);
                    }
                }
            }
        }

        using (new EditorGUILayout.HorizontalScope(GUILayout.MaxWidth(20f)))
        {
            if(0 < this.BaseData.preset.Length)
                for (int X = 0; X < BOARD_WIDTH; X++)
                {
                    using (new EditorGUILayout.VerticalScope())
                    {

                        for (int Y = BOARD_HEIGHT - 1; Y >= 0; Y--)
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                if (X == 0)
                                    EditorGUILayout.LabelField(Y.ToString(), GUILayout.MaxWidth(20f));
                                
                                Map_Object map_Object = BaseData.preset[Slider_Value_Map].Height[Y].Width[X];

                                ColorGUI(map_Object,new(X,Y));
                               
                                BaseData.preset[Slider_Value_Map].Height[Y].Width[X] =
                                (Map_Object)EditorGUILayout.EnumPopup(BaseData.preset[Slider_Value_Map].Height[Y].Width[X],
                                GUILayout.MaxWidth(20f), GUILayout.MaxHeight(20f));

                                GUI.backgroundColor = Color.white;
                            }
                        }
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (X == 0)
                                EditorGUILayout.LabelField("Y/X", GUILayout.MaxWidth(20f));
                            EditorGUILayout.LabelField(X.ToString(), GUILayout.MaxWidth(20f));
                        }
                    }
                }




            using (new EditorGUILayout.HorizontalScope(GUILayout.MaxWidth(300f)))
            {
                using (new EditorGUILayout.VerticalScope(GUILayout.MaxWidth(300f)))
                {
                    EditorGUILayout.LabelField("    範囲設置", GUILayout.MinWidth(180f));
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (new EditorGUILayout.VerticalScope())
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                Map_Object Set = (Map_Object)SerectObj;
                                ColorGUI(Set, new(-1, -1));
                                EditorGUILayout.LabelField("|", GUILayout.MaxWidth(10f));
                                SerectObj = (Map_Object_bata)EditorGUILayout.EnumPopup(SerectObj, GUILayout.MaxWidth(20f), GUILayout.MaxHeight(20f));
                                EditorGUILayout.LabelField("|", GUILayout.MaxWidth(10f));
                                GUI.backgroundColor = Color.white;
                            }

                            if (GUILayout.Button("設置", GUILayout.MaxWidth(40f)))
                            {
                                Undo.RecordObject(BaseData, "SET");
                                //int FloorPoint = i;
                                for (int _X = LDpos.x; _X < BOARD_WIDTH && _X <= RUpos.x; _X++)
                                    for (int _Y = LDpos.y; _Y < BOARD_HEIGHT && _Y <= RUpos.y; ++_Y)
                                        BaseData.preset[Slider_Value_Map].Height[_Y].Width[_X] = (Map_Object)SerectObj;
                            }
                        }

                        using (new EditorGUILayout.VerticalScope())
                        {
                            EditorGUILayout.LabelField("左下", GUILayout.MaxWidth(30f));
                            EditorGUILayout.LabelField("右上", GUILayout.MaxWidth(30f));
                        }

                        using (new EditorGUILayout.VerticalScope())
                        {
                            LDpos = EditorGUILayout.Vector2IntField("", LDpos, GUILayout.MaxWidth(80f));
                            RUpos = EditorGUILayout.Vector2IntField("", RUpos, GUILayout.MaxWidth(80f));
                        }
                    }

                    if (0 < this.BaseData.preset.Length && 0 < this.BaseData.preset[Slider_Value_Map].Agents.Length)
                    {
                        EditorGUILayout.LabelField("エージェント＆ゴール", GUILayout.MinWidth(180f));
                        using (var scroll = new EditorGUILayout.ScrollViewScope(scrollPosition, GUILayout.MinWidth(180f)))
                        {
                            scrollPosition = scroll.scrollPosition;                    
                            for (int i = 0; i < this.BaseData.preset[Slider_Value_Map].Agents.Length; i++)
                            {
                                GUILayout.Box("", GUILayout.Height(10), GUILayout.ExpandWidth(true));

                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    using (new EditorGUILayout.VerticalScope())
                                    {
                                        EditorGUILayout.LabelField(i.ToString(), GUILayout.MaxWidth(20f));
                                    }

                                    using (new EditorGUILayout.VerticalScope())
                                    {
                                        EditorGUILayout.LabelField("スタート", GUILayout.MaxWidth(40f));
                                        EditorGUILayout.LabelField(" ゴール ", GUILayout.MaxWidth(40f));
                                        EditorGUILayout.LabelField(" Color  ", GUILayout.MaxWidth(40f));
                                        EditorGUILayout.LabelField("obj_S", GUILayout.MaxWidth(40f));
                                        EditorGUILayout.LabelField("obj_G", GUILayout.MaxWidth(40f));
                                        EditorGUILayout.LabelField("symbol", GUILayout.MaxWidth(40f));
                                        //EditorGUILayout.LabelField("material", GUILayout.MaxWidth(40f));
                                    }

                                    using (new EditorGUILayout.VerticalScope())
                                    {
                                        BaseData.preset[Slider_Value_Map].Agents[i].StartPos =
                                                EditorGUILayout.Vector2IntField("", BaseData.preset[Slider_Value_Map].Agents[i].StartPos,
                                                GUILayout.MaxWidth(80f));

                                        BaseData.preset[Slider_Value_Map].Agents[i].GoalPos =
                                                EditorGUILayout.Vector2IntField("", BaseData.preset[Slider_Value_Map].Agents[i].GoalPos,
                                                GUILayout.MaxWidth(80f));

                                        BaseData.preset[Slider_Value_Map].Agents[i].MyColor =
                                            EditorGUILayout.ColorField("", BaseData.preset[Slider_Value_Map].Agents[i].MyColor,
                                                GUILayout.MaxWidth(80f));

                                        BaseData.preset[Slider_Value_Map].Agents[i].agentObj = (GameObject)
                                            EditorGUILayout.ObjectField(BaseData.preset[Slider_Value_Map].Agents[i].agentObj,
                                            typeof(GameObject),GUILayout.MaxWidth(80f));

                                        BaseData.preset[Slider_Value_Map].Agents[i].goalObj = (GameObject)
                                            EditorGUILayout.ObjectField(BaseData.preset[Slider_Value_Map].Agents[i].goalObj,
                                            typeof(GameObject),GUILayout.MaxWidth(80f));
                                   
                                        BaseData.preset[Slider_Value_Map].Agents[i].symbol = (GameObject)
                                           EditorGUILayout.ObjectField(BaseData.preset[Slider_Value_Map].Agents[i].symbol,
                                            typeof(GameObject), GUILayout.MaxWidth(80f));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        if (Event.current.type == EventType.DragUpdated)
        {
            if (DragAndDrop.objectReferences != null &&
                DragAndDrop.objectReferences.Length > 0 &&
                DragAndDrop.objectReferences[0] is MapDataBase)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                Event.current.Use();
            }
        }
        else if (Event.current.type == EventType.DragPerform)
        {
            Undo.RecordObject(this, "Change MapDataBase");
            this.BaseData = ((MapDataBase)DragAndDrop.objectReferences[0]).Clone();
            this.BaseDataPath = DragAndDrop.paths[0];
            DragAndDrop.AcceptDrag();
            Event.current.Use();
        }
        if (DragAndDrop.visualMode == DragAndDropVisualMode.Copy)
        {
            var rect = new Rect(Vector2.zero, this.position.size);
            var bgColor = Color.white * new Color(1f, 1f, 1f, 0.2f);
            EditorGUI.DrawRect(rect, bgColor);
            EditorGUI.LabelField(rect, "ここにアイテムデータをドラッグ＆ドロップしてください", this._skin.GetStyle("D&D"));
        }
    }
    public void AddItemsToMenu(GenericMenu menu)
    {
        menu.AddItem(new GUIContent("Original Menu"), false, () => Debug.Log("Press Menu!"));
    }
}
