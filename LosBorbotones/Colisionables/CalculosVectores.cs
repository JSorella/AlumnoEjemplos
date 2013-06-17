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

        public Vector3[] computeCorners(ObstaculoRigido obstaculo)
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

        public List<Plane> generarCaras(Vector3[] corners)
        {
            List<Plane> caras = new List<Plane>();
            List<Vector3> normales = new List<Vector3>();

            Vector3 normalCaraIzquierda = calcularNormalPlano(corners[0],corners[3],corners[1]);
            float dizq = this.calcularTerminoIndependiente( normalCaraIzquierda, corners[0]);
            Plane caraIzquierda = new Plane(normalCaraIzquierda.X,normalCaraIzquierda.Y,normalCaraIzquierda.Z,dizq);

            Vector3 normalCaraFrontal = calcularNormalPlano(corners[0], corners[4], corners[2]);
            float dfron = this.calcularTerminoIndependiente(normalCaraFrontal, corners[0]);
            Plane caraFrontal = new Plane(normalCaraFrontal.X, normalCaraFrontal.Y, normalCaraFrontal.Z, dfron);

            Vector3 normalCaraDerecha = calcularNormalPlano(corners[4], corners[5], corners[7]);
            float dder = this.calcularTerminoIndependiente(normalCaraDerecha, corners[4]);
            Plane caraDerecha = new Plane(normalCaraDerecha.X, normalCaraDerecha.Y, normalCaraDerecha.Z, dder);

            Vector3 normalCaraSuperior = calcularNormalPlano(corners[6], corners[7], corners[2]);
            float dsup = this.calcularTerminoIndependiente(normalCaraSuperior, corners[6]);
            Plane caraSuperior = new Plane(normalCaraSuperior.X, normalCaraSuperior.Y, normalCaraSuperior.Z, dsup);

            Vector3 normalCaraTrasera = calcularNormalPlano(corners[5], corners[1], corners[7]);
            float dtras = this.calcularTerminoIndependiente(normalCaraTrasera, corners[5]);
            Plane caraTrasera = new Plane(normalCaraTrasera.X, normalCaraTrasera.Y, normalCaraTrasera.Z, dtras);

            caras.Add(caraDerecha);
            caras.Add(caraIzquierda);
            caras.Add(caraFrontal);
            //caras.Add(caraSuperior);
            caras.Add(caraTrasera);
            return caras;
        }

        public Plane detectarCaraChocada(List<Plane> carasDelObstaculo, Vector3 puntoChoque)
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
        

        public Vector3[] computeCorners(Auto auto)
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

        public float calcularAnguloEntreVectoresNormalizados(Vector3 vector1, Vector3 vector2) 
        {
            return (float)FastMath.Acos(Vector3.Dot(vector1, vector2)); 
        }
    }
}
