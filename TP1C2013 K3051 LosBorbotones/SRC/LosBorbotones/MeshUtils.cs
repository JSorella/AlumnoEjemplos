﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.LosBorbotones
{
    class MeshUtils
    {

        public static TgcMesh loadMesh(string path)
        {
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene currentScene = loader.loadSceneFromFile(path);
            return currentScene.Meshes[0];
        }

    }
}
