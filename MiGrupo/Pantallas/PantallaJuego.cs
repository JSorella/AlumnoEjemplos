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
        private float velocidad_movimiento;
        private float velocidad_rotacion;
        private List<Renderizable> renderizables = new List<Renderizable>();

        public PantallaJuego(TgcMesh autito)
        {
            /*En PantallaInicio le paso a Pantalla juego con qué auto jugar. Acá lo asigno a la pantalla, cargo el coso
             que capta el teclado, creo el Nivel1 y lo pongo en la lista de renderizables, para que sepa con qué 
             escenario cargarse */
            
            this.auto = autito;
            this.entrada = GuiController.Instance.D3dInput;
            this.renderizables.Add(new Nivel1());
            this.velocidad_movimiento = 250f;
            this.velocidad_rotacion = 100f;

            // CAMARA TERCERA PERSONA
            GuiController.Instance.ThirdPersonCamera.Enable = true;
            GuiController.Instance.ThirdPersonCamera.resetValues();
            GuiController.Instance.ThirdPersonCamera.setCamera(auto.Position, 300, 700);
        }

        public void render(float elapsedTime)
        {
            //moverse y rotar son variables que me indican a qué velocidad se moverá o rotará el mesh respectivamente.
            //Se inicializan en 0, porque por defecto está quieto.
            float moverse = 0f;
            float rotar = 0f;
            
            //Procesa las entradas del teclado.
           if(entrada.keyDown(Key.S))
           {
              moverse = velocidad_movimiento;
           }
           if (entrada.keyDown(Key.W))
           {
               moverse = -velocidad_movimiento;
           }
           if (entrada.keyDown(Key.A))
           {
               rotar = -velocidad_rotacion;
           }
           if (entrada.keyDown(Key.D))
           {
               rotar = velocidad_rotacion;
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
           }

           GuiController.Instance.ThirdPersonCamera.Target = auto.Position;

            //dibuja el auto
            this.auto.render();
           //dibuja el nivel (acuerdense que "renderizable" es una lista que lo único que tiene adentro por ahora es el nivel 1.
            foreach (Renderizable renderizable in this.renderizables)
                renderizable.render();
        }
    }
}
