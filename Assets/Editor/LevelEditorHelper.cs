using UnityEngine;
using UnityEditor;
using System;

namespace ToolsBasicMiguel
{
    [CustomEditor(typeof(LevelEditor))]
    [InitializeOnLoad]
    public class LevelEditorHelper : Editor {

        private LevelEditor levelEditor;
        private const float ROWHEIGHT = 75;

        // Keeping the LevelEditor GameObject for as long is on "Building" mode
        private void CallbackFunction()
        {
            if (levelEditor)
            {
                if (levelEditor.on)
                {
                    Selection.activeGameObject = levelEditor.gameObject;
                    levelEditor.SetBrush();
                }
            }

        }
        void OnEnable()
        {
            EditorApplication.update += CallbackFunction;
        }
        void OnDisable()
        {
            EditorApplication.update -= CallbackFunction;
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (levelEditor == null)
                levelEditor = ((LevelEditor)target);

            var groundBrush = serializedObject.FindProperty("GroundBrush");
            var propBrush = serializedObject.FindProperty("Props");
            var snapOption = serializedObject.FindProperty("Snapping");

            DrawProperty(groundBrush, "Ground Set", levelEditor.GroundBrush != null ? levelEditor.GroundBrush.Count : 0, DropGroundBrushAction, DrawGroundBrush);
            DrawProperty(propBrush, "Props Set", levelEditor.Props != null ? levelEditor.Props.Count : 0, DropPropsBrushAction, DrawPropsBrush);
            EditorGUILayout.PropertyField(snapOption);

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }

        private void DropGroundBrushAction(UnityEngine.Object obj)
        {
            levelEditor.GroundBrush.Add((GameObject)obj);
            Repaint();
        }

        private void DrawGroundBrush(int elementIndex, Rect location)
        {
            Texture2D preview
                = AssetPreview.GetAssetPreview(levelEditor.GroundBrush[elementIndex]);

            GUIStyle style = new GUIStyle();
            if (elementIndex == levelEditor.selectedGround)
            {
                style = new GUIStyle(GUI.skin.box); 
            }


            GUI.Label(location, preview, style);
        }

        private void DropPropsBrushAction(UnityEngine.Object obj)
        {
            levelEditor.Props.Add((GameObject) obj);
            Repaint();
        }

        private void DrawPropsBrush(int elementIndex, Rect location)
        {
            Texture2D preview
                = AssetPreview.GetAssetPreview(levelEditor.Props[elementIndex]);

            GUI.Label(location, preview);
        }
        
        private void DrawProperty(
            SerializedProperty list, 
            string title,
            int listElementCount,
            Action<UnityEngine.Object> DropActionEvent,
            Action<int, Rect> DrawAction)
        {
            float row = Mathf.Floor(list.arraySize / 2);
            row = row < 1 ? 1 : row;

            GUILayout.Label(title);
            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(false), GUILayout.Height(row * ROWHEIGHT), GUILayout.Width(240));           
            GUI.Box(dropArea, "");
            
            int rowInt = Mathf.RoundToInt(row);
            GUI.BeginGroup(dropArea);

            if (listElementCount > 0)
            {
                int currentBrush = 0, xPos = 0, rowBreaker = 2;

                for (int j = 0; j < rowInt; j++)
                {
                    for (int i = currentBrush; i < listElementCount; i++)
                    {
                        if (i > rowBreaker)
                        {
                            rowBreaker *= 2;
                            currentBrush = i;
                            xPos = 0;
                            break;
                        }

                        float xRect = (xPos + 1) * dropArea.xMin + (60 * xPos);
                        Rect location = new Rect(xRect, j * 60 + 10, 60, 60);
                        DrawAction(i, location);

                        xPos++;
                    }
                }
            }
            else
            {
                Rect location = new Rect(dropArea.xMax * 0.15f, dropArea.height * 0.4f, 200, 50);
                GUI.Label(location, "<< Drop prefabs here >>", new GUIStyle(EditorStyles.boldLabel));
            }

            GUI.EndGroup();
            
