﻿using System;
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
        //public int MinTestValue { get; set; }
        public int[] MinValues { get; set; }

        /// <summary>
        /// Holds minimum value for test register.
        /// </summary>
        public int MaxTestValue { get; set; }

        public int[] MaxValues { get; set; }

        /// <summary>
        /// Hold progress information per test register.
        /// </summary>
        public double Progress { get; set; }

        /// <summary>
        /// Associated Omicron Test Set ID. Assigned by CM Engine.
        /// </summary>
        public int DeviceID { get; set; }

        /// <summary>
        /// Omicron Test Set debugging log levels.
        /// </summary>
        public enum OmicronLoggingLevels : short { None, Level1, Level2, Level3, };

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
                int progressStep = default(int);

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

                // report file id to distinguish between test results 
                string fileId = $"{DateTime.Now.ToLocalTime():yyyy_MM_dd_HH_mm}";

                // decides which signal is our ramping signal by comparing the mismatch of any "From" and "To" values.
                // after the user clicked "Go" button
                TestSignal testSignal = new TestSignal();

                // loads TestSignal information as a Tuple
                var (SignalName, From, To, Delta, Phase, Frequency) = testSignal.GetRampingSignal();

                // initialize new testStartValue
                double testStartValue = default(double);

                // verify we have a ramping signal
                if (string.IsNullOrWhiteSpace(SignalName))
                {
                    // inform the user there is no test case
                    IoC.Communication.Log = $"{DateTime.Now.ToLocalTime():MM/dd/yy HH:mm:ss.fff}: There is no ramping signal.Please check your entries.";

                    // return from this task.
                    return;
                }

                // create a TestResultLogger to generate a test report in .csv format.
                var testResultLog = new TestResultLogger
                    (
                        // set file path and name
                        filePath: Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "metering"), $"{IoC.TestDetails.Register}_{From:F3}-{To:F3}_{fileId}.csv"),
                        // no need to save time
                        logTime: false
                    );

                // generate fixed portion of header information for reporting.
                StringBuilder @string = new StringBuilder("Time, Test Value");

                // generate variable portion of header information based on Register entry.
                for (int i = 0; i < IoC.TestDetails.Register.ToString().Split(',').Length; i++)
                {
                    // append new variable header information to previous string generated.
                    @string.Append($" ,Register {i + 1}, Minimum Value {i + 1}, Maximum Value {i + 1}");
                }

                // add new line to generated string for the test values.
                @string.AppendLine();

                // inform the developer of test SignalName
                IoC.Logger.Log($"Test signal name: {SignalName}", LogLevel.Informative);

                // set maximum value for the progress bar
                IoC.Commands.MaximumTestCount = Math.Ceiling((Math.Abs(To - From) / Delta)) + 1;

                // inform the developer MaximumTestCount
                IoC.Logger.Log($"Maximum test count: {IoC.Commands.MaximumTestCount}", LogLevel.Informative);

                // Process test steps
                // due to nature of double calculations use this formula "testStartValue <= (To + Delta * 1 / 1000)" to make sure the last test step always runs
                for (testStartValue = From; testStartValue <= (To + Delta * 1 / 1000); testStartValue += Delta)
                {
                    // check if the user canceled the tests.
                    if (!IoC.Commands.Token.IsCancellationRequested)
                    {

                        // inform the developer about test register and start value.
                        IoC.Logger.Log($"Register: {IoC.TestDetails.Register}\tTest value: {testStartValue:F3} started", LogLevel.Informative);

                        // inform the user about test register and start value.
                        IoC.Communication.Log = $"{DateTime.Now.ToLocalTime():MM/dd/yy HH:mm:ss.fff}: Register: {IoC.TestDetails.Register} --- Test value: {testStartValue:F3} started";

                        // send Omicron commands
                        await IoC.Task.Run(async () =>
                        {
                            // send string commands to Omicron
                            await IoC.Task.Run(() => IoC.SetOmicron.SendOmicronCommands(SignalName, testStartValue));

                            // Turn On Omicron Analog Outputs per the user input
                            await IoC.Task.Run(() => IoC.PowerOptions.TurnOnCMC());

                        });

                        // initialize Minimum Value List with the highest int32 to capture every minimum value
                        MinValues = Enumerable.Repeat(Int32.MaxValue, IoC.TestDetails.Register.ToString().Split(',').Length).ToArray();

                        // initialize Maximum Value List with the lowest int32 to capture every maximum value
                        MaxValues = Enumerable.Repeat(Int32.MinValue, IoC.TestDetails.Register.ToString().Split(',').Length).ToArray();

                        // set timer to read modbus register per the user specified time.
                        MdbusTimer = new Timer(
                                                callback: IoC.Modbus.MeasurementIntervalWithCommaCallbackAsync,
                                                state: IoC.TestDetails.Register,
                                                dueTime: TimeSpan.FromSeconds(Convert.ToDouble(IoC.TestDetails.StartMeasurementDelay)),
                                                period: TimeSpan.FromMilliseconds(Convert.ToDouble(IoC.TestDetails.MeasurementInterval)
                                                ));

                        // Start reading the user specified Register
                        IoC.Task.Run(async () =>
                        {

                            // lock the task
                            await AsyncAwaiter.AwaitAsync(nameof(TestAsync), async () =>
                            {

                                // wait until the user specified "Dwell Time" expires.
                                await Task.Delay(TimeSpan.FromSeconds(Convert.ToDouble(IoC.TestDetails.DwellTime)));
                            });

                            // terminate reading modbus register because "Dwell Time" is over.
                            MdbusTimer.Dispose();

                            // Time Test Value Register 1  Min Value 1 Max Value 1 Register 2  Min Value 2 Max Value 2 Register 3  Min Value 3 Max Value 3
                            @string.Append($"{DateTime.Now.ToLocalTime():MM/dd/yy HH:mm:ss.fff},{testStartValue:F3}");

                            // generate variable portion of header information based on Register entry.
                            for (int i = 0; i < IoC.TestDetails.Register.ToString().Split(',').Length; i++)
                            {
                                // append new variable header information to previous string generated.
                                @string.Append($" ,{IoC.TestDetails.Register.ToString().Split(',').GetValue(i)} ,{MinValues[i]} , {MaxValues[i]}");
                            }


                            // wait task to be over with
                        }, IoC.Commands.Token).Wait();

                        // update test result report
                        testResultLog.Log(@string.ToString(), LogLevel.Informative);

                        // increment progress bar strip on the "Button"
                        IoC.Commands.TestProgress = Convert.ToDouble(progressStep);

                        // increment progress
                        progressStep++;

                        // inform the developer about test register and start value.
                        IoC.Logger.Log($"Register: {IoC.TestDetails.Register} --- Test value: {testStartValue:F3} completed", LogLevel.Informative);

                        // inform the user about test register and start value.
                        IoC.Communication.Log = $"{DateTime.Now.ToLocalTime():MM/dd/yy HH:mm:ss.fff}: Register: {IoC.TestDetails.Register} --- Test value: {testStartValue:F3} completed.";

                        // inform the developer
                        IoC.Logger.Log($"Test {progressStep} of {IoC.Commands.MaximumTestCount} completed", LogLevel.Informative);

                        // increment progress percentage
                        Progress = progressStep / IoC.Commands.MaximumTestCount;                        

                        // reset message for the next test step
                        @string.Clear();

                    }
                    else
                    {
                        // if timer is initialized
                        if (!MdbusTimer.Equals(null))
                            // terminate reading modbus register because the user canceled the test.
                            MdbusTimer.Dispose();

                        // exit from this task
                        return;
                    }
                }
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