using AlumnoEjemplos.LosBorbotones.Autos;
using AlumnoEjemplos.LosBorbotones.Niveles;
using TgcViewer;

namespace AlumnoEjemplos.LosBorbotones{

    class Globales
    {
        private static Globales instance = null;

        public static Globales getInstance()
        {
            if (instance == null)
            {
                instance = new Globales();
            }

            return instance;
        }

        private Globales()
        {
            //
        }

        public Auto AutoActual
        {
            get;
            set;
        }

        public Circuito CircuitoActual
        {
            get;
            set;
        }

        public float getVelocidadMaximaAbsolutaEnRealidad()
        {
            return 170;
        }

        public float getVelocidadMaximaAbsolutaEnJuego()
        {
            return getVelocidadMaximaAbsolutaEnRealidad() * getFactorRealidadVelocidad();
        }

        public float getFactorRealidadVelocidad()
        {
            return 10;
        }

        public float getAltoPantalla()
        {
            return GuiController.Instance.Panel3d.Size.Height;
        }

        public float getAnchoPantalla()
        {
            return GuiController.Instance.Panel3d.Size.Width;
        }
    }
}
