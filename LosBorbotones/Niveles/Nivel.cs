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
using Examples.Optimizacion.Quadtree;

namespace AlumnoEjemplos.LosBorbotones.Niveles
{
    public class Nivel// : Circuito
    {
        private TgcSkyBox cielo;
        public TgcBoundingBox escenarioBB = new TgcBoundingBox (new Vector3(-8000,-1500,-6000), new Vector3(8000,700,6000));
        private List<TgcBox> cajas = new List<TgcBox>();
        private List<TgcSimpleTerrain> terrenos = new List<TgcSimpleTerrain>();
        private List<TgcMesh> elementos = new List<TgcMesh>();
        public List<TgcMesh> todosLosMeshes = new List<TgcMesh>();
        public List<ObstaculoRigido> obstaculos = new List<ObstaculoRigido>(); //Coleccion de objetos para colisionar
        public List<Recursos> recursos = new List<Recursos>(); //Coleccion de objetos para agarrar
        public List<Checkpoint> checkpoints = new List<Checkpoint>(); //Coleccion de objetos para agarrar
        public TgcText2d checkpointsRestantes;
        public Checkpoint checkpointActual;
        string texturesPath = GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\";
        EjemploAlumno EjemploAlu = EjemploAlumno.getInstance();
        List<Vector3> PosicionesCheckpoints = new List<Vector3>();        
        public Quadtree quadtree;
        public List<TgcMesh> objetos = new List<TgcMesh>();
		
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
            currentHeightmap = Shared.mediaPath  + "heighmap.jpg";
            currentScaleXZ = 12f;
            currentScaleY = 2.2f;
            currentTexture = Shared.mediaPath + "block02.png";

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
                arbol.Scale = new Vector3(23f, 23f,23f);
                elementos.Add(arbol);
                separacionEntreArboles += 500f;
                inclinacionFila += 60f;
            }   
            
