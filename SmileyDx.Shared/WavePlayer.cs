using System;
using System.Collections.Generic;
using SharpDX.IO;
using SharpDX.Multimedia;
using SharpDX.XAudio2;

namespace SmileyDx {
    public class WavePlayer : IDisposable {
        private static readonly Lazy<WavePlayer> lazy = new Lazy<WavePlayer>(() => new WavePlayer());
        public static WavePlayer Instance { get { return lazy.Value; } }

        private readonly XAudio2 _xAudio;
        readonly Dictionary<string, MyWave> _sounds;

        private WavePlayer() {
            _xAudio = new XAudio2();
            _xAudio.StartEngine();
            var masteringVoice = new MasteringVoice(_xAudio);
            masteringVoice.SetVolume(1);
            _sounds = new Dictionary<string, MyWave>();
        }

        // Reads a sound and puts it in the dictionary
        public void AddWave(string key, string filepath) {
            var wave = new MyWave();

            var nativeFileStream = new NativeFileStream(filepath, NativeFileMode.Open, NativeFileAccess.Read);
            var soundStream = new SoundStream(nativeFileStream);
            var buffer = new AudioBuffer { Stream = soundStream, AudioBytes = (int)soundStream.Length, Flags = BufferFlags.EndOfStream };

            wave.Buffer = buffer;
            wave.DecodedPacketsInfo = soundStream.DecodedPacketsInfo;
            wave.WaveFormat = soundStream.Format;

            _sounds.Add(key, wave);
        }

        // Plays the sound
        public void PlayWave(string key) {
            if (!_sounds.ContainsKey(key)) return;
            var w = _sounds[key];

            var sourceVoice = new SourceVoice(_xAudio, w.WaveFormat);
            sourceVoice.SubmitSourceBuffer(w.Buffer, w.DecodedPacketsInfo);
            sourceVoice.Start();
        }

        public void Dispose() {
            _xAudio.Dispose();
        }
    }

    public class MyWave {
        public AudioBuffer Buffer { get; set; }
        public uint[] DecodedPacketsInfo { get; set; }
        public WaveFormat WaveFormat { get; set; }
    }
}
