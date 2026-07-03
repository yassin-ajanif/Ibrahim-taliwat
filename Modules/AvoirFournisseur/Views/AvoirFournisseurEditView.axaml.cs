using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GestionCommerciale.Modules.AvoirFournisseur.Views;

public partial class AvoirFournisseurEditView : UserControl
{
    public AvoirFournisseurEditView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
