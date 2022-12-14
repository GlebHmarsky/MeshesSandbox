using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CubeGen : MonoBehaviour
{
  public int xSize, ySize, zSize;
  public float roundness;

  private Mesh mesh;
  private Vector3[] vertices;
  private Vector3[] normals;
  private Color32[] cubeUV;

  private void Awake()
  {
    Generate();
  }

  private void Generate()

  {
    GetComponent<MeshFilter>().mesh = mesh = new Mesh();
    mesh.name = "Procedural Cube";

    CreateVertices();
    CreateTriangles();
    CreateColliders();
    mesh.RecalculateNormals();
    mesh.normals = mesh.normals.Select(normal => normal * -1).ToArray();
  }

  private void CreateVertices()
  {
    int cornerVertices = 8;
    int edgeVertices = (xSize + ySize + zSize - 3) * 4;
    int faceVertices = (
        (xSize - 1) * (ySize - 1) +
        (xSize - 1) * (zSize - 1) +
        (ySize - 1) * (zSize - 1)) * 2;
    vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
    normals = new Vector3[vertices.Length];
    cubeUV = new Color32[vertices.Length];

    int v = 0;
    for (int y = 0; y <= ySize; y++)
    {
      for (int x = 0; x <= xSize; x++)
      {
        SetVertex(v++, x, y, 0);
      }
      for (int z = 1; z <= zSize; z++)
      {
        SetVertex(v++, xSize, y, z);
      }
      for (int x = xSize - 1; x >= 0; x--)
      {
        SetVertex(v++, x, y, zSize);
      }
      for (int z = zSize - 1; z > 0; z--)
      {
        SetVertex(v++, 0, y, z);
      }
    }

    for (int z = 1; z < zSize; z++)
    {
      for (int x = 1; x < xSize; x++)
      {
        SetVertex(v++, x, ySize, z);
      }
    }
    for (int z = 1; z < zSize; z++)
    {
      for (int x = 1; x < xSize; x++)
      {
        SetVertex(v++, x, 0, z);
      }
    }

    mesh.vertices = vertices;
    mesh.normals = normals;
    mesh.colors32 = cubeUV;
  }

  private void SetVertex(int i, int x, int y, int z)
  {
    Vector3 inner = vertices[i] = new Vector3(x, y, z);

    if (x < roundness)
    {
      inner.x = roundness;
    }
    else if (x > xSize - roundness)
    {
      inner.x = xSize - roundness;
    }
    if (y < roundness)
    {
      inner.y = roundness;
    }
    else if (y > ySize - roundness)
    {
      inner.y = ySize - roundness;
    }
    if (z < roundness)
    {
      inner.z = roundness;
    }
    else if (z > zSize - roundness)
    {
      inner.z = zSize - roundness;
    }
    // Double genius lines about how to point normalized vector on spheare with radius = `roundness`
    normals[i] = (vertices[i] - inner).normalized;
    vertices[i] = inner + normals[i] * roundness;
    cubeUV[i] = new Color32((byte)x, (byte)y, (byte)z, 0);

  }

  private void CreateTriangles()
  {
    int[] trianglesZ = new int[(xSize * ySize) * 12];
    int[] trianglesX = new int[(ySize * zSize) * 12];
    int[] trianglesY = new int[(xSize * zSize) * 12];
    int ring = (xSize + zSize) * 2;
    int tZ = 0, tX = 0, tY = 0, quadIndex = 0;


    for (int y = 0; y < ySize; y++, quadIndex++)
    {
      for (int q = 0; q < xSize; q++, quadIndex++)
      {
        tZ = SetQuad(trianglesZ, tZ, quadIndex, quadIndex + 1, quadIndex + ring, quadIndex + ring + 1);
      }
      for (int q = 0; q < zSize; q++, quadIndex++)
      {
        tX = SetQuad(trianglesX, tX, quadIndex, quadIndex + 1, quadIndex + ring, quadIndex + ring + 1);
      }
      for (int q = 0; q < xSize; q++, quadIndex++)
      {
        tZ = SetQuad(trianglesZ, tZ, quadIndex, quadIndex + 1, quadIndex + ring, quadIndex + ring + 1);
      }
      for (int q = 0; q < zSize - 1; q++, quadIndex++)
      {
        tX = SetQuad(trianglesX, tX, quadIndex, quadIndex + 1, quadIndex + ring, quadIndex + ring + 1);
      }
      tX = SetQuad(trianglesX, tX, quadIndex, quadIndex - ring + 1, quadIndex + ring, quadIndex + 1);
    }

    tY = CreateTopFace(trianglesY, tY, ring);
    tY = CreateBottomFace(trianglesY, tY, ring);

    mesh.subMeshCount = 3;
    mesh.SetTriangles(trianglesZ, 0);
    mesh.SetTriangles(trianglesX, 1);
    mesh.SetTriangles(trianglesY, 2);
  }

  private int CreateTopFace(int[] triangles, int ti, int ring)
  {
    // int ring = (xSize + zSize) * 2;
    int quadIndex = ring * ySize;

    // This is for closest(by index) border on top 
    for (int x = 0; x < xSize - 1; x++, quadIndex++)
    {
      ti = SetQuad(triangles, ti, quadIndex, quadIndex + 1, quadIndex + ring - 1, quadIndex + ring);
    }
    ti = SetQuad(triangles, ti, quadIndex, quadIndex + 1, quadIndex + ring - 1, quadIndex + 2);


    int vMin = ring * (ySize + 1) - 1;
    int vMid = vMin + 1;
    int vMax = quadIndex + 2;

    // This is for middle fill up
    for (int z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++)
    {
      ti = SetQuad(triangles, ti, vMin, vMid, vMin - 1, vMid + xSize - 1);
      for (int x = 1; x < xSize - 1; x++, vMid++)
      {
        ti = SetQuad(
            triangles, ti,
            vMid, vMid + 1, vMid + xSize - 1, vMid + xSize);
      }
      ti = SetQuad(triangles, ti, vMid, vMax, vMid + xSize - 1, vMax + 1);
    }

    int vTop = vMin - 2;
    // And this is for far'est border
    ti = SetQuad(triangles, ti, vMin, vMid, vTop + 1, vTop);
    for (int x = 1; x < xSize - 1; x++, vTop--, vMid++)
    {
      ti = SetQuad(triangles, ti, vMid, vMid + 1, vTop, vTop - 1);
    }
    ti = SetQuad(triangles, ti, vMid, vTop - 2, vTop, vTop - 1);
    return ti;
  }

  private int CreateBottomFace(int[] triangles, int t, int ring)
  {
    int v = 1;
    int vMid = vertices.Length - (xSize - 1) * (zSize - 1);
    t = SetQuad(triangles, t, ring - 1, vMid, 0, 1);
    for (int x = 1; x < xSize - 1; x++, v++, vMid++)
    {
      t = SetQuad(triangles, t, vMid, vMid + 1, v, v + 1);
    }
    t = SetQuad(triangles, t, vMid, v + 2, v, v + 1);

    int vMin = ring - 2;
    vMid -= xSize - 2;
    int vMax = v + 2;

    for (int z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++)
    {
      t = SetQuad(triangles, t, vMin, vMid + xSize - 1, vMin + 1, vMid);
      for (int x = 1; x < xSize - 1; x++, vMid++)
      {
        t = SetQuad(
            triangles, t,
            vMid + xSize - 1, vMid + xSize, vMid, vMid + 1);
      }
      t = SetQuad(triangles, t, vMid + xSize - 1, vMax + 1, vMid, vMax);
    }

    int vTop = vMin - 1;
    t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
    for (int x = 1; x < xSize - 1; x++, vTop--, vMid++)
    {
      t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
    }
    t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);

    return t;
  }

  private static int SetQuad(int[] triangles, int i, int v00, int v10, int v01, int v11)
  {
    triangles[i] = v00;
    triangles[i + 1] = triangles[i + 4] = v01;
    triangles[i + 2] = triangles[i + 3] = v10;
    triangles[i + 5] = v11;
    return i + 6;
  }

  private void CreateColliders()
  {
    // For economy of resources we should no add a last of box colliders capsule collider
    if (roundness == 0)
    {
      AddBoxCollider(xSize, ySize, zSize);
      return;
    }

    AddBoxCollider(xSize, ySize - roundness * 2, zSize - roundness * 2);
    AddBoxCollider(xSize - roundness * 2, ySize, zSize - roundness * 2);
    AddBoxCollider(xSize - roundness * 2, ySize - roundness * 2, zSize);

    Vector3 min = Vector3.one * roundness;
    Vector3 half = new Vector3(xSize, ySize, zSize) * 0.5f;
    Vector3 max = new Vector3(xSize, ySize, zSize) - min;

    AddCapsuleCollider(0, half.x, min.y, min.z);
    AddCapsuleCollider(0, half.x, min.y, max.z);
    AddCapsuleCollider(0, half.x, max.y, min.z);
    AddCapsuleCollider(0, half.x, max.y, max.z);

    AddCapsuleCollider(1, min.x, half.y, min.z);
    AddCapsuleCollider(1, min.x, half.y, max.z);
    AddCapsuleCollider(1, max.x, half.y, min.z);
    AddCapsuleCollider(1, max.x, half.y, max.z);

    AddCapsuleCollider(2, min.x, min.y, half.z);
    AddCapsuleCollider(2, min.x, max.y, half.z);
    AddCapsuleCollider(2, max.x, min.y, half.z);
    AddCapsuleCollider(2, max.x, max.y, half.z);
  }

  private void AddBoxCollider(float x, float y, float z)
  {
    BoxCollider collider = gameObject.AddComponent<BoxCollider>();
    collider.size = new Vector3(x, y, z);
  }

  private void AddCapsuleCollider(int direction, float x, float y, float z)
  {
    CapsuleCollider collider = gameObject.AddComponent<CapsuleCollider>();
    collider.center = new Vector3(x, y, z);
    collider.direction = direction;
    collider.radius = roundness;
    collider.height = collider.center[direction] * 2f;
  }

  // private void OnDrawGizmos()
  // {
  //   if (vertices == null)
  //   {
  //     return;
  //   }
  //   Gizmos.color = Color.black;
  //   for (int i = 0; i < vertices.Length; i++)
  //   {
  //     Gizmos.color = Color.black;
  //     Gizmos.DrawSphere(vertices[i], 0.1f);
  //     Gizmos.color = Color.yellow;
  //     Gizmos.DrawRay(vertices[i], normals[i]);
  //   }
  // }
}
