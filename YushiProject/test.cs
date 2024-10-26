using OpenTap;
using System;

namespace BT2202a
{
    [Display("MyTestStep", Description: "Insert a description here", Group: "BT2202a")]
    public class MyTestStep : TestStep
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion

        public ScpiInstrument BT2202 { get; set; } 
        public double test { get; set; }
        public MyTestStep()
        {
            // ToDo: Set default values for properties / settings.
        }

        public override void PrePlanRun()
        {
            base.PrePlanRun();
            // ToDo: Optionally add any setup code this step needs to run before the testplan starts
        }

        public override void Run()
        {
            // run
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

                //instrument.ScpiCommand("CELL:ENABLE (@1001:1005),1");
                //instrument.ScpiCommand("CELL:INIT (@1001,1005)");

                Log.Info("Initializing Charge");
                Thread.Sleep(15000); // Wait for 15 seconds
                Log.Info("Charge Process Started");
            }
            catch (Exception ex)
            {
                Log.Error($"Error during PrePlanRun: {ex.Message}");
            }
            
            // ToDo: Add test case code here
            RunChildSteps(); //If step has child steps.

            UpgradeVerdict(Verdict.Pass);
        }

        public override void PostPlanRun()
        {
            // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
            base.PostPlanRun();
        }
    }
}
