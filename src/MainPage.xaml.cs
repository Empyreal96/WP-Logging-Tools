using ndtklib;
using Registry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telnet;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Logging_Enabler
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        string LocalPath = ApplicationData.Current.LocalFolder.Path;
        static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        TelnetClient client = new TelnetClient(TimeSpan.FromSeconds(1), cancellationTokenSource.Token);
        bool IsBootlogEnabled;
        bool IsUefiLogEnabled;
        NRPC rpc = new NRPC();

        public MainPage()
        {
            this.InitializeComponent();
            AppBusy(true);
            MainWindowPivot.IsEnabled = false;
            HomeText.Text = "Welcome, this app will let you configure basic logging settings for this device";
            try
            {
                rpc.Initialize();

            }
            catch (Exception ex)
            {
                Exceptions.CustomMessage("Error initializing Interop Capabilities");
            }

            CheckLoggingStatus();
            AppBusy(false);
        }

        /// <summary>
        /// Check all the values for each logging option
        /// </summary>
        public async void CheckLoggingStatus()
        {
            //Create the text files used for temporary storing logs
            await ApplicationData.Current.LocalFolder.CreateFileAsync("cmdstring.txt", CreationCollisionOption.ReplaceExisting);
            await ApplicationData.Current.LocalFolder.CreateFileAsync("ntbtlog.txt", CreationCollisionOption.ReplaceExisting);
            await ApplicationData.Current.LocalFolder.CreateFileAsync("ImgUpd.log", CreationCollisionOption.ReplaceExisting);
            await ApplicationData.Current.LocalFolder.CreateFileAsync("ImgUpd.log.cbs.log", CreationCollisionOption.ReplaceExisting);
            try
            {
                await client.Connect();

            }
            catch (Exception ex)
            {
                Exceptions.ThrowFullError(ex);
                return;
            }
            // Boot logging check
            await client.Send("bcdedit /enum {default} > " + $"\"{LocalPath}\\cmdstring.txt\"");
            string results = File.ReadAllText($"{LocalPath}\\cmdstring.txt");
            if (results.Contains("bootlog                 Yes"))
            {
                rpc.FileCopy(@"C:\Windows\ntbtlog.txt", $"{LocalPath}\\ntbtlog.txt", 0);
                IsBootlogEnabled = true;
                BootLogTog.IsOn = true;
                SaveLogBtn.IsEnabled = true;
                ViewLogBtn.IsEnabled = true;
            }
            else
            {
                IsBootlogEnabled = false;
                BootLogTog.IsOn = false;
                SaveLogBtn.IsEnabled = false;
                ViewLogBtn.IsEnabled = false;
            }

            // UEFI logging check
            await client.Send("if exist \"C:\\EFIESP\\Windows\\System32\\Boot\\UEFIChargingLogToDisplay.txt\" echo EXISTS > " + $"\"{LocalPath}\\cmdstring.txt\" 2>&1");
            string results2 = File.ReadAllText($"{LocalPath}\\cmdstring.txt");
            //Exceptions.CustomMessage(results2);
            if (results2.Contains("EXISTS"))
            {
                UefiTog.IsOn = true;
                IsUefiLogEnabled = true;


            }
            else
            {
                UefiTog.IsOn = false;
                IsUefiLogEnabled = false;
            }

            // Local crash dumps checks
            try
            {
                uint DumpType;
                uint dumpType;
                uint dumpCount;
                string dumpfolder;
                await client.Send("reg query \"HKLM\\SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting\\LocalDumps\" > " + $"{LocalPath}\\cmdstring.txt");
                string localDumpsKey = File.ReadAllText($"{LocalPath}\\cmdstring.txt");
                if (localDumpsKey.Contains("ERROR: The system was unable to find the specified registry key or value."))
                {
                    dumpType = 0;
                    dumpCount = 0;
                    dumpfolder = "";
                    
                }
                else
                {


                    await client.Send("reg query \"HKLM\\SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting\\LocalDumps\" /v DumpType > " + $"{LocalPath}\\cmdstring.txt");
                    string dumptypeResult = File.ReadAllText($"{LocalPath}\\cmdstring.txt");
                    if (dumptypeResult.Contains("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting\\LocalDumps"))
                    {
                        string tempname = dumptypeResult.Replace("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting\\LocalDumps", "");
                        string tempname2 = tempname.Replace("DumpType    REG_DWORD", "");
                        string tempresult = Regex.Replace(tempname2, @"\s+", "");
                        switch (tempresult)
                        {
                            case "0x0":
                                DumpTypeCombo.SelectedIndex = 0;
                                break;
                            case "0x1":
                                DumpTypeCombo.SelectedIndex = 1;
                                break;
                            case "0x2":
                                DumpTypeCombo.SelectedIndex = 2;
                                break;
                            default:
                                DumpTypeCombo.SelectedIndex = 0;
                                break;
                        }

                    }
                    else
                    {
                        DumpTypeCombo.SelectedIndex = 0;
                    }



                   await client.Send("reg query \"HKLM\\SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting\\LocalDumps\" /v DumpCount > " + $"{LocalPath}\\cmdstring.txt");
                    string dumpcountResult = File.ReadAllText($"{LocalPath}\\cmdstring.txt");
                    if (dumpcountResult.Contains("DumpCount    REG_DWORD"))
                    {

                        string tempname = dumpcountResult.Replace("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting\\LocalDumps", "");
                        string tempname2 = tempname.Replace("DumpCount    REG_DWORD", "");
                        string tempresult = Regex.Replace(tempname2, @"\s+", "");
                       // Exceptions.CustomMessage(tempresult);

                        switch (tempresult)
                        {
                            case "0x0":
                                DumpCountCombo.SelectedIndex = 0;
                                break;
                            case "0x1":
                                DumpCountCombo.SelectedIndex = 1;
                                break;
                            case "0x2":
                                DumpCountCombo.SelectedIndex = 2;
                                break;
                            case "0x3":
                                DumpCountCombo.SelectedIndex = 3;
                                break;
                            case "0x4":
                                DumpCountCombo.SelectedIndex = 4;
                                break;
                            case "0x5":
                                DumpCountCombo.SelectedIndex = 5;
                                break;
                            case "0x6":
                                DumpCountCombo.SelectedIndex = 6;
                                break;
                            case "0x7":
                                DumpCountCombo.SelectedIndex = 7;
                                break;
                            case "0x8":
                                DumpCountCombo.SelectedIndex = 8;
                                break;
                            case "0x9":
                                DumpCountCombo.SelectedIndex = 9;
                                break;
                            case "0xa":
                                DumpCountCombo.SelectedIndex = 10;
                                break;
                            default:
                                DumpCountCombo.SelectedIndex = 0;
                                break;
                        }

                    }
                    else
                    {
                        DumpCountCombo.SelectedIndex = 0;
                    }


                    await client.Send("reg query \"HKLM\\SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting\\LocalDumps\" /v DumpFolder > " + $"{LocalPath}\\cmdstring.txt");
                    string dumpfolderResult = File.ReadAllText($"{LocalPath}\\cmdstring.txt");
                    if (dumpfolderResult.Contains("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting\\LocalDumps"))
                    {

                        string tempname = dumpfolderResult.Replace("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting\\LocalDumps", "");
                        string tempname2 = tempname.Replace("DumpFolder    REG_EXPAND_SZ", "");
                        dumpfolder = Regex.Replace(tempname2, @"\s+", "");
                    }
                    else
                    {
                        Exceptions.CustomMessage(dumpfolderResult + "\nNot found in cmdstring.txt");
                        dumpfolder = "";
                    }






                    DumpsLocationBox.Text = dumpfolder;
                }

                // copy Image Update logs to Local Folder
                rpc.FileCopy("C:\\Data\\SystemData\\NonETWLogs\\ImgUpd.log", $"{LocalPath}\\ImgUpd.log", 0);
                rpc.FileCopy("C:\\Data\\SystemData\\NonETWLogs\\ImgUpd.log.cbs.log", $"{LocalPath}\\ImgUpd.log.cbs.log", 0);

                // set up Toggled events to avoid valuse changing and being written on load
                BootLogTog.Toggled += BootLogTog_Toggled;
                UefiTog.Toggled += UefiTog_Toggled;
                DumpCountCombo.SelectionChanged += DumpCountCombo_SelectionChanged;
                DumpTypeCombo.SelectionChanged += DumpTypeCombo_SelectionChanged;
                MainWindowPivot.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Exceptions.ThrowFullError(ex);
                AppBusy(false);

            }
        }


        /// <summary>
        /// View Boot Log button 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewLogBtn_Click(object sender, RoutedEventArgs e)
        {
            AppBusy(true);
            string bootLogText = File.ReadAllText($"{LocalPath}\\ntbtlog.txt");
            BootLogDisplay.Text = bootLogText;
            AppBusy(false);
        }

        /// <summary>
        /// Save the Boot Log to user specified location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SaveLogBtn_Click(object sender, RoutedEventArgs e)
        {
            AppBusy(true);
            try
            {
                StorageFile bootLogToSave = await ApplicationData.Current.LocalFolder.GetFileAsync("ntbtlog.txt");
                FolderPicker folderPicker = new FolderPicker();
                folderPicker.FileTypeFilter.Add(".txt");
                StorageFolder bootSaveFolder = await folderPicker.PickSingleFolderAsync();
                if (bootSaveFolder == null)
                {

                }
                else
                {
                    await bootLogToSave.CopyAsync(bootSaveFolder);
                    Exceptions.CustomMessage("Saved log to " + bootSaveFolder.Path + "\\" + bootLogToSave.Name);
                }
            }
            catch (Exception ex)
            {
                Exceptions.ThrowFullError(ex);
                AppBusy(false);
            }
            AppBusy(false);
        }

        /// <summary>
        /// Boot Log toggled event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BootLogTog_Toggled(object sender, RoutedEventArgs e)
        {
            AppBusy(true);
            if (BootLogTog.IsOn)
            {
                await client.Send("bcdedit /set {default} bootlog Yes > " + $"\"{LocalPath}\\cmdstring.txt\"");
                string result = File.ReadAllText($"{LocalPath}\\cmdstring.txt");

                if (result.Contains("The operation completed successfully."))
                {
                    IsBootlogEnabled = true;
                    Exceptions.CustomMessage("Enable boot logging successful");
                    SaveLogBtn.IsEnabled = true;
                    ViewLogBtn.IsEnabled = true;
                }
                else
                {
                    IsBootlogEnabled = false;
                    SaveLogBtn.IsEnabled = false;
                    ViewLogBtn.IsEnabled = false;
                    Exceptions.CustomMessage("There was an error enabling Boot Logging");
                }
            }
            else
            {
                if (IsBootlogEnabled == true)
                {
                    await client.Send("bcdedit /set {default} bootlog No > " + $"\"{LocalPath}\\cmdstring.txt\"");
                    string result = File.ReadAllText($"{LocalPath}\\cmdstring.txt");

                    if (result.Contains("The operation completed successfully."))
                    {
                        IsBootlogEnabled = false;
                        SaveLogBtn.IsEnabled = false;
                        ViewLogBtn.IsEnabled = false;
                        Exceptions.CustomMessage("Disable boot logging successful");
                    }
                    else
                    {
                        IsBootlogEnabled = true;
                        SaveLogBtn.IsEnabled = true;
                        ViewLogBtn.IsEnabled = true;
                        Exceptions.CustomMessage("There was an error disabling Boot Logging");
                    }
                }
            }
            AppBusy(false);
        }

        /// <summary>
        /// UEFI logging event toggle
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void UefiTog_Toggled(object sender, RoutedEventArgs e)
        {
            AppBusy(true);
            if (UefiTog.IsOn)
            {
                await client.Send("echo \"Created with Windows Logging Tools by Empyreal96\" > C:\\EFIESP\\Windows\\System32\\Boot\\UEFIChargingLogToDisplay.txt");
                await client.Send("if exist \"C:\\EFIESP\\Windows\\System32\\Boot\\UEFIChargingLogToDisplay.txt\" echo EXISTS > " + $"\"{LocalPath}\\cmdstring.txt\" 2>&1");
                string results2 = File.ReadAllText($"{LocalPath}\\cmdstring.txt");

                if (results2.Contains("EXISTS"))
                {
                    IsUefiLogEnabled = true;
                    Exceptions.CustomMessage("Enabled UEFI logging successfully");
                }
                else
                {
                    IsUefiLogEnabled = false;
                    Exceptions.CustomMessage("Error enabling UEFI logging");
                }
            }
            else
            {
                if (IsUefiLogEnabled == true)
                {
                    await client.Send("del C:\\EFIESP\\Windows\\System32\\Boot\\UEFIChargingLogToDisplay.txt");
                    IsUefiLogEnabled = false;
                    Exceptions.CustomMessage("Disabled UEFI logging successfully");
                }
            }
            AppBusy(false);
        }

        /// <summary>
        /// Local Dump Type combo box event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DumpTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppBusy(true);
            int value = DumpTypeCombo.SelectedIndex;

            await client.Send($"reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting\\LocalDumps\" /v DumpType /t REG_DWORD /d {value} /f > \"{LocalPath}\\cmdstring.txt\"");
            string results = File.ReadAllText($"{LocalPath}\\cmdstring.txt");
            if (results.Contains("The operation completed successfully."))
            {

            }
            else
            {
                Exceptions.CustomMessage("Error setting value for DumpType\n\n" + results);
                AppBusy(false);

            }
            AppBusy(false);
        }

        /// <summary>
        /// Local app dump count changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DumpCountCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppBusy(true);
            int result = DumpCountCombo.SelectedIndex;
            await client.Send($"reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting\\LocalDumps\" /v DumpCount /t REG_DWORD /d {result} /f > \"{LocalPath}\\cmdstring.txt\"");
            string results = File.ReadAllText($"{LocalPath}\\cmdstring.txt");
            if (results.Contains("The operation completed successfully."))
            {

            } else
            {
                Exceptions.CustomMessage("Error setting value for DumpCount\n\n" + results);
                AppBusy(false);
            }
            AppBusy(false);
        }

        /// <summary>
        /// Brows for user specified location to store crash dumps
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CrashBrowsebtn_Click(object sender, RoutedEventArgs e)
        {
            AppBusy(true);
            FolderPicker openFolder = new FolderPicker();
            openFolder.FileTypeFilter.Add(".dmp");
            StorageFolder savedFolder = await openFolder.PickSingleFolderAsync();
            if (savedFolder == null)
            {
                return;
            }
            else
            {
                try
                {
                    string savedPath = savedFolder.Path;
                    //NativeRegistry.WriteMultiString(RegistryHive.HKLM, "SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting\\LocalDumps", "DumpFolder", savedPath);
                    await client.Send("reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting\\LocalDumps\" /v DumpFolder /t REG_EXPAND_SZ /d " + $"{savedPath} /f > \"{LocalPath}\\cmdstring.txt\"");
                    string results3 = File.ReadAllText($"{LocalPath}\\cmdstring.txt");
                    if (results3.Contains("The operation completed successfully."))
                    {
                        DumpsLocationBox.Text = savedPath;
                    }
                    else
                    {
                        Exceptions.CustomMessage("An error occured while setting DumpFolder value");
                    }

                }
                catch (Exception ex)
                {
                    Exceptions.ThrowFullError(ex);
                    AppBusy(false);
                }
            }
            AppBusy(false);
        }

        /// <summary>
        /// Save basic Image Update log to user specified location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SaveUpdateBasicLog_Click(object sender, RoutedEventArgs e)
        {
            AppBusy(true);
            try
            {


                StorageFile UpdLogToSave = await ApplicationData.Current.LocalFolder.GetFileAsync("ImgUpd.log");
                FolderPicker folderPicker = new FolderPicker();
                folderPicker.FileTypeFilter.Add(".log");
                StorageFolder UpdSaveFolder = await folderPicker.PickSingleFolderAsync();
                if (UpdSaveFolder == null)
                {

                }
                else
                {
                    await UpdLogToSave.CopyAsync(UpdSaveFolder);
                    Exceptions.CustomMessage("Saved log to " + UpdSaveFolder.Path + "\\" + UpdLogToSave.Name);
                }
            }
            catch (Exception ex)
            {
                Exceptions.ThrowFullError(ex);
                AppBusy(false);
            }
            AppBusy(false);
        }

        /// <summary>
        /// Display the basic Image Update log
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewUpdateBasicLog_Click(object sender, RoutedEventArgs e)
        {
            AppBusy(true);
            try
            {
                UpdateLogText.Text = "";
                string UpdLogText = File.ReadAllText($"{LocalPath}\\ImgUpd.log");

                UpdateLogText.Text = UpdLogText;
            }
            catch (Exception ex)
            {
                Exceptions.ThrowFullError(ex);
                AppBusy(false);
            }
            AppBusy(false);
        }

        /// <summary>
        /// Save the advanced Image Update log to user specified location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SaveUpdateAdvLog_Click(object sender, RoutedEventArgs e)
        {
            AppBusy(true);
            try
            {
                StorageFile UpdLogToSave = await ApplicationData.Current.LocalFolder.GetFileAsync("ImgUpd.log.cbs.log");
                FolderPicker folderPicker = new FolderPicker();
                folderPicker.FileTypeFilter.Add(".log");
                StorageFolder UpdSaveFolder = await folderPicker.PickSingleFolderAsync();
                if (UpdSaveFolder == null)
                {

                }
                else
                {
                    await UpdLogToSave.CopyAsync(UpdSaveFolder);
                    Exceptions.CustomMessage("Saved log to " + UpdSaveFolder.Path + "\\" + UpdLogToSave.Name);
                }
            }

            catch (Exception ex)
            {
                Exceptions.ThrowFullError(ex);
                AppBusy(false);
            }
            AppBusy(false);
        }

        /// <summary>
        /// (Disabled for now) View the Advanced Image Update log
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewUpdateAdvLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateLogText.Text = "";
                string UpdLogText = File.ReadAllText($"{LocalPath}\\ImgUpd.log.cbs.log");

                UpdateLogText.Text = UpdLogText;
            }
            catch (Exception ex)
            {
                Exceptions.ThrowFullError(ex);
            }
        }

        /// <summary>
        /// Change working status of the progress bar and other UI elements
        /// </summary>
        /// <param name="enable"></param>
        private void AppBusy(bool enable)
        {
            if (enable == true)
            {
                AppBusyBar.IsEnabled = true;
                AppBusyBar.Visibility = Visibility.Visible;
                AppBusyBar.IsIndeterminate = true;
            } else
            {
                AppBusyBar.IsEnabled = false;
                AppBusyBar.Visibility = Visibility.Collapsed;
                AppBusyBar.IsIndeterminate = false;
            }
        }
    }
}
