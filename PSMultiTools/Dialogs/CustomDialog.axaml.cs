using Avalonia.Controls;
using Avalonia.Interactivity;

namespace PSMultiTools.Dialogs;

public partial class CustomDialog : Window
{
    public string? TextInputValue { get; set; }

    public CustomDialogResult? CustomDialogResultValue { get; set; }

    public enum CustomDialogResult
    {
        LoadNew,
        Append,
        OK,
        Cancel
    }

    public CustomDialog()
    {
        InitializeComponent();
    }

    private void ButtonsAddButton_Click(object sender, RoutedEventArgs e)
    {
        CustomDialogResultValue = CustomDialogResult.Append;
        Close(CustomDialogResultValue);
    }

    private void ButtonsCancelButton_Click(object sender, RoutedEventArgs e)
    {
        CustomDialogResultValue = CustomDialogResult.Cancel;
        Close(CustomDialogResultValue);
    }

    private void ButtonsLoadNewButton_Click(object sender, RoutedEventArgs e)
    {
        CustomDialogResultValue = CustomDialogResult.LoadNew;
        Close(CustomDialogResultValue);
    }

    private void TextInputCancelButton_Click(object sender, RoutedEventArgs e)
    {
        CustomDialogResultValue = CustomDialogResult.Cancel;
        Close(CustomDialogResultValue);
    }

    private void TextInputOKButton_Click(object sender, RoutedEventArgs e)
    {
        TextInputValue = TextInputTextBox.Text;
        CustomDialogResultValue = CustomDialogResult.OK;
        Close(CustomDialogResultValue);
    }

}