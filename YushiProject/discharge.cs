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
        [Display("Instrument", Order: 1, Description: "The instrument instrument to use for charging.")]
        public ScpiInstrument instrument { get; set; }
        // Properties for voltage, current, and time
        [Display("Voltage (V)", Order: 2, Description: "The voltage level to set during charging.")]
        public double Voltage { get; set; }

        [Display("Current (A)", Order: 3, Description: "The current level to set during charging.")]
        public double Current { get; set; }

        [Display("Time (s)", Order: 4, Description: "The duration of the charge in seconds.")]
        public double Time { get; set; }

        [Display("Sequence Number", Order: 5, Description: "The # of the sequence")]
        public double Sequence { get; set; }

        [Display("Step Number", Order: 6, Description: "The # of the step inside a sequence")]
        public double Step { get; set; }

        // Reference to the instrument Instrument
        [Display("Cell size", Order: 7, Description: "Number of channels per cell")]
        public double Channels { get; set; }

        [Display("Cell group", Order: 8, Description: "Number of cells per cell group, asign as lowest:highest or comma separated list")]
        public string cell_group { get; set; }
        #endregion

        public string[] cell_list;

        // Additional fields used during the charging process.
        // private bool abortAllProcesses = false;

        public Discharge()
        {
            // Set default values for the properties.
            Voltage = 0; // Default voltage, adjust as needed.
            Current = 0; // Default current, adjust as needed.
            Time = 0;   // Default duration, adjust as needed.
        }

        public override void PrePlanRun()
        {   // pre run
            var data = new { sleep = 1 };
            jsonHelper.jsonAider.write_json(data);
            base.PrePlanRun();
        }

        public override void Run()
        {
            // pre run
            var data = new { sleep = 1 };
            jsonHelper.jsonAider.write_json(data);

            try { 
                instrument.ScpiCommand("*IDN?");
                instrument.ScpiCommand("SYST:PROB:LIM 1,0");
                instrument.ScpiCommand($"CELL:DEF:QUICk {Channels}");

                Log.Info($"Discharge sequence step {Sequence},{Step} defined: Voltage = {Voltage} V, Current = {Current} A, Time = {Time} s");

                Log.Info("Initializing Discharge");
                Log.Info("Discharge Process Started");
            }
            catch (Exception ex) {
                Log.Error($"Error during PrePlanRun: {ex.Message}");
            }
            // run
            try
            {
                instrument.ScpiCommand($"SEQ:STEP:DEF {Sequence},{Step}, DISCHARGE, {Time}, {Current}, {Voltage}");
                char[] delimiterChars = { ',', ':' };
                cell_group = cell_group.Replace(" ", "");
                cell_list = cell_group.Split(delimiterChars);

                // Log the start of the charging process.
                Log.Info("Starting the discharging process.");

                //child steps
                RunChildSteps();

                // Enable and Initialize Cells
                instrument.ScpiCommand($"CELL:ENABLE (@{cell_group}),{Sequence}");
                instrument.ScpiCommand($"CELL:INIT (@{cell_group})");

                // Enable the output to start the sequence.
                instrument.ScpiCommand("OUTP ON");
                Log.Info("Output enabled.");

                // Update the test verdict to pass if everything went smoothly.
                UpgradeVerdict(Verdict.Pass);
                data = new { sleep = 0 };
                jsonHelper.jsonAider.write_json(data);
            }
            catch (Exception ex){
                data = new { sleep = 0 };
                jsonHelper.jsonAider.write_json(data);

                // Log the error and set the test verdict to fail.
                Log.Error($"An error occurred during the charging process: {ex.Message}");
                UpgradeVerdict(Verdict.Fail);
            }

            // post run
            try
            {
                UpgradeVerdict(Verdict.Pass);
                // Any cleanup code that needs to run after the test plan finishes.
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