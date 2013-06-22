using AlumnoEjemplos.LosBorbotones.Pantallas;
using TgcViewer;
using System;
using TgcViewer.Example;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Sound;
using TgcViewer.Utils;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using AlumnoEjemplos.LosBorbotones.Sonidos;
using AlumnoEjemplos.LosBorbotones.Autos;
using AlumnoEjemplos.LosBorbotones.Niveles;

namespace AlumnoEjemplos.LosBorbotones
{

    struct Shared
    {   
        //variables compartidas en todo el entorno
        public static float elapsedTimeChispa = 0;
        public static bool mostrarChispa = false;
        public static bool debugMode = false;
        public static string mediaPath = GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\";
    }

    public class EjemploAlumno : TgcExample
    {
        private Pantalla pantalla;
        private List<Auto> autos = new List<Auto>() ;
        private List<Nivel> niveles = new List<Nivel>();
        private List<TgcScene> columnas = new List<TgcScene>();
        private List<TgcScene> arboles = new List<TgcScene>();
        private List<Pantalla> pantallas = new List<Pantalla>();

        //variables para Blur
        Surface pOldRT;
        Texture renderTarget2D;
        VertexBuffer screenQuadVB;
        Effect effect;
        public bool activar_efecto = false;
        public float blur_intensity = 0.05f;
       
        public static EjemploAlumno instance;

        public static EjemploAlumno getInstance()
        {
            return instance;
        }

        public override string getCategory()
        {
            return "AlumnoEjemplos";
        }

        public override string getName()
        {
            return "LosBorbotones";
        }

        public override string getDescription()
        {
            return "Mario Kart - Física de Auto";
        }

        public override void init()
        {
            instance = this;

            pantallas.Add(new PantallaInicio());
            pantallas.Add(new PantallaFinalizacion(0));
            pantallas.Add(new PantallaFinalizacion(1));

            pantalla = pantallas[0];
          
            //Aumentamos el alcance del Frustum
            setDistanciaFrustum(200000f);
            
            //Paths de meshes de distintos vehículos
            string pathAutoMario = GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\Autos\\autoMario\\autoMario-TgcScene.xml";
            string pathAutoLuigi = GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\Autos\\autoLuigi\\autoLuigi-TgcScene.xml";

            // Creo los vehículos
            Auto autoMario = new Auto(pathAutoMario, "Mario", new Vector3(0, 0, 0), 2000, 90, 400, 47, new Vector3(2.4f, 2.4f, 2.4f), new Vector3(0,0,0));
            Auto autoLuigi = new Auto(pathAutoLuigi, "Luigi", new Vector3(0, 0, 0), 3000, 80, 500, 40, new Vector3(3f, 3f, 3f), new Vector3(0, 0, 0));
            this.autos.Add(autoMario);
            this.autos.Add(autoLuigi);

            TgcSceneLoader loader = new TgcSceneLoader();

            //Crea los arboles
            string pathArbol = GuiController.Instance.AlumnoEjemplosMediaDir + "\\LosBorbotones\\ambientacion\\arbol\\arbol-TgcScene.xml"; ;
            int cantidadDeArboles = 8;
            for (int i = 0; i < cantidadDeArboles;i++ )
                this.arboles.Add(loader.loadSceneFromFile(pathArbol));

            //Crea el circuito
            this.niveles.Add( new Nivel(1) );
            this.niveles.Add(new Nivel(2));
             
            /// EFECTO BLUR ///
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Activamos el renderizado customizado. De esta forma el framework nos delega control total sobre como dibujar en pantalla
            //La responsabilidad cae toda de nuestro lado
            GuiController.Instance.CustomRenderEnabled = true;


            //Se crean 2 triangulos (o Quad) con las dimensiones de la pantalla con sus posiciones ya transformadas
            // x = -1 es el extremo izquiedo de la pantalla, x = 1 es el extremo derecho
            // Lo mismo para la Y con arriba y abajo
            // la Z en 1 simpre
            CustomVertex.PositionTextured[] screenQuadVertices = new CustomVertex.PositionTextured[]
		    {
    			new CustomVertex.PositionTextured( -1, 1, 1, 0,0), 
			    new CustomVertex.PositionTextured(1,  1, 1, 1,0),
			    new CustomVertex.PositionTextured(-1, -1, 1, 0,1),
			    new CustomVertex.PositionTextured(1,-1, 1, 1,1)
    		};

            //vertex buffer de los triangulos
            screenQuadVB = new VertexBuffer(typeof(CustomVertex.PositionTextured),
                    4, d3dDevice, Usage.Dynamic | Usage.WriteOnly,
                        CustomVertex.PositionTextured.Format, Pool.Default);
            screenQuadVB.SetData(screenQuadVertices, 0, LockFlags.None);

            //Creamos un Render Targer sobre el cual se va a dibujar la pantalla        
            renderTarget2D = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                    , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                        Format.X8R8G8B8, Pool.Default);

