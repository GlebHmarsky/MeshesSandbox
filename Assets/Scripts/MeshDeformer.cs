using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour
{
  Mesh deformingMesh;
  Vector3[] originalVertices, displacedVertices;
  Vector3[] vertexVelocities;

  void Start()
  {
    deformingMesh = GetComponent<MeshFilter>().mesh;
    originalVertices = deformingMesh.vertices;
    displacedVertices = new Vector3[originalVertices.Length];
    Array.Copy(originalVertices, displacedVertices, originalVertices.Length);

    vertexVelocities = new Vector3[originalVertices.Length];
    // for (int i = 0; i < originalVertices.Length; i++)
    // {
    //   displacedVertices[i] = originalVertices[i];
    // }
  }
}
