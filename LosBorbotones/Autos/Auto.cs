using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using System;
using System.Drawing;
using AlumnoEjemplos.LosBorbotones.Pantallas;

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

        private Device d3dDevice = GuiController.Instance.D3dDevice;


        public TgcMesh sun;

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

            //// acá defino un mesh auxiliar para probar con el Debug mode 
            string sphere = GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Sphere\\Sphere-TgcScene.xml";
            TgcSceneLoader loader = new TgcSceneLoader();
            sun = loader.loadSceneFromFile(sphere).Meshes[0];
            
           
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
            if (velocidadActual < -0.5f)
            {
                velocidadActual = velocidadNueva(delta_t, 0.65f * aceleracion);
                return velocidadActual;
            }
            if(velocidadActual > 0.5f) 
            {
                velocidadActual = velocidadNueva(delta_t, -0.65f * aceleracion); 
                return velocidadActual;
            }
            if (FastMath.Abs(velocidadActual) < 0.5f)
            {
                velocidadActual = 0;
            }
            return velocidadActual;
           }

        public float velocidadNueva(float delta_t, float aceleracion)
        {
           
            float velocidadNueva = velocidadActual + aceleracion * delta_t;
            return velocidadNueva;
        }


        public float chocar(float delta_t)
        {
                velocidadActual = -0.7f*velocidadActual;
                return velocidadActual;             
        }

        public void deformarMesh(TgcObb obbColisionable, float velocidad)
        {
            object vertexBuffer = null;
            Type tipo;
            Vector3 puntoChoque;
            float distanciaMinima;
            float factorChoque = 1 * velocidad/150;

            switch (this.mesh.RenderType)
            {
                case TgcMesh.MeshRenderType.VERTEX_COLOR:
                    vertexBuffer = (TgcSceneLoader.VertexColorVertex[])this.mesh.D3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.VertexColorVertex), LockFlags.ReadOnly, this.mesh.D3dMesh.NumberVertices);

                    break;

                case TgcMesh.MeshRenderType.DIFFUSE_MAP:
                    vertexBuffer = (TgcSceneLoader.DiffuseMapVertex[])this.mesh.D3dMesh.LockVertexBuffer(
                       typeof(TgcSceneLoader.DiffuseMapVertex), LockFlags.ReadOnly, this.mesh.D3dMesh.NumberVertices);

                    break;

                case TgcMesh.MeshRenderType.DIFFUSE_MAP_AND_LIGHTMAP:
                    vertexBuffer = (TgcSceneLoader.DiffuseMapAndLightmapVertex[])this.mesh.D3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.DiffuseMapAndLightmapVertex), LockFlags.ReadOnly, this.mesh.D3dMesh.NumberVertices);

                    break;
            }
            tipo = vertexBuffer.GetType();
            System.Reflection.MethodInfo dameValorPorIndice = tipo.GetMethod("GetValue", new Type[] { typeof(int) });
            System.Reflection.MethodInfo insertaValorPorIndice = tipo.GetMethod("SetValue", new Type[] { typeof(object), typeof(int) });
            int cantidadDeVertices = (int)tipo.GetProperty("Length").GetValue(vertexBuffer, null);


            // HACER COSAS LOCAS :D (descomentar y probar)
            //for (int i = 0; i < cantidadDeVertices; i++)
            //{
            //    object vertice = dameValorPorIndice.Invoke(verts, new object[] { i });
            //    Vector3 posicion = (Vector3)vertice.GetType().GetField("Position").GetValue(vertice);
            //    /// azullll
            //    vertice.GetType().GetField("Color").SetValue(vertice, Color.Blue.ToArgb());

            //    if (posicion.X >= 30)
            //    {
            //        posicion.X -= 30;
            //        vertice.GetType().GetField("Position").SetValue(vertice, posicion);
            //        insertaValorPorIndice.Invoke(verts, new object[] { vertice, i });
            //    }
            //}

            //Calculo la distancia minima entre el centro del OBB colisionado y todos los vertices del mesh
            //...parto con la distancia entre centros
            distanciaMinima = 500*2;
            puntoChoque = this.obb.Center;

            for (int i = 0; i < cantidadDeVertices; i++)
            {
                object vertice = dameValorPorIndice.Invoke(vertexBuffer, new object[] { i });
                Vector3 unVerticeDelMesh = (Vector3)vertice.GetType().GetField("Position").GetValue(vertice);

                if (distancePointPoint(unVerticeDelMesh,obbColisionable.Center) < distanciaMinima)
                {
                    distanciaMinima = distancePointPoint(unVerticeDelMesh, obbColisionable.Center);
                    puntoChoque = unVerticeDelMesh; // acá es donde se genera el choque!!!
                }
            }

            if (PantallaJuego.debugMode)
            {
                // ya sé donde se genera el choque... ahí voy a crear una esfera
                sun.Position = puntoChoque + this.obb.Position;
                GuiController.Instance.UserVars.setValue("DistMinima", sun.Position);
            }

            // APLICO DEFORMACIÓN EN MALLA
            for (int i = 0; i < cantidadDeVertices; i++)
            {
                object vertice = dameValorPorIndice.Invoke(vertexBuffer, new object[] { i });
                Vector3 unVerticeDelMesh = (Vector3)vertice.GetType().GetField("Position").GetValue(vertice);

                if (distancePointPoint(unVerticeDelMesh, puntoChoque) < factorChoque * 2)
                {
                    Vector3 vectorDondeMoverElPunto = unVerticeDelMesh - puntoChoque;
                    //corro de lugar el vértice del mesh, usando el versor del vector
                    unVerticeDelMesh += factorChoque * Vector3.Normalize(vectorDondeMoverElPunto); 

                    vertice.GetType().GetField("Position").SetValue(vertice, unVerticeDelMesh);
                    insertaValorPorIndice.Invoke(vertexBuffer, new object[] { vertice, i });
                }
            }

            

            this.mesh.D3dMesh.SetVertexBufferData(vertexBuffer, LockFlags.None);
            this.mesh.D3dMesh.UnlockVertexBuffer();
        }

        /// <summary>
        /// Obtiene la distancia entre dos puntos.
        /// </summary>
        /// <param name="p1">Punto 1</param>
        /// <param name="p2">Punto 2</param>
        /// <returns>Distancia entre los dos puntos</returns>
        private float distancePointPoint(Vector3 p1, Vector3 p2)
        {
            Vector3 slidePlaneNormal = p1 - p2;
            slidePlaneNormal.Normalize();
            Plane slidePlane = Plane.FromPointNormal(p2, slidePlaneNormal);

            return TgcCollisionUtils.distPointPlane(p1, slidePlane);
        }


        /// <summary>
        /// Devuelve Modulo de un vector.
        /// </summary>
       float calcularModulo(Vector3 vector)
       {
            float modulo = (float)Math.Sqrt(Math.Pow(vector.X, 2.0f) + Math.Pow(vector.Y, 2.0f));

            return modulo;
       }

       /// <summary>
       /// Devuelve Versor de un vector.
       /// </summary>
       Vector3 calcularVersor(Vector3 vector)
       {
           if (calcularModulo(vector) != 0)
           {
               vector.X /= calcularModulo(vector);
               vector.Y /= calcularModulo(vector);
               vector.Z /= calcularModulo(vector);

               return vector;
           }
           else
           { 
               return vector; 
           }

       }

    }
}
