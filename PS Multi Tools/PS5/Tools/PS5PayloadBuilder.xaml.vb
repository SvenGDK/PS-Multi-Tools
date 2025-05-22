Imports System.IO
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Text
Imports System.Text.RegularExpressions
Imports ImageMagick
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports WinSCP

Public Class PS5PayloadBuilder

    Const UpdateUser As String = "EnterYourUsername"
    Const MinimalUserToken As String = "github_actions_token"
    Const RepoToUpdate As String = "EnterYourRepo"
    Const RepoBranch As String = "RepoBranch"

    Dim UserAppMetaSynced As SynchronizationResult

    Private Sub PS5PayloadBuilder_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        MsgBox("Welcome to the PS5 payload builder." & vbCrLf &
               "No SDK setup or knowledge is required." & vbCrLf &
               "However, this utility can only process 1 building instruction per user to not stress the API." & vbCrLf &
               "Simply retry after a couple of minutes if you get the message 'A payload is currently being built by another user'.",
               MsgBoxStyle.Information,
               "PS5 Payload Builder")
    End Sub

    Private Async Function CheckForActiveWorkflow(PayloadName As String) As Task(Of Integer)
        Dim WorkflowFile As String
        If PayloadName = "PS5 SELF Decrypter" Then
            WorkflowFile = "ps5_self_decrypter.yml"
        ElseIf PayloadName = "App Titles" Then
            WorkflowFile = "app_title.yml"
        Else
            Return 0
        End If

        Dim GitHubAPIURL As String = $"https://api.github.com/repos/{UpdateUser}/{RepoToUpdate}/actions/workflows/{WorkflowFile}/runs?branch={RepoBranch}&status=in_progress"
        Try
            Using NewHttpClient As New HttpClient()
                NewHttpClient.DefaultRequestHeaders.UserAgent.Add(New ProductInfoHeaderValue("PS-Multi-Tools", "15.4"))
                NewHttpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("token", MinimalUserToken)

                Dim WorkflowRunResponse As HttpResponseMessage = Await NewHttpClient.GetAsync(GitHubAPIURL)
                WorkflowRunResponse.EnsureSuccessStatusCode()
                Dim JSONContent As String = Await WorkflowRunResponse.Content.ReadAsStringAsync()
                Dim NewJSONObject As JObject = JObject.Parse(JSONContent)
                Dim RunsJSONArray As JArray = CType(NewJSONObject("workflow_runs"), JArray)

                If RunsJSONArray IsNot Nothing Then
                    Return RunsJSONArray.Count
                Else
                    Return 0
                End If
            End Using
        Catch ex As Exception
            Return 0
        End Try
    End Function

    Private Function CommitChanges(PayloadName As String) As Boolean
        'Always fall back to default values on error or no found values
        If PayloadName = "PS5 SELF Decrypter" Then
            Dim FileToUpdate As String = "PS5-SELF-Decrypter/source/main.c"
            Dim GitHubAPIURL As String = $"https://api.github.com/repos/{UpdateUser}/{RepoToUpdate}/contents/{FileToUpdate}"

            PayloadBuildProgressStatusTextBlock.Text = "Updating payload, please wait..."
            PayloadBuildProgressBar.IsIndeterminate = True

            Try
                Using NewHttpClient As New HttpClient()
                    NewHttpClient.DefaultRequestHeaders.UserAgent.Add(New ProductInfoHeaderValue("PS-Multi-Tools", "15.4"))
                    NewHttpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("token", MinimalUserToken)

                    'Check if file can be accessed
                    Dim TestResponse = NewHttpClient.GetAsync(GitHubAPIURL).Result
                    If Not TestResponse.IsSuccessStatusCode Then
                        Return False
                    End If

                    'Read existing content
                    Dim ExistingContentJSON As String = TestResponse.Content.ReadAsStringAsync().Result
                    Dim NewFileInfo As Dictionary(Of String, Object) = JsonConvert.DeserializeObject(Of Dictionary(Of String, Object))(ExistingContentJSON)
                    Dim FileSHA As String = NewFileInfo("sha").ToString()
                    Dim NewContentBytes() As Byte

                    'Prepare new content
                    Dim ExistingContentBase64 As String = NewFileInfo("content").ToString()
                    ExistingContentBase64 = ExistingContentBase64.Replace(vbLf, "").Replace(vbCr, "")
                    Dim ExistingContentBytes() As Byte = Convert.FromBase64String(ExistingContentBase64)
                    Dim ExistingFileContentString As String = Encoding.UTF8.GetString(ExistingContentBytes)

                    Dim UpdatedContent As String = ExistingFileContentString 'Base source file

                    'Replace IP in source if requested
                    If Not String.IsNullOrEmpty(SELFDecrypterIPTextBox.Text) Then
                        Dim IPPattern As String = "^(\s*#define\s+PC_IP\s+)""[^""]*"""
                        Dim ReplacementIP As String = "$1""" & SELFDecrypterIPTextBox.Text & """"
                        UpdatedContent = Regex.Replace(ExistingFileContentString, IPPattern, ReplacementIP, RegexOptions.Multiline)
                    End If

                    'Replace Port in source if requested
                    If Not String.IsNullOrEmpty(SELFDecrypterPortTextBox.Text) Then
                        Dim PORTPattern As String = "^(\s*#define\s+PC_PORT\s+)\d+"
                        Dim ReplacementPort As String = "${1}" & SELFDecrypterPortTextBox.Text
                        UpdatedContent = Regex.Replace(UpdatedContent, PORTPattern, ReplacementPort, RegexOptions.Multiline)
                    End If

                    'Add more directories to dump if requested
                    If FullDumpCheckBox.IsChecked Then
                        Dim RootDIR As String = "dump_queue_add_dir(sock, ""/"", 0);"
                        Dim SystemExCommonExLIB As String = "dump_queue_add_dir(sock, ""/system_ex/common_ex/lib"", 0);"
                        Dim CommonLIB As String = "dump_queue_add_dir(sock, ""/system/common/lib"", 0);"
                        Dim PrivLIB As String = "dump_queue_add_dir(sock, ""/system/priv/lib"", 0);"
                        Dim SystemVSH As String = "dump_queue_add_dir(sock, ""/system/vsh"", 1);"
                        Dim SystemSYS As String = "dump_queue_add_dir(sock, ""/system/sys"", 0);"

                        Dim TargetLine As String = "SOCK_LOG(sock, ""[+] got auth manager: %lu\n"", authmgr_handle);"

                        Dim DirectoriesToInsert() As String = {
                                $"    {RootDIR}",
                                $"    {SystemExCommonExLIB}",
                                $"    {CommonLIB}",
                                $"    {PrivLIB}",
                                $"    {SystemVSH}",
                                $"    {SystemSYS}"}

                        Dim InsertedBlock As String = TargetLine & Environment.NewLine & String.Join(Environment.NewLine, DirectoriesToInsert)
                        UpdatedContent = UpdatedContent.Replace(TargetLine, InsertedBlock)
                    Else
                        'Remove from source if they exist from a previous build
                        Dim DirectoriesToRemove As String() = {
                                "dump_queue_add_dir(sock, ""/"", 0);",
                                "dump_queue_add_dir(sock, ""/system_ex/common_ex/lib"", 0);",
                                "dump_queue_add_dir(sock, ""/system/common/lib"", 0);",
                                "dump_queue_add_dir(sock, ""/system/priv/lib"", 0);",
                                "dump_queue_add_dir(sock, ""/system/vsh"", 1);",
                                "dump_queue_add_dir(sock, ""/system/sys"", 0);"
                            }

                        Dim SourceLines As New List(Of String)(UpdatedContent.Split({Environment.NewLine}, StringSplitOptions.None))
                        SourceLines = SourceLines.Where(Function(line) Not DirectoriesToRemove.Any(Function(val) line.Trim() = val)).ToList()
                        UpdatedContent = String.Join(Environment.NewLine, SourceLines)
                    End If

                    'Set dump location
                    If SaveToUSBCheckBox.IsChecked Then
                        Dim OldDumpLocation As String = "dump(sock, authmgr_handle, &offsets, ""/data/dump"");"
                        Dim NewDumpLocation As String = "dump(sock, authmgr_handle, &offsets, ""/mnt/usb0/PS5"");"
                        UpdatedContent = UpdatedContent.Replace(OldDumpLocation, NewDumpLocation)
                    ElseIf SaveInternalCheckBox.IsChecked Then
                        Dim OldDumpLocation As String = "dump(sock, authmgr_handle, &offsets, ""/mnt/usb0/PS5"");"
                        Dim NewDumpLocation As String = "dump(sock, authmgr_handle, &offsets, ""/data/dump"");"
                        UpdatedContent = UpdatedContent.Replace(OldDumpLocation, NewDumpLocation)
                    Else
                        'Use default (replace USB by internal)
                        Dim OldDumpLocation As String = "dump(sock, authmgr_handle, &offsets, ""/mnt/usb0/PS5"");"
                        Dim NewDumpLocation As String = "dump(sock, authmgr_handle, &offsets, ""/data/dump"");"
                        UpdatedContent = UpdatedContent.Replace(OldDumpLocation, NewDumpLocation)
                    End If

                    NewContentBytes = Encoding.UTF8.GetBytes(UpdatedContent) 'Final source file

                    'Create JSON for updating the file
                    Dim ContentAsBase64 As String = Convert.ToBase64String(NewContentBytes)
                    Dim UpdateMessage As New Dictionary(Of String, Object) From {
                        {"message", "Update using PS Multi Tools"},
                        {"content", ContentAsBase64},
                        {"sha", FileSHA}
                    }
                    Dim UpdateMessageJSON As String = JsonConvert.SerializeObject(UpdateMessage)
                    Dim NewStringContent As New StringContent(UpdateMessageJSON, Encoding.UTF8, "application/json")

                    'Update the file
                    Dim UpdateResponse = NewHttpClient.PutAsync(GitHubAPIURL, NewStringContent).Result
                    If UpdateResponse.IsSuccessStatusCode Then
                        Return True
                    Else
                        Dim ErrorMessage As String = UpdateResponse.Content.ReadAsStringAsync().Result
                        MsgBox("Error Message: " & ErrorMessage)
                        Return False
                    End If
                End Using
            Catch ex As Exception
                Return False
            End Try
        ElseIf PayloadName = "App Titles" Then
            Dim FileToUpdate As String = "app_title/main.c"
            Dim GitHubAPIURL As String = $"https://api.github.com/repos/{UpdateUser}/{RepoToUpdate}/contents/{FileToUpdate}"

            PayloadBuildProgressStatusTextBlock.Text = "Updating payload, please wait..."
            PayloadBuildProgressBar.IsIndeterminate = True

            Try
                Using NewHttpClient As New HttpClient()
                    NewHttpClient.DefaultRequestHeaders.UserAgent.Add(New ProductInfoHeaderValue("PS-Multi-Tools", "15.4"))
                    NewHttpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("token", MinimalUserToken)

                    'Check if file can be accessed
                    Dim TestResponse = NewHttpClient.GetAsync(GitHubAPIURL).Result
                    If Not TestResponse.IsSuccessStatusCode Then
                        Return False
                    End If

                    'Read existing content
                    Dim ExistingContentJSON As String = TestResponse.Content.ReadAsStringAsync().Result
                    Dim NewFileInfo As Dictionary(Of String, Object) = JsonConvert.DeserializeObject(Of Dictionary(Of String, Object))(ExistingContentJSON)
                    Dim FileSHA As String = NewFileInfo("sha").ToString()
                    Dim NewContentBytes() As Byte

                    'Prepare new content
                    Dim ExistingContentBase64 As String = NewFileInfo("content").ToString()
                    ExistingContentBase64 = ExistingContentBase64.Replace(vbLf, "").Replace(vbCr, "")
                    Dim ExistingContentBytes() As Byte = Convert.FromBase64String(ExistingContentBase64)
                    Dim ExistingFileContentString As String = Encoding.UTF8.GetString(ExistingContentBytes)

                    Dim UpdatedContent As String = ExistingFileContentString 'Base source file

                    'Replace font size
                    If SetFontSizeCheckBox.IsChecked Then
                        Dim NewFontSize As String
                        If Not String.IsNullOrEmpty(FontSizeTextBox.Text) Then
                            NewFontSize = FontSizeTextBox.Text
                        Else
                            NewFontSize = "60" 'Default
                        End If

                        Dim NewRegexPattern As String = "(float\s+scale\s*=\s*stbtt_ScaleForPixelHeight\(&fi,\s*)(\d+)(\s*\);)"
                        Dim ReplacementString As String = "${1}" & NewFontSize & "${3}"
                        UpdatedContent = Regex.Replace(UpdatedContent, NewRegexPattern, ReplacementString)
                    Else
                        'Use default
                        Dim NewRegexPattern As String = "(float\s+scale\s*=\s*stbtt_ScaleForPixelHeight\(&fi,\s*)(\d+)(\s*\);)"
                        Dim ReplacementString As String = "${1}" & "60" & "${3}"
                        UpdatedContent = Regex.Replace(UpdatedContent, NewRegexPattern, ReplacementString)
                    End If

                    'Replace text position
                    If ShowTextOnBottomCheckBox.IsChecked Then
                        Dim BottomTopDrawingPositions As String() = {"int x = (w - total_w)/2, y = ascent + 10;", "int x = (w - total_w)/2, y = 10;"}
                        Dim FoundDrawingPosition As String = BottomTopDrawingPositions.FirstOrDefault(Function(pattern) UpdatedContent.Contains(pattern))
                        'Replace the top or bottom position code
                        If Not String.IsNullOrEmpty(FoundDrawingPosition) Then
                            Dim NewDrawingPosition As String = "int x = (w - total_w)/2, y = ascent + 10;"
                            UpdatedContent = UpdatedContent.Replace(FoundDrawingPosition, NewDrawingPosition)
                        Else
                            'Use default (same)
                            Dim NewDrawingPosition As String = "int x = (w - total_w)/2, y = ascent + 10;"
                            UpdatedContent = UpdatedContent.Replace(FoundDrawingPosition, NewDrawingPosition)
                        End If
                    ElseIf ShowTextOnTopCheckBox.IsChecked Then
                        Dim BottomTopDrawingPositions As String() = {"int x = (w - total_w)/2, y = ascent + 10;", "int x = (w - total_w)/2, y = 10;"}
                        Dim FoundDrawingPosition As String = BottomTopDrawingPositions.FirstOrDefault(Function(pattern) UpdatedContent.Contains(pattern))
                        'Replace the top or bottom position code
                        If Not String.IsNullOrEmpty(FoundDrawingPosition) Then
                            Dim NewDrawingPosition As String = "int x = (w - total_w)/2, y = 10;"
                            UpdatedContent = UpdatedContent.Replace(FoundDrawingPosition, NewDrawingPosition)
                        Else
                            'Use default
                            Dim NewDrawingPosition As String = "int x = (w - total_w)/2, y = ascent + 10;"
                            UpdatedContent = UpdatedContent.Replace(FoundDrawingPosition, NewDrawingPosition)
                        End If
                    ElseIf ShowTextOnLeftCheckBox.IsChecked Then
                        Dim DefaultLeftRightDrawingPositions As String() = {"draw_text(img,w,h,e->d_name);", "draw_text_left(img,w,h,e->d_name);", "draw_text_right(img,w,h,e->d_name);"}
                        Dim FoundDrawingCall As String = DefaultLeftRightDrawingPositions.FirstOrDefault(Function(pattern) UpdatedContent.Contains(pattern))
                        'Replace the draw call
                        If Not String.IsNullOrEmpty(FoundDrawingCall) Then
                            Dim NewDrawingCall As String = "draw_text_left(img,w,h,e->d_name);"
                            UpdatedContent = UpdatedContent.Replace(FoundDrawingCall, NewDrawingCall)
                        Else
                            'Use default
                            Dim NewDrawingCall As String = "draw_text(img,w,h,e->d_name);"
                            UpdatedContent = UpdatedContent.Replace(FoundDrawingCall, NewDrawingCall)
                        End If
                    ElseIf ShowTextOnRightCheckBox.IsChecked Then
                        Dim DefaultLeftRightDrawingPositions As String() = {"draw_text(img,w,h,e->d_name);", "draw_text_left(img,w,h,e->d_name);", "draw_text_right(img,w,h,e->d_name);"}
                        Dim FoundDrawingCall As String = DefaultLeftRightDrawingPositions.FirstOrDefault(Function(pattern) UpdatedContent.Contains(pattern))
                        'Replace the draw call
                        If Not String.IsNullOrEmpty(FoundDrawingCall) Then
                            Dim NewDrawingCall As String = "draw_text_right(img,w,h,e->d_name);"
                            UpdatedContent = UpdatedContent.Replace(FoundDrawingCall, NewDrawingCall)
                        Else
                            'Use default
                            Dim NewDrawingCall As String = "draw_text(img,w,h,e->d_name);"
                            UpdatedContent = UpdatedContent.Replace(FoundDrawingCall, NewDrawingCall)
                        End If
                    Else
                        'Use default
                        Dim BottomTopDrawingPositions As String() = {"int x = (w - total_w)/2, y = ascent + 10;", "int x = (w - total_w)/2, y = 10;"}
                        Dim DefaultLeftRightDrawingPositions As String() = {"draw_text(img,w,h,e->d_name);", "draw_text_left(img,w,h,e->d_name);", "draw_text_right(img,w,h,e->d_name);"}
                        Dim FoundDrawingPosition As String = BottomTopDrawingPositions.FirstOrDefault(Function(pattern) UpdatedContent.Contains(pattern))
                        Dim FoundDrawingCall As String = DefaultLeftRightDrawingPositions.FirstOrDefault(Function(pattern) UpdatedContent.Contains(pattern))
                        Dim DefaultDrawingPosition As String = "int x = (w - total_w)/2, y = ascent + 10;"
                        Dim DefaultDrawingCall As String = "draw_text(img,w,h,e->d_name);"

                        UpdatedContent = UpdatedContent.Replace(FoundDrawingPosition, DefaultDrawingPosition)
                        UpdatedContent = UpdatedContent.Replace(FoundDrawingCall, DefaultDrawingCall)
                    End If

                    NewContentBytes = Encoding.UTF8.GetBytes(UpdatedContent) 'Final source file

                    'Create JSON for updating the file
                    Dim ContentAsBase64 As String = Convert.ToBase64String(NewContentBytes)
                    Dim UpdateMessage As New Dictionary(Of String, Object) From {
                        {"message", "Update using PS Multi Tools"},
                        {"content", ContentAsBase64},
                        {"sha", FileSHA}
                    }
                    Dim UpdateMessageJSON As String = JsonConvert.SerializeObject(UpdateMessage)
                    Dim NewStringContent As New StringContent(UpdateMessageJSON, Encoding.UTF8, "application/json")

                    'Update the file
                    Dim UpdateResponse = NewHttpClient.PutAsync(GitHubAPIURL, NewStringContent).Result
                    If UpdateResponse.IsSuccessStatusCode Then
                        Return True
                    Else
                        Dim ErrorMessage As String = UpdateResponse.Content.ReadAsStringAsync().Result
                        MsgBox("Error Message: " & ErrorMessage)
                        Return False
                    End If
                End Using
            Catch ex As Exception
                Return False
            End Try
        Else
            Return False
        End If
    End Function

    Private Async Function RunWorkflow(PayloadName As String) As Task(Of Boolean)
        'Run the workflow
        Dim WorkflowFile As String
        If PayloadName = "PS5 SELF Decrypter" Then
            WorkflowFile = "ps5_self_decrypter.yml"
        ElseIf PayloadName = "App Titles" Then
            WorkflowFile = "app_title.yml"
        Else
            Return False
        End If

        Dim GitHubAPIURL As String = $"https://api.github.com/repos/{UpdateUser}/{RepoToUpdate}/actions/workflows/{WorkflowFile}/dispatches"
        Dim WorkflowPayload As New Dictionary(Of String, Object) From {{"ref", RepoBranch}}
        Dim WorkflowPayloadJSON As String = JsonConvert.SerializeObject(WorkflowPayload)

        Try
            Using NewHttpClient As New HttpClient()
                NewHttpClient.DefaultRequestHeaders.UserAgent.Add(New ProductInfoHeaderValue("PS-Multi-Tools", "15.4"))
                NewHttpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("token", MinimalUserToken)

                Dim NewStringContent As New StringContent(WorkflowPayloadJSON, Encoding.UTF8, "application/json")
                Dim WorkflowResponse = Await NewHttpClient.PostAsync(GitHubAPIURL, NewStringContent)
                If WorkflowResponse.IsSuccessStatusCode Then
                    Return True
                Else
                    Return False
                End If
            End Using
        Catch ex As Exception
            Return False
        End Try
    End Function

    Private Async Function GetLatestWorkflowRunId(PayloadName As String) As Task(Of Long)
        'Get the workflow run id for monitoring the status
        Dim WorkflowFile As String
        If PayloadName = "PS5 SELF Decrypter" Then
            WorkflowFile = "ps5_self_decrypter.yml"
        ElseIf PayloadName = "App Titles" Then
            WorkflowFile = "app_title.yml"
        Else
            Return 0
        End If

        Dim GitHubAPIURL As String = $"https://api.github.com/repos/{UpdateUser}/{RepoToUpdate}/actions/workflows/{WorkflowFile}/runs?branch={RepoBranch}&event=workflow_dispatch"
        Try
            Using NewHttpClient As New HttpClient()
                NewHttpClient.DefaultRequestHeaders.UserAgent.Add(New ProductInfoHeaderValue("PS-Multi-Tools", "15.4"))
                NewHttpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("token", MinimalUserToken)

                Dim WorkflowRunIDResponse = Await NewHttpClient.GetAsync(GitHubAPIURL)
                WorkflowRunIDResponse.EnsureSuccessStatusCode()
                Dim JSONContent As String = Await WorkflowRunIDResponse.Content.ReadAsStringAsync()
                Dim NewJSONObject As JObject = JObject.Parse(JSONContent)
                Dim RunsJSONArray As JArray = CType(NewJSONObject("workflow_runs"), JArray)

                If RunsJSONArray IsNot Nothing AndAlso RunsJSONArray.Count > 0 Then
                    Dim LatestRun As JObject = CType(RunsJSONArray(0), JObject)
                    Dim RunID As Long = LatestRun("id").ToObject(Of Long)()
                    Return RunID
                Else
                    Return 0
                End If
            End Using
        Catch ex As Exception
            Return 0
        End Try
    End Function

    Private Async Function MonitorAndDownloadArtifact(RunID As Long) As Task(Of String)
        'Monitor the status of the payload build process
        Try
            Using NewHttpClient As New HttpClient()
                NewHttpClient.DefaultRequestHeaders.UserAgent.Add(New ProductInfoHeaderValue("PS-Multi-Tools", "15.4"))
                NewHttpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("token", MinimalUserToken)

                Dim WorkflowURL As String = $"https://api.github.com/repos/{UpdateUser}/{RepoToUpdate}/actions/runs/{RunID}"
                Dim ArtifactsURL As String = $"https://api.github.com/repos/{UpdateUser}/{RepoToUpdate}/actions/runs/{RunID}/artifacts"

                Dim BuildCompleted As Boolean = False
                Do
                    Try
                        Dim RunResponse As HttpResponseMessage = Await NewHttpClient.GetAsync(WorkflowURL)
                        RunResponse.EnsureSuccessStatusCode()
                        Dim RunContent As String = Await RunResponse.Content.ReadAsStringAsync()
                        Dim RunJSON As JObject = JObject.Parse(RunContent)
                        Dim RunStatus As String = RunJSON("status").ToString()

                        If RunStatus.Equals("completed", StringComparison.OrdinalIgnoreCase) Then
                            BuildCompleted = True
                            Exit Do
                        End If
                    Catch ex As Exception
                        MsgBox("Error checking workflow status: " & ex.Message)
                    End Try
                    Await Task.Delay(8000) 'Check every 8secs until the run is completed
                Loop Until BuildCompleted

                ' Once the run is completed, retrieve the artifact list
                Try
                    Dim ArtifactsResponse As HttpResponseMessage = Await NewHttpClient.GetAsync(ArtifactsURL)
                    ArtifactsResponse.EnsureSuccessStatusCode()
                    Dim ArtifactsContent As String = Await ArtifactsResponse.Content.ReadAsStringAsync()
                    Dim ArtifactsJSON As JObject = JObject.Parse(ArtifactsContent)
                    Dim ArtifactsArray As JArray = CType(ArtifactsJSON("artifacts"), JArray)

                    If ArtifactsArray IsNot Nothing AndAlso ArtifactsArray.Count > 0 Then
                        Dim Artifact As JObject = CType(ArtifactsArray(0), JObject)
                        Dim DownloadURL As String = Artifact("archive_download_url").ToString()
                        Return DownloadURL
                    Else
                        Return ""
                    End If

                Catch ex As Exception
                    MsgBox("Error retrieving or downloading artifacts: " & ex.Message)
                    Return ""
                End Try
            End Using
        Catch ex As Exception
            Return ""
        End Try
    End Function

    Private Async Sub BuildPayloadButton_Click(sender As Object, e As RoutedEventArgs) Handles BuildPayloadButton.Click
        'Double check for any running workflow before proceeding
        Dim ActiveSELFDecrypterWorkflows As Integer = Await CheckForActiveWorkflow("PS5 SELF Decrypter")
        Dim ActiveAppTitlesWorkflows As Integer = Await CheckForActiveWorkflow("App Titles")
        Dim CurrentRepoState As String = GetSlotState()

        If BuildPayloadButton.Content.ToString() = "Build Payload" AndAlso (ActiveSELFDecrypterWorkflows + ActiveAppTitlesWorkflows) > 0 AndAlso Not CurrentRepoState = "reserved" Then
            MsgBox("A payload is currently being built by another user. Please try again in 1 minute.")
        Else
            If SelectedPayloadComboBox.SelectedIndex = 0 Then
                ReserveSlot("Reserve")
                'Commit the changes for the selected payload
                If CommitChanges("PS5 SELF Decrypter") Then
                    BuildPayloadButton.IsEnabled = False
                    SelectedPayloadComboBox.IsEnabled = False
                    PayloadBuildProgressStatusTextBlock.Text = "Payload updated, starting build"

                    'Run the build action for the selected payload
                    Dim RunNewWorkflowSucceeded As Boolean = Await RunWorkflow("PS5 SELF Decrypter")
                    If RunNewWorkflowSucceeded Then

                        PayloadBuildProgressStatusTextBlock.Text = "Build started, getting the build process id"

                        Threading.Thread.Sleep(10000) 'Wait 10sec before getting the latest run id

                        'Get the latest workflow run id
                        Dim RetrievedWorkflowID As Long = Await GetLatestWorkflowRunId("PS5 SELF Decrypter")
                        If RetrievedWorkflowID <> 0 Then

                            PayloadBuildProgressStatusTextBlock.Text = "Building payload, please wait..."

                            'Monitor the build process and get the produced artifact URL
                            Dim CreatedArtifcatURL As String = Await MonitorAndDownloadArtifact(RetrievedWorkflowID)
                            If Not String.IsNullOrEmpty(CreatedArtifcatURL) Then

                                PayloadBuildProgressStatusTextBlock.Text = "Build succeeded! Saving payload to \Downloads ..."
                                ReserveSlot("Free")

                                'Create Downloads directory if not exists
                                If Not Directory.Exists(Environment.CurrentDirectory + "\Downloads") Then Directory.CreateDirectory(Environment.CurrentDirectory + "\Downloads")
                                'Remove previously created payload
                                If File.Exists(Environment.CurrentDirectory + "\Downloads\ps5-self-decrypter-payload.zip") Then File.Delete(Environment.CurrentDirectory + "\Downloads\ps5-self-decrypter-payload.zip")

                                'Save the payload
                                Try
                                    Dim NewHttpClientHandler As New HttpClientHandler() With {.AllowAutoRedirect = False}
                                    Using NewHttpClient As New HttpClient(NewHttpClientHandler)
                                        NewHttpClient.DefaultRequestHeaders.UserAgent.Add(New ProductInfoHeaderValue("PS-Multi-Tools", "15.4"))
                                        NewHttpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("token", MinimalUserToken)

                                        Dim response = Await NewHttpClient.GetAsync(CreatedArtifcatURL)
                                        If response.StatusCode = System.Net.HttpStatusCode.Redirect OrElse response.StatusCode = System.Net.HttpStatusCode.TemporaryRedirect Then
                                            Dim RedirectedURL = response.Headers.Location.ToString()
                                            Using DLClient As New HttpClient()
                                                DLClient.DefaultRequestHeaders.UserAgent.Add(New ProductInfoHeaderValue("PS-Multi-Tools", "15.4"))
                                                Dim finalResponse = Await DLClient.GetByteArrayAsync(RedirectedURL)
                                                File.WriteAllBytes(Environment.CurrentDirectory + "\Downloads\ps5-self-decrypter-payload.zip", finalResponse)
                                            End Using
                                        ElseIf response.IsSuccessStatusCode Then
                                            Dim artifactZipBytes As Byte() = Await response.Content.ReadAsByteArrayAsync()
                                            File.WriteAllBytes(Environment.CurrentDirectory + "\Downloads\ps5-self-decrypter-payload.zip", artifactZipBytes)
                                        End If
                                    End Using

                                    BuildPayloadButton.IsEnabled = True
                                    SelectedPayloadComboBox.IsEnabled = True
                                    PayloadBuildProgressStatusTextBlock.Text = "Payload saved to \Downloads"
                                    PayloadBuildProgressBar.IsIndeterminate = False

                                    If MsgBox("Payload build and saved!" + vbCrLf + "The payload is saved as zip that can be extracted, do you want to extract it now ?", MsgBoxStyle.YesNo, "Extract payload ?") = MsgBoxResult.Yes Then
                                        'Check and extract the downloaded archive into the downloads folder
                                        If File.Exists(Environment.CurrentDirectory + "\Downloads\ps5-self-decrypter-payload.zip") Then

                                            Using ArchiveExtractor As New Process()
                                                ArchiveExtractor.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\7z.exe"
                                                ArchiveExtractor.StartInfo.Arguments = "x """ + Environment.CurrentDirectory + "\Downloads\ps5-self-decrypter-payload.zip""" +
                                                " -o""" + Environment.CurrentDirectory + "\Downloads\" + """"
                                                ArchiveExtractor.StartInfo.UseShellExecute = False
                                                ArchiveExtractor.StartInfo.CreateNoWindow = True
                                                ArchiveExtractor.Start()
                                                ArchiveExtractor.WaitForExit()
                                            End Using

                                            If MsgBox("Extraction done!" + vbCrLf + "Do you want to open the Downloads folder ?", MsgBoxStyle.YesNo, "Completed") = MsgBoxResult.Yes Then
                                                Process.Start("explorer", Environment.CurrentDirectory + "\Downloads")
                                            End If
                                        Else
                                            MsgBox("Could not find the saved payload.", MsgBoxStyle.Critical, "Error while extracting")
                                            If MsgBox("Do you want to open the Downloads folder ?", MsgBoxStyle.YesNo, "Completed") = MsgBoxResult.Yes Then
                                                Process.Start("explorer", Environment.CurrentDirectory + "\Downloads")
                                            End If
                                        End If
                                    Else
                                        If MsgBox("Do you want to open the Downloads folder ?", MsgBoxStyle.YesNo, "Completed") = MsgBoxResult.Yes Then
                                            Process.Start("explorer", Environment.CurrentDirectory + "\Downloads")
                                        End If
                                    End If
                                Catch ex As Exception
                                    MsgBox("Error downloading the payload.", MsgBoxStyle.Critical)
                                    BuildPayloadButton.IsEnabled = True
                                    SelectedPayloadComboBox.IsEnabled = True
                                End Try
                            Else
                                MsgBox("Error getting the created payload url.", MsgBoxStyle.Critical)
                                BuildPayloadButton.IsEnabled = True
                                SelectedPayloadComboBox.IsEnabled = True
                            End If
                        Else
                            MsgBox("Error retrieving the workflow id.", MsgBoxStyle.Critical)
                            BuildPayloadButton.IsEnabled = True
                            SelectedPayloadComboBox.IsEnabled = True
                        End If
                    Else
                        MsgBox("Error building the payload.", MsgBoxStyle.Critical)
                        BuildPayloadButton.IsEnabled = True
                        SelectedPayloadComboBox.IsEnabled = True
                    End If
                Else
                    ReserveSlot("Free")
                    MsgBox("Error updating the payload source.", MsgBoxStyle.Critical)
                End If

            ElseIf SelectedPayloadComboBox.SelectedIndex = 1 Then
                If RestoreCheckBox.IsChecked Then
                    'Restore by getting icon .dds files & converting to .png, then syncing back to the PS5

                    Dim PS5IP As String = PS5FTPIPTextBox.Text
                    Dim PS5Port As Integer = CInt(PS5FTPPortTextBox.Text)
                    Dim CacheFolder As String = Environment.CurrentDirectory + "\Cache\UserAppMeta"

                    Await Task.Run(Sub()
                                       Dispatcher.BeginInvoke(Sub()
                                                                  BuildPayloadButton.IsEnabled = False
                                                                  SelectedPayloadComboBox.IsEnabled = False
                                                                  PayloadBuildProgressStatusTextBlock.Text = "Getting /user/appmeta..."
                                                                  PayloadBuildProgressBar.IsIndeterminate = True
                                                              End Sub)

                                       'Dump /user/appmeta to get the DDS files
                                       Dim NewSessionOptions As New SessionOptions
                                       With NewSessionOptions
                                           .Protocol = Protocol.Ftp
                                           .HostName = PS5IP
                                           .UserName = "anonymous"
                                           .Password = "anonymous"
                                           .PortNumber = PS5Port
                                           .FtpMode = FtpMode.Passive
                                           .FtpSecure = FtpSecure.None
                                           .Secure = False
                                       End With

                                       'Create cache
                                       If Not Directory.Exists(CacheFolder) Then
                                           Directory.CreateDirectory(CacheFolder)
                                       Else
                                           Directory.Delete(CacheFolder, True)
                                           Directory.CreateDirectory(CacheFolder)
                                       End If

                                       'Sync content of /user/appmeta to .\Cache\UserAppMeta
                                       Dim NewSession As New Session()
                                       NewSession.Open(NewSessionOptions)
                                       UserAppMetaSynced = NewSession.SynchronizeDirectories(SynchronizationMode.Local, CacheFolder, "/user/appmeta", False)

                                       'Throw on any error
                                       UserAppMetaSynced.Check()
                                       Dispatcher.BeginInvoke(Sub() PayloadBuildProgressStatusTextBlock.Text = "Saved /user/appmeta. Converting .dds files...")

                                       'Check result
                                       If UserAppMetaSynced.IsSuccess Then
                                           'Check if there's any appmeta
                                           If Directory.EnumerateFileSystemEntries(CacheFolder).Any() Then
                                               'Convert all DDS icons to icon0.png
                                               For Each DumpedAppMeta In Directory.GetDirectories(CacheFolder)
                                                   If File.Exists(DumpedAppMeta + "\icon0.dds") Then
                                                       Dim IconToConvert As String = DumpedAppMeta + "\icon0.dds"
                                                       Dim OutputFileName As String = DumpedAppMeta + "\icon0.png"
                                                       Try
                                                           Using NewPNGImage As New MagickImage(IconToConvert)
                                                               NewPNGImage.SetCompression(CompressionMethod.NoCompression)
                                                               NewPNGImage.Format = MagickFormat.Png
                                                               NewPNGImage.Write(OutputFileName)
                                                           End Using
                                                       Catch ex As Exception
                                                           MsgBox(ex.Message)
                                                       End Try
                                                   Else
                                                       'To do
                                                   End If
                                               Next

                                               Dispatcher.BeginInvoke(Sub() PayloadBuildProgressStatusTextBlock.Text = "Syncing PNG icons back to the PS5...")

                                               'Sync back from .\Cache\UserAppMeta to /user/appmeta
                                               UserAppMetaSynced = Nothing
                                               UserAppMetaSynced = NewSession.SynchronizeDirectories(SynchronizationMode.Remote, CacheFolder, "/user/appmeta", False, criteria:=SynchronizationCriteria.Size)

                                               'Throw on any error
                                               UserAppMetaSynced.Check()
                                               Dispatcher.BeginInvoke(Sub() PayloadBuildProgressBar.IsIndeterminate = False)

                                               'Check result
                                               If UserAppMetaSynced.IsSuccess Then
                                                   Dispatcher.BeginInvoke(Sub()
                                                                              PayloadBuildProgressStatusTextBlock.Text = "Icons restored! Please restart your PS5."
                                                                              BuildPayloadButton.IsEnabled = True
                                                                              SelectedPayloadComboBox.IsEnabled = True
                                                                          End Sub)
                                               Else
                                                   Dispatcher.BeginInvoke(Sub()
                                                                              PayloadBuildProgressStatusTextBlock.Text = "Failed to restore the icons back to the PS5."
                                                                              BuildPayloadButton.IsEnabled = True
                                                                              SelectedPayloadComboBox.IsEnabled = True
                                                                          End Sub)
                                               End If

                                               'Close
                                               NewSession.Close()
                                           Else
                                               Dispatcher.BeginInvoke(Sub()
                                                                          PayloadBuildProgressStatusTextBlock.Text = "Failed getting files from the PS5."
                                                                          PayloadBuildProgressBar.IsIndeterminate = False
                                                                          BuildPayloadButton.IsEnabled = True
                                                                          SelectedPayloadComboBox.IsEnabled = True
                                                                      End Sub)
                                               MsgBox("Could not find any dumped appmeta.", MsgBoxStyle.Critical)
                                           End If
                                       Else
                                           Dispatcher.BeginInvoke(Sub()
                                                                      PayloadBuildProgressStatusTextBlock.Text = "Failed getting files from the PS5."
                                                                      PayloadBuildProgressBar.IsIndeterminate = False
                                                                      BuildPayloadButton.IsEnabled = True
                                                                      SelectedPayloadComboBox.IsEnabled = True
                                                                  End Sub)
                                           MsgBox("Could not dump any appmeta.", MsgBoxStyle.Critical)
                                       End If
                                   End Sub)
                Else
                    ReserveSlot("Reserve")
                    'Commit the changes for the selected payload
                    If CommitChanges("App Titles") Then

                        BuildPayloadButton.IsEnabled = False
                        SelectedPayloadComboBox.IsEnabled = False
                        PayloadBuildProgressStatusTextBlock.Text = "Payload updated, starting build"

                        'Run the build action for the selected payload
                        Dim RunNewWorkflowSucceeded As Boolean = Await RunWorkflow("App Titles")
                        If RunNewWorkflowSucceeded Then

                            PayloadBuildProgressStatusTextBlock.Text = "Build started, getting the build process id"

                            Threading.Thread.Sleep(10000) 'Wait 10sec before getting the latest run id

                            'Get the latest workflow run id
                            Dim RetrievedWorkflowID As Long = Await GetLatestWorkflowRunId("App Titles")
                            If RetrievedWorkflowID <> 0 Then

                                PayloadBuildProgressStatusTextBlock.Text = "Building payload, please wait..."

                                'Monitor the build process and get the produced artifact URL
                                Dim CreatedArtifcatURL As String = Await MonitorAndDownloadArtifact(RetrievedWorkflowID)
                                If Not String.IsNullOrEmpty(CreatedArtifcatURL) Then

                                    PayloadBuildProgressStatusTextBlock.Text = "Build succeeded! Saving payload to \Downloads ..."
                                    ReserveSlot("Free")

                                    'Create Downloads directory if not exists
                                    If Not Directory.Exists(Environment.CurrentDirectory + "\Downloads") Then Directory.CreateDirectory(Environment.CurrentDirectory + "\Downloads")
                                    'Remove previously created payload
                                    If File.Exists(Environment.CurrentDirectory + "\Downloads\app_title_payload.zip") Then File.Delete(Environment.CurrentDirectory + "\Downloads\app_title_payload.zip")

                                    'Save the payload
                                    Try
                                        Dim NewHttpClientHandler As New HttpClientHandler() With {.AllowAutoRedirect = False}
                                        Using NewHttpClient As New HttpClient(NewHttpClientHandler)
                                            NewHttpClient.DefaultRequestHeaders.UserAgent.Add(New ProductInfoHeaderValue("PS-Multi-Tools", "15.4"))
                                            NewHttpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("token", MinimalUserToken)

                                            Dim response = Await NewHttpClient.GetAsync(CreatedArtifcatURL)
                                            If response.StatusCode = System.Net.HttpStatusCode.Redirect OrElse response.StatusCode = System.Net.HttpStatusCode.TemporaryRedirect Then
                                                Dim RedirectedURL = response.Headers.Location.ToString()
                                                Using DLClient As New HttpClient()
                                                    DLClient.DefaultRequestHeaders.UserAgent.Add(New ProductInfoHeaderValue("PS-Multi-Tools", "15.4"))
                                                    Dim finalResponse = Await DLClient.GetByteArrayAsync(RedirectedURL)
                                                    File.WriteAllBytes(Environment.CurrentDirectory + "\Downloads\app_title_payload.zip", finalResponse)
                                                End Using
                                            ElseIf response.IsSuccessStatusCode Then
                                                Dim artifactZipBytes As Byte() = Await response.Content.ReadAsByteArrayAsync()
                                                File.WriteAllBytes(Environment.CurrentDirectory + "\Downloads\app_title_payload.zip", artifactZipBytes)
                                            End If
                                        End Using

                                        BuildPayloadButton.IsEnabled = True
                                        SelectedPayloadComboBox.IsEnabled = True
                                        PayloadBuildProgressStatusTextBlock.Text = "Payload saved to \Downloads"
                                        PayloadBuildProgressBar.IsIndeterminate = False

                                        If MsgBox("Payload build and saved!" + vbCrLf + "The payload is saved as zip that can be extracted, do you want to extract it now ?", MsgBoxStyle.YesNo, "Extract payload ?") = MsgBoxResult.Yes Then
                                            'Check and extract the downloaded archive into the downloads folder
                                            If File.Exists(Environment.CurrentDirectory + "\Downloads\app_title_payload.zip") Then

                                                Using ArchiveExtractor As New Process()
                                                    ArchiveExtractor.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\7z.exe"
                                                    ArchiveExtractor.StartInfo.Arguments = "x """ + Environment.CurrentDirectory + "\Downloads\app_title_payload.zip""" +
                                                        " -o""" + Environment.CurrentDirectory + "\Downloads\" + """"
                                                    ArchiveExtractor.StartInfo.UseShellExecute = False
                                                    ArchiveExtractor.StartInfo.CreateNoWindow = True
                                                    ArchiveExtractor.Start()
                                                    ArchiveExtractor.WaitForExit()
                                                End Using

                                                If MsgBox("Extraction done!" + vbCrLf + "Do you want to open the Downloads folder ?", MsgBoxStyle.YesNo, "Completed") = MsgBoxResult.Yes Then
                                                    Process.Start("explorer", Environment.CurrentDirectory + "\Downloads")
                                                End If
                                            Else
                                                MsgBox("Could not find the saved payload.", MsgBoxStyle.Critical, "Error while extracting")
                                                If MsgBox("Do you want to open the Downloads folder ?", MsgBoxStyle.YesNo, "Completed") = MsgBoxResult.Yes Then
                                                    Process.Start("explorer", Environment.CurrentDirectory + "\Downloads")
                                                End If
                                            End If
                                        Else
                                            If MsgBox("Do you want to open the Downloads folder ?", MsgBoxStyle.YesNo, "Completed") = MsgBoxResult.Yes Then
                                                Process.Start("explorer", Environment.CurrentDirectory + "\Downloads")
                                            End If
                                        End If
                                    Catch ex As Exception
                                        MsgBox("Error downloading the payload.", MsgBoxStyle.Critical)
                                        BuildPayloadButton.IsEnabled = True
                                        SelectedPayloadComboBox.IsEnabled = True
                                    End Try

                                Else
                                    MsgBox("Error getting the created payload url.", MsgBoxStyle.Critical)
                                    BuildPayloadButton.IsEnabled = True
                                    SelectedPayloadComboBox.IsEnabled = True
                                End If

                            Else
                                MsgBox("Error retrieving the workflow id.", MsgBoxStyle.Critical)
                                BuildPayloadButton.IsEnabled = True
                                SelectedPayloadComboBox.IsEnabled = True
                            End If

                        Else
                            MsgBox("Error building the payload.", MsgBoxStyle.Critical)
                            BuildPayloadButton.IsEnabled = True
                            SelectedPayloadComboBox.IsEnabled = True
                        End If

                    Else
                        ReserveSlot("Free")
                        MsgBox("Error updating the payload source.", MsgBoxStyle.Critical)
                    End If
                End If
            Else
                MsgBox("No payload selected.", MsgBoxStyle.Critical)
            End If
        End If
    End Sub

    Private Sub SelectedPayloadComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles SelectedPayloadComboBox.SelectionChanged
        If SelectedPayloadComboBox.SelectedIndex = 0 Then
            AppTitleOptionsGrid.Visibility = Visibility.Hidden
            SELFDecrypterOptionsGrid.Visibility = Visibility.Visible
        ElseIf SelectedPayloadComboBox.SelectedIndex = 1 Then
            SELFDecrypterOptionsGrid.Visibility = Visibility.Hidden
            AppTitleOptionsGrid.Visibility = Visibility.Visible
        Else
            AppTitleOptionsGrid.Visibility = Visibility.Hidden
            SELFDecrypterOptionsGrid.Visibility = Visibility.Hidden
        End If
    End Sub

    Private Sub SaveToUSBCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles SaveToUSBCheckBox.Checked
        SaveInternalCheckBox.IsEnabled = False
    End Sub

    Private Sub SaveInternalCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles SaveInternalCheckBox.Checked
        SaveToUSBCheckBox.IsEnabled = False
    End Sub

    Private Sub SaveToUSBCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles SaveToUSBCheckBox.Unchecked
        SaveInternalCheckBox.IsEnabled = True
    End Sub

    Private Sub SaveInternalCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles SaveInternalCheckBox.Unchecked
        SaveToUSBCheckBox.IsEnabled = True
    End Sub

    Private Sub ShowTextOnBottomCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles ShowTextOnBottomCheckBox.Checked
        RestoreCheckBox.IsEnabled = False
        ShowTextOnTopCheckBox.IsEnabled = False
        ShowTextOnLeftCheckBox.IsEnabled = False
        ShowTextOnRightCheckBox.IsEnabled = False
    End Sub

    Private Sub ShowTextOnTopCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles ShowTextOnTopCheckBox.Checked
        RestoreCheckBox.IsEnabled = False
        ShowTextOnBottomCheckBox.IsEnabled = False
        ShowTextOnLeftCheckBox.IsEnabled = False
        ShowTextOnRightCheckBox.IsEnabled = False
    End Sub

    Private Sub ShowTextOnLeftCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles ShowTextOnLeftCheckBox.Checked
        RestoreCheckBox.IsEnabled = False
        ShowTextOnBottomCheckBox.IsEnabled = False
        ShowTextOnTopCheckBox.IsEnabled = False
        ShowTextOnRightCheckBox.IsEnabled = False
    End Sub

    Private Sub ShowTextOnRightCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles ShowTextOnRightCheckBox.Checked
        RestoreCheckBox.IsEnabled = False
        ShowTextOnTopCheckBox.IsEnabled = False
        ShowTextOnLeftCheckBox.IsEnabled = False
        ShowTextOnBottomCheckBox.IsEnabled = False
    End Sub

    Private Sub ShowTextOnBottomCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles ShowTextOnBottomCheckBox.Unchecked
        RestoreCheckBox.IsEnabled = True
        ShowTextOnTopCheckBox.IsEnabled = True
        ShowTextOnLeftCheckBox.IsEnabled = True
        ShowTextOnRightCheckBox.IsEnabled = True
    End Sub

    Private Sub ShowTextOnTopCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles ShowTextOnTopCheckBox.Unchecked
        RestoreCheckBox.IsEnabled = True
        ShowTextOnBottomCheckBox.IsEnabled = True
        ShowTextOnLeftCheckBox.IsEnabled = True
        ShowTextOnRightCheckBox.IsEnabled = True
    End Sub

    Private Sub ShowTextOnLeftCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles ShowTextOnLeftCheckBox.Unchecked
        RestoreCheckBox.IsEnabled = True
        ShowTextOnBottomCheckBox.IsEnabled = True
        ShowTextOnTopCheckBox.IsEnabled = True
        ShowTextOnRightCheckBox.IsEnabled = True
    End Sub

    Private Sub ShowTextOnRightCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles ShowTextOnRightCheckBox.Unchecked
        RestoreCheckBox.IsEnabled = True
        ShowTextOnTopCheckBox.IsEnabled = True
        ShowTextOnLeftCheckBox.IsEnabled = True
        ShowTextOnBottomCheckBox.IsEnabled = True
    End Sub

    Private Sub RestoreCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles RestoreCheckBox.Checked
        SetFontSizeCheckBox.IsEnabled = False
        ShowTextOnTopCheckBox.IsEnabled = False
        ShowTextOnBottomCheckBox.IsEnabled = False
        ShowTextOnLeftCheckBox.IsEnabled = False
        ShowTextOnRightCheckBox.IsEnabled = False

        BuildPayloadButton.Content = "Reset Payload"
    End Sub

    Private Sub RestoreCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles RestoreCheckBox.Unchecked
        SetFontSizeCheckBox.IsEnabled = True
        ShowTextOnTopCheckBox.IsEnabled = True
        ShowTextOnBottomCheckBox.IsEnabled = True
        ShowTextOnLeftCheckBox.IsEnabled = True
        ShowTextOnRightCheckBox.IsEnabled = True

        BuildPayloadButton.Content = "Build Payload"
    End Sub

    Private Sub ReserveSlot(SlotAction As String)
        Dim FileToUpdate As String = "slot.txt"
        Dim GitHubAPIURL As String = $"https://api.github.com/repos/{UpdateUser}/{RepoToUpdate}/contents/{FileToUpdate}"
        Try
            Using NewHttpClient As New HttpClient()
                NewHttpClient.DefaultRequestHeaders.UserAgent.Add(New ProductInfoHeaderValue("PS-Multi-Tools", "15.4"))
                NewHttpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("token", MinimalUserToken)

                'Read existing content
                Dim ContentResponse = NewHttpClient.GetAsync(GitHubAPIURL).Result
                Dim ExistingContentJSON As String = ContentResponse.Content.ReadAsStringAsync().Result
                Dim NewFileInfo As Dictionary(Of String, Object) = JsonConvert.DeserializeObject(Of Dictionary(Of String, Object))(ExistingContentJSON)
                Dim FileSHA As String = NewFileInfo("sha").ToString()
                Dim NewContentBytes() As Byte

                Dim UpdatedContent As String = ""
                If SlotAction = "Reserve" Then
                    UpdatedContent = "reserved"
                Else
                    UpdatedContent = "free"
                End If

                NewContentBytes = Encoding.UTF8.GetBytes(UpdatedContent) 'Final source file

                'Create JSON for updating the file
                Dim ContentAsBase64 As String = Convert.ToBase64String(NewContentBytes)
                Dim UpdateMessage As New Dictionary(Of String, Object) From {
                        {"message", "Update using PS Multi Tools"},
                        {"content", ContentAsBase64},
                        {"sha", FileSHA}
                    }
                Dim UpdateMessageJSON As String = JsonConvert.SerializeObject(UpdateMessage)
                Dim NewStringContent As New StringContent(UpdateMessageJSON, Encoding.UTF8, "application/json")

                'Update the file
                Dim UpdateResponse = NewHttpClient.PutAsync(GitHubAPIURL, NewStringContent).Result
                If Not UpdateResponse.IsSuccessStatusCode Then
                    Dim ErrorMessage As String = UpdateResponse.Content.ReadAsStringAsync().Result
                    MsgBox("Error Message: " & ErrorMessage)
                End If
            End Using
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Function GetSlotState() As String
        Dim GitHubAPIURL As String = $"https://api.github.com/repos/{UpdateUser}/{RepoToUpdate}/contents/slot.txt"
        Try
            Using NewHttpClient As New HttpClient()
                NewHttpClient.DefaultRequestHeaders.UserAgent.Add(New ProductInfoHeaderValue("PS-Multi-Tools", "15.4"))
                NewHttpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("token", MinimalUserToken)

                'Read existing content
                Dim ContentResponse = NewHttpClient.GetAsync(GitHubAPIURL).Result
                Dim ExistingContentJSON As String = ContentResponse.Content.ReadAsStringAsync().Result
                Dim NewFileInfo As Dictionary(Of String, Object) = JsonConvert.DeserializeObject(Of Dictionary(Of String, Object))(ExistingContentJSON)
                Dim FileSHA As String = NewFileInfo("sha").ToString()
                Dim ExistingContentBase64 As String = NewFileInfo("content").ToString()
                ExistingContentBase64 = ExistingContentBase64.Replace(vbLf, "").Replace(vbCr, "")
                Dim ExistingContentBytes() As Byte = Convert.FromBase64String(ExistingContentBase64)
                Dim ExistingFileContentString As String = Encoding.UTF8.GetString(ExistingContentBytes)

                Return ExistingFileContentString
            End Using
        Catch ex As Exception
            MsgBox(ex.Message)
            Return ""
        End Try
    End Function

End Class
