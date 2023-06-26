using System;
using System.Windows.Forms;

namespace GrpcAudioStreaming.Client
{
    public class CustomApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon _trayIcon;
        private readonly AudioPlayer _audioPlayer;

        public CustomApplicationContext(AudioPlayer audioPlayer)
        {
            _audioPlayer = audioPlayer;

            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            _trayIcon = new NotifyIcon
            {
                Icon = new System.Drawing.Icon("icon.ico"),
                Visible = true,
            };

            _trayIcon.ContextMenuStrip = new ContextMenuStrip();
            _trayIcon.ContextMenuStrip.Items.Add("Play/Stop", null, TogglePlayStop);
            _trayIcon.ContextMenuStrip.Items.Add("Restart", null, RestartPlayer);
            _trayIcon.ContextMenuStrip.Items.Add("Exit", null, ExitApplication);
        }

        private void ExitApplication(object sender, EventArgs e)
        {
            _trayIcon.Visible = false;
            Environment.Exit(0);
        }

        private void TogglePlayStop(object sender, EventArgs e)
        {
            if (!_audioPlayer.Initilized) return;

            if (_audioPlayer.PlaybackState == NAudio.Wave.PlaybackState.Playing)
            {
                _audioPlayer.Stop();
            }
            else
            {
                _audioPlayer.Play();
            }
        }

        private void RestartPlayer(object sender, EventArgs e)
        {
            if (!_audioPlayer.Initilized) return;

            if (_audioPlayer.PlaybackState == NAudio.Wave.PlaybackState.Playing)
            {
                _audioPlayer.Stop();
            }

            _audioPlayer.Play();
        }

        private void OnProcessExit(object sender, EventArgs e)
        {
            _trayIcon.Dispose();
        }
    }
}