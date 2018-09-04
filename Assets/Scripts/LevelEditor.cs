using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToolsBasicMiguel
{
    public enum LESelectedBrush
    {
        GROUND,
        PROPS
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

        public LESelectedBrush eSelectedBrush = LESelectedBrush.GROUND;

        public List<GameObject> GroundBrush;

        public List<GameObject> Props;

        GameObject CurrentBrush;



        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
        }

        public void SetBrush()
        {
            if(Application.isEditor)
            {
                if (CurrentBrush != null)
                    return;

                if (LESelectedBrush.GROUND == eSelectedBrush && GroundBrush.Count > 0)
                {
                    CurrentBrush = Instantiate(GroundBrush[selectedGround], transform);
                }
            }
        }

        public void SetCursor(Vector3 point)
        {
            if (Application.isEditor && _On)
            {
                if (CurrentBrush == null)
                    return;

                CurrentBrush.transform.position = point;
            }
        }

        public void SpawnTile()
        {
            if (Application.isEditor && _On)
            {
                if (CurrentBrush == null)
                    return;

                if (LESelectedBrush.GROUND == eSelectedBrush && GroundBrush.Count > 0)
                {
                    Instantiate(GroundBrush[selectedGround], CurrentBrush.transform.position, CurrentBrush.transform.rotation, transform);
                }
            }
        }
    }
}
