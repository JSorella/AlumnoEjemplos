using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using System;

namespace AlumnoEjemplos.MiGrupo.Autos
{
    class Auto
    { 
        private TgcScene mesh;
        private string nombre;
        private Vector3 posicionInicial;
        private float elapsedTime;

        public void setElapsedTime(float _elapsedTime)
        {
            elapsedTime = _elapsedTime;
        }

        public Auto(string pathMeshAuto, string _nombre, Vector3 _posicionInicial) 
        {
            this.nombre = _nombre;
            this.posicionInicial = _posicionInicial;
            TgcScene meshAuto = loadMesh(pathMeshAuto);
            this.mesh = meshAuto;
        }

        public TgcScene loadMesh(string path)
        {
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene currentScene = loader.loadSceneFromFile(path);
            return currentScene;
        }
            
    }
}
