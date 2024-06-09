Public Class PSPEmulatorSettings

    Dim PPSSPPConfig As New psmt_lib.INI.IniFile(FileIO.FileSystem.CurrentDirectory + "\Emulators\ppsspp\memstick\PSP\SYSTEM\ppsspp.ini")

    Private Sub PSPEmulatorSettings_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

#Region "General"
        Dim EnableLogging As String = PPSSPPConfig.IniReadValue("General", "Enable Logging").Trim()
        Dim IgnoreBadMemAccess As String = PPSSPPConfig.IniReadValue("General", "IgnoreBadMemAccess").Trim()
        Dim EnableCheats As String = PPSSPPConfig.IniReadValue("General", "EnableCheats").Trim()
        Dim ScreenshotsAsPNG As String = PPSSPPConfig.IniReadValue("General", "ScreenshotsAsPNG").Trim()
        Dim UseFFV1 As String = PPSSPPConfig.IniReadValue("General", "UseFFV1").Trim()
        Dim MemStickInserted As String = PPSSPPConfig.IniReadValue("General", "MemStickInserted").Trim()
        Dim EnablePlugins As String = PPSSPPConfig.IniReadValue("General", "EnablePlugins").Trim()
#End Region
#Region "CPU"
        Dim SeparateSASThread As String = PPSSPPConfig.IniReadValue("CPU", "SeparateSASThread").Trim()
        Dim FastMemoryAccess As String = PPSSPPConfig.IniReadValue("CPU", "FastMemoryAccess").Trim()
        Dim FunctionReplacements As String = PPSSPPConfig.IniReadValue("CPU", "FunctionReplacements").Trim()
        Dim HideSlowWarnings As String = PPSSPPConfig.IniReadValue("CPU", "HideSlowWarnings").Trim()
        Dim HideStateWarnings As String = PPSSPPConfig.IniReadValue("CPU", "HideStateWarnings").Trim()
        Dim PreloadFunctions As String = PPSSPPConfig.IniReadValue("CPU", "PreloadFunctions").Trim()
#End Region
#Region "Graphics"
        Dim UseGeometryShader As String = PPSSPPConfig.IniReadValue("Graphics", "UseGeometryShader").Trim()
        Dim SkipBufferEffects As String = PPSSPPConfig.IniReadValue("Graphics", "SkipBufferEffects").Trim()
        Dim DisableRangeCulling As String = PPSSPPConfig.IniReadValue("Graphics", "DisableRangeCulling").Trim()
        Dim SoftwareRenderer As String = PPSSPPConfig.IniReadValue("Graphics", "SoftwareRenderer").Trim()
        Dim SoftwareRendererJit As String = PPSSPPConfig.IniReadValue("Graphics", "SoftwareRendererJit").Trim()
        Dim HardwareTransform As String = PPSSPPConfig.IniReadValue("Graphics", "HardwareTransform").Trim()
        Dim SoftwareSkinning As String = PPSSPPConfig.IniReadValue("Graphics", "SoftwareSkinning").Trim()
        Dim Smart2DTexFiltering As String = PPSSPPConfig.IniReadValue("Graphics", "Smart2DTexFiltering").Trim()
        Dim AutoFrameSkip As String = PPSSPPConfig.IniReadValue("Graphics", "AutoFrameSkip").Trim()
        Dim TextureBackoffCache As String = PPSSPPConfig.IniReadValue("Graphics", "TextureBackoffCache").Trim()
        Dim ImmersiveMode As String = PPSSPPConfig.IniReadValue("Graphics", "ImmersiveMode").Trim()
        Dim SustainedPerformanceMode As String = PPSSPPConfig.IniReadValue("Graphics", "SustainedPerformanceMode").Trim()
        Dim IgnoreScreenInsets As String = PPSSPPConfig.IniReadValue("Graphics", "IgnoreScreenInsets").Trim()
        Dim ReplaceTextures As String = PPSSPPConfig.IniReadValue("Graphics", "ReplaceTextures").Trim()
        Dim SaveNewTextures As String = PPSSPPConfig.IniReadValue("Graphics", "SaveNewTextures").Trim()
        Dim IgnoreTextureFilenames As String = PPSSPPConfig.IniReadValue("Graphics", "IgnoreTextureFilenames").Trim()
        Dim TexDeposterize As String = PPSSPPConfig.IniReadValue("Graphics", "TexDeposterize").Trim()
        Dim TexHardwareScaling As String = PPSSPPConfig.IniReadValue("Graphics", "TexHardwareScaling").Trim()
        Dim VSync As String = PPSSPPConfig.IniReadValue("Graphics", "VSync").Trim()
        Dim HardwareTessellation As String = PPSSPPConfig.IniReadValue("Graphics", "HardwareTessellation").Trim()
        Dim TextureShader As String = PPSSPPConfig.IniReadValue("Graphics", "TextureShader").Trim()
        Dim ShaderChainRequires60FPS As String = PPSSPPConfig.IniReadValue("Graphics", "ShaderChainRequires60FPS").Trim()
        Dim LogFrameDrops As String = PPSSPPConfig.IniReadValue("Graphics", "LogFrameDrops").Trim()
        Dim RenderDuplicateFrames As String = PPSSPPConfig.IniReadValue("Graphics", "RenderDuplicateFrames").Trim()
        Dim MultiThreading As String = PPSSPPConfig.IniReadValue("Graphics", "MultiThreading").Trim()
        Dim GpuLogProfiler As String = PPSSPPConfig.IniReadValue("Graphics", "GpuLogProfiler").Trim()
        Dim UberShaderVertex As String = PPSSPPConfig.IniReadValue("Graphics", "UberShaderVertex").Trim()
        Dim UberShaderFragment As String = PPSSPPConfig.IniReadValue("Graphics", "UberShaderFragment").Trim()
