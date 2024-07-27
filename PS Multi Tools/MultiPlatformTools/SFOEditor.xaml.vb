Imports System.Windows.Forms

Public Class SFOEditor

    Dim WithEvents LPCM71 As New Controls.CheckBox With {.Content = "7.1 Linear pulse-code modulation (LPCM)", .IsChecked = False}
    Dim WithEvents LPCM51 As New Controls.CheckBox With {.Content = "5.1 Linear pulse-code modulation (LPCM)", .IsChecked = False}
    Dim WithEvents LPCM20 As New Controls.CheckBox With {.Content = "2.0 Linear pulse-code modulation (LPCM)", .IsChecked = False}
    Dim WithEvents DTS As New Controls.CheckBox With {.Content = "DTS", .IsChecked = False}
    Dim WithEvents DolbyDigital As New Controls.CheckBox With {.Content = "Dolby Digital", .IsChecked = False}

    Dim WithEvents FULLHD As New Controls.CheckBox With {.Content = "1080p", .IsChecked = False}
    Dim WithEvents HD As New Controls.CheckBox With {.Content = "720p", .IsChecked = False}
    Dim WithEvents SD169 As New Controls.CheckBox With {.Content = "576p (16:9)", .IsChecked = False}
    Dim WithEvents SD As New Controls.CheckBox With {.Content = "576p", .IsChecked = False}
    Dim WithEvents LOWRES169 As New Controls.CheckBox With {.Content = "480p (16:9)", .IsChecked = False}
    Dim WithEvents LOWRES As New Controls.CheckBox With {.Content = "480p", .IsChecked = False}

    Public CurrentSFO As Param_SFO.PARAM_SFO
    Public CurrentSFOFilePath As String

    Public Structure ParamListViewItem
        Private _ParamName As String
        Private _ParamValue As String

        Public Property ParamName As String
            Get
                Return _ParamName
            End Get
            Set
                _ParamName = Value
            End Set
        End Property

        Public Property ParamValue As String
            Get
                Return _ParamValue
            End Get
            Set
                _ParamValue = Value
            End Set
        End Property
    End Structure

    Private Sub AddAudioFormats()
        AudioFormatsListBox.Items.Add(LPCM71)
        AudioFormatsListBox.Items.Add(LPCM51)
        AudioFormatsListBox.Items.Add(LPCM20)
        AudioFormatsListBox.Items.Add(DTS)
        AudioFormatsListBox.Items.Add(DolbyDigital)
    End Sub

    Private Sub AddVideoFormats()
        VideoFormatsListBox.Items.Add(FULLHD)
        VideoFormatsListBox.Items.Add(HD)
        VideoFormatsListBox.Items.Add(SD169)
        VideoFormatsListBox.Items.Add(SD)
        VideoFormatsListBox.Items.Add(LOWRES169)
        VideoFormatsListBox.Items.Add(LOWRES)
    End Sub

    Private Sub SFOEditor_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        AddAudioFormats()
        AddVideoFormats()
    End Sub

#Region "Help Buttons"

    Private Sub PS3ParamHelpButton_Click(sender As Object, e As RoutedEventArgs) Handles PS3ParamHelpButton.Click
        Process.Start("https://www.psdevwiki.com/ps3/PARAM.SFO")
    End Sub

    Private Sub PSPParamHelpButton_Click(sender As Object, e As RoutedEventArgs) Handles PSPParamHelpButton.Click
        Process.Start("https://www.psdevwiki.com/psp/Param.sfo")
    End Sub

    Private Sub PSVParamHelpButton_Click(sender As Object, e As RoutedEventArgs) Handles PSVParamHelpButton.Click
        Process.Start("https://www.psdevwiki.com/vita/System_File_Object_(SFO)_(PSF)")
    End Sub

    Private Sub PS4ParamHelpButton_Click(sender As Object, e As RoutedEventArgs) Handles PS4ParamHelpButton.Click
        Process.Start("https://www.psdevwiki.com/ps4/Param.sfo")
    End Sub

#End Region

    Private Sub LoadSFOButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadSFOButton.Click
        Dim NewOpenFileDialog As New Forms.OpenFileDialog() With {.Title = "Select a PARAM.SFO file", .Filter = "PARAM.SFO files (*.SFO)|*.SFO"}
        If NewOpenFileDialog.ShowDialog() = Forms.DialogResult.OK Then
            CurrentSFO = New Param_SFO.PARAM_SFO(NewOpenFileDialog.FileName)
            CurrentSFOFilePath = NewOpenFileDialog.FileName

            OpenPARAMSFO(NewOpenFileDialog.FileName)
        End If
    End Sub

    Public Sub OpenPARAMSFO(SFOFile As String)

        'Display the correct tab
        Dim SFOVer As Param_SFO.PARAM_SFO.Playstation = CurrentSFO.PlaystationVersion
        If SFOVer = Param_SFO.PARAM_SFO.Playstation.ps3 Then
            ConsolesTabControl.SelectedItem = PS3TabItem
        ElseIf SFOVer = Param_SFO.PARAM_SFO.Playstation.ps4 Then
            ConsolesTabControl.SelectedItem = PS4TabItem
        ElseIf SFOVer = Param_SFO.PARAM_SFO.Playstation.psp Then
            ConsolesTabControl.SelectedItem = PSPTabItem
        ElseIf SFOVer = Param_SFO.PARAM_SFO.Playstation.psvita Then
            ConsolesTabControl.SelectedItem = PSVTabItem
        End If

        Dim AddedParameters As New List(Of String)()

        For Each TableEntry As Param_SFO.PARAM_SFO.Table In CurrentSFO.Tables.ToList()

#Region "PSV"
            If TableEntry.Name = "PSP2_SYSTEM_VER" Then
                Dim Value As Integer = Convert.ToInt32(TableEntry.Value)
                Dim HexOutput As String = String.Format("{0:X}", Value)

                SystemVersionTextBox.Text = HexOutput
                SystemVersionTextBox.Tag = "PSP2_SYSTEM_VER"
                SystemVersionTextBox.MaxLength = Convert.ToInt32(TableEntry.Indextable.param_data_max_len)

                AddedParameters.Add("PSP2_SYSTEM_VER")
            End If
            If TableEntry.Name = "NP_COMMUNICATION_ID" Then
                VitaSupportGameBootMsgCheckBox.IsChecked = True
                VitaNPComIDTextBox.Text = TableEntry.Value
                VitaNPComIDTextBox.Tag = "NP_COMMUNICATION_ID"
                VitaNPComIDTextBox.MaxLength = Convert.ToInt32(TableEntry.Indextable.param_data_max_len)

                AddedParameters.Add("NP_COMMUNICATION_ID")
            End If
            If TableEntry.Name = "INSTALL_DIR_ADDCONT" Then
                VitaAddtionalContentCheckBox.IsChecked = True

                ShareAppTitleTextBox.Text = TableEntry.Value
                ShareAppTitleTextBox.Tag = "INSTALL_DIR_ADDCONT"
                ShareAppTitleTextBox.MaxLength = Convert.ToInt32(TableEntry.Indextable.param_data_max_len)

                AddedParameters.Add("INSTALL_DIR_ADDCONT")
            End If
            If TableEntry.Name = "INSTALL_DIR_SAVEDATA" Then
                VitaEnableShareSaveCheckBox.IsChecked = True

                VitaShareSaveDataTextBox.Text = TableEntry.Value
                VitaShareSaveDataTextBox.Tag = "INSTALL_DIR_SAVEDATA"
                VitaShareSaveDataTextBox.MaxLength = Convert.ToInt32(TableEntry.Indextable.param_data_max_len)

                AddedParameters.Add("INSTALL_DIR_SAVEDATA")
            End If
            If TableEntry.Name = "SAVEDATA_MAX_SIZE" Then
                VitaSaveDataQuotaTextBox.Text = TableEntry.Value
                VitaSaveDataQuotaTextBox.Tag = "SAVEDATA_MAX_SIZE"
                VitaSaveDataQuotaTextBox.MaxLength = Convert.ToInt32(TableEntry.Indextable.param_data_max_len)

                AddedParameters.Add("SAVEDATA_MAX_SIZE")
            End If
            If TableEntry.Name = "STITLE" Then
                AppShotTitleTextBox.Text = TableEntry.Value
                AppShotTitleTextBox.Tag = "STITLE"
                AppShotTitleTextBox.MaxLength = Convert.ToInt32(TableEntry.Indextable.param_data_max_len)

                AddedParameters.Add("STITLE")
            End If
#End Region

