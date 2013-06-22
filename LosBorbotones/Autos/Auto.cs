using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using System;
using System.Drawing;
using System.Collections.Generic;
using AlumnoEjemplos.LosBorbotones.Pantallas;
using AlumnoEjemplos.LosBorbotones.Sonidos;
using AlumnoEjemplos.LosBorbotones.Colisionables;


namespace AlumnoEjemplos.LosBorbotones.Autos
{
    public class Auto
    {
        public TgcMesh mesh;
        public TgcObb obb;
        public string nombre;
        public Vector3 posicionInicial;
        public float elapsedTime;
        public float velocidadMaxima, velocidadMaximaInicial;
        public float velocidadActual;
        public float velocidadRotacion;
        public float velocidadRotacionOriginal;
        public float aceleracion;
        private float masa;
        private Device d3dDevice = GuiController.Instance.D3dDevice;
        public TgcScene sceneAuto;
        public TgcMesh moon;
        public TgcMesh chispa;
        public List<Chispa> chispas = new List<Chispa>();
        public Vector3 puntoChoque;
        private Vector3 rotacionInicial;
        object vertexBufferBkp = null;
        int cantidadDeChispas = 60;
        public TgcArrow direccion;
        private Sonido sonidoChoque;

        public void setElapsedTime(float _elapsedTime)
        {
            elapsedTime = _elapsedTime;
        }

        public Auto(string pathMeshAuto, string _nombre, Vector3 _posicionInicial, float _velocidadMaxima, float _velocidadRotacion, float _aceleracion, float _masa, Vector3 _escala, Vector3 _rotacionInicial)
        {
            
            this.nombre = _nombre;
            this.posicionInicial = _posicionInicial;
           this.mesh = MeshUtils.loadMesh(pathMeshAuto);
           // sceneAuto = loadMesh(pathMeshAuto);
            //this.mesh = sceneAuto.Meshes[0];
            this.mesh.Scale = _escala;
            this.rotacionInicial = _rotacionInicial;
            this.backupVertices();
            this.velocidadActual = 0;
            this.velocidadMaxima = _velocidadMaxima;
            this.velocidadMaximaInicial = _velocidadMaxima;
            this.velocidadRotacion = _velocidadRotacion;
            this.velocidadRotacionOriginal = _velocidadRotacion;
            this.masa = _masa;
            this.aceleracion = _aceleracion;
            //Computar OBB a partir del AABB del mesh. Inicialmente genera el mismo volumen que el AABB, pero luego te permite rotarlo (cosa que el AABB no puede)
            this.obb = TgcObb.computeFromAABB(this.mesh.BoundingBox);

            this.puntoChoque = this.obb.Center;
            this.direccion = new TgcArrow();
            direccion.PStart = this.obb.Center;
            Vector3 rotacion = this.mesh.Rotation;
            direccion.PEnd = this.obb.Center + Vector3.Multiply(new Vector3(rotacion.X, rotacion.Y, rotacion.Z), 500f);

            //// acá defino un mesh auxiliar para probar con el Debug mode
            string sphere = GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Sphere\\Sphere-TgcScene.xml";
            TgcSceneLoader loader = new TgcSceneLoader();
            moon = loader.loadSceneFromFile(sphere).Meshes[0];
            moon.Scale = new Vector3(0.6f, 0.6f, 0.6f);

            //le asignamos una cantidad de chispas cada vez que choca
            for (int i = 0; i < cantidadDeChispas; i++)
            {
                chispas.Add(new Chispa());
            }

            //... y un poco de sonido a los choques
            this.sonidoChoque = new Sonido(Shared.mediaPath + "\\sonidos\\choque.wav");
        }
        
