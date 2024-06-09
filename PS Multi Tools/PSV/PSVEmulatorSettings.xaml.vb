Imports System.IO
Imports System.Text

Public Class PSVEmulatorSettings

    Private Sub PSVEmulatorSettings_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            Dim ValidationLayer As String = GetVitaConfigValue("validation-layer").Trim()
            Dim PSTVMode As String = GetVitaConfigValue("pstv-mode").Trim()
            Dim ShowMode As String = GetVitaConfigValue("show-mode").Trim()
            Dim DemoMode As String = GetVitaConfigValue("demo-mode").Trim()
            Dim DisplaySystemApps As String = GetVitaConfigValue("display-system-apps").Trim()
            Dim StretchTheDisplayArea As String = GetVitaConfigValue("stretch_the_display_area").Trim()
            Dim ShowLiveAreaScreen As String = GetVitaConfigValue("show-live-area-screen").Trim()
            Dim HighAccuracy As String = GetVitaConfigValue("high-accuracy").Trim()
            Dim DisableSurfaceSync As String = GetVitaConfigValue("disable-surface-sync").Trim()
            Dim VSync As String = GetVitaConfigValue("v-sync").Trim()
            Dim TextureCache As String = GetVitaConfigValue("texture-cache").Trim()
            Dim AsyncPipelineCompilation As String = GetVitaConfigValue("async-pipeline-compilation").Trim()
            Dim ShowCompileShaders As String = GetVitaConfigValue("show-compile-shaders").Trim()
            Dim HashlessTextureCache As String = GetVitaConfigValue("hashless-texture-cache").Trim()
            Dim ImportTextures As String = GetVitaConfigValue("import-textures").Trim()
            Dim ExportTextures As String = GetVitaConfigValue("export-textures").Trim()
            Dim ExportAsPNG As String = GetVitaConfigValue("export-as-png").Trim()
            Dim BootAppsFullScreen As String = GetVitaConfigValue("boot-apps-full-screen").Trim()
            Dim NGSEnable As String = GetVitaConfigValue("ngs-enable").Trim()
            Dim CPUOpt As String = GetVitaConfigValue("cpu-opt").Trim()
            Dim ShowTouchpadCursor As String = GetVitaConfigValue("show-touchpad-cursor").Trim()
            Dim PerformanceOverlay As String = GetVitaConfigValue("performance-overlay").Trim()
            Dim DisplayInfoMessage As String = GetVitaConfigValue("display-info-message").Trim()
            Dim AsiaFontSupport As String = GetVitaConfigValue("asia-font-support").Trim()
            Dim ShaderCache As String = GetVitaConfigValue("shader-cache").Trim()
            Dim SpirvShader As String = GetVitaConfigValue("spirv-shader").Trim()
            Dim FPSHack As String = GetVitaConfigValue("fps-hack").Trim()
            Dim HTTPEnable As String = GetVitaConfigValue("http-enable").Trim()

            If ValidationLayer = "true" Then ValidationLayerCheckBox.IsChecked = True
            If PSTVMode = "true" Then PSTVModeCheckBox.IsChecked = True
            If ShowMode = "true" Then ShowModeCheckBox.IsChecked = True
            If DemoMode = "true" Then DemoModeCheckBox.IsChecked = True
            If DisplaySystemApps = "true" Then DisplaySystemAppsCheckBox.IsChecked = True
            If StretchTheDisplayArea = "true" Then StretchDisplayAreaCheckBox.IsChecked = True
            If ShowLiveAreaScreen = "true" Then ShowLiveAreaScreenCheckBox.IsChecked = True
            If HighAccuracy = "true" Then HighAccuracyCheckBox.IsChecked = True
            If DisableSurfaceSync = "true" Then DisableSurfaceSyncCheckBox.IsChecked = True
            If VSync = "true" Then VSyncCheckBox.IsChecked = True
            If TextureCache = "true" Then TextureCacheCheckBox.IsChecked = True
            If AsyncPipelineCompilation = "true" Then AsyncPipeplineCompilationCheckBox.IsChecked = True
            If ShowCompileShaders = "true" Then ShowCompileShadersCheckBox.IsChecked = True
            If HashlessTextureCache = "true" Then HashlessTextureCacheCheckBox.IsChecked = True
            If ImportTextures = "true" Then ImportTexturesCheckBox.IsChecked = True
            If ExportTextures = "true" Then ExportTexturesCheckBox.IsChecked = True
            If ExportAsPNG = "true" Then ExportAsPNGCheckBox.IsChecked = True
            If BootAppsFullScreen = "true" Then BootAppsFSCheckBox.IsChecked = True
            If NGSEnable = "true" Then NGSEnableCheckBox.IsChecked = True
            If CPUOpt = "true" Then CPUOPTCheckBox.IsChecked = True
            If ShowTouchpadCursor = "true" Then ShowTouchpadCursorCheckBox.IsChecked = True
            If PerformanceOverlay = "true" Then PerformanceOverlayCheckBox.IsChecked = True
            If DisplayInfoMessage = "true" Then DisplayInfoMessageCheckBox.IsChecked = True
            If AsiaFontSupport = "true" Then AsiaFontSupportCheckBox.IsChecked = True
            If ShaderCache = "true" Then ShaderCacheCheckBox.IsChecked = True
            If SpirvShader = "true" Then SpirvCacheCheckBox.IsChecked = True
            If FPSHack = "true" Then FPSHackCheckBox.IsChecked = True
            If HTTPEnable = "true" Then HTTPEnableCheckBox.IsChecked = True
        End If
    End Sub

    Private Function GetVitaConfigValue(ConfigName As String) As String
        Dim ConfigValue As String = ""

        For Each Line As String In File.ReadAllLines(FileIO.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml")
            If Line.Contains(ConfigName) Then
                ConfigValue = Line.Split(":"c)(1)
                Exit For
            End If
        Next

        If Not String.IsNullOrEmpty(ConfigValue) Then
            Return ConfigValue
        Else
            Return String.Empty
        End If
    End Function

    Private Sub SetVitaConfigValue(ConfigName As String, NewValue As String)
        Dim NewConfigLines As New List(Of String)()

        For Each Line As String In File.ReadAllLines(FileIO.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml")
            If Line.Contains(ConfigName) Then
                'Split the line of the config -> initial-setup(0): false(1)
                Dim SplittedLine As String() = Line.Split(":"c)
                'Replace the line with the new value -> initial-setup(0) + ": " + NewValue 
                NewConfigLines.Add(SplittedLine(0) + ": " + NewValue)
            Else
                'Add the other lines of the config file
                NewConfigLines.Add(Line)
            End If
        Next

        'Save as new config
        File.WriteAllLines(FileIO.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml", NewConfigLines.ToArray(), Encoding.UTF8)
    End Sub

#Region "CheckBox Changes"

    Private Sub AsiaFontSupportCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles AsiaFontSupportCheckBox.Checked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("asia-font-support", "true")
        End If
    End Sub

    Private Sub AsyncPipeplineCompilationCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles AsyncPipeplineCompilationCheckBox.Checked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("async-pipeline-compilation", "true")
        End If
    End Sub

    Private Sub BootAppsFSCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles BootAppsFSCheckBox.Checked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("boot-apps-full-screen", "true")
        End If
    End Sub

    Private Sub CPUOPTCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles CPUOPTCheckBox.Checked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("cpu-opt", "true")
        End If
    End Sub

    Private Sub DemoModeCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles DemoModeCheckBox.Checked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("demo-mode", "true")
        End If
    End Sub

    Private Sub DisableSurfaceSyncCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles DisableSurfaceSyncCheckBox.Checked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("disable-surface-sync", "true")
        End If
    End Sub

    Private Sub DisplaySystemAppsCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles DisplaySystemAppsCheckBox.Checked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("display-system-apps", "true")
        End If
    End Sub

    Private Sub ExportAsPNGCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles ExportAsPNGCheckBox.Checked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("export-as-png", "true")
        End If
    End Sub

    Private Sub ExportTexturesCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles ExportTexturesCheckBox.Checked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("export-textures", "true")
        End If
    End Sub

    Private Sub FPSHackCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles FPSHackCheckBox.Checked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("fps-hack", "true")
        End If
    End Sub

    Private Sub HashlessTextureCacheCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles HashlessTextureCacheCheckBox.Checked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("hashless-texture-cache", "true")
        End If
    End Sub

    Private Sub HighAccuracyCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles HighAccuracyCheckBox.Checked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("high-accuracy", "true")
        End If
    End Sub

    Private Sub HTTPEnableCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles HTTPEnableCheckBox.Checked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("http-enable", "true")
        End If
    End Sub

    Private Sub ImportTexturesCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles ImportTexturesCheckBox.Checked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("import-textures", "true")
        End If
    End Sub

    Private Sub NGSEnableCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles NGSEnableCheckBox.Checked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("ngs-enable", "true")
        End If
    End Sub

    Private Sub PerformanceOverlayCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles PerformanceOverlayCheckBox.Checked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("performance-overlay", "true")
        End If
    End Sub

    Private Sub PSTVModeCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles PSTVModeCheckBox.Checked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("pstv-mode", "true")
        End If
    End Sub

    Private Sub ShaderCacheCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles ShaderCacheCheckBox.Checked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("shader-cache", "true")
        End If
    End Sub

    Private Sub ShowCompileShadersCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles ShowCompileShadersCheckBox.Checked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("show-compile-shaders", "true")
        End If
    End Sub

    Private Sub ShowLiveAreaScreenCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles ShowLiveAreaScreenCheckBox.Checked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("show-live-area-screen", "true")
        End If
    End Sub

    Private Sub ShowModeCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles ShowModeCheckBox.Checked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("show-mode", "true")
        End If
    End Sub

    Private Sub ShowTouchpadCursorCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles ShowTouchpadCursorCheckBox.Checked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("show-touchpad-cursor", "true")
        End If
    End Sub

    Private Sub SpirvCacheCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles SpirvCacheCheckBox.Checked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("spirv-shader", "true")
        End If
    End Sub

    Private Sub StretchDisplayAreaCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles StretchDisplayAreaCheckBox.Checked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("stretch_the_display_area", "true")
        End If
    End Sub

    Private Sub TextureCacheCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles TextureCacheCheckBox.Checked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("texture-cache", "true")
        End If
    End Sub

    Private Sub ValidationLayerCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles ValidationLayerCheckBox.Checked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("validation-layer", "true")
        End If
    End Sub

    Private Sub VSyncCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles VSyncCheckBox.Checked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("v-sync", "true")
        End If
    End Sub

    Private Sub AsiaFontSupportCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles AsiaFontSupportCheckBox.Unchecked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("asia-font-support", "false")
        End If
    End Sub

    Private Sub AsyncPipeplineCompilationCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles AsyncPipeplineCompilationCheckBox.Unchecked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("async-pipeline-compilation", "false")
        End If
    End Sub

    Private Sub BootAppsFSCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles BootAppsFSCheckBox.Unchecked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("boot-apps-full-screen", "false")
        End If
    End Sub

    Private Sub CPUOPTCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles CPUOPTCheckBox.Unchecked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("cpu-opt", "false")
        End If
    End Sub

    Private Sub DemoModeCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles DemoModeCheckBox.Unchecked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("demo-mode", "false")
        End If
    End Sub

    Private Sub DisableSurfaceSyncCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles DisableSurfaceSyncCheckBox.Unchecked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("disable-surface-sync", "false")
        End If
    End Sub

    Private Sub DisplaySystemAppsCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles DisplaySystemAppsCheckBox.Unchecked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("display-system-apps", "false")
        End If
    End Sub

    Private Sub ExportAsPNGCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles ExportAsPNGCheckBox.Unchecked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("export-as-png", "false")
        End If
    End Sub

    Private Sub ExportTexturesCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles ExportTexturesCheckBox.Unchecked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("export-textures", "false")
        End If
    End Sub

    Private Sub FPSHackCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles FPSHackCheckBox.Unchecked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("fps-hack", "false")
        End If
    End Sub

    Private Sub HashlessTextureCacheCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles HashlessTextureCacheCheckBox.Unchecked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("hashless-texture-cache", "false")
        End If
    End Sub

    Private Sub HighAccuracyCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles HighAccuracyCheckBox.Unchecked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("high-accuracy", "false")
        End If
    End Sub

    Private Sub HTTPEnableCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles HTTPEnableCheckBox.Unchecked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("http-enable", "false")
        End If
    End Sub

    Private Sub ImportTexturesCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles ImportTexturesCheckBox.Unchecked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("import-textures", "false")
        End If
    End Sub

    Private Sub NGSEnableCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles NGSEnableCheckBox.Unchecked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("ngs-enable", "false")
        End If
    End Sub

    Private Sub PerformanceOverlayCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles PerformanceOverlayCheckBox.Unchecked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("performance-overlay", "false")
        End If
    End Sub

    Private Sub PSTVModeCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles PSTVModeCheckBox.Unchecked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("pstv-mode", "false")
        End If
    End Sub

    Private Sub ShaderCacheCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles ShaderCacheCheckBox.Unchecked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("shader-cache", "false")
        End If
    End Sub

    Private Sub ShowCompileShadersCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles ShowCompileShadersCheckBox.Unchecked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("show-compile-shaders", "false")
        End If
    End Sub

    Private Sub ShowLiveAreaScreenCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles ShowLiveAreaScreenCheckBox.Unchecked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("show-live-area-screen", "false")
        End If
    End Sub

    Private Sub ShowModeCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles ShowModeCheckBox.Unchecked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("show-mode", "false")
        End If
    End Sub

    Private Sub ShowTouchpadCursorCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles ShowTouchpadCursorCheckBox.Unchecked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("show-touchpad-cursor", "false")
        End If
    End Sub

    Private Sub SpirvCacheCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles SpirvCacheCheckBox.Unchecked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("spirv-shader", "false")
        End If
    End Sub

    Private Sub StretchDisplayAreaCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles StretchDisplayAreaCheckBox.Unchecked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("stretch_the_display_area", "false")
        End If
    End Sub

    Private Sub TextureCacheCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles TextureCacheCheckBox.Unchecked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("texture-cache", "false")
        End If
    End Sub

    Private Sub ValidationLayerCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles ValidationLayerCheckBox.Unchecked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("validation-layer", "false")
        End If
    End Sub

    Private Sub VSyncCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles VSyncCheckBox.Unchecked
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\vita3k\config.yml") Then
            SetVitaConfigValue("v-sync", "false")
        End If
    End Sub

#End Region

End Class
