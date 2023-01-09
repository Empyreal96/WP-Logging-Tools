﻿using ndtklib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Telnet;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Logging_Enabler
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FirstRunPage : Page
    {
        bool IsCMDPresent;

        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        TelnetClient client = new TelnetClient(TimeSpan.FromSeconds(3), cancellationTokenSource.Token);
        NRPC rpc = new NRPC();

        public FirstRunPage()
        {
            try
            {
                this.InitializeComponent();


                progbar.IsEnabled = true;

                CMDpresent.Text = "Checking capabilities, please wait...";
                try
                {
                    rpc.Initialize();
                }
                catch (Exception ex)
                {
                    Exceptions.CustomMessage("Error Initilizing Interop Services, Are you Interop Unlocked?");
                    CMDpresent.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                    CMDpresent.Text = "Interop Service Failed";
                    progbar.IsEnabled = false;
                    progbar.IsIndeterminate = false;
                    return;
                }

                Connect();
                progbar.IsEnabled = false;
            }
            catch (Exception ex)
            {
                progbar.IsEnabled = false;

                Exceptions.ThrowFullError(ex);
            }

        }
        /// <summary>
        /// Connect function here checks for CMD access
        /// TODO: add Interop/NDTK checks
        /// </summary>
        private async void Connect()
        {
            try
            {

                await client.Connect();
                //  await Task.Delay(1000);
                await client.Send($"set");
                IsCMDPresent = true;

            }
            catch (Exception ex)
            {
                Exceptions.ThrowFullError(ex);
                IsCMDPresent = false;
            }

            if (IsCMDPresent == true)
            {
                CMDpresent.Foreground = new SolidColorBrush(Windows.UI.Colors.Green);
                CMDpresent.Text = "Checks Completed Successfully\nCopying files needed";
                try
                {
                    // Copy the Kernel Debug files used when enabling debugging
                    rpc.FileCopy($"{ApplicationData.Current.LocalFolder.Path}\\KDFiles\\kd.dll", "C:\\Windows\\System32\\kd.dll", 0);
                    rpc.FileCopy($"{ApplicationData.Current.LocalFolder.Path}\\KDFiles\\kdusb.dll", "C:\\Windows\\System32\\kdusb.dll", 0);
                    rpc.FileCopy($"{ApplicationData.Current.LocalFolder.Path}\\KDFiles\\kdstub.dll", "C:\\Windows\\System32\\kdstub.dll", 0);
                    rpc.FileCopy($"{ApplicationData.Current.LocalFolder.Path}\\KDFiles\\kdcom.dll", "C:\\Windows\\System32\\kdcom.dll", 0);
                    rpc.FileCopy($"{ApplicationData.Current.LocalFolder.Path}\\KDFiles\\kd_02_10df.dll", "C:\\Windows\\System32\\kd_02_10df.dll", 0);
                    rpc.FileCopy($"{ApplicationData.Current.LocalFolder.Path}\\KDFiles\\kd_02_10ec.dll", "C:\\Windows\\System32\\kd_02_10ec.dll", 0);
                    rpc.FileCopy($"{ApplicationData.Current.LocalFolder.Path}\\KDFiles\\kd_02_14e4.dll", "C:\\Windows\\System32\\kd_02_14e4.dll", 0);
                    rpc.FileCopy($"{ApplicationData.Current.LocalFolder.Path}\\KDFiles\\kd_02_1969.dll", "C:\\Windows\\System32\\kd_02_1969.dll", 0);
                    rpc.FileCopy($"{ApplicationData.Current.LocalFolder.Path}\\KDFiles\\kd_02_19a2.dll", "C:\\Windows\\System32\\kd_02_19a2.dll", 0);
                    rpc.FileCopy($"{ApplicationData.Current.LocalFolder.Path}\\KDFiles\\kd_02_1af4.dll", "C:\\Windows\\System32\\kd_02_1af4.dll", 0);
                    rpc.FileCopy($"{ApplicationData.Current.LocalFolder.Path}\\KDFiles\\kd_02_8086.dll", "C:\\Windows\\System32\\kd_02_8086.dll", 0);
                    rpc.FileCopy($"{ApplicationData.Current.LocalFolder.Path}\\KDFiles\\kd_07_1415.dll", "C:\\Windows\\System32\\kd_07_1415.dll", 0);
                    rpc.FileCopy($"{ApplicationData.Current.LocalFolder.Path}\\KDFiles\\kd_8003_5143.dll", "C:\\Windows\\System32\\kd_8003_5143.dll", 0);
                    rpc.FileCopy($"{ApplicationData.Current.LocalFolder.Path}\\KDFiles\\kdnet.dll", "C:\\Windows\\System32\\kdnet.dll", 0);
                    rpc.FileCopy($"{ApplicationData.Current.LocalFolder.Path}\\KDFiles\\kdnet_uart16550.dll.dll", "C:\\Windows\\System32\\kdnet_uart16550.dll.dll", 0);
                    rpc.FileCopy($"{ApplicationData.Current.LocalFolder.Path}\\KDFiles\\kd.dll", "C:\\Windows\\System32\\kd.dll", 0);

                }
                catch (Exception ex)
                {
                    Exceptions.ThrowFullError(ex);
                }
                // Test if all KD flles are needed
                CMDpresent.Text = "Setup completed";
                FinishBtn.IsEnabled = true;
                progbar.IsEnabled = false;
                progbar.IsIndeterminate = false;

            }
            else
            {
                CMDpresent.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                CMDpresent.Text = "CMD Access Not Found!";

                progbar.IsEnabled = false;
                progbar.IsIndeterminate = false;


            }
            progbar.IsEnabled = false;
        }
        string LocalPath = ApplicationData.Current.LocalFolder.Path;

        private IAsyncOperation<IUICommand> dialogTask;
        IPropertySet roamingProperties = ApplicationData.Current.RoamingSettings.Values;
        private async void FinishBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                roamingProperties["FirstRunDone"] = bool.TrueString;
                client.Disconnect();
                // The following code is a workaround to a bug. After finishing first run checks the values in MainPage don't get read/load until app restarts
                var ThrownException = new MessageDialog("App will close in 10 seconds for configuration to load properly, please reopen this app to continue.");
                ThrownException.Commands.Add(new UICommand("Close"));
                try
                {
                    dialogTask = ThrownException.ShowAsync();

                }
                catch (TaskCanceledException)
                {


                }
                DispatcherTimer dt = new DispatcherTimer();
                dt.Interval = TimeSpan.FromSeconds(10);
                dt.Tick += dt_Tick;
                dt.Start();


            }
            catch (Exception ex)
            {
                Exceptions.ThrowFullError(ex);
            }
        }

        void dt_Tick(object sender, object e)
        {
            (sender as DispatcherTimer).Stop();
            dialogTask.Cancel();
            Application.Current.Exit();
        }
        /// <summary>
        /// Copy loopback command to enable CMD access for the app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoopCmd_Tapped(object sender, TappedRoutedEventArgs e)
        {
            DataPackage dataPackage = new DataPackage();
            string command = "checknetisolation loopbackexempt -a -n=WindowsLoggingTools_6dg21qtxnde1e";
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(command);
            Clipboard.SetContent(dataPackage);
            Exceptions.CustomMessage("'checknetisolation loopbackexempt -a -n=WindowsLoggingTools_6dg21qtxnde1e' copied to clipboard");
        }
    }
}