            TgcTexture textura = TgcTexture.createTexture(GuiController.Instance.D3dDevice, GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\pista3.jpg");
            piso = TgcBox.fromSize(new Vector3(0, 0, 0), new Vector3(15000, 0, 10000), textura); //es un cubo plano con una textura (foto de la pista)

            cielo = new TgcSkyBox(); //Se crea el cielo, es como un cubo grande que envuelve todo y sirve de límite
            cielo.Center = new Vector3(0, 0, 0);
            cielo.Size = new Vector3(18000, 2000, 15000);
            cielo.setFaceTexture(TgcSkyBox.SkyFaces.Up, Shared.mediaPath + "cielo.jpg");
            cielo.setFaceTexture(TgcSkyBox.SkyFaces.Down, Shared.mediaPath + "cielo.jpg");
            cielo.setFaceTexture(TgcSkyBox.SkyFaces.Left, Shared.mediaPath + "cielo.jpg");
            cielo.setFaceTexture(TgcSkyBox.SkyFaces.Right, Shared.mediaPath + "cielo.jpg");
            cielo.setFaceTexture(TgcSkyBox.SkyFaces.Front, Shared.mediaPath + "cielo.jpg");
            cielo.setFaceTexture(TgcSkyBox.SkyFaces.Back, Shared.mediaPath + "cielo.jpg");
            cielo.updateValues();

           
            cajas.Add(piso);

            //CARGAR OBSTÁCULOS
            /*obstaculos.Add(new ObstaculoRigido(100, 0, -600, 80, 300, 80, GuiController.Instance.ExamplesMediaDir + "Texturas\\granito.jpg"));
            obstaculos.Add(new ObstaculoRigido(400, 0, 1000, 80, 300, 80, GuiController.Instance.ExamplesMediaDir + "Texturas\\madera.jpg"));
            obstaculos.Add(new ObstaculoRigido(3200, 0, 2000, 1200, 300, 80, GuiController.Instance.ExamplesMediaDir + "Texturas\\madera.jpg"));
            obstaculos.Add(new ObstaculoRigido(3200, 0, 2000, 300, 1200, 80, GuiController.Instance.ExamplesMediaDir + "Texturas\\madera.jpg"));*/
            obstaculos.Add(new ObstaculoRigido(-100, 0, -1800, 3700, 300, 80, Shared.mediaPath + "block01.jpg"));
            obstaculos.Add(new ObstaculoRigido(-1300, 0, -100, 80 , 300 , 3200, Shared.mediaPath + "block01.jpg"));
          
            obstaculos.Add(new ObstaculoRigido(Shared.mediaPath + "columna\\columna-TgcScene.xml", new Vector3(5650, 0, -3000), new Vector3(15f, 15f, 15f)));
            obstaculos.Add(new ObstaculoRigido(Shared.mediaPath + "columna\\columna-TgcScene.xml", new Vector3(5500, 0, -3250), new Vector3(10f, 10f, 10f)));
            obstaculos.Add(new ObstaculoRigido(Shared.mediaPath + "columna\\columna-TgcScene.xml", new Vector3(5850, 0, -3000), new Vector3(5f, 5f, 5f)));

            obstaculos.Add(new ObstaculoRigido(Shared.mediaPath + "carnivora\\carnivora-TgcScene.xml", new Vector3(2000, 0, 0), new Vector3(7f, 7f, 7f)));
            ObstaculoRigido p = new ObstaculoRigido(Shared.mediaPath + "carnivora\\carnivora-TgcScene.xml", new Vector3(2200, 0, 100), new Vector3(5f, 5f, 5f));
            p.mesh.rotateY(0.5f);
            obstaculos.Add(p);
           
            //guardabarros
            obstaculos.Add(new ObstaculoRigido(7625, -400, 0, 250, 1100, 10000, Shared.mediaPath + "block01.jpg"));
            obstaculos.Add(new ObstaculoRigido(-7625, -400, 0, 250, 1100, 10000, Shared.mediaPath + "block01.jpg"));
            obstaculos.Add(new ObstaculoRigido(0, -400, 5125, 15000, 1100, 250, Shared.mediaPath + "block01.jpg"));
            obstaculos.Add(new ObstaculoRigido(0, -400, -5125, 15000, 1100, 250, Shared.mediaPath + "block01.jpg"));
            
            /*ObstaculoRigido b = new ObstaculoRigido(0", -400, -5125, 15000, 1100, 250, Shared.mediaPath + "block01.jpg);
            Vector2 posicionbarra = (Vector2)GuiController.Instance.Modifiers[" PosicionFlechaDebug"];
            b.nuevaPos(posicionbarra);
            obstaculos.Add(b);*/


            //Checkpoints
            this.PosicionesCheckpoints.Add(new Vector3(5300, -4000, 0));
            this.PosicionesCheckpoints.Add(new Vector3(0, 0, 0));
            this.PosicionesCheckpoints.Add(new Vector3(6000, 2500, 0));
            this.PosicionesCheckpoints.Add(new Vector3(-5000, 4500, 0));
            this.PosicionesCheckpoints.Add(new Vector3(-5000, 1750, 0));
            this.PosicionesCheckpoints.Add(new Vector3(-2500, -500, 0));
            this.PosicionesCheckpoints.Add(new Vector3(-5500, -2500, 0));
            this.PosicionesCheckpoints.Add(new Vector3(-5000, -4500, 0));
            this.PosicionesCheckpoints.Add(new Vector3(0, -2500, 0));

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
           /* TgcMesh hongoRojoMesh = EjemploAlumno.getInstance().getHongoRojo();
            TgcMesh hongoVerdeMesh = EjemploAlumno.getInstance().getHongoVerde();
            // Recursos hongoVerde = new Recursos(800, 1350, 0, hongoVerdeMesh);
            Recursos hongoRojo = new Recursos(1200, -50, 0, hongoRojoMesh);
            //recursos.Add(hongoRojo);

            */
           ObstaculoRigido hV = new ObstaculoRigido(Shared.mediaPath + "HongoVerde\\HongoVerde-TgcScene.xml", new Vector3(-4300, 0, -300), new Vector3(2f, 2f, 2f));
           hV.mesh.rotateY(0.2f);
            obstaculos.Add(hV);
            obstaculos.Add(new ObstaculoRigido(Shared.mediaPath + "HongoRojo\\HongoRojo-TgcScene.xml", new Vector3(-4200, 0, -300), new Vector3(0.5f, 0.5f, 0.5f)));
            obstaculos.Add(new ObstaculoRigido(Shared.mediaPath + "HongoRojo\\HongoRojo-TgcScene.xml", new Vector3(-4300, 0, -400), new Vector3(1.2f, 1.2f, 1.2f)));

            obstaculos.Add(new ObstaculoRigido(Shared.mediaPath + "HongoRojo\\HongoRojo-TgcScene.xml", new Vector3(-5000, 0, 3000), new Vector3(2f, 2f, 2f)));
            obstaculos.Add(new ObstaculoRigido(Shared.mediaPath + "HongoVerde\\HongoVerde-TgcScene.xml", new Vector3(-5100, 0, 3000), new Vector3(0.5f, 0.5f, 0.5f)));
            obstaculos.Add(new ObstaculoRigido(Shared.mediaPath + "HongoVerde\\HongoVerde-TgcScene.xml", new Vector3(-5100, 0, 3000), new Vector3(1.5f, 1.5f, 1.5f)));
            obstaculos.Add(new ObstaculoRigido(Shared.mediaPath + "HongoVerde\\HongoVerde-TgcScene.xml", new Vector3(-4900, 0, 3100), new Vector3(0.2f, 0.2f, 0.2f)));
            
            ObstaculoRigido b = new ObstaculoRigido(Shared.mediaPath + "bar\\bar-TgcScene.xml", new Vector3(2300, 0, 2500), new Vector3(65f, 15f, 20f));
            b.mesh.rotateY(-0.2f);
            b.obb.rotate(new Vector3(0f,-0.2f,0f));
            b.mesh.createBoundingBox();
            obstaculos.Add(b);


            foreach (ObstaculoRigido obstaculo in obstaculos)
            {
                objetos.Add(obstaculo.mesh);
            }
            foreach (TgcMesh elemento in elementos)
            {
                objetos.Add(elemento);
            }
            foreach (Recursos recurso in recursos)
            {
                objetos.Add(recurso.modelo);
            }

            //Crear grilla
            quadtree = new Quadtree();
            quadtree.create(objetos, escenarioBB);
            quadtree.createDebugQuadtreeMeshes();

            GuiController.Instance.Modifiers.addBoolean("showQuadtree", "Show Quadtree", false);


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
            int i=0;
             foreach (Vector3 Posicion in this.PosicionesCheckpoints)
             { 
                 List<TgcScene> moneditas = EjemploAlu.getMonedas();
                 TgcMesh monedita =  moneditas[i].Meshes[0]; 
                 this.checkpoints.Add(new Checkpoint(Posicion.X, Posicion.Y, Posicion.Z, monedita));
                 i++;
             }
           /* int i;
            for (i=0; i < PosicionesCheckpoints.Count(); i++)
            {
               
                this.checkpoints.Add(new Checkpoint(PosicionesCheckpoints[i].X, PosicionesCheckpoints[i].Y, PosicionesCheckpoints[i].Z, monedita));
            }*/
        }

