using System.Collections.Generic;

namespace AlumnoEjemplos.MiGrupo.Autos
{
    class AutosHome
    {
        private static AutosHome instance = null;
        private List<Auto> autos;

        public static AutosHome getInstance()
        {
            if (instance == null)
            {
                instance = new AutosHome();
            }

            return instance;
        }

        private AutosHome()
        {
            autos = new List<Auto>();
            autos.Add(new AutoMario());
            //autos.Add(new AutoPolicia());
        }

        public void agregar(Auto auto)
        {
            autos.Add(auto);
        }

        public void eliminar(Auto auto)
        {
            autos.Remove(auto);
        }

        public List<Auto> getTodos()
        {
            return autos;
        }
    }
}
