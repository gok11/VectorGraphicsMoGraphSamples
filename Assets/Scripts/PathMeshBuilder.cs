using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VectorGraphics;
using System.Linq;
using System;
using System.Reflection;

///  !!! WIP Component !!!
public class PathMeshBuilder : MonoBehaviour
{
    private List<Vector2> points = null;
    [SerializeField] float appendDistance = 0.5f;
    private float appendSqrDistance;

    [SerializeField] bool keepPointLength;

    private struct Section
    {
        public Vector3 direction;
        public Vector3 left;
        public Vector3 right;
    }

    private Section[] sections = null;
    private float meshHalfWidth = 1f;

    private SpriteMask mask;

    void Awake()
    {
        mask = GetComponent<SpriteMask>();
        appendSqrDistance = Mathf.Pow(appendDistance, 2);
    }

    // Update is called once per frame
    void Update()
    {
        if(!transform.hasChanged) return;
        UpdatePoints();
        UpdateVectors();

        var sprite = BuildSprite();
        if(sprite == null) return;

        mask.sprite = sprite;
    }

    void UpdatePoints()
    {
        if(points == null) {
            points = new  List<Vector2>();
            points.Add(transform.position);
        }

        Vector2 curPos = transform.position;
        var distance = (curPos - points[points.Count - 1]);
        if (distance.sqrMagnitude >= appendSqrDistance)
        {
            points.Add(curPos);
        }
    }

    void UpdateVectors()
    {
        if (points == null || points.Count <= 1) return;

        sections = new Section[points.Count];

        for (int i = 0; i < points.Count; i++)
        {
            if(i == 0)
            {
                // 始点の場合.
                sections[i].direction = points[i + 1] - points[i];
            }
            else if (i == points.Count - 1)
            {
                // 終点の場合.
                sections[i].direction = points[i] - points[i - 1];
            }
            else
            {
                sections[i].direction = points[i + 1] - points[i - 1];
            }

            sections[i].direction.Normalize();

            Vector2 side = Quaternion.AngleAxis(90f, -Vector3.forward) * sections[i].direction;
            side.Normalize();

            sections[i].left = points[i] - side * meshHalfWidth / 2f;
            sections[i].right = points[i] + side * meshHalfWidth / 2f;
        }
    }

    Sprite BuildSprite()
    {
        if (points == null || points.Count <= 1) return null;
        
        int meshCount = points.Count - 1;

        Vector2[] vertices = new Vector2[(meshCount) * 4];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[(meshCount) * 2 * 3];

        for (int i = 0; i < meshCount; i++)
        {
            vertices[i * 4 + 0] = sections[i].left;
            vertices[i * 4 + 1] = sections[i].right;
            vertices[i * 4 + 2] = sections[i + 1].left;
            vertices[i * 4 + 3] = sections[i + 1].right;

            var step = (float)1 / meshCount;

            uvs[i * 4 + 0] = new Vector2(0f, i * step);
            uvs[i * 4 + 1] = new Vector2(1f, i * step);
            uvs[i * 4 + 2] = new Vector2(0f, (i + 1) * step);
            uvs[i * 4 + 3] = new Vector2(1f, (i + 1) * step);
        }

        var pivot = Vector2.one / 2;
        Texture2D texture = null;

        var xMin = vertices.Min(v => v.x);
        var xMax = vertices.Max(v => v.x);
        var yMin = vertices.Min(v => v.y);
        var yMax = vertices.Max(v => v.y);
        var rect = new Rect(xMin, yMin, xMax - xMin, yMax - yMin);

        int positionIndex = 0;

        for (int i = 0; i < meshCount; i++)
        {
            triangles[positionIndex++] = (i * 4) + 1;
            triangles[positionIndex++] = (i * 4) + 0;
            triangles[positionIndex++] = (i * 4) + 2;

            triangles[positionIndex++] = (i * 4) + 2;
            triangles[positionIndex++] = (i * 4) + 3;
            triangles[positionIndex++] = (i * 4) + 1;
        }

        var spriteCreateMethod = typeof(Sprite).GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, new Type[] { typeof(Rect), typeof(Vector2), typeof(float), typeof(Texture2D) }, null);
        var sprite = spriteCreateMethod.Invoke(null, new object[] { rect, pivot, 100, texture }) as Sprite;

        sprite.OverrideGeometry(vertices, triangles.Select(t => (UInt16)t).ToArray());
        return sprite;
    }
}
