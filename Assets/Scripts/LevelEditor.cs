using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ToolsBasicMiguel
{
    public enum LESelectedBrush
    {
        GROUND,
        PROPS
    }

    public enum LEChangeBrushDirection
    {
        LEFT = -1,
        RIGHT = 1
    }

    [ExecuteInEditMode]
    public class LevelEditor : MonoBehaviour
    {
        [HideInInspector]
        bool _On = false;
        [HideInInspector]
        public bool on
        {
            get { return _On; }
            set
            {
                _On = value;
                if (!_On && CurrentBrush != null)
                {
                    DestroyImmediate(CurrentBrush);
                }
            }
        }
        [HideInInspector]
        public int selectedGround = 0;

        [HideInInspector]
        public bool brushChange = false;

        public LESelectedBrush eSelectedBrush = LESelectedBrush.GROUND;
        
        [SerializeField]
        public List<GameObject> GroundBrush;

        [SerializeField]
        public List<GameObject> Props;

        public bool Snapping = false;

        private GameObject CurrentBrush;
        private Material[] BrushMaterialsInstance;
        private Vector3 lastPoint = Vector3.zero;

        public void SetBrush()
        {
            if(Application.isEditor)
            {
                if (CurrentBrush != null)
                    return;

                if (LESelectedBrush.GROUND == eSelectedBrush && GroundBrush.Count > 0)
                {
                    CurrentBrush = Instantiate(GroundBrush[selectedGround], transform);
                    CurrentBrush.transform.position = lastPoint;
                    BrushMaterialsInstance = CurrentBrush.GetComponentInChildren<Renderer>().sharedMaterials;
                }
            }
        }

        public void ChangeBrush(LEChangeBrushDirection direction)
        {
            if(Application.isEditor)
            {
                int value = (int)direction;
                if(LESelectedBrush.GROUND == eSelectedBrush)
                {
                    int aux = selectedGround + value;
                    if (aux < 0)
                        selectedGround = GroundBrush.Count - 1;                        
                    else if (aux >= GroundBrush.Count)
                        selectedGround = 0;
                    else
                        selectedGround = aux;

                    DestroyImmediate(CurrentBrush);
                    CurrentBrush = null;
                }
            }

        }

        public void SetCursor(Vector3 point)
        {
            if (Application.isEditor && _On)
            {
                if (CurrentBrush == null)
                    return;

                Vector3 snappedPoint = point;
                if (Snapping)
                {
                    snappedPoint.x = Mathf.RoundToInt(snappedPoint.x);
                    snappedPoint.z = Mathf.RoundToInt(snappedPoint.z);
                }

                CurrentBrush.transform.position = snappedPoint;
                lastPoint = point;
            }
        }

        public Transform BrushTransform()
        {
            Transform t = CurrentBrush == null ? null : CurrentBrush.transform;
            return t;
        }

        public void ResetBrushTransform()
        {
            if (CurrentBrush != null)
            {
                CurrentBrush.transform.localScale = Vector3.one;
                CurrentBrush.transform.rotation = Quaternion.identity;
            }
        }

        public void ScaleCurrentBrush(Vector3 newScale)
        {
            if(Application.isEditor && _On)
            {
                if(CurrentBrush != null)
                {
                    Vector3 snappedScale = newScale;
                    if (Snapping)
                    {
                        snappedScale.x = Mathf.RoundToInt(snappedScale.x) * -1;
                        snappedScale.y = Mathf.RoundToInt(snappedScale.y);
                        snappedScale.z = Mathf.RoundToInt(snappedScale.z) * -1;
                    }

                    CurrentBrush.transform.localScale = snappedScale;
                }
            }
        }

        public void RotateCurrentBrush(Quaternion newRotation)
        {
            if (Application.isEditor && _On)
            {
                if (CurrentBrush != null)
                {
                    Undo.RecordObject(CurrentBrush, "Rotated RotateAt Point");
                    CurrentBrush.transform.rotation = newRotation;
                }
            }
        }

        public void SpawnTile()
        {
            if (Application.isEditor && _On)
            {
                if (CurrentBrush == null && _On)
                    return;

                if (LESelectedBrush.GROUND == eSelectedBrush && GroundBrush.Count > 0)
                {
                    GameObject go = Instantiate(GroundBrush[selectedGround], CurrentBrush.transform.position, CurrentBrush.transform.rotation, transform);
                    go.transform.localScale = CurrentBrush.transform.localScale;
                }
            }
        }
    }
}
