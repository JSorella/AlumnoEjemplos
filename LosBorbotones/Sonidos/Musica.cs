using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using TgcViewer.Utils.Input;
using TgcViewer;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.Sound;

namespace AlumnoEjemplos.LosBorbotones.Sonidos
{
    class Musica 
    {
        #region DLLs externas (para el uso de volumen de Sonido)
        [DllImport("winmm.dll")]
        public static extern int mciSendString(string lpstrCommand,
        StringBuilder lpstrReturnString, int uReturnLengh, int hwndCallback);
        #endregion

        // Constructor
        public Musica(string sTrack)
        {
            cargarMusica( sTrack);
        }

        /// Cargar un nuevo MP3 
        public void cargarMusica(string sArchivo)
        {
         /*   GuiController.Instance.Modifiers.addFile("MP3-File", GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\" + sArchivo, "MP3s|*.mp3");
            //toma path absoluto (creo)
            string filePath = (string)GuiController.Instance.Modifiers["MP3-File"];
            */
            //Cargar archivo
            GuiController.Instance.Mp3Player.closeFile();
            GuiController.Instance.Mp3Player.FileName = GuiController.Instance.AlumnoEjemplosMediaDir + "LosBorbotones\\sonidos\\" + sArchivo;

        }

        //reproduce un tema previamente cargado, sino no pasa nada
        public void playMusica()
        {
            TgcMp3Player player = GuiController.Instance.Mp3Player;
            TgcMp3Player.States currentState = player.getStatus();

            if (currentState == TgcMp3Player.States.Open)
            {
                //Reproducir MP3
                player.play(true);

            }
        }

        public void setVolume(int iVolume)
        {
            mciSendString("setaudio TgcMp3MediaFile  Volume to " + iVolume, null, 0, 0);
        }

        public void muteUnmute()
        {
            TgcMp3Player player = GuiController.Instance.Mp3Player;
            TgcMp3Player.States currentState = player.getStatus();

            if (currentState == TgcMp3Player.States.Playing)
            {
                //Pausar el MP3
                player.pause();
                return;
            }
            else if (currentState == TgcMp3Player.States.Paused)
            {
                //Resumir la ejecución del MP3
                player.resume();
            } 
        }
    }
}
