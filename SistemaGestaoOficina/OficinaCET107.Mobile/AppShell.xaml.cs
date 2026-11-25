using OficinaCET107.Mobile.Views;

namespace OficinaCET107.Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(
               nameof(DashboardPage),
               typeof(DashboardPage));
        }
    }
}