#Region "PS3"
            If TableEntry.Name = "SOUND_FORMAT" Then
                Dim Value As Integer = Convert.ToInt32(TableEntry.Value)

                Select Case Value
                    Case 1
                        LPCM20.IsChecked = True
                    Case 4
                        LPCM51.IsChecked = True
                    Case 5
                        LPCM20.IsChecked = True
                        LPCM51.IsChecked = True
                    Case 16
                        LPCM71.IsChecked = True
                    Case 21
                        LPCM20.IsChecked = True
                        LPCM51.IsChecked = True
                        LPCM71.IsChecked = True
                    Case 258
                        DolbyDigital.IsChecked = True
                    Case 279
                        LPCM20.IsChecked = True
                        LPCM51.IsChecked = True
                        LPCM71.IsChecked = True
                        DolbyDigital.IsChecked = True
                    Case 514
                        DTS.IsChecked = True
                    Case 772
                        DolbyDigital.IsChecked = True
                        DTS.IsChecked = True
                    Case 791
                        LPCM20.IsChecked = True
                        LPCM51.IsChecked = True
                        LPCM71.IsChecked = True
                        DolbyDigital.IsChecked = True
                        DTS.IsChecked = True
                    Case 793
                        LPCM20.IsChecked = True
                        LPCM51.IsChecked = True
                        LPCM71.IsChecked = True
                        DolbyDigital.IsChecked = True
                        DTS.IsChecked = True
                End Select

                AddedParameters.Add("SOUND_FORMAT")
            End If
            If TableEntry.Name = "RESOLUTION" Then
                Dim Value As Integer = Convert.ToInt32(TableEntry.Value)

                Select Case Value
                    Case 1
                        LOWRES.IsChecked = True
                    Case 2
                        SD.IsChecked = True
                    Case 3
                        LOWRES.IsChecked = True
                        SD.IsChecked = True
                    Case 4
                        HD.IsChecked = True
                    Case 5
                        HD.IsChecked = True
                        LOWRES.IsChecked = True
                    Case 6
                        HD.IsChecked = True
                        SD.IsChecked = True
                    Case 7
                        HD.IsChecked = True
                        SD.IsChecked = True
                        LOWRES.IsChecked = True
                    Case 8
                        FULLHD.IsChecked = True
                    Case 9
                        FULLHD.IsChecked = True
                        LOWRES.IsChecked = True
                    Case 10
                        FULLHD.IsChecked = True
                        SD.IsChecked = True
                    Case 11
                        FULLHD.IsChecked = True
                        SD.IsChecked = True
                        LOWRES.IsChecked = True
                    Case 12
                        FULLHD.IsChecked = True
                        HD.IsChecked = True
                    Case 13
                        FULLHD.IsChecked = True
                        HD.IsChecked = True
                        LOWRES.IsChecked = True
                    Case 14
                        FULLHD.IsChecked = True
                        HD.IsChecked = True
                        SD.IsChecked = True
                    Case 15
                        FULLHD.IsChecked = True
                        HD.IsChecked = True
                        SD.IsChecked = True
                        LOWRES.IsChecked = True
                    Case 16
                        LOWRES169.IsChecked = True
                    Case 17
                        LOWRES169.IsChecked = True
                        LOWRES.IsChecked = True
                    Case 18
                        LOWRES169.IsChecked = True
                        SD.IsChecked = True
                    Case 19
                        LOWRES.IsChecked = True
                        LOWRES169.IsChecked = True
                        SD.IsChecked = True
                    Case 20
                        LOWRES169.IsChecked = True
                        HD.IsChecked = True
                    Case 21
                        LOWRES.IsChecked = True
                        LOWRES169.IsChecked = True
                        HD.IsChecked = True
                    Case 22
                        LOWRES169.IsChecked = True
                        SD.IsChecked = True
                        HD.IsChecked = True
                    Case 23
                        LOWRES169.IsChecked = True
                        LOWRES.IsChecked = True
                        SD.IsChecked = True
                        HD.IsChecked = True
                    Case 24
                        FULLHD.IsChecked = True
                        LOWRES169.IsChecked = True
                    Case 25
                        FULLHD.IsChecked = True
                        LOWRES.IsChecked = True
                        LOWRES169.IsChecked = True
                    Case 26
                        FULLHD.IsChecked = True
                        SD.IsChecked = True
                        LOWRES169.IsChecked = True
                    Case 27
                        FULLHD.IsChecked = True
                        SD.IsChecked = True
                        LOWRES.IsChecked = True
                        LOWRES169.IsChecked = True
                    Case 28
                        FULLHD.IsChecked = True
                        HD.IsChecked = True
                        LOWRES169.IsChecked = True
                    Case 29
                        FULLHD.IsChecked = True
                        HD.IsChecked = True
                        LOWRES.IsChecked = True
                        LOWRES169.IsChecked = True
                    Case 30
                        FULLHD.IsChecked = True
                        HD.IsChecked = True
                        SD.IsChecked = True
                        LOWRES169.IsChecked = True
                    Case 31
                        FULLHD.IsChecked = True
                        HD.IsChecked = True
                        LOWRES.IsChecked = True
                        LOWRES169.IsChecked = True
                        SD.IsChecked = True
                    Case 32
                        SD169.IsChecked = True
                    Case 33
                        SD169.IsChecked = True
                        LOWRES.IsChecked = True
                    Case 34
                        SD169.IsChecked = True
                        SD.IsChecked = True
                    Case 35
                        SD169.IsChecked = True
                        SD.IsChecked = True
                        LOWRES.IsChecked = True
                    Case 36
                        SD169.IsChecked = True
                        HD.IsChecked = True
                    Case 37
                        HD.IsChecked = True
                        SD169.IsChecked = True
                        LOWRES.IsChecked = True
                    Case 38
                        HD.IsChecked = True
                        SD.IsChecked = True
                        SD169.IsChecked = True
                    Case 39
                        HD.IsChecked = True
                        SD169.IsChecked = True
                        SD.IsChecked = True
                        LOWRES.IsChecked = True
                    Case 40
                        SD169.IsChecked = True
                        FULLHD.IsChecked = True
                    Case 41
                        FULLHD.IsChecked = True
                        SD169.IsChecked = True
                        LOWRES.IsChecked = True
                    Case 42
                        FULLHD.IsChecked = True
                        SD169.IsChecked = True
                        SD.IsChecked = True
                    Case 43
                        FULLHD.IsChecked = True
                        SD169.IsChecked = True
                        SD.IsChecked = True
                        LOWRES.IsChecked = True
                    Case 44
                        FULLHD.IsChecked = True
                        SD169.IsChecked = True
                        HD.IsChecked = True
                    Case 45
                        FULLHD.IsChecked = True
                        SD169.IsChecked = True
                        HD.IsChecked = True
                        LOWRES.IsChecked = True
                    Case 46
                        FULLHD.IsChecked = True
                        SD169.IsChecked = True
                        HD.IsChecked = True
                        SD.IsChecked = True
                    Case 47
                        FULLHD.IsChecked = True
                        SD169.IsChecked = True
                        HD.IsChecked = True
                        SD.IsChecked = True
                        LOWRES.IsChecked = True
                    Case 48
                        LOWRES169.IsChecked = True
                        SD169.IsChecked = True
                    Case 49
                        LOWRES169.IsChecked = True
                        SD169.IsChecked = True
                        LOWRES.IsChecked = True
                    Case 50
                        LOWRES169.IsChecked = True
                        SD169.IsChecked = True
                        SD.IsChecked = True
                    Case 51
                        LOWRES169.IsChecked = True
                        SD169.IsChecked = True
                        SD.IsChecked = True
                        LOWRES.IsChecked = True
                    Case 52
                        LOWRES169.IsChecked = True
                        SD169.IsChecked = True
                        HD.IsChecked = True
                    Case 53
                        LOWRES169.IsChecked = True
                        SD169.IsChecked = True
                        HD.IsChecked = True
                        LOWRES.IsChecked = True
                    Case 54
                        LOWRES169.IsChecked = True
                        SD169.IsChecked = True
                        HD.IsChecked = True
                        SD.IsChecked = True
                    Case 55
                        LOWRES169.IsChecked = True
                        SD169.IsChecked = True
                        HD.IsChecked = True
                        SD.IsChecked = True
                        LOWRES.IsChecked = True
                    Case 56
                        LOWRES169.IsChecked = True
                        SD169.IsChecked = True
                        FULLHD.IsChecked = True
                    Case 57
                        LOWRES169.IsChecked = True
                        SD169.IsChecked = True
                        FULLHD.IsChecked = True
                        LOWRES.IsChecked = True
                    Case 58
                        LOWRES169.IsChecked = True
                        SD169.IsChecked = True
                        FULLHD.IsChecked = True
                        SD.IsChecked = True
                    Case 59
                        LOWRES169.IsChecked = True
                        SD169.IsChecked = True
                        FULLHD.IsChecked = True
                        SD.IsChecked = True
                        LOWRES.IsChecked = True
                    Case 60
                        LOWRES169.IsChecked = True
                        SD169.IsChecked = True
                        FULLHD.IsChecked = True
                        HD.IsChecked = True
                    Case 61
                        LOWRES169.IsChecked = True
                        SD169.IsChecked = True
                        FULLHD.IsChecked = True
                        HD.IsChecked = True
                        LOWRES.IsChecked = True
                    Case 62
                        LOWRES169.IsChecked = True
                        SD169.IsChecked = True
                        FULLHD.IsChecked = True
                        HD.IsChecked = True
                        SD.IsChecked = True
                    Case 63
                        LOWRES169.IsChecked = True
                        SD169.IsChecked = True
                        FULLHD.IsChecked = True
                        HD.IsChecked = True
                        SD.IsChecked = True
                        LOWRES.IsChecked = True
                    Case Else
                End Select

                AddedParameters.Add("RESOLUTION")
            End If
            If TableEntry.Name = "PS3_SYSTEM_VER" Then
                SystemVersionTextBox.Tag = "PS3_SYSTEM_VER"
                SystemVersionTextBox.Text = TableEntry.Value
                SystemVersionTextBox.MaxLength = Convert.ToInt32(TableEntry.Indextable.param_data_max_len)

                AddedParameters.Add("PS3_SYSTEM_VER")
            End If
#End Region

