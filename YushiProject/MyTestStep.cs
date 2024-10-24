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
