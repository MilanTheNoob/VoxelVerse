using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkObject : MonoBehaviour
{
    [HideInInspector] public intVector2 Coord;
    [HideInInspector] public int lod = 1;

    public BlockType[,,] blocks = new BlockType[18, 255, 18];

    public MeshFilter meshFilter;
    public MeshCollider meshCollider;

    public void BuildMesh()
    {
        Mesh mesh = new Mesh();

        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        for(int x = 1; x < 17; x += lod)
        {
            for (int z = 1; z < 17; z += lod)
            {
                for (int y = 0; y < 255; y++)
                {
                    if (blocks[x, y, z] != BlockType.Air)
                    {
                        Vector3 blockPos = new Vector3(x - 1, y, z - 1);
                        int numFaces = 0;

                        // Top
                        try
                        {
                            if (y < 254 && blocks[x, y + lod, z] == BlockType.Air)
                            {
                                AddToVerts(blockPos, nv(0, lod, 0), nv(0, lod, lod), nv(lod, lod, lod), nv(lod, lod, 0), x, y, z);
                                numFaces++;
                            }
                        }
                        catch
                        {
                            AddToVerts(blockPos, nv(0, lod, 0), nv(0, lod, lod), nv(lod, lod, lod), nv(lod, lod, 0), x, y, z);
                            numFaces++;
                        }

                        // Bottom
                        try
                        {
                            if (y > 0 && blocks[x, y - lod, z] == BlockType.Air)
                            {
                                AddToVerts(blockPos, nv(0, 0, 0), nv(lod, 0, 0), nv(lod, 0, lod), nv(0, 0, lod), x, y, z);
                                numFaces++;
                            }
                        }
                        catch
                        {
                            AddToVerts(blockPos, nv(0, 0, 0), nv(lod, 0, 0), nv(lod, 0, lod), nv(0, 0, lod), x, y, z);
                            numFaces++;
                        }

                        // Front
                        try
                        {
                            if (blocks[x, y, z - lod] == BlockType.Air)
                            {
                                AddToVerts(blockPos, nv(0, 0, 0), nv(0, lod, 0), nv(lod, lod, 0), nv(lod, 0, 0), x, y, z);
                                numFaces++;
                            }
                        }
                        catch
                        {
                            AddToVerts(blockPos, nv(0, 0, 0), nv(0, lod, 0), nv(lod, lod, 0), nv(lod, 0, 0), x, y, z);
                            numFaces++;
                        }

                        // Right
                        try
                        {
                            if (blocks[x + lod, y, z] == BlockType.Air)
                            {
                                AddToVerts(blockPos, nv(lod, 0, 0), nv(lod, lod, 0), nv(lod, lod, lod), nv(lod, 0, lod), x, y, z);
                                numFaces++;
                            }
                        }
                        catch
                        {
                            AddToVerts(blockPos, nv(lod, 0, 0), nv(lod, lod, 0), nv(lod, lod, lod), nv(lod, 0, lod), x, y, z);
                            numFaces++;
                        }

                        // Back
                        try
                        {
                            if (blocks[x, y, z + lod] == BlockType.Air)
                            {
                                AddToVerts(blockPos, nv(lod, 0, lod), nv(lod, lod, lod), nv(0, lod, lod), nv(0, 0, lod), x, y, z);
                                numFaces++;
                            }
                        }
                        catch
                        {
                            AddToVerts(blockPos, nv(lod, 0, lod), nv(lod, lod, lod), nv(0, lod, lod), nv(0, 0, lod), x, y, z);
                            numFaces++;
                        }

                        // Left
                        try
                        {
                            if (blocks[x - lod, y, z] == BlockType.Air)
                            {
                                AddToVerts(blockPos, nv(0, 0, lod), nv(0, lod, lod), nv(0, lod, 0), nv(0, 0, 0), x, y, z);
                                numFaces++;
                            }
                        }
                        catch
                        {
                            AddToVerts(blockPos, nv(0, 0, lod), nv(0, lod, lod), nv(0, lod, 0), nv(0, 0, 0), x, y, z);
                            numFaces++;
                        }


                        int tl = verts.Count - 4 * numFaces;
                        for (int i = 0; i < numFaces; i++)
                        {
                            tris.AddRange(new int[] { tl + i * 4, tl + i * 4 + 1, tl + i * 4 + 2, tl + i * 4, tl + i * 4 + 2, tl + i * 4 + 3 });
                        }
                    }
                }
            }
        }

        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.name = transform.position.x + ", " + transform.position.z;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        Vector3 nv(float x, float y, float z) { return new Vector3(x, y, z); }

        void AddToVerts(Vector3 blockPos, Vector3 one, Vector3 two, Vector3 three, Vector3 four, int x, int y, int z)
        {
            verts.Add(blockPos + one);
            verts.Add(blockPos + two);
            verts.Add(blockPos + three);
            verts.Add(blockPos + four);

            uvs.AddRange(Block.blocks[blocks[x, y, z]].GetUVs());
        }
    }

    public void ResetBlocks() { blocks = new BlockType[18, 255, 18]; }
    public void SetupChunk(int lod) { meshFilter = GetComponent<MeshFilter>(); meshCollider = GetComponent<MeshCollider>(); this.lod = lod; }
}
