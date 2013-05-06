using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer;
using Microsoft.DirectX;

namespace AlumnoEjemplos.LosBorbotones.Colisionables
{
    class Colisiones
    {
        /// <summary>
        /// Crea un array de floats con X,Y,Z
        /// </summary>
        private static float[] toArray(Vector3 v)
        {
            return new float[] { v.X, v.Y, v.Z };
        }


        /// <summary>
        /// Testear si hay olision entre dos OBB
        /// </summary>
        /// <param name="a">Primer OBB</param>
        /// <param name="b">Segundo OBB</param>
        /// <returns>True si hay colision</returns>
        public static bool testObbObb2(TgcObb a, TgcObb b)
        {
            return Colisiones.testObbObb2(a.toStruct(), b.toStruct());
        }

        /// <summary>
        /// Testear si hay olision entre dos OBB
        /// </summary>
        /// <param name="a">Primer OBB</param>
        /// <param name="b">Segundo OBB</param>
        /// <returns>True si hay colision</returns>
        public static bool testObbObb2(TgcObb.OBBStruct a, TgcObb.OBBStruct b)
        {
            float ra, rb;
            float[,] R = new float[3, 3];
            float[,] AbsR = new float[3, 3];
            float[] ae = toArray(a.extents);
            float[] be = toArray(b.extents);


            // Compute rotation matrix expressing b in a’s coordinate frame
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    R[i, j] = Vector3.Dot(a.orientation[i], b.orientation[j]);

            // Compute translation vector t
            Vector3 tVec = b.center - a.center;
            // Bring translation into a’s coordinate frame
            float[] t = new float[3];
            t[0] = Vector3.Dot(tVec, a.orientation[0]);
            t[1] = Vector3.Dot(tVec, a.orientation[1]);
            t[2] = Vector3.Dot(tVec, a.orientation[2]);

            // Compute common subexpressions. Add in an epsilon term to
            // counteract arithmetic errors when two edges are parallel and
            // their cross product is (near) null (see text for details)
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    AbsR[i, j] = FastMath.Abs(R[i, j]) + float.Epsilon;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    ae[i] = FastMath.Abs(ae[i]);
                    be[i] = FastMath.Abs(be[i]);
                }
            }

            // Test axes L = A0, L = A1, L = A2
            for (int i = 0; i < 3; i++)
            {
                ra = ae[i];
                rb = be[0] * AbsR[i, 0] + be[1] * AbsR[i, 1] + be[2] * AbsR[i, 2];
                if (FastMath.Abs(t[i]) > FastMath.Abs(ra + rb)) return false;
            }

            // Test axes L = B0, L = B1, L = B2
            for (int i = 0; i < 3; i++)
            {
                ra = ae[0] * AbsR[0, i] + ae[1] * AbsR[1, i] + ae[2] * AbsR[2, i];
                rb = be[i];
                if (FastMath.Abs(t[0] * R[0, i] + t[1] * R[1, i] + t[2] * R[2, i]) > FastMath.Abs(ra + rb)) return false;
            }

            // Test axis L = A0 x B0
            ra = ae[1] * AbsR[2, 0] + ae[2] * AbsR[1, 0];
            rb = be[1] * AbsR[0, 2] + be[2] * AbsR[0, 1];
            if (FastMath.Abs(t[2] * R[1, 0] - t[1] * R[2, 0]) > FastMath.Abs(ra + rb)) return false;

            // Test axis L = A0 x B1
            ra = ae[1] * AbsR[2, 1] + ae[2] * AbsR[1, 1];
            rb = be[0] * AbsR[0, 2] + be[2] * AbsR[0, 0];
            if (FastMath.Abs(t[2] * R[1, 1] - t[1] * R[2, 1]) > FastMath.Abs(ra + rb)) return false;

            // Test axis L = A0 x B2
            ra = ae[1] * AbsR[2, 2] + ae[2] * AbsR[1, 2];
            rb = be[0] * AbsR[0, 1] + be[1] * AbsR[0, 0];
            if (FastMath.Abs(t[2] * R[1, 2] - t[1] * R[2, 2]) > FastMath.Abs(ra + rb)) return false;

            // Test axis L = A1 x B0
            ra = ae[0] * AbsR[2, 0] + ae[2] * AbsR[0, 0];
            rb = be[1] * AbsR[1, 2] + be[2] * AbsR[1, 1];
            if (FastMath.Abs(t[0] * R[2, 0] - t[2] * R[0, 0]) > FastMath.Abs(ra + rb)) return false;


            // Test axis L = A1 x B1
            ra = ae[0] * AbsR[2, 1] + ae[2] * AbsR[0, 1];
            rb = be[0] * AbsR[1, 2] + be[2] * AbsR[1, 0];
            if (FastMath.Abs(t[0] * R[2, 1] - t[2] * R[0, 1]) > FastMath.Abs(ra + rb)) return false;

            // Test axis L = A1 x B2
            ra = ae[0] * AbsR[2, 2] + ae[2] * AbsR[0, 2];
            rb = be[0] * AbsR[1, 1] + be[1] * AbsR[1, 0];
            if (FastMath.Abs(t[0] * R[2, 2] - t[2] * R[0, 2]) > FastMath.Abs(ra + rb)) return false;

            // Test axis L = A2 x B0
            ra = ae[0] * AbsR[1, 0] + ae[1] * AbsR[0, 0];
            rb = be[1] * AbsR[2, 2] + be[2] * AbsR[2, 1];
            if (FastMath.Abs(t[1] * R[0, 0] - t[0] * R[1, 0]) > FastMath.Abs(ra + rb)) return false;

            // Test axis L = A2 x B1
            ra = ae[0] * AbsR[1, 1] + ae[1] * AbsR[0, 1];
            rb = be[0] * AbsR[2, 2] + be[2] * AbsR[2, 0];
            if (FastMath.Abs(t[1] * R[0, 1] - t[0] * R[1, 1]) > FastMath.Abs(ra + rb)) return false;

            // Test axis L = A2 x B2
            ra = ae[0] * AbsR[1, 2] + ae[1] * AbsR[0, 2];
            rb = be[0] * AbsR[2, 1] + be[1] * AbsR[2, 0];
            if (FastMath.Abs(t[1] * R[0, 2] - t[0] * R[1, 2]) > FastMath.Abs(ra + rb)) return false;


            // Since no separating axis is found, the OBBs must be intersecting
            return true;
        }
    }
}
