public override void Run()
{
    if (Instrument == null)
    {
        Log.Error("Instrument is not configured.");
        UpgradeVerdict(Verdict.Error);
        return;
    }

    try
    {
        // Instrument setup
        Instrument.ScpiCommand("*IDN?");
        Instrument.ScpiCommand("*RST");
        Instrument.ScpiCommand("SYST:PROB:LIM 1,0");

        Instrument.ScpiCommand($"SEQ:STEP:DEF 1,1, CHARGE, {Time}, {Current}, {Voltage}");

        RunChildSteps();

        Instrument.ScpiCommand("CELL:ENABLE (@1001:1005),1");
        Instrument.ScpiCommand("CELL:INIT (@1001,1005)");
        Instrument.ScpiCommand("OUTP ON");
        Log.Info("Output enabled, starting charge.");

        TapThread.Sleep(2000);  // Wait 2 seconds to stabilize

        DateTime startTime = DateTime.Now;
        string csvPath = "Measurements_Charge.csv";

        using (StreamWriter writer = new StreamWriter(csvPath))
        {
            writer.WriteLine("Time (s), Voltage (V), Current (A), Temperature (C)");

            while ((DateTime.Now - startTime).TotalSeconds < Time)
            {
                int originalTimeout = Instrument.Timeout;
                Instrument.Timeout = 10000;  // Temporarily increase timeout for measurement

                try
                {
                    string measuredVoltage = Instrument.ScpiQuery("MEAS:CELL:VOLT? (@1001)");
                    string measuredCurrent = Instrument.ScpiQuery("MEAS:CELL:CURR? (@1001)");
                    double elapsedSeconds = (DateTime.Now - startTime).TotalSeconds;

                    Log.Info($"Time: {elapsedSeconds:F2}s, Voltage: {measuredVoltage} V, Current: {measuredCurrent} A");
                    writer.WriteLine($"{elapsedSeconds:F2}, {measuredVoltage}, {measuredCurrent}");
                }
                catch (TimeoutException)
                {
                    Log.Error("Measurement timeout. Aborting.");
                    UpgradeVerdict(Verdict.Error);
                    return;
                }
                finally
                {
                    Instrument.Timeout = originalTimeout;  // Restore original timeout
                }

                if (abortAllProcesses)
                {
                    Log.Warning("Charging process aborted by user.");
                    Instrument.ScpiCommand("OUTP OFF");
                    return;
                }

                TapThread.Sleep(1000);
            }
        }

        Instrument.ScpiCommand("OUTP OFF");
        Log.Info("Charging process completed and output disabled.");

        Instrument.ScpiCommand("*RST");
        Log.Info("Instrument reset after test completion.");

        UpgradeVerdict(Verdict.Pass);
    }
    catch (Exception ex)
    {
        Log.Error($"An error occurred during the charging process: {ex.Message}");
        UpgradeVerdict(Verdict.Fail);
    }
}