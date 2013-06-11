using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using System;
using System.Drawing;
using System.Collections.Generic;
using AlumnoEjemplos.LosBorbotones.Pantallas;

namespace AlumnoEjemplos.LosBorbotones.Autos
{
    public class Chispa
    {

        public TgcMesh chispa;
        private Device d3dDevice = GuiController.Instance.D3dDevice;
        private string sphere = GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Sphere\\Sphere-TgcScene.xml";
        private float INITIAL_HORIZONTAL_SPEED = -150f;
        object vertexBuffer = null;
        Type tipo;

        public Chispa()
        {
            TgcSceneLoader loader = new TgcSceneLoader();
            chispa = loader.loadSceneFromFile(sphere).Meshes[0];
            chispa.changeDiffuseMaps(new TgcTexture[] { TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesDir + "Transformations\\SistemaSolar\\SunTexture.jpg") });
            chispa.Scale = new Vector3(0.15f, 0.15f, 0.15f);
            //LUZ
            //Effect currentShader;
            //currentShader = GuiController.Instance.Shaders.TgcSkeletalMeshPointLightShader;
            ////Aplicar al mesh el shader actual
            //chispa.Effect = currentShader;
            ////El Technique depende del tipo RenderType del mesh
            ////chispa.Technique = chispa.RenderType;
            //chispa.Effect.SetValue("lightColor",ColorValue.FromColor( Color.White));
            //chispa.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(new Vector3(0, 70, 0)));
            //chispa.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(GuiController.Instance.FpsCamera.getPosition()));             
            //chispa.Effect.SetValue("lightIntensity", 100f);
            //chispa.Effect.SetValue("lightAttenuation", 0.9f);
            //chispa.Effect.SetValue("materialSpecularExp", 0.9f);

        //    switch (this.chispa.RenderType)
        //    {
        //        case TgcMesh.MeshRenderType.VERTEX_COLOR:
        //            vertexBuffer = (TgcSceneLoader.VertexColorVertex[])this.chispa.D3dMesh.LockVertexBuffer(
        //                typeof(TgcSceneLoader.VertexColorVertex), LockFlags.ReadOnly, this.chispa.D3dMesh.NumberVertices);

        //            break;

        //        case TgcMesh.MeshRenderType.DIFFUSE_MAP:
        //            vertexBuffer = (TgcSceneLoader.DiffuseMapVertex[])this.chispa.D3dMesh.LockVertexBuffer(
        //               typeof(TgcSceneLoader.DiffuseMapVertex), LockFlags.ReadOnly, this.chispa.D3dMesh.NumberVertices);

        //            break;

        //        case TgcMesh.MeshRenderType.DIFFUSE_MAP_AND_LIGHTMAP:
        //            vertexBuffer = (TgcSceneLoader.DiffuseMapAndLightmapVertex[])this.chispa.D3dMesh.LockVertexBuffer(
        //                typeof(TgcSceneLoader.DiffuseMapAndLightmapVertex), LockFlags.ReadOnly, this.chispa.D3dMesh.NumberVertices);

        //            break;
        //    }
        //    tipo = vertexBuffer.GetType();
        //    System.Reflection.MethodInfo dameValorPorIndice = tipo.GetMethod("GetValue", new Type[] { typeof(int) });
        //    System.Reflection.MethodInfo insertaValorPorIndice = tipo.GetMethod("SetValue", new Type[] { typeof(object), typeof(int) });
        //    int cantidadDeVertices = (int)tipo.GetProperty("Length").GetValue(vertexBuffer, null);


        //    for (int i = 0; i < cantidadDeVertices; i++)
        //    {
        //        object vertice = dameValorPorIndice.Invoke(vertexBuffer, new object[] { i });
        //        Vector3 posicion = (Vector3)vertice.GetType().GetField("Position").GetValue(vertice);
        //        vertice.GetType().GetField("Color").SetValue(vertice, Color.White.ToArgb());
        //    }
        //    this.chispa.D3dMesh.SetVertexBufferData(vertexBuffer, LockFlags.None);
        //    this.chispa.D3dMesh.UnlockVertexBuffer();
        }

        public void centerChange(Vector3 nuevaPosicion)
        {
            chispa.Position = nuevaPosicion;
        }

        public void render()
        {

                //this.flightTime += Shared.elapsedTimeChispa;
                Shared.elapsedTimeChispa++;
                //this.chispa.moveOrientedY(INITIAL_HORIZONTAL_SPEED);
                

                Random randomX = new Random();
                
                Random randomY = new Random();
                
                Random randomZ = new Random();

                this.chispa.move(randomX.Next(-10, 10), randomY.Next(-10, 10), randomZ.Next(-3, 5));
                //this.chispa.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(this.chispa.Position + new Vector3(10, 10, 10)));
                //this.chispa.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(GuiController.Instance.FpsCamera.getPosition()));   
                this.chispa.render();
                if (Shared.elapsedTimeChispa > 300)
                {
                    Shared.elapsedTimeChispa = 0f;
                    Shared.mostrarChispa = false; 
                }
            
        }

    }
}
