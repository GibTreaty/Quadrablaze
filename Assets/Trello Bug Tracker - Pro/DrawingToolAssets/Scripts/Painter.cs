using UnityEngine;
using System;
using System.Collections.Generic;

namespace DG.Util
{
    public class Painter : MonoBehaviour
    {

        public enum Tool
        {
            Brush,
            Eraser,
            None
        }
        [Header("Drawing Toolset")]
        public Tool tool = Tool.Brush;
        public Texture[] toolIcons;
        public Texture2D colorCircleTex;
        public Texture2D clearDwatingIcon;
        public Color brushColor = Color.white;

        [Header("Canvas")]
        [Tooltip("A value of 2 would mean that the transparent texture to be drawn on top off is going to be half of the resolution of the screenshot taken, this saves a lot of perfomance but makes drawing look a bit more pixelated")]
        public int drawingTextureDownScalingRatio = 2;
        public GUISkin gskin;

        [Header("Organization")]
        public UIOffsets uIOffsets;
        [Tooltip("The scale of the canvas in proportion to the size of the screen")]
        public float canvasScaleRatio = 0.675f;

        public Action _OnDisable;

        [HideInInspector]
        public Texture2D baseTex;
        private Texture2D drawingTex;

        private BrushTool brush = new BrushTool();
        private EraserTool eraser = new EraserTool();

        private Vector2 dragStart;
        private Vector2 dragEnd;

        void OnGUI()
        {
            GUI.skin = gskin;

            //Main frame
            GUILayout.BeginArea(new Rect(Screen.width * uIOffsets.mainPainterBox.x, Screen.height * uIOffsets.mainPainterBox.y, Screen.width * .9f, Screen.height * .9f), "", "PurpleBox");
            //Toolbox UI
            GUILayout.BeginArea(new Rect(Screen.width * uIOffsets.toolsbox.x, Screen.height * uIOffsets.toolsbox.y, Screen.width * .15f, Screen.height));

            GUILayout.Space(50);
            GUILayout.Label("Drawing Options");
            tool = (Tool)GUILayout.Toolbar((int)tool, toolIcons, "Tool");
            GUILayout.Space(50);
            switch (tool)
            {
                case Tool.Brush:
                    GUILayout.Label("Size " + Mathf.Round(brush.width * 10) / 10);
                    brush.width = GUILayout.HorizontalSlider(brush.width, 0, 40);
                    GUILayout.Space(20);
                    brushColor = GUIControls.RGBCircle(brushColor, "Color Picker", colorCircleTex);
                    GUILayout.Space(35);
                    break;
                case Tool.Eraser:
                    GUILayout.Label("Size " + Mathf.Round(eraser.width * 10) / 10);
                    eraser.width = GUILayout.HorizontalSlider(eraser.width, 0, 50);
                    GUILayout.Space(211f);
                    break;
            }
            if (GUILayout.Button(clearDwatingIcon, "ClearDrawing"))
            {
                OnEnable();
            }
            GUILayout.Space(25);
            if (GUILayout.Button("Save", "Save"))
            {
                Drawing.MergeTextures(ref baseTex, ref drawingTex, drawingTextureDownScalingRatio);
                this.enabled = false;
            }
            GUILayout.EndArea();
            //Screenshot texture
            GUI.DrawTexture(new Rect(Screen.width * uIOffsets.canvas.x, Screen.height * uIOffsets.canvas.y, Screen.width * canvasScaleRatio, Screen.height * canvasScaleRatio), baseTex);
            //transparent texture to draw on
            GUI.DrawTexture(new Rect(Screen.width * uIOffsets.canvas.x, Screen.height * uIOffsets.canvas.y, Screen.width * canvasScaleRatio, Screen.height * canvasScaleRatio), drawingTex);
            //Close Button
            GUILayout.BeginArea(new Rect(Screen.width * (1 - uIOffsets.toolsbox.x) - Screen.width * .15f, Screen.height * uIOffsets.toolsbox.y, Screen.width * .15f, Screen.height));
            if (GUILayout.Button("", "Close")) this.enabled = false;
            GUILayout.EndArea();
            GUILayout.EndArea();
        }

