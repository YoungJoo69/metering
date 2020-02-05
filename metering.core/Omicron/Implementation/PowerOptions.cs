﻿using System;
using System.Threading.Tasks;
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
        public async Task TurnOffCMCAsync()
        {
            try
            {
                // lock the task
                await AsyncAwaiter.AwaitAsync(nameof(TurnOffCMCAsync), async () =>
                {

                    // update the developer
                    IoC.Logger.Log($"{nameof(TurnOffCMCAsync)} started.", LogLevel.Informative);

                    // send Turn off command to Omicron Test Set
                    await IoC.Task.Run(async () => await IoC.StringCommands.SendStringCommandsAsync(omicronCommand: OmicronStringCmd.out_ana_off));

                    // update the developer
                    IoC.Logger.Log($"{nameof(TurnOffCMCAsync)} completed.", LogLevel.Informative);

                });
            }

            catch (Exception ex)
            {
                // inform the developer about error.
                IoC.Logger.Log($"InnerException: {ex.Message}");

                // inform the user about error.
                IoC.Communication.Log = $"Time: {DateTime.Now.ToLocalTime():MM/dd/yy hh:mm:ss.fff}\tturnOffCMC setup: error detected.";

                // catch inner exceptions if exists
                if (ex.InnerException != null)
                {
                    // inform the user about more details about error.
                    IoC.Communication.Log = $"{DateTime.Now.ToLocalTime():MM/dd/yy HH:mm:ss.fff}: Inner exception: {ex.InnerException}.";
                }
            }
        }

        /// <summary>
        /// Turns on outputs of Omicron Test Set.
        /// </summary>
        public async Task TurnOnCMCAsync()
        {
            try
            {
                // lock the task
                await AsyncAwaiter.AwaitAsync(nameof(TurnOnCMCAsync), async () =>
                {
                    // update the developer
                    IoC.Logger.Log($"{nameof(TurnOnCMCAsync)} started.", LogLevel.Informative);

                    // Send command to Turn On Analog Outputs
                    await IoC.Task.Run(() => IoC.StringCommands.SendStringCommandsAsync(OmicronStringCmd.out_ana_on));

                    // update the developer
                    IoC.Logger.Log($"{nameof(TurnOnCMCAsync)} completed.", LogLevel.Informative);

                });
            }
            catch (Exception ex)
            {
                // inform the developer about error.
                IoC.Logger.Log($"InnerException: {ex.Message}");

                // inform the user about error.
                IoC.Communication.Log = $"Time: {DateTime.Now.ToLocalTime():MM/dd/yy hh:mm:ss.fff}\tturnOnCMC setup: error detected.";

                // catch inner exceptions if exists
                if (ex.InnerException != null)
                {
                    // inform the user about more details about error.
                    IoC.Communication.Log = $"{DateTime.Now.ToLocalTime():MM/dd/yy HH:mm:ss.fff}: Inner exception: {ex.InnerException}.";
                }
            }
        }

        /// <summary>
        /// Turns off outputs of Omicron Test Set and release it.
        /// </summary>
        [Obsolete("use async version", true)]
        public async void TurnOffCMC()
        {
            try
            {
                // lock the task
                await AsyncAwaiter.AwaitAsync(nameof(TurnOffCMC), async () =>
                {

                    // update the developer
                    IoC.Logger.Log($"{nameof(TurnOffCMC)} started.", LogLevel.Informative);

                    // send Turn off command to Omicron Test Set
                    await IoC.Task.Run(() => IoC.StringCommands.SendStringCommandsAsync(OmicronStringCmd.out_ana_off));
                    
                    // update the developer
                    IoC.Logger.Log($"{nameof(TurnOffCMC)} completed.", LogLevel.Informative);

                });
            }

            catch (Exception ex)
            {
                // inform the developer about error.
                IoC.Logger.Log($"InnerException: {ex.Message}");

                // inform the user about error.
                IoC.Communication.Log = $"Time: {DateTime.Now.ToLocalTime():MM/dd/yy hh:mm:ss.fff}\tturnOffCMC setup: error detected.";

                // catch inner exceptions if exists
                if (ex.InnerException != null)
                {
                    // inform the user about more details about error.
                    IoC.Communication.Log = $"{DateTime.Now.ToLocalTime():MM/dd/yy HH:mm:ss.fff}: Inner exception: {ex.InnerException}.";
                }
            }
        }


        /// <summary>
        /// Turns on outputs of Omicron Test Set.
        /// </summary>
        [Obsolete("use async version", true)]
        public async void TurnOnCMC()
        {
            try
            {
                // lock the task
                await AsyncAwaiter.AwaitAsync(nameof(TurnOnCMC), async () =>
                {
                    // update the developer
                    IoC.Logger.Log($"{nameof(TurnOnCMC)} started.", LogLevel.Informative);

                    // Send command to Turn On Analog Outputs
                    await IoC.Task.Run(() => IoC.StringCommands.SendStringCommandsAsync(OmicronStringCmd.out_ana_on));

                    // update the developer
                    IoC.Logger.Log($"{nameof(TurnOnCMC)} completed.", LogLevel.Informative);

                });
            }
            catch (Exception ex)
            {
                // inform the developer about error.
                IoC.Logger.Log($"InnerException: {ex.Message}");

                // inform the user about error.
                IoC.Communication.Log = $"Time: {DateTime.Now.ToLocalTime():MM/dd/yy hh:mm:ss.fff}\tturnOnCMC setup: error detected.";

                // catch inner exceptions if exists
                if (ex.InnerException != null)
                {
                    // inform the user about more details about error.
                    IoC.Communication.Log = $"{DateTime.Now.ToLocalTime():MM/dd/yy HH:mm:ss.fff}: Inner exception: {ex.InnerException}.";
                }
            }
        }
    }
}
