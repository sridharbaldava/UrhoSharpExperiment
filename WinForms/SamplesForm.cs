using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Urho.Desktop;
using Urho.Extensions.WinForms;

namespace Urho.Samples.WinForms
{
	public partial class SamplesForm : Form
	{
		Application currentApplication;
		SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);
		UrhoSurface surface;

		public SamplesForm()
		{
			InitializeComponent();
			DesktopUrhoInitializer.AssetsDirectory = @"../../Assets";			

			surface = new UrhoSurface();
			surface.Dock = DockStyle.Fill;
			urhoSurfacePlaceholder.Controls.Add(surface);

		}

        private async void LoadModelButton_Click( object sender, EventArgs e )
        {
            var app = await surface.Show(typeof(StaticScene), new ApplicationOptions("Data"));
        }
    }
}
