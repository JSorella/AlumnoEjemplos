namespace AlumnoEjemplos.LosBorbotones.Pantallas
{
    public interface Pantalla
    {
        //No sé bien cómo funciona esto de las interfaces, pero así anda bárbaro. Lo saqué de SeniorCoders.
        void render(float elapsedTime);
    }
}