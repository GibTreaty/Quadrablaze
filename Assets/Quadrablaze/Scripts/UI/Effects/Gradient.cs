using System.Collections.Generic;

/// <summary>
/// Modified Gradient effect script from http://answers.unity3d.com/questions/1086415/gradient-text-in-unity-522-basevertexeffect-is-obs.html
/// -Uses Unity's Gradient class to define the color
/// -Offset is now limited to -1,1
/// -Multiple color blend modes
/// 
/// Remember that the colors are applied per-vertex so if you have multiple points on your gradient where the color changes and there aren't enough vertices, you won't see all of the colors.
/// </summary>
namespace UnityEngine.UI {
    [AddComponentMenu("UI/Effects/Gradient")]
    public class Gradient : BaseMeshEffect {
        [SerializeField]
        Type _gradientType;

        [SerializeField]
        float _angle;

        [SerializeField]
        Blend _blendMode = Blend.Multiply;

        [SerializeField]
        [Range(-1, 1)]
        float _offset = 0f;

        [SerializeField]
        UnityEngine.Gradient _effectGradient = new UnityEngine.Gradient() { colorKeys = new GradientColorKey[] { new GradientColorKey(Color.black, 0), new GradientColorKey(Color.white, 1) } };

        #region Properties
        public Blend BlendMode {
            get { return _blendMode; }
            set { _blendMode = value; }
        }

        public UnityEngine.Gradient EffectGradient {
            get { return _effectGradient; }
            set { _effectGradient = value; }
        }

        public Type GradientType {
            get { return _gradientType; }
            set { _gradientType = value; }
        }

        public float Offset {
            get { return _offset; }
            set { _offset = value; }
        }

        public float Angle {
            get {
                return _angle;
            }

            set {
                _angle = value;
            }
        }
        #endregion

        public override void ModifyMesh(VertexHelper helper) {
            if(!IsActive() || helper.currentVertCount == 0)
                return;

            List<UIVertex> _vertexList = new List<UIVertex>();

            helper.GetUIVertexStream(_vertexList);

            //int vertexCount = _vertexList.Count;

            //Vector2 minVertexPosition = _vertexList[0].position;
            //Vector2 maxVertexPosition = _vertexList[0].position;
            //Vector2 position = Vector2.zero;

            //for(int i = vertexCount - 1; i >= 1; i--) {
            //    position = _vertexList[i].position;

            //    if(minVertexPosition.x < position.x) minVertexPosition.x = position.x;
            //    else if(maxVertexPosition.x > position.x) maxVertexPosition.x = position.x;

            //    if(minVertexPosition.y < position.y) minVertexPosition.y = position.y;
            //    else if(maxVertexPosition.y > position.y) maxVertexPosition.y = position.y;
            //}

            //return;

            int nCount = _vertexList.Count;
            switch(GradientType) {
                case Type.Horizontal: {
                    float left = _vertexList[0].position.x;
                    float right = _vertexList[0].position.x;
                    float x = 0f;

                    for(int i = nCount - 1; i >= 1; --i) {
                        x = _vertexList[i].position.x;

                        if(x > right) right = x;
                        else if(x < left) left = x;
                    }

                    float width = 1f / (right - left);
                    UIVertex vertex = new UIVertex();

                    for(int i = 0; i < helper.currentVertCount; i++) {
                        helper.PopulateUIVertex(ref vertex, i);

                        vertex.color = BlendColor(vertex.color, EffectGradient.Evaluate((vertex.position.x - left) * width - Offset));

                        helper.SetUIVertex(vertex, i);
                    }
                }
                break;

                case Type.Vertical: {
                    float bottom = _vertexList[0].position.y;
                    float top = _vertexList[0].position.y;
                    float y = 0f;

                    for(int i = nCount - 1; i >= 1; --i) {
                        y = _vertexList[i].position.y;

                        if(y > top) top = y;
                        else if(y < bottom) bottom = y;
                    }

                    float height = 1f / (top - bottom);
                    UIVertex vertex = new UIVertex();

                    for(int i = 0; i < helper.currentVertCount; i++) {
                        helper.PopulateUIVertex(ref vertex, i);

                        vertex.color = BlendColor(vertex.color, EffectGradient.Evaluate((vertex.position.y - bottom * height - Offset)));

                        helper.SetUIVertex(vertex, i);
                    }
                }
                break;
            }
        }

        Color BlendColor(Color colorA, Color colorB) {
            switch(BlendMode) {
                default: return colorB;
                case Blend.Add: return colorA + colorB;
                case Blend.Multiply: return colorA * colorB;
                case Blend.Screen:
                    Color white = new Color(1, 1, 1, 0);
                    Color a = new Color(colorA.r, colorA.g, colorA.b, 0);
                    Color b = new Color(colorB.r, colorB.g, colorB.b, 0);
                    float alpha = colorA.a;

                    colorA = white - (white - a) * (white - b);
                    colorA.a = alpha;

                    return colorA;
            }
        }

        public enum Type {
            Horizontal,
            Vertical
        }

        public enum Blend {
            Override,
            Add,
            Multiply,
            Screen
        }
    }
}