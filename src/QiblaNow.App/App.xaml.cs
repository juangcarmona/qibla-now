namespace QiblaNow.App
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var shell = new AppShell();
            shell.FlowDirection = LocalizationHelper.GetFlowDirection();
            return new Window(shell);
        }
    }
}
