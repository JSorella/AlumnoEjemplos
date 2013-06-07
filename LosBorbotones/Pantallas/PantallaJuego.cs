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
using AlumnoEjemplos.LosBorbotones.Sonidos;



namespace AlumnoEjemplos.LosBorbotones.Pantallas
{
    class PantallaJuego : Pantalla
    { 
        private TgcD3dInput entrada;
        private Auto auto;
            
        private Musica musica;
        private Nivel nivel;
        private List<Renderizable> renderizables = new List<Renderizable>();     //Coleccion de objetos que se dibujan
        private List<ObstaculoRigido> obstaculos = new List<ObstaculoRigido>();  //Coleccion de objetos para colisionar
        private bool debugMode;
        private float auxRotation = 0f; 

        public PantallaJuego(Auto autito)
        {
            /*En PantallaInicio le paso a Pantalla juego con qué auto jugar. Acá lo asigno a la pantalla, cargo el coso
             que capta el teclado, creo el Nivel1 y lo pongo en la lista de renderizables, para que sepa con qué 
             escenario cargarse */

            this.auto = autito;
            //Computar OBB a partir del AABB del mesh. Inicialmente genera el mismo volumen que el AABB, pero luego te permite rotarlo (cosa que el AABB no puede)
            auto.obb = TgcObb.computeFromAABB(auto.mesh.BoundingBox);
         
            this.entrada = GuiController.Instance.D3dInput;
            this.nivel = EjemploAlumno.getInstance().getNiveles(0);
            // this.renderizables.Add(EjemploAlumno.getInstance().getNiveles(1));
           
            // CAMARA TERCERA PERSONA

            int offsetHeigth = 300;
            GuiController.Instance.Modifiers.addInt("AlturaCamara", 10, 1000, offsetHeigth);
            GuiController.Instance.ThirdPersonCamera.Enable = true;
            GuiController.Instance.ThirdPersonCamera.resetValues();
            GuiController.Instance.ThirdPersonCamera.setCamera( auto.mesh.Position , offsetHeigth, 700);
            
            

            //CARGAR MÚSICA.          
            Musica track = new Musica("ramones.mp3");
            this.musica = track;
            musica.playMusica();
            musica.setVolume(45);
            
            //MENSAJE CONSOLA
            GuiController.Instance.Logger.log("  [WASD] Controles Vehículo " 
                + Environment.NewLine + "  [M] Música On/Off"
                + Environment.NewLine + "  [R] Reset posición"
                + Environment.NewLine + "  [B] Modo Debug (muestra OBBs y otros datos útiles)"
                + Environment.NewLine + "  [Q] Volver al menú principal");

            //CARGAR OBSTÁCULOS
            obstaculos.Add(new ObstaculoRigido(50, 0, 800, 80, 300, 80, GuiController.Instance.ExamplesMediaDir + "Texturas\\baldosaFacultad.jpg"));
            obstaculos.Add(new ObstaculoRigido(100, 0, -600, 80, 300, 80, GuiController.Instance.ExamplesMediaDir + "Texturas\\granito.jpg"));
            obstaculos.Add(new ObstaculoRigido(400, 0, 1000, 80, 300, 80, GuiController.Instance.ExamplesMediaDir + "Texturas\\madera.jpg"));

            this.debugMode = false;
        }


