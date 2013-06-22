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
            float screenHeigth = Globales.getInstance().getAltoPantalla();
            float screenWidth = Globales.getInstance().getAnchoPantalla();

            float cx = 0.5f * screenWidth / 1500;
            float cy = 0.4f* screenHeigth / 500;
            float cvx = 0.65f * screenWidth / 1500;
            float cvy = 0.5f * screenHeigth / 500;

            if (ganadorOPerdedor == 0)
            {
                gameOver = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\frases\\GO.png");
                gameOver.setPosicion(new Vector2(0.15f * screenWidth, 0.2f* screenHeigth));
                gameOver.setEscala(new Vector2(cx, cy));

                bandera=false;
                
                /*mensaje.Text = "GAME OVER. Presione Q para volver a intentar";
                mensaje.Color = Color.DarkRed;*/
            }
            else 
            {
                ganaste = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\frases\\ganaste.png");
                //ganaste.setEscala(new Vector2(cx, cy));
                ganaste.setEscalarMaximo();
                ganaste.setCentrarAncho(0.2f * screenHeigth);
                
                
                bandera=true;
                /*mensaje.Text = "Ganaste! Presione Q para volver a jugar";
                mensaje.Color = Color.Green;*/
       
             }
            volverAEmpezar = new Imagen(GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\frases\\Q.png");
            
            volverAEmpezar.setPosicion(new Vector2(0.1f * screenWidth , 0.35f * screenHeigth));
            volverAEmpezar.setEscala(new Vector2(cvx, cvy));
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
                GuiController.Instance.UserVars.clearVars();
                GuiController.Instance.ThirdPersonCamera.resetValues();
                EjemploAlumno.instance.activar_efecto = false;
                EjemploAlumno.getInstance().setPantalla(new PantallaInicio());
            }
        }
    }
}
