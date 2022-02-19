using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using static Rubiks.Cubes.Notation;

namespace Rubiks.Cubes
{
    enum Notation
    {
        R, L, U, D, F, B,
        Rp, Lp, Up, Dp, Fp, Bp,
        r, l, u, d, f, b,
        rp, lp, up, dp, fp, bp,
        M, E, S, Mp, Ep, Sp,
        x, y, z, xp, yp, zp,
        None
    }
    static class Pieces
    {
        static public Dictionary<string, Notation[]> Centers = GenerateCenters();
        static public Dictionary<string, Notation[]> Corners = GenerateCorners();
        static public Dictionary<string, Notation[]> Edges = GenerateEdges();
        static public Dictionary<string, Notation[][]> Walls = GenerateWalls();

        private static Dictionary<string, Notation[]> GenerateCenters()
        {
            var dict = new Dictionary<string, Notation[]>();
            dict.Add("FRONT", new []{ F });
            dict.Add("BACK", new[] { B });
            dict.Add("RIGHT", new[] { R });
            dict.Add("LEFT", new[] { L });
            dict.Add("UP", new[] { U });
            dict.Add("DOWN", new[] { D });
            return dict;
        }
        private static Dictionary<string, Notation[]> GenerateCorners()
        {
            var dict = new Dictionary<string, Notation[]>();
            dict.Add("FUL", new[] { F, U, L });
            dict.Add("FUR", new[] { F, U, R });
            dict.Add("BUL", new[] { B, U, L });
            dict.Add("BUR", new[] { B, U, R });
            dict.Add("FDL", new[] { F, D, L });
            dict.Add("FDR", new[] { F, D, R });
            dict.Add("BDL", new[] { B, D, L });
            dict.Add("BDR", new[] { B, D, R });
            return dict;
        }
        private static Dictionary<string, Notation[]> GenerateEdges()
        {
            var dict = new Dictionary<string, Notation[]>();
            dict.Add("UF", new[] { U, F });
            dict.Add("UR", new[] { U, R });
            dict.Add("UB", new[] { U, B });
            dict.Add("UL", new[] { U, L });
            dict.Add("FR", new[] { F, R });
            dict.Add("FL", new[] { F, L });
            dict.Add("BL", new[] { B, L });
            dict.Add("BR", new[] { B, R });
            dict.Add("DF", new[] { D, F });
            dict.Add("DR", new[] { D, R });
            dict.Add("DB", new[] { D, B });
            dict.Add("DL", new[] { D, L });
            return dict;
        }
        private static Dictionary<string, Notation[][]> GenerateWalls()
        {
            var dict = new Dictionary<string, Notation[][]>();
            dict.Add("FRONT", WallPieces(new[]{ "FRONT" }, new[]{ "FUL", "FUR", "FDL", "FDR" }, new[]{"UF", "FR", "DF", "FL"}));
            dict.Add("BACK", WallPieces(new[] { "BACK" }, new[] { "BUL", "BUR", "BDL", "BDR" }, new[] { "UB", "BR", "DB", "BL" }));
            dict.Add("RIGHT", WallPieces(new[] { "RIGHT" }, new[] { "FUR", "BUR", "FDR", "BDR" }, new[] { "UR", "BR", "DR", "FR" }));
            dict.Add("LEFT", WallPieces(new[] { "LEFT" }, new[] { "FUL", "BUL", "FDL", "BDL" }, new[] { "UL", "BL", "DL", "FL" }));
            dict.Add("UP", WallPieces(new[] { "UP" }, new[] { "FUL", "FUR", "BUL", "BUR" }, new[] { "UF", "UR", "UB", "UL" }));
            dict.Add("DOWN", WallPieces(new[] { "DOWN" }, new[] { "FDL", "FDR", "BDL", "BDR" }, new[] { "DF", "DR", "DB", "DL" }));
            dict.Add("MIDDLE", WallPieces(new[] { "FRONT", "UP", "BACK", "DOWN" }, null, new[] { "UF", "UB", "DB", "DF" }));
            dict.Add("EQUATOR", WallPieces(new[] { "FRONT", "RIGHT", "BACK", "LEFT" }, null, new[] { "FL", "FR", "BL", "BR" }));
            dict.Add("STANDING", WallPieces(new[] { "UP", "RIGHT", "DOWN", "LEFT" }, null, new[] { "UL", "UR", "DR", "DL" }));
            return dict;
        }
        private static Notation[][] WallPieces(string[] centers, string[] corners, string[] edges)
        {
            var pieces = centers.Select(x => Centers[x]).ToList();
            pieces.AddRange(edges.Select(x => Edges[x]).ToList());
            if (corners != null)
                pieces.AddRange(corners.Select(x => Corners[x]).ToList());
            return pieces.ToArray();
        }
    }
    static class Notations
    {
        static public Notation[] All()
        {
            return Enum.GetValues(typeof(Notation)).Cast<Notation>().ToArray();
        }
        static public Notation[] Xaxis()
        {
            return new Notation[]{R, Rp, r, rp, L, Lp, l, lp, x, xp, M, Mp};
        }
        static public Notation[] Yaxis()
        {
            return new Notation[] { U, Up, u, up, D, Dp, d, dp, y, yp, E, Ep };
        }
        static public Notation[] Zaxis()
        {
            return new Notation[] { F, Fp, f, fp, B, Bp, b, bp, z, zp, S, Sp };
        }
        static public Notation[] Signed()
        {
            return new Notation[] { R, Lp, U, Dp, F, Bp, r, lp, u, dp, f, bp, x, y, z, Mp, S, Ep };
        }
        static public Notation[] Unsigned()
        {
            return All().Except(Signed()).ToArray();
        }
        static public Notation[] Xcw()
        {
            return new Notation[] { R, Lp, Mp, x, r, lp };
        }
        static public Notation[] Xccw()
        {
            return Xaxis().Except(Xcw()).ToArray();
        }
        static public Notation[] Ycw()
        {
            return new Notation[] { U, Dp, Ep, y, u, dp };
        }
        static public Notation[] Yccw()
        {
            return Yaxis().Except(Ycw()).ToArray();
        }
        static public Notation[] Zcw()
        {
            return new Notation[] { F, Bp, S, z, f, bp };
        }
        static public Notation[] Zccw()
        {
            return Zaxis().Except(Zcw()).ToArray();
        }
        static public Notation[] DoubleLayers()
        {
            return new Notation[] { d, dp, u, up, r, rp, l, lp, f, fp, b, bp };
        }
        static public Notation[] SingleLayers()
        {
            return new Notation[] { R, Rp, L, Lp, U, Up, D, Dp, F, Fp, B, Bp };
        }
        static public Notation[] MiddleLayers()
        {
            return new Notation[] { M, Mp, S, Sp, E, Ep };
        }
        static public Notation[] Axes()
        {
            return new Notation[] { x, xp, y, yp, z, zp };
        }
        static public Notation[] ToSingles(Notation notation)
        {
            if (notation == r)
                return new Notation[] {R, Mp};
            else if (notation == rp)
                return new Notation[] {Rp, M};
            else if (notation == l)
                return new Notation[] { L, M };
            else if (notation == lp)
                return new Notation[] { Lp, Mp };
            else if (notation == f)
                return new Notation[] { F, S };
            else if (notation == fp)
                return new Notation[] { Fp, Sp };
            else if (notation == b)
                return new Notation[] { B, Sp };
            else if (notation == bp)
                return new Notation[] { Bp, S };
            else if (notation == u)
                return new Notation[] { U, E };
            else if (notation == up)
                return new Notation[] { Up, Ep };
            else if (notation == d)
                return new Notation[] { D, Ep };
            else if (notation == dp)
                return new Notation[] { Dp, E };
            return null;
        }
        static public Notation[][] WallPieces(Notation notation)
        {
            var walls = new List<Notation[]>();
            if (new[] { R, Rp, r, rp }.Contains(notation))
                walls.AddRange(Pieces.Walls["RIGHT"]);
            else if (new[] { L, Lp, l, lp }.Contains(notation))
                walls.AddRange(Pieces.Walls["LEFT"]);
            else if (new[] { U, Up, u, up }.Contains(notation))
                walls.AddRange(Pieces.Walls["UP"]);
            else if (new[] { D, Dp, d, dp }.Contains(notation))
                walls.AddRange(Pieces.Walls["DOWN"]);
            else if (new[] { F, Fp, f, fp }.Contains(notation))
                walls.AddRange(Pieces.Walls["FRONT"]);
            else if (new[] { B, Bp, b, bp }.Contains(notation))
                walls.AddRange(Pieces.Walls["BACK"]);
            else if (notation == M || notation == Mp)
                walls.AddRange(Pieces.Walls["MIDDLE"]);
            else if (notation == E || notation == Ep)
                walls.AddRange(Pieces.Walls["EQUATOR"]);
            else if (notation == S || notation == Sp)
                walls.AddRange(Pieces.Walls["STANDING"]);

            if (new[] { r, rp, l, lp }.Contains(notation)) 
                walls.AddRange(Pieces.Walls["MIDDLE"]);
            else if (new[] { u, up, d, dp }.Contains(notation))
                walls.AddRange(Pieces.Walls["EQUATOR"]);
            else if (new[] { f, fp, b, bp }.Contains(notation))
                walls.AddRange(Pieces.Walls["STANDING"]);

            return walls.ToArray();
        }
        static public Notation ConvertXcw(Notation notation)
        {
            if (notation == D)
                return F;
            else if (notation == F)
                return U;
            else if (notation == U)
                return B;
            else if (notation == B)
                return D;
            return notation;
        }
        static public Notation StringToNotation(string sign)
        {
            switch (sign)
            {
                case "R":
                    return R;
                case "R'":
                    return Rp;
                case "L":
                    return L;
                case "L'":
                    return Lp;
                case "U":
                    return U;
                case "U'":
                    return Up;
                case "D":
                    return D;
                case "D'":
                    return Dp;
                case "F":
                    return F;
                case "F'":
                    return Fp;
                case "B":
                    return B;
                case "B'":
                    return Bp;
                case "r":
                    return r;
                case "r'":
                    return rp;
                case "l":
                    return l;
                case "l'":
                    return lp;
                case "u":
                    return u;
                case "u'":
                    return up;
                case "d":
                    return d;
                case "d'":
                    return dp;
                case "f":
                    return f;
                case "f'":
                    return fp;
                case "b":
                    return b;
                case "b'":
                    return bp;
                case "M":
                    return M;
                case "M'":
                    return Mp;
                case "S":
                    return S;
                case "S'":
                    return Sp;
                case "E":
                    return E;
                case "E'":
                    return Ep;
                case "x":
                    return x;
                case "x'":
                    return xp;
                case "y":
                    return y;
                case "y'":
                    return yp;
                case "z":
                    return z;
                case "z'":
                    return zp;
            }
            return None;
        }
    }
}
