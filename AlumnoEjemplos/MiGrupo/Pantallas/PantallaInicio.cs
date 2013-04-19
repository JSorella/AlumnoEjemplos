using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using TgcViewer;
using TgcViewer.Utils._2D;
using TgcViewer.Utils.Input;
using AlumnoEjemplos.MiGrupo;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.MiGrupo.Pantallas
{
    class PantallaInicio : Pantalla
    {
        private TgcText2d mensaje;
        private Imagen mario;
        private Imagen luigi;
        private Imagen recuadro;
        private TgcD3dInput entrada;
        private TgcSceneLoader loader;

        public PantallaInicio()
        {
            //Se cargan las imágenes que necesita la pantalla, el coso que carga los meshes y el que uso para captar el teclado.
            mario = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "MiGrupo\\mario.jpg");
            mario.setPosicion(new Vector2(100, 180));
            mario.setEscala(new Vector2(0.5f, 0.5f));

            luigi = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "MiGrupo\\luigi.jpg");
            luigi.setPosicion(new Vector2(500, 180));
            luigi.setEscala(new Vector2(1.0f, 1.0f));

            recuadro = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "MiGrupo\\recuadro.png");
            recuadro.setPosicion(new Vector2(87,165));
            recuadro.setEscala(new Vector2(0.55f, 0.55f));
                
            mensaje = new TgcText2d();
            mensaje.Text = "Elija con las flechitas el personaje para la simulación. Presione la J para comenzar a jugar";
            mensaje.Color = Color.DarkRed;
            mensaje.Align = TgcText2d.TextAlign.CENTER;
            mensaje.Position = new Point(200, 50);
            mensaje.Size = new Size(600, 50);
            mensaje.changeFont(new System.Drawing.Font("TimesNewRoman", 25, FontStyle.Bold));

            loader = new TgcSceneLoader();
            entrada = GuiController.Instance.D3dInput;
        }


        public void comenzar(TgcMesh autoElegido)
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
               TgcMesh autoElegido = EjemploAlumno.getInstance().getAutos(0);  //Me traigo el auto de Mario de la clase global
               comenzar(autoElegido); // Digo que quiero empezar a jugar con el auto elegido de Mario, en este caso
             }

            //Si apreto la J y estoy marcando a Luigi 
            if (entrada.keyDown(Key.J) && (this.recuadro.getPosition() == new Vector2(487, 165)))
            {
                TgcMesh autoElegido = EjemploAlumno.getInstance().getAutos(1); //Me traigo el auto de Luigi de la clase global
                comenzar(autoElegido); // Digo que quiero empezar a jugar con el auto elegido, el de Luigi en este caso
            };
           
         
            mensaje.render();
            recuadro.render();
            mario.render();
            luigi.render();
        }
    }
}
