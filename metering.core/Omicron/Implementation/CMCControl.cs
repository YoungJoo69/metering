﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace metering.core
{
    /// <summary>
    /// Handles Omicron Test Set operations.
    /// </summary>
    public class CMCControl

    {

        #region Private Members

        /// <summary>
        /// Omicron CM Engine
        /// </summary>
        private CMEngine.CMEngine engine;

        #endregion

        #region Public Properties

        /// <summary>
        /// Timer ticks used to read ModbusClient
        /// </summary>
        public Timer MdbusTimer { get; set; }

        /// <summary>
        /// Omicron CM Engine
        /// </summary>
        public CMEngine.CMEngine CMEngine
        {
            get
            {

                // if engine is null return a new engine
                if (engine == null)
                    return new CMEngine.CMEngine();

                // return engine
                return engine;
            }
            // set engine
            set => engine = value;
        }

        /// <summary>
        /// holds information about the test completion.
        /// if true test ran and completed without any interruption
        /// else the user interrupted the test
        /// </summary>
        public bool IsTestRunning { get; set; }

        /// <summary>
        /// Holds minimum value for test register.
        /// </summary>
        public int[] MinValues { get; set; }

        /// <summary>
        /// Holds minimum value for test register.
        /// </summary>
        public int[] MaxValues { get; set; }

        /// <summary>
        /// Holds total value for test register.
        /// </summary>
        public int[] Totals { get; set; }

        /// <summary>
        /// Holds average value for test register.
        /// </summary>
        public double[] Averages { get; set; }

        /// <summary>
        /// Holds successful reading number of the test register.
        /// </summary>
        public int[] SuccessCounters { get; set; }

        /// <summary>
        /// Hold progress information per test register.
        /// </summary>
        public double Progress { get; set; }

        /// <summary>
        /// Associated Omicron Test Set ID. Assigned by CM Engine.
        /// </summary>
        public int DeviceID { get; set; }

        /// <summary>
        /// Associated Omicron Test Set Information. Assigned by Omicron.
        /// </summary>
        public string DeviceInfo { get; set; }

        /// <summary>
        /// Omicron Test Set debugging log levels.
        /// </summary>
        public enum OmicronLoggingLevels : short { None, Level1, Level2, Level3, };

        /// <summary>
        /// Holds the test folder location
        /// </summary>
        public string TestsFolder => Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "metering\\tests"));

        /// <summary>
        /// Holds the results folder location
        /// </summary>
        public string ResultsFolder => Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "metering\\results"));

        /// <summary>
        /// Holds the logs folder location.
        /// Omicron logs and app logs.
        /// </summary>
        public string LogsFolder => Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "metering\\logs"));
        #endregion

        #region Public Methods

        /// <summary>
        /// Runs Test Steps
        /// </summary>
        /// <returns>Returns nothing</returns>
        public async Task TestAsync()
        {
            try
            {

                // update test progress
                int progressStep = default;

                // report file id to distinguish between test results 
                string fileId = $"{DateTime.Now.ToLocalTime():yyyy_MM_dd_HH_mm}";

                // check if the test is harmonic
                // yes => include harmonics portion of the test
                // no => move to core metering testing
                TestHarmonics testHarmonics = new TestHarmonics();

                // loads TestSignal information as a Tuple
                var (HarmonicStart, HarmonicEnd, HarmonicOrders, TotalHarmonicTestCount) = testHarmonics.GetHarmonicOrder();

                // decides which signal is our ramping signal by comparing the mismatch of any "From" and "To" values.
                // after the user clicked "Go" button
                TestSignal testSignal = new TestSignal();

                // loads TestSignal information as a Tuple
                var (SignalName, From, To, Delta, Phase, Frequency, Precision) = testSignal.GetRampingSignal();

                // initialize new testStartValue
                double testStartValue = default;

                //// precision format text to format strings
                //string precision = $"F{Precision}";

                // verify we have a ramping signal
                if (string.IsNullOrWhiteSpace(SignalName))
                {
                    // inform the user there is no test case
                    IoC.Communication.Log = $"{DateTime.Now.ToLocalTime():MM/dd/yy HH:mm:ss.fff}: There is no ramping signal.Please check your entries.";

                    // return from this task.
                    return;
                }

                //// generate fixed portion of header information for reporting.
                //StringBuilder @string = new StringBuilder("Time, Test Value");

                //// generate variable portion of header information based on Register entry.
                //IEnumerable<int> registerValues = Enumerable.Range(0, IoC.TestDetails.Register.ToString().Split(',').Length).Select(i => i);

                //foreach (int registerValue in registerValues)
                //{
                //    // append new variable header information to previous string generated.
                //    @string.Append($" ,Register {registerValue + 1}, Minimum Value {registerValue + 1}, Maximum Value {registerValue + 1}, Average Value {registerValue + 1}, Good Reading Value {registerValue + 1}");
                //}

                //// add new line to generated string for the test values.
                //@string.AppendLine();

                // inform the developer of test SignalName
                await Task.Run(() => IoC.Logger.Log($"Test signal name: {SignalName}", LogLevel.Informative));

                // set maximum value for the progress bar
                await Task.Run(() => IoC.Commands.MaximumTestCount = TotalHarmonicTestCount * (Math.Ceiling((Math.Abs(To - From) / Delta)) + 1));

                // inform the developer MaximumTestCount
                await Task.Run(() => IoC.Logger.Log($"Maximum test count: {IoC.Commands.MaximumTestCount}", LogLevel.Informative));

                // Wait StartDelayTime to start Modbus communication
                IoC.Task.Run(async () =>
                {
                    // lock the task
                    await AsyncAwaiter.AwaitAsync(nameof(TestAsync), async () =>
                    {
                        // wait for the user specified "Start Delay Time"
                        await Task.Delay(TimeSpan.FromMinutes(Convert.ToDouble(IoC.TestDetails.StartDelayTime)), IoC.Commands.Token);
                    });

                    // Progress bar is visible
                    IoC.Commands.IsConnectionCompleted = IoC.Commands.IsConnecting = IoC.Communication.EAModbusClient.Connected;

                    // change color of Cancel Command button to Green
                    IoC.Commands.CancelForegroundColor = "00ff00";

                    // test starts 
                    progressStep = 0;

                }, IoC.Commands.Token).Wait();

                // overall every harmonic test
                foreach (var harmonicTest in HarmonicOrders)
                {

                    // if there is no harmonics test this should run once.
                    // if there is harmonics test should run From "HarmonicStart" To "HarmonicEnd"
                    for (int orderNumber = HarmonicStart[HarmonicOrders.IndexOf(harmonicTest)]; orderNumber < HarmonicEnd[HarmonicOrders.IndexOf(harmonicTest)] + 1; orderNumber++)
                    {
                        // update testing harmonic number for file naming
                        IoC.Communication.TestingHarmonicOrder = orderNumber;

                        // generate fixed portion of header information for reporting.
                        StringBuilder @string = new StringBuilder("Time, Test Value");

                        // generate variable portion of header information based on Register entry.
                        IEnumerable<int> registerValues = Enumerable.Range(0, IoC.TestDetails.Register.ToString().Split(',').Length).Select(i => i);

                        foreach (int registerValue in registerValues)
                        {
                            // append new variable header information to previous string generated.
                            @string.Append($" ,Register {registerValue + 1}, Minimum Value {registerValue + 1}, Maximum Value {registerValue + 1}, Average Value {registerValue + 1}, Good Reading Value {registerValue + 1}");
                        }

                        // add new line to generated string for the test values.
                        @string.AppendLine();

                        // Process test steps
                        // due to nature of double calculations use this formula "testStartValue <= (To + Delta * 1 / 1000)" to make sure the last test step always runs
                        for (testStartValue = From; testStartValue <= (To + Delta * 1 / 1000); testStartValue += Delta)
                        {
                            // check if the user canceled the tests.
                            if (!IoC.Commands.Token.IsCancellationRequested)
                            {

                                // inform the developer about test register and start value.
                                await Task.Run(() => IoC.Logger.Log($"Register: {IoC.TestDetails.Register}\tTest value: {testStartValue:F6} started", LogLevel.Informative));

                                // inform the user about test register and start value.
                                IoC.Communication.Log = await Task.Run(() => $"{DateTime.Now.ToLocalTime():MM/dd/yy HH:mm:ss.fff}: Register: {IoC.TestDetails.Register} --- Test value: {testStartValue:F6} started");

                                // send Omicron commands
                                await IoC.Task.Run(async () =>
                                {
                                    // initialize Minimum Value List with the highest int32 to capture every minimum value
                                    MinValues = Enumerable.Repeat(int.MaxValue, IoC.TestDetails.Register.ToString().Split(',').Length).ToArray();

                                    // initialize Maximum Value List with the lowest int32 to capture every maximum value
                                    MaxValues = Enumerable.Repeat(int.MinValue, IoC.TestDetails.Register.ToString().Split(',').Length).ToArray();

                                    // initialize totals with 0.
                                    Totals = Enumerable.Repeat(0, IoC.TestDetails.Register.ToString().Split(',').Length).ToArray();

                                    // initialize average with 0.
                                    Averages = Enumerable.Repeat(0.0, IoC.TestDetails.Register.ToString().Split(',').Length).ToArray();

                                    // initial successful reading with 0.
                                    SuccessCounters = Enumerable.Repeat(0, IoC.TestDetails.Register.ToString().Split(',').Length).ToArray();

                                    // send string commands to Omicron
                                    await IoC.Task.Run(() => IoC.SetOmicron.SendOmicronCommands(SignalName, testStartValue));

                                    // delay little bit Omicron CMC power up
                                    await Task.Delay(1000);

                                    // Turn On Omicron Analog Outputs per the user input
                                    await IoC.Task.Run(() => IoC.PowerOptions.TurnOnCMC());

                                    // set timer to read modbus register per the user specified time.
                                    MdbusTimer =
                                                await Task.Run(() =>
                                                    new Timer(
                                                        // reads the user specified modbus register(s).
                                                        callback: IoC.Modbus.MeasurementIntervalCallback,
                                                        // pass the use specified modbus register(s) to callback.
                                                        state: IoC.TestDetails.Register,
                                                        // the time delay before modbus register(s) read start. 
                                                        dueTime: TimeSpan.FromSeconds(Convert.ToDouble(IoC.TestDetails.StartMeasurementDelay)),
                                                        // the interval between reading of the modbus register(s).
                                                        period: TimeSpan.FromMilliseconds(Convert.ToDouble(IoC.TestDetails.MeasurementInterval))));

                                    // cancel this task if cancellation requested
                                }, IoC.Commands.Token);

                                // Start reading the user specified Register
                                IoC.Task.Run(async () =>
                                {
                                    // lock the task
                                    await AsyncAwaiter.AwaitAsync(nameof(TestAsync) + testStartValue, async () =>
                                            {

                                            // wait until the user specified "Dwell Time" expires.
                                            await Task.Delay(TimeSpan.FromSeconds(Convert.ToDouble(IoC.TestDetails.DwellTime) + Convert.ToDouble(IoC.TestDetails.StartMeasurementDelay)));
                                            });

                                    // terminate reading modbus register because "Dwell Time" is over.
                                    MdbusTimer?.Dispose();

                                    // Time Test Value Register 1  Min Value 1 Max Value 1 Register 2  Min Value 2 Max Value 2 Register 3  Min Value 3 Max Value 3 ... etc.
                                    await Task.Run(() => @string.Append(value: $"{DateTime.Now.ToLocalTime():MM/dd/yy HH:mm:ss.fff},{testStartValue:F6}"));

                                    // generate variable portion of header information based on Register entry.
                                    foreach (int i in registerValues)
                                    {
                                        // append new variable header information to previous string generated.
                                        await Task.Run(() => @string.Append(value: $" ,{IoC.TestDetails.Register.ToString().Split(',').GetValue(i)} ,{MinValues[i]:F6} , {MaxValues[i]:F6}, {Averages[i]:F6}, {SuccessCounters[i]}"));
                                    }

                                    // wait task to be over with
                                }, IoC.Commands.Token).Wait();

                                // update test result report
                                await IoC.Task.Run(() =>
                                {
                                    // initialize test details string.
                                    string testDetailsFileName = string.Empty;

                                    if (string.IsNullOrWhiteSpace(IoC.TestDetails.TestFileName))
                                    {
                                        // test result file name contains Register, From, To, and test start time values
                                        testDetailsFileName = $"{(IoC.TestDetails.IsHarmonics ? $"[{IoC.Communication.TestingHarmonicOrder.ToString()}]" : string.Empty)}{IoC.TestDetails.Register}_{From:F6}-{To:F6}_{fileId}";
                                    }
                                    else
                                    {
                                        // test result file name contains "Test File Name" per the user input.
                                        // file name might contain multiple "."
                                        testDetailsFileName = $"{(IoC.TestDetails.IsHarmonics ? $"[{IoC.Communication.TestingHarmonicOrder.ToString()}]" : string.Empty)}{IoC.TestDetails.TestFileName.Substring(0, IoC.TestDetails.TestFileName.LastIndexOf('.'))}_{fileId}";
                                    }

                                    // create a TestResultLogger to generate a test report in .csv format.
                                    new TestResultLogger
                                                 (
                                                     // set file path and name
                                                     filePath: Path.Combine(ResultsFolder, $"{testDetailsFileName}.csv"),
                                                     // no need to save time
                                                     logTime: false
                                                 ).Log(@string.ToString(), LogLevel.Informative);
                                });

                                // increment progress bar strip on the "Button"
                                IoC.Commands.TestProgress = Convert.ToDouble(progressStep);

                                // increment progress
                                progressStep++;

                                // inform the developer about test register and start value.
                                await Task.Run(() => IoC.Logger.Log($"Register: {IoC.TestDetails.Register} --- Test value: {testStartValue:F6} completed", LogLevel.Informative));

                                // inform the user about test register and start value.
                                IoC.Communication.Log = await Task.Run(() => $"{DateTime.Now.ToLocalTime():MM/dd/yy HH:mm:ss.fff}: Register: {IoC.TestDetails.Register} --- Test value: {testStartValue:F6} completed.");

                                // inform the developer
                                await Task.Run(() => IoC.Logger.Log($"Test {progressStep} of {IoC.Commands.MaximumTestCount} completed", LogLevel.Informative));

                                // increment progress percentage
                                Progress = await Task.Run(() => progressStep / IoC.Commands.MaximumTestCount);

                                // reset message for the next test step
                                @string.Clear();

                                // delay little bit for communication to clear up
                                await Task.Delay(Convert.ToInt32(IoC.TestDetails.MeasurementInterval) * 2);
                            }
                            else
                            {
                                // if timer is initialized terminate reading modbus register because the user canceled the test.
                                MdbusTimer?.Dispose();

                                // throw an error if we canceled already.
                                IoC.Commands.Token.ThrowIfCancellationRequested();

                                // exit from this task
                                return;
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                // inform the developer about error
                IoC.Logger.Log($"Exception is : {ex.Message}");

                // update the user about the error.
                IoC.Communication.Log = $"{DateTime.Now.ToLocalTime():MM/dd/yy HH:mm:ss.fff}: Exception: {ex.Message}.";

                // Trying to stop the app gracefully.
                await IoC.Task.Run(() => IoC.ReleaseOmicron.ProcessErrors());
            }
            catch (Exception ex)
            {

                // inform developer
                IoC.Logger.Log($"Exception: {ex.Message}");

                // update the user about failed test.
                IoC.Communication.Log = $"{DateTime.Now.ToLocalTime():MM/dd/yy HH:mm:ss.fff}: Test failed: {ex.Message}.";

                // catch inner exceptions if exists
                if (ex.InnerException != null)
                {
                    // inform the user about more details about error.
                    IoC.Communication.Log = $"{DateTime.Now.ToLocalTime():MM/dd/yy HH:mm:ss.fff}: Inner exception: {ex.InnerException}.";
                }

                // Trying to stop the app gracefully.
                await IoC.Task.Run(() => IoC.ReleaseOmicron.ProcessErrors());

                // exit from this task
                return;
            }
            finally
            {
                // Trying to stop the app gracefully.
                await IoC.Task.Run(() => IoC.ReleaseOmicron.ProcessErrors(false));
            }
        }

        #endregion

        #region Private Methods

        #endregion
    }
}