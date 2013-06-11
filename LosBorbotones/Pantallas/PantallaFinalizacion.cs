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
    class PantallaFinalizacion : Pantalla
    {
        private TgcText2d mensaje;
        private TgcD3dInput entrada;

        public PantallaFinalizacion(int ganadorOPerdedor)
        {
            this.entrada = GuiController.Instance.D3dInput;
            mensaje = new TgcText2d();
            mensaje.Align = TgcText2d.TextAlign.CENTER;
            mensaje.Position = new Point(200, 200);
            mensaje.Size = new Size(600, 50);
            mensaje.changeFont(new System.Drawing.Font("TimesNewRoman", 25, FontStyle.Bold));

            if (ganadorOPerdedor == 0)
            {
                mensaje.Text = "GAME OVER. Presione Q para volver a intentar";
                mensaje.Color = Color.DarkRed;
            }
            else 
            {
                mensaje.Text = "Ganaste! Presione Q para volver a jugar";
                mensaje.Color = Color.Green;
            }
        }

        public void render(float elapsedTime) 
        {
            mensaje.render();

            if (entrada.keyDown(Key.Q))
            {
                GuiController.Instance.ThirdPersonCamera.resetValues();
                EjemploAlumno.getInstance().setPantalla(new PantallaInicio());
            }
        }
    }
}
