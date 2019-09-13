﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using OMICRON.CMEngAL;

namespace metering.core
{
    /// <summary>
    /// Handles Communication page.
    /// </summary>
    public class CommunicationViewModel : BaseViewModel
    {
        // TODO: squirrel.windows update tool
        // TODO: use dependency injection for EasyModbus library
        #region Public Properties

        /// <summary>
        /// ModbusClient for modbus protocol communication.
        /// </summary>
        public EasyModbus.ModbusClient EAModbusClient { get; set; }

        /// <summary>
        /// Hint value for IpAddress textbox
        /// </summary>
        public string IpAddressHint { get; set; } = Resources.Strings.tab_home_ipaddress;

        /// <summary>
        /// IpAddress of the test unit.
        /// </summary>
        public string IpAddress { get; set; } = "192.168.0.122";
        
        /// <summary>
        /// Hint value for Port textbox
        /// </summary>
        public string PortHint { get; set; } = Resources.Strings.tab_home_port;

        /// <summary>
        /// port number of communication port
        /// </summary>
        public string Port { get; set; } = "502";

        /// <summary>
        /// Holds info about IpAddress:port, connected Omicron Serial # and etc.
        /// </summary>
        public string Log { get; set; } = "";

        /// <summary>
        /// a flag indicating Omicron Test Set connected and running
        /// </summary>
        public bool IsOmicronConnected { get; set; } = false;

        /// <summary>
        /// a flag indication UUT is connected and responding
        /// </summary>
        public bool IsUnitUnderTestConnected { get; set; } = false;

        /// <summary>
        /// indicates if the current text double left clicked to highlight the text
        /// </summary>
        public bool Selected { get; set; }

        #endregion

        #region Public Commands

        /// <summary>
        /// Selects all text on left clicked text box.
        /// </summary>
        public ICommand SelectAllTextCommand { get; set; }

        #endregion

        #region Constructor
        /// <summary>
        /// default constructor
        /// </summary>
        public CommunicationViewModel()
        {

            // make aware of culture of the computer
            // in case this software turns to something else.
            CultureInfo ci = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = ci;

            // inform the user Application started.
            Log += $"{DateTime.Now.ToLocalTime():MM/dd/yy HH:mm:ss.fff}: Application Starts\n";

            // create command
            SelectAllTextCommand = new RelayCommand(SelectAll);
        }

        #endregion

        #region Public Method

        /// <summary>
        /// Selects all text on the text box
        /// </summary>
        public void SelectAll()
        {
            // simulate property change briefly to select all text in the text box
            // as selecting all text should be last until the user leaves the control or types something

            // Sets FocusAndSelectProperty to true
            Selected = true;

            // Sets FocusAndSelectProperty to false
            Selected = false;
        }

        /// <summary>
        /// Starts a test with the values specified in Nominal Values page and
        /// Communication page.
        /// </summary>
        public async Task StartCommunicationAsync()
        {
            // start point of all test steps with the first mouse click and it will ignore subsequent mouse clicks
            await RunCommand(() => IsUnitUnderTestConnected, async () =>
            {
                try
                {
                    // update the user
                    Log += $"{DateTime.Now.ToLocalTime():MM/dd/yy HH:mm:ss.fff}: Communication starts\n";

                    // get new construct of CMCControl
                    IoC.CMCControl.CMEngine = new CMEngine();

                    // get new construct of ModbusClient
                    EAModbusClient = new EasyModbus.ModbusClient
                    {
                        IPAddress = IpAddress,
                        Port = Convert.ToInt32(Port),
                        ConnectionTimeout = 20000,
                        // LogFileFilename = @"C:\Users\TBircek\Documents\metering\modbus.log"
                    };

                    // Checks if the Server IPAddress is available
                    if (EAModbusClient.Available(20000))
                    {
                        // connect to the server
                        EAModbusClient.Connect();

                        // find any CMCEngine attached to this computer
                        if (IoC.CMCControl.FindCMC())
                        {
                            // perform initial set up on CMCEngine
                            IoC.CMCControl.InitialSetup();

                            // Is there Omicron Test Set attached to this app?
                            if (IoC.CMCControl.DeviceID > 0)
                            {
                                // Run the test schedule per the user input
                                await IoC.CMCControl.TestSample
                                (
                                    // communication register to retrieve information from
                                    Register: Convert.ToInt32(IoC.TestDetails.Register),

                                    // start of the test steps value
                                    From: Convert.ToDouble(IoC.TestDetails.AnalogSignals[0].From),

                                    // end of the test steps value
                                    To: Convert.ToDouble(IoC.TestDetails.AnalogSignals[0].To),

                                    // increment of the steps
                                    Delta: Convert.ToDouble(IoC.TestDetails.AnalogSignals[0].Delta),

                                    // duration of the test steps
                                    DwellTime: Convert.ToDouble(IoC.TestDetails.DwellTime),

                                    // currently not used
                                    MeasurementDuration: 0d,

                                    // time to start tests
                                    StartDelayTime: Convert.ToDouble(IoC.TestDetails.StartDelayTime),

                                    // interval to read the register through communication protocol
                                    MeasurementInterval: Convert.ToDouble(IoC.TestDetails.MeasurementInterval),

                                    // Delay reading of the register to prevent out of range values due to ramp up
                                    StartMeasurementDelay: Convert.ToDouble(IoC.TestDetails.StartMeasurementDelay),

                                    // excel header values for reporting.
                                    message: $"Time,Register,Test Value,Min Value,Max Value\n"
                                );

                            }
                            else
                            {
                                // inform the user 
                                Log += $"{DateTime.Now.ToLocalTime():MM/dd/yy HH:mm:ss.fff}: Failed: Omicron Test Set ID is a zero\n";
                            }
                        }
                        else
                        {
                            // inform the developer
                            Debug.WriteLine("Find no Omicron");

                            // inform the user 
                            Log += $"{DateTime.Now.ToLocalTime():MM/dd/yy HH:mm:ss.fff}: Failed: There is no attached Omicron Test Set. Please attached a Omicron Test Set before test\n";
                        }
                    }
                    else
                    {
                        // inform the developer
                        Debug.WriteLine($"The server {EAModbusClient.IPAddress} is not available.");

                        // inform the user 
                        Log += $"{DateTime.Now.ToLocalTime():MM/dd/yy HH:mm:ss.fff}: Failed: The server is not available: {EAModbusClient.IPAddress}\n";
                    }
                }
                catch (Exception ex)
                {
                    // inform the developer about error.
                    Debug.WriteLine(ex.Message);

                    // inform the user about error.
                    Log += $"{DateTime.Now.ToLocalTime():MM/dd/yy HH:mm:ss.fff}: Start Communication failed: {ex.Message}.\n";

                    // catch inner exceptions if exists
                    if (ex.InnerException != null)
                    {
                        // inform the user about more details about error.
                        Log += $"{DateTime.Now.ToLocalTime():MM/dd/yy HH:mm:ss.fff}: Inner exception: {ex.InnerException}.\n";
                    }
                }
            });
        }
        #endregion

    }
}