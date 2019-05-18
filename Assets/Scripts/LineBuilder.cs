using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VectorGraphics;

[ExecuteInEditMode]
public class LineBuilder : ShapeBuilderBase
{
    [SerializeField] Vector2 from = Vector2.zero;
    [SerializeField] Vector2 to = Vector2.zero;

    [SerializeField] Color pathColor = Color.white;
    [SerializeField] float pathHalfThickness = 0.1f;
    [SerializeField] float stepDistance = 10f;


    // Update is called once per frame
    void Update()
    {
        DestroySpriteIfNeeded();

        var lineSegment = VectorUtils.MakeLine(from, to);
        var lineSegmentPath = VectorUtils.BezierSegmentToPath(lineSegment);

        var path = new Shape() {
            Contours = new BezierContour[]{ new BezierContour() { Segments = lineSegmentPath } },
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
