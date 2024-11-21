using OpenTap;
using System;
/*using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;*/
using System.Threading;

namespace BT2202a
{
    [Display("Measure", Group: "instrument", Description: "Measures voltage and current for step duration.")]
    [AllowAnyChild]
    public class Measure : TestStep
    {
        #region Settings
        [Display("Instrument", Order: 1, Description: "The instrument instrument to use for charging.")]
        public ScpiInstrument instrument { get; set; }

        [Display("Cell group", Order: 8, Description: "Cells to measure, asign as lowest:highest or comma separated list")]
        public string cell_group { get; set; }

        [Display("Seconds", Order:2, Description:"How many seconds measure will run, 0 is infinite")]
        public double seconds {get; set;}

        #endregion

        private int meas;

        public Measure(){
            
        }

        public override void PrePlanRun(){
            base.PrePlanRun();
        }

        public override void Run()
        {   // pre run
            meas = 1;
            if (seconds == 0){
                meas = -1;
            }


            try{
                // Log the start of the charging process.
                Log.Info("Starting the measure process.");

                instrument.ScpiCommand("OUTP ON");
                Log.Info("Output enabled.");

                //child steps
                RunChildSteps();
                Log.Info(meas.ToString());
                while (meas <= seconds) {
                    Log.Info(meas.ToString());
                    try{


                        //// Query the instrument for voltage and current measurements.
                        string statusResponse = instrument.ScpiQuery($"STATus:CELL:REPort? (@{cell_group})");
                        int statusValue = int.Parse(statusResponse);
                        Log.Info($"Status Value: {statusValue}");
                            if (statusValue == 2) {
                            UpgradeVerdict(Verdict.Pass);
                            instrument.ScpiCommand("OUTP OFF"); // Turn off output
                            return;
                        }

                        Thread.Sleep(1000);
                        
                        if (seconds != 0){
                            meas = meas + 1;
                        }
                         // BORRAR DESPUES
                        string measuredVoltage = instrument.ScpiQuery($"MEAS:CELL:VOLT? (@{cell_group})");
                        string measuredCurrent = instrument.ScpiQuery($"MEAS:CELL:CURR? (@{cell_group})");

                        // Log the measurements.
                        Log.Info($" Voltage: {measuredVoltage} V, Current: {measuredCurrent} A");

                    }
                    catch {
                        Log.Info("Salio mal");
                        return;
                    }
                }

                // Turn off the output after the charging process is complete.
                instrument.ScpiCommand("OUTP OFF");
                Log.Info("Measure process completed and output disabled.");

                // Update the test verdict to pass if everything went smoothly.
                UpgradeVerdict(Verdict.Pass);
            }

            catch (Exception ex){
                // Log the error and set the test verdict to fail.
                Log.Error($"An error occurred during the measure process: {ex.Message}");
                UpgradeVerdict(Verdict.Fail);
            }

            try{
                UpgradeVerdict(Verdict.Pass);
                // Any cleanup code that needs to run after the test plan finishes.
                Log.Info("Instrument reset after test completion.");
            }
            catch (Exception ex){
                Log.Error($"Error during PostPlanRun: {ex.Message}");
            }

        }

        public override void PostPlanRun(){
            base.PostPlanRun();
        }
    }
}