#Region "General parameters"
            If TableEntry.Name = "TITLE_ID" Then
                IDTextBox.Text = TableEntry.Value.Trim()
                IDTextBox.Tag = "TITLE_ID"
                IDTextBox.MaxLength = Convert.ToInt32(TableEntry.Indextable.param_data_max_len)

                AddedParameters.Add("TITLE_ID")
            End If
            If TableEntry.Name = "ATTRIBUTE" Then
                Dim Value As Integer = Convert.ToInt32(TableEntry.Value)
                Dim HexOutput As String = String.Format("{0:X}", Value)

                If SFOVer = Param_SFO.PARAM_SFO.Playstation.ps4 Then
                    Dim SplittedHex As IEnumerable(Of String) = Enumerable.Range(0, HexOutput.Length \ 2).[Select](Function(i) HexOutput.Substring(i * 2, 2))
                    'Not sure about this, cannot determine how the Attribute parameter is setup on PS4
                    If SplittedHex.Count <= 3 Then
                        Select Case SplittedHex(0)
                            Case "01"

                            Case "02"

                            Case "04"
                                PS4AppRequiresVRCheckBox.IsChecked = True
                            Case "08"

                            Case "10"

                            Case "20"
                                PS4AppSupportsHDRCheckBox.IsChecked = True
                            Case "40"

                            Case "80"
                                'Display location
                        End Select

                        Select Case SplittedHex(1)
                            Case "01"
                                PS4CPUMode7CheckBox.IsChecked = True
                            Case "02"

                            Case "04"

                            Case "08"

                            Case "10"

                            Case "20"

                            Case "40"

                            Case "80"
                                PS4SupportsNEOModeCheckBox.IsChecked = True
                        End Select

                        Select Case SplittedHex(2)
                            Case "01"
                                PS4SuspendAppOnSpecResAndPSButtonCheckBox.IsChecked = True
                            Case "02"
                                PS4HDCPEnabledCheckBox.IsChecked = True
                            Case "04"
                                PS4HDCPDisabledForNonGamesAppCheckBox.IsChecked = True
                            Case "08"

                            Case "10"

                            Case "20"

                            Case "40"
                                PS4AppSupportsVRCheckBox.IsChecked = True
                            Case "80"
                                PS4CPUMode6CheckBox.IsChecked = True
                        End Select

                        Select Case SplittedHex(3)
                            Case "01"
                                PS4InitLogoutCheckBox.IsChecked = True
                            Case "02"
                                PS4ButtonAssignmentCrossButtonCheckBox.IsChecked = True
                            Case "04"
                                PS4PSMoveWarningDialogCheckBox.IsChecked = True
                            Case "08"
                                PS43DSupportCheckBox.IsChecked = True
                            Case "10"
                                PS4SuspendOnPSButtonCheckBox.IsChecked = True
                            Case "20"
                                PS4ButtonAssigmentSystemSoftwareCheckBox.IsChecked = True
                            Case "40"
                                PS4AppOverwritesShareMenuCheckBox.IsChecked = True
                            Case "80"
                        End Select
                    End If
                ElseIf SFOVer = Param_SFO.PARAM_SFO.Playstation.psvita Then
                    Select Case Value
                        Case 2
                            VitaUseLibLocationCheckBox.IsChecked = True
                        Case 128
                            VitaDisplayInfoBarCheckBox.IsChecked = True
                        Case 130
                            VitaUseLibLocationCheckBox.IsChecked = True
                            VitaDisplayInfoBarCheckBox.IsChecked = True
                        Case 256
                            VitaColorInfoBarCheckBox.IsChecked = True
                        Case 258
                            VitaUseLibLocationCheckBox.IsChecked = True
                            VitaColorInfoBarCheckBox.IsChecked = True
                        Case 384
                            VitaDisplayInfoBarCheckBox.IsChecked = True
                            VitaColorInfoBarCheckBox.IsChecked = True
                        Case 386
                            VitaUseLibLocationCheckBox.IsChecked = True
                            VitaDisplayInfoBarCheckBox.IsChecked = True
                            VitaColorInfoBarCheckBox.IsChecked = True
                        Case 1024
                            VitaAppIsUpgradedableCheckBox.IsChecked = True
                        Case 1026
                            VitaUseLibLocationCheckBox.IsChecked = True
                            VitaColorInfoBarCheckBox.IsChecked = True
                        Case 2097152
                            VitaAddHealthInfoCheckBox.IsChecked = True
                        Case 33554432
                            VitaUseTWDialogCheckBox.IsChecked = True
                        Case 35652992
                            VitaUseLibLocationCheckBox.IsChecked = True
                            VitaDisplayInfoBarCheckBox.IsChecked = True
                            VitaColorInfoBarCheckBox.IsChecked = True
                            VitaAddHealthInfoCheckBox.IsChecked = True
                            VitaUseTWDialogCheckBox.IsChecked = True
                        Case 35652994
                            VitaUseLibLocationCheckBox.IsChecked = True
                            VitaDisplayInfoBarCheckBox.IsChecked = True
                            VitaColorInfoBarCheckBox.IsChecked = True
                            VitaAddHealthInfoCheckBox.IsChecked = True
                            VitaUseTWDialogCheckBox.IsChecked = True
                    End Select
                ElseIf SFOVer = Param_SFO.PARAM_SFO.Playstation.ps3 Then

                End If

                AddedParameters.Add("ATTRIBUTE")
            End If
            If TableEntry.Name = "BOOTABLE" Then
                If SFOVer = Param_SFO.PARAM_SFO.Playstation.ps3 Then
                    If TableEntry.Value = "0" Then
                        PS3BootableCheckBox.IsChecked = False
                    ElseIf TableEntry.Value = "1" Then
                        PS3BootableCheckBox.IsChecked = True
                        PS3BootableCheckBox.Content = "Bootable (Mode 1)"
                    ElseIf TableEntry.Value = "2" Then
                        PS3BootableCheckBox.IsChecked = True
                        PS3BootableCheckBox.Content = "Bootable (Mode 2)"
                    Else
                        PS3BootableCheckBox.IsChecked = True
                    End If
                ElseIf SFOVer = Param_SFO.PARAM_SFO.Playstation.psp Then
                    If TableEntry.Value = "0" Then
                        PSPBootableCheckBox.IsChecked = False
                    ElseIf TableEntry.Value = "1" Then
                        PSPBootableCheckBox.IsChecked = True
                        PSPBootableCheckBox.Content = "Bootable (Mode 1)"
                    ElseIf TableEntry.Value = "2" Then
                        PSPBootableCheckBox.IsChecked = True
                        PSPBootableCheckBox.Content = "Bootable (Mode 2)"
                    Else
                        PSPBootableCheckBox.IsChecked = True
                    End If
                End If
            End If
            If TableEntry.Name = "CONTENT_ID" Then
                ContentIDTextBox.Text = TableEntry.Value.Trim()
                ContentIDTextBox.Tag = "CONTENT_ID"
                ContentIDTextBox.MaxLength = Convert.ToInt32(TableEntry.Indextable.param_data_max_len)

                AddedParameters.Add("CONTENT_ID")
            End If
            If TableEntry.Name = "TITLE" Then
                TitleTextBox.Text = TableEntry.Value.Trim()
                TitleTextBox.Tag = "TITLE"
                TitleTextBox.MaxLength = Convert.ToInt32(TableEntry.Indextable.param_data_max_len)

                AddedParameters.Add("TITLE")
            End If
            If TableEntry.Name = "CATEGORY" Then
                CategoryTextBox.Text = TableEntry.Value.Trim()
                CategoryTextBox.Tag = "CATEGORY"
                CategoryTextBox.MaxLength = Convert.ToInt32(TableEntry.Indextable.param_data_max_len)

                AddedParameters.Add("CATEGORY")
            End If
            If TableEntry.Name = "APP_VER" Then
                AppVerTextBox.Text = TableEntry.Value.Trim()
                AppVerTextBox.Tag = "APP_VER"
                AppVerTextBox.MaxLength = Convert.ToInt32(TableEntry.Indextable.param_data_max_len)

                AddedParameters.Add("APP_VER")
            End If
            If TableEntry.Name = "VERSION" Then
                VerTextBox.Text = TableEntry.Value.Trim()
                VerTextBox.Tag = "VERSION"
                VerTextBox.MaxLength = Convert.ToInt32(TableEntry.Indextable.param_data_max_len)

                AddedParameters.Add("VERSION")
            End If
            If TableEntry.Name = "PARENTAL_LEVEL" Then
                If Not String.IsNullOrEmpty(TableEntry.Value) Then
                    ParentalComboBox.Tag = "PARENTAL_LEVEL"

                    Select Case TableEntry.Value
                        Case "0"
                            ParentalComboBox.SelectedIndex = 0
                        Case "1"
                            ParentalComboBox.SelectedIndex = 1
                        Case "2"
                            ParentalComboBox.SelectedIndex = 2
                        Case "3"
                            ParentalComboBox.SelectedIndex = 3
                        Case "4"
                            ParentalComboBox.SelectedIndex = 4
                        Case "5"
                            ParentalComboBox.SelectedIndex = 5
                        Case "6"
                            ParentalComboBox.SelectedIndex = 6
                        Case "7"
                            ParentalComboBox.SelectedIndex = 7
                        Case "8"
                            ParentalComboBox.SelectedIndex = 8
                        Case "9"
                            ParentalComboBox.SelectedIndex = 9
                        Case "10"
                            ParentalComboBox.SelectedIndex = 10
                        Case "11"
                            ParentalComboBox.SelectedIndex = 11
                        Case Else
                            ParentalComboBox.SelectedIndex = 0
                    End Select

                    AddedParameters.Add("PARENTAL_LEVEL")
                Else
                    ParentalComboBox.SelectedIndex = 0
                End If
            End If
#End Region

#Region "PS4"
            If TableEntry.Name = "APP_TYPE" Then
                PS4AppTypeComboBox.Tag = "APP_TYPE"

                Select Case TableEntry.Value
                    Case "0"
                        PS4AppTypeComboBox.SelectedIndex = 0
                    Case "1"
                        PS4AppTypeComboBox.SelectedIndex = 1
                    Case "2"
                        PS4AppTypeComboBox.SelectedIndex = 2
                    Case "3"
                        PS4AppTypeComboBox.SelectedIndex = 3
                    Case "4"
                        PS4AppTypeComboBox.SelectedIndex = 4
                End Select

                AddedParameters.Add("APP_TYPE")
            End If
            If TableEntry.Name = "PUBTOOLINFO" Then
                PS4PubToolInfoTextBox.Text = TableEntry.Value
                PS4PubToolInfoTextBox.Tag = "PUBTOOLINFO"

                AddedParameters.Add("PUBTOOLINFO")
            End If
            If TableEntry.Name = "PUBTOOLVER" Then
                Dim value As Integer = Convert.ToInt32(TableEntry.Value)
                Dim hexOutput As String = String.Format("{0:X}", value)

                PS4PubToolVersionTextBox.Text = hexOutput
                PS4PubToolVersionTextBox.Tag = "PUBTOOLVER"

                AddedParameters.Add("PUBTOOLVER")
            End If
            If TableEntry.Name = "SYSTEM_VER" Then
                Dim Value As Integer = Convert.ToInt32(TableEntry.Value)
                Dim HexOutput As String = String.Format("{0:X}", Value)

                SystemVersionTextBox.Text = HexOutput
                SystemVersionTextBox.Tag = "SYSTEM_VER"

                AddedParameters.Add("SYSTEM_VER")
            End If
