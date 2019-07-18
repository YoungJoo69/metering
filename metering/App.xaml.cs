﻿using MahApps.Metro;
//using Squirrel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;

namespace metering
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public enum ApplicationExitCode
        {
            Success = 0,
            Failure = 1,
            CantWriteToApplicationLog = 2,
            CantPersistApplicationState = 3
        }

        //private static Task<UpdateManager> _updateManager = null;

        /// <summary>
        /// Main App
        /// </summary>
        static App()
        {
            // This code is used to test the app when using other cultures.
            //
            //System.Threading.Thread.CurrentThread.CurrentUICulture =
            //    System.Threading.Thread.CurrentThread.CurrentUICulture =
            //    new System.Globalization.CultureInfo ( "it-IT" );

            // Ensure the current culture passed into bindings is the OS culture.
            // By default, WPF uses en-US as the culture, regardless of the system settings.
            //
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

        }

        //private static async void CheckForUpdates()
        //{
        //    _updateManager = UpdateManager.GitHubUpdateManager("https://bitbucket.org/tbircek/metering/", "metering");

        //    if (_updateManager.Result.IsInstalledApp)
        //    {
        //        await _updateManager.Result.UpdateApp();
        //    }
        //}

        /// <summary>
        /// Override Startup event to show default view and viewmodel.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            //// get the current app style (theme and accent) from the application
            //// you can then use the current theme and custom accent instead set a new theme
            //Tuple<AppTheme, Accent> appStyle = ThemeManager.DetectAppStyle(Application.Current);

            //// now set the Green accent and dark theme
            ThemeManager.ChangeAppStyle(Current,
                                        ThemeManager.GetAccent("Red"),
                                        ThemeManager.GetAppTheme("BaseLight")); // or appStyle.Item1

            base.OnStartup(e);

            //// Create default view.
            //MainWindow window = new MainWindow();

            //window.Show();

            //Task.Factory.StartNew(CheckForUpdates);
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Process unhandled exception
            WriteApplicationLogEntry("Unhandled failure. ", 1);

            // Prevent default unhandled exception processing
            e.Handled = true;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            //_updateManager?.Dispose();
            App_Exit(null, e);
            base.OnExit(e);
        }

        void App_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                // Write entry to application log
                if (e.ApplicationExitCode == (int)ApplicationExitCode.Success)
                {
                    WriteApplicationLogEntry("Success", e.ApplicationExitCode);
                }
                else
                {
                    WriteApplicationLogEntry("Failure", e.ApplicationExitCode);
                }
            }
            catch
            {
                // Update exit code to reflect failure to write to application log
                e.ApplicationExitCode = (int)ApplicationExitCode.CantWriteToApplicationLog;
            }

            // Persist application state
            try
            {
                PersistApplicationState();
            }
            catch
            {
                // Update exit code to reflect failure to persist application state
                e.ApplicationExitCode = (int)ApplicationExitCode.CantPersistApplicationState;
            }
        }

        void WriteApplicationLogEntry(string message, int exitCode)
        {
            // Write log entry to file in isolated storage for the user
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForAssembly();
            
            using (Stream stream = new IsolatedStorageFileStream("log.txt", FileMode.Append, FileAccess.Write, store))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                string entry = string.Format("{0}: {1} - {2}", message, exitCode, DateTime.Now);
                writer.WriteLine(entry);
            }
        }

        void PersistApplicationState()
        {
            // Persist application state to file in isolated storage for the user
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForAssembly();
            using (Stream stream = new IsolatedStorageFileStream("state.txt", FileMode.Create, store))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                foreach (DictionaryEntry entry in Properties)
                {
                    writer.WriteLine(entry.Value);
                }
            }
        }
    }
}