#End Region

#Region "CheckBox States"
        If EnableLogging = "True" Then EnableLoggingCheckBox.IsChecked = True
        If IgnoreBadMemAccess = "True" Then IgnoreBadMemAccessCheckBox.IsChecked = True
        If EnableCheats = "True" Then EnableCheatsCheckBox.IsChecked = True
        If ScreenshotsAsPNG = "True" Then ScreenshotsAsPNGCheckBox.IsChecked = True
        If UseFFV1 = "True" Then UseFFV1CheckBox.IsChecked = True
        If MemStickInserted = "True" Then MemStickInsertedCheckBox.IsChecked = True
        If EnablePlugins = "True" Then EnablePluginsCheckBox.IsChecked = True

        If SeparateSASThread = "True" Then SeparateSASThreadCheckBox.IsChecked = True
        If FastMemoryAccess = "True" Then FastMemoryAccessCheckBox.IsChecked = True
        If FunctionReplacements = "True" Then FunctionReplacementsCheckBox.IsChecked = True
        If HideSlowWarnings = "True" Then HideSlowWarningsCheckBox.IsChecked = True
        If HideStateWarnings = "True" Then HideStateWarningsCheckBox.IsChecked = True
        If PreloadFunctions = "True" Then PreloadFunctionsCheckBox.IsChecked = True

        If UseGeometryShader = "True" Then UseGeometryShaderCheckBox.IsChecked = True
        If SkipBufferEffects = "True" Then SkipBufferEffectsCheckBox.IsChecked = True
        If DisableRangeCulling = "True" Then DisableRangeCullingCheckBox.IsChecked = True
        If SoftwareRenderer = "True" Then SoftwareRendererCheckBox.IsChecked = True
        If SoftwareRendererJit = "True" Then SoftwareRendererJitCheckBox.IsChecked = True
        If HardwareTransform = "True" Then HardwareTransformCheckBox.IsChecked = True
        If SoftwareSkinning = "True" Then SoftwareSkinningCheckBox.IsChecked = True
        If Smart2DTexFiltering = "True" Then Smart2DTexFilteringCheckBox.IsChecked = True
        If AutoFrameSkip = "True" Then AutoFrameSkipCheckBox.IsChecked = True
        If TextureBackoffCache = "True" Then TextureBackoffCacheCheckBox.IsChecked = True
        If ImmersiveMode = "True" Then ImmersiveModeCheckBox.IsChecked = True
        If SustainedPerformanceMode = "True" Then SustainedPerformanceModeCheckBox.IsChecked = True
        If IgnoreScreenInsets = "True" Then IgnoreScreenInsetsCheckBox.IsChecked = True
        If ReplaceTextures = "True" Then ReplaceTexturesCheckBox.IsChecked = True
        If SaveNewTextures = "True" Then SaveNewTexturesCheckBox.IsChecked = True
        If IgnoreTextureFilenames = "True" Then IgnoreTextureFilenamesCheckBox.IsChecked = True
        If TexDeposterize = "True" Then TexDeposterizeCheckBox.IsChecked = True
        If TexHardwareScaling = "True" Then TexHardwareScalingCheckBox.IsChecked = True
        If VSync = "True" Then VSyncCheckBox.IsChecked = True
        If HardwareTessellation = "True" Then HardwareTessellationCheckBox.IsChecked = True
        If TextureShader = "True" Then TextureShaderCheckBox.IsChecked = True
        If ShaderChainRequires60FPS = "True" Then ShaderChainRequires60FPSCheckBox.IsChecked = True
        If LogFrameDrops = "True" Then LogFrameDropsCheckBox.IsChecked = True
        If RenderDuplicateFrames = "True" Then RenderDuplicateFramesCheckBox.IsChecked = True
        If MultiThreading = "True" Then MultiThreadingCheckBox.IsChecked = True
        If GpuLogProfiler = "True" Then GpuLogProfilerCheckBox.IsChecked = True
        If UberShaderVertex = "True" Then UberShaderVertexCheckBox.IsChecked = True
        If UberShaderFragment = "True" Then UberShaderFragmentCheckBox.IsChecked = True