#End Region

#Region "PSP"
            If TableEntry.Name = "PSP_SYSTEM_VER" Then
                SystemVersionTextBox.Tag = "PSP_SYSTEM_VER"
                SystemVersionTextBox.Text = TableEntry.Value
                SystemVersionTextBox.MaxLength = Convert.ToInt32(TableEntry.Indextable.param_data_max_len)

                AddedParameters.Add("PSP_SYSTEM_VER")
            End If
            If TableEntry.Name = "DISC_VERSION" Then
                VerTextBox.Text = TableEntry.Value.Trim()
                VerTextBox.Tag = "DISC_VERSION"
                VerTextBox.MaxLength = Convert.ToInt32(TableEntry.Indextable.param_data_max_len)

                AddedParameters.Add("DISC_VERSION")
            End If
            If TableEntry.Name = "DISC_ID" Then
                IDTextBox.Text = TableEntry.Value.Trim()
                IDTextBox.Tag = "DISC_ID"
                IDTextBox.MaxLength = Convert.ToInt32(TableEntry.Indextable.param_data_max_len)

                AddedParameters.Add("DISC_ID")
            End If
#End Region

        Next

    End Sub

    Public Function AddNewParam(Index As Integer, Name As String, Value As String, Format As Param_SFO.PARAM_SFO.FMT, Lenght As Integer, MaxLength As Integer, ParamTable As List(Of Param_SFO.PARAM_SFO.Table)) As List(Of Param_SFO.PARAM_SFO.Table)
        Dim IndexTable As New Param_SFO.PARAM_SFO.index_table With {.param_data_fmt = Format, .param_data_len = Convert.ToUInt32(Lenght), .param_data_max_len = Convert.ToUInt32(MaxLength)}
        Dim TableItem As New Param_SFO.PARAM_SFO.Table With {.index = Index, .Indextable = IndexTable, .Name = Name, .Value = Value}
        ParamTable.Add(TableItem)
        Return ParamTable
    End Function

    Private Sub NewSFOButton_Click(sender As Object, e As RoutedEventArgs) Handles NewSFOButton.Click
        Dim NewSFOType As String = ""

        If NewSFOType = "PS4" Then
            Dim ParamTables As New List(Of Param_SFO.PARAM_SFO.Table)()
            Dim NewItemIndex As Integer = 0

            NewItemIndex += 1
            AddNewParam(NewItemIndex, "APP_TYPE", "0", Param_SFO.PARAM_SFO.FMT.UINT32, 4, 4, ParamTables)
            NewItemIndex += 1
            AddNewParam(NewItemIndex, "ATTRIBUTE", "0", Param_SFO.PARAM_SFO.FMT.UINT32, 4, 4, ParamTables)
            NewItemIndex += 1
            AddNewParam(NewItemIndex, "APP_VER", "01.00", Param_SFO.PARAM_SFO.FMT.Utf8Null, 5, 8, ParamTables)
            NewItemIndex += 1
            AddNewParam(NewItemIndex, "CATEGORY", "gd", Param_SFO.PARAM_SFO.FMT.Utf8Null, 3, 4, ParamTables)
            NewItemIndex += 1
            AddNewParam(NewItemIndex, "CONTENT_ID", "XXYYYY-XXXXYYYYY_00-ZZZZZZZZZZZZZZZZ", Param_SFO.PARAM_SFO.FMT.Utf8Null, 37, 48, ParamTables)
            NewItemIndex += 1
            AddNewParam(NewItemIndex, "DOWNLOAD_DATA_SIZE", "0", Param_SFO.PARAM_SFO.FMT.UINT32, 4, 4, ParamTables)
            NewItemIndex += 1
            AddNewParam(NewItemIndex, "FORMAT", "obs", Param_SFO.PARAM_SFO.FMT.Utf8Null, 4, 4, ParamTables)
            NewItemIndex += 1
            AddNewParam(NewItemIndex, "PARENTAL_LEVEL", "1", Param_SFO.PARAM_SFO.FMT.UINT32, 4, 4, ParamTables)
            NewItemIndex += 1
            AddNewParam(NewItemIndex, "REMOTE_PLAY_KEY_ASSIGN", "1", Param_SFO.PARAM_SFO.FMT.UINT32, 4, 4, ParamTables)
            NewItemIndex += 1
            For i As Integer = 1 To 8 - 1
                AddNewParam(NewItemIndex, "SERVICE_ID_ADDCONT_ADD_" + i.ToString, "", Param_SFO.PARAM_SFO.FMT.Utf8Null, 1, 20, ParamTables)
            Next
            NewItemIndex += 1
            AddNewParam(NewItemIndex, "SYSTEM_VER", "0", Param_SFO.PARAM_SFO.FMT.UINT32, 4, 4, ParamTables)
            NewItemIndex += 1
            AddNewParam(NewItemIndex, "TITLE", "GameTitle ID", Param_SFO.PARAM_SFO.FMT.Utf8Null, 19, 128, ParamTables)
            NewItemIndex += 1
            AddNewParam(NewItemIndex, "TITLE_ID", "XXXXYYYYY", Param_SFO.PARAM_SFO.FMT.Utf8Null, 10, 12, ParamTables)
            NewItemIndex += 1
            AddNewParam(NewItemIndex, "VERSION", "01.00", Param_SFO.PARAM_SFO.FMT.Utf8Null, 6, 8, ParamTables)

            CurrentSFO = New Param_SFO.PARAM_SFO()
            Param_SFO.PARAM_SFO.Header.IndexTableEntries = Convert.ToUInt32(NewItemIndex)
            CurrentSFO.Tables = ParamTables
        End If

        'OpenPARAMSFO(CurrentSFO)
    End Sub

    Private Sub SaveSFOButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveSFOButton.Click
        Dim SFD As New SaveFileDialog With {.Filter = "PARAM.SFO (PARAM.SFO)|PARAM.SFO", .DefaultExt = "SFO", .AddExtension = True}
        If SFD.ShowDialog() = Forms.DialogResult.OK Then

            'Debug
            For Each tableval As Param_SFO.PARAM_SFO.Table In CurrentSFO.Tables()
                MsgBox("Table Name: " + tableval.Name + vbCrLf +
                       "Value: " + tableval.Value + vbCrLf +
                       "Param Lenght: " + tableval.Indextable.param_data_len.ToString() + vbCrLf +
                       "Param Max Lenght: " + tableval.Indextable.param_data_max_len.ToString())
            Next

            CurrentSFO.SaveSFO(CurrentSFO, SFD.FileName)
        End If
    End Sub

