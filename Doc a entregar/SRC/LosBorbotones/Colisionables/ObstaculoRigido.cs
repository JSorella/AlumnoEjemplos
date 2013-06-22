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

        // Constructor
        public ObstaculoRigido(float _x, float _z, float _y, float ancho, float alto, float largo, string textura)
        {
            TgcBox box = TgcBox.fromSize(
                 new Vector3(_x, _z, _y),             //posicion
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
        
        public ObstaculoRigido(string _pathMesh, Vector3 _posicion, Vector3 _escala )
        {  
            this.mesh = MeshUtils.loadMesh(_pathMesh);
            this.mesh.Position = _posicion;
            this.mesh.Scale = _escala;
            this.obb = TgcObb.computeFromAABB(this.mesh.BoundingBox);
        }

        public void nuevaPos(Vector2 pos)
        {
            this.mesh.Position = new Vector3(pos.X, this.mesh.Position.Y, pos.Y);

        }
      
        public void render(float elapsedTime)
        {
            
            mesh.render();
        }
    }
}