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
    class PantallaGameOver : Pantalla
    {
        //private TgcText2d mensaje;
        private TgcD3dInput entrada;
        private Imagen gameOver;

        public PantallaGameOver()
        {

            gameOver = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\GO.png");
            gameOver.setPosicion(new Vector2(150, 150));
            gameOver.setEscala(new Vector2(0.3f, 0.3f));

            this.entrada = GuiController.Instance.D3dInput;
           /* mensaje = new TgcText2d();
            mensaje.Text = "GAME OVER. Presione Q para volver a empezar";
            mensaje.Color = Color.DarkRed;
            mensaje.Align = TgcText2d.TextAlign.CENTER;
            mensaje.Position = new Point(200, 200);
            mensaje.Size = new Size(600, 50);
            mensaje.changeFont(new System.Drawing.Font("TimesNewRoman", 25, FontStyle.Bold));*/
        }

        public void render(float elapsedTime) 
        {
            //mensaje.render();
            gameOver.render();

            if (entrada.keyDown(Key.Q))
            {
                GuiController.Instance.ThirdPersonCamera.resetValues();
                EjemploAlumno.getInstance().setPantalla(new PantallaInicio());
            }
        }
    }
}
