using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcSkeletalAnimation;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Input;
using Microsoft.DirectX.DirectInput;
using System.Threading;
using TgcViewer.Utils;



namespace AlumnoEjemplos.UN_TERAPEUTA
{

    /// <summary>
    /// Ejemplo del alumno
    /// </summary>
    public class EjemploAlumno : TgcExample
    {
        /// <summary>
        /// Categoría a la que pertenece el ejemplo.
        /// Influye en donde se va a haber en el árbol de la derecha de la pantalla.
        /// </summary>
        /// 

        enum Sentido { Adelante = 0, Atras = 1, CostadoIzq = 2, CostadoDer = 3, Ninguno = 4 };

        object vertices;

        TgcBox piso;

        List<TgcBox> obstaculos;

        Vehiculo vehiculo = new Vehiculo(new Vector3(200, 0, 0), GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\" + "Auto\\Auto-TgcScene.xml");

        Microsoft.DirectX.DirectInput.Device keyboardDevice;

        List<KeyValuePair<string, object>> variablesDeInterfaz;

        const float CERO_UNIVERSAL = 0.000f;

        const float PIES_POR_METRO = 3.280839895013123f;
        const float KILOGRAMOS_POR_LIBRA = 0.4536f;
        const float LIBRAS_POR_SLUG = 32.1f;
        const float NEWTONS_POR_LIBRA = 4.45f;

        const float MASA_AUTO_PROMEDIO = 100 * LIBRAS_POR_SLUG * KILOGRAMOS_POR_LIBRA;
        const float COEFICIENTE_FRICCION_DINAMICO = 0.8f;
        const float GRAVEDAD = 9.81f;
        const float ACELERACION_DE_FRENADO = GRAVEDAD;

        const float COEFF_DRAG = 0.30f;
        const float FRONTAL_AREA = 20f;
        const float DENSITY_AIR = 0.0025f;
        const float ROLL_RESIST = 0.696f;
        const float ENGINE_TORQUE = 150f;
        const float REAR_END_RATIO = 3.07f;
        const float WHEEL_DIAMETER = 2.167f;

        const float FIRST_GEAR_RATIO = 2.88f;
        const float SECOND_GEAR_RATIO = 1.91f;
        const float THIRD_GEAR_RATIO = 1.33f;
        const float FOURTH_GEAR_RATIO = 1.00f;

        //deformacion malla
        const float FACTOR_RADIO_IMPACTO = 1f;
        const float FACTOR_DANIO = 1f;

        const float TIEMPO_PROMEDIO_DE_DETENCION_POR_COLISION = 0.1f;
        const float FACTOR_DE_ELASTICIDAD = 0.5f;

        const int CANT_POSICIONES = 1000;

        class BoundingBox
        {

            TgcBoundingBox box;

            public TgcBoundingBox Box
            {
                get { return box; }
                set { box = value; }
            }

            Vector3 pMinOriginal;
            Vector3 pMaxOriginal;

            Vector3 pMin;

            public Vector3 PMin
            {
                get { return pMin; }
            }

            Vector3 pMax;

            public Vector3 PMax
            {
                get { return pMax; }
            }

            int renderColor;


            CustomVertex.PositionColored[] vertices;
            bool dirtyValues;


            public BoundingBox()
            {
                renderColor = Color.Yellow.ToArgb();
                dirtyValues = true;
            }


            public BoundingBox(Vector3 pMin, Vector3 pMax)
                : this()
            {
                setExtremes(pMin, pMax);
            }

            public BoundingBox(TgcBoundingBox box)
                : this()
            {
                this.box = box;
                setExtremes(this.box.PMin, this.box.PMax);
            }


            public void setExtremes(Vector3 pMin, Vector3 pMax)
            {
                this.pMin = pMin;
                this.pMax = pMax;
                pMinOriginal = pMin;
                pMaxOriginal = pMax;

                dirtyValues = true;
            }

            public override string ToString()
            {
                return "Min[" + TgcParserUtils.printFloat(pMin.X) + ", " + TgcParserUtils.printFloat(pMin.Y) + ", " + TgcParserUtils.printFloat(pMin.Z) + "]" +
                    " Max[" + TgcParserUtils.printFloat(pMax.X) + ", " + TgcParserUtils.printFloat(pMax.Y) + ", " + TgcParserUtils.printFloat(pMax.Z) + "]";
            }

            public void scaleTranslate(Vector3 position, Vector3 scale)
            {

                //actualizar puntos extremos
                pMin.X = pMinOriginal.X * scale.X + position.X;
                pMin.Y = pMinOriginal.Y * scale.Y + position.Y;
                pMin.Z = pMinOriginal.Z * scale.Z + position.Z;

                pMax.X = pMaxOriginal.X * scale.X + position.X;
                pMax.Y = pMaxOriginal.Y * scale.Y + position.Y;
                pMax.Z = pMaxOriginal.Z * scale.Z + position.Z;

                dirtyValues = true;
            }


            public void dispose()
            {
                vertices = null;
            }

            public void render(double beta, double hip, double rotacion, Vector3 translacion)
            {
                Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;
                TgcTexture.Manager texturesManager = GuiController.Instance.TexturesManager;

                texturesManager.clear(0);
                texturesManager.clear(1);
                d3dDevice.Material = TgcD3dDevice.DEFAULT_MATERIAL;
                d3dDevice.Transform.World = Matrix.Identity;

                //Actualizar vertices de BoundingBox solo si hubo una modificación
                if (dirtyValues)
                {
                    updateValues(beta, hip, rotacion, translacion);
                    dirtyValues = false;
                }

                d3dDevice.VertexFormat = CustomVertex.PositionColored.Format;
                d3dDevice.DrawUserPrimitives(PrimitiveType.LineList, 12, vertices);
            }

            private void updateValues(double beta, double hip, double rotacion, Vector3 position)
            {
                if (vertices == null)
                {
                    vertices = vertices = new CustomVertex.PositionColored[24];
                }

                //Cuadrado de atras
                vertices[0] = new CustomVertex.PositionColored(pMin.X, pMin.Y, pMin.Z, renderColor);

                double gama = -beta - rotacion;

                float xMax = (float)(hip * Math.Cos(gama));
                float zMin = (float)(hip * Math.Sin(gama));

                xMax = xMax + position.X;
                zMin = zMin + position.Z;

                vertices[1] = new CustomVertex.PositionColored(xMax, pMin.Y, zMin, renderColor);

                vertices[2] = new CustomVertex.PositionColored(pMin.X, pMin.Y, pMin.Z, renderColor);
                vertices[3] = new CustomVertex.PositionColored(pMin.X, pMax.Y, pMin.Z, renderColor);



                vertices[4] = new CustomVertex.PositionColored(pMin.X, pMax.Y, pMin.Z, renderColor);
                vertices[5] = new CustomVertex.PositionColored(xMax, pMax.Y, zMin, renderColor);



                vertices[6] = new CustomVertex.PositionColored(xMax, pMin.Y, zMin, renderColor);
                vertices[7] = new CustomVertex.PositionColored(xMax, pMax.Y, zMin, renderColor);


                gama = Math.PI - beta - rotacion;

                float xMin = (float)(hip * Math.Cos(gama));
                float zMax = (float)(hip * Math.Sin(gama));

                xMin = xMin + position.X;
                zMax = zMax + position.Z;

                //Cuadrado de adelante
                vertices[8] = new CustomVertex.PositionColored(xMin, pMin.Y, zMax, renderColor);
                vertices[9] = new CustomVertex.PositionColored(pMax.X, pMin.Y, pMax.Z, renderColor);

                vertices[10] = new CustomVertex.PositionColored(xMin, pMin.Y, zMax, renderColor);
                vertices[11] = new CustomVertex.PositionColored(xMin, pMax.Y, zMax, renderColor);

                vertices[12] = new CustomVertex.PositionColored(xMin, pMax.Y, zMax, renderColor);
                vertices[13] = new CustomVertex.PositionColored(pMax.X, pMax.Y, pMax.Z, renderColor);

                vertices[14] = new CustomVertex.PositionColored(pMax.X, pMin.Y, pMax.Z, renderColor);
                vertices[15] = new CustomVertex.PositionColored(pMax.X, pMax.Y, pMax.Z, renderColor);

                //Union de ambos cuadrados
                vertices[16] = new CustomVertex.PositionColored(pMin.X, pMin.Y, pMin.Z, renderColor);
                vertices[17] = new CustomVertex.PositionColored(xMin, pMin.Y, zMax, renderColor);

                vertices[18] = new CustomVertex.PositionColored(xMax, pMin.Y, zMin, renderColor);
                vertices[19] = new CustomVertex.PositionColored(pMax.X, pMin.Y, pMax.Z, renderColor);

                vertices[20] = new CustomVertex.PositionColored(pMin.X, pMax.Y, pMin.Z, renderColor);
                vertices[21] = new CustomVertex.PositionColored(xMin, pMax.Y, zMax, renderColor);

                vertices[22] = new CustomVertex.PositionColored(xMax, pMax.Y, zMin, renderColor);
                vertices[23] = new CustomVertex.PositionColored(pMax.X, pMax.Y, pMax.Z, renderColor);
            }

        }

        class Velocidad
        {
            public float magnitud;
            public Vector3 vector;

            public Velocidad()
            {
                this.magnitud = 0f;
                vector = new Vector3(0f, 0f, 0f);
            }

            public void actualizarVector(float rotacion)
            {
                vector.X = this.magnitud * (float)Math.Cos(rotacion);
                vector.Z = this.magnitud * (float)Math.Sin(rotacion);
            }

            public void actualizarMagnitud()
            {
                int signo = (int)(this.magnitud / (float)Math.Abs(this.magnitud));

                this.magnitud = (float)Math.Sqrt(Math.Pow(this.vector.X, 2.0f) + Math.Pow(this.vector.Z, 2.0f)) * signo;
            }

            public float dameMagnitudAbsoluta()
            {
                return Math.Abs(this.magnitud);
            }
        }

        class Util
        {
            public static bool estaDentroDeLaTolerancia(float valor)
            {
                return Math.Abs(Math.Round(valor, 3)) == CERO_UNIVERSAL;
            }

            public static float dameDistanciaUnidimensional(float x, float y)
            {
                return Math.Abs(y - x);
            }

            public static float calcularDistanciaEntrePuntos(Vector3 punto1, Vector3 punto2)
            {
                return (float)Math.Sqrt(Math.Pow(punto1.X - punto2.X, 2.0) + Math.Pow(punto1.Z - punto2.Z, 2.0));
            }

            public static bool vectoresIguales(Vector3 punto1, Vector3 punto2)
            {
                return (punto1.X == punto2.X) && (punto1.Y == punto2.Y) && (punto1.Z == punto2.Z);
            }
        }

        class Vehiculo
        {
            public Vehiculo(Vector3 initialPos, string meshPath)
            {
                this.initialPos = initialPos;
                this.meshPath = meshPath;
                this.velocidad = new Velocidad();
            }

            public Vector3 initialPos;
            public string meshPath;
            public BoundingBox bB;
            public TgcMesh mesh;
            public Velocidad velocidad;
        }

        class Recta
        {
            public Vector3 puntoInicial;
            public Vector3 puntoFinal;
            public float pendiente;

            public Recta(Vector3 punto1, Vector3 punto2)
            {
                if (punto1.X < punto2.X)
                {
                    this.puntoInicial = punto1;
                    this.puntoFinal = punto2;
                }
                else
                {
                    this.puntoInicial = punto2;
                    this.puntoFinal = punto1;
                }

                this.pendiente = (this.puntoFinal.X == this.puntoInicial.X) ? float.PositiveInfinity : (this.puntoFinal.Z - this.puntoInicial.Z) / (this.puntoFinal.X - this.puntoInicial.X);
            }

            public void dameAnguloInt(Recta recta, Vector3 puntoInt, ref float angulo)
            {
                float altura = puntoInt.Z - this.puntoInicial.Z;
                float ancho = puntoInt.X - this.puntoInicial.X;

                float distancia1 = (float)Math.Sqrt(Math.Pow(puntoInt.X - this.puntoInicial.X, 2.0f) + Math.Pow(puntoInt.Z - this.puntoInicial.Z, 2.0f));
                float distancia2 = (float)Math.Sqrt(Math.Pow(puntoInt.X - this.puntoFinal.X, 2.0f) + Math.Pow(puntoInt.Z - this.puntoFinal.Z, 2.0f));

                if (distancia2 > distancia1)
                {
                    ancho = puntoInt.X - this.puntoFinal.X;
                    altura = puntoInt.Z - this.puntoFinal.Z;
                }

                float radio = (float)Math.Sqrt(Math.Pow(ancho, 2.0f) + Math.Pow(altura, 2.0f));

                if (recta.pendiente == 0f)
                {
                    if (Math.Abs(ancho) > 0)
                    {
                        angulo = (float)Math.Atan(Math.Abs(altura / ancho));
                    }

                }
                else
                {
                    if (Math.Abs(altura) > 0)
                    {
                        angulo = -1 * (float)Math.Atan(Math.Abs(ancho / altura));
                    }
                }

            }

            public bool damePuntoInt(Recta rec, ref Vector3 puntoInt)
            {
                float x;
                float z;

                if (this.pendiente == rec.pendiente)
                {
                    if (rec.pendiente == 0f)
                    {
                        if (Util.estaDentroDeLaTolerancia(this.puntoInicial.Z - rec.puntoInicial.Z))
                        {
                            z = this.puntoInicial.Z;

                            if (this.puntoInicial.X >= rec.puntoInicial.X && this.puntoInicial.X <= rec.puntoFinal.X)
                            {
                                if (Util.dameDistanciaUnidimensional(this.puntoInicial.X, rec.puntoFinal.X) < Util.dameDistanciaUnidimensional(this.puntoInicial.X, this.puntoFinal.X))
                                {
                                    x = (this.puntoInicial.X + rec.puntoFinal.X) / 2f;
                                }
                                else
                                {
                                    x = (this.puntoInicial.X + this.puntoFinal.X) / 2f;
                                }

                                puntoInt.X = x;
                                puntoInt.Z = z;

                                return true;
                            }
                            else if (this.puntoFinal.X >= rec.puntoInicial.X && this.puntoFinal.X <= rec.puntoFinal.X)
                            {
                                x = (rec.puntoInicial.X + this.puntoFinal.X) / 2f;

                                puntoInt.X = x;
                                puntoInt.Z = z;

                                return true;
                            }
                        }
                    }
                    else
                    {
                        if (Util.estaDentroDeLaTolerancia(this.puntoInicial.X - rec.puntoInicial.X))
                        {
                            x = this.puntoInicial.X;

                            if (this.puntoInicial.Z >= rec.puntoInicial.Z && this.puntoInicial.Z <= rec.puntoFinal.Z)
                            {
                                if (Util.dameDistanciaUnidimensional(this.puntoInicial.Z, rec.puntoFinal.Z) < Util.dameDistanciaUnidimensional(this.puntoInicial.Z, this.puntoFinal.Z))
                                {
                                    z = (this.puntoInicial.Z + rec.puntoFinal.Z) / 2f;
                                }
                                else
                                {
                                    z = (this.puntoInicial.Z + this.puntoFinal.Z) / 2f;
                                }

                                puntoInt.X = x;
                                puntoInt.Z = z;

                                return true;
                            }
                            else if (this.puntoFinal.Z >= rec.puntoInicial.Z && this.puntoFinal.Z <= rec.puntoFinal.Z)
                            {
                                z = (rec.puntoInicial.Z + this.puntoFinal.Z) / 2f;

                                puntoInt.X = x;
                                puntoInt.Z = z;

                                return true;
                            }
                        }
                    }

                    puntoInt.X = float.NaN;
                    puntoInt.Z = float.NaN;

                    return false;
                }

                if (rec.pendiente == 0f)
                {
                    x = (rec.puntoInicial.Z - this.puntoInicial.Z) / this.pendiente + this.puntoInicial.X;
                    z = rec.puntoInicial.Z;
                }
                else if (rec.pendiente == float.PositiveInfinity)
                {
                    x = rec.puntoInicial.X;
                    z = (x - this.puntoInicial.X) * this.pendiente + this.puntoInicial.Z;
                }
                else
                {
                    x = ((this.pendiente / rec.pendiente * -1) * this.puntoInicial.X + ((this.puntoInicial.Z - rec.puntoInicial.Z) / rec.pendiente) + rec.puntoInicial.X) / (1 - this.pendiente / rec.pendiente);
                    z = (x - this.puntoInicial.X) * this.pendiente + this.puntoInicial.Z;
                }

                puntoInt.X = x;
                puntoInt.Z = z;


                bool valor = (puntoInt.X >= this.puntoInicial.X && puntoInt.X <= this.puntoFinal.X) && ((puntoInt.Z >= this.puntoInicial.Z && puntoInt.Z <= this.puntoFinal.Z) || (puntoInt.Z >= this.puntoFinal.Z && puntoInt.Z <= this.puntoInicial.Z));

                return valor && (puntoInt.X >= rec.puntoInicial.X && puntoInt.X <= rec.puntoFinal.X) && ((puntoInt.Z >= rec.puntoInicial.Z && puntoInt.Z <= rec.puntoFinal.Z) || (puntoInt.Z >= rec.puntoFinal.Z && puntoInt.Z <= rec.puntoInicial.Z));
            }

            /*public bool interSecta(Recta rec, Vector3 puntoInt)
            {
                bool valor = (puntoInt.X >= this.puntoInicial.X && puntoInt.X <= this.puntoFinal.X) && ((puntoInt.Z >= this.puntoInicial.Z && puntoInt.Z <= this.puntoFinal.Z) || (puntoInt.Z >= this.puntoFinal.Z && puntoInt.Z <= this.puntoInicial.Z));

                return valor && (puntoInt.X >= rec.puntoInicial.X && puntoInt.X <= rec.puntoFinal.X) && ((puntoInt.Z >= rec.puntoInicial.Z && puntoInt.Z <= rec.puntoFinal.Z) || (puntoInt.Z >= rec.puntoFinal.Z && puntoInt.Z <= rec.puntoInicial.Z));
            }*/

            public override string ToString()
            {
                return this.puntoInicial.X + " ; " + this.puntoInicial.Z + " : " + this.puntoFinal.X + " ; " + this.puntoFinal.Z;
            }
        }

        public override string getCategory()
        {
            return "AlumnoEjemplos";
        }

        /// <summary>
        /// Completar nombre del grupo en formato Grupo NN
        /// </summary>
        public override string getName()
        {
            return "UN_TERAPEUTA";
        }

        /// <summary>
        /// Completar con la descripción del TP
        /// </summary>
        public override string getDescription()
        {
            return "UN_TERAPEUTA";
        }

        /// <summary>
        /// Método que se llama una sola vez,  al principio cuando se ejecuta el ejemplo.
        /// Escribir aquí todo el código de inicialización: cargar modelos, texturas, modifiers, uservars, etc.
        /// Borrar todo lo que no haga falta
        /// </summary>
        public override void init()
        {
            //GuiController.Instance: acceso principal a todas las herramientas del Framework
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

            //Device de DirectX para crear primitivas
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            //keyboard
            keyboardDevice = new Microsoft.DirectX.DirectInput.Device(SystemGuid.Keyboard);
            keyboardDevice.SetCooperativeLevel(GuiController.Instance.MainForm, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
            keyboardDevice.Acquire();

            //Carpeta de archivos Media del alumno
            string alumnoMediaFolder = GuiController.Instance.AlumnoEjemplosMediaDir;


            GuiController.Instance.UserVars.addVar("velocidad");


            //Modifier para ver BoundingBox
            GuiController.Instance.Modifiers.addBoolean("showBoundingBox", "Bouding Box", true);
            GuiController.Instance.Modifiers.addBoolean("seguirLaRotacion", "Rotar camara", true);

            variablesDeInterfaz = new List<KeyValuePair<string, object>>();

            variablesDeInterfaz.Add(new KeyValuePair<string, object>("velocidad_rotacion", 35f));
            variablesDeInterfaz.Add(new KeyValuePair<string, object>("velocidad_caminar", 0f));
            variablesDeInterfaz.Add(new KeyValuePair<string, object>("ultimaRotacion", 0f));
            variablesDeInterfaz.Add(new KeyValuePair<string, object>("colisiono", false));
            variablesDeInterfaz.Add(new KeyValuePair<string, object>("tiempoDeDesaceleracion", 0f));
            variablesDeInterfaz.Add(new KeyValuePair<string, object>("rotacionPorColision", 0f));
            variablesDeInterfaz.Add(new KeyValuePair<string, object>("lastPos", new List<Vector3>()));
            variablesDeInterfaz.Add(new KeyValuePair<string, object>("lastRot", new List<float>()));


            //Crear piso
            TgcTexture pisoTexture = TgcTexture.createTexture(d3dDevice,
                GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\TexturePack2\\rock_wall.jpg");
            piso = TgcBox.fromSize(new Vector3(0, -5, 0), new Vector3(100000, 5, 100000), pisoTexture);

            obstaculos = new List<TgcBox>();
            TgcBox obstaculo;


            for (int i = 1; i <= 1; i++)
            {
                //Obstaculo 1
                obstaculo = TgcBox.fromSize(
                    new Vector3(-100 * i, 75, 0),
                    new Vector3(80, 150, 80),
                    TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\baldosaFacultad.jpg"));
                obstaculos.Add(obstaculo);

                obstaculo = TgcBox.fromSize(
                    new Vector3(-200 * i, 95, 0),
                    new Vector3(80, 150, 80),
                    TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\baldosaFacultad.jpg"));
                obstaculos.Add(obstaculo);

                //Obstaculo 2
                obstaculo = TgcBox.fromSize(
                    new Vector3(50 * i, 150, 200),
                    new Vector3(80, 300, 80),
                    TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\madera.jpg"));
                obstaculos.Add(obstaculo);

                //Obstaculo 3
                obstaculo = TgcBox.fromSize(
                    new Vector3(300 * i, 50, 100),
                    new Vector3(80, 100, 150),
                    TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\granito.jpg"));
                obstaculos.Add(obstaculo);

                //Obstaculo 3
                obstaculo = TgcBox.fromSize(
                    new Vector3(400 * i, 100, 100),
                    new Vector3(80, 100, 150),
                    TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\granito.jpg"));
                obstaculos.Add(obstaculo);

            }

            float wallSize = 100000;
            float wallHeight = 500;

            //Obstaculo 5
            obstaculo = TgcBox.fromSize(
                new Vector3(0, 250, -1700),
                new Vector3(wallSize, wallHeight, 10),
                TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\madera.jpg"));
            obstaculos.Add(obstaculo);

            //Obstaculo 5
            obstaculo = TgcBox.fromSize(
                new Vector3(0, 250, 1700),
                new Vector3(wallSize, wallHeight, 10),
                TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\madera.jpg"));
            obstaculos.Add(obstaculo);

            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(vehiculo.meshPath);
            TgcMesh meshPrincipal = scene.Meshes[0];
            vehiculo.mesh = meshPrincipal;

            //Ubicarlo en escenario
            vehiculo.mesh.Position = vehiculo.initialPos;

            switch (vehiculo.mesh.RenderType)
            {
                case TgcMesh.MeshRenderType.VERTEX_COLOR:
                    TgcSceneLoader.VertexColorVertex[] verts1 = (TgcSceneLoader.VertexColorVertex[])vehiculo.mesh.D3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.VertexColorVertex), LockFlags.ReadOnly, vehiculo.mesh.D3dMesh.NumberVertices);
                    vehiculo.mesh.D3dMesh.UnlockVertexBuffer();
                    this.vertices = verts1;
                    break;

                case TgcMesh.MeshRenderType.DIFFUSE_MAP:
                    TgcSceneLoader.DiffuseMapVertex[] verts2 = (TgcSceneLoader.DiffuseMapVertex[])vehiculo.mesh.D3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.DiffuseMapVertex), LockFlags.ReadOnly, vehiculo.mesh.D3dMesh.NumberVertices);
                    this.vertices = verts2;
                    break;

                case TgcMesh.MeshRenderType.DIFFUSE_MAP_AND_LIGHTMAP:
                    TgcSceneLoader.DiffuseMapAndLightmapVertex[] verts3 = (TgcSceneLoader.DiffuseMapAndLightmapVertex[])vehiculo.mesh.D3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.DiffuseMapAndLightmapVertex), LockFlags.ReadOnly, vehiculo.mesh.D3dMesh.NumberVertices);
                    vehiculo.mesh.D3dMesh.UnlockVertexBuffer();
                    this.vertices = verts3;
                    break;
            }

            //Se crea un boundingBox propio
            TgcBoundingBox bB = TgcBoundingBox.computeFromPoints(vehiculo.mesh.getVertexPositions());//Los puntos son con respecto a las dimensiones de la malla
            vehiculo.bB = new BoundingBox(bB);
            variablesDeInterfaz.Add(new KeyValuePair<string, object>("beta", Math.Atan2(vehiculo.bB.PMax.Z, vehiculo.bB.PMax.X)));
            variablesDeInterfaz.Add(new KeyValuePair<string, object>("hip", Math.Sqrt(Math.Pow(vehiculo.bB.PMax.X, 2f) + Math.Pow(vehiculo.bB.PMax.Z, 2f))));
            variablesDeInterfaz.Add(new KeyValuePair<string, object>("radio", vehiculo.bB.PMax.Z));

            vehiculo.bB.scaleTranslate(vehiculo.initialPos, new Vector3(1f, 1f, 1f));//Hay que situar el boundingBox donde esta posicionado la malla
            vehiculo.bB.setExtremes(vehiculo.bB.PMin, vehiculo.bB.PMax); //llevar a valores absolutos

            //Camara en 3ra persona
            GuiController.Instance.ThirdPersonCamera.Enable = true;
            GuiController.Instance.ThirdPersonCamera.resetValues();
            GuiController.Instance.ThirdPersonCamera.setCamera(vehiculo.mesh.Position, 100, 400);

        }

        /// <summary>
        /// Método que se llama cada vez que hay que refrescar la pantalla.
        /// Escribir aquí todo el código referido al renderizado.
        /// Borrar todo lo que no haga falta
        /// </summary>
        /// <param name="elapsedTime">Tiempo en segundos transcurridos desde el último frame</param>
        public override void render(float elapsedTime)
        {
            actualizar(elapsedTime);
            renderizar();
        }

        private void renderizar()
        {

            GuiController.Instance.UserVars.setValue("velocidad", Math.Round((vehiculo.velocidad.magnitud) * (3600 / 1000), 1, MidpointRounding.AwayFromZero).ToString("0.0"));

            //Render piso
            piso.render();

            //Renderizar meshPrincipal
            vehiculo.mesh.render();

            if ((bool)GuiController.Instance.Modifiers.getValue("showBoundingBox"))
                vehiculo.bB.render((double)this.getValorDeVariable("beta"), (double)this.getValorDeVariable("hip"), vehiculo.mesh.Rotation.Y, vehiculo.mesh.Position);


            //Render obstaculos
            foreach (TgcBox obstaculo in obstaculos)
            {
                obstaculo.render();
                if ((bool)GuiController.Instance.Modifiers.getValue("showBoundingBox"))
                    obstaculo.BoundingBox.render();
            }
        }

        private void actualizar(float elapsedTime)
        {
            //Calcular proxima posicion de personaje segun Input
            float moveForward = 0f;
            float rotate = vehiculo.mesh.Rotation.Y;
            float velocidad = vehiculo.velocidad.magnitud;//(float)this.getValorDeVariable("velocidad_caminar");
            bool[] sentido = new bool[] { false, false, false, false, true };
            Vector3 lastPos = vehiculo.mesh.Position;
            float lastRot = vehiculo.mesh.Rotation.Y;
            bool sePresionaFreno = false;
            bool rotoMediaVuelta = false;
            float aceleracion = 0f;
            float anguloColision = 0f;
            bool colisiono = false;
            float rotacion = 0f;

            if (!(bool)this.getValorDeVariable("colisiono"))
            {
                this.calcularFw(elapsedTime, ref aceleracion);

                this.gestionarTeclas(ref rotate, ref velocidad, ref sentido, ref sePresionaFreno, ref rotoMediaVuelta, ref rotacion, aceleracion, elapsedTime);

                this.aplicarRozamiento(ref velocidad, elapsedTime);

                this.aplicarRotacion(rotate); //Si hubo rotacion              

                vehiculo.velocidad.magnitud = velocidad;
                vehiculo.velocidad.actualizarVector((float)Math.PI / 2 - vehiculo.mesh.Rotation.Y);

                moveForward = velocidad * -1;

                this.aplicarMovimiento(sePresionaFreno);

                bool[] resultado = null;


                foreach (TgcBox obstaculo in obstaculos)
                {
                    resultado = this.colisionaConElObjeto(vehiculo.bB, obstaculo.BoundingBox, velocidad, ref anguloColision);
                    colisiono = this.colisiono(resultado);

                    if (colisiono)
                    {
                        break;
                    }
                }

                float anguloColisionEnGrados = Geometry.RadianToDegree(anguloColision);

                //Si hubo colision, restaurar la posicion anterior

                if (colisiono)
                {
                    float velocidadAngular = 0f;
                    int signo = (int)Math.Round((vehiculo.velocidad.vector.Z * vehiculo.velocidad.vector.X) / (float)Math.Abs(vehiculo.velocidad.vector.Z * vehiculo.velocidad.vector.X));

                    this.volverAtras();

                    this.setValorDeVariable("colisiono", true);

                    if (Math.Abs(anguloColisionEnGrados) > 45)
                    {
                        velocidad = velocidad * FACTOR_DE_ELASTICIDAD * -1;

                        vehiculo.velocidad.magnitud = velocidad;
                        vehiculo.velocidad.actualizarVector((float)Math.PI / 2 - vehiculo.mesh.Rotation.Y);

                    }
                    else
                    {
                        sePresionaFreno = GuiController.Instance.D3dInput.keyDown(Key.Space);

                        if (anguloColision > 0)
                        {
                            signo *= -1;

                            vehiculo.velocidad.vector.Z = vehiculo.velocidad.vector.Z * FACTOR_DE_ELASTICIDAD * -1;
                            vehiculo.velocidad.actualizarMagnitud();

                            if (!Util.estaDentroDeLaTolerancia(vehiculo.velocidad.vector.Z) )
                                velocidadAngular = Math.Abs(vehiculo.velocidad.vector.Z) / (float)this.getValorDeVariable("radio");
                            else if (sePresionaFreno)
                                velocidadAngular = rotacion * elapsedTime * FACTOR_DE_ELASTICIDAD * 2;
                            
                        }
                        else
                        {

                            vehiculo.velocidad.vector.X = vehiculo.velocidad.vector.X * FACTOR_DE_ELASTICIDAD * -1;
                            vehiculo.velocidad.actualizarMagnitud();

                            if (!Util.estaDentroDeLaTolerancia(vehiculo.velocidad.vector.X))
                                velocidadAngular = Math.Abs(vehiculo.velocidad.vector.X) / (float)this.getValorDeVariable("radio");
                            else if (sePresionaFreno)
                                velocidadAngular = rotacion * elapsedTime * FACTOR_DE_ELASTICIDAD * 2;

                        }

                    }

                    this.setValorDeVariable("rotacionPorColision", (velocidadAngular * signo));
                }


                if (Util.estaDentroDeLaTolerancia(velocidad))
                {
                    velocidad = CERO_UNIVERSAL;
                }

            }
            else
            {
                float tiempoDesaceleracion = (float)this.getValorDeVariable("tiempoDeDesaceleracion");

                tiempoDesaceleracion += elapsedTime;

                float rotar = (float)this.getValorDeVariable("rotacionPorColision");

                this.aplicarRotacion(this.vehiculo.mesh.Rotation.Y - rotar);

                this.aplicarMovimiento(false);

                if (tiempoDesaceleracion > TIEMPO_PROMEDIO_DE_DETENCION_POR_COLISION)
                {

                    this.setValorDeVariable("colisiono", false);
                    this.setValorDeVariable("rotacionPorColision", 0f);
                    this.setValorDeVariable("tiempoDeDesaceleracion", 0f);
                }
                else
                {
                    this.setValorDeVariable("tiempoDeDesaceleracion", tiempoDesaceleracion);
                }
            }

            this.rotarBoundingBox();
            this.moverBoundingBox(moveForward, Sentido.Adelante);

            if (!colisiono)
                this.guardarUltimaPos(lastPos, lastRot);

            //Hacer que la camara siga al personaje en su nueva posicion
            GuiController.Instance.ThirdPersonCamera.Target = vehiculo.mesh.Position;

            if ((bool)GuiController.Instance.Modifiers.getValue("seguirLaRotacion"))
            {
                float anguloRotacion;

                float rotacionVehiculo = (!rotoMediaVuelta) ? vehiculo.mesh.Rotation.Y : vehiculo.mesh.Rotation.Y + (float)Math.PI;

                float diferencia = rotacionVehiculo - GuiController.Instance.ThirdPersonCamera.RotationY;

                diferencia *= elapsedTime;

                if (!sePresionaFreno)
                    diferencia *= Math.Abs(velocidad);

                anguloRotacion = GuiController.Instance.ThirdPersonCamera.RotationY + diferencia;

                GuiController.Instance.ThirdPersonCamera.RotationY = anguloRotacion;
            }

        }

        private void guardarUltimaPos(Vector3 lastPos, float lastRot)
        {
            List<Vector3> ultimasPos = (List<Vector3>)this.getValorDeVariable("lastPos");
            List<float> ultimasRot = (List<float>)this.getValorDeVariable("lastRot");

            if (ultimasPos.Count > CANT_POSICIONES && ultimasRot.Count > CANT_POSICIONES)
            {
                ultimasPos.RemoveRange(CANT_POSICIONES, ultimasPos.Count - CANT_POSICIONES);
                ultimasRot.RemoveRange(CANT_POSICIONES, ultimasRot.Count - CANT_POSICIONES);
            }

            if (ultimasPos.Count == 0 || !Util.vectoresIguales(lastPos, ultimasPos[ultimasPos.Count - 1]))
                ultimasPos.Add(lastPos);

            if (ultimasRot.Count == 0 || lastRot != ultimasRot[ultimasRot.Count - 1])
                ultimasRot.Add(lastRot);

            this.setValorDeVariable("lastPos", ultimasPos);
            this.setValorDeVariable("lastRot", ultimasRot);
        }

        private void volverAtras()
        {
            List<Vector3> ultimasPos = (List<Vector3>)this.getValorDeVariable("lastPos");
            List<float> ultimasRot = (List<float>)this.getValorDeVariable("lastRot");

            Vector3 lastPos = ultimasPos[ultimasPos.Count - 1];
            float lastRot = ultimasRot[ultimasRot.Count - 1];

            volverPosicionAnterior(lastPos);
            volverRotacionAnterior(lastRot);

            ultimasPos.RemoveAt(ultimasPos.Count - 1);
            ultimasRot.RemoveAt(ultimasRot.Count - 1);
        }

        private bool colisiono(bool[] resultado)
        {
            return resultado[(int)Sentido.Adelante] || resultado[(int)Sentido.Atras] || resultado[(int)Sentido.CostadoIzq] || resultado[(int)Sentido.CostadoDer];
        }

        private bool colisionFrontal(bool[] resultado)
        {
            return resultado[(int)Sentido.Adelante] || resultado[(int)Sentido.Atras];
        }

        private bool colisionaAmbosFrente(bool[] resultado)
        {
            return resultado[(int)Sentido.Adelante] && resultado[(int)Sentido.Atras];
        }

        private void volverRotacionAnterior(float rotacionAbsoluta)
        {
            vehiculo.mesh.rotateY(rotacionAbsoluta - vehiculo.mesh.Rotation.Y);
        }

        private void volverPosicionAnterior(Vector3 lastPos)
        {
            vehiculo.mesh.Position = lastPos;
        }

        private void moverBoundingBox(float moveForward, Sentido sentido)
        {
            float rotacion = (float)this.getValorDeVariable("ultimaRotacion");

            float x = (float)Math.Sin(rotacion) * moveForward;
            float z = (float)Math.Cos(rotacion) * moveForward;

            Vector3 moverse = new Vector3(x * (int)sentido, 0f, z * (int)sentido);
            vehiculo.bB.scaleTranslate(moverse, new Vector3(1f, 1f, 1f));
            vehiculo.bB.setExtremes(vehiculo.bB.PMin, vehiculo.bB.PMax);
        }

        private void gestionarTeclas(ref float rotate, ref float velocidad, ref bool[] sentido, ref bool sePresionaFreno, ref bool rotoMediaVuelta, ref float rotacion, float aceleracion, float elapsedTime)
        {
            Key[] teclas = keyboardDevice.GetPressedKeys();
            float velocidadRotacion;
            float signoRotacion = 1;

            if (velocidad < CERO_UNIVERSAL && !rotoMediaVuelta)
            {
                rotoMediaVuelta = true;
            }

            #region switcht
            if (teclas.Length > 0)
            {
                foreach (Key tecla in teclas)
                {
                    switch (tecla)
                    {
                        case Key.W:

                            velocidad += aceleracion;
                            sentido[(int)Sentido.Adelante] = true;

                            break;
                        case Key.S:

                            velocidad -= aceleracion;
                            sentido[(int)Sentido.Atras] = true;

                            break;
                        case Key.Space:

                            if (!Util.estaDentroDeLaTolerancia(velocidad))
                            {

                                float restar = ACELERACION_DE_FRENADO * elapsedTime;

                                if (velocidad < CERO_UNIVERSAL)
                                { restar *= -1; }

                                velocidad -= restar;

                                sePresionaFreno = true;
                                rotate += (rotacion * 2) * elapsedTime * signoRotacion;
                            }

                            break;
                        case Key.D:

                            if (!Util.estaDentroDeLaTolerancia(velocidad))
                            {
                                velocidadRotacion = Geometry.DegreeToRadian((float)this.getValorDeVariable("velocidad_rotacion"));
                                rotate += velocidadRotacion * elapsedTime;
                                signoRotacion = 1;
                                rotacion += velocidadRotacion;
                                sentido[(int)Sentido.CostadoDer] = true;
                            }

                            break;
                        case Key.A:
                            if (!Util.estaDentroDeLaTolerancia(velocidad))
                            {
                                velocidadRotacion = -1 * Geometry.DegreeToRadian((float)this.getValorDeVariable("velocidad_rotacion"));
                                rotate += velocidadRotacion * elapsedTime;
                                signoRotacion = -1;
                                rotacion += velocidadRotacion * signoRotacion;
                                sentido[(int)Sentido.CostadoIzq] = true;

                            }
                            break;
                    }

                }
            }

            #endregion

            if (rotoMediaVuelta && (teclas.Length == 0 || (teclas[0] == Key.W)))
            {
                rotoMediaVuelta = false;
            }
        }

        private void aplicarMovimiento(bool sePresionaFreno)
        {
            float rotacion = (float)this.getValorDeVariable("ultimaRotacion");

            if (!sePresionaFreno)
            {
                rotacion = vehiculo.mesh.Rotation.Y;
                this.setValorDeVariable("ultimaRotacion", rotacion);
            }
            else
            {
                rotacion = (vehiculo.mesh.Rotation.Y + rotacion) / 2;
                vehiculo.velocidad.actualizarVector((float)Math.PI / 2 - rotacion);
            }

            float x = vehiculo.velocidad.vector.X * -1;
            float z = vehiculo.velocidad.vector.Z * -1;

            vehiculo.mesh.move(x, 0f, z);
        }

        private void aplicarRotacion(float rotar)
        {
            vehiculo.mesh.rotateY(rotar - vehiculo.mesh.Rotation.Y);
        }

        private void rotarBoundingBox()
        {
            Vector3[] rotados = this.dameRotados();

            TgcBoundingBox box = vehiculo.bB.Box;

            vehiculo.bB = new BoundingBox(rotados[0], rotados[1]);
            vehiculo.bB.Box = box;

            vehiculo.bB.scaleTranslate(vehiculo.mesh.Position, new Vector3(1f, 1f, 1f));
            vehiculo.bB.setExtremes(vehiculo.bB.PMin, vehiculo.bB.PMax);
        }

        private Vector3[] dameRotados()
        {
            List<Vector3> points = new List<Vector3>();
            float rotacion = vehiculo.mesh.Rotation.Y;

            double gama = (double)this.getValorDeVariable("beta") - rotacion;

            float xMax = (float)((double)this.getValorDeVariable("hip") * Math.Cos(gama));
            float zMax = (float)((double)this.getValorDeVariable("hip") * Math.Sin(gama));

            double betaPrima = -Math.PI + (double)this.getValorDeVariable("beta");

            gama = betaPrima - rotacion;

            float xMin = (float)((double)this.getValorDeVariable("hip") * Math.Cos(gama));
            float zMin = (float)((double)this.getValorDeVariable("hip") * Math.Sin(gama));

            points.Add(new Vector3(xMin, vehiculo.bB.PMin.Y, zMin));
            points.Add(new Vector3(xMax, vehiculo.bB.PMax.Y, zMax));

            return points.ToArray();
        }

        private void aplicarRozamiento(ref float velocidad, float elapsedTime)
        {
            if (!Util.estaDentroDeLaTolerancia(velocidad))
            {
                float velocidadEnFt = Math.Abs(velocidad) * PIES_POR_METRO;

                float fd = 0.5f * COEFF_DRAG * FRONTAL_AREA * (DENSITY_AIR * LIBRAS_POR_SLUG * KILOGRAMOS_POR_LIBRA) * ((float)Math.Pow(velocidadEnFt, 2.0f));
                fd *= (1 / PIES_POR_METRO);
                float fr = ROLL_RESIST * NEWTONS_POR_LIBRA * velocidadEnFt;
                float frozamiento = COEFICIENTE_FRICCION_DINAMICO * (MASA_AUTO_PROMEDIO * GRAVEDAD);

                float rozamiento = (fd + fr + frozamiento) / MASA_AUTO_PROMEDIO;
                rozamiento *= elapsedTime;
                //rozamiento *= (3600 / 1000); // lo paso a km por hora

                velocidad = ((velocidad > CERO_UNIVERSAL) ? velocidad - rozamiento : (velocidad < CERO_UNIVERSAL) ? velocidad + rozamiento : CERO_UNIVERSAL);
            }
        }

        /// <summary>
        /// Método que se llama cuando termina la ejecución del ejemplo.
        /// Hacer dispose() de todos los objetos creados.
        /// </summary>
        public override void close()
        {
            keyboardDevice.Unacquire();
            keyboardDevice.Dispose();

            piso.dispose();
            vehiculo.mesh.dispose();
            vehiculo.bB.dispose();
        }

        private object getValorDeVariable(string key)
        {
            foreach (KeyValuePair<string, object> clave in variablesDeInterfaz)
            {
                if (clave.Key == key)
                    return clave.Value;
            }

            return null;
        }

        private void setValorDeVariable(string key, object valor)
        {
            KeyValuePair<string, object> keyABorrar = new KeyValuePair<string, object>();

            foreach (KeyValuePair<string, object> clave in variablesDeInterfaz)
            {
                if (clave.Key == key)
                    keyABorrar = clave;
            }

            variablesDeInterfaz.Remove(keyABorrar);

            variablesDeInterfaz.Add(new KeyValuePair<string, object>(key, valor));
        }

        private void calcularFw(float elapsedTime, ref float aceleracion)
        {

            float fw = 0f;

            float[] gear_ratios = new float[] { FIRST_GEAR_RATIO, SECOND_GEAR_RATIO, THIRD_GEAR_RATIO, FOURTH_GEAR_RATIO };

            foreach (float g in gear_ratios)
            {
                fw += ((ENGINE_TORQUE * REAR_END_RATIO * g) * 2) / WHEEL_DIAMETER;
            }

            aceleracion = (NEWTONS_POR_LIBRA * fw) / MASA_AUTO_PROMEDIO;
            aceleracion *= elapsedTime;
            //aceleracion *= (3600 / 1000); // lo paso a km por hora

        }

        private bool[] colisionaConElObjeto(BoundingBox auto, TgcBoundingBox box, float velocidad, ref float anguloInt)
        {
            List<Vector3> corners = this.dameCorners(auto);
            bool[] resultado = new bool[] { false, false, false, false };
            object[] valores = new object[4];

            anguloInt = this.calcularColision(auto, box, anguloInt, corners, resultado, valores);

            this.aplicarDeformacion(corners, resultado, valores);

            return resultado;
        }

        private float calcularColision(BoundingBox auto, TgcBoundingBox box, float anguloInt, List<Vector3> corners, bool[] resultado, object[] valores)
        {
            List<Recta> aristasAuto = new List<Recta>();

            aristasAuto.Add(new Recta(corners[3], corners[1])); //Frente  
            aristasAuto.Add(new Recta(corners[2], corners[0])); //Atras            
            aristasAuto.Add(new Recta(corners[2], corners[1])); //CostadoIzq
            aristasAuto.Add(new Recta(corners[3], corners[0])); //CostadoDer    


            Sentido sentidoActual = Sentido.Adelante;

            List<Recta> aristasObstaculo = new List<Recta>();

            aristasObstaculo.Add(new Recta(box.PMin, new Vector3(box.PMax.X, 0f, box.PMin.Z)));
            aristasObstaculo.Add(new Recta(box.PMin, new Vector3(box.PMin.X, 0f, box.PMax.Z)));
            aristasObstaculo.Add(new Recta(box.PMax, new Vector3(box.PMax.X, 0f, box.PMin.Z)));
            aristasObstaculo.Add(new Recta(box.PMax, new Vector3(box.PMin.X, 0f, box.PMax.Z)));

            anguloInt = (float)Math.PI / 2.0f;

            foreach (Recta aristaAuto in aristasAuto)
            {
                foreach (Recta aristaObst in aristasObstaculo)
                {
                    Vector3 puntoInt = new Vector3(0f, 0f, 0f);
                    bool colisiona = aristaAuto.damePuntoInt(aristaObst, ref puntoInt);

                    colisiona = colisiona && ((auto.PMin.Y >= box.PMin.Y && auto.PMin.Y <= box.PMax.Y) || (auto.PMax.Y >= box.PMin.Y && auto.PMax.Y <= box.PMax.Y));

                    if (colisiona)
                    {
                        valores[(int)sentidoActual] = new object[] { aristaObst, puntoInt };

                        //Sentido sentido = this.aplicarDeformacionDeMalla(corners, sentidoActual, aristaObst, puntoInt);

                        if (sentidoActual == Sentido.CostadoDer || sentidoActual == Sentido.CostadoIzq)
                        {
                            aristaAuto.dameAnguloInt(aristaObst, puntoInt, ref anguloInt);
                        }

                        resultado[(int)sentidoActual] = true;
                    }
                }

                sentidoActual++;
            }
            return anguloInt;
        }

        private void aplicarDeformacion(List<Vector3> corners, bool[] resultado, object[] valores)
        {
            for (Sentido sentidoActual = Sentido.Adelante; (int)sentidoActual < valores.Length; sentidoActual++)
            {
                if (resultado[(int)sentidoActual])
                {
                    this.aplicarDeformacionDeMalla(corners, sentidoActual, (Recta)((object[])valores[(int)sentidoActual])[0], (Vector3)((object[])valores[(int)sentidoActual])[1]);
                }
            }
        }

        private List<Vector3> dameCorners(BoundingBox auto)
        {
            double beta = (double)this.getValorDeVariable("beta");
            double hip = (double)this.getValorDeVariable("hip");
            double rotacion = vehiculo.mesh.Rotation.Y;
            Vector3 position = vehiculo.mesh.Position;
            List<Vector3> corners = new List<Vector3>();

            double gama = -beta - rotacion;

            float xMax = (float)(hip * Math.Cos(gama));
            float zMin = (float)(hip * Math.Sin(gama));

            xMax = xMax + position.X;
            zMin = zMin + position.Z;

            corners.Add(new Vector3(xMax, auto.PMin.Y, zMin));

            gama = Math.PI - beta - rotacion;

            float xMin = (float)(hip * Math.Cos(gama));
            float zMax = (float)(hip * Math.Sin(gama));

            xMin = xMin + position.X;
            zMax = zMax + position.Z;

            corners.Add(new Vector3(xMin, auto.PMin.Y, zMax));

            corners.Add(auto.PMin);
            corners.Add(auto.PMax);

            return corners;
        }

        private Sentido aplicarDeformacionDeMalla(List<Vector3> corners, Sentido sentido, Recta aristaObst, Vector3 puntoInt)
        {
            // se trata la deformaciones de la malla    
            float distancia;
            float X;
            float Z;
            Type tipo = this.vertices.GetType();
            System.Reflection.MethodInfo dameValorPorIndice = tipo.GetMethod("GetValue", new Type[] { typeof(int) });
            System.Reflection.MethodInfo insertaValorPorIndice = tipo.GetMethod("SetValue", new Type[] { typeof(object), typeof(int) });
            int cantidadDeVertices = (int)tipo.GetProperty("Length").GetValue(this.vertices, null);

            float distanciaExtremo1;
            float distanciaExtremo2;
            float largoObstaculo;
            float largoAuto;
            float X1 = float.NaN;
            float Z1 = float.NaN;
            System.Reflection.MethodInfo met = null;

            float energiaCinetica = vehiculo.velocidad.dameMagnitudAbsoluta();

            switch (sentido)
            {
                case Sentido.Atras:

                    distancia = Util.calcularDistanciaEntrePuntos(puntoInt, corners[2]);

                    X = vehiculo.bB.Box.PMin.X + distancia;
                    Z = vehiculo.bB.Box.PMin.Z;

                    for (int j = 0; j < cantidadDeVertices; j++)
                    {
                        object vertice = dameValorPorIndice.Invoke(this.vertices, new object[] { j });

                        Vector3 posicion = (Vector3)vertice.GetType().GetField("Position").GetValue(vertice);

                        if (posicion.X >= X - energiaCinetica * FACTOR_RADIO_IMPACTO && posicion.X <= X + energiaCinetica * FACTOR_RADIO_IMPACTO)
                        {
                            if (posicion.Z <= Z + energiaCinetica * 2 * FACTOR_RADIO_IMPACTO)
                            {
                                posicion.Z += energiaCinetica * FACTOR_DANIO;
                                vertice.GetType().GetField("Position").SetValue(vertice, posicion);
                                insertaValorPorIndice.Invoke(this.vertices, new object[] { vertice, j });
                            }

                        }
                    }

                    break;

                case Sentido.CostadoIzq:

                    distancia = Util.calcularDistanciaEntrePuntos(puntoInt, corners[2]);

                    X = vehiculo.bB.Box.PMin.X;
                    Z = vehiculo.bB.Box.PMin.Z + distancia;

                    if (aristaObst.pendiente == 0f && (new Recta(corners[2], corners[0])).pendiente == 0f)
                    {
                        distanciaExtremo1 = Util.calcularDistanciaEntrePuntos(puntoInt, corners[2]);
                        distanciaExtremo2 = Util.calcularDistanciaEntrePuntos(puntoInt, corners[1]);
                        largoObstaculo = Util.calcularDistanciaEntrePuntos(puntoInt, aristaObst.puntoFinal);
                        largoAuto = Util.calcularDistanciaEntrePuntos(corners[0], corners[2]);
                        X1 = (largoObstaculo > largoAuto) ? (vehiculo.bB.Box.PMin.X + vehiculo.bB.Box.PMax.X) / 2 : vehiculo.bB.Box.PMin.X + (largoObstaculo / 2);
                        Z1 = vehiculo.bB.Box.PMin.Z;

                        if (distanciaExtremo1 < distanciaExtremo2)
                        {
                            met = this.GetType().GetMethod("calcularImpactoExtInf");
                            sentido = Sentido.Atras;
                        }
                        else
                        {
                            Z1 = vehiculo.bB.Box.PMax.Z;
                            met = this.GetType().GetMethod("calcularImpactoExtSup");
                            sentido = Sentido.Adelante;
                        }

                        //sentido = Sentido.Atras;
                    }
                    for (int j = 0; j < cantidadDeVertices; j++)
                    {
                        object vertice = dameValorPorIndice.Invoke(this.vertices, new object[] { j });

                        Vector3 posicion = (Vector3)vertice.GetType().GetField("Position").GetValue(vertice);

                        if (posicion.Z >= Z - energiaCinetica * FACTOR_RADIO_IMPACTO && posicion.Z <= Z + energiaCinetica * FACTOR_RADIO_IMPACTO)
                        {
                            if (posicion.X <= X + energiaCinetica * FACTOR_RADIO_IMPACTO)
                            {
                                posicion.X += energiaCinetica * FACTOR_DANIO;
                                vertice.GetType().GetField("Position").SetValue(vertice, posicion);
                                insertaValorPorIndice.Invoke(this.vertices, new object[] { vertice, j });
                            }
                        }

                        if (met != null)
                            met.Invoke(this, new object[] { insertaValorPorIndice, j, vertice, posicion, X1, Z1, energiaCinetica });
                    }

                    break;

                case Sentido.CostadoDer:

                    distancia = Util.calcularDistanciaEntrePuntos(puntoInt, corners[3]);

                    X = vehiculo.bB.Box.PMax.X;
                    Z = vehiculo.bB.Box.PMax.Z - distancia;

                    if (aristaObst.pendiente == 0f && (new Recta(corners[2], corners[0])).pendiente == 0f)
                    {
                        distanciaExtremo1 = Util.calcularDistanciaEntrePuntos(puntoInt, corners[3]);
                        distanciaExtremo2 = Util.calcularDistanciaEntrePuntos(puntoInt, corners[0]);
                        largoObstaculo = Util.calcularDistanciaEntrePuntos(puntoInt, aristaObst.puntoInicial);
                        largoAuto = Util.calcularDistanciaEntrePuntos(corners[1], corners[3]);
                        X1 = (largoObstaculo > largoAuto) ? (vehiculo.bB.Box.PMin.X + vehiculo.bB.Box.PMax.X) / 2 : vehiculo.bB.Box.PMax.X - (largoObstaculo / 2);
                        Z1 = vehiculo.bB.Box.PMax.Z;

                        if (distanciaExtremo2 < distanciaExtremo1)
                        {
                            Z1 = vehiculo.bB.Box.PMin.Z;
                            met = this.GetType().GetMethod("calcularImpactoExtInf");
                            sentido = Sentido.Atras;
                        }
                        else
                        {
                            met = this.GetType().GetMethod("calcularImpactoExtSup");
                            sentido = Sentido.Adelante;
                        }

                    }

                    for (int j = 0; j < cantidadDeVertices; j++)
                    {
                        object vertice = dameValorPorIndice.Invoke(this.vertices, new object[] { j });

                        Vector3 posicion = (Vector3)vertice.GetType().GetField("Position").GetValue(vertice);

                        if (posicion.Z >= Z - energiaCinetica * FACTOR_RADIO_IMPACTO && posicion.Z <= Z + energiaCinetica * FACTOR_RADIO_IMPACTO)
                        {
                            if (posicion.X >= X - energiaCinetica * FACTOR_RADIO_IMPACTO)
                            {
                                posicion.X -= energiaCinetica * FACTOR_DANIO;
                                vertice.GetType().GetField("Position").SetValue(vertice, posicion);
                                insertaValorPorIndice.Invoke(this.vertices, new object[] { vertice, j });
                            }

                        }

                        if (met != null)
                            met.Invoke(this, new object[] { insertaValorPorIndice, j, vertice, posicion, X1, Z1, energiaCinetica });
                    }

                    break;

                case Sentido.Adelante:

                    distancia = Util.calcularDistanciaEntrePuntos(puntoInt, corners[3]);

                    X = vehiculo.bB.Box.PMax.X - distancia;
                    Z = vehiculo.bB.Box.PMax.Z;

                    for (int j = 0; j < cantidadDeVertices; j++)
                    {
                        object vertice = dameValorPorIndice.Invoke(this.vertices, new object[] { j });

                        Vector3 posicion = (Vector3)vertice.GetType().GetField("Position").GetValue(vertice);

                        if (posicion.X >= X - energiaCinetica * FACTOR_RADIO_IMPACTO && posicion.X <= X + energiaCinetica * FACTOR_RADIO_IMPACTO)
                        {
                            if (posicion.Z >= Z - energiaCinetica * 2 * FACTOR_RADIO_IMPACTO)
                            {
                                posicion.Z -= energiaCinetica * FACTOR_DANIO;
                                vertice.GetType().GetField("Position").SetValue(vertice, posicion);
                                insertaValorPorIndice.Invoke(this.vertices, new object[] { vertice, j });
                            }

                        }
                    }


                    break;
            }

            vehiculo.mesh.D3dMesh.SetVertexBufferData(this.vertices, LockFlags.None);
            return sentido;
        }

        public Vector3 calcularImpactoExtSup(System.Reflection.MethodInfo insertaValorPorIndice, int j, object vertice, Vector3 posicion, float X1, float Z1, float energiaCinetica)
        {
            if (posicion.X >= X1 - energiaCinetica * FACTOR_RADIO_IMPACTO && posicion.X <= X1 + energiaCinetica * FACTOR_RADIO_IMPACTO)
            {
                if (posicion.Z >= Z1 - energiaCinetica * 2 * FACTOR_RADIO_IMPACTO)
                {
                    posicion.Z -= energiaCinetica * FACTOR_DANIO;
                    vertice.GetType().GetField("Position").SetValue(vertice, posicion);
                    insertaValorPorIndice.Invoke(this.vertices, new object[] { vertice, j });
                }

            }
            return posicion;
        }

        public Vector3 calcularImpactoExtInf(System.Reflection.MethodInfo insertaValorPorIndice, int j, object vertice, Vector3 posicion, float X1, float Z1, float energiaCinetica)
        {
            if (posicion.X >= X1 - energiaCinetica * FACTOR_RADIO_IMPACTO && posicion.X <= X1 + energiaCinetica * FACTOR_RADIO_IMPACTO)
            {
                if (posicion.Z <= Z1 + energiaCinetica * 2 * FACTOR_RADIO_IMPACTO)
                {
                    posicion.Z += energiaCinetica * FACTOR_DANIO;
                    vertice.GetType().GetField("Position").SetValue(vertice, posicion);
                    insertaValorPorIndice.Invoke(this.vertices, new object[] { vertice, j });
                }

            }
            return posicion;
        }

    }
}
