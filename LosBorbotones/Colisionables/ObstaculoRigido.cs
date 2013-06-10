using System;
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


namespace AlumnoEjemplos.LosBorbotones.Colisionables
{
   public class ObstaculoRigido 
    {
        public TgcBox box;
        public TgcObb obb;

        // Constructor
        public ObstaculoRigido( float x, float z, float y, float ancho, float alto, float largo, string textura)
        {
            this.box = TgcBox.fromSize(
                 new Vector3(x, z, y),             //posicion
                 new Vector3(ancho, alto, largo),  //tamaño
                 TgcTexture.createTexture(textura));
            //Computar OBB a partir del AABB del mesh. Inicialmente genera el mismo volumen que el AABB, pero luego te permite rotarlo (cosa que el AABB no puede)
            this.obb = TgcObb.computeFromAABB(this.box.BoundingBox);
        }

        

        public void render()
        {
            box.render();
        }

       
    }
}
