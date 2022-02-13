using System;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace Rubiks.Cubes
{
    class Cube
    {
        private List<Piece> pieces = new List<Piece>();
        public Cube(Vector3 position, float size)
        {
            float offset = 0.1f + size;
            XmlDocument xml = new XmlDocument();
            xml.Load(new XmlTextReader("../../../Resources/pieces.xml"));

            foreach (XmlNode piece in xml.FirstChild.ChildNodes)
            {
                if (XmlNodeType.Element != piece.NodeType)
                    continue;
                Vector3[] colorsPiece =
                {
                    new Vector3(0.0f),
                    new Vector3(0.0f),
                    new Vector3(0.0f),
                    new Vector3(0.0f),
                    new Vector3(0.0f),
                    new Vector3(0.0f)
                };
                Console.WriteLine(piece.Name);
                Vector3 piecePos = new Vector3(position);
                foreach (XmlNode wall in piece.ChildNodes)
                {
                    if (XmlNodeType.Element != wall.NodeType)
                        continue;
                    if (wall.Attributes["value"].Value == "1")
                    {
                        switch (wall.Attributes["type"].Value)
                        {
                            case "top":
                                colorsPiece[0] = new Vector3(1.0f, 1.0f, 1.0f);
                                piecePos = Vector3.Add(piecePos, new Vector3(0.0f, offset, 0.0f));
                                break;
                            case "bottom":
                                colorsPiece[1] = new Vector3(1.0f, 1.0f, 0.0f);
                                piecePos = Vector3.Add(piecePos, new Vector3(0.0f, -offset, 0.0f));
                                break;
                            case "right":
                                colorsPiece[2] = new Vector3(1.0f, 0.0f, 0.0f);
                                piecePos = Vector3.Add(piecePos, new Vector3(offset, 0.0f, 0.0f));
                                break;
                            case "left":
                                colorsPiece[3] = new Vector3(1.0f, 0.5f, 0.0f);
                                piecePos = Vector3.Add(piecePos, new Vector3(-offset, 0.0f, 0.0f));
                                break;
                            case "front":
                                colorsPiece[4] = new Vector3(0.0f, 1.0f, 0.0f);
                                piecePos = Vector3.Add(piecePos, new Vector3(0.0f, 0.0f, offset));
                                break;
                            case "back":
                                colorsPiece[5] = new Vector3(0.0f, 0.0f, 1.0f);
                                piecePos = Vector3.Add(piecePos, new Vector3(0.0f, 0.0f, -offset));
                                break;
                        }
                    }
                }
                pieces.Add(new Piece(colorsPiece, piecePos, size / 2));
            }
        }
        public float[] GetVertices()
        {
            var vertices = new float[] { };
            foreach (var piece in pieces)
            {
                vertices = vertices.Concat(piece.vertices).ToArray();
            }
            return vertices;
        }
        public uint[] GetIndicies()
        {
            var indicies = new uint[] { };
            foreach (var piece in pieces)
            {
                uint length = (uint)indicies.Length;
                indicies = indicies.Concat(piece.indicies.Select(i => i+(length / 6)*4).ToArray()).ToArray();
            }
            return indicies;
        }
    }
}
