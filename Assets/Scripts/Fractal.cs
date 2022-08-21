using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fractal : MonoBehaviour
{
    public int maxDepth;
    private int depth;
    public Mesh mesh;
    public Material material;
    public float childScale;


    class FractialDirection
    {
        public Vector3 direction { get; set; }
        public Quaternion orientation { get; set; }
    }

    private static FractialDirection[] fractioalDirections = {
        new FractialDirection {direction= Vector3.up, orientation=Quaternion.identity},
        new FractialDirection {direction= Vector3.right, orientation=Quaternion.Euler(0f, 0f, -90f)},
        new FractialDirection {direction= Vector3.left, orientation=Quaternion.Euler(0f, 0f, 90f)},
        new FractialDirection {direction= Vector3.forward, orientation=Quaternion.Euler(90f, 0f, 0f)},
        new FractialDirection {direction= Vector3.back, orientation=Quaternion.Euler(-90f, 0f, 0f)},
    };
    private Material[] materials;

    private void InitializeMaterials()
    {
        materials = new Material[maxDepth + 1];
        for (int i = 0; i <= maxDepth; i++)
        {
            materials[i] = new Material(material);
            materials[i].color =
                Color.Lerp(Color.white, Color.yellow, (float)i / maxDepth);
        }
    }

    private void Start()
    {
        if (materials == null)
        {
            InitializeMaterials();
        }
        gameObject.AddComponent<MeshFilter>().mesh = mesh;
        gameObject.AddComponent<MeshRenderer>().material = materials[depth];
        GetComponent<MeshRenderer>().material.color =
            Color.Lerp(Color.white, Color.yellow, (float)depth / maxDepth);
        if (depth >= maxDepth) return;

        StartCoroutine(CreateChildren());
    }

    private IEnumerator CreateChildren()
    {
        foreach (var fd in fractioalDirections)
        {
            yield return new WaitForSeconds(0.2f);
            new GameObject("Fractal Child").AddComponent<Fractal>().
             Initialize(this, fd.direction, fd.orientation);
        }
    }

    private void Initialize(Fractal parent, Vector3 direction, Quaternion orientation)
    {

        //foreach (PropertyInfo property in typeof(Fractal).GetProperties().Where(p => p.CanWrite))
        //{
        //    property.SetValue(targetObject, property.GetValue(sourceObject, null), null);
        //}

        mesh = parent.mesh;
        material = parent.material;
        materials = parent.materials;
        maxDepth = parent.maxDepth;
        depth = parent.depth + 1;
        childScale = parent.childScale;

        transform.parent = parent.transform;
        transform.position = parent.transform.position;

        transform.localScale = Vector3.one * childScale;
        transform.localPosition = direction * (0.5f + 0.5f * childScale);
        transform.localRotation = orientation;
    }
}
