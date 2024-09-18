using UnityEngine;
using UnityEditor;


    public class MapWindow : EditorWindow
    {
        [MenuItem("Window/OriginalPanels/MapWindow", priority = 1)]
        static public void CreateWindow()
        {
            EditorWindow.GetWindow<MapWindow>();
        }

        [SerializeField]
        private MapDataObject BaseData;

        [SerializeField]
        private string BaseDataPath;

        [SerializeField]
        private GUISkin skin;
        private void OnEnable()
        {
            var defaultData  = AssetDatabase.LoadAssetAtPath<MapDataObject>("Assets/Kenkyu/KeiroTansaku/NewRocason/ClassData/MapDataObject.asset");
            //var defaultData = Resources.Load<MapDataBase>("MapDataBase");
            this.BaseData = defaultData.Clone();
            this.BaseDataPath = AssetDatabase.GetAssetPath(defaultData);
        }
        const int BOARD_WIDTH = 30;
        const int BOARD_HEIGHT = 20;
        private Vector2 scrollPosition_A, scrollPosition_G;
        private Vector2Int LDpos, RUpos;
        private int Slider_Value_Map = 0;
        
        public enum Map_Object_bata
        {
            Wall = 0,
            Spase = 1,
        }
        
        private Map_Object_bata SerectObj = Map_Object_bata.Spase;
        
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
                {
                    var preset = BaseData.preset[Slider_Value_Map];
                    for (int i = 0; i < preset.agents.Length; i++)
                    {
                        bool _agent = (preset.height[pos.y].width[pos.x] == Map_Object.Agent);
                        bool _posint = (preset.agents[i].startPos == pos);
                        if (_agent && _posint)
                        {
                            GUI.backgroundColor = preset.agents[i].myColor; 
                            break;
                        }
                        if(i + 1 == preset.agents.Length)
                            GUI.backgroundColor = Color.gray;
                    }
                }
                    break;
                case Map_Object.Goal:
                {
                    var preset = BaseData.preset[Slider_Value_Map];
                    for (int i = 0; i < preset.goal.Length; i++)
                    {
                        bool _agent = (preset.height[pos.y].width[pos.x] == Map_Object.Goal);
                        bool _posint = (preset.goal[i].goalPos == pos);
                        if (_agent && _posint)
                        {
                            GUI.backgroundColor = preset.goal[i].myColor;
                            break;
                        }
                        if (i + 1 == preset.goal.Length)
                            GUI.backgroundColor = Color.gray;
                    }
                }
                    break;
                default:
                    GUI.backgroundColor = Color.gray;
                    break;
            }
        }

        bool SaveCheck()
        {
            var preset = BaseData.preset[Slider_Value_Map];

        if (preset.agents.Length <= 0 || preset.goal.Length <= 0)
        {
            return false;
        }

        for (int i = 0; i < preset.agents.Length; i++)
        {
            Vector2Int S_pos = new(preset.agents[i].startPos.x, preset.agents[i].startPos.y);
            bool C_start = false;

            if (S_pos.x >= 0 && S_pos.x < BOARD_WIDTH && S_pos.y >= 0 && S_pos.y < BOARD_HEIGHT)
            {
                C_start = (preset.height[S_pos.y].width[S_pos.x] == Map_Object.Agent);

            }
            if (!C_start)
            {
                return false;
            }
        }

        for (int i = 0; i < preset.goal.Length; i++)
        {
            Vector2Int G_pos = preset.goal[i].goalPos;
            bool C_goal = false;
            if (G_pos.x >= 0 && G_pos.x < BOARD_WIDTH && G_pos.y >= 0 && G_pos.y < BOARD_HEIGHT)
            {
                C_goal = (preset.height[G_pos.y].width[G_pos.x] == Map_Object.Goal);
            }
            if (!C_goal)
            {
                return false;
            }
        }

        return true;
        }
        private void OnGUI()
        {
        using (new EditorGUILayout.VerticalScope())
        {
            if(BaseData == null) this.BaseData = AssetDatabase.LoadAssetAtPath<MapDataObject>(this.BaseDataPath).Clone();

            Undo.RecordObject(BaseData, "Modify MapDataBase");
     
            using (new EditorGUILayout.HorizontalScope())
            {
                Slider_Value_Map = (int)EditorGUILayout.Slider("Map_ID", Slider_Value_Map, 0, this.BaseData.preset.Length - 1, GUILayout.MaxWidth(300f));
                EditorGUILayout.LabelField("||Map:", GUILayout.MaxWidth(40f));
               
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("|Floor:", GUILayout.MaxWidth(40f));
                if (GUILayout.Button("AddPreset", GUILayout.MaxWidth(90f), GUILayout.MaxHeight(20f)))
                {
                    Undo.RecordObject(BaseData, "Add Map");
                    int Map_Length = this.BaseData.preset.Length;
                    System.Array.Resize(ref this.BaseData.preset, Map_Length + 1);
                    this.BaseData.preset[Map_Length] = new MapDataObject.Preset()
                    {
                        agents = new MapDataObject.Preset.Agent[1],
                        height =  new MapDataObject.Preset.HEIGHT[BOARD_HEIGHT],
                        goal = new MapDataObject.Preset.Goal[1],
                    };
                    
                    this.BaseData.preset[Map_Length].agents[0]  = new MapDataObject.Preset.Agent()
                    {
                        startPos = new Vector2Int(0, 0),
                        myColor = new Color(255,255,255,1f),
                    };
                    
                    for(int _Y = 0; BOARD_HEIGHT >_Y; _Y++)
                    {
                        this.BaseData.preset[Map_Length].height[_Y] = new MapDataObject.Preset.HEIGHT()
                        {
                            width = new Map_Object[BOARD_WIDTH],
                        };
                        //System.Array.Resize(ref this.BaseData.preset[Map_Length].Height[_Y].Width, BOARD_WIDTH);
                        for (int _X = 0; BOARD_WIDTH > _X; _X++)
                            this.BaseData.preset[Map_Length].height[_Y].width[_X] = Map_Object.Wall;
                    }
                    
                    this.BaseData.preset[Map_Length].goal[0]  = new MapDataObject.Preset.Goal()
                    {
                        goalPos = new Vector2Int(0, 0),
                        myColor = new Color(255,255,255,1f),
                    };
                }
                
                if (GUILayout.Button("AddAgent", GUILayout.MaxWidth(90f), GUILayout.MaxHeight(20f)))
                {
                    Undo.RecordObject(BaseData, "Add Agent");
                    int Point_Length = this.BaseData.preset[Slider_Value_Map].agents.Length;
                    System.Array.Resize(ref this.BaseData.preset[Slider_Value_Map].agents, Point_Length + 1);
                    this.BaseData.preset[Slider_Value_Map].agents[Point_Length]  = new MapDataObject.Preset.Agent()
                    {
                        startPos = new Vector2Int(0, 0),
                        myColor = new Color(255,255,255,1f),
                    };
                }
                if (GUILayout.Button("AddGoal", GUILayout.MaxWidth(90f), GUILayout.MaxHeight(20f)))
                {
                    Undo.RecordObject(BaseData, "Add Goal");
                    int Point_Length = this.BaseData.preset[Slider_Value_Map].goal.Length;
                    System.Array.Resize(ref this.BaseData.preset[Slider_Value_Map].goal, Point_Length + 1);
                    this.BaseData.preset[Slider_Value_Map].goal[Point_Length]  = new MapDataObject.Preset.Goal()
                    {
                        goalPos = new Vector2Int(0, 0),
                        myColor = new Color(255,255,255,1f),
                    };
                }
                if (GUILayout.Button("Reset", GUILayout.MaxWidth(60f), GUILayout.MaxHeight(20f)))
                {
                    this.BaseData = AssetDatabase.LoadAssetAtPath<MapDataObject>(this.BaseDataPath).Clone();
                    EditorGUIUtility.editingTextField = false;
                }

                if (GUILayout.Button("Save", GUILayout.MaxWidth(60f), GUILayout.MaxHeight(20f)))
                {
                    bool Chack = SaveCheck();
                    if (Chack)
                    {
                        var data = AssetDatabase.LoadAssetAtPath<MapDataObject>(this.BaseDataPath);
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
                                
                                Map_Object map_Object = BaseData.preset[Slider_Value_Map].height[Y].width[X];

                                ColorGUI(map_Object,new(X,Y));
                               
                                BaseData.preset[Slider_Value_Map].height[Y].width[X] =
                                (Map_Object)EditorGUILayout.EnumPopup(BaseData.preset[Slider_Value_Map].height[Y].width[X],
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
                    EditorGUILayout.LabelField("RengeSet", GUILayout.MinWidth(180f));
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

                            if (GUILayout.Button("Set", GUILayout.MaxWidth(40f)))
                            {
                                Undo.RecordObject(BaseData, "SET");
                                //int FloorPoint = i;
                                for (int _X = LDpos.x; _X < BOARD_WIDTH && _X <= RUpos.x; _X++)
                                    for (int _Y = LDpos.y; _Y < BOARD_HEIGHT && _Y <= RUpos.y; ++_Y)
                                        BaseData.preset[Slider_Value_Map].height[_Y].width[_X] = (Map_Object)SerectObj;
                            }
                        }

                        using (new EditorGUILayout.VerticalScope())
                        {
                            EditorGUILayout.LabelField("L_D", GUILayout.MaxWidth(30f));
                            EditorGUILayout.LabelField("R_U", GUILayout.MaxWidth(30f));
                        }

                        using (new EditorGUILayout.VerticalScope())
                        {
                            LDpos = EditorGUILayout.Vector2IntField("", LDpos, GUILayout.MaxWidth(80f));
                            RUpos = EditorGUILayout.Vector2IntField("", RUpos, GUILayout.MaxWidth(80f));
                        }
                    }

                    if (0 < this.BaseData.preset.Length)
                    {
                        var agentData = BaseData.preset[Slider_Value_Map].agents;
                    if (0 < agentData.Length)
                    {
                        EditorGUILayout.LabelField("AgentInfomation", GUILayout.MinWidth(180f));
                        using (var scroll = new EditorGUILayout.ScrollViewScope(scrollPosition_A, GUILayout.MinWidth(180f)))
                        {
                            scrollPosition_A = scroll.scrollPosition;
                          
                            for (int i = 0; i < agentData.Length; i++)
                            {
                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    using (new EditorGUILayout.VerticalScope())
                                    {
                                        EditorGUILayout.LabelField(i.ToString(), GUILayout.MaxWidth(20f));
                                    }

                                    using (new EditorGUILayout.VerticalScope())
                                    {
                                        EditorGUILayout.LabelField("S_Pos", GUILayout.MaxWidth(40f));
                                        EditorGUILayout.LabelField(" Color  ", GUILayout.MaxWidth(40f));
                                        EditorGUILayout.LabelField("obj_S", GUILayout.MaxWidth(40f));
                                        EditorGUILayout.LabelField("symbol", GUILayout.MaxWidth(40f));
                                    }
                                    
                                    using (new EditorGUILayout.VerticalScope())
                                    {
                                        agentData[i].startPos =
                                                EditorGUILayout.Vector2IntField("", agentData[i].startPos,
                                                GUILayout.MaxWidth(80f));

                                        agentData[i].myColor =
                                            EditorGUILayout.ColorField("", agentData[i].myColor,
                                                GUILayout.MaxWidth(80f));

                                        agentData[i].agentObj = (GameObject)
                                            EditorGUILayout.ObjectField(agentData[i].agentObj,
                                            typeof(GameObject),GUILayout.MaxWidth(80f));
                                   
                                        agentData[i].symbol = (GameObject)
                                           EditorGUILayout.ObjectField(agentData[i].symbol,
                                            typeof(GameObject), GUILayout.MaxWidth(80f));
                                    }
                                }
                                GUILayout.Box("", GUILayout.Height(10), GUILayout.ExpandWidth(true));
                            }
                        }
                    }
                    
                    var goalData = BaseData.preset[Slider_Value_Map].goal;
                    if (0 < goalData.Length)
                    {
                        EditorGUILayout.LabelField("GoalInfomation", GUILayout.MinWidth(180f));
                        using (var scroll = new EditorGUILayout.ScrollViewScope(scrollPosition_G, GUILayout.MinWidth(180f)))
                        {
                            scrollPosition_G = scroll.scrollPosition;                    
                            for (int i = 0; i < goalData.Length; i++)
                            {
                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    using (new EditorGUILayout.VerticalScope())
                                    {
                                        EditorGUILayout.LabelField(i.ToString(), GUILayout.MaxWidth(20f));
                                    }

                                    using (new EditorGUILayout.VerticalScope())
                                    {
                                        EditorGUILayout.LabelField("G_Pos", GUILayout.MaxWidth(40f));
                                        EditorGUILayout.LabelField(" Color  ", GUILayout.MaxWidth(40f));
                                        EditorGUILayout.LabelField("obj_G", GUILayout.MaxWidth(40f));
                                    }

                                    using (new EditorGUILayout.VerticalScope())
                                    {
                                        goalData[i].goalPos =
                                            EditorGUILayout.Vector2IntField("", goalData[i].goalPos,
                                                GUILayout.MaxWidth(80f));

                                        goalData[i].myColor =
                                            EditorGUILayout.ColorField("", goalData[i].myColor,
                                                GUILayout.MaxWidth(80f));

                                        goalData[i].goalObj = (GameObject)
                                            EditorGUILayout.ObjectField(goalData[i].goalObj,
                                            typeof(GameObject),GUILayout.MaxWidth(80f));
                                    }
                                }
                                GUILayout.Box("", GUILayout.Height(10), GUILayout.ExpandWidth(true));
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
                DragAndDrop.objectReferences[0] is MapDataObject)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                Event.current.Use();
            }
        }
        else if (Event.current.type == EventType.DragPerform)
        {
            Undo.RecordObject(this, "Change MapDataBase");
            this.BaseData = ((MapDataObject)DragAndDrop.objectReferences[0]).Clone();
            this.BaseDataPath = DragAndDrop.paths[0];
            DragAndDrop.AcceptDrag();
            Event.current.Use();
        }
        if (DragAndDrop.visualMode == DragAndDropVisualMode.Copy)
        {
            var rect = new Rect(Vector2.zero, this.position.size);
            var bgColor = Color.white * new Color(1f, 1f, 1f, 0.2f);
            EditorGUI.DrawRect(rect, bgColor);
            EditorGUI.LabelField(rect, "�����ɃA�C�e���f�[�^���h���b�O���h���b�v���Ă�������", this.skin.GetStyle("D&D"));
        }
    }
    public void AddItemsToMenu(GenericMenu menu)
    {
        menu.AddItem(new GUIContent("Original Menu"), false, () => Debug.Log("Press Menu!"));
    }
    }


