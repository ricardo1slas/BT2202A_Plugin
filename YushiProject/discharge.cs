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
    [Display("discharge", Group: "instrument", Description: "discharges a device with specified voltage and current for a set duration.")]
   
    public class discharge : TestStep
    {
        #region Settings

        // Reference to the instrument Instrument
        [Display("Instrument", Order: 1, Description: "The instrument instrument to use for charging.")]
        public ScpiInstrument instrument { get; set; }
        // Properties for voltage, current, and time
        [Display("Voltage (V)", Order: 2, Description: "The voltage level to set during charging.")]
        public double Voltage { get; set; }

        [Display("Current (A)", Order: 3, Description: "The current level to set during charging.")]
        public double Current { get; set; }

        [Display("Time (s)", Order: 4, Description: "The duration of the discharge in seconds.")]
        public double Time { get; set; }

        [Display("Cell size", Order: 5, Description:"Number of channels per cell")]
        public double Channels {get; set;}

        [Display("Cell group", Order: 6, Description:"Number of cells per cell group, asign as lowest:highest or comma separated list")]
        public string cell_group;
        #endregion

        public string[] cell_list;

        // Additional fields used during the charging process.
        private bool abortAllProcesses = false;
       
        public discharge()
        {
            // Set default values for the properties.
            Voltage = 0; // Default voltage, adjust as needed.
            Current = 0; // Default current, adjust as needed.
            Time = 0;   // Default duration, adjust as needed.
            Channels = 4;
            cell_group = "1001:1005";
        }


        public override void PrePlanRun()
        {   // pre run
            base.PrePlanRun();
        }

        public override void Run()
        {
            // pre run
            try
            {
                instrument.ScpiCommand("*IDN?");
                instrument.ScpiCommand("*RST");
                instrument.ScpiCommand("SYST:PROB:LIM 1,0");

                instrument.ScpiCommand($"CELL:DEF:QUICk {Channels}");
                Log.Info($"Assigned {Channels} per cell");

                Log.Info($"discharge sequence step defined: Voltage = {Voltage} V, Current = {Current} A, Time = {Time} s");

                Log.Info("Initializing discharge");
                Log.Info("discharge Process Started");
            }
            catch (Exception ex)
            {
                Log.Error($"Error during PrePlanRun: {ex.Message}");
            }
            // run
            try
            {
                instrument.ScpiCommand($"SEQ:STEP:DEF 1,1, DISCHARGE, {Time}, {Current}, {Voltage}");

                char[] delimiterChars = {',', ':'};
                cell_group = cell_group.Replace(" ","");
                cell_list = cell_group.Split(delimiterChars);

                // Enable and Initialize Cells
                instrument.ScpiCommand($"CELL:ENABLE (@{cell_group}),1");
                instrument.ScpiCommand($"CELL:INIT (@{cell_group})");

                // Log the start of the charging process.
                Log.Info("Starting the charging process.");

                // Enable the output to start the sequence.
                instrument.ScpiCommand("OUTP ON");
                Log.Info("Output enabled.");

                // Wait for the specified charging time to elapse.
                DateTime startTime = DateTime.Now;
                string csvPath = "Measurements_discharge.csv";  // Path for the CSV file

                using (StreamWriter writer = new StreamWriter(csvPath))
                {
                    writer.WriteLine("Time (s), Voltage (V), Current(A)");
                    
                    while ((DateTime.Now - startTime).TotalSeconds < Time)
                    {
                        // Query the instrument for voltage and current measurements.
                        string measuredVoltage = instrument.ScpiQuery($"MEAS:CELL:VOLT? (@{cell_list[0]})");
                        string measuredCurrent = instrument.ScpiQuery($"MEAS:CELL:CURR? (@{cell_list[0]})");

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