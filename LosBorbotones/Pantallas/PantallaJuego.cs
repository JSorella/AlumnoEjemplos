﻿using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using TgcViewer;
using TgcViewer.Utils.Input;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcKeyFrameLoader;
using TgcViewer.Utils._2D;
using TgcViewer.Utils.Sound;
using AlumnoEjemplos.LosBorbotones.Niveles;
using AlumnoEjemplos.LosBorbotones;
using AlumnoEjemplos.LosBorbotones.Autos;
using AlumnoEjemplos.LosBorbotones.Colisionables;
using AlumnoEjemplos.LosBorbotones.Sonidos;


namespace AlumnoEjemplos.LosBorbotones.Pantallas
{
    class PantallaJuego : Pantalla
    {
        int a = 0;
        private TgcD3dInput entrada;
        private Auto auto;
        private Musica musica;
        private Nivel nivel;
        public CalculosVectores calculadora = new CalculosVectores();
        private TgcText2d puntos;
        private DateTime horaInicio;
        private TgcText2d tiempoRestante;
        private int segundosAuxiliares = 1;
        private Plane caraChocada;
        private ObstaculoRigido obstaculoChocado = null;
        private TgcArrow collisionNormalArrow, debugArrow;

        EjemploAlumno EjemploAlu = EjemploAlumno.getInstance();

        Imagen vida, barra, barra2;
        Vector2 escalaInicial = new Vector2(5.65f, 0.7f);
        Vector2 escalaVida = new Vector2(5.65f, 0.7f);
        bool modoDios = false;
        bool muerte = false;
        bool finDeJuego = false;



        Imagen uno, dos, tres;
        Imagen misionLuigi, misionMario;

        public PantallaJuego(Auto autito)
        {
            /*En PantallaInicio le paso a Pantalla juego con qué auto jugar. Acá lo asigno a la pantalla, cargo el coso
que capta el teclado, creo el Nivel1 y lo pongo en la lista de renderizables, para que sepa con qué
escenario cargarse */

            this.auto = autito;
            auto.mesh.move(new Vector3(0, 0, -3100));
            auto.mesh.rotateY(-1.57f);

            this.entrada = GuiController.Instance.D3dInput;
            this.nivel = EjemploAlumno.getInstance().getNiveles(0);

            //Barrita de vida
            vida = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\vida.jpg");
            barra = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\fondobarra.png");
            barra2 = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\fondobarra2.png");

            vida.setEscala(escalaInicial);
            barra.setEscala(new Vector2(6.81f, 1f));
            barra2.setEscala(new Vector2(6.81f, 1f));
            Vector2 posicionbarra = (Vector2)GuiController.Instance.Modifiers["PosicionBarra"];

            vida.setPosicion(new Vector2(155f, 9.3f));
            //vida.setPosicion(new Vector2(posicionbarra.X-1, posicionbarra.Y+5));
            barra.setPosicion(new Vector2(posicionbarra.X, posicionbarra.Y));
            barra2.setPosicion(new Vector2(posicionbarra.X, posicionbarra.Y));

            //CUENTA REGRESIVA
            uno = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\1.png");
            uno.setPosicion(new Vector2(200f, 0f));
            dos = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\2.png");
            dos.setPosicion(new Vector2(200f, 0f));
            tres = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\3.png");
            tres.setPosicion(new Vector2(200f, 0f));

            misionMario = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\m_mario.jpg");
            misionMario.setPosicion(new Vector2(200f, 0f));
            misionLuigi = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\m_luigi.jpg");
            misionLuigi.setPosicion(new Vector2(200f, 0f));


            //misione

            // CAMARA TERCERA PERSONA
            GuiController.Instance.ThirdPersonCamera.Enable = true;
            GuiController.Instance.ThirdPersonCamera.resetValues();
            Vector2 vectorCam = (Vector2)GuiController.Instance.Modifiers["AlturaCamara"];
            GuiController.Instance.ThirdPersonCamera.setCamera(auto.mesh.Position, vectorCam.X, vectorCam.Y);
            GuiController.Instance.ThirdPersonCamera.rotateY(auto.mesh.Rotation.Y);

            //CARGAR MÚSICA.
            Musica track = new Musica("ramones.mp3");
            this.musica = track;
            musica.playMusica();
            musica.setVolume(35);

            Shared.debugMode = false;

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

            //FLECHA NORMAL colision
            collisionNormalArrow = new TgcArrow();
            collisionNormalArrow.BodyColor = Color.Blue;
            collisionNormalArrow.HeadColor = Color.Yellow;
            collisionNormalArrow.Thickness = 1.4f;
            collisionNormalArrow.HeadSize = new Vector2(10, 20);

            //FLECHA debug (la usamos para conocer posiciones donde querramos posicionar meshes)
            debugArrow = new TgcArrow();
            debugArrow.BodyColor = Color.Purple;
            debugArrow.HeadColor = Color.Yellow;
            debugArrow.Thickness = 3f;
            debugArrow.HeadSize = new Vector2(10, 20);
            debugArrow.PStart = new Vector3(0, 400f, 0);
            debugArrow.PEnd = new Vector3(0, 10f, 0);
            debugArrow.updateValues();

            //MODIFIERS
            GuiController.Instance.UserVars.addVar("DistMinima");
            GuiController.Instance.UserVars.addVar("Velocidad");
            GuiController.Instance.UserVars.addVar("Vida");
            GuiController.Instance.UserVars.addVar("AngCol");
            GuiController.Instance.UserVars.addVar("AngRot");
            GuiController.Instance.UserVars.addVar("NormalObstaculoX");
            GuiController.Instance.UserVars.addVar("NormalObstaculoZ");
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
                if (TgcCollisionUtils.intersectSegmentAABB(segmentB, segmentA, obstaculo.mesh.BoundingBox, out q))
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

            camera.OffsetForward = newOffsetForward;
        }

