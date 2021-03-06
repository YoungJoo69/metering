﻿using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using System.Windows.Input;

namespace metering.core
{
    /// <summary>
    /// a view model for each analog signal in the SettingsPage
    /// </summary>
    public class SettingsListItemViewModel : BaseViewModel
    {
        #region Private Properties

        /// <summary>
        /// holder for the WiringDiagramFileLocation
        /// </summary>
        private string wiringDiagramFileLocation = "not used";

        #endregion

        #region Public Properties

        /// <remarks>For more information 
        /// "CMEngine.pdf" page 206 of Test Universe 4.00 CMEngine.ENU.21 — Year: 2018</remarks>
        /// 
        /// <summary>
        /// ID of the configuration to be used in the amp:route, if you
        /// want to route a triple to this configuration.
        /// This is a number of type integer. 
        /// However to combined amplifiers this app will hold it as string.
        /// </summary>
        public ObservableCollection<int> ConfigIDs { get; set; } = new ObservableCollection<int>();

        /// <summary>
        /// Number of phases of the virtual amplifier. It is the number of phases that can be
        /// independently addressed and set with the out:ana commands.The number of
        /// phases that are actually output may be different (for instance, when Vo is
        /// automatically calculated from the three phase voltages in the CMC 256 or newer
        /// test set). This is a number of type integer.
        /// </summary>
        public ObservableCollection<int> PhaseCounts { get; set; } = new ObservableCollection<int>();

        /// <summary>
        /// Maximum output can be generated by the current hardware configuration.
        /// This value reported by the Omicron Test Set.
        /// </summary>
        public ObservableCollection<double> MaxOutput { get; set; } = new ObservableCollection<double>();

        /// <summary>
        /// a detailed text reference to Omicron Hardware Configuration available to the specific
        /// Omicron Test Set. ex: "3x300V, 85VA @ 85V, 1Arms"
        /// </summary>
        public string WiringDiagramString { get; set; } = string.Empty;

        /// <summary>
        /// Returns file name of the hardware configuration
        /// </summary>
        public string WiringDiagramFileName
        {
            get
            {
                // return wiring diagram file name.
                return wiringDiagramFileLocation; // .Substring(wiringDiagramFileLocation.LastIndexOf('/'), wiringDiagramFileLocation.LastIndexOf('/') - wiringDiagramFileLocation.LastIndexOf('.'));
            }
            set
            {
                // if new selection is different than previous
                if (!Equals(value, wiringDiagramFileLocation))
                {
                    // update the old value.
                    // check if the file loading
                    if (value.Contains("/"))
                    {
                        // value loaded from the saved file
                        wiringDiagramFileLocation = value.Substring(value.LastIndexOf('/') + 1, value.LastIndexOf('.') - value.LastIndexOf('/') - 1);
                    }
                    else
                    {
                        // value selected from the user interface
                        wiringDiagramFileLocation = value;
                    }
                }
            }
        }

        /// <summary>
        /// path to the wiring diagram associated with this hardware configuration
        /// </summary>
        public string WiringDiagramFileLocation
        {
            get
            {
                // return wiring diagram location and file name.
                return $"../Images/Omicron/{wiringDiagramFileLocation}.png";
            }
            set
            {
                // if new selection is different than previous
                if (!Equals(value, wiringDiagramFileLocation))
                {
                    // update the old value.
                    // check if the file loading
                    if (value.Contains("/"))
                    {
                        // value loaded from the saved file
                        wiringDiagramFileLocation = value.Substring(value.LastIndexOf('/') + 1, value.LastIndexOf('.') - value.LastIndexOf('/') - 1);
                    }
                    else
                    {
                        // value selected from the user interface
                        wiringDiagramFileLocation = value;
                    }
                }
            }
        }

        /// <summary>
        /// It encodes the way the different outputs of the physical
        /// amplifiers involved in the configuration are to be tied together to achieve the
        /// configuration’s characteristics. This value would decide which diagram to show to the user.
        /// Value of type string. 
        /// </summary>
        public string Mode { get; set; } = string.Empty;

        /// <summary>
        /// List of all physical amplifiers involved in the configuration. It is composed of one or
        /// more double entries.Each entry is composed of two parts: an amplifier descriptor
        /// and an amplifier number.
        /// The descriptor is a string, which may be either "amp_no" or "amp_id", if the
        /// amplifier is respectively internal or external.In both cases, the amplifier number
        /// follows, with type integer.If the amplifier is identified as amp_no, the number
        /// corresponds to that returned by the out:cfg? command, whereas for amplifiers
        /// identified as amp_id, the number corresponds to the id returned by amp:scan?
        /// This allows you to identify the amplifiers and find out whether they are Voltage or
        /// Current amplifiers.
        /// </summary>
        public ObservableCollection<int> AmplifierNumber { get; set; } = new ObservableCollection<int>();

        /// <summary>
        /// Holds radio button group names
        /// </summary>
        public string GroupName { get; set; } = string.Empty;

        /// <summary>
        /// Holds whether check box selected or not
        /// </summary>
        public bool CurrentWiringDiagram { get; set; } = false;

        /// <summary>
        /// Internal OMICRON identifier. It refers to a connection diagram depicting the
        /// connections encoded in <mode>. This field is of integer type.
        /// </summary>
        public int WiringID { get; set; } = -1;

        /// <summary>
        /// Holds raw Omicron Hardware configuration response for troubleshooting
        /// </summary>
        public string RawOmicronResponse { get; set; } = string.Empty;

        #endregion

        #region Public Commands

        /// <summary>
        /// Gets the user selected wiring diagram image file location
        /// </summary>
        public ICommand GetWiringDiagramCommand { get; set; }

        #endregion

        #region Constructor
        /// <summary>
        /// default constructor
        /// </summary>
        public SettingsListItemViewModel()
        {

            // make aware of culture of the computer
            // in case this software turns to something else.
            CultureInfo ci = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = ci;

            // retrieve wiring diagram image file location and show it to the user.
            GetWiringDiagramCommand = new RelayParameterizedCommand(async (parameter) => await IoC.Settings.GetWiringDiagram(parameter));
        }

        #endregion

        #region Public Method

        #endregion
    }
}
