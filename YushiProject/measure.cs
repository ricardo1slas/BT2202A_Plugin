using OpenTap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
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
        #endregion

        public int meas = 1; 

        public Measure(){

        }

        public override void PrePlanRun(){
            base.PrePlanRun();
        }

        public override void Run()
        {   // pre run
            try{

                Log.Info("Initializing Measure");

            }
            catch (Exception ex){
                Log.Error($"Error during PrePlanRun: {ex.Message}");
            }

            // run

            try{
                // Log the start of the charging process.
                Log.Info("Starting the measure process.");

                instrument.ScpiCommand("OUTP ON");
                Log.Info("Output enabled.");

                //child steps
                RunChildSteps();

                while (meas == 1){
                    try{
                        // Query the instrument for voltage and current measurements.
                        string statusResponse = instrument.ScpiQuery("STATus:CELL:REPort? (@1001)");
                        int statusValue = int.Parse(statusResponse);
                        Log.Info($"Status Value: {statusValue}");
                            if (statusValue == 2) {
                            UpgradeVerdict(Verdict.Fail);
                            instrument.ScpiCommand("OUTP OFF"); // Turn off output
                            return;
                        }

                        Thread.Sleep(1000);

                        string measuredVoltage = instrument.ScpiQuery("MEAS:CELL:VOLT? (@1001)");
                        string measuredCurrent = instrument.ScpiQuery("MEAS:CELL:CURR? (@1001)");

                        // Log the measurements.
                        Log.Info($" Voltage: {measuredVoltage} V, Current: {measuredCurrent} A");

                    }
                    catch {
                        UpgradeVerdict(Verdict.Fail);
                        instrument.ScpiCommand("OUTP OFF"); // Turn off output
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