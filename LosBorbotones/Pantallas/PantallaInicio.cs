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
            //Se cargan las imágenes que necesita la pantalla, el coso que carga los meshes y el que uso para captar el teclado.
            mario = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\mario.jpg");
            mario.setPosicion(new Vector2(100, 170));
            mario.setEscala(new Vector2(0.5f, 0.5f));

            luigi = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\luigi.jpg");
            luigi.setPosicion(new Vector2(500, 170));//500,180
            luigi.setEscala(new Vector2(0.5f, 0.5f));

            recuadro = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\recuadro.png");
            recuadro.setPosicion(new Vector2(87,165));
            recuadro.setEscala(new Vector2(0.55f, 0.55f));

            //MARIO KART
            marioKart = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\D.png");
            marioKart.setPosicion(new Vector2(149, 30));
            marioKart.setEscala(new Vector2(0.3f, 0.3f));

            //"PRESIONE LA J PARA EMPEZAR A JUGAR"
            iniciar = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\P.png");
            iniciar.setPosicion(new Vector2(150, 80));
            iniciar.setEscala(new Vector2(0.3f, 0.3f));



            /*mensaje = new TgcText2d();
            mensaje.Text = "Elija con las flechitas el personaje para la simulación. Presione la J para comenzar a jugar";
            mensaje.Color = Color.DarkRed;
            mensaje.Align = TgcText2d.TextAlign.CENTER;
            mensaje.Position = new Point(200, 50);
            mensaje.Size = new Size(600, 50);
            mensaje.changeFont(new System.Drawing.Font("TimesNewRoman", 25, FontStyle.Bold));
            //"TimesNewRoman"
              */

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

        public void render(float elapsedTime)
        {
            //Si toco la flecha derecha, el recuadro apunta a Luigi
            if (entrada.keyDown(Key.RightArrow)) 
            {
                this.recuadro.setPosicion(new Vector2(487, 165));
            };
            //Si toco la flecha izquierda, el recuadro apunta a Mario
            if (entrada.keyDown(Key.LeftArrow))
            {
                this.recuadro.setPosicion(new Vector2(87, 165));
            };


            //Si apreto la J y estoy marcando a Mario 
            if (entrada.keyDown(Key.J) && (this.recuadro.getPosition() == new Vector2(87, 165)))
            {
                Auto autoElegido = EjemploAlumno.getInstance().getAutos(0);  //Me traigo el auto de Mario de la clase global
                comenzar(autoElegido);
               
             }

          


            //Si apreto la J y estoy marcando a Luigi 
            if (entrada.keyDown(Key.J) && (this.recuadro.getPosition() == new Vector2(487, 165)))
            {
                 Auto autoElegido = EjemploAlumno.getInstance().getAutos(1); //Me traigo el auto de Luigi de la clase global
                 comenzar(autoElegido);
                
               
            };
           
         
           // mensaje.render();
        
                iniciar.render();
                marioKart.render();
                recuadro.render();
                mario.render();
                luigi.render();
            
        }
    }
}
