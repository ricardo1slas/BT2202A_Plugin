public override void Run()
{
    try
    {
        // Log the start of the charging process.
        Log.Info("Starting the charging process.");

        // Define the sequence step with the desired parameters.
        // Adjust the command string according to the BT2202A's SCPI command set.
        // SEQ:STEP:DEF <step_id>, CHARGE, <duration>, <CC>, <CV>
        // Assuming CHARGE mode with Constant Current (CC) and Constant Voltage (CV)
        instrument.ScpiCommand($"SEQ:STEP:DEF 1, CHARGE, {Time}, {Current}, {Voltage}");
        Log.Info($"Defined sequence step: Step ID = 1, Mode = CHARGE, Time = {Time}s, Current = {Current}A, Voltage = {Voltage}V");

        // Enable the output to start the sequence.
        instrument.ScpiCommand("SEQ:INIT");
        Log.Info("Sequence initialized.");

        // Monitor the charging process.
        DateTime startTime = DateTime.Now;
        string csvPath = "Measurements_Charge.csv";  // Path for the CSV file

        using (StreamWriter writer = new StreamWriter(csvPath))
        {
            writer.WriteLine("Time (s), Voltage (V), Current (A), Temperature (C)");

            while ((DateTime.Now - startTime).TotalSeconds < Time)
            {
                // Query the instrument for voltage and current measurements.
                string measuredVoltage = instrument.ScpiQuery("MEAS:CELL:VOLT? (@1001)");
                string measuredCurrent = instrument.ScpiQuery("MEAS:CELL:CURR? (@1001)");
                // Assuming the temperature measurement command; adjust as needed.
                string temperature = instrument.ScpiQuery("MEAS:TEMP? (@1001)");

                // Log the measurements.
                double elapsedSeconds = (DateTime.Now - startTime).TotalSeconds;
                Log.Info($"Time: {elapsedSeconds:F2}s, Voltage: {measuredVoltage} V, Current: {measuredCurrent} A, Temperature: {temperature} C");

                // Write to the CSV file.
                writer.WriteLine($"{elapsedSeconds:F2}, {measuredVoltage}, {measuredCurrent}, {temperature}");

                // Check for abort signals or abnormal conditions.
                if (abortAllProcesses)
                {
                    Log.Warning("Charging process aborted by user.");
                    break;
                }

                if (double.TryParse(temperature, out double tempValue) && tempValue >= 30)
                {
                    Log.Error($"Temperature exceeded 30°C: {tempValue}°C at second {elapsedSeconds:F2}. Aborting.");
                    abortAllProcesses = true;
                    instrument.ScpiCommand("SEQ:ABORT");
                    Log.Info("Sequence aborted due to high temperature.");
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
}