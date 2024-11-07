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
    [Display("Charge", Group: "instrument", Description: "Charges a device with specified voltage and current for a set duration.")]
    [AllowAnyChild]
    public class Measure : TestStep
    {
        #region Settings
        [Display("Instrument", Order: 1, Description: "The instrument instrument to use for charging.")]
        public ScpiInstrument instrument { get; set; }
        #endregion

        public string[] cell_list;
        public int meas = 1; 

        public Measure(){

        }

        public override void PrePlanRun(){
            base.PrePlanRun();
        }

        public override void Run()
        {   // pre run
            try{

                instrument.ScpiCommand("*IDN?");
                instrument.ScpiCommand("*RST");
                instrument.ScpiCommand("SYST:PROB:LIM 1,0");


                Log.Info("Initializing Charge");
                Log.Info("Charge Process Started");
            }
            catch (Exception ex){
                Log.Error($"Error during PrePlanRun: {ex.Message}");
            }

            // run

            try{
                // Log the start of the charging process.
                Log.Info("Starting the charging process.");

                instrument.ScpiCommand("OUTP ON");
                Log.Info("Output enabled.");

                //child steps
                RunChildSteps();


                DateTime startTime = DateTime.Now;

                while (meas == 1){
                    try{
                        // Query the instrument for voltage and current measurements.
                        string statusResponse = instrument.ScpiQuery("STATus:CELL:REPort?");
                        int statusValue = int.Parse(statusResponse);
                        Log.Info($"Status Value: {statusValue}");
                        Thread.Sleep(1000);
                            if (statusValue == 2) {
                            UpgradeVerdict(Verdict.Fail);
                            instrument.ScpiCommand("OUTP OFF"); // Turn off output
                            return;
                        }

                        string measuredVoltage = instrument.ScpiQuery("MEAS:CELL:VOLT?");
                        string measuredCurrent = instrument.ScpiQuery("MEAS:CELL:CURR?");

                        // Log the measurements.
                        double elapsedSeconds = (DateTime.Now - startTime).TotalSeconds;
                        Log.Info($"Time: {elapsedSeconds:F2}s, Voltage: {measuredVoltage} V, Current: {measuredCurrent} A");

                    }
                    catch {
                        UpgradeVerdict(Verdict.Fail);
                        instrument.ScpiCommand("OUTP OFF"); // Turn off output
                        return;
                    }
                }

                // Turn off the output after the charging process is complete.
                instrument.ScpiCommand("OUTP OFF");
                Log.Info("Charging process completed and output disabled.");

                // Update the test verdict to pass if everything went smoothly.
                UpgradeVerdict(Verdict.Pass);
            }

            catch (Exception ex){
                // Log the error and set the test verdict to fail.
                Log.Error($"An error occurred during the charging process: {ex.Message}");
                UpgradeVerdict(Verdict.Fail);
            }

            try{
                UpgradeVerdict(Verdict.Pass);
                // Any cleanup code that needs to run after the test plan finishes.
                instrument.ScpiCommand("*RST"); // Reset the instrument again after the test.
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