        private void crearNivel2()
        {
            //Construcción del escenario del nivel 1
            
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

            cajas.Add(piso);
            
            terrenos.Add(terrain);

        }


        public string getNombre()
        {
            return "Nivel";
        }

       
        public void render(float elapsedTime)
        {
            bool showQuadtree = (bool)GuiController.Instance.Modifiers["showQuadtree"];
            TgcThirdPersonCamera camera = GuiController.Instance.ThirdPersonCamera;
            //TgcFrustum frustum = GuiController.Instance.Frustum;
            quadtree.render(GuiController.Instance.Frustum, showQuadtree);
            //foreach (TgcMesh mesh in todosLosMeshes)
            //{
            //    //Solo mostrar la malla si colisiona contra el Frustum
            //    TgcCollisionUtils.FrustumResult r = TgcCollisionUtils.classifyFrustumAABB(frustum, mesh.BoundingBox);
            //    if (r != TgcCollisionUtils.FrustumResult.OUTSIDE)
            //    {
            //        Vector3 q;
            //        if (!TgcCollisionUtils.intersectSegmentAABB(camera.Position, camera.Target, mesh.BoundingBox, out q))
            //        {
            //            mesh.render();
            //        }
            //        if (Shared.debugMode)
            //        {
            //                mesh.BoundingBox.render();
            //        }

            //    }
            //}

            if (Shared.debugMode)
            {
                foreach (TgcMesh objeto in objetos)
                {
                    objeto.BoundingBox.render();
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
            checkpointActual.obb.rotate(new Vector3(0, 5f * elapsedTime, 0));
            if(Shared.debugMode) checkpointActual.obb.render();
           
            cielo.render();
        }
    }
}
