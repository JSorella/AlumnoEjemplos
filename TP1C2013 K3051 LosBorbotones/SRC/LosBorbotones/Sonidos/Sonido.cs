using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using TgcViewer.Utils.Sound;

namespace AlumnoEjemplos.LosBorbotones.Sonidos
{
    class Sonido
    {

        private TgcStaticSound sonido;
        public bool mute = false;

        public Sonido(string _path)
        {
            this.sonido = new TgcStaticSound();
            this.sonido.loadSound(_path,0);
        }

        public void play(int _volume)
        {
            //setea volumen
            //Min	-10000	 y Max 0
            if (!mute)
            {
                sonido.SoundBuffer.Volume = _volume;
                sonido.play(false);
            }      
        }
    }
}
