using AlumnoEjemplos.MiGrupo.Pantallas;
using TgcViewer;
using TgcViewer.Example;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.MiGrupo
{
    public class EjemploAlumno : TgcExample
    {
        private Pantalla pantalla;
        private TgcScene autos;
        private static EjemploAlumno instance;

        public static EjemploAlumno getInstance()
        {
            return instance;
        }

        public override string getCategory()
        {
            return "AlumnoEjemplos";
        }

        public override string getName()
        {
            return "MiGrupo";
        }

        public override string getDescription()
        {
            return "Trabajo práctico de Técnicas de Gráficos por Computadora";
        }

        public override void init()
        {
            instance = this;
            //CREA ACA TODOS AUTOS. Capaz en un futuro puedo agregar que cree los niveles también.
            pantalla = new PantallaInicio();
            string pathAutoMario= GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\TanqueFuturistaRuedas\\TanqueFuturistaRuedas-TgcScene.xml";
            string pathAutoLuigi = GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\CamionDeAgua\\" + "CamionDeAgua-TgcScene.xml"; ;
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene _autos = loader.loadSceneFromFile(pathAutoMario);
            TgcScene luigi = loader.loadSceneFromFile(pathAutoLuigi);
            _autos.Meshes.Add(luigi.Meshes[0]);

            this.autos = _autos;

        }
       

        public TgcMesh getAutos(int posicion) 
        {
            //La clase tiene un atributo "autos" de tipo TgcScene. Una TgcScene es un array de TgcMesh (meshes).
            //dada una posición, te devuelve el mesh que hay en autos en esa posicion.
           return autos.Meshes[posicion];
        }
        public override void render(float elapsedTime)
        {
            pantalla.render(elapsedTime);
        }

        public override void close()
        {
            //
        }

        public void setPantalla(Pantalla _pantalla)
        {
            pantalla = _pantalla;
        }

    }
}
