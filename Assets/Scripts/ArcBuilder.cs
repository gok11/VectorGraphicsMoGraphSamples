using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.U2D;
using Unity.VectorGraphics;
using Unity.Collections;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ArcBuilder : MonoBehaviour
{
    [Header("____Main Props____")]
    [SerializeField] Color color = Color.black;

    [Range(-1080, 1080)]
    [SerializeField] float startAngleDeg = 0f;

    [Range(-1080, 1080)]
    [SerializeField] float endAngleDeg = 0f;

	[Range(3, 72)]
	[SerializeField] int arcSides = 3;
	[SerializeField] float arcRadius = 1f;

    [Header("____Mask Props____")]
    [SerializeField] float maskRadius = 0f;

    [Header("____General Props____")]
    [SerializeField] float svgPixelsPerUnit = 128;
	
    protected SpriteRenderer spriteRenderer 
    {
        get
        {
            if(_spriteRenderer) return _spriteRenderer;                        
            _spriteRenderer = GetComponent<SpriteRenderer>();
            return _spriteRenderer;
        }
    }
    SpriteRenderer _spriteRenderer = null;

    readonly Vector2 Vector2Zero = Vector2.zero;

    void Update() {
        #if UNITY_EDITOR
        if(spriteRenderer.sprite != null)
            DestroyImmediate(spriteRenderer.sprite);
        #else
        if(spriteRenderer.sprite != null)
            Destroy(spriteRenderer.sprite);
        #endif

        var sprite = BuildArcSprite();
        spriteRenderer.sprite = sprite;
	}

    Sprite BuildArcSprite()
    {
        ArcMeshData arcMeshData = MakeMeshData();

        var pivot = Vector2.one / 2;
        Texture2D texture = null;

        var correctedRadius = arcRadius * svgPixelsPerUnit / 2;
        var rect = new Rect(-correctedRadius, -correctedRadius, correctedRadius * 2, correctedRadius * 2);
        var spriteCreateMethod = typeof(Sprite).GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, new Type[] { typeof(Rect), typeof(Vector2), typeof(float), typeof(Texture2D) }, null);
        var sprite = spriteCreateMethod.Invoke(null, new object[] { rect, pivot, svgPixelsPerUnit, texture }) as Sprite;

        sprite.OverrideGeometry(arcMeshData.vertices.ToArray(), arcMeshData.triangles.Select(t => (UInt16)t).ToArray());

        var color32 = Enumerable.Repeat(color, arcMeshData.vertices.Length).Select(c => (Color32)c);
        using(var nativeColors = new NativeArray<Color32>(color32.ToArray(), Allocator.Temp))
            sprite.SetVertexAttribute<Color32>(VertexAttribute.Color, nativeColors);

        return sprite;
    }

    ArcMeshData MakeMeshData()
    {
        ArcMeshData top =    MakeArcMeshData(false);
        ArcMeshData bottom = MakeArcMeshData(true);

        for (int i = 0; i < bottom.triangles.Length; i++) {
            bottom.triangles[i] += top.vertices.Length;
        }

        List<int> tris = new List<int>();
        tris.AddRange(top.triangles);
        tris.AddRange(bottom.triangles);
        int[] _tris = tris.ToArray();

        List<Vector2> verts = new List<Vector2>();
        verts.AddRange(top.vertices);
        verts.AddRange(bottom.vertices);
        Vector2[] _verts = verts.ToArray();

        return new ArcMeshData() {
            vertices = _verts,
            triangles = _tris
        };
    }

    private class ArcMeshData {
        public Vector2[] vertices;
		public int[] triangles;
    }

    public static Vector2 GetAngleVectorByDegree(float angleDegree) {
		return new Vector2(Mathf.Sin(angleDegree * Mathf.Deg2Rad),Mathf.Cos(angleDegree * Mathf.Deg2Rad));
	}

    ArcMeshData MakeArcMeshData(bool reverse)
    {
        int vertexCount = arcSides * 2;
        int arcVertex = arcSides * 2 + 2;

        Vector2[] _vertices = new Vector2[arcVertex];
        int[] _triangles = new int[vertexCount * 3];

        float angleStep = (startAngleDeg - endAngleDeg) / (float)arcSides;
        int steps = arcSides + 1;

        var offset =  new Vector2(arcRadius * svgPixelsPerUnit / 2, arcRadius * svgPixelsPerUnit / 2);

        for (int i = 0; i < steps; i++)
        {
            float angle = endAngleDeg + i * angleStep;
            Vector2 dir = GetAngleVectorByDegree(angle);

            var correctedRadius = arcRadius * svgPixelsPerUnit / 2;
            var correctedMaskRadius = maskRadius * svgPixelsPerUnit / 2;
            Vector2 outer = dir * correctedRadius;
            Vector2 inner = outer - dir * (correctedMaskRadius > correctedRadius ? 0 : correctedRadius - correctedMaskRadius);
            
            _vertices[i * 2] = inner + offset;
            _vertices[i * 2 + 1] = outer + offset;
            int ti = i * 6; //triangle index

            {
                if (i < arcSides) {

                    if (reverse) {
                        
                        _triangles[ti]     = i * 2 + 1;
                        _triangles[ti + 1] = i * 2 + 0;
                        _triangles[ti + 2] = i * 2 + 2;

                        _triangles[ti + 3] = i * 2 + 3;
                        _triangles[ti + 4] = i * 2 + 1;
                        _triangles[ti + 5] = i * 2 + 2;

                    } else {
                        
                        _triangles[ti]     = i * 2 + 1;
                        _triangles[ti + 1] = i * 2 + 2;
                        _triangles[ti + 2] = i * 2 + 0;

                        _triangles[ti + 3] = i * 2 + 3;
                        _triangles[ti + 4] = i * 2 + 2;
                        _triangles[ti + 5] = i * 2 + 1;
                    }
                }

            }
        }

        ArcMeshData info = new ArcMeshData();
        info.triangles = _triangles;
        info.vertices = _vertices;
        return info;
    }

    // ↓ 今後 MakeArcShape ができたら使えるかも
    // private BezierContour BuildContour()
    // {
    //     var arc = VectorUtils.MakeArc(Vector2Zero,
    //                 startAngleDeg * Mathf.Deg2Rad,
    //                 endAngleDeg * Mathf.Deg2Rad, radius);
    //     return new BezierContour() { Segments = arc };
    // }

    // protected List<VectorUtils.Geometry> BuildGeometry(Shape shape, float stepDistance = float.MaxValue)
    // {
    //     var node = new SceneNode() {
    //         Shapes = new List<Shape>  { shape }
    //     };
    //     var scene = new Scene() { Root = node };

    //     var options = new VectorUtils.TessellationOptions()
    //     {
    //         StepDistance =  stepDistance,
    //         MaxCordDeviation =  float.MaxValue,
    //         MaxTanAngleDeviation = Mathf.PI/2.0f,
    //         SamplingStepSize = 0.01f
    //     };

    //     var geom = VectorUtils.TessellateScene(scene,options);
    //     return geom;
    // }

    // // Update is called once per frame
    // void Update()
    // {
    //     var shape = new Shape();
    //     shape.Contours = new BezierContour[1];
    //     var arc = VectorUtils.MakeArc(Vector2Zero,
    //                 startAngleDeg * Mathf.Deg2Rad,
    //                 endAngleDeg * Mathf.Deg2Rad, radius);
    //     shape.Contours[0] = new BezierContour() { Segments = arc };

    //     shape.IsConvex = true;
    //     shape.Fill = new SolidFill()  { Color = color };

    //     var geo = BuildGeometry(shape, stepDistance);
    //     var sprite = VectorUtils.BuildSprite(
    //                 geo, 100.0f,
    //                 VectorUtils.Alignment.Center,
    //                 Vector2.zero, 128);
    //     spriteRenderer.sprite = sprite;
    // }
}
