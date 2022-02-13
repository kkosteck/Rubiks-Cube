using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace Rubiks.Cubes
{
    class Piece
    {
        public float[] vertices { get; set; }
        public uint[] indicies { get; set; }
        public Piece(Vector3[] colors, Vector3 position, float size)
        {
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

            var verts = new List<float>();
            var inds = new List<uint>();
            uint counter = 0;
            for (int i = 0; i < offsets.Length; i++)
            {
                foreach (var w in wall)
                {
                    Vector3 t = Vector3.Add(w * transforms[i], offsets[i]);
                    verts.AddRange(new float[] {
                        t.X, t.Y, t.Z, colors[i].X, colors[i].Y, colors[i].Z
                    });
                }
                inds.AddRange(new uint[] {
                    counter, counter+1, counter+3, 
                    counter+1, counter+2, counter+3 
                });
                counter += 4;
            }
            vertices = verts.ToArray();
            indicies = inds.ToArray();
        }
    }
}
