﻿
using System;

namespace metering.core
{
    /// <summary>
    /// Disconnects and releases associated Omicron Test Set.
    /// </summary>
    public class ReleaseOmicron
    {
        #region Public Method

        /// <summary>
        /// Disconnects and releases associated Omicron Test Set.
        /// </summary>
        public async void Release()
        {
            try
            {
                // lock the task
                await AsyncAwaiter.AwaitAsync(nameof(Release), async () =>
                {
                    // inform the developer
                    IoC.Logger.Log($"Release: started", LogLevel.Informative);

                    // unlock attached Omicron Test Set                    
                    await IoC.Task.Run(() => IoC.CMCControl.CMEngine.DevUnlock(IoC.CMCControl.DeviceID));

                    // Destruct Omicron Test set
                    IoC.CMCControl.CMEngine = null;
                });
            }
            catch (Exception ex)
            {
                // inform the developer about error
                IoC.Logger.Log($"InnerException is : {ex.Message}");

                // inform the user about error
                IoC.Communication.Log += $"Time: {DateTime.Now.ToLocalTime():MM/dd/yy hh:mm:ss.fff}\trelease Omicron: error detected\n";

                // catch inner exceptions if exists
                if (ex.InnerException != null)
                {
                    // inform the user about more details about error.
                    IoC.Communication.Log += $"{DateTime.Now.ToLocalTime():MM/dd/yy HH:mm:ss.fff}: Inner exception: {ex.InnerException}.\n";
                }
            }
        }

        /// <summary>
        /// Handles errors and stops the app gracefully.
        /// </summary>
        /// <param name="userRequest">true if test interrupt requested by the user
        /// true if test completed itself</param>
        public void ProcessErrors(bool userRequest = true)
        {
            if (userRequest)
            {
                // update developer "Test interrupted"
                IoC.Logger.Log($"Test interrupted", LogLevel.Informative);

                // update the user "Test interrupted"
                IoC.Communication.Log += $"{DateTime.Now.ToLocalTime():MM/dd/yy HH:mm:ss.fff}: Test interrupted by the user.\n";

                // try to cancel thread running Omicron Test Set
                IoC.Commands.TokenSource.Cancel();

                // Trying to stop the communication timer.
                IoC.CMCControl.MdbusTimer.Dispose();

            }
            else
            {
                // update developer "Test completed"
                IoC.Logger.Log($"Test completed", LogLevel.Informative);

                // update the user "Test interrupted"
                IoC.Communication.Log += $"{DateTime.Now.ToLocalTime():MM/dd/yy HH:mm:ss.fff}: Test completed.\n";
            }

            // test completed
            IoC.CMCControl.IsTestRunning ^= true;

            // Turn off outputs of Omicron Test Set and release it.
            IoC.PowerOptions.TurnOffCMC();

            // Disconnect Modbus Communication
            IoC.Communication.EAModbusClient.Disconnect();

            // Progress bar is invisible
            IoC.Commands.IsConnectionCompleted = IoC.Commands.IsConnecting = IoC.Communication.EAModbusClient.Connected;

            // change color of Cancel Command button to Red
            IoC.Commands.CancelForegroundColor = "ff0000";
        }
    }


    #endregion
}