        public void reiniciar()
        {
            puntos.Text = "0";
            tiempoRestante.Text = "60";
        }

        float anguloColision = 0f;
        float anguloARotar = 0f;
        Color colorDeColision = Color.Yellow;

        public void render(float elapsedTime)
        {
            //moverse y rotar son variables que me indican a qué velocidad se moverá o rotará el mesh respectivamente.
            //Se inicializan en 0, porque por defecto está quieto.

            float moverse = 0f;
            float rotar = 0f;

            GuiController.Instance.UserVars.setValue("Velocidad", Math.Abs(auto.velocidadActual));
            GuiController.Instance.UserVars.setValue("Vida", escalaVida.X);
            GuiController.Instance.UserVars.setValue("AngCol", Geometry.RadianToDegree(anguloColision));
            GuiController.Instance.UserVars.setValue("AngRot", Geometry.RadianToDegree(anguloARotar));

            //Procesa las entradas del teclado.
            if (entrada.keyDown(Key.Q))
            {
                finDeJuego = true;
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
                auto.mesh.move(new Vector3(0, 0, -3100));
                auto.mesh.rotateY(-1.57f);
                EjemploAlumno.instance.activar_efecto = false;
                nivel.reiniciar();
                this.reiniciar();
                GuiController.Instance.ThirdPersonCamera.resetValues();
                GuiController.Instance.ThirdPersonCamera.rotateY(-1.57f);

            }
            if (entrada.keyPressed(Key.B)) //Modo debug para visualizar BoundingBoxes entre otras cosas que nos sirvan a nosotros
            {
                Shared.debugMode = !Shared.debugMode;
            }
            if (entrada.keyPressed(Key.I))
            {
                modoDios = !modoDios;
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
                    float rotacionReducida = rotAngle * 0.5f;
                    auto.mesh.rotateY(rotAngle);
                    auto.obb.rotate(new Vector3(0, rotAngle, 0));
                    if (dif < Geometry.DegreeToRadian(20) /*angulo de incidencia de la camara respecto del versor del mesh*/) GuiController.Instance.ThirdPersonCamera.rotateY(rotacionReducida);
                    else GuiController.Instance.ThirdPersonCamera.rotateY(rotAngle);

                }
                else //rotacion normal
                {
                    auto.mesh.rotateY(rotAngle);
                    auto.obb.rotate(new Vector3(0, rotAngle, 0));
                    GuiController.Instance.ThirdPersonCamera.rotateY(rotAngle);
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
                if (deltaRotacion < 0 || ((3.15f < deltaRotacion) && (deltaRotacion < 6.5f))) sentidoRotacion = -1;
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

            if (moverse != 0 || auto.velocidadActual != 0) //Si hubo movimiento
            {
                Vector3 lastPos = auto.mesh.Position;
                auto.mesh.moveOrientedY(moverse * elapsedTime);
                Vector3 position = auto.mesh.Position;
                Vector3 posDiff = position - lastPos;
                auto.obb.move(posDiff);
                Vector3 direccion = new Vector3(FastMath.Sin(auto.mesh.Rotation.Y) * moverse, 0, FastMath.Cos(auto.mesh.Rotation.Y) * moverse);
                auto.direccion.PEnd = auto.obb.Center + Vector3.Multiply(direccion, 50f);

                //Detectar colisiones de BoundingBox utilizando herramienta TgcCollisionUtils
                bool collide = false;
                //        ObstaculoRigido obstaculoChocado = null;
                Vector3[] cornersAuto;
                Vector3[] cornersObstaculo;
                foreach (ObstaculoRigido obstaculo in nivel.obstaculos)
                {
                    if (Colisiones.testObbObb2(auto.obb, obstaculo.obb)) //chequeo obstáculo por obstáculo si está chocando con auto
                    {
                        collide = true;
                        obstaculoChocado = obstaculo;
                        Shared.mostrarChispa = true;
                        if (FastMath.Abs(auto.velocidadActual) > 800)
                        {
                            auto.deformarMesh(obstaculo.obb, FastMath.Abs(auto.velocidadActual));
                        }
                        if (FastMath.Abs(auto.velocidadActual) > 800 && !modoDios)
                        {

                            escalaVida.X -= 0.00001f * Math.Abs(auto.velocidadActual) * escalaInicial.X;
                            if (escalaVida.X > 0.03f)
                            {
                                vida.setEscala(new Vector2(escalaVida.X, escalaVida.Y));
                            }
                            else
                            {
                                finDeJuego = true;
                                muerte = true;
                            }
                        }
                        break;
                    }
                }
                //Si hubo colision, restaurar la posicion anterior (sino sigo de largo)
                if (collide)
                {
                    auto.mesh.Position = lastPos;
                    auto.obb.updateValues();
                    moverse = auto.chocar(elapsedTime);

                    if (FastMath.Abs(auto.velocidadActual) > 0)
                    {
                        cornersAuto = this.calculadora.computeCorners(auto);
                        cornersObstaculo = this.calculadora.computeCorners(obstaculoChocado);
                        List<Plane> carasDelObstaculo = this.calculadora.generarCaras(cornersObstaculo);
                        Vector3 NormalAuto = direccion;
                        caraChocada = this.calculadora.detectarCaraChocada(carasDelObstaculo, auto.puntoChoque);
                        Vector3 NormalObstaculo = new Vector3(caraChocada.A, caraChocada.B, caraChocada.C);
                        GuiController.Instance.UserVars.setValue("NormalObstaculoX", NormalObstaculo.X);
                        GuiController.Instance.UserVars.setValue("NormalObstaculoZ", NormalObstaculo.Z);

                        //Calculo el angulo entre ambos vectores
                        anguloColision = this.calculadora.calcularAnguloEntreVectoresNormalizados(NormalAuto, NormalObstaculo);//Angulo entre ambos vectores
                        //rota mesh
                        if (FastMath.Abs(auto.velocidadActual) > 800)
                        {
                            if (Geometry.RadianToDegree(anguloColision) < 25)
                            {
                                auto.velocidadActual = -auto.velocidadActual;
                            }
                            else
                            {
                                if (NormalObstaculo.Z > 0 && direccion.X > 0 && direccion.Z > 0)
                                {
                                    anguloARotar = 1.3f * (Geometry.DegreeToRadian(90) - anguloColision);
                                    auto.mesh.move(new Vector3(0, 0, -10));
                                    colorDeColision = Color.Red;
                                }

                                if (NormalObstaculo.X > 0 && direccion.X > 0 && direccion.Z > 0)
                                {
                                    anguloARotar = -1.3f * (Geometry.DegreeToRadian(90) - anguloColision);
                                    auto.mesh.move(new Vector3(-10, 0, 0));
                                    colorDeColision = Color.Salmon;
                                }

                                if (NormalObstaculo.X > 0 && direccion.X > 0 && direccion.Z < 0)
                                {

                                    anguloARotar = 1.3f * (Geometry.DegreeToRadian(90) - anguloColision);
                                    colorDeColision = Color.Blue;
                                    auto.mesh.move(new Vector3(-10, 0, 0));
                                }

                                if (NormalObstaculo.Z < 0 && direccion.X > 0 && direccion.Z < 0)
                                {
                                    anguloARotar = -1.3f * (Geometry.DegreeToRadian(90) - anguloColision);
                                    auto.mesh.move(new Vector3(0, 0, 10));
                                    colorDeColision = Color.Green;
                                }

                                if (NormalObstaculo.Z < 0 && direccion.X < 0 && direccion.Z < 0)
                                {
                                    anguloARotar = 1.3f * (Geometry.DegreeToRadian(90) - anguloColision);
                                    auto.mesh.move(new Vector3(0, 0, 10));
                                    colorDeColision = Color.Pink;
                                }


                                if (NormalObstaculo.X < 0 && direccion.X < 0 && direccion.Z < 0)
                                {
                                    anguloARotar = -1.3f * (Geometry.DegreeToRadian(90) - anguloColision);
                                    auto.mesh.move(new Vector3(10, 0, 0));
                                    colorDeColision = Color.Silver;
                                }

                                if (NormalObstaculo.X < 0 && direccion.X < 0 && direccion.Z > 0)
                                {
                                    anguloARotar = 1.3f * (Geometry.DegreeToRadian(90) - anguloColision);
                                    auto.mesh.move(new Vector3(10, 0, 0));
                                    colorDeColision = Color.Aquamarine;
                                }

                                if (NormalObstaculo.Z > 0 && direccion.X < 0 && direccion.Z > 0)
                                {
                                    anguloARotar = -1.3f * (Geometry.DegreeToRadian(90) - anguloColision);
                                    auto.mesh.move(new Vector3(0, 0, -10));
                                    colorDeColision = Color.Yellow;
                                }
                                GuiController.Instance.ThirdPersonCamera.updateCamera();
                                auto.mesh.rotateY(anguloARotar);
                                GuiController.Instance.ThirdPersonCamera.rotateY(anguloARotar);

                                GuiController.Instance.ThirdPersonCamera.updateCamera();
                            }
                        }
                        else
                        {
                            if (rotar != 0)
                            {
                                if (NormalObstaculo.X < 0)
                                {
                                    auto.mesh.move(new Vector3(1, 0, 0));
                                }
                                if (NormalObstaculo.X > 0)
                                {
                                    auto.mesh.move(new Vector3(-1, 0, 0));
                                }
                                if (NormalObstaculo.Z < 0)
                                {
                                    auto.mesh.move(new Vector3(0, 0, 1));
                                }
                                if (NormalObstaculo.Z > 0)
                                {
                                    auto.mesh.move(new Vector3(0, 0, -1));
                                }
                            }
                        }
                    }
                }

                foreach (Recursos recurso in nivel.recursos)
                {
                    TgcCollisionUtils.BoxBoxResult result = TgcCollisionUtils.classifyBoxBox(auto.mesh.BoundingBox, recurso.modelo.BoundingBox);
                    if (result == TgcCollisionUtils.BoxBoxResult.Adentro || result == TgcCollisionUtils.BoxBoxResult.Atravesando)
                    {
                        nivel.recursos.Remove(recurso); //Saca el recurso de la lista para que no se renderice más
                        float puntos = Convert.ToSingle(this.puntos.Text) + 100f;// me suma 100 puntos jejeje (?
                        this.puntos.Text = Convert.ToString(puntos);
                        break;
                    }
                }
                //foreach (Recursos checkpoint in nivel.checkpoints)
                // {
                //Chequeo si el auto agarro el checkpoint actual
                if (Colisiones.testObbObb2(auto.obb, nivel.checkpointActual.obb))
                {
                    if (nivel.checkpointsRestantes.Text != "1")
                    {
                        nivel.checkpoints.Remove(nivel.checkpointActual); //Saca el checkpoint de la lista para que no se renderice más
                        int restantes = (Convert.ToInt16(nivel.checkpointsRestantes.Text) - 1);
                        nivel.checkpointsRestantes.Text = restantes.ToString(); //Le resto uno a los restantes
                        this.tiempoRestante.Text = (Convert.ToSingle(this.tiempoRestante.Text) + 10f).ToString();
                        nivel.checkpointActual = nivel.checkpoints.ElementAt(0);
                    }
                    else
                    {
                        finDeJuego = true;
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

            //dibuja el auto y todo lo que lleve dentro
            auto.render();

            // computar OBB
            auto.obb = TgcObb.computeFromAABB(auto.mesh.BoundingBox);
            auto.obb.setRotation(auto.mesh.Rotation);
            auto.obb.setRenderColor(colorDeColision);

            //dibuja el nivel
            nivel.render(elapsedTime);

            //AJUSTE DE CAMARA SEGUN COLISION
            ajustarCamaraSegunColision(auto, nivel.obstaculos);

            //Dibujo checkpoints restantes
            nivel.checkpointsRestantes.render();

            //Dibujo el puntaje del juego
            this.puntos.render();

            //CUENTA REGRESIVA
            if (this.tiempoRestante.Text == "1")
            {
                uno.render();
            }
            if (this.tiempoRestante.Text == "2")
            {
                dos.render();
            }

            if (this.tiempoRestante.Text == "3")
            {
                tres.render();
            }

            //Actualizo y dibujo el relops
            if ((DateTime.Now.Subtract(this.horaInicio).TotalSeconds) > segundosAuxiliares && !modoDios)
            {
                this.tiempoRestante.Text = (Convert.ToDouble(tiempoRestante.Text) - 1).ToString(); //Pobre expresividad, como pierde frente al rendimiento...
                if (this.tiempoRestante.Text == "0") //Si se acaba el tiempo, me muestra el game over y reseetea todo
                {
                    finDeJuego = true;
                    muerte = true;
                }
                segundosAuxiliares++;
            }
            this.tiempoRestante.render();

            //Si se le acabo el tiempo o la vida
            if (finDeJuego)
            {
                //corta la música al salir
                TgcMp3Player player = GuiController.Instance.Mp3Player;
                player.closeFile();
                GuiController.Instance.UserVars.clearVars();
                //saca el blur
                EjemploAlumno.instance.activar_efecto = false;
                //reinicia los valores de las cosas del juego
                auto.reiniciar();
                nivel.reiniciar();
                this.reiniciar();
                //reinicia la camara
                GuiController.Instance.ThirdPersonCamera.resetValues();
                if (muerte)
                {
                    EjemploAlu.setPantalla(EjemploAlu.getPantalla(1));
                }
                else
                {
                    EjemploAlu.setPantalla(EjemploAlu.getPantalla(2));
                }
            }

            if (auto.nombre == "Luigi")
            {
                if (Convert.ToDecimal(tiempoRestante.Text)== 55)
                {
                    misionLuigi.render();
                }
                if (Convert.ToDecimal(tiempoRestante.Text) == 59)
                {
                    misionLuigi.render();
                    a = 1;
                }
                if (Convert.ToDecimal(tiempoRestante.Text) == 58)
                {
                    misionLuigi.render();
                }
                if (Convert.ToDecimal(tiempoRestante.Text) == 57)
                {
                    misionLuigi.render();
                }
                if (Convert.ToDecimal(tiempoRestante.Text) == 56)
                {
                    misionLuigi.render();
                    
                }
            }
            else
            {

                if (Convert.ToDecimal(tiempoRestante.Text) == 55)
                {
                    misionMario.render();
                    a = 1;
                }
                if (Convert.ToDecimal(tiempoRestante.Text) == 59)
                {
                    misionMario.render();
                }
                if (Convert.ToDecimal(tiempoRestante.Text) == 58)
                {
                    misionMario.render();
                }
                if (Convert.ToDecimal(tiempoRestante.Text) == 57)
                {
                    misionMario.render();
                }
                if (Convert.ToDecimal(tiempoRestante.Text) == 56)
                {
                    misionMario.render();
                  
                }
            }

            if (a != 0)
            {
                //Dibujo barrita
                if (auto.nombre == "Luigi")
                {
                    barra2.render();
                }
                else
                {
                    barra.render();
                }
                vida.render();

            }
                //renderizo utilidades del debugMode
                if (Shared.debugMode)
                {
                    Vector2 vectorModifier = (Vector2)GuiController.Instance.Modifiers["PosicionFlechaDebug"];
                    Vector3 vectorPosicion = new Vector3(vectorModifier.X, 10, vectorModifier.Y);
                    debugArrow.PStart = vectorPosicion + new Vector3(0, 400f, 0);
                    debugArrow.PEnd = vectorPosicion;
                    debugArrow.updateValues();
                    debugArrow.render();

                    //renderizo normal al plano chocado
                    if (obstaculoChocado != null)
                    {
                        collisionNormalArrow.PStart = obstaculoChocado.obb.Center;
                        collisionNormalArrow.PEnd = obstaculoChocado.obb.Center + Vector3.Multiply(new Vector3(caraChocada.A, caraChocada.B, caraChocada.C), 500f);
                        collisionNormalArrow.updateValues();
                        collisionNormalArrow.render();
                    }
                }
            }
        }
    }
