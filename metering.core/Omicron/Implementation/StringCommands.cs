﻿using System;
using System.Text;
using System.Threading.Tasks;
using metering.core.Resources;

namespace metering.core
{
    /// <summary>
    /// Generates necessary strings to control Omicron Test Set.
    /// </summary>
    public class StringCommands
    {        
        #region Private Members

        #endregion

        /// <summary>
        /// Omicron Test Set generator short names.
        /// </summary>
        public enum GeneratorList : short { v, i };

        /// <summary>
        /// Omicron Test Set generator short signal names.
        /// </summary>
        public enum SignalType : short { a, f, p };

        /// <summary>
        /// Sends "ON" command to Omicron Test Set.
        /// </summary>
        /// <remarks>
        /// Command format:
        /// "out:[ana:]v|i(generator_list):[sig(no):]signalType(fValue) with omitted optional "step" parameter.
        /// </remarks>
        /// <param name="generator">Triple list type: "v" for Voltage, "i" for current amplifier.</param>
        /// <param name="generatorNumber">This parameter is 1 or 2 and selects either signal component 1 or component 2. Ex: "1:1".</param>
        /// <param name="amplitude">Magnitude of analog signal.</param>
        /// <param name="phase">Phase of analog signal.</param>
        /// <param name="frequency">Frequency of analog signal.</param>
        public async void SendOutAnaAsync(int generator, string generatorNumber, double amplitude, double phase, double frequency)
        {
            // obtain appropriate generator short name 
            string generatorType = Enum.GetName(typeof(GeneratorList), generator);

            try
            {
                // lock the task
                await AsyncAwaiter.AwaitAsync(nameof(SendOutAnaAsync), async () =>
                {
                    // check if the user canceling test
                    if (!IoC.Commands.Token.IsCancellationRequested)
                    {
                        // build a string to send to Omicron Test set
                        StringBuilder stringBuilder = new StringBuilder(string.Format(OmicronStringCmd.out_ana_setOutput, generatorType, generatorNumber, amplitude, phase, frequency, "sin"));

                        // send newly generated string command to Omicron Test Set
                        await IoC.Task.Run(() => IoC.CMCControl.CMEngine.Exec(IoC.CMCControl.DeviceID, stringBuilder.ToString()));

                        // inform developer about string command send to omicron test set
                        IoC.Logger.Log($"device ID: {IoC.CMCControl.DeviceID}\tcommand: {stringBuilder}", LogLevel.Informative);
                    }
                });
            }
            catch (Exception err)
            {
                // inform the developer about error.
                IoC.Logger.Log($"Exception: {err.Message}");
            }
        }

        /// <summary>
        /// Sends string command to Omicron Test Set.
        /// </summary>
        /// <param name="omicronCommand">This is the command to send Omicron Test Set.</param>
        public async void SendStringCommandAsync(string omicronCommand)
        {
            try
            {
                // lock the task
                await AsyncAwaiter.AwaitAsync(nameof(SendOutAnaAsync), async () =>
                {

                    // check if the user canceling test
                    if (!IoC.Commands.Token.IsCancellationRequested)
                    {
                        // pass received string command to Omicron Test set
                        await IoC.Task.Run(() => IoC.CMCControl.CMEngine.Exec(IoC.CMCControl.DeviceID, omicronCommand));
                    }
                });
            }
            catch (Exception err)
            {
                // inform the developer about error.
                IoC.Logger.Log($"Exception: {err.Message}");
            }
        }

        public async Task<string> SendStringCommandWithResponseAsync(string omicronCommand)
        {
            try
            {
                // lock the task
                return await AsyncAwaiter.AwaitResultAsync(nameof(SendStringCommandWithResponseAsync), async () =>
                {
                    string result = string.Empty;

                    // check if the user canceling test
                    if (!IoC.Commands.Token.IsCancellationRequested)
                     {
                        // pass received string command to Omicron Test set
                       result = await IoC.Task.Run(() => IoC.CMCControl.CMEngine.Exec(IoC.CMCControl.DeviceID, omicronCommand));
                     }

                    return result;
                 });
            }
            catch (Exception err)
            {
                // inform the developer about error.
                IoC.Logger.Log($"Exception: {err.Message}");

                return string.Empty;
            }
        }
    }
}