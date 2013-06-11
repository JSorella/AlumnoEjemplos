﻿using System;
using System.Drawing;
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
using TgcViewer.Utils._2D;
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
        private List<ObstaculoRigido> obstaculos = new List<ObstaculoRigido>();  //Coleccion de objetos para colisionar
        private List<ObstaculoRigido> recursos = new List<ObstaculoRigido>(); //Coleccion de objetos para agarrar
        private List<ObstaculoRigido> checkpoints = new List<ObstaculoRigido>(); //Coleccion de objetos para agarrar
        public static bool debugMode;
        public CalculosVectores calculadora = new CalculosVectores();
        private float auxRotation = 0f;
        private TgcText2d puntos;
        private DateTime horaInicio;
        private TgcText2d tiempoRestante;
        private int segundosAuxiliares = 1;
        private TgcText2d checkpointsRestantes;
        
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
           
           
            // CAMARA TERCERA PERSONA
            GuiController.Instance.ThirdPersonCamera.Enable = true;
            GuiController.Instance.ThirdPersonCamera.resetValues();
            Vector2 vectorCam = (Vector2)GuiController.Instance.Modifiers["AlturaCamara"];
            GuiController.Instance.ThirdPersonCamera.setCamera(auto.mesh.Position, vectorCam.X, vectorCam.Y);
            
            

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
            obstaculos.Add(new ObstaculoRigido(3000, 0, 1500, 899, 300, 80, GuiController.Instance.ExamplesMediaDir + "Texturas\\madera.jpg"));


            string texturesPath = GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\";

            obstaculos.Add(new ObstaculoRigido(7505, 0, 0, 0, 100, 10000, texturesPath + "transparente.png"));
            obstaculos.Add(new ObstaculoRigido(-7505, 0, 0, 0, 100, 10000, texturesPath + "transparente.png"));
            obstaculos.Add(new ObstaculoRigido(0, 0, 5005, 15000, 100, 0, texturesPath + "transparente.png"));
            obstaculos.Add(new ObstaculoRigido(0, 0, -5005, 15000, 100, 0, texturesPath + "transparente.png"));
            debugMode = false;

            //Recursos

            //Carga los recursos
            TgcMesh hongoRojoMesh = EjemploAlumno.getInstance().getHongoRojo();
            TgcMesh hongoVerdeMesh = EjemploAlumno.getInstance().getHongoVerde();
            ObstaculoRigido hongoVerde = new ObstaculoRigido(800, 1350, 0, hongoVerdeMesh);
            ObstaculoRigido hongoRojo = new ObstaculoRigido(1200, -50, 0, hongoRojoMesh);
            ObstaculoRigido hongoVerde2 = new ObstaculoRigido(1800, 510, 0, hongoVerdeMesh);
            ObstaculoRigido hongoRojo2 = new ObstaculoRigido(-1200, 10, 0, hongoRojoMesh);
            this.recursos.Add(hongoVerde);
            this.recursos.Add(hongoRojo);
            this.recursos.Add(hongoVerde2);
            this.recursos.Add(hongoRojo2);
   
            //Checkpoints
            checkpoints.Add(new ObstaculoRigido(2000, 0, 100, 250, 150, 400, texturesPath + "honguito.jpg"));
            checkpoints.Add(new ObstaculoRigido(6000, 0, -4100, 250, 150, 400, texturesPath + "honguito.jpg"));
            checkpoints.Add(new ObstaculoRigido(4000, 0, 4100, 250, 150, 400, texturesPath + "honguito.jpg"));

            checkpointsRestantes = new TgcText2d();
            checkpointsRestantes.Text = checkpoints.Count().ToString();
            checkpointsRestantes.Color = Color.DarkRed;
            checkpointsRestantes.Align = TgcText2d.TextAlign.RIGHT;
            checkpointsRestantes.Position = new Point(630, 30);
            checkpointsRestantes.Size = new Size(100, 50);
            checkpointsRestantes.changeFont(new System.Drawing.Font("TimesNewRoman", 25, FontStyle.Bold));

            
            //Puntos de juego
            puntos = new TgcText2d();
            puntos.Text = "0";
            puntos.Color = Color.DarkRed;
            puntos.Align = TgcText2d.TextAlign.RIGHT;
            puntos.Position = new Point(30, 30);
            puntos.Size = new Size(100, 50);
            puntos.changeFont(new System.Drawing.Font("TimesNewRoman", 25, FontStyle.Bold));

            //Reloxxxx
            this.horaInicio = DateTime.Now;
            this.tiempoRestante = new TgcText2d();
            this.tiempoRestante.Text = "30";
            this.tiempoRestante.Color = Color.Green;
            this.tiempoRestante.Align = TgcText2d.TextAlign.RIGHT;
            this.tiempoRestante.Position = new Point(300, 30);
            this.tiempoRestante.Size = new Size(100, 50);
            this.tiempoRestante.changeFont(new System.Drawing.Font("TimesNewRoman", 25, FontStyle.Bold));

            

            GuiController.Instance.UserVars.addVar("DistMinima");
            GuiController.Instance.UserVars.addVar("Velocidad");
        }


        Vector3 corrimiento = new Vector3(1, 1, 1);
          
        public void render(float elapsedTime)
        {
            //moverse y rotar son variables que me indican a qué velocidad se moverá o rotará el mesh respectivamente.
            //Se inicializan en 0, porque por defecto está quieto.

            float moverse = 0f;
            float rotar = 0f;
            GuiController.Instance.UserVars.setValue("Velocidad", FastMath.Abs(auto.velocidadActual));

            //Procesa las entradas del teclado.          
            if (entrada.keyDown(Key.Q))
            {
                GuiController.Instance.ThirdPersonCamera.resetValues();
                //corta la música al salir
                TgcMp3Player player = GuiController.Instance.Mp3Player;
                player.closeFile();
                auto.reiniciar();
                EjemploAlumno.getInstance().setPantalla( EjemploAlumno.getInstance().getPantalla(0));
            }
            
           if(entrada.keyDown(Key.S))
           {
               moverse = auto.irParaAtras(elapsedTime);
           }
           if (entrada.keyDown(Key.W))
           {
               moverse = auto.irParaAdelante(elapsedTime);
           }
           if (entrada.keyDown(Key.A) && (auto.velocidadActual>0.5f || auto.velocidadActual<-0.5f))  //izquierda
           {
               rotar = -auto.velocidadRotacion;
           }
           if (entrada.keyDown(Key.D) && (auto.velocidadActual > 0.5f || auto.velocidadActual < -0.5f))  //derecha
           {
               rotar = auto.velocidadRotacion;
           }
           if (entrada.keyPressed(Key.M))
           {
               musica.muteUnmute();
           }
           if (entrada.keyPressed(Key.R)) //boton de reset, el mesh vuelve a la posicion (0,0,0)
           {
               auto.reiniciar();
               GuiController.Instance.ThirdPersonCamera.resetValues();
               
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

            int sentidoRotacion = 0;
            if (rotar != 0) //Si hubo rotacion
            {
                float rotAngle = Geometry.DegreeToRadian(rotar * elapsedTime);
                float rotacionDelAuto = auto.mesh.Rotation.Y;
                float rotacionDeLaCamara = GuiController.Instance.ThirdPersonCamera.RotationY;
                float dif = FastMath.Abs(rotacionDelAuto - rotacionDeLaCamara);
                if (FastMath.Abs(auto.velocidadActual) > 1500) //superada cierta velocidad ya no puede rotar tanto y derrapa
                {
                    auto.mesh.moveOrientedY(rotAngle);
                    auto.mesh.rotateY(rotAngle);
                    auto.obb.rotate(new Vector3(0, rotAngle, 0));
                    auxRotation = 0.8f * rotAngle;
                    if (dif < Geometry.DegreeToRadian(25) /*angulo de incidencia de la camara respecto del versor del mesh*/) GuiController.Instance.ThirdPersonCamera.rotateY(auxRotation);
                    else GuiController.Instance.ThirdPersonCamera.rotateY(rotAngle);
                }
                else //rotacion normal
                {
                    float rotacionReducida = rotAngle * 0.5f;
                    auto.mesh.rotateY(rotacionReducida);
                    auto.obb.rotate(new Vector3(0, rotacionReducida, 0));
                    auxRotation = rotacionReducida;
                    if (dif < Geometry.DegreeToRadian(40) /*angulo de incidencia de la camara respecto del versor del mesh*/) GuiController.Instance.ThirdPersonCamera.rotateY(auxRotation);
                    else GuiController.Instance.ThirdPersonCamera.rotateY(rotacionReducida);
                }
                if (!entrada.keyDown(Key.W))// si no se acelera al coche, que se ajuste la camara
                {
                    float rotCamara = GuiController.Instance.ThirdPersonCamera.RotationY;
                    float rotAngulo = auto.mesh.Rotation.Y;
                    float aceleracionRotacion = 0.8f;
                    float deltaRotacion = rotAngulo - rotCamara;
                    if (deltaRotacion < 0) sentidoRotacion = -1;
                    else sentidoRotacion = 1;
                    if (rotAngulo != rotCamara)
                    {
                        GuiController.Instance.ThirdPersonCamera.rotateY(aceleracionRotacion * elapsedTime * sentidoRotacion);
                    }
                    if (FastMath.Abs(deltaRotacion) % Geometry.DegreeToRadian(360) < Geometry.DegreeToRadian(1))
                    {
                        GuiController.Instance.ThirdPersonCamera.RotationY = rotAngulo;
                    }
                }

            }
            else //ajuste de camara cuando no hay rotacion (cuando no se esta presionando A o D)
            {
                float rotCamara = GuiController.Instance.ThirdPersonCamera.RotationY;
                float rotAngulo = auto.mesh.Rotation.Y;
                float aceleracionRotacion = 0.8f;
                float deltaRotacion = rotAngulo - rotCamara;
                if (deltaRotacion < 0) sentidoRotacion = -1;
                else sentidoRotacion = 1;
                if (rotAngulo != rotCamara)
                {
                    GuiController.Instance.ThirdPersonCamera.rotateY(aceleracionRotacion * elapsedTime * sentidoRotacion);
                }
                if (FastMath.Abs(deltaRotacion) % Geometry.DegreeToRadian(360) < Geometry.DegreeToRadian(1))
                {
                    GuiController.Instance.ThirdPersonCamera.RotationY = rotAngulo;
                }
            }


            if (moverse != 0) //Si hubo movimiento
            {
                Vector3 lastPos = auto.mesh.Position;
                auto.mesh.moveOrientedY(moverse * elapsedTime);
                Vector3 position = auto.mesh.Position;
                Vector3 posDiff = position - lastPos;
                auto.obb.move(posDiff);



                //Detectar colisiones de BoundingBox utilizando herramienta TgcCollisionUtils
                bool collide = false;
                ObstaculoRigido obstaculoChocado = null;
               // Vector3[] cornersAuto;
                //Vector3[] cornersObstaculo;
                foreach (ObstaculoRigido obstaculo in obstaculos)
                {
                    if (Colisiones.testObbObb2(auto.obb, obstaculo.obb)) //chequeo obstáculo por obstáculo si está chocando con auto
                    {

                        collide = true;
                        obstaculoChocado = obstaculo;
                        Shared.mostrarChispa = true;
                       if (FastMath.Abs(auto.velocidadActual) > 250)
                       {
                         auto.deformarMesh(obstaculo.obb, FastMath.Abs(auto.velocidadActual));
                       }
                        break;
                    }
                }
                //Si hubo colision, restaurar la posicion anterior (sino sigo de largo)
                if (collide)
                {
                    auto.mesh.Position = lastPos;
                    moverse = auto.chocar(elapsedTime);
                 
                    /*cornersAuto = this.calculadora.computeCorners(auto);
                    cornersObstaculo = this.calculadora.computeCorners(obstaculoChocado);

                    //Calculo los vectores normales a las caras frontales (no va a ser así, pero la idea sería ver en qué cara chocó y según eso, elegir los corners correspondientes
                    Vector3 NormalAuto = this.calculadora.calcularNormalPlano(cornersAuto[5], cornersAuto[6], cornersAuto[7]); //el indice depende de la cara en la que chocan
                    Vector3 NormalObstaculo = this.calculadora.calcularNormalPlano(cornersObstaculo[5], cornersObstaculo[6], cornersObstaculo[7]);
                   //Calculo el angulo entre ambos vectores
                    float anguloColision = this.calculadora.calcularAnguloEntreVectoresNormalizados(NormalAuto,NormalObstaculo);//Angulo entre ambos vectores

                  //  auto.mesh.rotateY(anguloColision); // Hay que ver cómo influye el ángulo de choque en la rotación que va a tener el auto
                     */
                }


                foreach (ObstaculoRigido recurso in recursos)
                {
                    TgcCollisionUtils.BoxBoxResult result = TgcCollisionUtils.classifyBoxBox(auto.mesh.BoundingBox, recurso.modelo.BoundingBox);
                    if (result == TgcCollisionUtils.BoxBoxResult.Adentro || result == TgcCollisionUtils.BoxBoxResult.Atravesando)
                    {
                        recursos.Remove(recurso); //Saca el recurso de la lista para que no se renderice más
                        float puntos = Convert.ToSingle(this.puntos.Text) + 100f;// me suma 100 puntos jejeje (?
                        this.puntos.Text = Convert.ToString(puntos); 
                        break;
                    }
                }
                foreach (ObstaculoRigido checkpoint in checkpoints)
                {
                    TgcCollisionUtils.BoxBoxResult result = TgcCollisionUtils.classifyBoxBox(auto.mesh.BoundingBox, checkpoint.box.BoundingBox);
                    if (result == TgcCollisionUtils.BoxBoxResult.Adentro || result == TgcCollisionUtils.BoxBoxResult.Atravesando)
                    {
                        checkpoints.Remove(checkpoint); //Saca el checkpoint de la lista para que no se renderize más
                        this.checkpointsRestantes.Text = (Convert.ToSingle(this.checkpointsRestantes.Text) - 1).ToString(); //Le resto uno a los restantes
                        this.tiempoRestante.Text = (Convert.ToSingle(this.tiempoRestante.Text) + 10f).ToString();

                        if (this.checkpointsRestantes.Text == "0")
                        {
                            auto.reiniciar();
                            GuiController.Instance.ThirdPersonCamera.resetValues();
                            EjemploAlumno.getInstance().setPantalla( new PantallaFinalizacion(1));
                        }
                        break;
                    }
                }
               
                //Efecto blur
                if (FastMath.Abs(auto.velocidadActual) > (auto.velocidadMaxima * 0.5555))
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

            //actualizo cam
            Vector2 vectorCam = (Vector2)GuiController.Instance.Modifiers["AlturaCamara"];
            GuiController.Instance.ThirdPersonCamera.setCamera(auto.mesh.Position, vectorCam.X, vectorCam.Y);

            //dibuja el auto
            auto.mesh.render();
            // renderizar OBB

            if (debugMode)
            {
                auto.obb = TgcObb.computeFromAABB(auto.mesh.BoundingBox);
                auto.obb.setRotation(auto.mesh.Rotation);
                auto.obb.render();
            }
            //dibuja el nivel
            nivel.render();

            // y dibujo todos los obstaculos de la colección obstáculos
            foreach (ObstaculoRigido obstaculo in this.obstaculos)
            {
                obstaculo.render(elapsedTime);
                if (debugMode)
                    obstaculo.obb.render();
            }

            //Dibujo los honguitos y giladitas que vayamos a poner
            foreach (ObstaculoRigido recurso in this.recursos)
            {
                recurso.render(elapsedTime);
                if (debugMode)
                {
                    recurso.modelo.BoundingBox.render();
                }
            }
            
            //Dibujo el checkpoints y la cantidad restante. (Onda GTA??)
           if(checkpointsRestantes.Text != "0"){
                checkpoints[0].render(elapsedTime);
                if (debugMode)
                {
                    checkpoints[0].box.BoundingBox.render();
                }
            
            checkpointsRestantes.render();
           }
            //Dibujo el puntaje del juego
            this.puntos.render();

            //Actualizo y dibujo el relops
         
            if ((DateTime.Now.Subtract(this.horaInicio).TotalSeconds) > segundosAuxiliares)
            {
                this.tiempoRestante.Text = (Convert.ToDouble(tiempoRestante.Text) - 1).ToString();    //Pobre expresividad, como pierde frente al rendimiento...
                if(this.tiempoRestante.Text == "0") //Si se acaba el tiempo, me muestra el game over y reseetea todo
                {
                    auto.reiniciar();
                    GuiController.Instance.ThirdPersonCamera.resetValues();
                    TgcMp3Player player = GuiController.Instance.Mp3Player;
                    player.closeFile();
                    EjemploAlumno.getInstance().setPantalla(new PantallaFinalizacion(0));
                    
                }
                segundosAuxiliares++;
            }
            this.tiempoRestante.render();


            // chispas si hay choque
            if (Shared.mostrarChispa)
            {
                foreach(Chispa chispa in auto.chispas)
                {
                    chispa.render();
                }
            }
            
            //... todo lo que debería renderizar con debugMode ON
            if (debugMode)
            {
                auto.moon.render();
            }
        }
    }
}