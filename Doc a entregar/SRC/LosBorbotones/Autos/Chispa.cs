using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using System;
using System.Drawing;
using System.Collections.Generic;
using AlumnoEjemplos.LosBorbotones.Pantallas;
using AlumnoEjemplos.LosBorbotones.Colisionables;

namespace AlumnoEjemplos.LosBorbotones.Autos
{
    public class Chispa
    {

        public TgcMesh mesh;
        private Device d3dDevice = GuiController.Instance.D3dDevice;
        private string sphere = GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Sphere\\Sphere-TgcScene.xml";
        public Vector3 direccion;
        public float velocidad = 15f;
        public float tiempoChispas;

        public Chispa()
        {
            TgcSceneLoader loader = new TgcSceneLoader();
            mesh = loader.loadSceneFromFile(sphere).Meshes[0];
            mesh.changeDiffuseMaps(new TgcTexture[] { TgcTexture.createTexture(d3dDevice, Shared.mediaPath + "\\otros\\giallo.png") });
            mesh.Scale = new Vector3(0.08f, 0.08f, 0.08f);
        }

        public void centerChange(Vector3 nuevaPosicion)
        {
            mesh.Position = nuevaPosicion;
        }

        public void asignarDireccion(Vector3 puntoOrigen, Vector3 puntoDestino, Vector3 delta)
        {
            //this.direccion = CalculosVectores.calcularNormalPlano(puntoDestino, delta, puntoOrigen);
            this.direccion = puntoDestino + delta - puntoOrigen;
        }

        public void render()
        {
                Shared.elapsedTimeChispa++;
                
                this.mesh.render();
                if (Shared.elapsedTimeChispa > this.tiempoChispas)
                {
                    Shared.elapsedTimeChispa = 0f;
                    Shared.mostrarChispa = false; 
                }
                this.mesh.Position += velocidad * Vector3.Normalize(this.direccion);
        }

    }
}
