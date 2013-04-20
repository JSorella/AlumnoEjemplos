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



namespace AlumnoEjemplos.LosBorbotones.Pantallas
{
    class PantallaJuego : Pantalla
    {
        private TgcD3dInput entrada;
        private TgcMesh auto;
        private Musica musica;
        private float velocidad_rotacion;
        private float velocidad_maxima = 2500f;
        private float velocidad_actual = 0f;
        private List<Renderizable> renderizables = new List<Renderizable>();
        private List<TgcBox> obstaculos = new List<TgcBox>();

        public PantallaJuego(TgcMesh autito)
        {
            /*En PantallaInicio le paso a Pantalla juego con qué auto jugar. Acá lo asigno a la pantalla, cargo el coso
             que capta el teclado, creo el Nivel1 y lo pongo en la lista de renderizables, para que sepa con qué 
             escenario cargarse */
            
            this.auto = autito;
            this.entrada = GuiController.Instance.D3dInput;
            this.renderizables.Add(new Nivel1());
            this.velocidad_rotacion = 100f;

            // CAMARA TERCERA PERSONA
            GuiController.Instance.ThirdPersonCamera.Enable = true;
            GuiController.Instance.ThirdPersonCamera.resetValues();
            GuiController.Instance.ThirdPersonCamera.setCamera(auto.Position, 300, 700);

            //CARGAR MÚSICA.          
            Musica track = new Musica("ramones.mp3");
            this.musica = track;
            track.playMusica();
            track.setVolume(30);

            //CARGAR OBSTÁCULOS
            TgcBox obstaculo;
            obstaculo = TgcBox.fromSize(
                 new Vector3(-100, 0, 0),
                 new Vector3(80, 150, 80),
                 TgcTexture.createTexture(GuiController.Instance.D3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\baldosaFacultad.jpg"));
            obstaculos.Add(obstaculo);

            //Obstaculo 2
            obstaculo = TgcBox.fromSize(
                new Vector3(50, 0, 200),
                new Vector3(80, 300, 80),
                TgcTexture.createTexture(GuiController.Instance.D3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\madera.jpg"));
            obstaculos.Add(obstaculo);

            //Obstaculo 3
            obstaculo = TgcBox.fromSize(
                new Vector3(300, 0, 100),
                new Vector3(80, 100, 150),
                TgcTexture.createTexture(GuiController.Instance.D3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\granito.jpg"));
            obstaculos.Add(obstaculo);
            
        }
        //Método que calcula la velocidad con aceleracion y frenado, modelado como MRUV
        public float velocidadNueva(float velocidadAnterior, float delta_t, float aceleracion) 
        {
            //implementar velocidad maxima
                float velocidadNueva = velocidadAnterior + aceleracion * delta_t;
                return velocidadNueva;
        }

        public void render(float elapsedTime)
        {
            //moverse y rotar son variables que me indican a qué velocidad se moverá o rotará el mesh respectivamente.
            //Se inicializan en 0, porque por defecto está quieto.
            float moverse = 0f;
            float rotar = 0f;
            float aceleracion;
            
            //Procesa las entradas del teclado.

            
            if(entrada.keyDown(Key.S))
           {
               if (velocidad_actual <= 0) aceleracion = 500;
               else aceleracion = 100;
            
               moverse = velocidadNueva(velocidad_actual, elapsedTime, aceleracion);
               velocidad_actual = moverse;
           }
           if (entrada.keyDown(Key.W))
           {
               if (velocidad_actual > 0) aceleracion = -500;
               else aceleracion = -100;
               moverse = velocidadNueva(velocidad_actual, elapsedTime, aceleracion);
               velocidad_actual = moverse;
              
           }
           if (entrada.keyDown(Key.A))
           {
               rotar = -velocidad_rotacion;
           }
           if (entrada.keyDown(Key.D))
           {
               rotar = velocidad_rotacion;
           }
        
            //parte de frenado por inercia
           if (!entrada.keyDown(Key.W) && !entrada.keyDown(Key.S) && velocidad_actual != 0)
           {
               if (velocidad_actual > 0)
               {
                   moverse = velocidadNueva(velocidad_actual, elapsedTime, -300);
                   velocidad_actual = moverse;
               }
               else
               {
                   moverse = velocidadNueva(velocidad_actual, elapsedTime, 300);
                   velocidad_actual = moverse;
               };

           }

           if (rotar != 0) //Si hubo rotacion,
           {
               float rotAngle = Geometry.DegreeToRadian(rotar * elapsedTime);
               auto.rotateY(rotAngle); //roto el auto
               GuiController.Instance.ThirdPersonCamera.rotateY(rotAngle); //y la cámara
           }
           if (moverse != 0) //Si hubo movimiento
           {
               Vector3 lastPos = auto.Position;
               auto.moveOrientedY(moverse * elapsedTime); //muevo el auto
               //Detectar colisiones
               bool collide = false;
               
               foreach (TgcBox obstaculo in obstaculos)
               {
                   TgcCollisionUtils.BoxBoxResult result = TgcCollisionUtils.classifyBoxBox(auto.BoundingBox, obstaculo.BoundingBox);
                   if (result == TgcCollisionUtils.BoxBoxResult.Adentro || result == TgcCollisionUtils.BoxBoxResult.Atravesando)
                   {
                       collide = true;
                       break;
                   }
               }

               //Si hubo colision, restaurar la posicion anterior
               if (collide)
               {
                   auto.Position = lastPos;
               }
            
           }

           GuiController.Instance.ThirdPersonCamera.Target = auto.Position;

            //dibuja el auto
            this.auto.render();
           //dibuja el nivel (acuerdense que "renderizable" es una lista que lo único que tiene adentro por ahora es el nivel 1.
            foreach (Renderizable renderizable in this.renderizables)
                renderizable.render();
            foreach (TgcBox obstaculo in this.obstaculos)
                obstaculo.render();
        }

    }
}
