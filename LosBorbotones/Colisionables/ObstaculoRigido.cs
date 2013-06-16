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
        public TgcMesh mesh;
        public TgcObb obb;
        public int checkpoint;

        // Constructor
        public ObstaculoRigido(float x, float z, float y, float ancho, float alto, float largo, string textura)
        {
            TgcBox box = TgcBox.fromSize(
                 new Vector3(x, z, y),             //posicion
                 new Vector3(ancho, alto, largo),  //tamaño
                 TgcTexture.createTexture(textura));
            //Computar OBB a partir del AABB del mesh. Inicialmente genera el mismo volumen que el AABB, pero luego te permite rotarlo (cosa que el AABB no puede)
            this.obb = TgcObb.computeFromAABB(box.BoundingBox);
            this.mesh = box.toMesh("caja");
        }

        public ObstaculoRigido(TgcMesh _mesh)
        {
            this.obb = TgcObb.computeFromAABB(_mesh.BoundingBox);
            this.mesh = _mesh;
        }
      
        public void render(float elapsedTime)
        {
            
            mesh.render();
        }
    }
}