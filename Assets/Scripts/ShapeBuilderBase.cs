using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VectorGraphics;

[RequireComponent(typeof(SpriteRenderer))]
public class ShapeBuilderBase : MonoBehaviour
{
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

    protected readonly Vector2 Vector2Zero = Vector2.zero;

    protected void DestroySpriteIfNeeded ()
    {
        #if UNITY_EDITOR
        if(spriteRenderer.sprite != null)
            DestroyImmediate(spriteRenderer.sprite);
        #else
        if(spriteRenderer.sprite != null)
            Destroy(spriteRenderer.sprite);
        #endif
    }

    protected List<VectorUtils.Geometry> BuildGeometry (Shape shape, VectorUtils.TessellationOptions options)
    {
        var node = new SceneNode() {
            Shapes = new List<Shape>  { shape }
        };
        var scene = new Scene() { Root = node };
        var geom = VectorUtils.TessellateScene(scene,options);
        return geom;
    }

    protected VectorUtils.TessellationOptions MakeShapeOptions (float stepDistance = float.MaxValue)
    {
        var options = new VectorUtils.TessellationOptions()
        {
            StepDistance =  stepDistance,
            MaxCordDeviation =  float.MaxValue,
            MaxTanAngleDeviation = Mathf.PI/2.0f,
            SamplingStepSize = 0.01f
        };

        return options;
    }

    protected VectorUtils.TessellationOptions MakeLineOptions(float stepDistance = float.MaxValue)
    {
        var options = new VectorUtils.TessellationOptions()
        {
            StepDistance =  stepDistance,
            MaxCordDeviation =  0.05f,
            MaxTanAngleDeviation = 0.05f,
            SamplingStepSize = 0.01f
        };

        return options;
    }
}
