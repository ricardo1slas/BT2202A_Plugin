using OpenTap;
using System;
using System.Collections.Generic;

namespace childStep
{
    [Display("Test", Group: "instrument", Description: "Enables Pass/Fail.")]
    public class DurationTestStep : TestStep
    {
        #region Settings
        public enum TestType
        {
            Voltage,
            Current
        }

        public enum Compare
        {
            GreaterThanOrEqual,
            LessThanOrEqual
        }

        public enum When
        {
            Before,
            After,
            At
        }

        public enum Result
        {
            FailAndRemove,
            AdvanceToNextStep
        }

        // Mappings for SCPI-friendly strings
        private static readonly Dictionary<Compare, string> CompareMapping = new Dictionary<Compare, string>
        {
            { Compare.GreaterThanOrEqual, "GE" },
            { Compare.LessThanOrEqual, "LE" }
        };

        private static readonly Dictionary<When, string> WhenMapping = new Dictionary<When, string>
        {
            { When.Before, "BEFORE" },
            { When.After, "AFTER" },
            { When.At, "AT" }
        };

        private static readonly Dictionary<TestType, string> TestTypeMapping = new Dictionary<TestType, string>
        {
            { TestType.Voltage, "VOLT" },
            { TestType.Current, "CURR" }
        };

        // Reference to the instrument
        [Display("Instrument", Order: 4, Description: "The instrument to use for charging.")]
        public ScpiInstrument Instrument { get; set; }
        #endregion

        [Display("Test Type", Order: 1, Description: "Parameter to test.")]
        public TestType SelectedTestType { get; set; }

        [Display("Voltage (V)", Order: 2, Description: "The voltage level to set during charging.")]
        [EnabledIf("SelectedTestType", TestType.Voltage, HideIfDisabled = true)]
        public double Voltage { get; set; }

        [Display("Current (A)", Order: 3, Description: "The current level to set during charging.")]
        [EnabledIf("SelectedTestType", TestType.Current, HideIfDisabled = true)]
        public double Current { get; set; }

        [Display("Compare", Order: 4, Description: "Comparison with the parameter.")]
        public Compare ComparisonOperator { get; set; }

        [Display("When", Order: 5, Description: "When?")]
        public When WhenCondition { get; set; }

        [Display("Time (s)", Order: 6, Description: "The duration of the charge in seconds.")]
        public double Time { get; set; }

        [Display("Result", Order: 7, Description: "Action to take based on result.")]
        public Result ActionResult { get; set; }

        // Constructor to set default values
        public DurationTestStep()
        {
            Voltage = 0;
            Current = 0;
            Time = 10.0;  // Default duration
            SelectedTestType = TestType.Voltage;
            ComparisonOperator = Compare.GreaterThanOrEqual;
            WhenCondition = When.At;
            ActionResult = Result.AdvanceToNextStep;
        }

        public override void Run()
        {
            if (Instrument == null)
            {
                Log.Error("Instrument is not configured.");
                UpgradeVerdict(Verdict.Error);
                return;
            }

            // Fixed initial values as specified in your example
            string fixedValues = "1,1,1";

            // Combine TestType and ComparisonOperator
            string parameterComparison = string.Format("{0}_{1}", TestTypeMapping[SelectedTestType], CompareMapping[ComparisonOperator]);

            // Get the parameter value (Voltage or Current) based on TestType
            double parameterValue = SelectedTestType == TestType.Voltage ? Voltage : Current;

            // Retrieve the WhenCondition and ActionResult as SCPI-compatible strings
            string whenCondition = WhenMapping[WhenCondition];
            string resultAction = ActionResult == Result.FailAndRemove ? "FAIL" : "NEXT";

            // Construct the SCPI command
            string scpiCommand = string.Format("SEQ:TEST:DEF {0},{1},{2},{3},{4},{5}", fixedValues, parameterComparison, parameterValue, whenCondition, Time, resultAction);

            // Send the SCPI command
            try
            {
                Instrument.ScpiCommand(scpiCommand);
                Log.Info($"Sent SCPI command: {scpiCommand}");
                UpgradeVerdict(Verdict.Pass);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to send SCPI command: {ex.Message}");
                UpgradeVerdict(Verdict.Fail);
            }
        }
    }
}