using System;
using System.Windows.Forms;

namespace GrpcAudioStreaming.Client
{
    public class CustomApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon _trayIcon;

        public CustomApplicationContext()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            _trayIcon = new NotifyIcon
            {
                Icon = new System.Drawing.Icon("icon.ico"),
                Visible = true,
            };

            _trayIcon.ContextMenuStrip = new ContextMenuStrip();
            _trayIcon.ContextMenuStrip.Items.Add("Exit", null, ExitApplication);
        }

        private void ExitApplication(object sender, EventArgs e)
        {
            _trayIcon.Visible = false;
            Environment.Exit(1);
        }

        private void OnProcessExit(object sender, EventArgs e)
        {
            _trayIcon.Dispose();
        }
    }
}