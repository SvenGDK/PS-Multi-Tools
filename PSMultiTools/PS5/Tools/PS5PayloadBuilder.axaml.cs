using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using FluentFTP;
using ImageMagick;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PSMultiTools.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PSMultiTools.PS5.Tools;

public partial class PS5PayloadBuilder : Window
{

    private const string UpdateUser = "EnterYourUsername";
    private const string MinimalUserToken = "github_actions_token";
    private const string RepoToUpdate = "EnterYourRepo";
    private const string RepoBranch = "RepoBranch";

    public PS5PayloadBuilder()
    {
        InitializeComponent();

        Loaded += PS5PayloadBuilder_Loaded;
        SelectedPayloadComboBox.SelectionChanged += SelectedPayloadComboBox_SelectionChanged;

        SaveToUSBCheckBox.IsCheckedChanged += SaveToUSBCheckBox_IsCheckedChanged;
        SaveInternalCheckBox.IsCheckedChanged += SaveInternalCheckBox_IsCheckedChanged;

        ShowTextOnBottomCheckBox.IsCheckedChanged += ShowTextOnBottomCheckBox_IsCheckedChanged;
        ShowTextOnTopCheckBox.IsCheckedChanged += ShowTextOnTopCheckBox_IsCheckedChanged;
        ShowTextOnLeftCheckBox.IsCheckedChanged += ShowTextOnLeftCheckBox_IsCheckedChanged;
        ShowTextOnRightCheckBox.IsCheckedChanged += ShowTextOnRightCheckBox_IsCheckedChanged;

        RestoreCheckBox.IsCheckedChanged += RestoreCheckBox_IsCheckedChanged;
    }

