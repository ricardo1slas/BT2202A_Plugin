using OpenTap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BT2202a
{
    [Display("Charge", Group: "BT2202A", Description: "Charges a device with specified voltage and current for a set duration.")]
    public class Charge : TestStep
    {
        #region Settings
        // Properties for voltage, current, and time
        [Display("Voltage (V)", Order: 1, Description: "The voltage level to set during charging.")]
        public double Voltage { get; set; }

        [Display("Current (A)", Order: 2, Description: "The current level to set during charging.")]
        public double Current { get; set; }

        [Display("Time (s)", Order: 3, Description: "The duration of the charge in seconds.")]
        public double Time { get; set; }

        // Reference to the BT2202A Instrument
        [Display("Instrument", Order: 4, Description: "The BT2202A instrument to use for charging.")]
        public BT2202AInstrument MyInstrument { get; set; }
        #endregion
    
        public charge()
        {
            // Set default values for the properties.
            Voltage = 0; // Default voltage, adjust as needed.
            Current = 0; // Default current, adjust as needed.
            Time = 0;   // Default duration, adjust as needed.
        }

        public override void PrePlanRun()
        {
            base.PrePlanRun();
            // ToDo: Optionally add any setup code this step needs to run before the testplan starts

            // Custom setup logic for the Charge test step.
            if (MyInstrument == null)
            {
                Log.Error("No BT2202A instrument is configured.");
                UpgradeVerdict(Verdict.Error);
            }


        }

        public override void Run()
        {
            // ToDo: Add test case code here
            RunChildSteps(); //If step has child steps.
            UpgradeVerdict(Verdict.Pass);
        }

        public override void PostPlanRun()
        {
            // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
            base.PostPlanRun();
        }
    }
}
