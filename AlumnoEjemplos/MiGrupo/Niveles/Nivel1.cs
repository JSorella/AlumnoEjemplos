using Microsoft.DirectX;
using TgcViewer;
using TgcViewer.Utils.Terrain;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.MiGrupo.Niveles
{
    class Nivel1 : Circuito
    {
        private TgcBox piso;
        private TgcSkyBox cielo;
        private TgcBox caja1;
        private TgcBox caja2;

        public Nivel1()
        {
            //Construcción del escenario del nivel 1

            TgcTexture textura = TgcTexture.createTexture(GuiController.Instance.D3dDevice, GuiController.Instance.AlumnoEjemplosMediaDir + "MiGrupo\\pista3.jpg");
            piso = TgcBox.fromSize(new Vector3(0, 0, 0), new Vector3(15000, 0, 5000), textura); //es un cubo plano con una textura (foto de la pista)



            cielo = new TgcSkyBox(); //Se crea el cielo, es como un cubo grande que envuelve todo y sirve de límite
            cielo.Center = new Vector3(0, 0, 0);
            cielo.Size = new Vector3(15000, 5000, 5000);
            string texturesPath = GuiController.Instance.AlumnoEjemplosMediaDir + "MiGrupo\\";
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

            TgcTexture textura2 = TgcTexture.createTexture(GuiController.Instance.D3dDevice, GuiController.Instance.AlumnoEjemplosMediaDir + "MiGrupo\\honguito.jpg");
            caja2 = TgcBox.fromSize(new Vector3(200, 0, 160), new Vector3(70, 70, 70), textura2);
           
        }

        public string getNombre()
        {
            return "Nivel 1";
        }

       
        public void render()
        {
            piso.render();
            caja1.render();
            caja2.render();
            cielo.render();
        }
    }
}
