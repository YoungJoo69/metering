﻿using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace metering.core
{
    /// <summary>
    /// Handles Commands page.
    /// </summary>
    public class CommandsViewModel : BaseViewModel
    {
        #region Private Members

        #endregion

        #region Public Properties

        /// <summary>
        /// Cancellation token source for Omicron Test Set to stop and power down Omicron Test Set.
        /// </summary>
        public CancellationTokenSource TokenSource { get; private set; }

        /// <summary>
        /// Cancellation token for Omicron Test Set to stop and power down Omicron Test Set.
        /// </summary>
        public CancellationToken Token { get; set; }

        /// <summary>
        /// True if the user hit "+" button
        /// </summary>
        public bool NewTestAvailable { get; set; } = false;

        /// <summary>
        /// Holds visibility information of "Start test" and "Save test" button
        /// </summary>
        public bool StartTestAvailable { get; set; } = false;

        /// <summary>
        /// Holds visibility information of "Cancel tests" button
        /// </summary>
        public bool Cancellation { get; set; } = false;

        /// <summary>
        /// Holds visibility information of "Hardware Configuration" button
        /// </summary>
        public bool ConfigurationAvailable { get; set; } = true;

        /// <summary>
        /// Allows animation of "Hardware Configuration" button
        /// </summary>
        public bool IsConfigurationAvailable { get; set; } = false;

        /// <summary>
        /// Hold visibility information of "LoadTest" button
        /// </summary>
        public bool LoadTestAvailable { get; set; } = true;

        /// <summary>
        /// Holds Foreground color information for the Start Test Command button
        /// </summary>
        public string StartTestForegroundColor { get; set; }

        /// <summary>
        /// Holds Foreground color information for the Cancel Command button
        /// </summary>
        public string CancelForegroundColor { get; set; }

        /// <summary>
        /// Set ProgressAssist IsIndicatorVisible on the floating StartTestCommand button
        /// </summary>
        public bool IsConnecting { get; set; }

        /// <summary>
        /// To change icon on the floating StartTestCommand button
        /// </summary>
        public bool IsConnectionCompleted { get; set; }

        /// <summary>
        /// Progress percentage of the test completion
        /// </summary>
        public double TestProgress { get; set; }

        /// <summary>
        /// Sets maximum value for the progress bar around StartTestCommand button
        /// </summary>
        public double MaximumTestCount { get; set; }

        #endregion

        #region Public Commands

        /// <summary>
        /// The command to handle change view to test plan detail view
        /// and populate items with nominal values
        /// </summary>
        public ICommand AddNewTestCommand { get; set; }

        /// <summary>
        /// The command handles canceling New Test addition view and returns default view
        /// </summary>
        public ICommand CancelNewTestCommand { get; set; }

        /// <summary>
        /// The command to handle connecting associated Omicron Test Set
        /// and communication to the UUT
        /// </summary>
        public ICommand StartTestCommand { get; set; }

        /// <summary>
        /// The command to handle saving test step to the user specified location
        /// default location interface opens at "\\my documents\\metering\\tests"
        /// </summary>
        public ICommand SaveNewTestCommand { get; set; }

        /// <summary>
        /// The command to handle loading test step(s) from the user specified location
        /// default location interface opens at "\\my documents\\metering\\tests"
        /// </summary>
        public ICommand LoadTestsCommand { get; set; }

        /// <summary>
        /// The command to handle removing test step(s) from the test strip in the <see cref="CommunicationViewModel"/>
        /// this command will not delete any test step from folder that located at "\\my documents\\metering\\tests"
        /// </summary>
        public ICommand DeleteSelectedTestCommand { get; set; }

        /// <summary>
        /// The command to handle Omicron Hardware Configuration settings view
        /// </summary>
        public ICommand OmicronHardwareConfigurationCommand { get; set; }
        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public CommandsViewModel()
        {
            // make aware of culture of the computer
            // in case this software turns to something else.
            CultureInfo ci = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = ci;

            // Show a new Test details page populated with the user specified/accepted values
            AddNewTestCommand = new RelayCommand(() => IoC.NominalValues.CopyNominalValues());

            // starts the test specified by the user.
            StartTestCommand = new RelayCommand(async () => await ConnectOmicronAndUnitAsync());

            // navigate back to nominal values page.
            CancelNewTestCommand = new RelayCommand(() => CancelTestDetailsPageShowing());

            // save the test step to the user specified location.
            SaveNewTestCommand = new RelayCommand(async () => await IoC.Commander.ShowFileDialogAsync(FileDialogOption.Save));

            // load the test step(s) from the user specified location.
            LoadTestsCommand = new RelayCommand(async () => await IoC.Commander.ShowFileDialogAsync(FileDialogOption.Open));

            // show the Omicron Hardware Configuration Settings page.
            OmicronHardwareConfigurationCommand = new RelayCommand(async () => await IoC.Settings.HardwareConfiguration());

            // default StartTestForegroundColor is Green
            StartTestForegroundColor = "00ff00";

            // default CancelForegroundColor is Green
            CancelForegroundColor = "00ff00";
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Navigate backwards to main view / shows default nominal values
        /// resets values specified in test step view to nominal values
        /// </summary>
        private void CancelTestDetailsPageShowing()
        {

            // set visibility of "Cancel tests" button
            Cancellation = false;

            // set visibility of "Hardware Configuration" button and animation
            ConfigurationAvailable = true;
            IsConfigurationAvailable = false;

            // if the test completed normally no need to cancel token as it is canceled already
            if (IoC.CMCControl.IsTestRunning)
            {
                // set visibility of the Command Buttons
                StartTestAvailable = true;
                NewTestAvailable = false;
                Cancellation = true;

                // reset maximum value for progress bar for the next run
                MaximumTestCount = 0d;

                // change CancelForegroundColor to Red
                CancelForegroundColor = "00ff00";

                // reset test progress to show test canceled.
                TestProgress = 0d;

                // check if Omicron Test Set is running
                // if the user never started test and press "Back" button 
                // DeviceID would be a zero
                if (IoC.CMCControl.DeviceID > 0)
                {
                    // try to stop Omicron Test Set gracefully
                    IoC.Task.Run(async () => await IoC.ReleaseOmicron.ProcessErrorsAsync(true));
                }
            }
            else
            {

                // Update NominalValues RadioButtons to run a PropertyUpdate event
                IoC.NominalValues.SelectedVoltagePhase = "AllZero";
                IoC.NominalValues.SelectedCurrentPhase = "AllZero";

                // set visibility of the Command Buttons
                StartTestAvailable = false;
                LoadTestAvailable = true;
                Cancellation = false;
                NewTestAvailable = false;

                // Show NominalValues page
                IoC.Application.GoToPage(ApplicationPage.NominalValues, new NominalValuesViewModel());

                // reset items in TestDetails page.
                IoC.TestDetails.SelectedVoltageConfiguration = new SettingsListItemViewModel();
                IoC.TestDetails.SelectedCurrentConfiguration = new SettingsListItemViewModel();
                IoC.TestDetails.AnalogSignals = new ObservableCollection<AnalogSignalListItemViewModel>();
                IoC.TestDetails.Register = "2279";
                IoC.TestDetails.Progress = "0.0";
                IoC.TestDetails.DwellTime = "20";
                IoC.TestDetails.StartDelayTime = "0.1";
                IoC.TestDetails.MeasurementInterval = "250";
                IoC.TestDetails.StartMeasurementDelay = "5";
                IoC.TestDetails.Selected = IoC.TestDetails.IsPhase = IoC.TestDetails.IsFrequency = IoC.TestDetails.IsLinked = false;
                IoC.TestDetails.IsMagnitude = true;
                IoC.TestDetails.SelectedRampingSignal = "Magnitude";                
                IoC.TestDetails.TestFileName = string.Empty;

                // reset selected voltage hardware configuration
                IoC.Settings.OmicronVoltageOutputs = new ObservableCollection<SettingsListItemViewModel>();
                IoC.Settings.VoltageDiagramLocation = "../Images/Omicron/not used voltage.png";
                IoC.Settings.SelectedVoltage = "not used voltage";

                // reset selected current hardware configuration
                IoC.Settings.OmicronCurrentOutputs = new ObservableCollection<SettingsListItemViewModel>();
                IoC.Settings.CurrentDiagramLocation = "../Images/Omicron/not used current.png";
                IoC.Settings.SelectedCurrent = "not used current";
            }
        }

        /// <summary>
        /// connects to omicron and test unit.
        /// </summary>    
        private async Task ConnectOmicronAndUnitAsync()
        {
            // there is a test set attached so run specified tests.
            // lock the task
            await AsyncAwaiter.AwaitAsync(nameof(ConnectOmicronAndUnitAsync), async () =>
            {

                // decides which signal is our ramping signal by comparing the mismatch of any "From" and "To" values.
                // after the user clicked "Go" button
                TestSignal testSignal = new TestSignal();

                // is any signal ramping?
                if (testSignal.IsRamping)
                {
                    // is the user selected a hardware configuration?
                    if (testSignal.IsRunningPermitted)
                    {

                        // define the cancellation token source.
                        TokenSource = new CancellationTokenSource();

                        // define the cancellation token to use 
                        // terminate tests prematurely.
                        Token = TokenSource.Token;

                        // Run test command
                        await IoC.Task.Run(() => IoC.TestDetails.ConnectCommand.Execute(IoC.TestDetails), Token);

                    }
                    else
                    {
                        // inform the user there is no hardware configuration available
                        IoC.Communication.Log = $"{DateTime.Now.ToLocalTime():MM/dd/yy HH:mm:ss.fff}: Running test is not permitted due to hardware configuration. Please check your configuration.";
                    }
                }
                else
                {
                    // inform the user there is no test case
                    IoC.Communication.Log = $"{DateTime.Now.ToLocalTime():MM/dd/yy HH:mm:ss.fff}: There is no ramping signal. Please check your entries.";
                }
            });
        }

        #endregion
    }
}
