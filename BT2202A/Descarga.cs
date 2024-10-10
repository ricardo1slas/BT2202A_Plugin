using OpenTap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;


// genera test steps para la descarga definiendo unos parametros 
namespace OpenTap.Plugins.Carga
{
    [Display("Descarga", Group: "OpenTap.Plugins.Carga", Description: "Proceso descarga bateria")]
    public class Descarga : TestStep
    {
        #region Settings

        public const double prueba = 3;
        public const double prueba2 = 0.1;

        [Display("Instrument", Group: "Resources")]
        public BT2202A BT2202A { get; set; }

        [Display("Cutoff Voltage", Group: "Connections", Order: 1.1)]
        public double CVoltage { get; set; } = prueba;

        [Display("Charge Current", Group: "Connections", Order: 1.2)]
        public double CCurrent { get; set; } = prueba2;

        [Display("Cutoff Temperature", Group: "Connections", Order: 1.3)]
        public double CTemperature { get; set; } = 0;

        [Display("Time (seconds)", Group: "Connections", Order: 1.4)]
        public double Seconds { get; set; } = 0;
        #endregion

        public Descarga()
        {
            // ToDo: Set default values for properties / settings.
        }

        public override void Run()
        {
            BT2202A.Descarga(CVoltage, CCurrent, CTemperature, Seconds);
            RunChildSteps(); //If the step supports child steps.

            // If no verdict is used, the verdict will default to NotSet.
            // You can change the verdict using UpgradeVerdict() as shown below.
            // UpgradeVerdict(Verdict.Pass);
        }
    }
}