#Region "General Value Changes"

    Private Sub IDTextBox_TextChanged(sender As Object, e As Controls.TextChangedEventArgs) Handles IDTextBox.TextChanged
        If Not String.IsNullOrEmpty(IDTextBox.Text) Then
            For i As Integer = 0 To CurrentSFO.Tables.Count - 1
                If CurrentSFO.Tables(i).Name = "TITLE_ID" Then
                    Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)

                    If CUInt(IDTextBox.Text.Trim().Length) > CurrentSFO.Tables(i).Indextable.param_data_max_len Then
                        MsgBox("Title ID is too long." + vbCrLf + "Max lenght: " + CurrentSFO.Tables(i).Indextable.param_data_max_len.ToString(), MsgBoxStyle.Critical, "Error")
                        Exit For
                    Else
                        TempTableItem.Value = IDTextBox.Text.Trim()
                        TempTableItem.Indextable.param_data_len = CUInt(IDTextBox.Text.Trim().Length)
                        TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                        CurrentSFO.Tables(i) = TempTableItem
                        Exit For
                    End If

                End If
            Next
        End If
    End Sub

    Private Sub ContentIDTextBox_TextChanged(sender As Object, e As Controls.TextChangedEventArgs) Handles ContentIDTextBox.TextChanged
        If Not String.IsNullOrEmpty(ContentIDTextBox.Text) Then
            For i As Integer = 0 To CurrentSFO.Tables.Count - 1
                If CurrentSFO.Tables(i).Name = "CONTENT_ID" Then
                    Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)

                    If CUInt(ContentIDTextBox.Text.Trim().Length) > CurrentSFO.Tables(i).Indextable.param_data_max_len Then
                        MsgBox("Content ID is too long." + vbCrLf + "Max lenght: " + CurrentSFO.Tables(i).Indextable.param_data_max_len.ToString(), MsgBoxStyle.Critical, "Error")
                        Exit For
                    Else
                        TempTableItem.Value = ContentIDTextBox.Text.Trim()
                        TempTableItem.Indextable.param_data_len = CUInt(ContentIDTextBox.Text.Trim().Length)
                        TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                        CurrentSFO.Tables(i) = TempTableItem
                        Exit For
                    End If

                End If
            Next
        End If
    End Sub

    Private Sub TitleTextBox_TextChanged(sender As Object, e As Controls.TextChangedEventArgs) Handles TitleTextBox.TextChanged
        If Not String.IsNullOrEmpty(TitleTextBox.Text) Then
            For i As Integer = 0 To CurrentSFO.Tables.Count - 1
                If CurrentSFO.Tables(i).Name = "TITLE" Then
                    Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)

                    If CUInt(TitleTextBox.Text.Trim().Length) > CurrentSFO.Tables(i).Indextable.param_data_max_len Then
                        MsgBox("Title is too long." + vbCrLf + "Max lenght: " + CurrentSFO.Tables(i).Indextable.param_data_max_len.ToString(), MsgBoxStyle.Critical, "Error")
                        Exit For
                    Else
                        TempTableItem.Value = TitleTextBox.Text.Trim()
                        TempTableItem.Indextable.param_data_len = CUInt(TitleTextBox.Text.Trim().Length)
                        TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                        CurrentSFO.Tables(i) = TempTableItem
                        Exit For
                    End If

                End If
            Next
        End If
    End Sub

    Private Sub VerTextBox_TextChanged(sender As Object, e As Controls.TextChangedEventArgs) Handles VerTextBox.TextChanged
        If Not String.IsNullOrEmpty(VerTextBox.Text) Then
            For i As Integer = 0 To CurrentSFO.Tables.Count - 1
                If CurrentSFO.Tables(i).Name = "VERSION" Then
                    Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)

                    If CUInt(VerTextBox.Text.Trim().Length) > CurrentSFO.Tables(i).Indextable.param_data_max_len Then
                        MsgBox("Version value is too long." + vbCrLf + "Max lenght: " + CurrentSFO.Tables(i).Indextable.param_data_max_len.ToString(), MsgBoxStyle.Critical, "Error")
                        Exit For
                    Else
                        TempTableItem.Value = VerTextBox.Text.Trim()
                        TempTableItem.Indextable.param_data_len = CUInt(VerTextBox.Text.Trim().Length)
                        TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                        CurrentSFO.Tables(i) = TempTableItem
                        Exit For
                    End If

                End If
            Next
        End If
    End Sub

    Private Sub AppVerTextBox_TextChanged(sender As Object, e As Controls.TextChangedEventArgs) Handles AppVerTextBox.TextChanged
        If Not String.IsNullOrEmpty(AppVerTextBox.Text) Then
            For i As Integer = 0 To CurrentSFO.Tables.Count - 1
                If CurrentSFO.Tables(i).Name = "APP_VER" Then
                    Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)

                    If CUInt(AppVerTextBox.Text.Trim().Length) > CurrentSFO.Tables(i).Indextable.param_data_max_len Then
                        MsgBox("App Version value is too long." + vbCrLf + "Max lenght: " + CurrentSFO.Tables(i).Indextable.param_data_max_len.ToString(), MsgBoxStyle.Critical, "Error")
                        Exit For
                    Else
                        TempTableItem.Value = AppVerTextBox.Text.Trim()
                        TempTableItem.Indextable.param_data_len = CUInt(AppVerTextBox.Text.Trim().Length)
                        TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                        CurrentSFO.Tables(i) = TempTableItem
                        Exit For
                    End If

                End If
            Next
        End If
    End Sub

    Private Sub ParentalComboBox_SelectionChanged(sender As Object, e As Controls.SelectionChangedEventArgs) Handles ParentalComboBox.SelectionChanged
        If ParentalComboBox.SelectedItem IsNot Nothing Then
            'Double check selected value
            Dim SelectedCBItem As Controls.ComboBoxItem = CType(ParentalComboBox.SelectedItem, Controls.ComboBoxItem)

            If Not String.IsNullOrEmpty(SelectedCBItem.Content.ToString()) Then
                Dim SelectedValue As String = SelectedCBItem.Content.ToString()

                For i As Integer = 0 To CurrentSFO.Tables.Count - 1
                    If CurrentSFO.Tables(i).Name = "PARENTAL_LEVEL" Then
                        Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)

                        If CUInt(SelectedValue.Length) > CurrentSFO.Tables(i).Indextable.param_data_max_len Then
                            MsgBox("Parental Level value is too long." + vbCrLf + "Max lenght: " + CurrentSFO.Tables(i).Indextable.param_data_max_len.ToString(), MsgBoxStyle.Critical, "Error")
                            Exit For
                        Else
                            TempTableItem.Value = SelectedValue
                            TempTableItem.Indextable.param_data_len = CUInt(SelectedValue.Length)
                            TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                            CurrentSFO.Tables(i) = TempTableItem
                            Exit For
                        End If

                    End If
                Next

            Else
                MsgBox("Could not determine the Parental Level value, please re-select it and try again.", MsgBoxStyle.Exclamation, "Error")
            End If
        End If
    End Sub

    Private Sub CategoryTextBox_TextChanged(sender As Object, e As Controls.TextChangedEventArgs) Handles CategoryTextBox.TextChanged
        If Not String.IsNullOrEmpty(CategoryTextBox.Text) Then
            For i As Integer = 0 To CurrentSFO.Tables.Count - 1
                If CurrentSFO.Tables(i).Name = "CATEGORY" Then
                    Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)

                    If CUInt(CategoryTextBox.Text.Trim().Length) > CurrentSFO.Tables(i).Indextable.param_data_max_len Then
                        MsgBox("Category value is too long." + vbCrLf + "Max lenght: " + CurrentSFO.Tables(i).Indextable.param_data_max_len.ToString(), MsgBoxStyle.Critical, "Error")
                        Exit For
                    Else
                        TempTableItem.Value = CategoryTextBox.Text.Trim()
                        TempTableItem.Indextable.param_data_len = CUInt(CategoryTextBox.Text.Trim().Length)
                        TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                        CurrentSFO.Tables(i) = TempTableItem
                        Exit For
                    End If

                End If
            Next
        End If
    End Sub

    Private Sub SystemVersionTextBox_TextChanged(sender As Object, e As Controls.TextChangedEventArgs) Handles SystemVersionTextBox.TextChanged
        If Not String.IsNullOrEmpty(SystemVersionTextBox.Text) Then

            Dim SystemVersionParam As String = ""
            Select Case CurrentSFO.PlaystationVersion
                Case Param_SFO.PARAM_SFO.Playstation.psp
                    SystemVersionParam = "PSP_SYSTEM_VER"
                Case Param_SFO.PARAM_SFO.Playstation.ps3
                    SystemVersionParam = "PS3_SYSTEM_VER"
                Case Param_SFO.PARAM_SFO.Playstation.psvita
                    SystemVersionParam = "PSP2_SYSTEM_VER"
                Case Param_SFO.PARAM_SFO.Playstation.ps4
                    SystemVersionParam = "SYSTEM_VER"
            End Select

            If Not String.IsNullOrEmpty(SystemVersionParam) Then
                For i As Integer = 0 To CurrentSFO.Tables.Count - 1

                    If CurrentSFO.Tables(i).Name = SystemVersionParam Then
                        Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)

                        Select Case CurrentSFO.PlaystationVersion
                            Case Param_SFO.PARAM_SFO.Playstation.psp, Param_SFO.PARAM_SFO.Playstation.ps3
                                TempTableItem.Value = SystemVersionTextBox.Text.Trim()
                                TempTableItem.Indextable.param_data_len = CurrentSFO.Tables(i).Indextable.param_data_len
                                TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                                CurrentSFO.Tables(i) = TempTableItem
                                Exit For
                            Case Param_SFO.PARAM_SFO.Playstation.psvita, Param_SFO.PARAM_SFO.Playstation.ps4
                                'Convert given hex string back (Vita & PS4)
                                Dim NewSystemVersionValue As String = CInt("&H" + SystemVersionTextBox.Text).ToString()

                                TempTableItem.Value = NewSystemVersionValue.Trim()
                                TempTableItem.Indextable.param_data_len = CurrentSFO.Tables(i).Indextable.param_data_len
                                TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                                CurrentSFO.Tables(i) = TempTableItem
                                Exit For
                        End Select

                    End If
                Next
            End If

        End If
    End Sub

#End Region

#Region "PSP Value Changes"

    Private Sub PSPBootableCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles PSPBootableCheckBox.Checked
        For i As Integer = 0 To CurrentSFO.Tables.Count - 1
            If CurrentSFO.Tables(i).Name = "BOOTABLE" Then
                Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)

                TempTableItem.Value = "1"
                TempTableItem.Indextable.param_data_len = CurrentSFO.Tables(i).Indextable.param_data_len
                TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                CurrentSFO.Tables(i) = TempTableItem
                Exit For
            End If
        Next
    End Sub

    Private Sub PSPBootableCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles PSPBootableCheckBox.Unchecked
        For i As Integer = 0 To CurrentSFO.Tables.Count - 1
            If CurrentSFO.Tables(i).Name = "BOOTABLE" Then
                Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)

                TempTableItem.Value = "0"
                TempTableItem.Indextable.param_data_len = CurrentSFO.Tables(i).Indextable.param_data_len
                TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                CurrentSFO.Tables(i) = TempTableItem
                Exit For
            End If
        Next
    End Sub

#End Region

#Region "PS3 Value Changes"

    Private Function GetPS3AttributeValue() As String
        Return "0"
    End Function

    Private Sub PS3BackgroundMusicCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles PS3BackgroundMusicCheckBox.Checked

    End Sub

    Private Sub PS3BackgroundMusicCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles PS3BackgroundMusicCheckBox.Unchecked

    End Sub

    Private Sub PS3RemoteEnabled_Checked(sender As Object, e As RoutedEventArgs) Handles PS3RemoteEnabled.Checked

    End Sub

    Private Sub PS3RemoteEnabled_Unchecked(sender As Object, e As RoutedEventArgs) Handles PS3RemoteEnabled.Unchecked

    End Sub

#End Region

