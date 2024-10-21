using OpenTap; // Use OpenTAP infrastructure/core components (log, TestStep definition, etc.)
using System;

// Generates test steps for charges with the following parameters
namespace OpenTap.Plugins.Carga
{
    [Display("Carga", Group: "OpenTap.Plugins.Carga", Description: "Carga de celda")]
    public class Charge : TestStep
    {
        private static int GlobalCounter = 0;

        #region Settings
        [Display("Instrument", Group: "Resources")]
        public BT2202A BT2202A { get; set; }

        [Display("Overvoltage", Group: "Connections", Order: 1.1)]
        public double Overvoltage { get; set; } = 0;

        [Display("Charge Current", Group: "Connections", Order: 1.2)]
        public double CCurrent { get; set; } = 0;

        [Display("Time (seconds)", Group: "Connections", Order: 1.4)]
        public double Seconds { get; set; } = 0;
        #endregion

        public Carga()
        {
            // Set default values for properties / settings if necessary.
        }

        public override void Run()
        {
            if (BT2202A == null)
            {
                Log.Error("Instrument not configured. Please assign an instance of BT2202A.");
                UpgradeVerdict(Verdict.Error);
                return;
            }

            // Execute the charge method.
            BT2202A.Carga(Overvoltage, CCurrent, Seconds);
            Log.Info($"GlobalCounter before increment: {GlobalCounter}");

            GlobalCounter += 3;
            Log.Info($"GlobalCounter after increment: {GlobalCounter}");

            // Execute any child steps if present.
            RunChildSteps();

            // Set the verdict to Pass if all steps run successfully.
            UpgradeVerdict(Verdict.Pass);
        }
    }
}