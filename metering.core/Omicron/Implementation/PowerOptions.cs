﻿using System;
using metering.core.Resources;

namespace metering.core
{
    /// <summary>
    /// Handles Omicron Power up, down and etc. options
    /// </summary>
    public class PowerOptions
    {

        /// <summary>
        /// Turns off outputs of Omicron Test Set and release it.
        /// </summary>
        public async void TurnOffCMC()
        {
            try
            {
                // lock the task
                await AsyncAwaiter.AwaitAsync(nameof(TurnOffCMC), async () =>
                {

                    // update the developer
                    IoC.Logger.Log($"turnOffCMC setup: runs", LogLevel.Informative);

                    // send Turn off command to Omicron Test Set
                    await IoC.Task.Run(() => IoC.StringCommands.SendStringCommand(OmicronStringCmd.out_analog_outputOff));

                    // release Omicron Test Set.
                    await IoC.Task.Run(() => IoC.ReleaseOmicron.Release());
                });
            }

            catch (Exception ex)
            {
                // inform the developer about error.
                IoC.Logger.Log($"InnerException: {ex.Message}");

                // inform the user about error.
                IoC.Communication.Log += $"Time: {DateTime.Now.ToLocalTime():MM/dd/yy hh:mm:ss.fff}\tturnOffCMC setup: error detected\n";

                // catch inner exceptions if exists
                if (ex.InnerException != null)
                {
                    // inform the user about more details about error.
                    IoC.Communication.Log += $"{DateTime.Now.ToLocalTime():MM/dd/yy HH:mm:ss.fff}: Inner exception: {ex.InnerException}.\n";
                }
            }
        }


        /// <summary>
        /// Turns on outputs of Omicron Test Set.
        /// </summary>
        public async void TurnOnCMC()
        {
            try
            {
                // lock the task
                await AsyncAwaiter.AwaitAsync(nameof(TurnOnCMC), async () =>
                {
                    // update the developer
                    IoC.Logger.Log($"turnOnCMC setup runs", LogLevel.Informative);

                    // Send command to Turn On Analog Outputs
                    await IoC.Task.Run(() => IoC.StringCommands.SendStringCommand(OmicronStringCmd.out_analog_outputOn));

                });
            }
            catch (Exception ex)
            {
                // inform the developer about error.
                IoC.Logger.Log($"InnerException: {ex.Message}");

                // inform the user about error.
                IoC.Communication.Log += $"Time: {DateTime.Now.ToLocalTime():MM/dd/yy hh:mm:ss.fff}\tturnOnCMC setup: error detected\n";

                // catch inner exceptions if exists
                if (ex.InnerException != null)
                {
                    // inform the user about more details about error.
                    IoC.Communication.Log += $"{DateTime.Now.ToLocalTime():MM/dd/yy HH:mm:ss.fff}: Inner exception: {ex.InnerException}.\n";
                }
            }
        }

    }
}