#Region "PS4 Value Changes"

    Private Function GetPS4AttributeValue() As String
        Return "0"
    End Function

    Private Sub PS4AppTypeComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles PS4AppTypeComboBox.SelectionChanged
        If ParentalComboBox.SelectedItem IsNot Nothing Then
            'Double check selected value
            Dim SelectedCBItem As Controls.ComboBoxItem = CType(PS4AppTypeComboBox.SelectedItem, Controls.ComboBoxItem)

            If Not String.IsNullOrEmpty(SelectedCBItem.Content.ToString()) Then
                Dim SelectedValue As String = SelectedCBItem.Content.ToString()
                Dim NewAppTypeValue As String = ""
                Select Case SelectedValue
                    Case "Not Specified"
                        NewAppTypeValue = "0"
                    Case "Paid Standalone Full App"
                        NewAppTypeValue = "1"
                    Case "Upgradable App"
                        NewAppTypeValue = "2"
                    Case "Demo App"
                        NewAppTypeValue = "3"
                    Case "Freemium App"
                        NewAppTypeValue = "4"
                End Select

                If Not String.IsNullOrEmpty(NewAppTypeValue) Then
                    For i As Integer = 0 To CurrentSFO.Tables.Count - 1
                        If CurrentSFO.Tables(i).Name = "APP_TYPE" Then
                            Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)

                            TempTableItem.Value = NewAppTypeValue
                            TempTableItem.Indextable.param_data_len = CurrentSFO.Tables(i).Indextable.param_data_len
                            TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                            CurrentSFO.Tables(i) = TempTableItem
                            Exit For

                        End If
                    Next
                End If

            End If
        End If
    End Sub

    Private Sub PS4PubToolInfoTextBox_TextChanged(sender As Object, e As Controls.TextChangedEventArgs) Handles PS4PubToolInfoTextBox.TextChanged
        If Not String.IsNullOrEmpty(PS4PubToolInfoTextBox.Text) Then
            For i As Integer = 0 To CurrentSFO.Tables.Count - 1
                If CurrentSFO.Tables(i).Name = "PUBTOOLINFO" Then
                    Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)

                    TempTableItem.Value = PS4PubToolInfoTextBox.Text
                    TempTableItem.Indextable.param_data_len = CurrentSFO.Tables(i).Indextable.param_data_len
                    TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                    CurrentSFO.Tables(i) = TempTableItem
                    Exit For
                End If
            Next
        End If
    End Sub

    Private Sub PS4PubToolVersionTextBox_TextChanged(sender As Object, e As Controls.TextChangedEventArgs) Handles PS4PubToolVersionTextBox.TextChanged
        If Not String.IsNullOrEmpty(PS4PubToolVersionTextBox.Text) Then
            For i As Integer = 0 To CurrentSFO.Tables.Count - 1
                If CurrentSFO.Tables(i).Name = "PUBTOOLVER" Then
                    Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)
                    Dim NewPubToolVersionValue As String = CInt("&H" + PS4PubToolVersionTextBox.Text).ToString()

                    TempTableItem.Value = NewPubToolVersionValue.Trim()
                    TempTableItem.Indextable.param_data_len = CurrentSFO.Tables(i).Indextable.param_data_len
                    TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                    CurrentSFO.Tables(i) = TempTableItem
                    Exit For
                End If
            Next
        End If
    End Sub

#End Region

