﻿using System.Collections.ObjectModel;

namespace metering.core
{
    /// <summary>
    /// Design time data for a <see cref="SettingsListViewModel"/> Current configurations.
    /// </summary>
    public class SettingsDesignModelCurrent : SettingsViewModel
    {
        #region Singleton       

        /// <summary>
        /// Single instance of the design time model
        /// </summary>
        public static SettingsDesignModelCurrent Instance => new SettingsDesignModelCurrent();

        #endregion

        #region Constructor

        /// <summary>
        /// default constructor
        /// </summary>
        public SettingsDesignModelCurrent()
        {
            OmicronCurrentOutputs = new ObservableCollection<SettingsListItemViewModel>
            {
                new SettingsListItemViewModel
                {
                    // 0,16,3,1.250000e+01,7.000000e+01,7.500000e+00,1.000000e+01,zero,40,amp_no,2,amp_no,6;
                    // Omicron UI shows: 6x12.5A, 70VA @ 7.5A, 10Vrms
                    ConfigIDs = new ObservableCollection<int>{ 16 }, // 16
                    PhaseCounts = new ObservableCollection<int>{ 6 }, // 6,
                    WiringDiagramString = "6x12.5A, 70VA @ 7.5A, 10Vrms",
                    Mode = "zero40", // zero,40,
                    WiringDiagramFileLocation = "zero40",
                    CurrentWiringDiagram = false,
                },

                new SettingsListItemViewModel
                {
                    // 0,14,3,1.250000e+01,7.000000e+01,7.500000e+00,1.000000e+01,std,18,amp_no,2;
                    // Omicron UI shows: 3x12.5A, 70VA @ 7.5A, 10Vrms
                    ConfigIDs = new ObservableCollection<int>{ 14 }, // 14,
                    PhaseCounts = new ObservableCollection<int>{ 3 }, // 3,
                    WiringDiagramString = "3x12.5A, 70VA @ 7.5A, 10Vrms",
                    Mode = "std18", // std,18,
                    WiringDiagramFileLocation = "std18", // std,18,
                    CurrentWiringDiagram = false,
                },

                new SettingsListItemViewModel
                {
                    // 0,15,3,1.250000e+01,7.000000e+01,7.500000e+00,1.000000e+01,std,19,amp_no,6;
                    // Omicron UI shows: 3x12.5A, 70VA @ 7.5A, 10Vrms
                    ConfigIDs = new ObservableCollection<int>{ 15 }, // 15,
                    PhaseCounts = new ObservableCollection<int>{ 3 }, // 3,
                    WiringDiagramString = "3x12.5A, 70VA @ 7.5A, 10Vrms",
                    Mode = "std19", // std,19,
                    WiringDiagramFileLocation = "std19", // std,19,
                    CurrentWiringDiagram = true,
                },

                new SettingsListItemViewModel
                {
                    // Not in the responses: 0,15,3,1.250000e+01,7.000000e+01,7.500000e+00,1.000000e+01,std,19,amp_no,6;
                    // Omicron UI shows: 3x12.5A, 70VA @ 7.5A, 10Vrms, IE automatically calculated
                    ConfigIDs = new ObservableCollection<int>{ 15 }, // 15,
                    PhaseCounts = new ObservableCollection<int>{ 3 }, //  3,
                    WiringDiagramString = "3x12.5A, 70VA @ 7.5A, 10Vrms, IE automatically calculated",
                    Mode = "std19", // std,19,
                    WiringDiagramFileLocation = "std19", // std,19,
                    CurrentWiringDiagram = false,
                },

                new SettingsListItemViewModel
                {
                    // 0,17,3,2.500000e+01,1.400000e+02,1.500000e+01,1.000000e+01,par3,5,amp_no,2,amp_no,6;
                    // Omicron UI shows: 3x25A, 140VA @ 15A, 10Vrms
                    ConfigIDs = new ObservableCollection<int>{ 17 }, // 17,
                    PhaseCounts = new ObservableCollection<int>{ 3 }, //  3,
                    WiringDiagramString = "3x25A, 140VA @ 15A, 10Vrms",
                    Mode = "par35", // par3,5, 
                    WiringDiagramFileLocation = "par35", // par3,5, 
                    CurrentWiringDiagram = false,
                },

                new SettingsListItemViewModel
                {
                    // 0,18,1,3.750000e+01,2.100000e+02,2.250000e+01,1.000000e+01,par1,20,amp_no,2;
                    // Omicron UI shows: 1x37.5A, 210VA @ 22.5A, 10Vrms
                    ConfigIDs = new ObservableCollection<int>{ 18 }, // 18,
                    PhaseCounts = new ObservableCollection<int>{ 1 }, // 1,
                    WiringDiagramString = "1x37.5A, 210VA @ 22.5A, 10Vrms",
                    Mode = "par120", // par1,20,         
                    WiringDiagramFileLocation = "par120", // par1,20,  
                    CurrentWiringDiagram = false,
                },

                new SettingsListItemViewModel
                {
                    // 0,19,1,3.750000e+01,2.100000e+02,2.250000e+01,1.000000e+01,par1,21,amp_no,6;
                    // Omicron UI shows: 1x37.5A, 210VA @ 22.5A, 10Vrms
                    ConfigIDs = new ObservableCollection<int>{ 19 }, // 19,
                    PhaseCounts = new ObservableCollection<int>{ 1 }, // 1,
                    WiringDiagramString = "1x37.5A, 210VA @ 22.5A, 10Vrms",
                    Mode = "par121", // par1,21,
                    WiringDiagramFileLocation = "par121", // par1,21,
                    CurrentWiringDiagram = false,
                },

            };
        }

        #endregion
    }
}