    private void RestoreCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (RestoreCheckBox.IsChecked == true)
        {
            SetFontSizeCheckBox.IsEnabled = false;
            ShowTextOnTopCheckBox.IsEnabled = false;
            ShowTextOnBottomCheckBox.IsEnabled = false;
            ShowTextOnLeftCheckBox.IsEnabled = false;
            ShowTextOnRightCheckBox.IsEnabled = false;

            BuildPayloadButton.Content = "Reset Payload";
        }
        else
        {
            SetFontSizeCheckBox.IsEnabled = true;
            ShowTextOnTopCheckBox.IsEnabled = true;
            ShowTextOnBottomCheckBox.IsEnabled = true;
            ShowTextOnLeftCheckBox.IsEnabled = true;
            ShowTextOnRightCheckBox.IsEnabled = true;

            BuildPayloadButton.Content = "Build Payload";
        }
    }

    private void ShowTextOnTopCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (ShowTextOnTopCheckBox.IsChecked == true)
        {
            RestoreCheckBox.IsEnabled = false;
            ShowTextOnBottomCheckBox.IsEnabled = false;
            ShowTextOnLeftCheckBox.IsEnabled = false;
            ShowTextOnRightCheckBox.IsEnabled = false;
        }
        else
        {
            RestoreCheckBox.IsEnabled = true;
            ShowTextOnBottomCheckBox.IsEnabled = true;
            ShowTextOnLeftCheckBox.IsEnabled = true;
            ShowTextOnRightCheckBox.IsEnabled = true;
        }
    }

    private void ShowTextOnRightCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (ShowTextOnRightCheckBox.IsChecked == true)
        {
            RestoreCheckBox.IsEnabled = false;
            ShowTextOnTopCheckBox.IsEnabled = false;
            ShowTextOnLeftCheckBox.IsEnabled = false;
            ShowTextOnBottomCheckBox.IsEnabled = false;
        }
        else
        {
            RestoreCheckBox.IsEnabled = true;
            ShowTextOnTopCheckBox.IsEnabled = true;
            ShowTextOnLeftCheckBox.IsEnabled = true;
            ShowTextOnBottomCheckBox.IsEnabled = true;
        }
    }

    private void ShowTextOnLeftCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (ShowTextOnLeftCheckBox.IsChecked == true)
        {
            RestoreCheckBox.IsEnabled = false;
            ShowTextOnBottomCheckBox.IsEnabled = false;
            ShowTextOnTopCheckBox.IsEnabled = false;
            ShowTextOnRightCheckBox.IsEnabled = false;
        }
        else
        {
            RestoreCheckBox.IsEnabled = true;
            ShowTextOnBottomCheckBox.IsEnabled = true;
            ShowTextOnTopCheckBox.IsEnabled = true;
            ShowTextOnRightCheckBox.IsEnabled = true;
        }
    }

    private void ShowTextOnBottomCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (ShowTextOnBottomCheckBox.IsChecked == true)
        {
            RestoreCheckBox.IsEnabled = false;
            ShowTextOnTopCheckBox.IsEnabled = false;
            ShowTextOnLeftCheckBox.IsEnabled = false;
            ShowTextOnRightCheckBox.IsEnabled = false;
        }
        else
        {
            RestoreCheckBox.IsEnabled = true;
            ShowTextOnTopCheckBox.IsEnabled = true;
            ShowTextOnLeftCheckBox.IsEnabled = true;
            ShowTextOnRightCheckBox.IsEnabled = true;
        }
    }

    private void SaveInternalCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (SaveToUSBCheckBox.IsEnabled == false)
        {
            SaveToUSBCheckBox.IsEnabled = true;
        }
        else
        {
            SaveToUSBCheckBox.IsEnabled = false;
        }
    }

    private void SaveToUSBCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (SaveInternalCheckBox.IsEnabled == false)
        {
            SaveInternalCheckBox.IsEnabled = true;
        }
        else
        {
            SaveInternalCheckBox.IsEnabled = false;
        }
    }

    private async void PS5PayloadBuilder_Loaded(object? sender, RoutedEventArgs e)
    {
        var box = MessageBoxManager.GetMessageBoxStandard("PS5 Payload Builder", "Welcome to the PS5 payload builder." + Environment.NewLine +
            "No SDK setup or knowledge is required." + Environment.NewLine +
            "However, this utility can only process 1 building instruction per user to not stress the API." + Environment.NewLine +
            "Simply retry after a couple of minutes if you get the message 'A payload is currently being built by another user'.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
        await box.ShowWindowAsync();
    }

    private static async Task<int> CheckForActiveWorkflow(string PayloadName)
    {
        string WorkflowFile;
        if (PayloadName == "PS5 SELF Decrypter")
        {
            WorkflowFile = "ps5_self_decrypter.yml";
        }
        else if (PayloadName == "App Titles")
        {
            WorkflowFile = "app_title.yml";
        }
        else
        {
            return 0;
        }

        string GitHubAPIURL = $"https://api.github.com/repos/{UpdateUser}/{RepoToUpdate}/actions/workflows/{WorkflowFile}/runs?branch={RepoBranch}&status=in_progress";
        try
        {
            using var NewHttpClient = new HttpClient();
            NewHttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("PSMultiTools", "16.0"));
            NewHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", MinimalUserToken);

            var WorkflowRunResponse = await NewHttpClient.GetAsync(GitHubAPIURL);
            WorkflowRunResponse.EnsureSuccessStatusCode();
            string JSONContent = await WorkflowRunResponse.Content.ReadAsStringAsync();
            var NewJSONObject = JObject.Parse(JSONContent);
            JArray RunsJSONArray = (JArray)NewJSONObject["workflow_runs"]!;

            if (RunsJSONArray is not null)
            {
                return RunsJSONArray.Count;
            }
            else
            {
                return 0;
            }
        }
        catch (Exception)
        {
            return 0;
        }
    }

    private async Task<bool> CommitChangesAsync(string PayloadName)
    {
        // Always fall back to default values on error or no found values
        if (PayloadName == "PS5 SELF Decrypter")
        {
            string FileToUpdate = "PS5-SELF-Decrypter/source/main.c";
            string GitHubAPIURL = $"https://api.github.com/repos/{UpdateUser}/{RepoToUpdate}/contents/{FileToUpdate}";

            PayloadBuildProgressStatusTextBlock.Text = "Updating payload, please wait...";
            PayloadBuildProgressBar.IsIndeterminate = true;

            try
            {
                using var NewHttpClient = new HttpClient();
                NewHttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("PSMultiTools", "16.0"));
                NewHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", MinimalUserToken);

                // Check if file can be accessed
                var TestResponse = NewHttpClient.GetAsync(GitHubAPIURL).Result;
                if (!TestResponse.IsSuccessStatusCode)
                {
                    return false;
                }

                // Read existing content
                string ExistingContentJSON = TestResponse.Content.ReadAsStringAsync().Result;
                var NewFileInfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(ExistingContentJSON)!;
                string FileSHA = NewFileInfo["sha"].ToString()!;
                byte[] NewContentBytes;

                // Prepare new content
                string ExistingContentBase64 = NewFileInfo["content"].ToString()!;
                ExistingContentBase64 = ExistingContentBase64.Replace("\n", "").Replace("\r", "");
                byte[] ExistingContentBytes = Convert.FromBase64String(ExistingContentBase64);
                string ExistingFileContentString = Encoding.UTF8.GetString(ExistingContentBytes);

                string UpdatedContent = ExistingFileContentString; // Base source file

                // Replace IP in source if requested
                if (!string.IsNullOrEmpty(SELFDecrypterIPTextBox.Text))
                {
                    string IPPattern = @"^(\s*#define\s+PC_IP\s+)""[^""]*""";
                    string ReplacementIP = "$1\"" + SELFDecrypterIPTextBox.Text + "\"";
                    UpdatedContent = Regex.Replace(ExistingFileContentString, IPPattern, ReplacementIP, RegexOptions.Multiline);
                }

                // Replace Port in source if requested
                if (!string.IsNullOrEmpty(SELFDecrypterPortTextBox.Text))
                {
                    string PORTPattern = @"^(\s*#define\s+PC_PORT\s+)\d+";
                    string ReplacementPort = "${1}" + SELFDecrypterPortTextBox.Text;
                    UpdatedContent = Regex.Replace(UpdatedContent, PORTPattern, ReplacementPort, RegexOptions.Multiline);
                }

                // Add more directories to dump if requested
                if (FullDumpCheckBox.IsChecked == true)
                {
                    string RootDIR = "dump_queue_add_dir(sock, \"/\", 0);";
                    string SystemExCommonExLIB = "dump_queue_add_dir(sock, \"/system_ex/common_ex/lib\", 0);";
                    string CommonLIB = "dump_queue_add_dir(sock, \"/system/common/lib\", 0);";
                    string PrivLIB = "dump_queue_add_dir(sock, \"/system/priv/lib\", 0);";
                    string SystemVSH = "dump_queue_add_dir(sock, \"/system/vsh\", 1);";
                    string SystemSYS = "dump_queue_add_dir(sock, \"/system/sys\", 0);";

                    string TargetLine = @"SOCK_LOG(sock, ""[+] got auth manager: %lu\n"", authmgr_handle);";

                    string[] DirectoriesToInsert = [$"    {RootDIR}", $"    {SystemExCommonExLIB}", $"    {CommonLIB}", $"    {PrivLIB}", $"    {SystemVSH}", $"    {SystemSYS}"];

                    string InsertedBlock = TargetLine + Environment.NewLine + string.Join(Environment.NewLine, DirectoriesToInsert);
                    UpdatedContent = UpdatedContent.Replace(TargetLine, InsertedBlock);
                }
                else
                {
                    // Remove from source if they exist from a previous build
                    string[] DirectoriesToRemove = ["dump_queue_add_dir(sock, \"/\", 0);", "dump_queue_add_dir(sock, \"/system_ex/common_ex/lib\", 0);", "dump_queue_add_dir(sock, \"/system/common/lib\", 0);", "dump_queue_add_dir(sock, \"/system/priv/lib\", 0);", "dump_queue_add_dir(sock, \"/system/vsh\", 1);", "dump_queue_add_dir(sock, \"/system/sys\", 0);"];

                    var SourceLines = new List<string>(UpdatedContent.Split([Environment.NewLine], StringSplitOptions.None));
                    SourceLines = [.. SourceLines.Where(line => !DirectoriesToRemove.Any(val => (line.Trim() ?? "") == (val ?? "")))];
                    UpdatedContent = string.Join(Environment.NewLine, SourceLines);
                }

                // Set dump location
                if (SaveToUSBCheckBox.IsChecked == true)
                {
                    string OldDumpLocation = "dump(sock, authmgr_handle, &offsets, \"/data/dump\");";
                    string NewDumpLocation = "dump(sock, authmgr_handle, &offsets, \"/mnt/usb0/PS5\");";
                    UpdatedContent = UpdatedContent.Replace(OldDumpLocation, NewDumpLocation);
                }
                else if (SaveInternalCheckBox.IsChecked == true)
                {
                    string OldDumpLocation = "dump(sock, authmgr_handle, &offsets, \"/mnt/usb0/PS5\");";
                    string NewDumpLocation = "dump(sock, authmgr_handle, &offsets, \"/data/dump\");";
                    UpdatedContent = UpdatedContent.Replace(OldDumpLocation, NewDumpLocation);
                }
                else
                {
                    // Use default (replace USB by internal)
                    string OldDumpLocation = "dump(sock, authmgr_handle, &offsets, \"/mnt/usb0/PS5\");";
                    string NewDumpLocation = "dump(sock, authmgr_handle, &offsets, \"/data/dump\");";
                    UpdatedContent = UpdatedContent.Replace(OldDumpLocation, NewDumpLocation);
                }

                NewContentBytes = Encoding.UTF8.GetBytes(UpdatedContent); // Final source file

                // Create JSON for updating the file
                string ContentAsBase64 = Convert.ToBase64String(NewContentBytes);
                var UpdateMessage = new Dictionary<string, object>() { { "message", "Update using PS Multi Tools" }, { "content", ContentAsBase64 }, { "sha", FileSHA } };
                string UpdateMessageJSON = JsonConvert.SerializeObject(UpdateMessage);
                var NewStringContent = new StringContent(UpdateMessageJSON, Encoding.UTF8, "application/json");

                // Update the file
                var UpdateResponse = NewHttpClient.PutAsync(GitHubAPIURL, NewStringContent).Result;
                if (UpdateResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    string ErrorMessage = UpdateResponse.Content.ReadAsStringAsync().Result;
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Error Message: " + ErrorMessage, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        else if (PayloadName == "App Titles")
        {
            string FileToUpdate = "app_title/main.c";
            string GitHubAPIURL = $"https://api.github.com/repos/{UpdateUser}/{RepoToUpdate}/contents/{FileToUpdate}";

            PayloadBuildProgressStatusTextBlock.Text = "Updating payload, please wait...";
            PayloadBuildProgressBar.IsIndeterminate = true;

            try
            {
                using var NewHttpClient = new HttpClient();
                NewHttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("PSMultiTools", "16.0"));
                NewHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", MinimalUserToken);

                // Check if file can be accessed
                var TestResponse = NewHttpClient.GetAsync(GitHubAPIURL).Result;
                if (!TestResponse.IsSuccessStatusCode)
                {
                    return false;
                }

                // Read existing content
                string ExistingContentJSON = TestResponse.Content.ReadAsStringAsync().Result;
                var NewFileInfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(ExistingContentJSON)!;
                string FileSHA = NewFileInfo["sha"].ToString()!;
                byte[] NewContentBytes;

                // Prepare new content
                string ExistingContentBase64 = NewFileInfo["content"].ToString()!;
                ExistingContentBase64 = ExistingContentBase64.Replace("\n", "").Replace("\r", "");
                byte[] ExistingContentBytes = Convert.FromBase64String(ExistingContentBase64);
                string ExistingFileContentString = Encoding.UTF8.GetString(ExistingContentBytes);

                string UpdatedContent = ExistingFileContentString; // Base source file

                // Replace font size
                if (SetFontSizeCheckBox.IsChecked == true)
                {
                    string NewFontSize;
                    if (!string.IsNullOrEmpty(FontSizeTextBox.Text))
                    {
                        NewFontSize = FontSizeTextBox.Text;
                    }
                    else
                    {
                        NewFontSize = "60";
                    } // Default

                    string NewRegexPattern = @"(float\s+scale\s*=\s*stbtt_ScaleForPixelHeight\(&fi,\s*)(\d+)(\s*\);)";
                    string ReplacementString = "${1}" + NewFontSize + "${3}";
                    UpdatedContent = Regex.Replace(UpdatedContent, NewRegexPattern, ReplacementString);
                }
                else
                {
                    // Use default
                    string NewRegexPattern = @"(float\s+scale\s*=\s*stbtt_ScaleForPixelHeight\(&fi,\s*)(\d+)(\s*\);)";
                    string ReplacementString = "${1}" + "60" + "${3}";
                    UpdatedContent = Regex.Replace(UpdatedContent, NewRegexPattern, ReplacementString);
                }

                // Replace text position
                if (ShowTextOnBottomCheckBox.IsChecked == true)
                {
                    string[] BottomTopDrawingPositions = ["int x = (w - total_w)/2, y = ascent + 10;", "int x = (w - total_w)/2, y = 10;"];
                    string FoundDrawingPosition = BottomTopDrawingPositions.FirstOrDefault(pattern => UpdatedContent.Contains(pattern))!;
                    // Replace the top or bottom position code
                    if (!string.IsNullOrEmpty(FoundDrawingPosition))
                    {
                        string NewDrawingPosition = "int x = (w - total_w)/2, y = ascent + 10;";
                        UpdatedContent = UpdatedContent.Replace(FoundDrawingPosition, NewDrawingPosition);
                    }
                    else
                    {
                        // Use default (same)
                        string NewDrawingPosition = "int x = (w - total_w)/2, y = ascent + 10;";
                        UpdatedContent = UpdatedContent.Replace(FoundDrawingPosition, NewDrawingPosition);
                    }
                }
                else if (ShowTextOnTopCheckBox.IsChecked == true)
                {
                    string[] BottomTopDrawingPositions = ["int x = (w - total_w)/2, y = ascent + 10;", "int x = (w - total_w)/2, y = 10;"];
                    string FoundDrawingPosition = BottomTopDrawingPositions.FirstOrDefault(pattern => UpdatedContent.Contains(pattern))!;
                    // Replace the top or bottom position code
                    if (!string.IsNullOrEmpty(FoundDrawingPosition))
                    {
                        string NewDrawingPosition = "int x = (w - total_w)/2, y = 10;";
                        UpdatedContent = UpdatedContent.Replace(FoundDrawingPosition, NewDrawingPosition);
                    }
                    else
                    {
                        // Use default
                        string NewDrawingPosition = "int x = (w - total_w)/2, y = ascent + 10;";
                        UpdatedContent = UpdatedContent.Replace(FoundDrawingPosition, NewDrawingPosition);
                    }
                }
                else if (ShowTextOnLeftCheckBox.IsChecked == true)
                {
                    string[] DefaultLeftRightDrawingPositions = ["draw_text(img,w,h,e->d_name);", "draw_text_left(img,w,h,e->d_name);", "draw_text_right(img,w,h,e->d_name);"];
                    string FoundDrawingCall = DefaultLeftRightDrawingPositions.FirstOrDefault(pattern => UpdatedContent.Contains(pattern))!;
                    // Replace the draw call
                    if (!string.IsNullOrEmpty(FoundDrawingCall))
                    {
                        string NewDrawingCall = "draw_text_left(img,w,h,e->d_name);";
                        UpdatedContent = UpdatedContent.Replace(FoundDrawingCall, NewDrawingCall);
                    }
                    else
                    {
                        // Use default
                        string NewDrawingCall = "draw_text(img,w,h,e->d_name);";
                        UpdatedContent = UpdatedContent.Replace(FoundDrawingCall, NewDrawingCall);
                    }
                }
                else if (ShowTextOnRightCheckBox.IsChecked == true)
                {
                    string[] DefaultLeftRightDrawingPositions = ["draw_text(img,w,h,e->d_name);", "draw_text_left(img,w,h,e->d_name);", "draw_text_right(img,w,h,e->d_name);"];
                    string FoundDrawingCall = DefaultLeftRightDrawingPositions.FirstOrDefault(pattern => UpdatedContent.Contains(pattern))!;
                    // Replace the draw call
                    if (!string.IsNullOrEmpty(FoundDrawingCall))
                    {
                        string NewDrawingCall = "draw_text_right(img,w,h,e->d_name);";
                        UpdatedContent = UpdatedContent.Replace(FoundDrawingCall, NewDrawingCall);
                    }
                    else
                    {
                        // Use default
                        string NewDrawingCall = "draw_text(img,w,h,e->d_name);";
                        UpdatedContent = UpdatedContent.Replace(FoundDrawingCall, NewDrawingCall);
                    }
                }
                else
                {
                    // Use default
                    string[] BottomTopDrawingPositions = ["int x = (w - total_w)/2, y = ascent + 10;", "int x = (w - total_w)/2, y = 10;"];
                    string[] DefaultLeftRightDrawingPositions = ["draw_text(img,w,h,e->d_name);", "draw_text_left(img,w,h,e->d_name);", "draw_text_right(img,w,h,e->d_name);"];
                    string FoundDrawingPosition = BottomTopDrawingPositions.FirstOrDefault(pattern => UpdatedContent.Contains(pattern))!;
                    string FoundDrawingCall = DefaultLeftRightDrawingPositions.FirstOrDefault(pattern => UpdatedContent.Contains(pattern))!;
                    string DefaultDrawingPosition = "int x = (w - total_w)/2, y = ascent + 10;";
                    string DefaultDrawingCall = "draw_text(img,w,h,e->d_name);";

                    UpdatedContent = UpdatedContent.Replace(FoundDrawingPosition, DefaultDrawingPosition);
                    UpdatedContent = UpdatedContent.Replace(FoundDrawingCall, DefaultDrawingCall);
                }

                NewContentBytes = Encoding.UTF8.GetBytes(UpdatedContent); // Final source file

                // Create JSON for updating the file
                string ContentAsBase64 = Convert.ToBase64String(NewContentBytes);
                var UpdateMessage = new Dictionary<string, object>() { { "message", "Update using PS Multi Tools" }, { "content", ContentAsBase64 }, { "sha", FileSHA } };
                string UpdateMessageJSON = JsonConvert.SerializeObject(UpdateMessage);
                var NewStringContent = new StringContent(UpdateMessageJSON, Encoding.UTF8, "application/json");

                // Update the file
                var UpdateResponse = NewHttpClient.PutAsync(GitHubAPIURL, NewStringContent).Result;
                if (UpdateResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    string ErrorMessage = UpdateResponse.Content.ReadAsStringAsync().Result;
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Error Message: " + ErrorMessage, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    private static async Task<bool> RunWorkflow(string PayloadName)
    {
        // Run the workflow
        string WorkflowFile;
        if (PayloadName == "PS5 SELF Decrypter")
        {
            WorkflowFile = "ps5_self_decrypter.yml";
        }
        else if (PayloadName == "App Titles")
        {
            WorkflowFile = "app_title.yml";
        }
        else
        {
            return false;
        }

        string GitHubAPIURL = $"https://api.github.com/repos/{UpdateUser}/{RepoToUpdate}/actions/workflows/{WorkflowFile}/dispatches";
        var WorkflowPayload = new Dictionary<string, object>() { { "ref", RepoBranch } };
        string WorkflowPayloadJSON = JsonConvert.SerializeObject(WorkflowPayload);

        try
        {
            using var NewHttpClient = new HttpClient();
            NewHttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("PSMultiTools", "16.0"));
            NewHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", MinimalUserToken);

            var NewStringContent = new StringContent(WorkflowPayloadJSON, Encoding.UTF8, "application/json");
            var WorkflowResponse = await NewHttpClient.PostAsync(GitHubAPIURL, NewStringContent);
            if (WorkflowResponse.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static async Task<long> GetLatestWorkflowRunId(string PayloadName)
    {
        // Get the workflow run id for monitoring the status
        string WorkflowFile;
        if (PayloadName == "PS5 SELF Decrypter")
        {
            WorkflowFile = "ps5_self_decrypter.yml";
        }
        else if (PayloadName == "App Titles")
        {
            WorkflowFile = "app_title.yml";
        }
        else
        {
            return 0L;
        }

        string GitHubAPIURL = $"https://api.github.com/repos/{UpdateUser}/{RepoToUpdate}/actions/workflows/{WorkflowFile}/runs?branch={RepoBranch}&event=workflow_dispatch";
        try
        {
            using var NewHttpClient = new HttpClient();
            NewHttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("PSMultiTools", "16.0"));
            NewHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", MinimalUserToken);

            var WorkflowRunIDResponse = await NewHttpClient.GetAsync(GitHubAPIURL);
            WorkflowRunIDResponse.EnsureSuccessStatusCode();
            string JSONContent = await WorkflowRunIDResponse.Content.ReadAsStringAsync();
            var NewJSONObject = JObject.Parse(JSONContent);
            JArray RunsJSONArray = (JArray)NewJSONObject["workflow_runs"]!;

            if (RunsJSONArray is not null && RunsJSONArray.Count > 0)
            {
                JObject LatestRun = (JObject)RunsJSONArray[0];
                long RunID = LatestRun["id"]!.ToObject<long>();
                return RunID;
            }
            else
            {
                return 0L;
            }
        }
        catch (Exception)
        {
            return 0L;
        }
    }

    private static async Task<string> MonitorAndDownloadArtifact(long RunID)
    {
        // Monitor the status of the payload build process
        try
        {
            using var NewHttpClient = new HttpClient();
            NewHttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("PSMultiTools", "16.0"));
            NewHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", MinimalUserToken);

            string WorkflowURL = $"https://api.github.com/repos/{UpdateUser}/{RepoToUpdate}/actions/runs/{RunID}";
            string ArtifactsURL = $"https://api.github.com/repos/{UpdateUser}/{RepoToUpdate}/actions/runs/{RunID}/artifacts";

            bool BuildCompleted = false;
            do
            {
                try
                {
                    var RunResponse = await NewHttpClient.GetAsync(WorkflowURL);
                    RunResponse.EnsureSuccessStatusCode();
                    string RunContent = await RunResponse.Content.ReadAsStringAsync();
                    var RunJSON = JObject.Parse(RunContent);
                    string RunStatus = RunJSON["status"]!.ToString();

                    if (RunStatus.Equals("completed", StringComparison.OrdinalIgnoreCase))
                    {
                        BuildCompleted = true;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Error checking workflow status: " + ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
                await Task.Delay(8000); // Check every 8secs until the run is completed
            }
            while (!BuildCompleted);

            // Once the run is completed, retrieve the artifact list
            try
            {
                var ArtifactsResponse = await NewHttpClient.GetAsync(ArtifactsURL);
                ArtifactsResponse.EnsureSuccessStatusCode();
                string ArtifactsContent = await ArtifactsResponse.Content.ReadAsStringAsync();
                var ArtifactsJSON = JObject.Parse(ArtifactsContent);
                JArray ArtifactsArray = (JArray)ArtifactsJSON["artifacts"]!;

                if (ArtifactsArray is not null && ArtifactsArray.Count > 0)
                {
                    JObject Artifact = (JObject)ArtifactsArray[0];
                    string DownloadURL = Artifact["archive_download_url"]!.ToString();
                    return DownloadURL;
                }
                else
                {
                    return "";
                }
            }

            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Error retrieving or downloading artifacts: " + ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
                return "";
            }
        }
        catch (Exception)
        {
            return "";
        }
    }

    private async void BuildPayloadButton_Click(object? sender, RoutedEventArgs e)
    {
        // Double check for any running workflow before proceeding
        int ActiveSELFDecrypterWorkflows = await CheckForActiveWorkflow("PS5 SELF Decrypter");
        int ActiveAppTitlesWorkflows = await CheckForActiveWorkflow("App Titles");
        string CurrentRepoState = await GetSlotStateAsync();

        if (BuildPayloadButton.Content!.ToString() == "Build Payload" && ActiveSELFDecrypterWorkflows + ActiveAppTitlesWorkflows > 0 && !(CurrentRepoState == "reserved"))
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Info", "A payload is currently being built by another user. Please try again in 1 minute.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning);
            await box.ShowWindowAsync();
        }
        else if (SelectedPayloadComboBox.SelectedIndex == 0)
        {
            ReserveSlot("Reserve");
            // Commit the changes for the selected payload
            if (await CommitChangesAsync("PS5 SELF Decrypter"))
            {
                BuildPayloadButton.IsEnabled = false;
                SelectedPayloadComboBox.IsEnabled = false;
                PayloadBuildProgressStatusTextBlock.Text = "Payload updated, starting build";

                // Run the build action for the selected payload
                bool RunNewWorkflowSucceeded = await RunWorkflow("PS5 SELF Decrypter");
                if (RunNewWorkflowSucceeded)
                {

                    PayloadBuildProgressStatusTextBlock.Text = "Build started, getting the build process id";

                    System.Threading.Thread.Sleep(10000); // Wait 10sec before getting the latest run id

                    // Get the latest workflow run id
                    long RetrievedWorkflowID = await GetLatestWorkflowRunId("PS5 SELF Decrypter");
                    if (RetrievedWorkflowID != 0L)
                    {

                        PayloadBuildProgressStatusTextBlock.Text = "Building payload, please wait...";

                        // Monitor the build process and get the produced artifact URL
                        string CreatedArtifcatURL = await MonitorAndDownloadArtifact(RetrievedWorkflowID);
                        if (!string.IsNullOrEmpty(CreatedArtifcatURL))
                        {

                            PayloadBuildProgressStatusTextBlock.Text = @"Build succeeded! Saving payload to \Downloads ...";
                            ReserveSlot("Free");

                            // Remove previously created payload
                            if (File.Exists(Path.Combine(Utils.GetDownloadsFolderPath(), "ps5-self-decrypter-payload.zip")))
                                File.Delete(Path.Combine(Utils.GetDownloadsFolderPath(), "ps5-self-decrypter-payload.zip"));

                            // Save the payload
                            try
                            {
                                var NewHttpClientHandler = new HttpClientHandler() { AllowAutoRedirect = false };
                                using (var NewHttpClient = new HttpClient(NewHttpClientHandler))
                                {
                                    NewHttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("PSMultiTools", "16.0"));
                                    NewHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", MinimalUserToken);

                                    var response = await NewHttpClient.GetAsync(CreatedArtifcatURL);
                                    if (response.StatusCode == System.Net.HttpStatusCode.Redirect || response.StatusCode == System.Net.HttpStatusCode.TemporaryRedirect)
                                    {
                                        string RedirectedURL = response.Headers.Location!.ToString();
                                        using var DLClient = new HttpClient();
                                        DLClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("PSMultiTools", "16.0"));
                                        byte[] finalResponse = await DLClient.GetByteArrayAsync(RedirectedURL);
                                        File.WriteAllBytes(Path.Combine(Utils.GetDownloadsFolderPath(), "ps5-self-decrypter-payload.zip"), finalResponse);
                                    }
                                    else if (response.IsSuccessStatusCode)
                                    {
                                        byte[] artifactZipBytes = await response.Content.ReadAsByteArrayAsync();
                                        File.WriteAllBytes(Path.Combine(Utils.GetDownloadsFolderPath(), "ps5-self-decrypter-payload.zip"), artifactZipBytes);
                                    }
                                }

                                BuildPayloadButton.IsEnabled = true;
                                SelectedPayloadComboBox.IsEnabled = true;
                                PayloadBuildProgressStatusTextBlock.Text = @"Payload saved to Downloads";
                                PayloadBuildProgressBar.IsIndeterminate = false;

                                var box = MessageBoxManager.GetMessageBoxStandard("Extract payload ?", "Payload build and saved!" + Environment.NewLine + "The payload is saved as zip that can be extracted, do you want to extract it now ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                                var boxresult = await box.ShowWindowDialogAsync(this);
                                if (boxresult == ButtonResult.Yes)
                                {
                                    // Check and extract the downloaded archive into the downloads folder
                                    if (File.Exists(Path.Combine(Utils.GetDownloadsFolderPath(), "ps5-self-decrypter-payload.zip")))
                                    {
                                        Process ArchiveExtractor = new();
                                        ArchiveExtractor.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "7z.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "7zz");
                                        ArchiveExtractor.StartInfo.Arguments = $"x \"{Path.Combine(Utils.GetDownloadsFolderPath(), "ps5-self-decrypter-payload.zip")}\" -o\"{Utils.EnsureTrailingSeparator(Utils.GetDownloadsFolderPath())}\" -y";
                                        ArchiveExtractor.StartInfo.UseShellExecute = false;
                                        ArchiveExtractor.StartInfo.CreateNoWindow = true;
                                        ArchiveExtractor.Start();
                                        await ArchiveExtractor.WaitForExitAsync();
                                        ArchiveExtractor.Close();

                                        var box2 = MessageBoxManager.GetMessageBoxStandard("Completed", "Extraction done!" + Environment.NewLine + "Do you want to open the Downloads folder ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                                        var boxresult2 = await box2.ShowWindowDialogAsync(this);
                                        if (boxresult2 == ButtonResult.Yes)
                                        {
                                            Utils.OpenDownloadsFolder();
                                        }
                                    }
                                    else
                                    {
                                        var box2 = MessageBoxManager.GetMessageBoxStandard("Error while extracting", "Could not find the saved payload.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                        await box2.ShowWindowAsync();

                                        var box3 = MessageBoxManager.GetMessageBoxStandard("Completed", "Do you want to open the Downloads folder ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                                        var boxresult2 = await box3.ShowWindowDialogAsync(this);
                                        if (boxresult2 == ButtonResult.Yes)
                                        {
                                            Utils.OpenDownloadsFolder();
                                        }
                                    }
                                }
                                else
                                {
                                    var box3 = MessageBoxManager.GetMessageBoxStandard("Completed", "Do you want to open the Downloads folder ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                                    var boxresult2 = await box3.ShowWindowDialogAsync(this);
                                    if (boxresult2 == ButtonResult.Yes)
                                    {
                                        Utils.OpenDownloadsFolder();
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                BuildPayloadButton.IsEnabled = true;
                                SelectedPayloadComboBox.IsEnabled = true;

                                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Error downloading the payload.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                await box.ShowWindowAsync();
                            }
                        }
                        else
                        {
                            BuildPayloadButton.IsEnabled = true;
                            SelectedPayloadComboBox.IsEnabled = true;

                            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Error getting the created payload url.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box.ShowWindowAsync();
                        }
                    }
                    else
                    {
                        BuildPayloadButton.IsEnabled = true;
                        SelectedPayloadComboBox.IsEnabled = true;

                        var box = MessageBoxManager.GetMessageBoxStandard("Error", "Error retrieving the workflow id.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box.ShowWindowAsync();
                    }
                }
                else
                {
                    BuildPayloadButton.IsEnabled = true;
                    SelectedPayloadComboBox.IsEnabled = true;

                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Error building the payload.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
            else
            {
                ReserveSlot("Free");
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Error updating the payload source.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }

        else if (SelectedPayloadComboBox.SelectedIndex == 1)
        {
            if (RestoreCheckBox.IsChecked == true)
            {
                // Restore by getting icon .dds files & converting to .png, then syncing back to the PS5

                string PS5IP = PS5FTPIPTextBox.Text!;
                int PS5Port = Convert.ToInt32(PS5FTPPortTextBox.Text);
                string CacheFolder = Path.Combine(Environment.CurrentDirectory, "Cache", "UserAppMeta");

                await Task.Run(async () =>
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        BuildPayloadButton.IsEnabled = false;
                        SelectedPayloadComboBox.IsEnabled = false;
                        PayloadBuildProgressStatusTextBlock.Text = "Getting /user/appmeta...";
                        PayloadBuildProgressBar.IsIndeterminate = true;
                    });

                    // Dump /user/appmeta to get the DDS files
                    // Configurate the FTP connection
                    using var conn = new FtpClient(PS5IP, "anonymous", "anonymous", PS5Port);
                    conn.Config.EncryptionMode = FtpEncryptionMode.None;
                    conn.Config.SslProtocols = SslProtocols.None;
                    conn.Config.DataConnectionEncryption = false;

                    // Connect
                    conn.Connect();

                    // Create cache
                    if (!Directory.Exists(CacheFolder))
                    {
                        Directory.CreateDirectory(CacheFolder);
                    }
                    else
                    {
                        Directory.Delete(CacheFolder, true);
                        Directory.CreateDirectory(CacheFolder);
                    }

                    // Sync content of /user/appmeta to .\Cache\UserAppMeta
                    try
                    {
                        DownloadRecursively(conn, "/user/appmeta", CacheFolder);
                        Dispatcher.UIThread.Invoke(() => PayloadBuildProgressStatusTextBlock.Text = "Saved /user/appmeta. Converting .dds files...");

                        // Check if there's any appmeta
                        if (Directory.EnumerateFiles(CacheFolder).Any())
                        {
                            // Convert all DDS icons to icon0.png
                            foreach (var DumpedAppMeta in Directory.GetDirectories(CacheFolder))
                            {
                                if (File.Exists(Path.Combine(DumpedAppMeta, "icon0.dds")))
                                {
                                    string IconToConvert = Path.Combine(DumpedAppMeta, "icon0.dds");
                                    string OutputFileName = Path.Combine(DumpedAppMeta, "icon0.png");
                                    try
                                    {
                                        using var NewPNGImage = new MagickImage(IconToConvert);
                                        NewPNGImage.SetCompression(CompressionMethod.NoCompression);
                                        NewPNGImage.Format = MagickFormat.Png;
                                        NewPNGImage.Write(OutputFileName);
                                    }
                                    catch (Exception ex)
                                    {
                                        var box = MessageBoxManager.GetMessageBoxStandard("Error", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                        await box.ShowWindowAsync();
                                    }
                                }
                                else
                                {
                                    // To do
                                }
                            }

                            Dispatcher.UIThread.Invoke(() => PayloadBuildProgressStatusTextBlock.Text = "Syncing PNG icons back to the PS5...");
                            Dispatcher.UIThread.Invoke(() => PayloadBuildProgressBar.IsIndeterminate = false);

                            // Sync back from .\Cache\UserAppMeta to /user/appmeta
                            try
                            {
                                UploadRecursively(conn, CacheFolder, "/user/appmeta");

                                Dispatcher.UIThread.Invoke(() =>
                                {
                                    PayloadBuildProgressStatusTextBlock.Text = "Icons restored! Please restart your PS5.";
                                    BuildPayloadButton.IsEnabled = true;
                                    SelectedPayloadComboBox.IsEnabled = true;
                                });
                            }
                            catch (Exception)
                            {
                                Dispatcher.UIThread.Invoke(() =>
                                {
                                    PayloadBuildProgressStatusTextBlock.Text = "Failed to restore the icons back to the PS5.";
                                    BuildPayloadButton.IsEnabled = true;
                                    SelectedPayloadComboBox.IsEnabled = true;
                                });
                            }

                            // Close
                            conn.Disconnect();
                        }
                        else
                        {
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                PayloadBuildProgressStatusTextBlock.Text = "Failed getting files from the PS5.";
                                PayloadBuildProgressBar.IsIndeterminate = false;
                                BuildPayloadButton.IsEnabled = true;
                                SelectedPayloadComboBox.IsEnabled = true;
                            });
                            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not find any dumped appmeta.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box.ShowWindowAsync();
                        }
                    }
                    catch (Exception)
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            PayloadBuildProgressStatusTextBlock.Text = "Failed getting files from the PS5.";
                            PayloadBuildProgressBar.IsIndeterminate = false;
                            BuildPayloadButton.IsEnabled = true;
                            SelectedPayloadComboBox.IsEnabled = true;
                        });
                        var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not dump any appmeta.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box.ShowWindowAsync();
                    }
                });
            }
            else
            {
                ReserveSlot("Reserve");
                // Commit the changes for the selected payload
                if (await CommitChangesAsync("App Titles"))
                {

                    BuildPayloadButton.IsEnabled = false;
                    SelectedPayloadComboBox.IsEnabled = false;
                    PayloadBuildProgressStatusTextBlock.Text = "Payload updated, starting build";

                    // Run the build action for the selected payload
                    bool RunNewWorkflowSucceeded = await RunWorkflow("App Titles");
                    if (RunNewWorkflowSucceeded)
                    {

                        PayloadBuildProgressStatusTextBlock.Text = "Build started, getting the build process id";

                        System.Threading.Thread.Sleep(10000); // Wait 10sec before getting the latest run id

                        // Get the latest workflow run id
                        long RetrievedWorkflowID = await GetLatestWorkflowRunId("App Titles");
                        if (RetrievedWorkflowID != 0L)
                        {

                            PayloadBuildProgressStatusTextBlock.Text = "Building payload, please wait...";

                            // Monitor the build process and get the produced artifact URL
                            string CreatedArtifcatURL = await MonitorAndDownloadArtifact(RetrievedWorkflowID);
                            if (!string.IsNullOrEmpty(CreatedArtifcatURL))
                            {

                                PayloadBuildProgressStatusTextBlock.Text = @"Build succeeded! Saving payload to \Downloads ...";
                                ReserveSlot("Free");

                                // Remove previously created payload
                                if (File.Exists(Path.Combine(Utils.GetDownloadsFolderPath(), "app_title_payload.zip")))
                                    File.Delete(Path.Combine(Utils.GetDownloadsFolderPath(), "app_title_payload.zip"));

                                // Save the payload
                                try
                                {
                                    var NewHttpClientHandler = new HttpClientHandler() { AllowAutoRedirect = false };
                                    using (var NewHttpClient = new HttpClient(NewHttpClientHandler))
                                    {
                                        NewHttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("PSMultiTools", "16.0"));
                                        NewHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", MinimalUserToken);

                                        var response = await NewHttpClient.GetAsync(CreatedArtifcatURL);
                                        if (response.StatusCode == System.Net.HttpStatusCode.Redirect || response.StatusCode == System.Net.HttpStatusCode.TemporaryRedirect)
                                        {
                                            string RedirectedURL = response.Headers.Location!.ToString();
                                            using var DLClient = new HttpClient();
                                            DLClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("PSMultiTools", "16.0"));
                                            byte[] finalResponse = await DLClient.GetByteArrayAsync(RedirectedURL);
                                            File.WriteAllBytes(Path.Combine(Utils.GetDownloadsFolderPath(), "app_title_payload.zip"), finalResponse);
                                        }
                                        else if (response.IsSuccessStatusCode)
                                        {
                                            byte[] artifactZipBytes = await response.Content.ReadAsByteArrayAsync();
                                            File.WriteAllBytes(Path.Combine(Utils.GetDownloadsFolderPath(), "app_title_payload.zip"), artifactZipBytes);
                                        }
                                    }

                                    BuildPayloadButton.IsEnabled = true;
                                    SelectedPayloadComboBox.IsEnabled = true;
                                    PayloadBuildProgressStatusTextBlock.Text = @"Payload saved to Downloads";
                                    PayloadBuildProgressBar.IsIndeterminate = false;

                                    var box = MessageBoxManager.GetMessageBoxStandard("Extract payload ?", "Payload build and saved!" + Environment.NewLine + "The payload is saved as zip that can be extracted, do you want to extract it now ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                                    var boxresult = await box.ShowWindowDialogAsync(this);
                                    if (boxresult == ButtonResult.Yes)
                                    {
                                        // Check and extract the downloaded archive into the downloads folder
                                        if (File.Exists(Path.Combine(Utils.GetDownloadsFolderPath(), "app_title_payload.zip")))
                                        {
                                            Process ArchiveExtractor = new();
                                            ArchiveExtractor.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "7z.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "7zz");
                                            ArchiveExtractor.StartInfo.Arguments = $"x \"{Path.Combine(Utils.GetDownloadsFolderPath(), "app_title_payload.zip")}\" -o\"{Utils.EnsureTrailingSeparator(Utils.GetDownloadsFolderPath())}\" -y";
                                            ArchiveExtractor.StartInfo.UseShellExecute = false;
                                            ArchiveExtractor.StartInfo.CreateNoWindow = true;
                                            ArchiveExtractor.Start();
                                            await ArchiveExtractor.WaitForExitAsync();
                                            ArchiveExtractor.Close();

                                            var box2 = MessageBoxManager.GetMessageBoxStandard("Completed", "Extraction done!" + Environment.NewLine + "Do you want to open the Downloads folder ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                                            var boxresult2 = await box2.ShowWindowDialogAsync(this);
                                            if (boxresult2 == ButtonResult.Yes)
                                            {
                                                Utils.OpenDownloadsFolder();
                                            }
                                        }
                                        else
                                        {
                                            var box2 = MessageBoxManager.GetMessageBoxStandard("Error while extracting", "Could not find the saved payload.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                            await box2.ShowWindowAsync();

                                            var box3 = MessageBoxManager.GetMessageBoxStandard("Completed", "Do you want to open the Downloads folder ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                                            var boxresult2 = await box3.ShowWindowDialogAsync(this);
                                            if (boxresult2 == ButtonResult.Yes)
                                            {
                                                Utils.OpenDownloadsFolder();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var box2 = MessageBoxManager.GetMessageBoxStandard("Completed", "Do you want to open the Downloads folder ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                                        var boxresult2 = await box2.ShowWindowDialogAsync(this);
                                        if (boxresult2 == ButtonResult.Yes)
                                        {
                                            Utils.OpenDownloadsFolder();
                                        }
                                    }

                                }
                                catch (Exception)
                                {
                                    BuildPayloadButton.IsEnabled = true;
                                    SelectedPayloadComboBox.IsEnabled = true;

                                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Error downloading the payload.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                    await box.ShowWindowAsync();
                                }
                            }

                            else
                            {
                                BuildPayloadButton.IsEnabled = true;
                                SelectedPayloadComboBox.IsEnabled = true;

                                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Error getting the created payload url.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                await box.ShowWindowAsync();
                            }
                        }

                        else
                        {
                            BuildPayloadButton.IsEnabled = true;
                            SelectedPayloadComboBox.IsEnabled = true;

                            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Error retrieving the workflow id.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box.ShowWindowAsync();
                        }
                    }

                    else
                    {
                        BuildPayloadButton.IsEnabled = true;
                        SelectedPayloadComboBox.IsEnabled = true;

                        var box = MessageBoxManager.GetMessageBoxStandard("Error", "Error building the payload.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box.ShowWindowAsync();
                    }
                }

                else
                {
                    ReserveSlot("Free");

                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Error updating the payload source.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "No payload selected.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    private void SelectedPayloadComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (SelectedPayloadComboBox.SelectedIndex == 0)
        {
            AppTitleOptionsGrid.IsVisible = false;
            SELFDecrypterOptionsGrid.IsVisible = true;
        }
        else if (SelectedPayloadComboBox.SelectedIndex == 1)
        {
            SELFDecrypterOptionsGrid.IsVisible = false;
            AppTitleOptionsGrid.IsVisible = true;
        }
        else
        {
            AppTitleOptionsGrid.IsVisible = false;
            SELFDecrypterOptionsGrid.IsVisible = false;
        }
    }

    private static async void ReserveSlot(string SlotAction)
    {
        string FileToUpdate = "slot.txt";
        string GitHubAPIURL = $"https://api.github.com/repos/{UpdateUser}/{RepoToUpdate}/contents/{FileToUpdate}";
        try
        {
            using var NewHttpClient = new HttpClient();
            NewHttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("PSMultiTools", "16.0"));
            NewHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", MinimalUserToken);

            // Read existing content
            var ContentResponse = NewHttpClient.GetAsync(GitHubAPIURL).Result;
            string ExistingContentJSON = ContentResponse.Content.ReadAsStringAsync().Result;
            var NewFileInfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(ExistingContentJSON)!;
            string FileSHA = NewFileInfo["sha"].ToString()!;
            byte[] NewContentBytes;

            string UpdatedContent = "";
            if (SlotAction == "Reserve")
            {
                UpdatedContent = "reserved";
            }
            else
            {
                UpdatedContent = "free";
            }

            NewContentBytes = Encoding.UTF8.GetBytes(UpdatedContent); // Final source file

            // Create JSON for updating the file
            string ContentAsBase64 = Convert.ToBase64String(NewContentBytes);
            var UpdateMessage = new Dictionary<string, object>() { { "message", "Update using PS Multi Tools" }, { "content", ContentAsBase64 }, { "sha", FileSHA } };
            string UpdateMessageJSON = JsonConvert.SerializeObject(UpdateMessage);
            var NewStringContent = new StringContent(UpdateMessageJSON, Encoding.UTF8, "application/json");

            // Update the file
            var UpdateResponse = NewHttpClient.PutAsync(GitHubAPIURL, NewStringContent).Result;
            if (!UpdateResponse.IsSuccessStatusCode)
            {
                string ErrorMessage = UpdateResponse.Content.ReadAsStringAsync().Result;
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Error Message: " + ErrorMessage, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    private static async Task<string> GetSlotStateAsync()
    {
        string GitHubAPIURL = $"https://api.github.com/repos/{UpdateUser}/{RepoToUpdate}/contents/slot.txt";
        try
        {
            using var NewHttpClient = new HttpClient();
            NewHttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("PSMultiTools", "16.0"));
            NewHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", MinimalUserToken);

            // Read existing content
            var ContentResponse = NewHttpClient.GetAsync(GitHubAPIURL).Result;
            string ExistingContentJSON = ContentResponse.Content.ReadAsStringAsync().Result;
            var NewFileInfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(ExistingContentJSON)!;
            string FileSHA = NewFileInfo["sha"].ToString()!;
            string ExistingContentBase64 = NewFileInfo["content"].ToString()!;
            ExistingContentBase64 = ExistingContentBase64.Replace("\n", "").Replace(Environment.NewLine, "");
            byte[] ExistingContentBytes = Convert.FromBase64String(ExistingContentBase64);
            string ExistingFileContentString = Encoding.UTF8.GetString(ExistingContentBytes);

            return ExistingFileContentString;
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
            return "";
        }
    }

    private static void DownloadRecursively(FtpClient ftp, string remoteDir, string localDir)
    {
        ftp.SetWorkingDirectory(remoteDir);

        foreach (var item in ftp.GetListing())
        {
            var localPath = Path.Combine(localDir, item.Name);
            var remotePath = item.FullName;

            if (item.Type == FtpObjectType.Directory)
            {
                Directory.CreateDirectory(localPath);
                DownloadRecursively(ftp, remotePath, localPath);
            }
            else if (item.Type == FtpObjectType.File)
            {
                ftp.DownloadFile(localPath, remotePath, FtpLocalExists.Overwrite);
            }
        }
    }

    private static void UploadRecursively(FtpClient ftp, string localDir, string remoteDir)
    {
        if (!ftp.DirectoryExists(remoteDir))
        {
            ftp.CreateDirectory(remoteDir);
        }

        foreach (var entry in Directory.EnumerateFileSystemEntries(localDir))
        {
            var name = Path.GetFileName(entry);
            var remotePath = $"{remoteDir}/{name}";

            if (Directory.Exists(entry))
            {
                UploadRecursively(ftp, entry, remotePath);
            }
            else
            {
                ftp.UploadFile(entry, remotePath, FtpRemoteExists.Overwrite, true);
            }
        }
    }

}