#Region "PS Vita Value Changes"

    Private Sub AppShotTitleTextBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles AppShotTitleTextBox.TextChanged
        If Not String.IsNullOrEmpty(AppShotTitleTextBox.Text) Then
            For i As Integer = 0 To CurrentSFO.Tables.Count - 1
                If CurrentSFO.Tables(i).Name = "STITLE" Then
                    Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)

                    If CUInt(AppShotTitleTextBox.Text.Trim().Length) > CurrentSFO.Tables(i).Indextable.param_data_max_len Then
                        MsgBox("Short Title is too long." + vbCrLf + "Max lenght: " + CurrentSFO.Tables(i).Indextable.param_data_max_len.ToString(), MsgBoxStyle.Critical, "Error")
                        Exit For
                    Else
                        TempTableItem.Value = AppShotTitleTextBox.Text.Trim()
                        TempTableItem.Indextable.param_data_len = CUInt(AppShotTitleTextBox.Text.Trim().Length)
                        TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                        CurrentSFO.Tables(i) = TempTableItem
                        Exit For
                    End If

                End If
            Next
        End If
    End Sub

    Private Sub VitaAddtionalContentCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles VitaAddtionalContentCheckBox.Checked
        If Not String.IsNullOrEmpty(ShareAppTitleTextBox.Text) Then
            For i As Integer = 0 To CurrentSFO.Tables.Count - 1
                If CurrentSFO.Tables(i).Name = "INSTALL_DIR_ADDCONT" Then
                    Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)

                    If CUInt(ShareAppTitleTextBox.Text.Trim().Length) > CurrentSFO.Tables(i).Indextable.param_data_max_len Then
                        MsgBox("Title ID of share app is too long." + vbCrLf + "Max lenght: " + CurrentSFO.Tables(i).Indextable.param_data_max_len.ToString(), MsgBoxStyle.Critical, "Error")
                        Exit For
                    Else
                        TempTableItem.Value = ShareAppTitleTextBox.Text.Trim()
                        TempTableItem.Indextable.param_data_len = CUInt(ShareAppTitleTextBox.Text.Trim().Length)
                        TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                        CurrentSFO.Tables(i) = TempTableItem
                        Exit For
                    End If

                End If
            Next
        End If
    End Sub

    Private Sub VitaAddtionalContentCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles VitaAddtionalContentCheckBox.Unchecked
        If Not String.IsNullOrEmpty(ShareAppTitleTextBox.Text) Then
            For i As Integer = 0 To CurrentSFO.Tables.Count - 1
                If CurrentSFO.Tables(i).Name = "INSTALL_DIR_ADDCONT" Then
                    Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)

                    TempTableItem.Value = ""
                    TempTableItem.Indextable.param_data_len = 0
                    TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                    CurrentSFO.Tables(i) = TempTableItem
                    Exit For

                End If
            Next
        End If
    End Sub

    Private Sub ShareAppTitleTextBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles ShareAppTitleTextBox.TextChanged
        If Not String.IsNullOrEmpty(ShareAppTitleTextBox.Text) Then
            For i As Integer = 0 To CurrentSFO.Tables.Count - 1
                If CurrentSFO.Tables(i).Name = "INSTALL_DIR_ADDCONT" Then
                    Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)

                    If CUInt(ShareAppTitleTextBox.Text.Trim().Length) > CurrentSFO.Tables(i).Indextable.param_data_max_len Then
                        MsgBox("Title ID of share app is too long." + vbCrLf + "Max lenght: " + CurrentSFO.Tables(i).Indextable.param_data_max_len.ToString(), MsgBoxStyle.Critical, "Error")
                        Exit For
                    Else
                        TempTableItem.Value = ShareAppTitleTextBox.Text.Trim()
                        TempTableItem.Indextable.param_data_len = CUInt(ShareAppTitleTextBox.Text.Trim().Length)
                        TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                        CurrentSFO.Tables(i) = TempTableItem
                        Exit For
                    End If

                End If
            Next
        End If
    End Sub

    Private Function GetVitaAttributeValue() As String
        If VitaUseLibLocationCheckBox.IsChecked AndAlso VitaDisplayInfoBarCheckBox.IsChecked AndAlso VitaColorInfoBarCheckBox.IsChecked AndAlso VitaAddHealthInfoCheckBox.IsChecked AndAlso VitaUseTWDialogCheckBox.IsChecked Then
            Return "35652994"
            Exit Function
        ElseIf VitaUseLibLocationCheckBox.IsChecked AndAlso VitaDisplayInfoBarCheckBox.IsChecked AndAlso VitaColorInfoBarCheckBox.IsChecked AndAlso VitaAddHealthInfoCheckBox.IsChecked AndAlso VitaUseTWDialogCheckBox.IsChecked Then
            Return "35652992"
            Exit Function
        ElseIf VitaUseLibLocationCheckBox.IsChecked AndAlso VitaDisplayInfoBarCheckBox.IsChecked AndAlso VitaColorInfoBarCheckBox.IsChecked Then
            Return "386"
            Exit Function
        ElseIf VitaUseLibLocationCheckBox.IsChecked AndAlso VitaColorInfoBarCheckBox.IsChecked Then
            Return "1026"
            Exit Function
        ElseIf VitaDisplayInfoBarCheckBox.IsChecked AndAlso VitaColorInfoBarCheckBox.IsChecked Then
            Return "384"
            Exit Function
        ElseIf VitaUseLibLocationCheckBox.IsChecked AndAlso VitaColorInfoBarCheckBox.IsChecked Then
            Return "258"
            Exit Function
        ElseIf VitaUseLibLocationCheckBox.IsChecked AndAlso VitaDisplayInfoBarCheckBox.IsChecked Then
            Return "130"
            Exit Function
        ElseIf VitaUseTWDialogCheckBox.IsChecked Then
            Return "33554432"
            Exit Function
        ElseIf VitaAddHealthInfoCheckBox.IsChecked Then
            Return "2097152"
            Exit Function
        ElseIf VitaAppIsUpgradedableCheckBox.IsChecked Then
            Return "1024"
            Exit Function
        ElseIf VitaColorInfoBarCheckBox.IsChecked Then
            Return "256"
            Exit Function
        ElseIf VitaDisplayInfoBarCheckBox.IsChecked Then
            Return "128"
            Exit Function
        Else
            Return "0"
            Exit Function
        End If
    End Function

    Private Sub VitaUseLibLocationCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles VitaUseLibLocationCheckBox.Checked
        For i As Integer = 0 To CurrentSFO.Tables.Count - 1
            If CurrentSFO.Tables(i).Name = "ATTRIBUTE" Then
                Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)
                Dim NewAttributeValue As String = GetVitaAttributeValue()

                If Not String.IsNullOrEmpty(NewAttributeValue) Then
                    If NewAttributeValue.Length > "2147483648".Length Then
                        MsgBox("Attribute value is too high." + vbCrLf + "Max lenght: " + CurrentSFO.Tables(i).Indextable.param_data_max_len.ToString(), MsgBoxStyle.Critical, "Error")
                        Exit For
                    Else
                        TempTableItem.Value = NewAttributeValue.Trim()
                        TempTableItem.Indextable.param_data_len = CurrentSFO.Tables(i).Indextable.param_data_len
                        TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                        CurrentSFO.Tables(i) = TempTableItem
                        Exit For
                    End If
                End If

            End If
        Next
    End Sub

    Private Sub VitaUseLibLocationCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles VitaUseLibLocationCheckBox.Unchecked
        For i As Integer = 0 To CurrentSFO.Tables.Count - 1
            If CurrentSFO.Tables(i).Name = "ATTRIBUTE" Then
                Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)
                Dim NewAttributeValue As String = GetVitaAttributeValue()

                If Not String.IsNullOrEmpty(NewAttributeValue) Then
                    MsgBox(NewAttributeValue)
                    If NewAttributeValue.Trim().Length > "2147483648".Length Then
                        MsgBox("Attribute value is too high." + vbCrLf + "Max lenght: " + CurrentSFO.Tables(i).Indextable.param_data_max_len.ToString(), MsgBoxStyle.Critical, "Error")
                        Exit For
                    Else
                        TempTableItem.Value = NewAttributeValue.Trim()
                        TempTableItem.Indextable.param_data_len = CurrentSFO.Tables(i).Indextable.param_data_len
                        TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                        CurrentSFO.Tables(i) = TempTableItem
                        Exit For
                    End If
                End If

            End If
        Next
    End Sub

    Private Sub VitaAppIsUpgradedableCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles VitaAppIsUpgradedableCheckBox.Checked
        For i As Integer = 0 To CurrentSFO.Tables.Count - 1
            If CurrentSFO.Tables(i).Name = "ATTRIBUTE" Then
                Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)
                Dim NewAttributeValue As String = GetVitaAttributeValue()

                If Not String.IsNullOrEmpty(NewAttributeValue) Then
                    MsgBox(NewAttributeValue)
                    If NewAttributeValue.Trim().Length > "2147483648".Length Then
                        MsgBox("Attribute value is too high." + vbCrLf + "Max lenght: " + CurrentSFO.Tables(i).Indextable.param_data_max_len.ToString(), MsgBoxStyle.Critical, "Error")
                        Exit For
                    Else
                        TempTableItem.Value = NewAttributeValue.Trim()
                        TempTableItem.Indextable.param_data_len = CurrentSFO.Tables(i).Indextable.param_data_len
                        TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                        CurrentSFO.Tables(i) = TempTableItem
                        Exit For
                    End If
                End If

            End If
        Next
    End Sub

    Private Sub VitaAppIsUpgradedableCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles VitaAppIsUpgradedableCheckBox.Unchecked
        For i As Integer = 0 To CurrentSFO.Tables.Count - 1
            If CurrentSFO.Tables(i).Name = "ATTRIBUTE" Then
                Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)
                Dim NewAttributeValue As String = GetVitaAttributeValue()

                If Not String.IsNullOrEmpty(NewAttributeValue) Then
                    MsgBox(NewAttributeValue)
                    If NewAttributeValue.Trim().Length > "2147483648".Length Then
                        MsgBox("Attribute value is too high." + vbCrLf + "Max lenght: " + CurrentSFO.Tables(i).Indextable.param_data_max_len.ToString(), MsgBoxStyle.Critical, "Error")
                        Exit For
                    Else
                        TempTableItem.Value = NewAttributeValue.Trim()
                        TempTableItem.Indextable.param_data_len = CurrentSFO.Tables(i).Indextable.param_data_len
                        TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                        CurrentSFO.Tables(i) = TempTableItem
                        Exit For
                    End If
                End If

            End If
        Next
    End Sub

    Private Sub VitaDisplayInfoBarCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles VitaDisplayInfoBarCheckBox.Checked
        For i As Integer = 0 To CurrentSFO.Tables.Count - 1
            If CurrentSFO.Tables(i).Name = "ATTRIBUTE" Then
                Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)
                Dim NewAttributeValue As String = GetVitaAttributeValue()

                If Not String.IsNullOrEmpty(NewAttributeValue) Then
                    MsgBox(NewAttributeValue)
                    If NewAttributeValue.Trim().Length > "2147483648".Length Then
                        MsgBox("Attribute value is too high." + vbCrLf + "Max lenght: " + CurrentSFO.Tables(i).Indextable.param_data_max_len.ToString(), MsgBoxStyle.Critical, "Error")
                        Exit For
                    Else
                        TempTableItem.Value = NewAttributeValue.Trim()
                        TempTableItem.Indextable.param_data_len = CurrentSFO.Tables(i).Indextable.param_data_len
                        TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                        CurrentSFO.Tables(i) = TempTableItem
                        Exit For
                    End If
                End If

            End If
        Next
    End Sub

    Private Sub VitaDisplayInfoBarCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles VitaDisplayInfoBarCheckBox.Unchecked
        For i As Integer = 0 To CurrentSFO.Tables.Count - 1
            If CurrentSFO.Tables(i).Name = "ATTRIBUTE" Then
                Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)
                Dim NewAttributeValue As String = GetVitaAttributeValue()

                If Not String.IsNullOrEmpty(NewAttributeValue) Then
                    MsgBox(NewAttributeValue)
                    If NewAttributeValue.Trim().Length > "2147483648".Length Then
                        MsgBox("Attribute value is too high." + vbCrLf + "Max lenght: " + CurrentSFO.Tables(i).Indextable.param_data_max_len.ToString(), MsgBoxStyle.Critical, "Error")
                        Exit For
                    Else
                        TempTableItem.Value = NewAttributeValue.Trim()
                        TempTableItem.Indextable.param_data_len = CurrentSFO.Tables(i).Indextable.param_data_len
                        TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                        CurrentSFO.Tables(i) = TempTableItem
                        Exit For
                    End If
                End If

            End If
        Next
    End Sub

    Private Sub VitaColorInfoBarCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles VitaColorInfoBarCheckBox.Checked
        For i As Integer = 0 To CurrentSFO.Tables.Count - 1
            If CurrentSFO.Tables(i).Name = "ATTRIBUTE" Then
                Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)
                Dim NewAttributeValue As String = GetVitaAttributeValue()

                If Not String.IsNullOrEmpty(NewAttributeValue) Then
                    MsgBox(NewAttributeValue)
                    If NewAttributeValue.Trim().Length > "2147483648".Length Then
                        MsgBox("Attribute value is too high." + vbCrLf + "Max lenght: " + CurrentSFO.Tables(i).Indextable.param_data_max_len.ToString(), MsgBoxStyle.Critical, "Error")
                        Exit For
                    Else
                        TempTableItem.Value = NewAttributeValue.Trim()
                        TempTableItem.Indextable.param_data_len = CurrentSFO.Tables(i).Indextable.param_data_len
                        TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                        CurrentSFO.Tables(i) = TempTableItem
                        Exit For
                    End If
                End If

            End If
        Next
    End Sub

    Private Sub VitaColorInfoBarCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles VitaColorInfoBarCheckBox.Unchecked
        For i As Integer = 0 To CurrentSFO.Tables.Count - 1
            If CurrentSFO.Tables(i).Name = "ATTRIBUTE" Then
                Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)
                Dim NewAttributeValue As String = GetVitaAttributeValue()

                If Not String.IsNullOrEmpty(NewAttributeValue) Then
                    MsgBox(NewAttributeValue)
                    If NewAttributeValue.Trim().Length > "2147483648".Length Then
                        MsgBox("Attribute value is too high." + vbCrLf + "Max lenght: " + CurrentSFO.Tables(i).Indextable.param_data_max_len.ToString(), MsgBoxStyle.Critical, "Error")
                        Exit For
                    Else
                        TempTableItem.Value = NewAttributeValue.Trim()
                        TempTableItem.Indextable.param_data_len = CurrentSFO.Tables(i).Indextable.param_data_len
                        TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                        CurrentSFO.Tables(i) = TempTableItem
                        Exit For
                    End If
                End If

            End If
        Next
    End Sub

    Private Sub VitaAddHealthInfoCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles VitaAddHealthInfoCheckBox.Checked
        For i As Integer = 0 To CurrentSFO.Tables.Count - 1
            If CurrentSFO.Tables(i).Name = "ATTRIBUTE" Then
                Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)
                Dim NewAttributeValue As String = GetVitaAttributeValue()

                If Not String.IsNullOrEmpty(NewAttributeValue) Then
                    MsgBox(NewAttributeValue)
                    If NewAttributeValue.Trim().Length > "2147483648".Length Then
                        MsgBox("Attribute value is too high." + vbCrLf + "Max lenght: " + CurrentSFO.Tables(i).Indextable.param_data_max_len.ToString(), MsgBoxStyle.Critical, "Error")
                        Exit For
                    Else
                        TempTableItem.Value = NewAttributeValue.Trim()
                        TempTableItem.Indextable.param_data_len = CurrentSFO.Tables(i).Indextable.param_data_len
                        TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                        CurrentSFO.Tables(i) = TempTableItem
                        Exit For
                    End If
                End If

            End If
        Next
    End Sub

    Private Sub VitaAddHealthInfoCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles VitaAddHealthInfoCheckBox.Unchecked
        For i As Integer = 0 To CurrentSFO.Tables.Count - 1
            If CurrentSFO.Tables(i).Name = "ATTRIBUTE" Then
                Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)
                Dim NewAttributeValue As String = GetVitaAttributeValue()

                If Not String.IsNullOrEmpty(NewAttributeValue) Then
                    MsgBox(NewAttributeValue)
                    If NewAttributeValue.Trim().Length > "2147483648".Length Then
                        MsgBox("Attribute value is too high." + vbCrLf + "Max lenght: " + CurrentSFO.Tables(i).Indextable.param_data_max_len.ToString(), MsgBoxStyle.Critical, "Error")
                        Exit For
                    Else
                        TempTableItem.Value = NewAttributeValue.Trim()
                        TempTableItem.Indextable.param_data_len = CurrentSFO.Tables(i).Indextable.param_data_len
                        TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                        CurrentSFO.Tables(i) = TempTableItem
                        Exit For
                    End If
                End If

            End If
        Next
    End Sub

    Private Sub VitaUseTWDialogCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles VitaUseTWDialogCheckBox.Checked
        For i As Integer = 0 To CurrentSFO.Tables.Count - 1
            If CurrentSFO.Tables(i).Name = "ATTRIBUTE" Then
                Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)
                Dim NewAttributeValue As String = GetVitaAttributeValue()

                If Not String.IsNullOrEmpty(NewAttributeValue) Then
                    MsgBox(NewAttributeValue)
                    If NewAttributeValue.Trim().Length > "2147483648".Length Then
                        MsgBox("Attribute value is too high." + vbCrLf + "Max lenght: " + CurrentSFO.Tables(i).Indextable.param_data_max_len.ToString(), MsgBoxStyle.Critical, "Error")
                        Exit For
                    Else
                        TempTableItem.Value = NewAttributeValue.Trim()
                        TempTableItem.Indextable.param_data_len = CurrentSFO.Tables(i).Indextable.param_data_len
                        TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                        CurrentSFO.Tables(i) = TempTableItem
                        Exit For
                    End If
                End If

            End If
        Next
    End Sub

    Private Sub VitaUseTWDialogCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles VitaUseTWDialogCheckBox.Unchecked
        For i As Integer = 0 To CurrentSFO.Tables.Count - 1
            If CurrentSFO.Tables(i).Name = "ATTRIBUTE" Then
                Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)
                Dim NewAttributeValue As String = GetVitaAttributeValue()

                If Not String.IsNullOrEmpty(NewAttributeValue) Then
                    If CUInt(NewAttributeValue.Trim().Length) > "2147483648".Length Then
                        MsgBox("Attribute value is too high." + vbCrLf + "Max lenght: " + CurrentSFO.Tables(i).Indextable.param_data_max_len.ToString(), MsgBoxStyle.Critical, "Error")
                        Exit For
                    Else
                        TempTableItem.Value = NewAttributeValue.Trim()
                        TempTableItem.Indextable.param_data_len = CUInt(NewAttributeValue.Trim().Length)
                        TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                        CurrentSFO.Tables(i) = TempTableItem
                        Exit For
                    End If
                End If

            End If
        Next
    End Sub

    Private Sub VitaSaveDataQuotaTextBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles VitaSaveDataQuotaTextBox.TextChanged
        If Not String.IsNullOrEmpty(VitaSaveDataQuotaTextBox.Text) Then
            For i As Integer = 0 To CurrentSFO.Tables.Count - 1
                If CurrentSFO.Tables(i).Name = "SAVEDATA_MAX_SIZE" Then
                    Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)

                    If CUInt(VitaSaveDataQuotaTextBox.Text.Trim().Length) > Convert.ToInt32(CurrentSFO.Tables(i).Indextable.param_data_max_len) Then
                        MsgBox("Save Data Quota is too long." + vbCrLf + "Max lenght: " + CurrentSFO.Tables(i).Indextable.param_data_max_len.ToString(), MsgBoxStyle.Critical, "Error")
                        Exit For
                    Else
                        TempTableItem.Value = VitaSaveDataQuotaTextBox.Text.Trim()
                        TempTableItem.Indextable.param_data_len = CUInt(VitaSaveDataQuotaTextBox.Text.Trim().Length)
                        TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                        CurrentSFO.Tables(i) = TempTableItem
                        Exit For
                    End If

                End If
            Next
        End If
    End Sub

    Private Sub VitaEnableShareSaveCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles VitaEnableShareSaveCheckBox.Checked
        If Not String.IsNullOrEmpty(VitaShareSaveDataTextBox.Text) Then
            For i As Integer = 0 To CurrentSFO.Tables.Count - 1
                If CurrentSFO.Tables(i).Name = "INSTALL_DIR_SAVEDATA" Then
                    Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)

                    If CUInt(VitaShareSaveDataTextBox.Text.Trim().Length) > CurrentSFO.Tables(i).Indextable.param_data_max_len Then
                        MsgBox("Title ID of share app is too long." + vbCrLf + "Max lenght: " + CurrentSFO.Tables(i).Indextable.param_data_max_len.ToString(), MsgBoxStyle.Critical, "Error")
                        Exit For
                    Else
                        TempTableItem.Value = VitaShareSaveDataTextBox.Text.Trim()
                        TempTableItem.Indextable.param_data_len = CUInt(VitaShareSaveDataTextBox.Text.Trim().Length)
                        TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                        CurrentSFO.Tables(i) = TempTableItem
                        Exit For
                    End If

                End If
            Next
        End If
    End Sub

    Private Sub VitaEnableShareSaveCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles VitaEnableShareSaveCheckBox.Unchecked
        If Not String.IsNullOrEmpty(VitaShareSaveDataTextBox.Text) Then
            For i As Integer = 0 To CurrentSFO.Tables.Count - 1
                If CurrentSFO.Tables(i).Name = "INSTALL_DIR_SAVEDATA" Then
                    Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)

                    If CUInt(VitaShareSaveDataTextBox.Text.Trim().Length) > CurrentSFO.Tables(i).Indextable.param_data_max_len Then
                        MsgBox("Title ID of share app is too long." + vbCrLf + "Max lenght: " + CurrentSFO.Tables(i).Indextable.param_data_max_len.ToString(), MsgBoxStyle.Critical, "Error")
                        Exit For
                    Else
                        TempTableItem.Value = ""
                        TempTableItem.Indextable.param_data_len = 0
                        TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                        CurrentSFO.Tables(i) = TempTableItem
                        Exit For
                    End If

                End If
            Next
        End If
    End Sub

    Private Sub VitaShareSaveDataTextBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles VitaShareSaveDataTextBox.TextChanged
        If Not String.IsNullOrEmpty(VitaShareSaveDataTextBox.Text) Then
            For i As Integer = 0 To CurrentSFO.Tables.Count - 1
                If CurrentSFO.Tables(i).Name = "INSTALL_DIR_SAVEDATA" Then
                    Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)

                    If CUInt(VitaShareSaveDataTextBox.Text.Trim().Length) > CurrentSFO.Tables(i).Indextable.param_data_max_len Then
                        MsgBox("Title ID of share app is too long." + vbCrLf + "Max lenght: " + CurrentSFO.Tables(i).Indextable.param_data_max_len.ToString(), MsgBoxStyle.Critical, "Error")
                        Exit For
                    Else
                        TempTableItem.Value = VitaShareSaveDataTextBox.Text.Trim()
                        TempTableItem.Indextable.param_data_len = CUInt(VitaShareSaveDataTextBox.Text.Trim().Length)
                        TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                        CurrentSFO.Tables(i) = TempTableItem
                        Exit For
                    End If

                End If
            Next
        End If
    End Sub

    Private Sub VitaSupportGameBootMsgCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles VitaSupportGameBootMsgCheckBox.Checked
        If Not String.IsNullOrEmpty(VitaNPComIDTextBox.Text) Then
            For i As Integer = 0 To CurrentSFO.Tables.Count - 1
                If CurrentSFO.Tables(i).Name = "NP_COMMUNICATION_ID" Then
                    Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)

                    If CUInt(VitaNPComIDTextBox.Text.Trim().Length) > CurrentSFO.Tables(i).Indextable.param_data_max_len Then
                        MsgBox("NP Comunications ID is too long." + vbCrLf + "Max lenght: " + CurrentSFO.Tables(i).Indextable.param_data_max_len.ToString(), MsgBoxStyle.Critical, "Error")
                        Exit For
                    Else
                        TempTableItem.Value = VitaNPComIDTextBox.Text.Trim()
                        TempTableItem.Indextable.param_data_len = CUInt(VitaNPComIDTextBox.Text.Trim().Length)
                        TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                        CurrentSFO.Tables(i) = TempTableItem
                        Exit For
                    End If

                End If
            Next
        End If
    End Sub

    Private Sub VitaSupportGameBootMsgCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles VitaSupportGameBootMsgCheckBox.Unchecked
        If Not String.IsNullOrEmpty(VitaNPComIDTextBox.Text) Then
            For i As Integer = 0 To CurrentSFO.Tables.Count - 1
                If CurrentSFO.Tables(i).Name = "NP_COMMUNICATION_ID" Then
                    Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)

                    If CUInt(VitaNPComIDTextBox.Text.Trim().Length) > CurrentSFO.Tables(i).Indextable.param_data_max_len Then
                        MsgBox("NP Comunications ID is too long." + vbCrLf + "Max lenght: " + CurrentSFO.Tables(i).Indextable.param_data_max_len.ToString(), MsgBoxStyle.Critical, "Error")
                        Exit For
                    Else
                        TempTableItem.Value = ""
                        TempTableItem.Indextable.param_data_len = 0
                        TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                        CurrentSFO.Tables(i) = TempTableItem
                        Exit For
                    End If

                End If
            Next
        End If
    End Sub

    Private Sub VitaNPComIDTextBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles VitaNPComIDTextBox.TextChanged
        If Not String.IsNullOrEmpty(VitaNPComIDTextBox.Text) Then
            For i As Integer = 0 To CurrentSFO.Tables.Count - 1
                If CurrentSFO.Tables(i).Name = "NP_COMMUNICATION_ID" Then
                    Dim TempTableItem As Param_SFO.PARAM_SFO.Table = CurrentSFO.Tables(i)

                    If CUInt(VitaNPComIDTextBox.Text.Trim().Length) > CurrentSFO.Tables(i).Indextable.param_data_max_len Then
                        MsgBox("NP Comunications ID is too long." + vbCrLf + "Max lenght: " + CurrentSFO.Tables(i).Indextable.param_data_max_len.ToString(), MsgBoxStyle.Critical, "Error")
                        Exit For
                    Else
                        TempTableItem.Value = VitaNPComIDTextBox.Text.Trim()
                        TempTableItem.Indextable.param_data_len = CUInt(VitaNPComIDTextBox.Text.Trim().Length)
                        TempTableItem.Indextable.param_data_max_len = CurrentSFO.Tables(i).Indextable.param_data_max_len

                        CurrentSFO.Tables(i) = TempTableItem
                        Exit For
                    End If

                End If
            Next
        End If
    End Sub

#End Region

End Class