        Vector2 preDrag;
        Rect imgRect;
        void Update()
        {
            imgRect = new Rect(Screen.width * (uIOffsets.canvas.x + uIOffsets.mainPainterBox.x), Screen.height * (uIOffsets.canvas.y + uIOffsets.mainPainterBox.y), Screen.width * canvasScaleRatio, Screen.height * canvasScaleRatio);

            Vector2 mouse = Input.mousePosition;
            mouse.y = Screen.height - mouse.y;

            if (Input.GetKeyDown("mouse 0"))
            {
                if (imgRect.Contains(mouse))
                {
                    dragStart = mouse - new Vector2(imgRect.x, imgRect.y);
                    dragStart.y = imgRect.height - dragStart.y;
                    //Rounds and takes into account the zoom of the drawing canvas
                    dragStart.x = Mathf.Round(dragStart.x * (canvasScaleRatio * drawingTextureDownScalingRatio));
                    dragStart.y = Mathf.Round(dragStart.y * (canvasScaleRatio * drawingTextureDownScalingRatio));

                    ClampCursor(mouse, imgRect);
                }
                else
                {
                    dragStart = Vector3.zero;
                }

            }
            if (Input.GetKey("mouse 0"))
            {
                if (dragStart == Vector2.zero)
                {
                    return;
                }

                ClampCursor(mouse, imgRect);

                if (tool == Tool.Brush)
                {
                    Brush(dragEnd, preDrag);
                }
                if (tool == Tool.Eraser)
                {
                    Eraser(dragEnd, preDrag);
                }
            }
            if (Input.GetKeyUp("mouse 0") && dragStart != Vector2.zero)
            {
                dragStart = Vector2.zero;
                dragEnd = Vector2.zero;
            }
            preDrag = dragEnd;
        }

        void ClampCursor(Vector2 mouse, Rect imgRect)
        {
            dragEnd = mouse - new Vector2(imgRect.x, imgRect.y);
            dragEnd.x = Mathf.Clamp(dragEnd.x, 0, imgRect.width);
            dragEnd.y = imgRect.height - Mathf.Clamp(dragEnd.y, 0, imgRect.height * 2);
            
            //Rounds and takes into account the zoom of the drawing canvas
            dragEnd.x = Mathf.Round(dragEnd.x / (canvasScaleRatio * drawingTextureDownScalingRatio));
            dragEnd.y = Mathf.Round(dragEnd.y / (canvasScaleRatio * drawingTextureDownScalingRatio));
        }

        Drawing.Stroke stroke = new Drawing.Stroke();
        void Brush(Vector2 p1, Vector2 p2)
        {
            if (p2 == Vector2.zero)
            {
                p2 = p1;
            }
            stroke.Set(p1, p2, brush.width, brush.hardness, brushColor);
            Drawing.PaintLine(stroke, ref drawingTex);
        }

        void Eraser(Vector2 p1, Vector2 p2)
        {
            if (p2 == Vector2.zero)
            {
                p2 = p1;
            }
            stroke.Set(p1, p2, eraser.width, eraser.hardness, Color.clear);
            Drawing.PaintLine(stroke, ref drawingTex);
        }

        public void OnDisable()
        {
            if (_OnDisable != null)
                _OnDisable();
        }

        public void OnEnable()
        {
            if (drawingTex != null) Destroy(drawingTex);
            drawingTex = new Texture2D(Screen.width / drawingTextureDownScalingRatio, Screen.height / drawingTextureDownScalingRatio, TextureFormat.RGBA32, false, true);
            Color[] pix = new Color[(Screen.width * Screen.height) / drawingTextureDownScalingRatio];
            //drawingTex.alphaIsTransparency = true;
            drawingTex.SetPixels(pix, 0);
            drawingTex.Apply(false);
        }

        public class EraserTool
        {
            public float width = 2f;
            public float hardness = 50;
        }

        public class BrushTool
        {
            public float width = 1f;
            public float hardness = 50f;
            public float spacing = 10;
        }

        [System.Serializable]
        public class UIOffsets
        {
            public Vector2 mainPainterBox = new Vector2(.05f, 0.05f);
            public Vector2 toolsbox = new Vector2(.025f, 0.025f);
            public float canvasBox = .2f;


            private Vector2 _canvas = new Vector2();
            [HideInInspector]
            public Vector2 canvas
            {
                get
                {
                    _canvas.Set(canvasBox, canvasBox * 0.5f);
                    return _canvas;
                }
                set { }
            }

        }
    }
}