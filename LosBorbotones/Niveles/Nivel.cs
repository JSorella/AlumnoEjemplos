using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AlumnoEjemplos.LosBorbotones.Colisionables;
using Microsoft.DirectX;
using TgcViewer;
using TgcViewer.Utils._2D;
using TgcViewer.Utils.Input;
using TgcViewer.Utils.Terrain;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.LosBorbotones.Niveles
{
    public class Nivel// : Circuito
    {
        private TgcSkyBox cielo;
        private List<TgcBox> cajas = new List<TgcBox>();
        private List<TgcSimpleTerrain> terrenos = new List<TgcSimpleTerrain>();
        private List<TgcMesh> elementos = new List<TgcMesh>();
        private List<TgcMesh> todosLosMeshes = new List<TgcMesh>();
        public List<ObstaculoRigido> obstaculos = new List<ObstaculoRigido>(); //Coleccion de objetos para colisionar
        public List<Recursos> recursos = new List<Recursos>(); //Coleccion de objetos para agarrar
        public List<Recursos> checkpoints = new List<Recursos>(); //Coleccion de objetos para agarrar
        public TgcText2d checkpointsRestantes;
        public Recursos checkpointActual;
        string texturesPath = GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\";
        EjemploAlumno EjemploAlu = EjemploAlumno.getInstance();
        List<Vector3> PosicionesCheckpoints = new List<Vector3>();        
        
        public Nivel(int numeroNivel)
        {
            switch (numeroNivel)
            {
                case 1:
                    crearNivel1();
                    break;
                case 2:
                    crearNivel2();
                    break;
            }

            //Meshes
            int i = 0;
            obstaculos.ForEach(obst => { todosLosMeshes.Add(obst.mesh); i++; });
            elementos.ForEach(elemento => todosLosMeshes.Add(elemento));
            recursos.ForEach(elemento => todosLosMeshes.Add(elemento.modelo));
        }

        private void crearNivel1(  )
        {
            //Construcción del escenario del nivel 1
            //TgcBox caja1;
            //TgcBox caja2;
            TgcBox piso;

            // ----- TUNEL ----- //
            TgcSimpleTerrain terrain;
            string currentHeightmap;
            string currentTexture;
            float currentScaleXZ;
            float currentScaleY;
           
            //Path de Heightmap default del terreno y Modifier para cambiarla
            currentHeightmap = Shared.texturesPath  + "heighmap.jpg";
            currentScaleXZ = 12f;
            currentScaleY = 2.2f;
            currentTexture = Shared.texturesPath + "block02.png";

            //Cargar terreno: cargar heightmap y textura de color
            terrain = new TgcSimpleTerrain();
            terrain.loadHeightmap(currentHeightmap, currentScaleXZ, currentScaleY, new Vector3(0, 0, -300));
            terrain.loadTexture(currentTexture);

            //elementos.Add(hongo);
            List<TgcScene> arboles = EjemploAlumno.getInstance().getArboles();
            float separacionEntreArboles = 0f;
            float inclinacionFila = 0f;
            foreach (TgcScene escenaArbol in arboles) 
            {
                TgcMesh arbol = escenaArbol.Meshes[0];
                arbol.Position= new Vector3(600+separacionEntreArboles, 0, 2400+inclinacionFila);
                elementos.Add(arbol);
                separacionEntreArboles += 500f;
                inclinacionFila += 60f;
            }

            List<TgcScene> columnas = EjemploAlumno.getInstance().getColumnas();
            int l = 1;
            foreach (TgcScene escenaColumna in columnas)
            {
                TgcMesh columna = escenaColumna.Meshes[0];

                if (l == 1)
                {
                    columna.Position = new Vector3(900, 0, 1900);
                    columna.Scale = new Vector3(5f, 5f, 5f);
                }
                if (l == 2)
                {
                    columna.Position = new Vector3(1000, 0, 1000);
                    columna.Scale = new Vector3(5f, 5f, 5f);
                }
                if (l == 3)
                {
                    columna.Position = new Vector3(1000, 0, 800);
                    columna.Scale = new Vector3(5f, 5f, 5f);
                }
                obstaculos.Add(new ObstaculoRigido (columna));
                l++;
            }            
            
            TgcTexture textura = TgcTexture.createTexture(GuiController.Instance.D3dDevice, GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\pista3.jpg");
            piso = TgcBox.fromSize(new Vector3(0, 0, 0), new Vector3(15000, 0, 10000), textura); //es un cubo plano con una textura (foto de la pista)

            cielo = new TgcSkyBox(); //Se crea el cielo, es como un cubo grande que envuelve todo y sirve de límite
            cielo.Center = new Vector3(0, 0, 0);
            cielo.Size = new Vector3(20000, 5000, 20000);
            cielo.setFaceTexture(TgcSkyBox.SkyFaces.Up, Shared.texturesPath + "cielo.jpg");
            cielo.setFaceTexture(TgcSkyBox.SkyFaces.Down, Shared.texturesPath + "cielo.jpg");
            cielo.setFaceTexture(TgcSkyBox.SkyFaces.Left, Shared.texturesPath + "cielo.jpg");
            cielo.setFaceTexture(TgcSkyBox.SkyFaces.Right, Shared.texturesPath + "cielo.jpg");
            cielo.setFaceTexture(TgcSkyBox.SkyFaces.Front, Shared.texturesPath + "cielo.jpg");
            cielo.setFaceTexture(TgcSkyBox.SkyFaces.Back, Shared.texturesPath + "cielo.jpg");
            cielo.updateValues();

           
            cajas.Add(piso);

            //CARGAR OBSTÁCULOS
            obstaculos.Add(new ObstaculoRigido(50, 0, 800, 80, 300, 80, GuiController.Instance.ExamplesMediaDir + "Texturas\\baldosaFacultad.jpg"));
            obstaculos.Add(new ObstaculoRigido(100, 0, -600, 80, 300, 80, GuiController.Instance.ExamplesMediaDir + "Texturas\\granito.jpg"));
            obstaculos.Add(new ObstaculoRigido(400, 0, 1000, 80, 300, 80, GuiController.Instance.ExamplesMediaDir + "Texturas\\madera.jpg"));
            obstaculos.Add(new ObstaculoRigido(3200, 0, 2000, 1200, 300, 80, GuiController.Instance.ExamplesMediaDir + "Texturas\\madera.jpg"));
            obstaculos.Add(new ObstaculoRigido(3200, 0, 2000, 300, 1200, 80, GuiController.Instance.ExamplesMediaDir + "Texturas\\madera.jpg"));
            obstaculos.Add(new ObstaculoRigido(-100, 0, -1800, 3700, 300, 80, Shared.texturesPath + "block01.jpg"));

            //guardabarros
            obstaculos.Add(new ObstaculoRigido(7625, -400, 0, 250, 1100, 10000, Shared.texturesPath + "block01.jpg"));
            obstaculos.Add(new ObstaculoRigido(-7625, -400, 0, 250, 1100, 10000, Shared.texturesPath + "block01.jpg"));
            obstaculos.Add(new ObstaculoRigido(0, -400, 5125, 15000, 1100, 250, Shared.texturesPath + "block01.jpg"));
            obstaculos.Add(new ObstaculoRigido(0, -400, -5125, 15000, 1100, 250, Shared.texturesPath + "block01.jpg"));

            //Checkpoints
            this.PosicionesCheckpoints.Add(new Vector3(2000, 80, 100));
            this.PosicionesCheckpoints.Add(new Vector3(6000, 80, 100));
            this.PosicionesCheckpoints.Add(new Vector3(-2000, 580, 100));
            this.PosicionesCheckpoints.Add(new Vector3(-2000, -80, 100));
            this.PosicionesCheckpoints.Add(new Vector3(1500, -80, 100));
            this.PosicionesCheckpoints.Add(new Vector3(2000, -580, 100));
            this.PosicionesCheckpoints.Add(new Vector3(0, 0, 100));

            this.agregarCheckpoints();

            checkpointActual = checkpoints.ElementAt(0);
            checkpointsRestantes = new TgcText2d();
            checkpointsRestantes.Text = checkpoints.Count().ToString();
            checkpointsRestantes.Color = Color.DarkRed;
            checkpointsRestantes.Align = TgcText2d.TextAlign.RIGHT;
            checkpointsRestantes.Position = new Point(630, 30);
            checkpointsRestantes.Size = new Size(100, 50);
            checkpointsRestantes.changeFont(new System.Drawing.Font("TimesNewRoman", 25, FontStyle.Bold));

            //Recursos
            TgcMesh hongoRojoMesh = EjemploAlumno.getInstance().getHongoRojo();
            TgcMesh hongoVerdeMesh = EjemploAlumno.getInstance().getHongoVerde();
            // Recursos hongoVerde = new Recursos(800, 1350, 0, hongoVerdeMesh);
            Recursos hongoRojo = new Recursos(1200, -50, 0, hongoRojoMesh);
            //recursos.Add(hongoRojo);


            terrenos.Add(terrain);
        }


        public void reiniciar() 
        {
            checkpoints.RemoveRange(0, checkpoints.Count());
            this.agregarCheckpoints();
            checkpointsRestantes.Text = checkpoints.Count().ToString();
            checkpointActual = checkpoints.ElementAt(0);
            
        }





        public void agregarCheckpoints()
        {
            foreach (Vector3 Posicion in this.PosicionesCheckpoints)
            {
                this.checkpoints.Add(new Recursos(Posicion.X, Posicion.Y, Posicion.Z, texturesPath + "cuadritos.jpg", 1));
            }
        }

        private void crearNivel2()
        {
            //Construcción del escenario del nivel 1
            //TgcBox caja1;
            //TgcBox caja2;
            TgcBox piso;

            TgcSimpleTerrain terrain;
            string currentHeightmap;
            string currentTexture;
            float currentScaleXZ;
            float currentScaleY;

            //Path de Heightmap default del terreno y Modifier para cambiarla
            currentHeightmap = GuiController.Instance.ExamplesMediaDir + "Heighmaps\\" + "Heightmap2.jpg";
            //GuiController.Instance.Modifiers.addTexture("heightmap", currentHeightmap);

            //Modifiers para variar escala del mapa
            currentScaleXZ = 20f;
            //GuiController.Instance.Modifiers.addFloat("scaleXZ", 0.1f, 100f, currentScaleXZ);
            currentScaleY = 1.3f;
           // GuiController.Instance.Modifiers.addFloat("scaleY", 0.1f, 10f, currentScaleY);

            //Path de Textura default del terreno y Modifier para cambiarla
            currentTexture = GuiController.Instance.ExamplesMediaDir + "Heighmaps\\" + "TerrainTexture2.jpg";
            //GuiController.Instance.Modifiers.addTexture("texture", currentTexture);

            //Cargar terreno: cargar heightmap y textura de color
            terrain = new TgcSimpleTerrain();
            terrain.loadHeightmap(currentHeightmap, currentScaleXZ, currentScaleY, new Vector3(50, -120, 50));
            terrain.loadTexture(currentTexture);

            TgcTexture textura = TgcTexture.createTexture(GuiController.Instance.D3dDevice, GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\pista3.jpg");
            piso = TgcBox.fromSize(new Vector3(0, 0, 0), new Vector3(15000, 0, 5000), textura); //es un cubo plano con una textura (foto de la pista)

            cielo = new TgcSkyBox();
            cielo.Center = new Vector3(0, 500, 0);
            cielo.Size = new Vector3(20000, 5000, 20000);
            string texturesPath = GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\SkyBox LostAtSeaDay\\";
            cielo.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "lostatseaday_up.jpg");
            cielo.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "lostatseaday_dn.jpg");
            cielo.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "lostatseaday_lf.jpg");
            cielo.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "lostatseaday_rt.jpg");
            cielo.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "lostatseaday_bk.jpg");
            cielo.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "lostatseaday_ft.jpg");
            cielo.updateValues();


            //son las dos cajitas que se ven
            /*TgcTexture textura1 = TgcTexture.createTexture(GuiController.Instance.D3dDevice, GuiController.Instance.ExamplesMediaDir + "\\Texturas\\madera.jpg");
            caja1 = TgcBox.fromSize(new Vector3(100, 0, 100), new Vector3(50, 50, 50), textura1);

            TgcTexture textura2 = TgcTexture.createTexture(GuiController.Instance.D3dDevice, GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\honguito.jpg");
            caja2 = TgcBox.fromSize(new Vector3(200, 0, 160), new Vector3(70, 70, 70), textura2);*/

            //cajas.Add(caja1);
            //cajas.Add(caja2);
            cajas.Add(piso);
            terrenos.Add(terrain);

        }


        public string getNombre()
        {
            return "Nivel";
        }

       
        public void render(float elapsedTime)
        {

            TgcThirdPersonCamera camera = GuiController.Instance.ThirdPersonCamera;
            TgcFrustum frustum = GuiController.Instance.Frustum;
            foreach (TgcMesh mesh in todosLosMeshes)
            {
                //Solo mostrar la malla si colisiona contra el Frustum
                TgcCollisionUtils.FrustumResult r = TgcCollisionUtils.classifyFrustumAABB(frustum, mesh.BoundingBox);
                if (r != TgcCollisionUtils.FrustumResult.OUTSIDE)
                {
                    Vector3 q;
                    if (!TgcCollisionUtils.intersectSegmentAABB(camera.Position, camera.Target, mesh.BoundingBox, out q))
                    {
                        mesh.render();
                    }
                    if (Shared.debugMode)
                    {
                            mesh.BoundingBox.render();
                    }

                }
            }
                
            foreach (TgcSimpleTerrain terreno in this.terrenos)
            {
                terreno.render();
            }

            foreach (TgcBox caja in this.cajas)
            {
                caja.render();
            }
            checkpointActual.render(elapsedTime);
            cielo.render();
        }
    }
}
