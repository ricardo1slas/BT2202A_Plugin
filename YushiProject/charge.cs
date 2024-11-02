using OpenTap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace BT2202a
{
    [Display("Charge", Group: "instrument", Description: "Charges a device with specified voltage and current for a set duration.")]
    [AllowAnyChild]
    public class Charge : TestStep
    {
        #region Settings
        // Properties for voltage, current, and time
        [Display("Voltage (V)", Order: 1, Description: "The voltage level to set during charging.")]
        public double Voltage { get; set; }

        [Display("Current (A)", Order: 2, Description: "The current level to set during charging.")]
        public double Current { get; set; }

        [Display("Time (s)", Order: 3, Description: "The duration of the charge in seconds.")]
        public double Time { get; set; }

        // Reference to the instrument Instrument
        [Display("Instrument", Order: 4, Description: "The instrument instrument to use for charging.")]
        public ScpiInstrument instrument { get; set; }
        #endregion

        // Additional fields used during the charging process.
        private bool abortAllProcesses = false;
        private List<string> moduleCommands = new List<string>(); // Example placeholder; populate as needed.
        private List<string> chargeCommands = new List<string>(); // Example placeholder; populate as needed.
        private int chargeCommandCounter = 0;
        private Dictionary<string, int> commandIterationCount = new Dictionary<string, int>(); // Example placeholder.

        public Charge()
        {
            // Set default values for the properties.
            Voltage = 0; // Default voltage, adjust as needed.
            Current = 0; // Default current, adjust as needed.
            Time = 0;   // Default duration, adjust as needed.
        }

        public override void PrePlanRun()
        {
            base.PrePlanRun();
        }

        public override void Run()
        {   // pre run
            try
            {
                // Custom setup logic for the Charge test step.
                if (abortAllProcesses)
                {
                    Log.Error("Process aborted. Exiting PrePlanRun.");
                    return;
                }

                instrument.ScpiCommand("*IDN?");
                instrument.ScpiCommand("*RST");
                instrument.ScpiCommand("SYST:PROB:LIM 1,0");

                foreach (var command in moduleCommands)
                {
                    instrument.ScpiCommand(command);
                    Log.Info($"Executed: {command}");
                }

                if (chargeCommandCounter < chargeCommands.Count)
                {
                    instrument.ScpiCommand(chargeCommands[chargeCommandCounter]);
                    Log.Info($"Executed Charge Command: {chargeCommands[chargeCommandCounter]}");
                    chargeCommandCounter++;
                }
                else
                {
                    Log.Info("All charge commands executed.");
                }
                instrument.ScpiCommand("CELL:DEF:QUICk 4");

                Log.Info($"Charge sequence step defined: Voltage = {Voltage} V, Current = {Current} A, Time = {Time} s");

                Log.Info("Initializing Charge");
                Log.Info("Charge Process Started");
            }
            catch (Exception ex)
            {
                Log.Error($"Error during PrePlanRun: {ex.Message}");
            }

            // run

            try
            {
                instrument.ScpiCommand($"SEQ:STEP:DEF 1,1, CHARGE, {Time}, {Current}, {Voltage}");

                // Log the start of the charging process.
                Log.Info("Starting the charging process.");

                instrument.ScpiCommand("OUTP ON");
                Log.Info("Output enabled.");

                //child steps
                RunChildSteps();

                // Enable and Initialize Cells
                instrument.ScpiCommand("CELL:ENABLE (@1001:1005),1");
                instrument.ScpiCommand("CELL:INIT (@1001,1005)");
                DateTime startTime = DateTime.Now;

                {
                while ((DateTime.Now - startTime).TotalSeconds < Time)
                {
                    try {
                        // Query the instrument for voltage and current measurements.
                        string statusResponse = instrument.ScpiQuery("STATus:CELL:REPort? (@1001)");
                        int statusValue = int.Parse(statusResponse);
                        Log.Info($"Status Value: {statusValue}");
                        
                        if (statusValue == 2) {
                            UpgradeVerdict(Verdict.Fail);
                            instrument.ScpiCommand("OUTP OFF"); // Turn off output
                            return;
                        }

                        string measuredVoltage = instrument.ScpiQuery("MEAS:CELL:VOLT? (@1001)");
                        string measuredCurrent = instrument.ScpiQuery("MEAS:CELL:CURR? (@1001)");

                        // Log the measurements.
                        double elapsedSeconds = (DateTime.Now - startTime).TotalSeconds;
                        //Log.Info($"Time: {elapsedSeconds:F2}s, Voltage: {measuredVoltage} V, Current: {measuredCurrent} A, Temperature: {temperature} C");
                        Log.Info($"Time: {elapsedSeconds:F2}s, Voltage: {measuredVoltage} V, Current: {measuredCurrent} A");

                        if (abortAllProcesses)
                        {
                            Log.Warning("Charging process aborted by user.");
                            break;
                        }
                    }
                    catch {
                        UpgradeVerdict(Verdict.Fail);
                        instrument.ScpiCommand("OUTP OFF"); // Turn off output
                        return;
                    }
                    }
                    Thread.Sleep(1000);
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
                UpgradeVerdict(Verdict.Pass);
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