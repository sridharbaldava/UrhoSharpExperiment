using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrhoSharpExperiment
{
    static class Program
    {
        /// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
        static void Main( )
        {
            System.Windows.Forms.Application.EnableVisualStyles( );
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault( false );
            System.Windows.Forms.Application.Run( new SampleForm( ) );
        }
    }
}
