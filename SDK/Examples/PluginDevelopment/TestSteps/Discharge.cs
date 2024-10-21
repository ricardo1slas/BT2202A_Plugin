using OpenTap; // Use OpenTAP infrastructure/core components.
using System;

// Generates test steps for discharge following the next parameters:
namespace OpenTap.Plugins.Carga
{
    [Display("Discharge", Group: "OpenTap.Plugins.Carga", Description: "Proceso Discharge batería")]
    public class Discharge : TestStep
    {
        #region Settings
        public const double Prueba = 3;
        public const double Prueba2 = 0.1;

        [Display("Instrument", Group: "Resources")]
        public BT2202A BT2202A { get; set; }

        [Display("Cutoff Voltage", Group: "Connections", Order: 1.1)]
        public double CVoltage { get; set; } = Prueba;

        [Display("Charge Current", Group: "Connections", Order: 1.2)]
        public double CCurrent { get; set; } = Prueba2;

        [Display("Time (seconds)", Group: "Connections", Order: 1.4)]
        public double Seconds { get; set; } = 0;
        #endregion

        public Discharge()
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

            // Execute the discharge method.
            BT2202A.Discharge(CVoltage, CCurrent, Seconds);

            // Execute any child steps if present.
            RunChildSteps();

            // Set the verdict to Pass if all steps run successfully.
            UpgradeVerdict(Verdict.Pass);
        }
    }
}