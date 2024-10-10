using OpenTap;   // Use OpenTAP infrastructure/core components (log,TestStep definition, etc)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// genera test steps para la carga definiendo unos parametros 
namespace OpenTap.Plugins.Carga
{
    [Display("Carga", Group: "OpenTap.Plugins.Carga", Description: "Carga de celda")]
    public class Carga : TestStep
    {
        private static int GlobalCounter = 0;
        #region Settings
        [Display("Instrument", Group: "Resources")]
        public BT2202A BT2202A { get; set; }

        [Display("Overvoltage", Group: "Connections", Order: 1.1)]
        public double Overvoltage { get; set; } = 0;

        [Display("Charge Current", Group: "Connections", Order: 1.2)]
        public double CCurrent { get; set; } = 0;

        [Display("Cutoff Temperature", Group: "Connections", Order: 1.3)]
        public double CTemperature { get; set; } = 0;

        [Display("Time (seconds)", Group: "Connections", Order: 1.4)]
        public double Seconds { get; set; } = 0;



        #endregion


        public Carga()
        {
            // ToDo: Set default values for properties / settings.
        }

        public override void Run()
        {
            BT2202A.Carga(Overvoltage, CCurrent, CTemperature, Seconds);
            Log.Info(Carga.GlobalCounter.ToString());
            GlobalCounter += 3;
            Log.Info(Carga.GlobalCounter.ToString());



            // ToDo: Add test case code.
            RunChildSteps(); //If the step supports child steps.

            // If no verdict is used, the verdict will default to NotSet.
            // You can change the verdict using UpgradeVerdict() as shown below.
            // UpgradeVerdict(Verdict.Pass);
        }
    }
}