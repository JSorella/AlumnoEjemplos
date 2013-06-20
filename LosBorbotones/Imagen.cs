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

        public void render()
        {
            GuiController.Instance.Drawer2D.beginDrawSprite();
            sprite.render();
            GuiController.Instance.Drawer2D.endDrawSprite();
        }
    }
}
