using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VectorGraphics;

[ExecuteInEditMode]
public class SplineBuilder : ShapeBuilderBase
{
    [System.Serializable]
    private struct SegmentInfo
    {
        public Vector2 P0, P1, P2;

        public SegmentInfo(Vector2 p0, Vector2 p1, Vector2 p2) {
            P0 = p0;
            P1 = p1;
            P2 = p2;
        }
    }

    [SerializeField]  SegmentInfo[] segmentInfo = null;
    [SerializeField] Color pathColor = Color.white;
    [SerializeField] float pathHalfThickness = 0.1f;
    [SerializeField] float stepDistance = 10f;

    // Update is called once per frame
    void Update()
    {
        DestroySpriteIfNeeded();

        var segments = new BezierPathSegment[segmentInfo.Length];
        for (int i = 0; i < segments.Length; ++i) {
            segments[i].P0 = segmentInfo[i].P0;
            segments[i].P1 = segmentInfo[i].P1;
            segments[i].P2 = segmentInfo[i].P2;
        }

        var path = new Shape() {
            Contours = new BezierContour[]{ new BezierContour() { Segments = segments } },
            PathProps = new PathProperties() {
                Stroke = new Stroke() { Color = pathColor, HalfThickness = pathHalfThickness }
            }
        };

        var options = MakeLineOptions(stepDistance);
        var geo = BuildGeometry(path, options);
        var sprite = VectorUtils.BuildSprite(
                            geo, 1.0f,
                            VectorUtils.Alignment.Center,
                            Vector2.zero, 128);
        spriteRenderer.sprite = sprite;
    }
}
