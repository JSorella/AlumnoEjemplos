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
    public class Checkpoint 
    {
        public TgcObb obb;
        public TgcMesh modelo;
        public Checkpoint(float x, float z, float y, TgcMesh _modelo)
        {
            _modelo.Position = new Vector3(x, y, z);
            this.modelo = _modelo;
            this.modelo.Scale = new Vector3(5, 5, 5);
            this.obb = TgcObb.computeFromAABB(this.modelo.BoundingBox);
        }
        public void render(float elapsedTime) 
        {
            this.modelo.rotateY(5f * elapsedTime);
            this.modelo.render();
        }
            
    }
}