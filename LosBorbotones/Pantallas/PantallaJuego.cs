using AlumnoEjemplos.LosBorbotones.Sonidos;
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



namespace AlumnoEjemplos.LosBorbotones.Pantallas
{
    class PantallaJuego : Pantalla
    { 
        private TgcD3dInput entrada;
        private Auto auto;
        private Musica musica;
        private Nivel1 nivel;
        private List<Renderizable> renderizables = new List<Renderizable>();     //Coleccion de objetos que se dibujan
        private List<ObstaculoRigido> obstaculos = new List<ObstaculoRigido>();  //Coleccion de objetos para colisionar

        public PantallaJuego(Auto autito)
        {
            /*En PantallaInicio le paso a Pantalla juego con qué auto jugar. Acá lo asigno a la pantalla, cargo el coso
             que capta el teclado, creo el Nivel1 y lo pongo en la lista de renderizables, para que sepa con qué 
             escenario cargarse */

            this.auto = autito;
            this.entrada = GuiController.Instance.D3dInput;
            this.nivel = EjemploAlumno.getInstance().getNiveles(0);
            // this.renderizables.Add(EjemploAlumno.getInstance().getNiveles(1));
           

            // CAMARA TERCERA PERSONA
            GuiController.Instance.ThirdPersonCamera.Enable = true;
            GuiController.Instance.ThirdPersonCamera.resetValues();
            GuiController.Instance.ThirdPersonCamera.setCamera(auto.mesh.Position, 300, 700);

            //CARGAR MÚSICA.          
            Musica track = new Musica("ramones.mp3");
            this.musica = track;
            musica.playMusica();
            musica.setVolume(30);
            

            //MENSAJE CONSOLA
            GuiController.Instance.Logger.log("  [WASD] Controles Vehículo " + Environment.NewLine + "  [M] Música On/Off"
                + Environment.NewLine + "[Q] Volver al menú principal");

            //CARGAR OBSTÁCULOS
            obstaculos.Add(new ObstaculoRigido(50, 0, 800, 80, 300, 80, GuiController.Instance.ExamplesMediaDir + "Texturas\\baldosaFacultad.jpg"));
            obstaculos.Add(new ObstaculoRigido(100, 0, -600, 80, 300, 80, GuiController.Instance.ExamplesMediaDir + "Texturas\\granito.jpg"));
            obstaculos.Add(new ObstaculoRigido(400, 0, 1000, 80, 300, 80, GuiController.Instance.ExamplesMediaDir + "Texturas\\madera.jpg"));
    
        }



        public void render(float elapsedTime)
        {
            //moverse y rotar son variables que me indican a qué velocidad se moverá o rotará el mesh respectivamente.
            //Se inicializan en 0, porque por defecto está quieto.
            float moverse = 0f;
            float rotar = 0f;

            //Procesa las entradas del teclado.          
            if (entrada.keyDown(Key.Q))
            {
                GuiController.Instance.ThirdPersonCamera.resetValues();
                EjemploAlumno.getInstance().setPantalla(new PantallaInicio());
            }
            
            if(entrada.keyDown(Key.S))
           {
               moverse = auto.irParaAtras(elapsedTime);
           }
           if (entrada.keyDown(Key.W))
           {
               moverse = auto.irParaAdelante(elapsedTime);
           }
           if (entrada.keyDown(Key.A))
           {
               rotar = -auto.velocidadRotacion;
           }
           if (entrada.keyDown(Key.D))
           {
               rotar = auto.velocidadRotacion;
           }
           if (entrada.keyPressed(Key.M))
           {
               musica.muteUnmute();
           }
           if (entrada.keyPressed(Key.R)) //boton de reset, el mesh vuelve a la posicion (0,0,0)
           {
               Vector3 posicionInicio = new Vector3(0,0,0);
               auto.velocidadActual = 0;
               auto.mesh.Position = posicionInicio;
               GuiController.Instance.ThirdPersonCamera.Target = posicionInicio;
           }

            
            //Frenado por inercia
           if (!entrada.keyDown(Key.W) && !entrada.keyDown(Key.S) && auto.velocidadActual != 0)
           {
              moverse = auto.frenarPorInercia(elapsedTime);
           }
           if (moverse > auto.velocidadMaxima)
           {
               auto.velocidadActual = auto.velocidadMaxima;
           }
            if(moverse < (-auto.velocidadMaxima))
            {
                auto.velocidadActual = -auto.velocidadMaxima;
            }
         
           if (rotar != 0) //Si hubo rotacion,
           {
               float rotAngle = Geometry.DegreeToRadian(rotar * elapsedTime);
               auto.mesh.rotateY(rotAngle); //roto el auto
               GuiController.Instance.ThirdPersonCamera.rotateY(rotAngle); //y la cámara
           }
           if (moverse != 0) //Si hubo movimiento
           {
               Vector3 lastPos = auto.mesh.Position;
               auto.mesh.moveOrientedY(moverse * elapsedTime); //muevo el auto

               //Detectar colisiones de BoundingBox utilizando herramienta TgcCollisionUtils
               bool collide = false;
               foreach (ObstaculoRigido obstaculo in obstaculos)
               {
                   TgcCollisionUtils.BoxBoxResult result = TgcCollisionUtils.classifyBoxBox(auto.mesh.BoundingBox, obstaculo.box.BoundingBox);
                   if (result == TgcCollisionUtils.BoxBoxResult.Adentro || result == TgcCollisionUtils.BoxBoxResult.Atravesando)
                   {
                       collide = true;
                       break;
                   }
               }


               //Si hubo colision, restaurar la posicion anterior
               if (collide)
               {
                   auto.mesh.Position = lastPos;
                   auto.velocidadActual = 0; //frena al auto
               }
           }

           GuiController.Instance.ThirdPersonCamera.Target = auto.mesh.Position;

            //dibuja el auto
            auto.mesh.render();
            //dibuja el nivel
            nivel.render();
            
            foreach (ObstaculoRigido obstaculo in this.obstaculos)
                obstaculo.render();
        }

    }
}
    
