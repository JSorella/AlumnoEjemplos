using Microsoft.DirectX;
using TgcViewer;
using TgcViewer.Utils._2D;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.LosBorbotones
{
    class Imagen
    {
        //Esta clase se usa para levantar, rotar, escalar y posicionar las imágenes. No la toquen (?
        public TgcSprite sprite;

        public Imagen(string ruta)
        {
            sprite = new TgcSprite();
            sprite.Texture = TgcTexture.createTexture(ruta);
        }

        public void setPosicion(Vector2 posicion)
        {
            sprite.Position = posicion;
        }

        public void setRotacion(float rotacion, Vector2 respecto)
        {
            sprite.RotationCenter = respecto;
            sprite.Rotation = FastMath.ToRad(rotacion);
        }

        public void setEscala(Vector2 escala)
        {
            sprite.Scaling = escala;
        }

        public int getAlto()
        {
            return sprite.Texture.Size.Height;
        }

        public int getAncho()
        {
            return sprite.Texture.Size.Width;
        }
        
        public Vector2 getPosition()
        {
            return sprite.Position;
        }


        /// <summary>
        /// Centra en el medio de la pantalla una imágen, y reescalo si la imágen es más grande que la resolución
        /// </summary>
        public void setCentrarYEscalar ()
        {
            setEscalarMaximo();
            setCentrar();
        
        }

        /// <summary>
        /// Reescalo solo si la imágen es más grande que la resolución
        /// </summary>
        public void setEscalarMaximo()
        {
            if (this.getAlto() > Globales.getInstance().getAltoPantalla())
            {
                float k = Globales.getInstance().getAltoPantalla() / this.getAlto();
                this.setEscala(new Vector2(k, k));
            }
            if (this.getAncho() > Globales.getInstance().getAnchoPantalla())
            {
                float k = Globales.getInstance().getAnchoPantalla() / this.getAncho();
                this.setEscala(new Vector2(k, k));
            }
        }

        /// <summary>
        /// Centro en el medio de la pantalla
        /// </summary>
        public void setCentrar()
        {
            float posX = Globales.getInstance().getAnchoPantalla() * 0.5f - this.getAncho() * 0.5f;
            float posY = Globales.getInstance().getAltoPantalla() * 0.5f - this.getAlto() * 0.5f;
            this.setPosicion(new Vector2(posX, posY));
        }

        /// <summary>
        /// Centra solo el ancho, pero le tenés que pasar donde lo querés de alto
        /// </summary>
        public void setCentrarAncho(float posY)
        {
            float posX = Globales.getInstance().getAnchoPantalla() * 0.5f - (this.getAncho() * 0.22f);
            this.setPosicion(new Vector2(posX, posY));
        }

        public void render()
        {
            GuiController.Instance.Drawer2D.beginDrawSprite();
            sprite.render();
            GuiController.Instance.Drawer2D.endDrawSprite();
        }
    }
}
