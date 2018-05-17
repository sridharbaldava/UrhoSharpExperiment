using System.Threading;
using System.Windows.Forms;
using Urho.Desktop;
using Urho.Extensions.WinForms;
using Urho;

namespace UrhoSharpExperiment
{
    public partial class SampleForm : Form
    {
        SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);
        UrhoSurface surface;

        public SampleForm( )
        {
            InitializeComponent( );
            DesktopUrhoInitializer.AssetsDirectory = @"../../../Assets";

            surface = new UrhoSurface( );
            surface.Dock = DockStyle.Fill;
            toolStripContainer1.ContentPanel.Controls.Add( surface );            
        }

        private async void loadToolStripMenuItem_Click( object sender, System.EventArgs e )
        {
            var app = await surface.Show(typeof(StaticScene), new ApplicationOptions("Data"));
        }
    }
}
