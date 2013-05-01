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
    class ObstaculoRigido 
    {
        public TgcBox box;

        // Constructor
        public ObstaculoRigido( float x, float z, float y, float ancho, float alto, float largo, string textura)
        {
            box = TgcBox.fromSize(
                 new Vector3(x, z, y),             //posicion
                 new Vector3(ancho, alto, largo),  //tamaño
                 TgcTexture.createTexture(textura));   
        }


        public void render()
        {
            box.render();
        }

       
    }
}
