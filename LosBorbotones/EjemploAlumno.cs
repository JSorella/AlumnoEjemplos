using AlumnoEjemplos.LosBorbotones.Pantallas;
using TgcViewer;
using System;
using TgcViewer.Example;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Sound;
using System.Collections.Generic;
using Microsoft.DirectX;
using AlumnoEjemplos.LosBorbotones.Sonidos;
using AlumnoEjemplos.LosBorbotones.Autos;
using AlumnoEjemplos.LosBorbotones.Niveles;

namespace AlumnoEjemplos.LosBorbotones
{
    public class EjemploAlumno : TgcExample
    {
        private Pantalla pantalla;
        private List<Auto> autos = new List<Auto>() ;
        private List<Nivel1> niveles = new List<Nivel1>();
       
       public static EjemploAlumno instance;

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
            return "LosBorbotones";
        }

        public override string getDescription()
        {
            return "Trabajo práctico de Técnicas de Gráficos por Computadora";
        }

        public override void init()
        {
            instance = this;
            pantalla = new PantallaInicio();
            
            //Crea los autos
            string pathAutoMario= GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\TanqueFuturistaRuedas\\TanqueFuturistaRuedas-TgcScene.xml";
            string pathAutoLuigi = GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\CamionDeAgua\\" + "CamionDeAgua-TgcScene.xml"; ;
            Auto autoMario = new Auto(pathAutoMario, "Mario", new Vector3(0, 0, 0), 60000, 100, 200, 500);
            Auto autoLuigi = new Auto(pathAutoMario, "Luiigi", new Vector3(0, 0, 0), 60000, 100, 200, 500);
            this.autos.Add(autoMario);
            this.autos.Add(autoLuigi);

            //Crea el circuito

            Nivel1 nivel1 = new Nivel1();
            this.niveles.Add(nivel1);
            

            Console.WriteLine("[WASD] Controles Vehículo - [M] Música On/Off");

        }

        public Auto getAutos(int posicion)
        {
            return this.autos[posicion];
        }

        public Nivel1 getNiveles(int posicion) 
        {
            return this.niveles[posicion];
        }
       
        public override void render(float elapsedTime)
        {
            pantalla.render(elapsedTime);
        }

        public override void close()
        {
            //corta la música al salir
            TgcMp3Player player = GuiController.Instance.Mp3Player;
            player.closeFile();
        }

        public void setPantalla(Pantalla _pantalla)
        {
            pantalla = _pantalla;
        }

    }
}
