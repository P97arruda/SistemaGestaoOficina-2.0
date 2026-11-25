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

            Routing.RegisterRoute(
               nameof(VeiculosPage),
               typeof(VeiculosPage));

            Routing.RegisterRoute(
                nameof(DetalhesVeiculoPage),
                typeof(DetalhesVeiculoPage));
        }
    }
}
