using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using System;

namespace AlumnoEjemplos.LosBorbotones.Autos
{
   public class Auto
    { 
        public TgcMesh mesh;
        public TgcObb obb;
        public string nombre;
        public Vector3 posicionInicial;
        public float elapsedTime;
        public float velocidadMaxima;
        public float velocidadActual;
        public float velocidadRotacion;
        public float aceleracion;
        public float masa;

        public void setElapsedTime(float _elapsedTime)
        {
            elapsedTime = _elapsedTime;
        }

        public Auto(string pathMeshAuto, string _nombre, Vector3 _posicionInicial, float _velocidadMaxima, float _velocidadRotacion, float _aceleracion, float _masa) 
        {
            this.nombre = _nombre;
            this.posicionInicial = _posicionInicial;
            TgcScene sceneAuto = loadMesh(pathMeshAuto);
            this.mesh = sceneAuto.Meshes[0];
            this.velocidadActual = 0;
            this.velocidadMaxima = _velocidadMaxima;
            this.velocidadRotacion = _velocidadRotacion;
            this.masa = _masa;
            this.aceleracion = _aceleracion;
            //Computar OBB a partir del AABB del mesh. Inicialmente genera el mismo volumen que el AABB, pero luego te permite rotarlo (cosa que el AABB no puede)
            this.obb = TgcObb.computeFromAABB(this.mesh.BoundingBox);
        }

        public TgcScene loadMesh(string path)
        {
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene currentScene = loader.loadSceneFromFile(path);
            return currentScene;
        }

        public float irParaAdelante(float delta_t)
        {
            float acelerar;
           
            if (velocidadActual <= 0) 
            {
                acelerar= -aceleracion;
               
            }
            else 
            {
                acelerar = -5 * aceleracion;
               
            }
            velocidadActual = velocidadNueva(delta_t, acelerar);
            return velocidadActual;
        }

        public float irParaAtras(float delta_t)
        {
            float acelerar;

            if (velocidadActual >= 0)
            {
                acelerar = aceleracion;

            }
            else
            {
                acelerar = 5 * aceleracion;

            }
            velocidadActual = velocidadNueva(delta_t, acelerar);
            return velocidadActual;
        }

        public float frenarPorInercia(float delta_t) 
        {
            if (velocidadActual < 0)
            {
                velocidadActual = velocidadNueva(delta_t, 0.65f * aceleracion);
                return velocidadActual;
            }
            else 
            {
                velocidadActual = velocidadNueva(delta_t, -0.65f * aceleracion); 
                return velocidadActual;
            }
        }

        public float velocidadNueva(float delta_t, float aceleracion)
        {
           
            float velocidadNueva = velocidadActual + aceleracion * delta_t;
            return velocidadNueva;
        }

    }
}
