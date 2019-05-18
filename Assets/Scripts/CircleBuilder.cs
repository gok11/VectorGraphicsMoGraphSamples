using UnityEngine;
using Unity.VectorGraphics;

[ExecuteInEditMode]
public class CircleBuilder : ShapeBuilderBase
{
    [Header("____Main Props____")]
    [SerializeField] Color color = Color.black;
    [SerializeField] float radiusX = 0f;
    [SerializeField] float radiusY = 0f;

    [Header("____Mask Props____")]
    [SerializeField] float maskX = 0f;
    [SerializeField] float maskY = 0f;

    [Header("____General Props____")]
    [SerializeField] float stepDistance = 10f;

    private BezierContour[] BuildCircleContourWithMask(Rect rect, Vector2 rad, Rect maskRect, Vector2 maskRad)
    {
        var contours = new BezierContour[2];
        contours[0] = VectorUtils.BuildRectangleContour(rect, rad, rad, rad, rad);
        contours[1] = VectorUtils.BuildRectangleContour(maskRect, maskRad, maskRad, maskRad, maskRad);

        return contours;
    }

    // Update is called once per frame
    void Update()
    {
        DestroySpriteIfNeeded();

        var shape = new Shape();

        var rect1 = new Rect(-radiusX, -radiusY, radiusX+radiusX, radiusY+radiusY);
        var rad1 = new Vector2(radiusX, radiusY);

        var rect2 = new Rect(-maskX, -maskY, maskX+maskX, maskY+maskY);
        var rad2 = new Vector2(maskX, maskY);

        shape.Contours = BuildCircleContourWithMask(rect1, rad1, rect2, rad2);

        shape.IsConvex = true;
        shape.Fill = new SolidFill() { Color = color, Mode = FillMode.OddEven };

        var options = MakeShapeOptions(stepDistance);
        var geo = BuildGeometry(shape, options);
        var sprite = VectorUtils.BuildSprite(
                            geo, 100.0f,
                            VectorUtils.Alignment.Center,
                            Vector2.zero, 128);
        spriteRenderer.sprite = sprite;
    }
}
