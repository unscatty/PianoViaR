using System;
using UnityEngine;

namespace BasicShapes
{
    public enum Plane { XY, YX, XZ, ZX, YZ, ZY };
    public class Ellipse
    {
        private Vector3[] vertices;
        public Vector3[] Vertices
        {
            get
            {
                return vertices;
            }
        }
        public int Sides { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public Plane basePlane = Plane.ZY;
        public Plane Plane
        {
            get
            {
                return basePlane;
            }

            set
            {
                basePlane = value;
            }
        }
        // public Vector3 NormalDirection { get; set; } = Vector3.back;
        public Vector3 Center { get; set; } = Vector3.zero;

        public Ellipse(int sides, float width, float height, Plane plane = Plane.XY)
        {
            CheckSides(sides);

            this.Sides = sides;
            this.Width = width;
            this.Height = height;
            this.Plane = plane;

            CalculateVertices();
        }

        public Ellipse(int sides, float width, float height, Vector3 center, Plane plane = Plane.XY)
        : this(sides, width, height, plane)
        {
            this.Center = center;
            CenterTo(Center);
        }

        private static void CheckSides(int sides)
        {
            if (sides < 0)
            {
                throw new ArgumentException($"Argument sides must be positive, current value is : {sides}");
            }

            if (sides < 3)
            {
                throw new ArgumentException($"Argument sides must be greater than 2, current value is : {sides}");
            }
        }

        private static Vector3 PointInPlane(Plane plane, float width, float height)
        {
            Vector3 result = new Vector3(width, height, 0);
            switch (plane)
            {
                case Plane.XY: result = new Vector3(width, height, 0); break;
                case Plane.XZ: result = new Vector3(width, 0, height); break;
                case Plane.YZ: result = new Vector3(0, width, height); break;
                case Plane.YX: result = new Vector3(height, width, 0); break;
                case Plane.ZX: result = new Vector3(height, 0, width); break;
                case Plane.ZY: result = new Vector3(0, height, width); break;
                    // default: result = new Vector3(width, height, 0);
            }

            return result;
        }

        public static Vector3[] CalculateVertices(int sides, float width, float height, Vector3 center, Vector3 normalDirection, float theta = 0, Plane plane = Plane.XY)
        {
            CheckSides(sides);

            var vertices = new Vector3[sides];
            Quaternion qRotation = Quaternion.AngleAxis(theta, normalDirection);

            // for (int i = 0; i < sides; i++)
            for (int i = 0; i < sides; i++)
            {
                float angle = (float)i / (float)sides * 2.0f * Mathf.PI;
                // vertices[sides - 1 - i] = qRotation * PointInPlane(plane, width * Mathf.Cos(angle), height * Mathf.Sin(angle)) + center;
                vertices[i] = qRotation * PointInPlane(plane, -width * Mathf.Cos(angle), height * Mathf.Sin(angle)) + center;
            }

            return vertices;
        }

        public void CalculateVertices()
        {
            CheckSides(Sides);

            vertices = new Vector3[Sides];
            for (int i = 0; i < Sides; i++)
            {
                float angle = (float)i / (float)Sides * 2.0f * Mathf.PI;
                // vertices[sides - 1 - i] = qRotation * PointInPlane(plane, width * Mathf.Cos(angle), height * Mathf.Sin(angle)) + center;
                vertices[i] = PointInPlane(basePlane, -Width * Mathf.Cos(angle), Height * Mathf.Sin(angle)) + Center;
            }
        }

        public void CenterTo(Vector3 center)
        {
            Center = center;
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i] = vertices[i] + center;
            }
        }

        public void ReCenter()
        {
            CenterTo(Center);
        }

        public void Rotate(float theta, Vector3 normalDirection)
        {
            Quaternion qRotation = Quaternion.AngleAxis(theta, normalDirection);
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i] = qRotation * (vertices[i] - Center) + Center;
            }
        }

        public void Rotate(float theta, Plane plane, bool reverseNormal = false)
        {
            Vector3 normalDirection = NormalDirection(plane);
            Rotate(theta, reverseNormal ? -normalDirection : normalDirection);
        }

        public void Rotate(float theta, bool reverseNormal = false)
        {
            Rotate(theta, basePlane, reverseNormal);
        }

        private Vector3 NormalDirection(Plane plane)
        {
            switch (plane)
            {
                // On plane XY: Facing back
                case Plane.XY: case Plane.YX: return Vector3.back;
                // On plane XZ: Facing up
                case Plane.XZ: case Plane.ZX: return Vector3.up;
                // On plane YZ: Facing right
                case Plane.YZ: case Plane.ZY: return Vector3.right;
                default: return Vector3.back;
            }
        }
    }
}