using System;
using System.Threading;

namespace XAMLEditor
{
    public class Startup
    {
        [STAThread]
        [LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        static void Main()
        {
            var initialDomain = AppDomain.CreateDomain(GetNewDomainName());
            initialDomain.DoCallBack(Action);
        }

        public static string GetNewDomainName() => $"XAMLEditor{++DomainCount}";

        public static CrossAppDomainDelegate Action { get; private set; } = () =>
        {
            Thread thread = new Thread(() =>
            {
                App app = new App();
                app.MainWindow = new MainWindow();
                app.MainWindow.Show();
                app.Run();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        };

        public static int DomainCount { get; private set; }
    }
}