            Event evt = Event.current;
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(evt.mousePosition))
                        return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (UnityEngine.Object dragged_object in DragAndDrop.objectReferences)
                        {
                            DropActionEvent(dragged_object);
                        }
                    }
                    break;
            }
        }


        private bool isScaling = false;
        private bool isRotating = false;
        private void OnSceneGUI()
        {
            if (levelEditor == null)
                levelEditor = (LevelEditor)target;

            Event e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                if (e.shift && e.keyCode == KeyCode.S)
                {
                    // Toggle Level Editor on SHIFT + S
                    levelEditor.on = !levelEditor.on;
                    if(!levelEditor.on)
                    {
                        isScaling = false;
                        isRotating = false;
                    }

                }else if(e.shift && e.keyCode == KeyCode.Q)
                {
                    levelEditor.ChangeBrush(LEChangeBrushDirection.LEFT);
                }
                else if(e.shift && e.keyCode == KeyCode.E)
                {
                    levelEditor.ChangeBrush(LEChangeBrushDirection.RIGHT);
                }
                else if(e.shift && e.keyCode == KeyCode.R && levelEditor.on) 
                {
                    isScaling = !isScaling;
                    if (!isScaling)
                        isRotating = false;
                }
                else if(e.shift && e.keyCode == KeyCode.T && levelEditor.on)
                {
                    isRotating = !isRotating;
                    if (!isRotating)
                        isScaling = false;
                }
            }
            
            if(e.type == EventType.KeyUp && e.control && e.keyCode == KeyCode.Z && levelEditor.on)
            {
                levelEditor.ResetBrushTransform();
            }

            if (isScaling)
            {
                EditorGUI.BeginChangeCheck();
                
                float handleSize = HandleUtility.GetHandleSize(levelEditor.BrushTransform().position );
                Vector3 newScale = Handles.ScaleHandle(
                    levelEditor.BrushTransform().localScale, 
                    levelEditor.BrushTransform().position,
                    levelEditor.BrushTransform().rotation, handleSize );

                if (EditorGUI.EndChangeCheck())
                {
                    levelEditor.ScaleCurrentBrush(newScale);
                }
            }

            if(isRotating)
            {
                EditorGUI.BeginChangeCheck();
                Quaternion newRotation = Handles.RotationHandle(
                    levelEditor.BrushTransform().rotation, 
                    levelEditor.BrushTransform().localPosition);

                if (EditorGUI.EndChangeCheck())
                {
                    levelEditor.RotateCurrentBrush(newRotation);
                }
            }
            
            GUILayout.BeginArea(new Rect(20, 20, 190, 270));
            {
                var rect = EditorGUILayout.BeginVertical();
                {
                    GUI.Box(rect, GUIContent.none);
                    
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        GUIStyle myLabelStyle = new GUIStyle(GUI.skin.label);
                        myLabelStyle.normal.textColor = Color.black;
                        myLabelStyle.fontSize = 12;

                        GUILayout.Label("Level Editor", myLabelStyle);

                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

                    GUILayout.BeginHorizontal();
                    {
                        GUIStyle statusStyle = new GUIStyle(EditorStyles.boldLabel);

                        if(levelEditor.on)
                        {
                            GUILayout.Label("Current Status: ACTIVE \n(SHIFT + S to deactivate)", statusStyle);
                        }
                        else
                        {
                            GUILayout.Label("Current Status: INNACTIVE \n(SHIFT + S to activate)");
                        }

                    }
                    GUILayout.EndHorizontal();


                    GUILayout.BeginVertical();
                    {
                        GUIStyle selectedSet = new GUIStyle(EditorStyles.boldLabel);
                        if (levelEditor.eSelectedBrush == LESelectedBrush.GROUND)
                        {

                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Current Set: ");
                            GUILayout.Label("GROUND", selectedSet);
                            GUILayout.EndHorizontal();
                        }
                        else
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Current Set: ");
                            GUILayout.Label("PROPS", selectedSet);
                            GUILayout.EndHorizontal();
                        }

                        GUILayout.Label("(Shift + W to change set)");

                        if (levelEditor.GroundBrush.Count > 0)
                        { 
                            GameObject obj = levelEditor.GroundBrush[levelEditor.selectedGround];
                            Texture2D preview = AssetPreview.GetAssetPreview(obj);

                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.FlexibleSpace();
                                
                                GUILayout.Label("Selected Brush: ", new GUIStyle(EditorStyles.boldLabel));
                                GUILayout.FlexibleSpace();
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            {
                                if(GUILayout.Button("<"))
                                {
                                    levelEditor.ChangeBrush(LEChangeBrushDirection.LEFT);
                                }

                                GUILayout.FlexibleSpace();
                                GUILayout.Label(preview);
                                GUILayout.FlexibleSpace();
                                
                                if(GUILayout.Button(">"))
                                {
                                    levelEditor.ChangeBrush(LEChangeBrushDirection.RIGHT);
                                }
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();                           
                                GUILayout.Label("To change Brush press \nSHIFT-Q or SHIFT-E");
                            GUILayout.EndHorizontal();
                        }
                    }
                    GUILayout.EndVertical();
                }
                EditorGUILayout.EndVertical();
            }
            GUILayout.EndArea();

            if (levelEditor.on && !isScaling && !isRotating)
            {
                // Way of controlling the drag-click-move
                //
                //int controlId = GUIUtility.GetControlID(FocusType.Passive);
                //GUIUtility.hotControl = controlId;
                //HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));
                //Event.current.Use();
                int controlId = GUIUtility.GetControlID(FocusType.Passive);

                switch (Event.current.type)
                {
                    case EventType.MouseDown:
                        OnMouseDown(Event.current);
                        break;
                    case EventType.MouseMove:
                        OnMouseMove(Event.current);
                        break;
                    case EventType.MouseDrag:
                        GUIUtility.hotControl = controlId;
                        OnMouseDrag(Event.current);
                        Event.current.Use();
                        break;
                    default:
                        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));
                        break;
                }
            }
            
        }

        private void OnHoldKeyDown(Event current)
        {

        }

        private void OnMouseDrag(Event current)
        {
            //For now lets just spawn like crazy
            if (!Application.isPlaying 
                && current.button == 0)
            {
                OnMouseMove(current);
                OnMouseDown(current);
            }
        }

        private void OnMouseDown(Event current)
        {
            if (!Application.isPlaying && current.button == 0)
            {
                levelEditor.SpawnTile();
            }
        }

        private void OnMouseMove(Event current)
        {
            if (!Application.isPlaying)
            {
                SceneView view = SceneView.currentDrawingSceneView;
                if (view != null)
                {
                    Ray ray = HandleUtility.GUIPointToWorldRay(current.mousePosition);                    
                    RaycastHit[] hits = Physics.RaycastAll(ray);

                    int i = 0;
                    while(i < hits.Length)
                    {
                        if(hits[i].collider.gameObject.GetComponent<LevelEditor>() != null)
                        {
                            levelEditor.SetCursor(hits[i].point);
                            break;
                        }
                    }
                    /*
                    if (Physics.Raycast(ray, out hit, 500))
                    {
                        levelEditor.SetCursor(hit.point);
                    }*/
                }
            }
        }

    }
    
    public class MassRenamePopup : EditorWindow
    {
        private const string DEFAULT_TEXT = "Asset", DEFAULT_SEPARATOR = "_";
        private string newNameTF;

        [MenuItem("Assets/Custom Tools/Mass Rename")]
        public static void Init()
        {
            MassRenamePopup window = new MassRenamePopup(); Debug.LogWarning("[Tool] While the renaming is in process, the editor might not be responsive");
            Debug.LogWarning("[Tool] While the renaming is in process, the editor might not be responsive");
            window.ShowUtility();            
        }
        

        private void OnGUI()
        {
            newNameTF 
                = EditorGUILayout.TextField("Base name", DEFAULT_TEXT);
            
            if (GUILayout.Button("Apply"))
            {
                int index = 0;
                foreach (GameObject obj
                    in Selection.gameObjects)
                {
                    string oldPath = AssetDatabase.GetAssetPath(obj);
                    AssetDatabase.RenameAsset(oldPath, newNameTF + DEFAULT_SEPARATOR + (++index));
                }
                Close();
            }
            
        }
    }

}
 