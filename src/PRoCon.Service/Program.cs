using System.ServiceProcess;
//using System.Text;

namespace PRoCon.Service
{
    //static class Program
    class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new PRoConService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
