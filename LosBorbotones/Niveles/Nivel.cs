using Microsoft.DirectX;
using TgcViewer;
using TgcViewer.Utils.Terrain;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using System.Collections.Generic;

namespace AlumnoEjemplos.LosBorbotones.Niveles
{
    public class Nivel : Circuito
    {
        private TgcSkyBox cielo;
        private List<TgcBox> cajas = new List<TgcBox>();
        private List<TgcSimpleTerrain> terrenos = new List<TgcSimpleTerrain>();
        private List<TgcMesh> elementos = new List<TgcMesh>();
        private TgcObb obb;

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
        }

        private void crearNivel1(  )
        {
            //Construcción del escenario del nivel 1
            TgcBox caja1;
            TgcBox caja2;
            TgcBox piso;

            TgcMesh hongo = EjemploAlumno.getInstance().getHongo(0);

            elementos.Add(hongo);
            TgcMesh arbol = EjemploAlumno.getInstance().getArbol();
            TgcMesh arbol2 = arbol;
            arbol.Position = new Vector3(460, 0, 1000);
           this.elementos.Add(arbol2);
            arbol.Position = new Vector3(1760, 0, 1050);
            this.elementos.Add(arbol);
          //  arbol.Position = new Vector3(1060, 0, 1050);
            this.elementos.Add(arbol);

            TgcTexture textura = TgcTexture.createTexture(GuiController.Instance.D3dDevice, GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\pista3.jpg");
            piso = TgcBox.fromSize(new Vector3(0, 0, 0), new Vector3(15000, 0, 5000), textura); //es un cubo plano con una textura (foto de la pista)

            cielo = new TgcSkyBox(); //Se crea el cielo, es como un cubo grande que envuelve todo y sirve de límite
            cielo.Center = new Vector3(0, 0, 0);
            cielo.Size = new Vector3(15000, 5000, 5000);
            string texturesPath = GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\";
            cielo.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "cielo.jpg");
            cielo.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "cielo.jpg");
            cielo.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "cielo.jpg");
            cielo.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "cielo.jpg");
            cielo.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "cielo.jpg");
            cielo.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "cielo.jpg");
            cielo.updateValues();

            

            //son las dos cajitas que se ven
            TgcTexture textura1 = TgcTexture.createTexture(GuiController.Instance.D3dDevice, GuiController.Instance.ExamplesMediaDir + "\\Texturas\\madera.jpg");
            caja1 = TgcBox.fromSize(new Vector3(100, 0, 100), new Vector3(50, 50, 50), textura1);

            TgcTexture textura2 = TgcTexture.createTexture(GuiController.Instance.D3dDevice, GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\honguito.jpg");
            caja2 = TgcBox.fromSize(new Vector3(200, 0, 160), new Vector3(70, 70, 70), textura2);

            cajas.Add(caja1);
            cajas.Add(caja2);
            cajas.Add(piso);
        }

        private void crearNivel2()
        {
            //Construcción del escenario del nivel 1
            TgcBox caja1;
            TgcBox caja2;
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

            cielo = new TgcSkyBox(); //Se crea el cielo, es como un cubo grande que envuelve todo y sirve de límite
            cielo.Center = new Vector3(0, 0, 0);
            cielo.Size = new Vector3(15000, 5000, 5000);
            string texturesPath = GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\";
            cielo.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "cielo.jpg");
            cielo.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "cielo.jpg");
            cielo.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "cielo.jpg");
            cielo.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "cielo.jpg");
            cielo.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "cielo.jpg");
            cielo.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "cielo.jpg");
            cielo.updateValues();

            //son las dos cajitas que se ven
            TgcTexture textura1 = TgcTexture.createTexture(GuiController.Instance.D3dDevice, GuiController.Instance.ExamplesMediaDir + "\\Texturas\\madera.jpg");
            caja1 = TgcBox.fromSize(new Vector3(100, 0, 100), new Vector3(50, 50, 50), textura1);

            TgcTexture textura2 = TgcTexture.createTexture(GuiController.Instance.D3dDevice, GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\honguito.jpg");
            caja2 = TgcBox.fromSize(new Vector3(200, 0, 160), new Vector3(70, 70, 70), textura2);

            cajas.Add(caja1);
            cajas.Add(caja2);
            cajas.Add(piso);
            terrenos.Add(terrain);

        }


        public string getNombre()
        {
            return "Nivel";
        }

       
        public void render()
        {
            foreach (TgcBox caja in this.cajas)
            {
                caja.render();
            }
            foreach (TgcSimpleTerrain terreno in this.terrenos)
            {
                terreno.render();
            }

            foreach (TgcMesh elemento in this.elementos)
            {
                elemento.render();
            }
            
            
            cielo.render();
            
        }
    }
}
