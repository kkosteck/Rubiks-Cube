using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace Rubiks.Cubes
{
    enum PieceType
    {
        Center = 1, Edge = 2, Corner = 3
    }
    class Piece
    {
        public Vector3 Center { get; set; }
        public PieceType PieceType { get; set; }
        public Notation[] WallTypes { get; set; }
        public List<(Vector3 Pos, Vector3 Color)> Vectors { get; set; } = new List<(Vector3 Pos, Vector3 Color)>();
        public Piece(Vector3[] colors, Vector3 position, float size, Notation[] wallTypes)
        {
            Center = position;
            PieceType = (PieceType)wallTypes.Length;
            this.WallTypes = wallTypes;
            Vector3[] offsets =
            {
                Vector3.Add(position, new Vector3(0.0f, size, 0.0f)),
                Vector3.Add(position, new Vector3(0.0f, -size, 0.0f)),
                Vector3.Add(position, new Vector3(size, 0.0f, 0.0f)),
                Vector3.Add(position, new Vector3(-size, 0.0f, 0.0f)),
                Vector3.Add(position, new Vector3(0.0f, 0.0f, size)),
                Vector3.Add(position, new Vector3(0.0f, 0.0f, -size))
            };
            Vector3[] wall =
            {
                new Vector3(size, size, 0.0f), // top right
                new Vector3(size, -size, 0.0f), // bottom right
                new Vector3(-size, -size, 0.0f), // bottom left
                new Vector3(-size, size, 0.0f), // top left
            };
            Matrix3[] transforms =
            {
                Matrix3.Identity * Matrix3.CreateRotationX(MathHelper.DegreesToRadians(90)), // top
                Matrix3.Identity * Matrix3.CreateRotationX(MathHelper.DegreesToRadians(90)), // bottom
                Matrix3.Identity * Matrix3.CreateRotationY(MathHelper.DegreesToRadians(90)), // left
                Matrix3.Identity * Matrix3.CreateRotationY(MathHelper.DegreesToRadians(90)), // right
                Matrix3.Identity, // front
                Matrix3.Identity, // back
            };

            for (int i = 0; i < offsets.Length; i++)
            {
                foreach (var w in wall)
                {
                    Vector3 t = Vector3.Add(w * transforms[i], offsets[i]);
                    Vectors.Add((new Vector3(t.X, t.Y, t.Z), new Vector3(colors[i].X, colors[i].Y, colors[i].Z)));
                }
            }
        }
        public float[] GetVertices()
        {
            List<float> vertices = new List<float>();
            foreach (var v in Vectors)
            {
                vertices.AddRange(new float[] {
                    v.Pos.X, v.Pos.Y, v.Pos.Z, v.Color.X, v.Color.Y, v.Color.Z
                });
            }
            return vertices.ToArray();
        }
        public uint[] GetIndicies()
        {
            List<uint> indicies = new List<uint>();
            uint counter = 0;
            foreach (var v in Vectors)
            {
                indicies.AddRange(new uint[] {
                    counter, counter+1, counter+3,
                    counter+1, counter+2, counter+3
                });
                counter += 4;
            }
            return indicies.ToArray();
        }
        public void UpdateCenter()
        {
            var sum = new Vector3(0.0f);
            foreach (var v in Vectors)
                sum = Vector3.Add(sum, v.Pos);
            Center = sum / Vectors.Count;
        }
    }
}
