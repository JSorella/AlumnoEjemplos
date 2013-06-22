using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using TgcViewer;
using TgcViewer.Utils;
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
        private TgcD3dInput entrada;
        private Auto auto;
        private Musica musica;
        private Nivel nivel;
        private TgcText2d puntos;
        private DateTime horaInicio;
        private TgcText2d tiempoRestante;
        private bool comienzoNivel;
        private int segundosAuxiliares = 1;
        private Plane caraChocada;
        private ObstaculoRigido obstaculoChocado = null;
        private TgcArrow collisionNormalArrow, debugArrow;
        private float tiempoTrans = 0f; //tiempo transcurrido desde el defasaje de rotacion de camara y rotacion del mesh
        private float pasaronSegundos = 0f;
        private bool habilitarContador = false;
        private bool habilitarDecremento = false;
        private bool ajustar = false;
        private float sentidoAnterior = 1;
        EjemploAlumno EjemploAlu = EjemploAlumno.getInstance();
        Imagen vida, barra, barra2;
        Vector2 escalaInicial = new Vector2(5.65f, 0.7f);
        Vector2 escalaVida = new Vector2(5.65f, 0.7f);
        bool modoDios = false;
        bool muerte = false;
        bool salirConQ = false;
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
            vida = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\vidaPersonaje\\vida.jpg");
            barra = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\vidaPersonaje\\fondobarra.png");
            barra2 = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\vidaPersonaje\\fondobarra2.png");

            vida.setEscala(escalaInicial);
            barra.setEscala(new Vector2(6.81f, 1f));
            barra2.setEscala(new Vector2(6.81f, 1f));
            Vector2 posicionbarra = new Vector2(10, 5);

            vida.setPosicion(new Vector2(155f, 9.3f));

            //CUENTA REGRESIVA
            uno = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\cuentaRegresiva\\1.png");
            uno.setCentrarYEscalar();
            dos = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\cuentaRegresiva\\2.png");
            dos.setCentrarYEscalar();
            tres = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\cuentaRegresiva\\3.png");
            tres.setCentrarYEscalar();

            //Instrucción de misión del juego
            misionMario = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\Mision\\m_mario.png");
            misionMario.setCentrarYEscalar();
            misionLuigi = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\Mision\\m_luigi.png");
            misionLuigi.setCentrarYEscalar();

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
            this.tiempoRestante.Text = "65";
            this.tiempoRestante.Color = Color.Green;
            this.tiempoRestante.Align = TgcText2d.TextAlign.RIGHT;
            this.tiempoRestante.Position = new Point(300, 30);
            this.tiempoRestante.Size = new Size(100, 50);
            this.tiempoRestante.changeFont(new System.Drawing.Font("TimesNewRoman", 25, FontStyle.Bold));
            this.comienzoNivel = true;

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

            //USER VARS
            GuiController.Instance.UserVars.addVar("DistMinima");
            GuiController.Instance.UserVars.addVar("Velocidad");
            GuiController.Instance.UserVars.addVar("Vida");
            GuiController.Instance.UserVars.addVar("AngCol");
            GuiController.Instance.UserVars.addVar("AngRot");
            GuiController.Instance.UserVars.addVar("NormalObstaculoX");
            GuiController.Instance.UserVars.addVar("NormalObstaculoZ");
        }

        public void incrementarTiempo(PantallaJuego pantalla, float elapsedTime, bool habilitarDecremento)
        {
            if (habilitarDecremento) pantalla.tiempoTrans -= elapsedTime * 1.1f;
            else pantalla.tiempoTrans += elapsedTime;
            if (pantalla.tiempoTrans < 0) pantalla.tiempoTrans = 0;
            if (pantalla.tiempoTrans > 1.5f) pantalla.tiempoTrans = 1.5f;
        }

        public void modificarVelocidadRotacion(Auto auto)
        {
            if (FastMath.Abs(auto.velocidadActual) > auto.velocidadActual / 3)
            {
                if (FastMath.Abs(auto.velocidadActual) > auto.velocidadActual / 2)
                {
                    if (FastMath.Abs(auto.velocidadActual) > auto.velocidadActual / 1.2f) auto.velocidadRotacion = auto.velocidadRotacionOriginal * 1.7f;
                    else auto.velocidadRotacion = auto.velocidadRotacionOriginal * 1.4f;
                }
                else auto.velocidadRotacion = auto.velocidadRotacionOriginal * 1.2f;
            }
            else
            {
                auto.velocidadRotacion = auto.velocidadRotacionOriginal;
            }
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

            habilitarDecremento = true;

            GuiController.Instance.UserVars.setValue("Velocidad", Math.Abs(auto.velocidadActual));
            GuiController.Instance.UserVars.setValue("Vida", escalaVida.X);
            GuiController.Instance.UserVars.setValue("AngCol", Geometry.RadianToDegree(anguloColision));
            GuiController.Instance.UserVars.setValue("AngRot", Geometry.RadianToDegree(anguloARotar));

            //aumento de la velocidad de rotacion al derrapar
            modificarVelocidadRotacion(auto);

            //Procesa las entradas del teclado.
            if (entrada.keyDown(Key.Q))
            {
                finDeJuego = true;
                salirConQ = true;
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
                auto.mutearSonido();
            }
            if (entrada.keyPressed(Key.R)) //boton de reset, el mesh vuelve a la posicion de inicio y restaura todos sus parametros
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

            int sentidoRotacion = 0; //sentido de rotacion del reajuste de camara
            float rotCamara = GuiController.Instance.ThirdPersonCamera.RotationY;
            float rotAuto = auto.mesh.Rotation.Y;
            float deltaRotacion = rotAuto - rotCamara;
            float dif = FastMath.Abs(Geometry.RadianToDegree(deltaRotacion));
            float rapidez = 5f; //aceleracion de reajuste de camara

            if (rotar != 0)
            {
                habilitarDecremento = false;
                habilitarContador = true;
            }
            if (dif < 40)
            {
                if (dif < 30)
                {
                    if (dif < 20) rapidez = 0.8f;
                    else rapidez = 2f;
                }
                else rapidez = 3f;
            }

            if (habilitarContador) pasaronSegundos += elapsedTime;

            if (deltaRotacion < 0) sentidoRotacion = -1;
            else sentidoRotacion = 1;

            if (deltaRotacion != 0 && pasaronSegundos > 0.5f)
            {
                ajustar = true;
                pasaronSegundos = 0f;
                habilitarContador = false;
            }


            float rotAngle = Geometry.DegreeToRadian(rotar * elapsedTime);

            if (ajustar) GuiController.Instance.ThirdPersonCamera.rotateY(sentidoRotacion * rapidez * elapsedTime);

            if (deltaRotacion < 0) sentidoRotacion = -1;
            else sentidoRotacion = 1;
            incrementarTiempo(this, elapsedTime, habilitarDecremento);
            auto.mesh.rotateY(rotAngle);
            auto.obb.rotate(new Vector3(0, rotAngle, 0));
            if (FastMath.Abs(Geometry.RadianToDegree(deltaRotacion)) % 360 < 3)
            {
                GuiController.Instance.ThirdPersonCamera.RotationY = rotAuto;
                ajustar = false;
            }


            if (habilitarDecremento) incrementarTiempo(this, elapsedTime, habilitarDecremento);

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
                            auto.reproducirSonidoChoque(FastMath.Abs(auto.velocidadActual));
                            auto.deformarMesh(obstaculo.obb, FastMath.Abs(auto.velocidadActual));
                        }
                        if (FastMath.Abs(auto.velocidadActual) > 800 && !modoDios)
                        {

                            escalaVida.X -= 0.00003f * Math.Abs(auto.velocidadActual) * escalaInicial.X;
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
                        cornersAuto = CalculosVectores.computeCorners(auto);
                        cornersObstaculo = CalculosVectores.computeCorners(obstaculoChocado);
                        List<Plane> carasDelObstaculo = CalculosVectores.generarCaras(cornersObstaculo);
                        Vector3 NormalAuto = direccion;
                        caraChocada = CalculosVectores.detectarCaraChocada(carasDelObstaculo, auto.puntoChoque);
                        Vector3 NormalObstaculo = new Vector3(caraChocada.A, caraChocada.B, caraChocada.C);
                        GuiController.Instance.UserVars.setValue("NormalObstaculoX", NormalObstaculo.X);
                        GuiController.Instance.UserVars.setValue("NormalObstaculoZ", NormalObstaculo.Z);

                        float desplazamientoInfinitesimal = 5f;
                        float constanteDesvio = 1.3f;
                        //Calculo el angulo entre ambos vectores
                        anguloColision = CalculosVectores.calcularAnguloEntreVectoresNormalizados(NormalAuto, NormalObstaculo);//Angulo entre ambos vectores
                        //rota mesh
                        if (FastMath.Abs(auto.velocidadActual) > 800)
                        {
                            if (Geometry.RadianToDegree(anguloColision) < 25) //dado un cierto umbral, el coche rebota sin cambiar su direccion
                            {
                                auto.velocidadActual = -auto.velocidadActual;
                            }
                            else //el coche choca y cambia su direccion
                            {

                                if (NormalObstaculo.Z > 0 && direccion.X > 0 && direccion.Z > 0)
                                {
                                    anguloARotar = constanteDesvio * (Geometry.DegreeToRadian(90) - anguloColision);
                                    auto.mesh.move(new Vector3(0, 0, -10));
                                    colorDeColision = Color.Red;
                                }

                                if (NormalObstaculo.X > 0 && direccion.X > 0 && direccion.Z > 0)
                                {
                                    anguloARotar = -constanteDesvio * (Geometry.DegreeToRadian(90) - anguloColision);
                                    auto.mesh.move(new Vector3(-5, 0, 0));
                                    colorDeColision = Color.Salmon;
                                }

                                if (NormalObstaculo.X > 0 && direccion.X > 0 && direccion.Z < 0)
                                {

                                    anguloARotar = constanteDesvio * (Geometry.DegreeToRadian(90) - anguloColision);
                                    colorDeColision = Color.Blue;
                                    auto.mesh.move(new Vector3(-desplazamientoInfinitesimal, 0, 0));
                                }

                                if (NormalObstaculo.Z < 0 && direccion.X > 0 && direccion.Z < 0)
                                {
                                    anguloARotar = -constanteDesvio * (Geometry.DegreeToRadian(90) - anguloColision);
                                    auto.mesh.move(new Vector3(0, 0, desplazamientoInfinitesimal));
                                    colorDeColision = Color.Green;
                                }

                                if (NormalObstaculo.Z < 0 && direccion.X < 0 && direccion.Z < 0)
                                {
                                    anguloARotar = constanteDesvio * (Geometry.DegreeToRadian(90) - anguloColision);
                                    auto.mesh.move(new Vector3(0, 0, desplazamientoInfinitesimal));
                                    colorDeColision = Color.Pink;
                                }


                                if (NormalObstaculo.X < 0 && direccion.X < 0 && direccion.Z < 0)
                                {
                                    anguloARotar = -constanteDesvio * (Geometry.DegreeToRadian(90) - anguloColision);
                                    auto.mesh.move(new Vector3(desplazamientoInfinitesimal, 0, 0));
                                    colorDeColision = Color.Silver;
                                }

                                if (NormalObstaculo.X < 0 && direccion.X < 0 && direccion.Z > 0)
                                {
                                    anguloARotar = constanteDesvio * (Geometry.DegreeToRadian(90) - anguloColision);
                                    auto.mesh.move(new Vector3(desplazamientoInfinitesimal, 0, 0));
                                    colorDeColision = Color.Aquamarine;
                                }

                                if (NormalObstaculo.Z > 0 && direccion.X < 0 && direccion.Z > 0)
                                {
                                    anguloARotar = -constanteDesvio * (Geometry.DegreeToRadian(90) - anguloColision);
                                    auto.mesh.move(new Vector3(0, 0, -desplazamientoInfinitesimal));
                                    colorDeColision = Color.Yellow;
                                }

                                auto.mesh.rotateY(anguloARotar);

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
                        float puntos = Convert.ToSingle(this.puntos.Text) + 100f;
                        this.puntos.Text = Convert.ToString(puntos);
                        break;
                    }
                }
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

            float tope = 1f;
            float constanteDerrape = ((tiempoTrans / 2) < tope) ? (tiempoTrans / 2) : tope;
            float proporcion = FastMath.Abs(auto.velocidadActual / auto.velocidadMaxima);
            if (sentidoAnterior != sentidoRotacion && tiempoTrans != 0) incrementarTiempo(this, elapsedTime * 5, true);
            if (tiempoTrans == 0) sentidoAnterior = sentidoRotacion;

            auto.mesh.rotateY(constanteDerrape * sentidoAnterior * proporcion);

            auto.render();

            auto.obb = TgcObb.computeFromAABB(auto.mesh.BoundingBox);
            auto.obb.setRotation(auto.mesh.Rotation);
            auto.obb.setRenderColor(colorDeColision);

            auto.mesh.rotateY(-constanteDerrape * sentidoAnterior * proporcion);

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
                this.tiempoRestante.Text = (Convert.ToDouble(tiempoRestante.Text) - 1).ToString();
                if (this.tiempoRestante.Text == "0") //Si se acaba el tiempo, me muestra el game over y reseetea todo
                {
                    finDeJuego = true;
                    muerte = true;
                }
                segundosAuxiliares++;
            }
            this.tiempoRestante.render();

            //Si se le acabo el tiempo o la vida, o apretó "Q"
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
                else if (salirConQ)
                {
                    EjemploAlumno.getInstance().setPantalla(new PantallaInicio());
                }
                else
                {
                    EjemploAlu.setPantalla(EjemploAlu.getPantalla(2));
                }
            }

            if (comienzoNivel)
            {
                if (DateTime.Now.Subtract(this.horaInicio).TotalSeconds < 3)
                {
                    if (auto.nombre == "Luigi")
                    {
                        misionLuigi.render();
                    }
                    else
                    {
                        misionMario.render();
                    }
                }
                else
                {
                    comienzoNivel = false;
                }
            }
            else
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