            //Cargar shader con efectos de Post-Procesado
            string compilationErrors;
            effect = Effect.FromFile(GuiController.Instance.D3dDevice,
                GuiController.Instance.ExamplesMediaDir + "Shaders\\PostProcess.fx",
                null, null, ShaderFlags.None, null, out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            //Configurar Technique dentro del shader
            effect.Technique = "BlurTechnique";
            /// FIN EFECTO BLUR ///

            //MODIFIERS
            GuiController.Instance.Modifiers.addVertex2f("AlturaCamara", new Vector2(50, 200), new Vector2(800, 2000), new Vector2(200, 600));
            GuiController.Instance.Modifiers.addVertex2f("PosicionFlechaDebug", new Vector2(-5000, -5000), new Vector2(5000, 5000), new Vector2(0, 0));
        }

        public Pantalla getPantalla(int posicion) 
        {
            return this.pantallas[posicion];
            //0 es Inicio
            //1 es GameOver
        }
           
        public List<TgcScene> getArboles()
        {
            return this.arboles;
        }

        public Auto getAutos(int posicion)
        {
            return this.autos[posicion];
        }

        public Nivel getNiveles(int posicion) 
        {
            return this.niveles[posicion];
        }

       
        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            /// BLUR ///
            //Cargamos el Render Targer al cual se va a dibujar la escena 3D. Antes nos guardamos el surface original
            //En vez de dibujar a la pantalla, dibujamos a un buffer auxiliar, nuestro Render Target.
            pOldRT = d3dDevice.GetRenderTarget(0);
            Surface pSurf = renderTarget2D.GetSurfaceLevel(0);
            d3dDevice.SetRenderTarget(0, pSurf);
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            //Dibujamos la escena comun, pero en vez de a la pantalla al Render Target
            drawSceneToRenderTarget(d3dDevice,elapsedTime);

            //Liberar memoria de surface de Render Target
            pSurf.Dispose();

            //Ahora volvemos a restaurar el Render Target original (osea dibujar a la pantalla)
            d3dDevice.SetRenderTarget(0, pOldRT);

            //Luego tomamos lo dibujado antes y lo combinamos con una textura con efecto de blur
            drawPostProcess(d3dDevice);

            /// FIN BLUR ///
        }

        /// <summary>
        /// Dibujamos toda la escena pero en vez de a la pantalla, la dibujamos al Render Target que se cargo antes.
        /// Es como si dibujaramos a una textura auxiliar, que luego podemos utilizar.
        /// </summary>
        private void drawSceneToRenderTarget(Device d3dDevice, float elapsedTime)
        {
            //Arrancamos el renderizado. Esto lo tenemos que hacer nosotros a mano porque estamos en modo CustomRenderEnabled = true
            d3dDevice.BeginScene();

            //Dibujamos todos los meshes del escenario
            pantalla.render(elapsedTime);

            //Terminamos manualmente el renderizado de esta escena. Esto manda todo a dibujar al GPU al Render Target que cargamos antes
            d3dDevice.EndScene();
        }


        /// <summary>
        /// Se toma todo lo dibujado antes, que se guardo en una textura, y se le aplica un shader para borronear la imagen
        /// </summary>
        private void drawPostProcess(Device d3dDevice)
        {
            //Arrancamos la escena
            d3dDevice.BeginScene();

            //Cargamos para renderizar el unico modelo que tenemos, un Quad que ocupa toda la pantalla, con la textura de todo lo dibujado antes
            d3dDevice.VertexFormat = CustomVertex.PositionTextured.Format;
            d3dDevice.SetStreamSource(0, screenQuadVB, 0);

            //Ver si el efecto de oscurecer esta activado, configurar Technique del shader segun corresponda
            if (activar_efecto)
            {
                effect.Technique = "BlurTechnique";
            }
            else
            {
                effect.Technique = "DefaultTechnique";
            }

            //Cargamos parametros en el shader de Post-Procesado
            effect.SetValue("render_target2D", renderTarget2D);
            effect.SetValue("blur_intensity", blur_intensity);


            //Limiamos la pantalla y ejecutamos el render del shader
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.Begin(FX.None);
            effect.BeginPass(0);
            d3dDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();
            effect.End();

            //Terminamos el renderizado de la escena
            d3dDevice.EndScene();
        }

        public override void close()
        {
            //corta la música al salir
            TgcMp3Player player = GuiController.Instance.Mp3Player;
            player.closeFile();

            //dispose de blur
            effect.Dispose();
            screenQuadVB.Dispose();
            renderTarget2D.Dispose();
        }

        public void setPantalla(Pantalla _pantalla)
        {
            pantalla = _pantalla;
        }

        private void setDistanciaFrustum(float _distanciaFarPlane)
        {
            TgcD3dDevice.zFarPlaneDistance = _distanciaFarPlane;
            TgcD3dDevice tgcD3dDevice = new TgcD3dDevice(GuiController.Instance.Panel3d);
            tgcD3dDevice.OnResetDevice(tgcD3dDevice.D3dDevice, null);
        }

    }
}