        public float irParaAdelante(float delta_t)
        {
            float acelerar;

            if (velocidadActual <= 0)
            {
                acelerar = -aceleracion;
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
            if (velocidadActual > 0.5f)
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
            velocidadActual =0.7f * velocidadActual;
            return velocidadActual;
        }


        public void reiniciar()
        {
            Vector3 posicionInicio = posicionInicial;
            this.velocidadActual = 0;
            this.velocidadRotacion = velocidadRotacionOriginal;
            restaurarVertices();
            this.velocidadMaxima = this.velocidadMaximaInicial;
            this.mesh.Rotation = rotacionInicial;
            this.mesh.Position = posicionInicio;
            this.obb = TgcObb.computeFromAABB(this.mesh.BoundingBox);
        }

        public void backupVertices()
        {
            switch (this.mesh.RenderType)
            {
                case TgcMesh.MeshRenderType.VERTEX_COLOR:
                    vertexBufferBkp = (TgcSceneLoader.VertexColorVertex[])this.mesh.D3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.VertexColorVertex), LockFlags.ReadOnly, this.mesh.D3dMesh.NumberVertices);

                    break;

                case TgcMesh.MeshRenderType.DIFFUSE_MAP:
                    vertexBufferBkp = (TgcSceneLoader.DiffuseMapVertex[])this.mesh.D3dMesh.LockVertexBuffer(
                       typeof(TgcSceneLoader.DiffuseMapVertex), LockFlags.ReadOnly, this.mesh.D3dMesh.NumberVertices);

                    break;

                case TgcMesh.MeshRenderType.DIFFUSE_MAP_AND_LIGHTMAP:
                    vertexBufferBkp = (TgcSceneLoader.DiffuseMapAndLightmapVertex[])this.mesh.D3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.DiffuseMapAndLightmapVertex), LockFlags.ReadOnly, this.mesh.D3dMesh.NumberVertices);
                    break;
            }
            this.mesh.D3dMesh.SetVertexBufferData(vertexBufferBkp, LockFlags.None);
            this.mesh.D3dMesh.UnlockVertexBuffer();
            
        }

        public void restaurarVertices()
        {
            this.mesh.D3dMesh.SetVertexBufferData(vertexBufferBkp, LockFlags.None);
            this.mesh.D3dMesh.UnlockVertexBuffer();
        }


        public void deformarMesh(TgcObb obbColisionable, float velocidad)
        {
            object vertexBuffer = null;
            Type tipo;
            float distanciaMinima;
            float factorChoque = Math.Abs(velocidad) * this.masa * 0.001F;

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


            //Calculo la distancia minima entre el centro del OBB colisionado y todos los vertices del mesh
            //...parto con la distancia entre centros
            distanciaMinima = (this.obb.Extents.Length() + obbColisionable.Extents.Length()) * 6;
            this.puntoChoque = this.obb.Center;


            Vector3[] cornersObbCoche = computeCorners(this.obb);
            Vector3[] cornersObstaculo = computeCorners(obbColisionable);
            int idPuntoChoque = 0;

            for (int j = 0; j < 8; j++)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (distanciaMinimaAlPlano(cornersObbCoche[i], cornersObstaculo[j]) < distanciaMinima)
                    {
                        distanciaMinima = distanciaMinimaAlPlano(cornersObbCoche[i], cornersObstaculo[j]);
                        this.puntoChoque = cornersObbCoche[i]; // acá es donde se genera el choque!!! (es un corner del obb)
                        idPuntoChoque = i;
                    }
                }
            }


            if (Shared.debugMode)
            {
                // ya sé donde se genera el choque... ahí voy a crear una esfera para verlo en el DebugMode
                moon.Position = this.puntoChoque;                
            }

            //Armo un obb auxiliar para rotarlo a la orientación original (porque el VertexBuffer me carga los vértices sin rotar!!!)
            TgcObb obbAuxiliar = this.obb;
            obbAuxiliar.setRotation(rotacionInicial);
            Vector3[] nuevosCorners = computeCorners(obbAuxiliar);

            Vector3 puntoChoqueDeformacion = nuevosCorners[idPuntoChoque];

            // APLICO DEFORMACIÓN EN MALLA
            /// .... mientras, aprovecho para buscar al verdadero punto de choque (el otro es un corner del obb)
            distanciaMinima = (this.obb.Extents.Length() + obbColisionable.Extents.Length()) * 6;
            Vector3 verdaderoPuntoDeChoque = puntoChoque;  // lo voy a usar para las chispas!

            for (int i = 0; i < cantidadDeVertices; i++)
            {
                object vertice = dameValorPorIndice.Invoke(vertexBuffer, new object[] { i });
                Vector3 unVerticeDelMesh = (Vector3)vertice.GetType().GetField("Position").GetValue(vertice);
                unVerticeDelMesh += this.obb.Position;

                if (Math.Abs(distancePointPoint(unVerticeDelMesh, puntoChoqueDeformacion)) < distanciaMinima)
                {
                    distanciaMinima = Math.Abs(distancePointPoint(unVerticeDelMesh, puntoChoqueDeformacion));
                    verdaderoPuntoDeChoque = unVerticeDelMesh;
                }

                /// voy tirando los vertices al centro del mesh  
                if (Math.Abs(distancePointPoint(unVerticeDelMesh, puntoChoqueDeformacion)) < factorChoque)
                {
                    float factorDeformacion = factorChoque * 0.1f;
                    Vector3 vectorDondeMoverElPunto = unVerticeDelMesh - puntoChoqueDeformacion;
                    //vectorDondeMoverElPunto.Z = unVerticeDelMesh.Z; // fuerzo al plano Z para que no pasen cosas raras
                    //corro de lugar el vértice del mesh, usando el versor del vector
                    unVerticeDelMesh += factorDeformacion * Vector3.Normalize(vectorDondeMoverElPunto);

                    //restauro como estaba antes, sino guardo cqcosa
                    unVerticeDelMesh -= this.obb.Position;
                    vertice.GetType().GetField("Position").SetValue(vertice, unVerticeDelMesh);
                    insertaValorPorIndice.Invoke(vertexBuffer, new object[] { vertice, i });
                }
            }
            this.mesh.D3dMesh.SetVertexBufferData(vertexBuffer, LockFlags.None);
            this.mesh.D3dMesh.UnlockVertexBuffer();
            // FIN DEFORMACIÓN

            //Mientras más se deforma, más se reduce su velocidad máxima
            this.velocidadMaxima -= factorChoque * 0.5f;

            //Ahora vienen las CHISPAS
            int k = 0;
            int cte = chispas.Count * 4;
            foreach (Chispa chispa in this.chispas)
            {
                chispa.mesh.Position = verdaderoPuntoDeChoque;
                chispa.asignarDireccion(puntoChoque, verdaderoPuntoDeChoque, new Vector3(k * cte, k * cte, k * cte));
                chispa.tiempoChispas = factorChoque * 5;
                k++;
            }
        }

        public void render()
        {
            this.mesh.render();
            this.direccion.PStart = obb.Center;       

            // chispas si hay choque
            if (Shared.mostrarChispa)
            {
                this.chispas.ForEach(o => o.render());
            }

            //... todo lo que debería renderizar con debugMode ON
            if (Shared.debugMode)
            {
                this.obb.render();
                this.moon.render();
                this.direccion.updateValues();
                this.direccion.render();
            }   
        }

        /// <summary>
        /// Reproduce su sonido de choque. (mientras más rápido choca, mayor es el volumen)
        /// </summary>
        public void reproducirSonidoChoque(float _velocidadAlChocar)
        {
            int volumen = (int)FastMath.Floor(((_velocidadAlChocar / this.velocidadMaxima)) * 10000f) - 10000;
            sonidoChoque.play(volumen);
        }

        /// <summary>
        /// Mutea/Desmutea sonido de choque
        /// </summary>
        public void mutearSonido()
        {
            sonidoChoque.mute = !sonidoChoque.mute;
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

        private float distanciaMinimaAlPlano(Vector3 p1, Vector3 p2)
        {
            return Math.Min(Math.Abs(p1.Z - p2.Z), Math.Abs(p1.X - p2.X));
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

        /// <summary>
        /// Crea un array con los 8 vertices del OBB
        /// </summary>
        private Vector3[] computeCorners(TgcObb obb)
        {
            Vector3[] corners = new Vector3[8];

            Vector3 eX = obb.Extents.X * obb.Orientation[0];
            Vector3 eY = obb.Extents.Y * obb.Orientation[1];
            Vector3 eZ = obb.Extents.Z * obb.Orientation[2];

            corners[0] = obb.Center - eX - eY - eZ;
            corners[1] = obb.Center - eX - eY + eZ;

            corners[2] = obb.Center - eX + eY - eZ;
            corners[3] = obb.Center - eX + eY + eZ;

            corners[4] = obb.Center + eX - eY - eZ;
            corners[5] = obb.Center + eX - eY + eZ;

            corners[6] = obb.Center + eX + eY - eZ;
            corners[7] = obb.Center + eX + eY + eZ;

            return corners;
        }

    }
}