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
    [AllowAnyChild]
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


                Log.Info("Initializing Discharge");
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

                // Log the start of the charging process.
                Log.Info("Starting the discharging process.");

                // Enable the output to start the sequence.
                instrument.ScpiCommand("OUTP ON");
                Log.Info("Output enabled.");

                //child steps
                RunChildSteps();

                // Enable and Initialize Cells
                instrument.ScpiCommand("CELL:ENABLE (@1001:1005),1");
                instrument.ScpiCommand("CELL:INIT (@1001,1005)");

                // Wait for the specified charging time to elapse.
                DateTime startTime = DateTime.Now;

                {
                    while ((DateTime.Now - startTime).TotalSeconds < Time)
                    {
                        // Query the instrument for voltage and current measurements.
                        instrument.ScpiQuery("STAT:CELL:REP? (@1001)");
                        string measuredVoltage = instrument.ScpiQuery("MEAS:CELL:VOLT? (@1001)");
                        string measuredCurrent = instrument.ScpiQuery("MEAS:CELL:CURR? (@1001)");

                        double elapsedSeconds = (DateTime.Now - startTime).TotalSeconds;
                        Log.Info($"Time: {elapsedSeconds:F2}s, Voltage: {measuredVoltage} V, Current: {measuredCurrent} A");

                        // Check for any abort signal or abnormal conditions (e.g., overheating).
                        if (abortAllProcesses)
                        {
                            Log.Warning("Discharging process aborted by user.");
                            break;
                        }

                        // Sleep for 1 second before the next measurement.
                        Thread.Sleep(1000);
                    }
                }

                // Turn off the output after the charging process is complete.
                instrument.ScpiCommand("OUTP OFF");
                Log.Info("Discharging process completed and output disabled.");

                // Update the test verdict to pass if everything went smoothly.
            }
            catch (Exception ex)
            {
                // Log the error and set the test verdict to fail.
                Log.Error($"An error occurred during the discharging process: {ex.Message}");
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
            base.PostPlanRun();
        }
    }
}