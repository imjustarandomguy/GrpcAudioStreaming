using NAudio.Utils;
using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.DataFormats;

namespace GrpcAudioStreaming.Client
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            new Thread(delegate ()
            {
                Application.Run(new CustomApplicationContext());
            }).Start();

            //var samples = JsonConvert.DeserializeObject<List<ModelTest>>(File.ReadAllText(@"C:\Users\random\Downloads\data.json"));

            //var audioPlayer = new AudioPlayer(new WaveFormat(44100, 16, 2));

            //for (int i = 0; i < samples.Count; i++)
            //{
            //    var current = samples[i];

            //    var data = Convert.FromBase64String(current.Data);
            //    audioPlayer.AddSample(data);

            //    var nextSample = samples.Count > i + 1 ? samples[i + 1] : null;
            //    if (nextSample != null)
            //    {
            //        var diff = (int)((DateTime.Parse(nextSample.Timestamp) - DateTime.Parse(current.Timestamp)).TotalMilliseconds);
            //        Thread.Sleep(diff);
            //    }
            //}

            while (true)
            {
                try
                {
                    using var client = new Client();
                    await client.ReceiveAndPlayData();
                }
                catch (Exception)
                {
                    await Task.Delay(60 * 1000);
                }
            }
        }
    }
}