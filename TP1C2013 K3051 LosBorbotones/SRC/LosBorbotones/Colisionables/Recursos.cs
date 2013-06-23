using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.DirectX;
using TgcViewer.Utils.Input;
using TgcViewer;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using Microsoft.DirectX.Direct3D;

namespace AlumnoEjemplos.LosBorbotones.Colisionables
{
 public class Recursos
    {
         public TgcMesh modelo;
         public int checkpoint;
         public TgcBox box;

           public Recursos(float x, float z, float y, TgcMesh _modelo)
        {
            _modelo.Position = new Vector3(x,y,z);
            this.modelo = _modelo;
        }

         public Recursos(float x, float z, float y, string textura, int check)
        {
            this.box = TgcBox.fromSize(
                 new Vector3(x, y, z),             //posicion
                 new Vector3(50, 150, 150),  //tamaño
                 TgcTexture.createTexture(textura));
            this.box.rotateX(Geometry.DegreeToRadian(45));
            this.checkpoint = check;
        }


        public void render(float elapsedTime)
        {
            if (checkpoint != 1)
            {
                this.modelo.rotateY(5f * elapsedTime);
                this.modelo.render();
            }
            else 
            {
                this.box.render();
            }
        }

    }
}