#End Region

    End Sub

#Region "CheckBox Changes"

    Private Sub AutoFrameSkipCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles AutoFrameSkipCheckBox.Checked
        PPSSPPConfig.IniWriteValue("Graphics", "AutoFrameSkip", "True")
    End Sub

    Private Sub DisableRangeCullingCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles DisableRangeCullingCheckBox.Checked
        PPSSPPConfig.IniWriteValue("Graphics", "DisableRangeCulling", "True")
    End Sub

    Private Sub EnableCheatsCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles EnableCheatsCheckBox.Checked
        PPSSPPConfig.IniWriteValue("General", "EnableCheats", "True")
    End Sub

    Private Sub EnableLoggingCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles EnableLoggingCheckBox.Checked
        PPSSPPConfig.IniWriteValue("General", "Enable Logging", "True")
    End Sub

    Private Sub EnablePluginsCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles EnablePluginsCheckBox.Checked
        PPSSPPConfig.IniWriteValue("General", "EnablePlugins", "True")
    End Sub

    Private Sub FastMemoryAccessCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles FastMemoryAccessCheckBox.Checked
        PPSSPPConfig.IniWriteValue("CPU", "FastMemoryAccess", "True")
    End Sub

    Private Sub FunctionReplacementsCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles FunctionReplacementsCheckBox.Checked
        PPSSPPConfig.IniWriteValue("CPU", "FunctionReplacements", "True")
    End Sub

    Private Sub GpuLogProfilerCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles GpuLogProfilerCheckBox.Checked
        PPSSPPConfig.IniWriteValue("Graphics", "GpuLogProfiler", "True")
    End Sub

    Private Sub HardwareTessellationCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles HardwareTessellationCheckBox.Checked
        PPSSPPConfig.IniWriteValue("Graphics", "HardwareTessellation", "True")
    End Sub

    Private Sub HardwareTransformCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles HardwareTransformCheckBox.Checked
        PPSSPPConfig.IniWriteValue("Graphics", "HardwareTransform", "True")
    End Sub

    Private Sub HideSlowWarningsCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles HideSlowWarningsCheckBox.Checked
        PPSSPPConfig.IniWriteValue("CPU", "HideSlowWarnings", "True")
    End Sub

    Private Sub HideStateWarningsCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles HideStateWarningsCheckBox.Checked
        PPSSPPConfig.IniWriteValue("CPU", "HideStateWarnings", "True")
    End Sub

    Private Sub IgnoreBadMemAccessCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles IgnoreBadMemAccessCheckBox.Checked
        PPSSPPConfig.IniWriteValue("General", "IgnoreBadMemAccess", "True")
    End Sub

    Private Sub IgnoreScreenInsetsCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles IgnoreScreenInsetsCheckBox.Checked
        PPSSPPConfig.IniWriteValue("Graphics", "IgnoreScreenInsets", "True")
    End Sub

    Private Sub IgnoreTextureFilenamesCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles IgnoreTextureFilenamesCheckBox.Checked
        PPSSPPConfig.IniWriteValue("Graphics", "IgnoreTextureFilenames", "True")
    End Sub

    Private Sub ImmersiveModeCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles ImmersiveModeCheckBox.Checked
        PPSSPPConfig.IniWriteValue("Graphics", "ImmersiveMode", "True")
    End Sub

    Private Sub LogFrameDropsCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles LogFrameDropsCheckBox.Checked
        PPSSPPConfig.IniWriteValue("Graphics", "LogFrameDrops", "True")
    End Sub

    Private Sub MemStickInsertedCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles MemStickInsertedCheckBox.Checked
        PPSSPPConfig.IniWriteValue("General", "MemStickInserted", "True")
    End Sub

    Private Sub MultiThreadingCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles MultiThreadingCheckBox.Checked
        PPSSPPConfig.IniWriteValue("Graphics", "MultiThreading", "True")
    End Sub

    Private Sub PreloadFunctionsCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles PreloadFunctionsCheckBox.Checked
        PPSSPPConfig.IniWriteValue("CPU", "PreloadFunctions", "True")
    End Sub

    Private Sub RenderDuplicateFramesCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles RenderDuplicateFramesCheckBox.Checked
        PPSSPPConfig.IniWriteValue("Graphics", "RenderDuplicateFrames", "True")
    End Sub

    Private Sub ReplaceTexturesCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles ReplaceTexturesCheckBox.Checked
        PPSSPPConfig.IniWriteValue("Graphics", "ReplaceTextures", "True")
    End Sub

    Private Sub SaveNewTexturesCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles SaveNewTexturesCheckBox.Checked
        PPSSPPConfig.IniWriteValue("Graphics", "SaveNewTextures", "True")
    End Sub

    Private Sub ScreenshotsAsPNGCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles ScreenshotsAsPNGCheckBox.Checked
        PPSSPPConfig.IniWriteValue("General", "ScreenshotsAsPNG", "True")
    End Sub

    Private Sub SeparateSASThreadCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles SeparateSASThreadCheckBox.Checked
        PPSSPPConfig.IniWriteValue("CPU", "SeparateSASThread", "True")
    End Sub

    Private Sub ShaderChainRequires60FPSCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles ShaderChainRequires60FPSCheckBox.Checked
        PPSSPPConfig.IniWriteValue("Graphics", "ShaderChainRequires60FPS", "True")
    End Sub

    Private Sub SkipBufferEffectsCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles SkipBufferEffectsCheckBox.Checked
        PPSSPPConfig.IniWriteValue("Graphics", "SkipBufferEffects", "True")
    End Sub

    Private Sub Smart2DTexFilteringCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles Smart2DTexFilteringCheckBox.Checked
        PPSSPPConfig.IniWriteValue("Graphics", "Smart2DTexFiltering", "True")
    End Sub

    Private Sub SoftwareRendererCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles SoftwareRendererCheckBox.Checked
        PPSSPPConfig.IniWriteValue("Graphics", "SoftwareRenderer", "True")
    End Sub

    Private Sub SoftwareRendererJitCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles SoftwareRendererJitCheckBox.Checked
        PPSSPPConfig.IniWriteValue("Graphics", "SoftwareRendererJit", "True")
    End Sub

    Private Sub SoftwareSkinningCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles SoftwareSkinningCheckBox.Checked
        PPSSPPConfig.IniWriteValue("Graphics", "SoftwareSkinning", "True")
    End Sub

    Private Sub SustainedPerformanceModeCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles SustainedPerformanceModeCheckBox.Checked
        PPSSPPConfig.IniWriteValue("Graphics", "SustainedPerformanceMode", "True")
    End Sub

    Private Sub TexDeposterizeCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles TexDeposterizeCheckBox.Checked
        PPSSPPConfig.IniWriteValue("Graphics", "TexDeposterize", "True")
    End Sub

    Private Sub TexHardwareScalingCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles TexHardwareScalingCheckBox.Checked
        PPSSPPConfig.IniWriteValue("Graphics", "TexHardwareScaling", "True")
    End Sub

    Private Sub TextureBackoffCacheCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles TextureBackoffCacheCheckBox.Checked
        PPSSPPConfig.IniWriteValue("Graphics", "TextureBackoffCache", "True")
    End Sub

    Private Sub TextureShaderCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles TextureShaderCheckBox.Checked
        PPSSPPConfig.IniWriteValue("Graphics", "TextureShader", "True")
    End Sub

    Private Sub UberShaderFragmentCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles UberShaderFragmentCheckBox.Checked
        PPSSPPConfig.IniWriteValue("Graphics", "UberShaderFragment", "True")
    End Sub

    Private Sub UberShaderVertexCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles UberShaderVertexCheckBox.Checked
        PPSSPPConfig.IniWriteValue("Graphics", "UberShaderVertex", "True")
    End Sub

    Private Sub UseFFV1CheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles UseFFV1CheckBox.Checked
        PPSSPPConfig.IniWriteValue("General", "UseFFV1", "True")
    End Sub

    Private Sub UseGeometryShaderCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles UseGeometryShaderCheckBox.Checked
        PPSSPPConfig.IniWriteValue("Graphics", "UseGeometryShader", "True")
    End Sub

    Private Sub VSyncCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles VSyncCheckBox.Checked
        PPSSPPConfig.IniWriteValue("Graphics", "VSync", "True")
    End Sub

    Private Sub AutoFrameSkipCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles AutoFrameSkipCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("Graphics", "AutoFrameSkip", "False")
    End Sub

    Private Sub DisableRangeCullingCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles DisableRangeCullingCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("Graphics", "DisableRangeCulling", "False")
    End Sub

    Private Sub EnableCheatsCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles EnableCheatsCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("General", "EnableCheats", "False")
    End Sub

    Private Sub EnableLoggingCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles EnableLoggingCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("General", "Enable Logging", "False")
    End Sub

    Private Sub EnablePluginsCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles EnablePluginsCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("General", "EnablePlugins", "False")
    End Sub

    Private Sub FastMemoryAccessCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles FastMemoryAccessCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("CPU", "FastMemoryAccess", "False")
    End Sub

    Private Sub FunctionReplacementsCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles FunctionReplacementsCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("CPU", "FunctionReplacements", "False")
    End Sub

    Private Sub GpuLogProfilerCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles GpuLogProfilerCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("Graphics", "GpuLogProfiler", "False")
    End Sub

    Private Sub HardwareTessellationCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles HardwareTessellationCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("Graphics", "HardwareTessellation", "False")
    End Sub

    Private Sub HardwareTransformCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles HardwareTransformCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("Graphics", "HardwareTransform", "False")
    End Sub

    Private Sub HideSlowWarningsCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles HideSlowWarningsCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("CPU", "HideSlowWarnings", "False")
    End Sub

    Private Sub HideStateWarningsCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles HideStateWarningsCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("CPU", "HideStateWarnings", "False")
    End Sub

    Private Sub IgnoreBadMemAccessCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles IgnoreBadMemAccessCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("General", "IgnoreBadMemAccess", "False")
    End Sub

    Private Sub IgnoreScreenInsetsCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles IgnoreScreenInsetsCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("Graphics", "IgnoreScreenInsets", "False")
    End Sub

    Private Sub IgnoreTextureFilenamesCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles IgnoreTextureFilenamesCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("Graphics", "IgnoreTextureFilenames", "False")
    End Sub

    Private Sub ImmersiveModeCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles ImmersiveModeCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("Graphics", "ImmersiveMode", "False")
    End Sub

    Private Sub LogFrameDropsCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles LogFrameDropsCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("Graphics", "LogFrameDrops", "False")
    End Sub

    Private Sub MemStickInsertedCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles MemStickInsertedCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("General", "MemStickInserted", "False")
    End Sub

    Private Sub MultiThreadingCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles MultiThreadingCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("Graphics", "MultiThreading", "False")
    End Sub

    Private Sub PreloadFunctionsCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles PreloadFunctionsCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("CPU", "PreloadFunctions", "False")
    End Sub

    Private Sub RenderDuplicateFramesCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles RenderDuplicateFramesCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("Graphics", "RenderDuplicateFrames", "False")
    End Sub

    Private Sub ReplaceTexturesCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles ReplaceTexturesCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("Graphics", "ReplaceTextures", "False")
    End Sub

    Private Sub SaveNewTexturesCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles SaveNewTexturesCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("Graphics", "SaveNewTextures", "False")
    End Sub

    Private Sub ScreenshotsAsPNGCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles ScreenshotsAsPNGCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("General", "ScreenshotsAsPNG", "False")
    End Sub

    Private Sub SeparateSASThreadCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles SeparateSASThreadCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("CPU", "SeparateSASThread", "False")
    End Sub

    Private Sub ShaderChainRequires60FPSCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles ShaderChainRequires60FPSCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("Graphics", "ShaderChainRequires60FPS", "False")
    End Sub

    Private Sub SkipBufferEffectsCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles SkipBufferEffectsCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("Graphics", "SkipBufferEffects", "False")
    End Sub

    Private Sub Smart2DTexFilteringCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles Smart2DTexFilteringCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("Graphics", "Smart2DTexFiltering", "False")
    End Sub

    Private Sub SoftwareRendererCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles SoftwareRendererCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("Graphics", "SoftwareRenderer", "False")
    End Sub

    Private Sub SoftwareRendererJitCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles SoftwareRendererJitCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("Graphics", "SoftwareRendererJit", "False")
    End Sub

    Private Sub SoftwareSkinningCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles SoftwareSkinningCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("Graphics", "SoftwareSkinning", "False")
    End Sub

    Private Sub SustainedPerformanceModeCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles SustainedPerformanceModeCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("Graphics", "SustainedPerformanceMode", "False")
    End Sub

    Private Sub TexDeposterizeCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles TexDeposterizeCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("Graphics", "TexDeposterize", "False")
    End Sub

    Private Sub TexHardwareScalingCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles TexHardwareScalingCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("Graphics", "TexHardwareScaling", "False")
    End Sub

    Private Sub TextureBackoffCacheCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles TextureBackoffCacheCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("Graphics", "TextureBackoffCache", "False")
    End Sub

    Private Sub TextureShaderCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles TextureShaderCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("Graphics", "TextureShader", "False")
    End Sub

    Private Sub UberShaderFragmentCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles UberShaderFragmentCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("Graphics", "UberShaderFragment", "False")
    End Sub

    Private Sub UberShaderVertexCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles UberShaderVertexCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("Graphics", "UberShaderVertex", "False")
    End Sub

    Private Sub UseFFV1CheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles UseFFV1CheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("General", "UseFFV1", "False")
    End Sub

    Private Sub UseGeometryShaderCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles UseGeometryShaderCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("Graphics", "UseGeometryShader", "False")
    End Sub

    Private Sub VSyncCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles VSyncCheckBox.Unchecked
        PPSSPPConfig.IniWriteValue("Graphics", "VSync", "False")
    End Sub


#End Region

End Class
