using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils._2D;
using TgcViewer.Utils.Input;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace AlumnoEjemplos.LosBorbotones.Autos
{
    class AutoMario : Auto
    {
        public AutoMario()
            : base(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\TanqueFuturistaRuedas\\TanqueFuturistaRuedas-TgcScene.xml",
                   "Mario",
                    new Vector3(0,0,0))
                    
                    {
            ;
        }

    }
}
