using System.Windows.Controls;
using System.Windows;

namespace ZumaWPF.Views;

public partial class ManualView : UserControl
{
    public event System.Action? Back;
    
    public ManualView()
    {
        InitializeComponent();
    }
    
    private void Back_Click(object sender, RoutedEventArgs e)
    {
        Back?.Invoke();
    }
}

