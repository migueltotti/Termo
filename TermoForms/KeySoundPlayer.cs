using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TermoForms;

internal class KeySoundPlayer
{
    private WaveOutEvent waveOut;
    private AudioFileReader audioFile;

    public KeySoundPlayer(string soundFilePath)
    {
        // Carrega o arquivo uma vez na memória
        audioFile = new AudioFileReader(soundFilePath);
    }

    public void PlayKeySound()
    {
        // Cria uma nova instância para permitir sobreposição
        var reader = new AudioFileReader(audioFile.FileName);
        var output = new WaveOutEvent();
        output.Init(reader);
        output.Play();

        // Limpa recursos quando terminar
        output.PlaybackStopped += (s, e) => {
            output.Dispose();
            reader.Dispose();
        };
    }
}