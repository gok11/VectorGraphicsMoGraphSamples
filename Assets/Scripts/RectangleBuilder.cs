using UnityEngine;
using Unity.VectorGraphics;

[ExecuteInEditMode]
public class RectangleBuilder : ShapeBuilderBase
{
    [Header("____Main Props_____")]
    [SerializeField] Color color = Color.black;

    [SerializeField] float height = 0;
    [SerializeField] float width = 0;

    [Header("____Mask Props____")]
    [SerializeField] float maskHeight = 0;
    [SerializeField] float maskWidth = 0;

    [Header("____General Props____")]
    [SerializeField] float svgPixelsPerUnit = 100f;

    private BezierContour[] BuildRectangleContourWithMask(Rect rect, Rect maskRect)
    {
        // ToDo: Clamp [maskRect] by [rect]

        var contours = new BezierContour[2];
        contours[0] = VectorUtils.BuildRectangleContour(rect, Vector2Zero, Vector2Zero, Vector2Zero, Vector2Zero);
        contours[1] = VectorUtils.BuildRectangleContour(maskRect, Vector2Zero, Vector2Zero, Vector2Zero, Vector2Zero);
        return contours;
    }

    // Update is called once per frame
    void Update()
    {
        DestroySpriteIfNeeded();

        var shape = new Shape();
        shape.Contours = BuildRectangleContourWithMask(new Rect(0, 0, width, height), new Rect(width/2 - maskWidth/2, height/2 - maskHeight/2, maskWidth, maskHeight));
        shape.IsConvex = true;
        shape.Fill = new SolidFill() { Color = color, Mode = FillMode.OddEven };

        var options = MakeShapeOptions();
        var geo = BuildGeometry(shape, options);
        var sprite = VectorUtils.BuildSprite(
                            geo, svgPixelsPerUnit,
                            VectorUtils.Alignment.Center,
                            Vector2.zero, 128);
        spriteRenderer.sprite = sprite;
    }
}
