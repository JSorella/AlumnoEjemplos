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
using AlumnoEjemplos.MiGrupo.Niveles;
using AlumnoEjemplos.MiGrupo;
using Microsoft.DirectX.DirectInput;

namespace AlumnoEjemplos.MiGrupo.Pantallas
{
    class PantallaJuego : Pantalla
    {
        private TgcD3dInput entrada;
        private TgcMesh auto;
        private List<Renderizable> renderizables = new List<Renderizable>();

        public PantallaJuego(TgcMesh autito)
        {
            /*En PantallaInicio le paso a Pantalla juego con qué auto jugar. Acá lo asigno a la pantalla, cargo el coso
             que capta el teclado, creo el Nivel1 y lo pongo en la lista de renderizables, para que sepa con qué 
             escenario cargarse */
            
            this.auto = autito;
            this.entrada = GuiController.Instance.D3dInput;
            this.renderizables.Add(new Nivel1());

          //CAMARA NO POSTA. ESTA NO ES LA CAMARA DEFINITIVA, VAMOS A USAR LA QUE ESTA ABAJO, PERO TENGO QUE PREGUNTAR ALGO ANTES.
           GuiController.Instance.FpsCamera.Enable = true;
           GuiController.Instance.FpsCamera.AccelerationEnable = true;
        

            // CAMARA POSTA (NO ESTA LISTA)
            //Vector3 posicion = (this.auto.Meshes[0].getPosition(new Vector3));
            //GuiController.Instance.ThirdPersonCamera.Enable = true;
            //GuiController.Instance.ThirdPersonCamera.resetValues();
            //GuiController.Instance.ThirdPersonCamera.setCamera(auto.Translation.X toDirectXVector(), 300, 700);
        }

        public void render(float elapsedTime)
        {
            //Procesa las entradas del teclado.
           if(entrada.keyDown(Key.DownArrow))
           {
              auto.move(new Vector3(0,0,40) * elapsedTime);
           }
           if (entrada.keyDown(Key.UpArrow))
           {
              auto.move(new Vector3(0, 0, -40) * elapsedTime);
           }
           if (entrada.keyDown(Key.LeftArrow))
           {
              auto.move(new Vector3(40, 0, 0) * elapsedTime);
           }
           if (entrada.keyDown(Key.RightArrow))
           {
              auto.move(new Vector3(-40, 0, 0) * elapsedTime);
           }

           //dibuja el auto
            this.auto.render();
           //dibuja el nivel (acuerdense que "renderizable" es una lista que lo único que tiene adentro por ahora es el nivel 1.
            foreach (Renderizable renderizable in this.renderizables)
                renderizable.render();
        }
    }
}
