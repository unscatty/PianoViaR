using System;
using System.Linq;
using BasicShapes;
using UnityEngine;


namespace PianoViaR.Score.Creation.Custom
{
    public static class ArrayExtensions
    {
        public static void Fill<T>(this T[] originalArray, T with)
        {
            for (int i = 0; i < originalArray.Length; i++)
            {
                originalArray[i] = with;
            }
        }
    }

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class Beam : MonoBehaviour
    {
        public Ellipse Face1 { get; set; }
        public Ellipse Face2 { get; set; }

        public Material material;

        public void Initialise(Ellipse face1, Ellipse face2)
        {
            Face1 = face1;
            Face2 = face2;
        }

        private void GenerateBeam()
        {
            var verticesFace1 = Face1.Vertices;
            var verticesFace2 = Face2.Vertices;

            var (newVertices, newIndexes) = ConnectFaces(verticesFace1, verticesFace2);

            var vertices = newVertices.Concat(verticesFace1).Concat(verticesFace2).ToArray();

            var indexesFace1 = TrianglesClosedShape(Face1.Sides, clockWise: false, offset: newIndexes.Length);
            var indexesFace2 = TrianglesClosedShape(Face2.Sides, offset: verticesFace1.Length + newIndexes.Length, clockWise: true);

            var indexes = newIndexes.Concat(indexesFace1).Concat(indexesFace2).ToArray();

            GenerateMesh(vertices, indexes);
        }

        void Start()
        {
            GenerateBeam();
        }

        private ValueTuple<Vector3[], int[]> ConnectFaces(Vector3[] verticesFace1, Vector3[] verticesFace2)
        {
            if (verticesFace1.Length != verticesFace2.Length)
            {
                throw new ArgumentException("Both vertices must have the same length");
            }

            var sides = verticesFace1.Length;

            var numVertices = sides * 6;
            var trianglesIdxs = new int[numVertices];
            var newVertices = new Vector3[numVertices];
            // trianglesIdxs.Fill(0);

            int count = 0;
            for (int /* count = 0, */ face1Idx = 0, face2Idx = 0, sideIdx = 0; sideIdx < sides - 1; count += 6, face1Idx++, face2Idx++, sideIdx++)
            {
                // Triangle with base in face1
                newVertices[count] = verticesFace1[face1Idx];
                newVertices[count + 1] = verticesFace1[face1Idx + 1];
                newVertices[count + 2] = verticesFace2[face2Idx];

                // Triangle with base in face2
                newVertices[count + 3] = verticesFace2[face2Idx + 1];
                newVertices[count + 4] = verticesFace2[face2Idx]; //% newVertices.Length;
                newVertices[count + 5] = verticesFace1[face1Idx + 1];
            }

            // int half = triangles / 2;

            count = numVertices - 6;


            newVertices[count] = verticesFace1[sides - 1];
            newVertices[count + 1] = verticesFace1[0];
            newVertices[count + 2] = verticesFace2[sides - 1];

            newVertices[count + 3] = verticesFace2[0];
            newVertices[count + 4] = verticesFace2[sides - 1];
            newVertices[count + 5] = verticesFace1[0];

            for (int i = 0; i < numVertices; i++)
            {
                trianglesIdxs[i] = i;
            }

            return ValueTuple.Create<Vector3[], int[]>(newVertices, trianglesIdxs);
        }

        private int[] TrianglesBetweenFaces(int sides, int off = 0)
        {
            var triangles = sides * 2;
            var trianglesIdxs = new int[triangles * 3];
            trianglesIdxs.Fill(0);

            int count = 0;
            for (int /* count = 0, */ face1Idx = 0, face2Idx = sides; count < trianglesIdxs.Length - 5 - 6 * off; count += 6, face1Idx++, face2Idx++)
            {
                // Triangle with base in face1
                trianglesIdxs[count] = face1Idx;
                trianglesIdxs[count + 1] = face1Idx + 1;
                trianglesIdxs[count + 2] = face2Idx;

                // Triangle with base in face2
                trianglesIdxs[count + 3] = face2Idx + 1;
                trianglesIdxs[count + 4] = face2Idx; //% trianglesIdxs.Length;
                trianglesIdxs[count + 5] = face1Idx + 1;
            }

            int half = triangles / 2;

            count -= 6;

            trianglesIdxs[count] = half - 1;
            trianglesIdxs[count + 1] = 0;
            trianglesIdxs[count + 2] = triangles - 1;

            trianglesIdxs[count + 3] = half;
            trianglesIdxs[count + 4] = triangles - 1;
            trianglesIdxs[count + 5] = 0;

            return trianglesIdxs;
        }

        private void GenerateMesh(Vector3[] vertices, int[] indexes, Vector3[] normals = null, string meshName = "Beam")
        {
            Mesh beamMesh = new Mesh();
            beamMesh.name = meshName;

            var meshFilter = this.GetComponent<MeshFilter>();
            var meshRenderer = this.GetComponent<MeshRenderer>();

            meshRenderer.material = material;

            meshFilter.mesh = beamMesh;

            beamMesh.vertices = vertices;
            beamMesh.triangles = indexes;
            beamMesh.RecalculateBounds();
            beamMesh.RecalculateNormals();
            beamMesh.Optimize();
        }

        int[] TrianglesClosedShape(int edges, int offset = 0, bool clockWise = true)
        {
            // Debug.Log($"Edges: {edges}");
            if (edges < 0)
            {
                throw new ArgumentException($"Argument edges must be positive, current value is : {edges}");
            }

            if (edges < 3)
            {
                throw new ArgumentException($"Argument edges must be greater than 3, current value is : {edges}");
            }

            int triangles = edges - 2;
            int[] trianglesIdxs = new int[triangles * 3];

            int firstIdxOffset = 2;
            int secondIdxOffset = 1;

            if (clockWise)
            {
                firstIdxOffset = 1;
                secondIdxOffset = 2;
            }


            for (int vertexIdx = 0, traingleIdx = offset, count = 0; count < triangles; vertexIdx += 3, traingleIdx++, count++)
            // for (int vertexIdx = 0, traingleIdx = offset, count = 0; count < triangles * 0.6; vertexIdx += 3, traingleIdx++, count++)
            {
                trianglesIdxs[vertexIdx] = offset;
                // Counter-clockwise render direction
                trianglesIdxs[vertexIdx + firstIdxOffset] = traingleIdx + 1;// + offsetTriangles;
                trianglesIdxs[vertexIdx + secondIdxOffset] = traingleIdx + 2;// + offsetTriangles;

                // // Clockwise render direction
                // triangles[vertexIdx + 2] = traingleIdx + 1;
                // triangles[vertexIdx + 1] = traingleIdx + 2;
            }

            return trianglesIdxs;
        }
    }
}