        Vector3 corrimiento = new Vector3(1, 1, 1);
          
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
                //corta la música al salir
                TgcMp3Player player = GuiController.Instance.Mp3Player;
                player.closeFile();
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
           if (entrada.keyDown(Key.A) && (auto.velocidadActual>0.5 || auto.velocidadActual<-0.5))  //izquierda
           {
               rotar = -auto.velocidadRotacion;
           }
           if (entrada.keyDown(Key.D) && (auto.velocidadActual > 0.5 || auto.velocidadActual < -0.5))  //derecha
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
               auto.mesh.Rotation = new Vector3(0, 0, 0);
               auto.mesh.Position = posicionInicio;
               auto.obb = TgcObb.computeFromAABB(auto.mesh.BoundingBox);
               GuiController.Instance.ThirdPersonCamera.Target = posicionInicio;
               GuiController.Instance.ThirdPersonCamera.RotationY = 0;
           }
           if (entrada.keyPressed(Key.B)) //Modo debug para visualizar BoundingBoxes entre otras cosas que nos sirvan a nosotros
           {
               debugMode = !debugMode;
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

               auto.obb.rotate(new Vector3(0, rotAngle, 0)); // .. y el OBB!

               if (rotAngle > 0)
               {
                   if (auxRotation < rotAngle)
                   {
                       auxRotation = 0.65f * rotAngle;
                       GuiController.Instance.ThirdPersonCamera.Position = auto.mesh.Position - 0.8f * auto.mesh.Position;
                       GuiController.Instance.ThirdPersonCamera.rotateY(auxRotation);
                       auxRotation -= 0.152f * rotAngle;
                       GuiController.Instance.ThirdPersonCamera.Position = auto.mesh.Position - 0.8f * auto.mesh.Position;
                       GuiController.Instance.ThirdPersonCamera.rotateY(auxRotation);
                       auxRotation -= 0.152f * rotAngle;
                       GuiController.Instance.ThirdPersonCamera.Position = auto.mesh.Position - 0.8f * auto.mesh.Position;
                       GuiController.Instance.ThirdPersonCamera.rotateY(auxRotation);
                   }

                   GuiController.Instance.ThirdPersonCamera.Position = auto.mesh.Position;
               }

               else
               {
                   if (auxRotation > rotAngle)
                   {
                       auxRotation = 0.65f * FastMath.Abs(rotAngle);
                       GuiController.Instance.ThirdPersonCamera.Position = auto.mesh.Position - 0.8f * auto.mesh.Position;
                       GuiController.Instance.ThirdPersonCamera.rotateY(auxRotation);
                       auxRotation += 0.152f * rotAngle;
                       GuiController.Instance.ThirdPersonCamera.Position = auto.mesh.Position - 0.8f * auto.mesh.Position;
                       GuiController.Instance.ThirdPersonCamera.rotateY(auxRotation);
                       auxRotation += 0.152f * rotAngle;
                       GuiController.Instance.ThirdPersonCamera.Position = auto.mesh.Position - 0.8f * auto.mesh.Position;
                       GuiController.Instance.ThirdPersonCamera.rotateY(auxRotation);
                   }
               }
                 
                             

             

                /*
                auxRotation = 0.8f * rotAngle+ 0.3f*auxRotation;
                GuiController.Instance.ThirdPersonCamera.rotateY(auxRotation);
                //auxRotation *= 1.15f;
                  if (rotAngle > 0)
                {
                    while (auxRotation < rotAngle)
                    {
                        GuiController.Instance.ThirdPersonCamera.rotateY(auxRotation);
                        auxRotation += 0.2f*rotAngle;   //y la cámara
                        GuiController.Instance.ThirdPersonCamera.rotateY(auxRotation);
                    }
                }
                else 
                {
                    while (auxRotation > rotAngle)
                    {
                        GuiController.Instance.ThirdPersonCamera.rotateY(auxRotation);
                        auxRotation *= 1.16f;   //y la cámara
                    }
                }

                GuiController.Instance.ThirdPersonCamera.Target = auto.mesh.Position;
           */ }
            
                
               



           if (moverse != 0) //Si hubo movimiento
           {
               Vector3 lastPos = auto.mesh.Position;
               auto.mesh.moveOrientedY(moverse * elapsedTime); //muevo el auto

               // y muevo el OBB
               Vector3 position = auto.mesh.Position;
               Vector3 posDiff = position - lastPos;
               auto.obb.move(posDiff); 

               //Detectar colisiones de BoundingBox utilizando herramienta TgcCollisionUtils
               bool collide = false;
               foreach (ObstaculoRigido obstaculo in obstaculos)
               {
                   if (Colisiones.testObbObb2(auto.obb, obstaculo.obb)) //chequeo obstáculo por obstáculo si está chocando con auto
                   {
                       
                       collide = true;
                       break;
                   }
               }
               //Si hubo colision, restaurar la posicion anterior (sino sigo de largo)
               if (collide)
               {
                   auto.mesh.Position = lastPos;
                   moverse=auto.chocar(elapsedTime);
                 
               }

               //Efecto blur
               if ( FastMath.Abs(auto.velocidadActual) > (auto.velocidadMaxima*0.5555))
               {
                   EjemploAlumno.instance.activar_efecto = true;
                   EjemploAlumno.instance.blur_intensity = 0.003f * (float)Math.Round(FastMath.Abs(auto.velocidadActual) / (auto.velocidadMaxima), 5);
               }
               else
               {
                   EjemploAlumno.instance.activar_efecto = false;
               }
           }
           GuiController.Instance.ThirdPersonCamera.Target = auto.mesh.Position;

            //dibuja el auto
            auto.mesh.render();
            // renderizar OBB
            if (debugMode)
                auto.obb.render();
            //dibuja el nivel
            nivel.render();

            // y dibujo todos los obstaculos de la colección obstáculos
            foreach (ObstaculoRigido obstaculo in this.obstaculos)
            {
                obstaculo.render();
                if (debugMode)
                    obstaculo.obb.render();
            }
        }
    }
}