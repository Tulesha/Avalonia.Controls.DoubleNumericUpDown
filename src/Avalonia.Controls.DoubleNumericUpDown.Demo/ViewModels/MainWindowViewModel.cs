using CommunityToolkit.Mvvm.ComponentModel;

namespace Avalonia.Controls.DoubleNumericUpDown.Demo.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private double? _value = 10;
}