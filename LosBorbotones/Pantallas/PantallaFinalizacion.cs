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
       //private TgcText2d mensaje;
        private TgcD3dInput entrada;
        private Imagen gameOver;
        private Imagen ganaste;
        private Imagen volverAEmpezar;
        private bool bandera;

        public PantallaFinalizacion(int ganadorOPerdedor)
        {
            this.entrada = GuiController.Instance.D3dInput;
           /* mensaje = new TgcText2d();
            mensaje.Align = TgcText2d.TextAlign.CENTER;
            mensaje.Position = new Point(200, 200);
            mensaje.Size = new Size(600, 50);
            mensaje.changeFont(new System.Drawing.Font("TimesNewRoman", 25, FontStyle.Bold));*/

            if (ganadorOPerdedor == 0)
            {
                gameOver = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\GO.png");
                gameOver.setPosicion(new Vector2(150, 150));
                gameOver.setEscala(new Vector2(0.3f, 0.3f));

                bandera=false;
                
                /*mensaje.Text = "GAME OVER. Presione Q para volver a intentar";
                mensaje.Color = Color.DarkRed;*/
            }
            else 
            {
                ganaste = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\ganaste.png");
                ganaste.setPosicion(new Vector2(150, 150));
                ganaste.setEscala(new Vector2(0.3f, 0.3f));
                
                bandera=true;
                /*mensaje.Text = "Ganaste! Presione Q para volver a jugar";
                mensaje.Color = Color.Green;*/
       
             }
            volverAEmpezar = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\Q.png");
            volverAEmpezar.setPosicion(new Vector2(150, 200));
            volverAEmpezar.setEscala(new Vector2(0.3f, 0.3f));
        }

       public void render(float elapsedTime) 
        {
            if (bandera==false)
            {
                gameOver.render();
            }
            else
            {
                ganaste.render();
            }

            volverAEmpezar.render();

            if (entrada.keyDown(Key.Q))
            {
                GuiController.Instance.ThirdPersonCamera.resetValues();
                EjemploAlumno.getInstance().setPantalla(new PantallaInicio());
            }
        }
    }
}
