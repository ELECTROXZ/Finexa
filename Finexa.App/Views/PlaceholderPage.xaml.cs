using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Finexa_App.Views
{
    public sealed partial class PlaceholderPage : Page
    {
        public PlaceholderPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is string moduleName)
            {
                TitleText.Text = $"{moduleName} Module";
                DescriptionText.Text = $"The {moduleName} features are currently under active development. This core module will be fully integrated and functional in the upcoming release phase.";
            }
        }
    }
}
