// Copyright (c) 2025 tuke productions. All rights reserved.
using NAudio.Wave;
using System;
using System.IO;

namespace SimTuning.Core.Helpers
{
    public static class AudioUtils
    {
        /// <summary>
        /// Erstellt mit übergebenen Stream eine .wav Datei im lokalen Verzeichnis.
        /// </summary>
        /// <param name="fileName">Datei-Name (.wav oder .mp3).</param>
        /// <param name="fileData">Datei-Stream.</param>
        /// <returns>status.</returns>
        public static bool AudioCopy(string fileName, Stream fileData)
        {
            try
            {
                // alte Dyno Audio löschen
                if (File.Exists(SimTuning.Core.GeneralSettings.AudioAccelerationFilePath))
                {
                    File.Delete(SimTuning.Core.GeneralSettings.AudioAccelerationFilePath);
                }

                string localFile = Path.Combine(Data.DatabaseSettings.FileDirectory, fileName);
                MemoryStream memoryStream = new MemoryStream();
                fileData.CopyTo(memoryStream);

                // lokale Temp-Datei erstellen/überschreiben, in der Stream geschrieben
                // wird
                FileStream file = new FileStream(localFile, FileMode.OpenOrCreate, FileAccess.Write);
                memoryStream.WriteTo(file);
                file.Close();

                // in lokales Verzeichnis kopieren
                if (fileName.EndsWith(".mp3"))
                {
                    Mp3ToWav(fileName, SimTuning.Core.GeneralSettings.AudioAccelerationFilePath);
                    File.Delete(localFile);
                }
                else if (fileName.EndsWith(".wav"))
                {
                    File.Move(localFile, SimTuning.Core.GeneralSettings.AudioAccelerationFilePath);
                }
            }
            catch
            {
                return false;
                // MessageBox.Show(e.Message, "Fehler beim kopieren der Audio Datei");
            }

            return true;
        }

        /// <summary>
        /// Konvertiert .mp3 zu .wav.
        /// </summary>
        /// <param name="mp3File">Pfad Der MP3 Datei.</param>
        /// <param name="outputFile">
        /// Pfad wo die .wav Datei gespeichert werden soll.
        /// </param>
        public static void Mp3ToWav(string mp3InputFile, string wavOutputFile)
        {
            try
            {
                using (Mp3FileReader reader = new Mp3FileReader(mp3InputFile))
                {
                    using (WaveStream pcmStream = WaveFormatConversionStream.CreatePcmStream(reader))
                    {
                        WaveFileWriter.CreateWaveFile(wavOutputFile, pcmStream);
                    }
                }
            }
            catch (Exception)
            {
                // MessageBox.Show(e.Message, "Fehler beim konvertieren der Audio Datei");
            }
        }

        /// <summary>
        /// von startpos bis endpos wird die Audio Datei behalten start-und endpos in
        /// bytes.
        /// </summary>
        /// <param name="inPath">The in path.</param>
        /// <param name="outPath">The out path.</param>
        /// <param name="cutFromStart">The cut from start.</param>
        /// <param name="cutFromEnd">The cut from end.</param>
        public static void TrimWavFile(TimeSpan cutFromStart, TimeSpan cutFromEnd, ref Stream outStream, string inPath = null, string outPath = null, Stream inStream = null)
        {
            WaveFileReader reader;
            if (inPath != null) { reader = new WaveFileReader(inPath); }
            else { reader = new WaveFileReader(inStream); }

            WaveFileWriter writer;
            if (outPath != null) { writer = new WaveFileWriter(outPath, reader.WaveFormat); }
            else { writer = new WaveFileWriter(outStream, reader.WaveFormat); }

            int bytesPerMillisecond = reader.WaveFormat.AverageBytesPerSecond / 1000;

            int startPos = (int)cutFromStart.TotalMilliseconds * bytesPerMillisecond;
            startPos = startPos - startPos % reader.WaveFormat.BlockAlign;

            int endBytes = (int)cutFromEnd.TotalMilliseconds * bytesPerMillisecond;
            endBytes = endBytes - endBytes % reader.WaveFormat.BlockAlign;
            int endPos = (int)reader.Length - endBytes;

            TrimWavFile(reader, ref writer, startPos, endPos);
        }

        /// <summary>
        /// Trims the wav file.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="startPos">The start position.</param>
        /// <param name="endPos">The end position.</param>
        private static void TrimWavFile(WaveFileReader reader, ref WaveFileWriter writer, int startPos, int endPos)
        {
            reader.Position = startPos;
            byte[] buffer = new byte[1024];
            while (reader.Position < endPos)
            {
                int bytesRequired = (int)(endPos - reader.Position);
                if (bytesRequired > 0)
                {
                    int bytesToRead = Math.Min(bytesRequired, buffer.Length);
                    int bytesRead = reader.Read(buffer, 0, bytesToRead);
                    if (bytesRead > 0)
                    {
                        writer.Write(buffer, 0, bytesRead);
                    }
                }
            }
        }
    }
}