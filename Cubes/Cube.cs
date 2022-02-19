using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace Rubiks.Cubes
{
    class Cube
    {
        private List<Piece> pieces = new List<Piece>();
        public int rotateDegrees { get; set; } = 0;
        private Vector3 cubeCenter;
        public Notation rotating;
        public List<Notation> algorithm = new List<Notation>();
        private int degreeStep = 4;

        public Cube(Vector3 position, float size)
        {
            rotating = Notation.None;
            cubeCenter = position;
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
                Vector3 piecePos = new Vector3(position);
                List<Notation> wallTypes = new List<Notation>();
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
                                wallTypes.Add(Notation.U);
                                break;
                            case "bottom":
                                colorsPiece[1] = new Vector3(1.0f, 1.0f, 0.0f);
                                piecePos = Vector3.Add(piecePos, new Vector3(0.0f, -offset, 0.0f));
                                wallTypes.Add(Notation.D);
                                break;
                            case "right":
                                colorsPiece[2] = new Vector3(1.0f, 0.0f, 0.0f);
                                piecePos = Vector3.Add(piecePos, new Vector3(offset, 0.0f, 0.0f));
                                wallTypes.Add(Notation.R);
                                break;
                            case "left":
                                colorsPiece[3] = new Vector3(1.0f, 0.5f, 0.0f);
                                piecePos = Vector3.Add(piecePos, new Vector3(-offset, 0.0f, 0.0f));
                                wallTypes.Add(Notation.L);
                                break;
                            case "front":
                                colorsPiece[4] = new Vector3(0.0f, 1.0f, 0.0f);
                                piecePos = Vector3.Add(piecePos, new Vector3(0.0f, 0.0f, offset));
                                wallTypes.Add(Notation.F);
                                break;
                            case "back":
                                colorsPiece[5] = new Vector3(0.0f, 0.0f, 1.0f);
                                piecePos = Vector3.Add(piecePos, new Vector3(0.0f, 0.0f, -offset));
                                wallTypes.Add(Notation.B);
                                break;
                        }
                    }
                }
                pieces.Add(new Piece(colorsPiece, piecePos, size / 2, wallTypes.ToArray()));
            }
        }
        public float[] GetVertices()
        {
            var vertices = new float[] { };
            foreach (var piece in pieces)
            {
                vertices = vertices.Concat(piece.GetVertices()).ToArray();
            }
            return vertices;
        }
        public uint[] GetIndicies()
        {
            var indicies = new uint[] { };
            foreach (var piece in pieces)
            {
                uint length = (uint)indicies.Length;
                indicies = indicies.Concat(piece.GetIndicies().Select(i => i+(length / 6)*4).ToArray()).ToArray();
            }
            return indicies;
        }
        public float[] Rotate(Notation notation)
        {
            int degrees = degreeStep;
            if (Notations.Signed().Contains(notation))
                degrees = degreeStep > 0 ? degreeStep * (-1) : degreeStep;
            else
                degrees = degreeStep < 0 ? degreeStep * (-1) : degreeStep;

            if (Math.Abs(rotateDegrees + degrees) > 90)
                degrees = Math.Sign(degrees) * (90 - Math.Abs(rotateDegrees));
            rotateDegrees += degrees;

            int[] wallIDs = GetWallIDs(notation);
            Vector3 centerPos = cubeCenter;
            foreach (var id in wallIDs)
            {
                Matrix4 rotation = new Matrix4();
                if (Notations.Xaxis().Contains(notation))
                {
                    centerPos.X = 0.0f;
                    rotation = Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(degrees));
                }
                else if (Notations.Yaxis().Contains(notation))
                {
                    centerPos.Y = 0.0f;
                    rotation = Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(degrees));
                }
                else if (Notations.Zaxis().Contains(notation))
                {
                    centerPos.Z = 0.0f;
                    rotation = Matrix4.CreateRotationZ((float)MathHelper.DegreesToRadians(degrees));
                }
                for (int i = 0; i < pieces[id].Vectors.Count; i++)
                {
                    var position = pieces[id].Vectors[i];
                    var distance = Vector3.Subtract(new Vector3(0.0f), centerPos);
                    var transform = Matrix4.Identity * Matrix4.CreateTranslation(distance) * rotation * Matrix4.CreateTranslation(-distance);
                    position.Pos = new Vector3(new Vector4(position.Pos, 1.0f) * transform);
                    pieces[id].Vectors[i] = position;
                }
            }
            if (Math.Abs(rotateDegrees) == 90)
            {
                rotating = Notation.None;
                rotateDegrees = 0;
                UpdateWallRotation(notation);
                if (algorithm.Count > 0)
                {
                    rotating = algorithm[0];
                    algorithm.RemoveAt(0);
                }
            }
            return GetVertices();
        }
        private Notation[] UpdateWallTypes(Notation notation, Notation[] wallTypes)
        {
            List<Notation> temp = new List<Notation>();
            foreach (var w in wallTypes)
            {
                if (Notations.Xcw().Contains(notation))
                {
                    temp.Add(Notations.ConvertXcw(notation));
                    if (w == Notation.D)
                        temp.Add(Notation.F);
                    else if (w == Notation.F)
                        temp.Add(Notation.U);
                    else if (w == Notation.U)
                        temp.Add(Notation.B);
                    else if (w == Notation.B)
                        temp.Add(Notation.D);
                    else
                        temp.Add(w);
                }
                else if (Notations.Xccw().Contains(notation))
                {
                    if (w == Notation.D)
                        temp.Add(Notation.B);
                    else if (w == Notation.B)
                        temp.Add(Notation.U);
                    else if (w == Notation.U)
                        temp.Add(Notation.F);
                    else if (w == Notation.F)
                        temp.Add(Notation.D);
                    else
                        temp.Add(w);
                }
                else if (Notations.Ycw().Contains(notation))
                {
                    if (w == Notation.F)
                        temp.Add(Notation.L);
                    else if (w == Notation.L)
                        temp.Add(Notation.B);
                    else if (w == Notation.B)
                        temp.Add(Notation.R);
                    else if (w == Notation.R)
                        temp.Add(Notation.F);
                    else
                        temp.Add(w);
                }
                else if (Notations.Yccw().Contains(notation))
                {
                    if (w == Notation.F)
                        temp.Add(Notation.R);
                    else if (w == Notation.R)
                        temp.Add(Notation.B);
                    else if (w == Notation.B)
                        temp.Add(Notation.L);
                    else if (w == Notation.L)
                        temp.Add(Notation.F);
                    else
                        temp.Add(w);
                }
                else if (Notations.Zcw().Contains(notation))
                {
                    if (w == Notation.U)
                        temp.Add(Notation.R);
                    else if (w == Notation.R)
                        temp.Add(Notation.D);
                    else if (w == Notation.D)
                        temp.Add(Notation.L);
                    else if (w == Notation.L)
                        temp.Add(Notation.U);
                    else
                        temp.Add(w);
                }
                else if (Notations.Zccw().Contains(notation))
                {
                    if (w == Notation.U)
                        temp.Add(Notation.L);
                    else if (w == Notation.L)
                        temp.Add(Notation.D);
                    else if (w == Notation.D)
                        temp.Add(Notation.R);
                    else if (w == Notation.R)
                        temp.Add(Notation.U);
                    else
                        temp.Add(w);
                }
            }
            return temp.ToArray();
        }
        private int[] GetWallIDs(Notation notation)
        {
            if (Notations.Axes().Contains(notation))
                return Enumerable.Range(0, pieces.Count).ToArray();
            List<int> wallIDs = new List<int>();
            for (int i = 0; i < pieces.Count; i++)
            {
                foreach (var w in Notations.WallPieces(notation))
                {
                    if (new HashSet<Notation>(w).SetEquals(pieces[i].WallTypes))
                    {
                        wallIDs.Add(i);
                        break;
                    }
                }
            }
            return wallIDs.ToArray();
        }
        public void UpdateWallRotation(Notation notation)
        {
            int[] wallIDs = GetWallIDs(notation);
            foreach (var id in wallIDs)
            {
                pieces[id].WallTypes = UpdateWallTypes(notation, pieces[id].WallTypes);
            }
        }
        public List<Notation> GenerateScramble(int length)
        {
            var scramble = new List<Notation>();
            var nots = Notations.SingleLayers();
            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                var picked = nots[random.Next(0, nots.Length)];
                scramble.Add(picked);
            }
            algorithm.AddRange(scramble);
            return scramble;
        }
        public List<Notation> LoadAlg()
        {
            var alg = new List<Notation>();
            var text = File.ReadAllText("../../../Resources/alg.txt").Split(" ");
            foreach (var t in text)
            {
                if (t.Contains("2"))
                {
                    alg.Add(Notations.StringToNotation(t.Replace("2", "")));
                    alg.Add(Notations.StringToNotation(t.Replace("2", "")));
                }
                else
                    alg.Add(Notations.StringToNotation(t));
            }
            return alg;
        }
        public void Update()
        {
            if (algorithm.Count > 0)
                rotating = algorithm[0];
            else
                rotating = Notation.None;
        }
    }
}
