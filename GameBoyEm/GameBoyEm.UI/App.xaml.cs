using log4net.Config;
using System.Windows;

namespace GameBoyEm.UI
{
    public partial class App : Application
    {
        public App()
        {
            XmlConfigurator.Configure();
        }
    }
}
