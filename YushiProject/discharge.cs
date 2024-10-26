using OpenTap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace BT2202a
{
    [Display("Discharge", Group: "instrument", Description: "Discharges a device with specified voltage and current for a set duration.")]
   
    public class Discharge : TestStep
    {
        #region Settings
        // Properties for voltage, current, and time
        [Display("Voltage (V)", Order: 1, Description: "The voltage level to set during charging.")]
        public double Voltage { get; set; }

        [Display("Current (A)", Order: 2, Description: "The current level to set during charging.")]
        public double Current { get; set; }

        [Display("Time (s)", Order: 3, Description: "The duration of the Discharge in seconds.")]
        public double Time { get; set; }

        // Reference to the instrument Instrument
        [Display("Instrument", Order: 4, Description: "The instrument instrument to use for charging.")]
        public ScpiInstrument instrument { get; set; }
        #endregion

        // Additional fields used during the charging process.
        private bool abortAllProcesses = false;
       
        public Discharge()
        {
            // Set default values for the properties.
            Voltage = 0; // Default voltage, adjust as needed.
            Current = 0; // Default current, adjust as needed.
            Time = 0;   // Default duration, adjust as needed.
        }

        public override void PrePlanRun()
        {   // pre run
            base.PrePlanRun();
            /*
            try
            {
                instrument.ScpiCommand("*IDN?");
                instrument.ScpiCommand("*RST");
                instrument.ScpiCommand("SYST:PROB:LIM 1,0");

                instrument.ScpiCommand("CELL:DEF:QUICk 4");

               
                Log.Info($"Discharge sequence step defined: Voltage = {Voltage} V, Current = {Current} A, Time = {Time} s");

                //instrument.ScpiCommand("CELL:ENABLE (@1001:1005),1");
                //instrument.ScpiCommand("CELL:INIT (@1001,1005)");

                Log.Info("Initializing Discharge");
                Thread.Sleep(15000); // Wait for 15 seconds
                Log.Info("Discharge Process Started");
            }
            catch (Exception ex)
            {
                Log.Error($"Error during PrePlanRun: {ex.Message}");
            }*/
        }

        public override void Run()
        {
            // pre run
            try
            {
                instrument.ScpiCommand("*IDN?");
                instrument.ScpiCommand("*RST");
                instrument.ScpiCommand("SYST:PROB:LIM 1,0");

                instrument.ScpiCommand("CELL:DEF:QUICk 4");


                Log.Info($"Discharge sequence step defined: Voltage = {Voltage} V, Current = {Current} A, Time = {Time} s");

                //instrument.ScpiCommand("CELL:ENABLE (@1001:1005),1");
                //instrument.ScpiCommand("CELL:INIT (@1001,1005)");

                Log.Info("Initializing Discharge");
                Thread.Sleep(15000); // Wait for 15 seconds
                Log.Info("Discharge Process Started");
            }
            catch (Exception ex)
            {
                Log.Error($"Error during PrePlanRun: {ex.Message}");
            }
            // run
            try
            {
                instrument.ScpiCommand($"SEQ:STEP:DEF 1,1, Discharge, {Time}, {Current}, {Voltage}");

                // Enable and Initialize Cells
                instrument.ScpiCommand("CELL:ENABLE (@1001:1005),1");
                instrument.ScpiCommand("CELL:INIT (@1001,1005)");

                // Log the start of the charging process.
                Log.Info("Starting the charging process.");

                // Enable the output to start the sequence.
                instrument.ScpiCommand("OUTP ON");
                Log.Info("Output enabled.");

                // Wait for the specified charging time to elapse.
                DateTime startTime = DateTime.Now;
                string csvPath = "Measurements_Discharge.csv";  // Path for the CSV file

                using (StreamWriter writer = new StreamWriter(csvPath))
                {
                    writer.WriteLine("Time (s), Voltage (V), Current(A)");
                    
                    while ((DateTime.Now - startTime).TotalSeconds < Time)
                    {
                        // Query the instrument for voltage and current measurements.
                        string measuredVoltage = instrument.ScpiQuery("MEAS:CELL:VOLT? (@1001)");
                        string measuredCurrent = instrument.ScpiQuery("MEAS:CELL:CURR? (@1001)");

                        double elapsedSeconds = (DateTime.Now - startTime).TotalSeconds;
                        Log.Info($"Time: {elapsedSeconds:F2}s, Voltage: {measuredVoltage} V, Current: {measuredCurrent} A");

                        writer.WriteLine($"{elapsedSeconds:F2}, {measuredVoltage}, {measuredCurrent}");

                        // Check for any abort signal or abnormal conditions (e.g., overheating).
                        if (abortAllProcesses)
                        {
                            Log.Warning("Charging process aborted by user.");
                            break;
                        }

                        // Sleep for 1 second before the next measurement.
                        Thread.Sleep(1000);
                    }
                }

                // Turn off the output after the charging process is complete.
                instrument.ScpiCommand("OUTP OFF");
                Log.Info("Charging process completed and output disabled.");

                // Update the test verdict to pass if everything went smoothly.
                UpgradeVerdict(Verdict.Pass);
            }
            catch (Exception ex)
            {
                // Log the error and set the test verdict to fail.
                Log.Error($"An error occurred during the charging process: {ex.Message}");
                UpgradeVerdict(Verdict.Fail);
            }

            // post run
            try
            {
                // Any cleanup code that needs to run after the test plan finishes.
                instrument.ScpiCommand("*RST"); // Reset the instrument again after the test.
                Log.Info("Instrument reset after test completion.");
            }
            catch (Exception ex)
            {
                Log.Error($"Error during PostPlanRun: {ex.Message}");
            }

        }

        public override void PostPlanRun()
        {
            /*try
            {
                // Any cleanup code that needs to run after the test plan finishes.
                instrument.ScpiCommand("*RST"); // Reset the instrument again after the test.
                Log.Info("Instrument reset after test completion.");
            }
            catch (Exception ex)
            {
                Log.Error($"Error during PostPlanRun: {ex.Message}");
            }*/
            base.PostPlanRun();
        }
    }
}