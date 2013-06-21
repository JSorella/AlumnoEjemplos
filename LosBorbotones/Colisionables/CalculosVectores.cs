using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer;
using TgcViewer.Utils.Input;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcKeyFrameLoader;
using AlumnoEjemplos.LosBorbotones.Niveles;
using AlumnoEjemplos.LosBorbotones;
using Microsoft.DirectX.DirectInput;
using TgcViewer.Utils.Sound;
using AlumnoEjemplos.LosBorbotones.Autos;
using AlumnoEjemplos.LosBorbotones.Colisionables;
using AlumnoEjemplos.LosBorbotones.Sonidos;


namespace AlumnoEjemplos.LosBorbotones.Colisionables
{
    public class CalculosVectores
    {

        public static Vector3[] computeCorners(ObstaculoRigido obstaculo)
        {
            TgcObb obb = obstaculo.obb;
            Vector3[] corners = new Vector3[8];
            Vector3 extents;
            Vector3[] orientation = obb.Orientation;
            Vector3 center = obb.Center;

            extents = obstaculo.mesh.BoundingBox.calculateAxisRadius();
            extents = TgcVectorUtils.abs(extents);

            Vector3 eX = extents.X * orientation[0];
            Vector3 eY = extents.Y * orientation[1];
            Vector3 eZ = extents.Z * orientation[2];

            corners[0] = center - eX - eY - eZ;
            corners[1] = center - eX - eY + eZ;

            corners[2] = center - eX + eY - eZ;
            corners[3] = center - eX + eY + eZ;

            corners[4] = center + eX - eY - eZ;
            corners[5] = center + eX - eY + eZ;

            corners[6] = center + eX + eY - eZ;
            corners[7] = center + eX + eY + eZ;

            return corners;
        }

        public float calcularTerminoIndependiente(Vector3 normal, Vector3 punto) 
        {
            return Vector3.Dot(normal, punto);
        }

        public static List<Plane> generarCaras(Vector3[] corners)
        {
            List<Plane> caras = new List<Plane>();
            List<Vector3> normales = new List<Vector3>();
            Plane caraIzquierda = Plane.FromPoints(corners[0], corners[3], corners[1]);
            Plane caraFrontal = Plane.FromPoints(corners[0], corners[4], corners[2]);
            Plane caraDerecha = Plane.FromPoints(corners[4], corners[5], corners[7]);
            Plane caraTrasera = Plane.FromPoints(corners[5], corners[1], corners[7]);
         
            caras.Add(caraIzquierda);
            caras.Add(caraDerecha);
            caras.Add(caraFrontal);
            caras.Add(caraTrasera);
           
            return caras;
        }

        public static Plane detectarCaraChocada(List<Plane> carasDelObstaculo, Vector3 puntoChoque)
        {
            Plane caraMasCercana = carasDelObstaculo[0];
            float distMinima = FastMath.Abs(TgcCollisionUtils.distPointPlane(puntoChoque, carasDelObstaculo[0]));

            foreach(Plane cara in carasDelObstaculo)
            {
                float unaDistancia = FastMath.Abs(TgcCollisionUtils.distPointPlane(puntoChoque, cara));

                if (unaDistancia < distMinima)
                {
                    distMinima = unaDistancia;
                    caraMasCercana = cara;
                }
            }

            GuiController.Instance.UserVars.setValue("DistMinima", distMinima);
            return caraMasCercana;
        }
        

        public static Vector3[] computeCorners(Auto auto)
        {
            TgcObb obbAuto = auto.obb;
            Vector3[] corners = new Vector3[8];
            Vector3 extents;
            Vector3[] orientation = obbAuto.Orientation;
            Vector3 center = obbAuto.Center;

            extents = auto.mesh.BoundingBox.calculateAxisRadius();
            extents = TgcVectorUtils.abs(extents);

            Vector3 eX = extents.X * orientation[0];
            Vector3 eY = extents.Y * orientation[1];
            Vector3 eZ = extents.Z * orientation[2];

            corners[0] = center - eX - eY - eZ;
            corners[1] = center - eX - eY + eZ;

            corners[2] = center - eX + eY - eZ;
            corners[3] = center - eX + eY + eZ;

            corners[4] = center + eX - eY - eZ;
            corners[5] = center + eX - eY + eZ;

            corners[6] = center + eX + eY - eZ;
            corners[7] = center + eX + eY + eZ;

            return corners;
        }

        public static Vector3 calcularVector(Vector3 puntoA, Vector3 puntoB)
        {
            //Dados dos puntos, calcula el vector que los tiene por extremos.
            return Vector3.Subtract(puntoB, puntoA);
        }

        public static Vector3 calcularNormalPlano(Vector3 puntoA, Vector3 puntoB, Vector3 puntoC) 
        {
            //Dados tres puntos de un plano, calcula el vector normal normalizado.
            Vector3 vector1 = calcularVector(puntoA, puntoB);
            Vector3 vector2 = calcularVector(puntoA, puntoC);

            Vector3 perpendicular = Vector3.Cross(vector1, vector2);
            Vector3 normal = Vector3.Normalize(perpendicular);

            return normal;
        }

        public static float calcularAnguloEntreVectoresNormalizados(Vector3 vector1, Vector3 vector2) 
        {
            vector1 = Vector3.Normalize(vector1);
            vector2 = Vector3.Normalize(vector2);
            
            return (float)FastMath.Acos(Vector3.Dot(vector1, vector2)); 
        }
    }
}
