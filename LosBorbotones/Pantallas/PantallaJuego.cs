using System;
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
        private List<ObstaculoRigido> obstaculos = new List<ObstaculoRigido>(); //Coleccion de objetos para colisionar
        private List<Recursos> recursos = new List<Recursos>(); //Coleccion de objetos para agarrar
        private List<Recursos> checkpoints = new List<Recursos>(); //Coleccion de objetos para agarrar
        public CalculosVectores calculadora = new CalculosVectores();
        private float auxRotation = 0f;
        private TgcText2d puntos;
        private DateTime horaInicio;
        private TgcText2d tiempoRestante;
        private int segundosAuxiliares = 1;
        private TgcText2d checkpointsRestantes;
        private Recursos checkpointActual;
        string texturesPath = GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\";
        EjemploAlumno EjemploAlu = EjemploAlumno.getInstance();
        List<Vector3> PosicionesCheckpoints = new List<Vector3>();
        Imagen vida, barra;
        Vector2 escalaInicial = new Vector2(6.72f, 0.7f); 
        Vector2 escalaVida = new Vector2(6.72f, 0.7f);


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
           
            //Barrita de vida
            vida = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\vida.jpg");
            barra = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\fondobarra.jpg");
       
            vida.setEscala(escalaInicial);
            barra.setEscala(new Vector2(6.81f, 1f));
            Vector2 posicionbarra = (Vector2)GuiController.Instance.Modifiers["PosicionBarra"];
            vida.setPosicion(new Vector2(posicionbarra.X+5, posicionbarra.Y+5));
            barra.setPosicion(new Vector2(posicionbarra.X, posicionbarra.Y));


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
            GuiController.Instance.Logger.log(" [WASD] Controles Vehículo "
                + Environment.NewLine + " [M] Música On/Off"
                + Environment.NewLine + " [R] Reset posición"
                + Environment.NewLine + " [B] Modo Debug (muestra OBBs y otros datos útiles)"
                + Environment.NewLine + " [Q] Volver al menú principal");

            //CARGAR OBSTÁCULOS
            obstaculos.Add(new ObstaculoRigido(50, 0, 800, 80, 300, 80, GuiController.Instance.ExamplesMediaDir + "Texturas\\baldosaFacultad.jpg"));
            obstaculos.Add(new ObstaculoRigido(100, 0, -600, 80, 300, 80, GuiController.Instance.ExamplesMediaDir + "Texturas\\granito.jpg"));
            obstaculos.Add(new ObstaculoRigido(400, 0, 1000, 80, 300, 80, GuiController.Instance.ExamplesMediaDir + "Texturas\\madera.jpg"));
            obstaculos.Add(new ObstaculoRigido(3000, 0, 1500, 1200, 300, 80, GuiController.Instance.ExamplesMediaDir + "Texturas\\madera.jpg"));
            obstaculos.Add(new ObstaculoRigido(3000, 0, 1500, 300, 1200, 80, GuiController.Instance.ExamplesMediaDir + "Texturas\\madera.jpg"));




            obstaculos.Add(new ObstaculoRigido(7505, 0, 0, 0, 10000, 10000, texturesPath + "transparente.png"));
            obstaculos.Add(new ObstaculoRigido(-7505, 0, 0, 0, 10000, 10000, texturesPath + "transparente.png"));
            obstaculos.Add(new ObstaculoRigido(0, 0, 5005, 15000, 100000, 0, texturesPath + "transparente.png"));
            obstaculos.Add(new ObstaculoRigido(0, 0, -5005, 15000, 100000, 0, texturesPath + "transparente.png"));
            Shared.debugMode = false;

            //Recursos

            //Carga los recursos
            TgcMesh hongoRojoMesh = EjemploAlumno.getInstance().getHongoRojo();
            TgcMesh hongoVerdeMesh = EjemploAlumno.getInstance().getHongoVerde();
            // Recursos hongoVerde = new Recursos(800, 1350, 0, hongoVerdeMesh);
            Recursos hongoRojo = new Recursos(1200, -50, 0, hongoRojoMesh);
            //Recursos hongoVerde2 = new Recursos(1800, 510, 0, hongoVerdeMesh);
            //Recursos hongoRojo2 = new Recursos(-1200, 10, 0, hongoRojoMesh);
            //this.recursos.Add(hongoVerde);
            this.recursos.Add(hongoRojo);
            //this.recursos.Add(hongoVerde2);
            // this.recursos.Add(hongoRojo2);

            //Checkpoints
            PosicionesCheckpoints.Add(new Vector3(2000, 80, 100));
            PosicionesCheckpoints.Add(new Vector3(6000, 80, 100));
            PosicionesCheckpoints.Add(new Vector3(-2000, 580, 100));
            PosicionesCheckpoints.Add(new Vector3(-2000, -80, 100));
            PosicionesCheckpoints.Add(new Vector3(1500, -80, 100));
            PosicionesCheckpoints.Add(new Vector3(2000, -580, 100));
            PosicionesCheckpoints.Add(new Vector3(0, 0, 100));


            this.agregarCheckpoints();

            checkpointActual = checkpoints.ElementAt(0);
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
            this.tiempoRestante.Text = "60";
            this.tiempoRestante.Color = Color.Green;
            this.tiempoRestante.Align = TgcText2d.TextAlign.RIGHT;
            this.tiempoRestante.Position = new Point(300, 30);
            this.tiempoRestante.Size = new Size(100, 50);
            this.tiempoRestante.changeFont(new System.Drawing.Font("TimesNewRoman", 25, FontStyle.Bold));



            GuiController.Instance.UserVars.addVar("DistMinima");
            GuiController.Instance.UserVars.addVar("Velocidad");
            GuiController.Instance.UserVars.addVar("Vida"); 
        }

        public void ajustarCamaraSegunColision(Auto auto, List<ObstaculoRigido> obstaculos)
        {
            TgcThirdPersonCamera camera = GuiController.Instance.ThirdPersonCamera;
            Vector3 segmentA;
            Vector3 segmentB;
            camera.generateViewMatrix(out segmentA, out segmentB);

            //Detectar colisiones entre el segmento de recta camara-personaje y todos los objetos del escenario
            Vector3 q;
            float minDistSq = FastMath.Pow2(camera.OffsetForward);

            foreach (ObstaculoRigido obstaculo in obstaculos)
            {
                //Hay colision del segmento camara-personaje y el objeto
                if (TgcCollisionUtils.intersectSegmentAABB(segmentB, segmentA, obstaculo.box.BoundingBox, out q))
                {
                    //Si hay colision, guardar la que tenga menor distancia
                    float distSq = (Vector3.Subtract(q, segmentB)).LengthSq();
                    if (distSq < minDistSq)
                    {
                        minDistSq = distSq;

                        //Le restamos un poco para que no se acerque tanto
                        minDistSq /= 2;
                    }
                }
            }

            //Acercar la camara hasta la minima distancia de colision encontrada (pero ponemos un umbral maximo de cercania)
            float newOffsetForward = FastMath.Sqrt(minDistSq);
            /*
if(newOffsetForward < 10)
{
newOffsetForward = 10;
}*/
            camera.OffsetForward = newOffsetForward;
        }


        public void agregarCheckpoints()
        {
            foreach (Vector3 Posicion in PosicionesCheckpoints)
            {
                this.checkpoints.Add(new Recursos(Posicion.X, Posicion.Y, Posicion.Z, texturesPath + "cuadritos.jpg", 1));
            }
        }



        public void render(float elapsedTime)
        {
            //moverse y rotar son variables que me indican a qué velocidad se moverá o rotará el mesh respectivamente.
            //Se inicializan en 0, porque por defecto está quieto.

            float moverse = 0f;
            float rotar = 0f;
            GuiController.Instance.UserVars.setValue("Velocidad", FastMath.Abs(auto.velocidadActual));
            GuiController.Instance.UserVars.setValue("Vida", escalaVida.X);

            //Procesa las entradas del teclado.
            if (entrada.keyDown(Key.Q))
            {
                GuiController.Instance.ThirdPersonCamera.resetValues();
                //corta la música al salir
                TgcMp3Player player = GuiController.Instance.Mp3Player;
                player.closeFile();
                auto.reiniciar();
                EjemploAlumno.getInstance().setPantalla(EjemploAlumno.getInstance().getPantalla(0));
            }

            if (entrada.keyDown(Key.S))
            {
                moverse = auto.irParaAtras(elapsedTime);
            }
            if (entrada.keyDown(Key.W))
            {
                moverse = auto.irParaAdelante(elapsedTime);
            }
            if (entrada.keyDown(Key.A) && (auto.velocidadActual > 0.5f || auto.velocidadActual < -0.5f)) //izquierda
            {
                rotar = -auto.velocidadRotacion;
            }
            if (entrada.keyDown(Key.D) && (auto.velocidadActual > 0.5f || auto.velocidadActual < -0.5f)) //derecha
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
                checkpoints.RemoveRange(0, checkpoints.Count());
                this.agregarCheckpoints();
                checkpointsRestantes.Text = checkpoints.Count().ToString();
                checkpointActual = checkpoints.ElementAt(0);
                puntos.Text = "0";
                tiempoRestante.Text = "60";
                GuiController.Instance.ThirdPersonCamera.resetValues();

            }
            if (entrada.keyPressed(Key.B)) //Modo debug para visualizar BoundingBoxes entre otras cosas que nos sirvan a nosotros
            {
                Shared.debugMode = !Shared.debugMode;
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
            if (moverse < (-auto.velocidadMaxima))
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
                Vector3[] cornersAuto;
                Vector3[] cornersObstaculo;
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
                        
                        escalaVida.X-= 0.00025f*Math.Abs(auto.velocidadActual) * escalaInicial.X;
                        if (escalaVida.X > 0.03f)
                        {
                            vida.setEscala(new Vector2(escalaVida.X, escalaVida.Y));
                        }
                        else 
                        {
                            auto.reiniciar();
                            GuiController.Instance.ThirdPersonCamera.resetValues();
                            TgcMp3Player player = GuiController.Instance.Mp3Player;
                            player.closeFile();
                            EjemploAlu.setPantalla(EjemploAlu.getPantalla(1));
                        }
                        break;
                    }
                }
                //Si hubo colision, restaurar la posicion anterior (sino sigo de largo)
                if (collide)
                {
                    auto.mesh.Position = lastPos;
                    moverse = auto.chocar(elapsedTime);

                    cornersAuto = this.calculadora.computeCorners(auto);
                    cornersObstaculo = this.calculadora.computeCorners(obstaculoChocado);
                    List<Plane> carasDelObstaculo = this.calculadora.generarCaras(cornersObstaculo);
                    Vector3 NormalAuto = auto.mesh.Rotation;
                 /* Plane caraChocada = this.calculadora.detectarCaraChocada(carasDelObstaculo);
                    Vector3 NormalObstaculo = new Vector3(caraChocada.A,caraChocada.B,caraChocada.C);
                
                     //Calculo el angulo entre ambos vectores
                    float anguloColision = this.calculadora.calcularAnguloEntreVectoresNormalizados(NormalAuto,NormalObstaculo);//Angulo entre ambos vectores
                    //Roto el mesh como para que rebote como un billar
                     auto.mesh.rotateY(Geometry.DegreeToRadian(180)) - anguloColision); */
                    
                  
                   
                }


                foreach (Recursos recurso in recursos)
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
                foreach (Recursos checkpoint in checkpoints)
                {
                    TgcCollisionUtils.BoxBoxResult result = TgcCollisionUtils.classifyBoxBox(auto.mesh.BoundingBox, checkpoint.box.BoundingBox);
                    if (result == TgcCollisionUtils.BoxBoxResult.Adentro || result == TgcCollisionUtils.BoxBoxResult.Atravesando)
                    {
                        checkpoints.Remove(checkpoint); //Saca el checkpoint de la lista para que no se renderice más
                        int restantes = (Convert.ToInt16(this.checkpointsRestantes.Text) - 1);
                        this.checkpointsRestantes.Text = restantes.ToString(); //Le resto uno a los restantes
                        this.tiempoRestante.Text = (Convert.ToSingle(this.tiempoRestante.Text) + 10f).ToString();




                        if (this.checkpointsRestantes.Text == "0")
                        {
                            auto.reiniciar();
                            GuiController.Instance.ThirdPersonCamera.resetValues();
                            EjemploAlu.setPantalla(EjemploAlu.getPantalla(2));
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
            auto.sceneAuto.renderAll();
            // renderizar OBB
            auto.obb = TgcObb.computeFromAABB(auto.mesh.BoundingBox);
            auto.obb.setRotation(auto.mesh.Rotation);
            if (Shared.debugMode)
            {
               auto.obb.render();
            }
            //dibuja el nivel
            nivel.render();

            //AJUSTE DE CAMARA SEGUN COLISION

            ajustarCamaraSegunColision(auto, obstaculos);

            // y dibujo todos los obstaculos de la colección obstáculos
            foreach (ObstaculoRigido obstaculo in this.obstaculos)
            {
                obstaculo.render(elapsedTime);
                if (Shared.debugMode)
                    obstaculo.obb.render();
            }

            //Dibujo los honguitos y giladitas que vayamos a poner
            foreach (Recursos recurso in this.recursos)
            {
                recurso.render(elapsedTime);
                if (Shared.debugMode)
                {
                    recurso.modelo.BoundingBox.render();
                }
            }

            //Dibujo el checkpoints y la cantidad restante. (Onda GTA??)
            if (checkpointsRestantes.Text != "0")
            {
                checkpoints[0].render(elapsedTime);
                if (Shared.debugMode)
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
                this.tiempoRestante.Text = (Convert.ToDouble(tiempoRestante.Text) - 1).ToString(); //Pobre expresividad, como pierde frente al rendimiento...
                if (this.tiempoRestante.Text == "0") //Si se acaba el tiempo, me muestra el game over y reseetea todo
                {
                    auto.reiniciar();
                    GuiController.Instance.ThirdPersonCamera.resetValues();
                    TgcMp3Player player = GuiController.Instance.Mp3Player;
                    player.closeFile();
                    EjemploAlu.setPantalla(EjemploAlu.getPantalla(1));
                }
                segundosAuxiliares++;
            }
            this.tiempoRestante.render();


            // chispas si hay choque
            if (Shared.mostrarChispa)
            {
                foreach (Chispa chispa in auto.chispas)
                {
                    chispa.render();
                }
            }

            //... todo lo que debería renderizar con debugMode ON
            if (Shared.debugMode)
            {
                auto.moon.render();
            }

            //Dibujo barrita
            barra.render();
            vida.render();
        }
    }
}