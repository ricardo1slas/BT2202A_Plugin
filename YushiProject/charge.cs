using OpenTap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace BT2202a
{
    [Display("Charge", Group: "instrument", Description: "Charges a device with specified voltage and current for a set duration.")]
    [AllowAnyChild]
    public class Charge : TestStep
    {
        #region Settings
        [Display("Voltage (V)", Order: 1, Description: "The voltage level to set during charging.")]
        public double Voltage { get; set; }

        [Display("Current (A)", Order: 2, Description: "The current level to set during charging.")]
        public double Current { get; set; }

        [Display("Time (s)", Order: 3, Description: "The duration of the charge in seconds.")]
        public double Time { get; set; }

        [Display("Instrument", Order: 4, Description: "The instrument to use for charging.")]
        public ScpiInstrument Instrument { get; set; }
        #endregion

        private bool abortAllProcesses = false;

        public Charge()
        {
            Voltage = 0;
            Current = 0;
            Time = 0;
        }

        public override void Run()
        {
            // Verify that the instrument is properly configured
            if (Instrument == null)
            {
                Log.Error("Instrument is not configured.");
                UpgradeVerdict(Verdict.Error);
                return;
            }

            try
            {
                // Initialize the instrument setup
                Instrument.ScpiCommand("*IDN?");
                Instrument.ScpiCommand("*RST");
                Instrument.ScpiCommand("SYST:PROB:LIM 1,0");

                // Define the charge sequence step
                Instrument.ScpiCommand($"SEQ:STEP:DEF 1,1, CHARGE, {Time}, {Current}, {Voltage}");

                // Execute child steps in sequence
                foreach (var childStep in EnabledChildSteps)
                {
                    // Run each child step
                    RunChildStep(childStep);

                    // Check the verdict of the child step
                    if (childStep.Verdict == Verdict.Fail)
                    {
                        Log.Error($"Child step '{childStep.Name}' failed.");
                        UpgradeVerdict(Verdict.Fail);
                        return;
                    }
                }

                // Enable and initialize cells for charging
                Instrument.ScpiCommand("CELL:ENABLE (@1001:1005),1");
                Instrument.ScpiCommand("CELL:INIT (@1001,1005)");
                Instrument.ScpiCommand("OUTP ON");
                Log.Info("Output enabled, starting charge.");

                // Log and record measurements during the charging process
                DateTime startTime = DateTime.Now;
                string csvPath = "Measurements_Charge.csv";

                using (StreamWriter writer = new StreamWriter(csvPath))
                {
                    writer.WriteLine("Time (s), Voltage (V), Current (A), Temperature (C)");

                    while ((DateTime.Now - startTime).TotalSeconds < Time)
                    {
                        // Get measurements from the instrument
                        string measuredVoltage = Instrument.ScpiQuery("MEAS:CELL:VOLT? (@1001)");
                        string measuredCurrent = Instrument.ScpiQuery("MEAS:CELL:CURR? (@1001)");
                        double elapsedSeconds = (DateTime.Now - startTime).TotalSeconds;

                        Log.Info($"Time: {elapsedSeconds:F2}s, Voltage: {measuredVoltage} V, Current: {measuredCurrent} A");

                        // Write measurements to the CSV file
                        writer.WriteLine($"{elapsedSeconds:F2}, {measuredVoltage}, {measuredCurrent}");

                        // Check for an abort signal
                        if (abortAllProcesses)
                        {
                            Log.Warning("Charging process aborted by user.");
                            Instrument.ScpiCommand("OUTP OFF");
                            return;
                        }

                        Thread.Sleep(1000);  // Sleep for 1 second between measurements
                    }
                }

                // Complete the charging process by turning off the output
                Instrument.ScpiCommand("OUTP OFF");
                Log.Info("Charging process completed and output disabled.");

                // Reset the instrument after the test
                Instrument.ScpiCommand("*RST");
                Log.Info("Instrument reset after test completion.");

                // Set verdict to Pass if everything completed without errors
                UpgradeVerdict(Verdict.Pass);
            }
            catch (Exception ex)
            {
                Log.Error($"An error occurred during the charging process: {ex.Message}");
                UpgradeVerdict(Verdict.Fail);
            }
        }
    }
}