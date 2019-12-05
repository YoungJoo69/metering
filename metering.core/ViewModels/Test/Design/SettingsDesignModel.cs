﻿using System.Collections.ObjectModel;

namespace metering.core
{
    /// <summary>
    /// Design time data for a <see cref="SettingsListViewModel"/>
    /// </summary>
    public class SettingsDesignModel : SettingsViewModel
    {
        #region Singleton       

        /// <summary>
        /// Single instance of the design time model
        /// </summary>
        public static SettingsDesignModel Instance => new SettingsDesignModel();

        #endregion

        #region Constructor

        /// <summary>
        /// default constructor
        /// </summary>
        public SettingsDesignModel()
        {
            OmicronOutputSignals = new ObservableCollection<SettingsListItemViewModel>
            {
                new SettingsListItemViewModel
                {
                    // Omicron returns: 0,9,3,3.000000e+02,8.500000e+01,8.500000e+01,1.000000e+00,std,0,amp_no,1;
                    // Omicron UI shows: 3x300V, 85VA @ 85V, 1Arms
                    ConfigID = 9, // 9,
                    UIString = "3x300V, 85VA @ 85V, 1Arms",
                    Mode = "std0", // std,0,
                },

                new SettingsListItemViewModel
                {
                    // 0,10,1,3.000000e+02,1.500000e+02,7.500000e+01,2.000000e+00,std,14,amp_no,5;
                    // Omicron UI shows: 1x300V, 150VA @ 75V, 2Arms
                    ConfigID = 10, // 10,
                    UIString = "1x300V, 150VA @ 75V, 2Arms",
                    Mode = "std0", // std,14,    
                },

                new SettingsListItemViewModel
                {
                    // 0,11,3,3.000000e+02,5.000000e+01,7.500000e+01,6.600000e-01,zero,13,amp_no,1,amp_no,5;
                    // Omicron UI shows: 3x300V, 50VA @ 75V, 660mArms
                    ConfigID = 11, // 11,
                    UIString = "3x300V, 50VA @ 75V, 660mArms",
                    Mode = "zero13", // zero, 
                },

                new SettingsListItemViewModel
                {
                    // 0,12,1,6.000000e+02,2.500000e+02,2.000000e+02,1.250000e+00,ser13,4,amp_no,1;
                    // Omicron UI shows: 1x600V, 250VA @ 200V, 1.25Arms
                    ConfigID = 12, // 12,                    
                    UIString = "1x600V, 250VA @ 200V, 1.25Arms",
                    Mode = "ser134", // ser13,4, 
                },

                new SettingsListItemViewModel
                {
                    // 0,13,2,6.000000e+02,1.250000e+02,1.500000e+02,1.000000e+00,ser2,17,amp_no,1,amp_no,5;
                    // Omicron UI shows: 2x600V, 125VA @ 150V, 1Arms
                    ConfigID = 13, // 13,
                    UIString = "2x600V, 125VA @ 150V, 1Arms",
                    Mode = "ser217", // ser2,17,  
                },

                new SettingsListItemViewModel
                {
                    // 0,14,3,1.250000e+01,7.000000e+01,7.500000e+00,1.000000e+01,std,18,amp_no,2;
                    // Omicron UI shows: 3x12.5A, 70VA @ 7.5A, 10Vrms
                    ConfigID = 14, // 14,
                    UIString = "3x12.5A, 70VA @ 7.5A, 10Vrms",
                    Mode = "std18", // std,18,
                },

                new SettingsListItemViewModel
                {
                    // 0,15,3,1.250000e+01,7.000000e+01,7.500000e+00,1.000000e+01,std,19,amp_no,6;
                    // Omicron UI shows: 3x12.5A, 70VA @ 7.5A, 10Vrms
                    ConfigID = 15, // 15,
                    UIString = "3x12.5A, 70VA @ 7.5A, 10Vrms",
                    Mode = "std19", // std,19,
                },

                new SettingsListItemViewModel
                {
                    // 0,16,3,1.250000e+01,7.000000e+01,7.500000e+00,1.000000e+01,zero,40,amp_no,2,amp_no,6;
                    // Omicron UI shows: 6x12.5A, 70VA @ 7.5A, 10Vrms
                    ConfigID = 16, // 16,
                    UIString = "6x12.5A, 70VA @ 7.5A, 10Vrms",
                    Mode = "zero40", // zero,40,
                },

                new SettingsListItemViewModel
                {
                    // 0,17,3,2.500000e+01,1.400000e+02,1.500000e+01,1.000000e+01,par3,5,amp_no,2,amp_no,6;
                    // Omicron UI shows: 3x25A, 140VA @ 15A, 10Vrms
                    ConfigID = 17, // 17,
                    UIString = "3x25A, 140VA @ 15A, 10Vrms",
                    Mode = "par35", // par3,5, 
                },

                new SettingsListItemViewModel
                {
                    // 0,18,1,3.750000e+01,2.100000e+02,2.250000e+01,1.000000e+01,par1,20,amp_no,2;
                    // Omicron UI shows: 1x37.5A, 210VA @ 22.5A, 10Vrms
                    ConfigID = 18, // 18,
                    UIString = "1x37.5A, 210VA @ 22.5A, 10Vrms",
                    Mode = "par120", // par1,20,         
                },

                new SettingsListItemViewModel
                {
                    // 0,19,1,3.750000e+01,2.100000e+02,2.250000e+01,1.000000e+01,par1,21,amp_no,6;
                    // Omicron UI shows: 1x37.5A, 210VA @ 22.5A, 10Vrms
                    ConfigID = 19, // 19,
                    UIString = "1x37.5A, 210VA @ 22.5A, 10Vrms",
                    Mode = "par121", // par1,21,
                },

            };
        }

        #endregion
    }
}
