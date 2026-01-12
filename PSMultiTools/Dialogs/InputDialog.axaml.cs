using Avalonia.Controls;

namespace PSMultiTools.Dialogs;

public partial class InputDialog : Window
{
    public InputDialog()
    {
        InitializeComponent();
    }

    private void ConfirmButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close(NewValueTextBox.Text);
    }

}