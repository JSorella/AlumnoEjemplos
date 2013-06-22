using System;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using TgcViewer;
using TgcViewer.Utils._2D;
using TgcViewer.Utils.Input;
using AlumnoEjemplos.LosBorbotones;
using TgcViewer.Utils.TgcSceneLoader;
using AlumnoEjemplos.LosBorbotones.Autos;


namespace AlumnoEjemplos.LosBorbotones.Pantallas
{
    class PantallaInicio : Pantalla
    {
        private Imagen mario;
        private Imagen luigi;
        private Imagen recuadro;
        private TgcD3dInput entrada;
        private Imagen marioKart;
        private Imagen iniciar;

        public PantallaInicio()
        {
        
         //Alto y ancho total de la pantalla
            float screenHeigth = Globales.getInstance().getAltoPantalla();
            float screenWidth = Globales.getInstance().getAnchoPantalla();
         //Coeficientes para la posicion relativa de las fotos de Mario y Luigi
            float xrelatMario = 0.1112f;
            float yrelatAmbos = 0.38f;
            float xrelatLuigi = 0.5631f;
        //Coeficientes absolutos para el escalado de Mario y Luigi
            float xrelat = 0.266f;
            float yrelat = 0.5f;
        //Coeficientes que regulan el tamaño especifico de cada imagen. 
            float relatMarioX = xrelat * screenWidth / 480; //Los denominadores son los tamaños de alto y ancho de la imagen.
            float relatMarioY = yrelat * screenHeigth / 480;
            float relatRecux = 0.2904f * screenWidth / 500;
            float relatRecuy = 0.5510f * screenHeigth / 500;
            float relatMKx = 0.18f * screenWidth / 500;
            float relatMKy = 0.25f * screenHeigth / 500;
            float relatInicx = 0.2f * screenWidth / 500;
            float relatInicy = 0.5f * screenHeigth / 500;

            //Se cargan las imágenes que necesita la pantalla, el coso que carga los meshes y el que uso para captar el teclado.
            mario = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\personajes\\mario.jpg");
            mario.setPosicion(new Vector2(xrelatMario * screenWidth, yrelatAmbos * screenHeigth));
            mario.setEscala(new Vector2(relatMarioX, relatMarioY));

            luigi = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\personajes\\luigi.jpg");
            luigi.setPosicion(new Vector2(xrelatLuigi * screenWidth, yrelatAmbos * screenHeigth));//500,180
            luigi.setEscala(new Vector2(relatMarioX, relatMarioY));

            recuadro = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\otros\\recuadro.png");
            recuadro.setPosicion(0.95f*mario.getPosition());
            recuadro.setEscala(new Vector2(relatRecux,relatRecuy));

            //MARIO KART
            marioKart = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\frases\\D.png");
            marioKart.setPosicion(new Vector2(0.1345f*screenWidth, 0.05f*screenWidth));
            marioKart.setEscala(new Vector2(relatMKx,relatMKy));

            //"PRESIONE LA J PARA EMPEZAR A JUGAR"
            iniciar = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\frases\\P.png");
            iniciar.setPosicion(new Vector2(0.9f*marioKart.getPosition().X, 1.045f*marioKart.getPosition().Y ));
            iniciar.setEscala(new Vector2(relatInicx, relatInicy));

            //MENSAJE CONSOLA
            GuiController.Instance.Logger.log(" [WASD] Controles Vehículo "
                + Environment.NewLine + " [M] Música On/Off"
                + Environment.NewLine + " [R] Reset posición"
                + Environment.NewLine + " [B] Debug Mode (muestra OBBs y otros datos útiles)"
                + Environment.NewLine + " [I] Degreelessness Mode (modo Dios)"
                + Environment.NewLine + " [Q] Volver al menú principal");
           
            entrada = GuiController.Instance.D3dInput;

         }
     
      
        public void comenzar(Auto autoElegido)
        {
            /*Se llama al método de la clase EjemploAlumno que carga las pantalla. Si quiero empezar, 
             elijo una pantalla de juego y le digo con qué autito cargarse*/

          EjemploAlumno.getInstance().setPantalla(new PantallaJuego(autoElegido));
        }
        bool esMario = true;
        public void render(float elapsedTime)
        {
          
            //Si toco la flecha derecha, el recuadro apunta a Luigi
            if (entrada.keyDown(Key.RightArrow)) 
            {
               this.recuadro.setPosicion(new Vector2(luigi.getPosition().X*0.99f, 0.95f*luigi.getPosition().Y));
                esMario = false;
            };
            //Si toco la flecha izquierda, el recuadro apunta a Mario
            if (entrada.keyDown(Key.LeftArrow))
            {
                this.recuadro.setPosicion(0.95f * mario.getPosition());
                esMario = true;
            };


            //Si apreto la J y estoy marcando a Mario 
            if (entrada.keyDown(Key.J))
            {
                Auto autoElegido;
                if (esMario)
                {
                    autoElegido = EjemploAlumno.getInstance().getAutos(0);  //Me traigo el auto de Mario de la clase global
                }
                else 
                {
                    autoElegido = EjemploAlumno.getInstance().getAutos(1); //Me traigo el auto de Luigi de la clase global
                }
                comenzar(autoElegido);
               
             }

                recuadro.render();
                mario.render();
                luigi.render();
                iniciar.render();
                marioKart.render();
            
        }
    }
}
