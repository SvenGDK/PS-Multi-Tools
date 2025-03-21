Imports System.IO
Imports System.Windows.Forms

Public Class PBPPacker

#Region "Browse Buttons"

    Private Sub BrowsePBPButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePBPButton.Click
        Dim OFD As New OpenFileDialog() With {.CheckFileExists = True, .Filter = "PBP files (*.PBP)|*.PBP", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedPBPTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowsePARAMButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePARAMButton.Click
        Dim OFD As New OpenFileDialog() With {.CheckFileExists = True, .Filter = "SFO files (*.SFO)|*.SFO", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedPBPTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowseICON0Button_Click(sender As Object, e As RoutedEventArgs) Handles BrowseICON0Button.Click
        Dim OFD As New OpenFileDialog() With {.CheckFileExists = True, .Filter = "PNG files (*.PNG)|*.PNG", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedICON0TextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowseICON1PMFButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseICON1PMFButton.Click
        Dim OFD As New OpenFileDialog() With {.CheckFileExists = True, .Filter = "PMF files (*.PMF)|*.PMF", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedICON1TextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowsePIC0Button_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePIC0Button.Click
        Dim OFD As New OpenFileDialog() With {.CheckFileExists = True, .Filter = "PNG files (*.PNG)|*.PNG", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedPIC0TextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowseSND0Button_Click(sender As Object, e As RoutedEventArgs) Handles BrowseSND0Button.Click
        Dim OFD As New OpenFileDialog() With {.CheckFileExists = True, .Filter = "AT3 files (*.AT3)|*.AT3", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedSND0TextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowseDataPSPButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseDataPSPButton.Click
        Dim OFD As New OpenFileDialog() With {.CheckFileExists = True, .Filter = "PSP files (*.PSP)|*.PSP", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedDataPSPTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowseDATAPSARButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseDATAPSARButton.Click
        Dim OFD As New OpenFileDialog() With {.CheckFileExists = True, .Filter = "PSAR files (*.PSAR)|*.PSAR", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedDataPSARTextBox.Text = OFD.FileName
        End If
    End Sub

#End Region

    Private Sub UnpackPBPButton_Click(sender As Object, e As RoutedEventArgs) Handles UnpackPBPButton.Click

        If Path.GetFileNameWithoutExtension(SelectedPBPTextBox.Text) = "EBOOT" Then
            MsgBox("Please rename EBOOT.PBP to GAME_TITLE.PBP before you continue, this will help to keep you folders organized.", MsgBoxStyle.Information)
            Exit Sub
        End If

        If Not String.IsNullOrEmpty(SelectedPBPTextBox.Text) And File.Exists(SelectedPBPTextBox.Text) Then

            Dim ProcessOutput As String = ""
            'Create a folder based on the FileName of the selected PBP
            Dim NewFolder As String = Path.GetFileNameWithoutExtension(SelectedPBPTextBox.Text)

            Using ISOPBPConverter As New Process()
                ISOPBPConverter.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\pbpunpack.exe"
                ISOPBPConverter.StartInfo.Arguments = """" + SelectedPBPTextBox.Text + """"
                ISOPBPConverter.StartInfo.RedirectStandardOutput = True
                ISOPBPConverter.StartInfo.UseShellExecute = False
                ISOPBPConverter.StartInfo.CreateNoWindow = True
                ISOPBPConverter.Start()

                'Read the output
                Dim OutputReader As StreamReader = ISOPBPConverter.StandardOutput
                ProcessOutput = OutputReader.ReadToEnd()
            End Using

            'Create extraction folder if not exists
            If Not Directory.Exists(Environment.CurrentDirectory + "\Extracted") Then
                Directory.CreateDirectory(Environment.CurrentDirectory + "\Extracted")
                If Not Directory.Exists(Environment.CurrentDirectory + "\Extracted\PBP") Then
                    Directory.CreateDirectory(Environment.CurrentDirectory + "\Extracted\PBP")
                End If
            End If
            Directory.CreateDirectory(Environment.CurrentDirectory + "\Extracted\PBP\" + NewFolder)

            'Move unpacked files
            If File.Exists("PARAM.SFO") Then
                File.Move("PARAM.SFO", Environment.CurrentDirectory + "\Extracted\PBP\" + NewFolder + "\PARAM.SFO")
            End If
            If File.Exists("ICON0.PNG") Then
                File.Move("ICON0.PNG", Environment.CurrentDirectory + "\Extracted\PBP\" + NewFolder + "\ICON0.PNG")
            End If
            If File.Exists("ICON1.PMF") Then
                File.Move("ICON1.PMF", Environment.CurrentDirectory + "\Extracted\PBP\" + NewFolder + "\ICON1.PMF")
            End If
            If File.Exists("PIC0.PNG") Then
                File.Move("PIC0.PNG", Environment.CurrentDirectory + "\Extracted\PBP\" + NewFolder + "\PIC0.PNG")
            End If
            If File.Exists("PIC1.PNG") Then
                File.Move("PIC1.PNG", Environment.CurrentDirectory + "\Extracted\PBP\" + NewFolder + "\PIC1.PNG")
            End If
            If File.Exists("SND0.AT3") Then
                File.Move("SND0.AT3", Environment.CurrentDirectory + "\Extracted\PBP\" + NewFolder + "\SND0.AT3")
            End If
            If File.Exists("DATA.PSP") Then
                File.Move("DATA.PSP", Environment.CurrentDirectory + "\Extracted\PBP\" + NewFolder + "\DATA.PSP")
            End If
            If File.Exists("DATA.PSAR") Then
                File.Move("DATA.PSAR", Environment.CurrentDirectory + "\Extracted\PBP\" + NewFolder + "\DATA.PSAR")
            End If

            If MsgBox("Extracted files :" + vbCrLf + vbCrLf + ProcessOutput + vbCrLf + "Do you want to open the folder with the unpacked PBP ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                Process.Start("explorer", Environment.CurrentDirectory + "\Extracted\PBP\" + NewFolder)
            End If

        End If
    End Sub

    Private Sub PackPBPButton_Click(sender As Object, e As RoutedEventArgs) Handles PackPBPButton.Click
        If Not String.IsNullOrEmpty(SelectedPARAMTextBox.Text) And File.Exists(SelectedPARAMTextBox.Text) And Not String.IsNullOrEmpty(SelectedICON0TextBox.Text) And File.Exists(SelectedICON0TextBox.Text) Then
            If Not String.IsNullOrEmpty(SelectedICON1TextBox.Text) And File.Exists(SelectedICON1TextBox.Text) And Not String.IsNullOrEmpty(SelectedPIC0TextBox.Text) And File.Exists(SelectedPIC0TextBox.Text) Then
                If Not String.IsNullOrEmpty(SelectedSND0TextBox.Text) And File.Exists(SelectedSND0TextBox.Text) And Not String.IsNullOrEmpty(SelectedDataPSPTextBox.Text) And File.Exists(SelectedDataPSPTextBox.Text) Then
                    If Not String.IsNullOrEmpty(SelectedDataPSARTextBox.Text) And File.Exists(SelectedDataPSARTextBox.Text) And Not String.IsNullOrEmpty(NewFileNameTextBox.Text) Then

                        Dim ProcessOutput As String = ""
                        Dim NewFile As String = NewFileNameTextBox.Text

                        If Not NewFileNameTextBox.Text.Contains(".PBP") Then
                            NewFile += ".PBP"
                        End If

                        Using ISOPBPConverter As New Process()
                            ISOPBPConverter.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\pbppack.exe"

                            ISOPBPConverter.StartInfo.Arguments = NewFile + " """ + SelectedPARAMTextBox.Text + """ """ + SelectedICON0TextBox.Text + """ """ +
                                SelectedICON1TextBox.Text + """ """ + SelectedPIC0TextBox.Text + """ """ + SelectedSND0TextBox.Text + """ """ + SelectedDataPSPTextBox.Text + """ """ + SelectedDataPSARTextBox.Text + """"

                            ISOPBPConverter.StartInfo.RedirectStandardOutput = True
                            ISOPBPConverter.StartInfo.UseShellExecute = False
                            ISOPBPConverter.StartInfo.CreateNoWindow = True
                            ISOPBPConverter.Start()

                            'Read the output
                            Dim OutputReader As StreamReader = ISOPBPConverter.StandardOutput
                            ProcessOutput = OutputReader.ReadToEnd()
                        End Using

                        'Create Builds folder if not exists
                        If Not Directory.Exists(Environment.CurrentDirectory + "\Builds") Then
                            Directory.CreateDirectory(Environment.CurrentDirectory + "\Builds")
                            If Not Directory.Exists(Environment.CurrentDirectory + "\Builds\PBP") Then
                                Directory.CreateDirectory(Environment.CurrentDirectory + "\Builds\PBP")
                            End If
                        End If

                        If File.Exists(NewFile) Then
                            File.Move(NewFile, Environment.CurrentDirectory + "\Builds\PBP\" + NewFile)
                            If MsgBox(ProcessOutput + vbCrLf + "Do you want to open the folder with the created PBP file ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                                Process.Start("explorer", Environment.CurrentDirectory + "\Builds\PBP")
                            End If
                        Else
                            MsgBox("Could not find the created PBP, please check the 'Tools' folder.", MsgBoxStyle.Information)
                        End If

                    End If
                End If
            End If
        End If
    End Sub

End Class
