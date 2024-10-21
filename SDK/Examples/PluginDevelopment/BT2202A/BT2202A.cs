using OpenTap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace OpenTap.Plugins.Carga
//Aqui empezamos a declarar los instrumentos que vamos a ustilizar
{

    // lo mismo, declaramos el uso de instrumenot BT
    [Display("BT2202A", Group: "OpenTap.Plugins.Carga", Description: "Equipo para carga y descarga")]
    public class BT2202A : ScpiInstrument
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion

        private List<string[]> numberOfChannelsMatrix;
        private List<string[]> actionMatrix;
        private List<string[]> moduleNumberMatrix;

        private List<string> chargeCommands;
        private List<string> dischargeCommands;
        private List<string> restCommands;
        private List<string> moduleCommands;

        private Dictionary<string, int> commandIterationCount;
        private int chargeCommandCounter = 0;
        private int dischargeCommandCounter = 0;

        private bool abortAllProcesses = false;

        public BT2202A()
        {
            Name = "BT2202A";
            VisaAddress = "USB0::0x008D::0x3602::MY59002216::0::INSTR";


            // Paramenter Setup
            numberOfChannelsMatrix = new List<string[]>();
            actionMatrix = new List<string[]>();
            moduleNumberMatrix = new List<string[]>();
            chargeCommands = new List<string>();
            dischargeCommands = new List<string>();
            restCommands = new List<string>();
            moduleCommands = new List<string>();
            commandIterationCount = new Dictionary<string, int>();

        }


        public override void Open()
        {
            base.Open();
            string Identifier = ScpiQuery("*IDN?");
            Log.Info(Identifier);


            // Load and parse the CSV file desde un dispositivo local
            string filePath = @"C:\Users\Admin\Desktop\BT2202A\BT2202A\Yushipoo.csv";
            ParseCsvFile(filePath);
            GenerateScpiCommands();
        }

        private void ParseCsvFile(string filePath)
        {
            using (var reader = new StreamReader(filePath))  //Lee el archivo CSV, se separa por comas, se guarda todo en una matriz dependiendo de clasificacion.
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    if (values[0].Contains("Number of Channels per Cell"))
                    {
                        numberOfChannelsMatrix.Add(values);
                    }
                    else if (values[0].Contains("Action"))
                    {
                        actionMatrix.Add(values);
                    }
                    else if (values[0].Contains("Module Number"))
                    {
                        moduleNumberMatrix.Add(values);
                    }
                }
            }
        }

        private void GenerateScpiCommands()
        {
            Dictionary<string, int> moduleCounter = new Dictionary<string, int>(); // Contador de módulos por columna
            foreach (var row in moduleNumberMatrix) // Itera sobre cada fila de la matriz de números de módulo
            {
                if (row.Length >= 4)  // Asegura que la fila tenga al menos 4 columnas
                {
                    string secondColumn = row[1];
                    string fourthColumn = row[3];

                    if (!moduleCounter.ContainsKey(secondColumn)) // Si la columna no está en el contador
                    {
                        moduleCounter[secondColumn] = 1;// Inicializa el contador
                    }
                    else // si no
                    {
                        moduleCounter[secondColumn]++;// Incrementa el contador
                    }

                    int moduleNumber = 1000 * int.Parse(secondColumn) + moduleCounter[secondColumn];
                    string scpiCommand = $"CELL:DEFINE {moduleNumber},(@{fourthColumn})";  //damos el formato a los comandos segun la CSV
                    moduleCommands.Add(scpiCommand); // Add command to the list
                    Log.Info($"Generated: {scpiCommand}");// Registra el comando generado
                }
            }

            int count = 1;
            foreach (var row in actionMatrix)  // Itera sobre cada fila de la matriz de acciones
            {
                if (row.Length >= 14) // Asegura que la fila tenga al menos 14 columnas
                {
                    string secondColumn = row[1];
                    string fourteenthColumn = row[13];
                    string sixthColumn = row[5];
                    string fourthColumn = row[3];
                    string scpiCommand = $"SEQ:STEP:DEF 1,1,{secondColumn},{fourteenthColumn},{sixthColumn},{fourthColumn}"; //definimos el orden dependiendo de ka informacion de CSV
                    // para Generar el comando SCPI

                    int iterations = int.Parse(fourteenthColumn);
                    commandIterationCount[scpiCommand] = iterations;

                    if (secondColumn.Equals("CHARGE", StringComparison.OrdinalIgnoreCase))// Si es un comando de carga
                    {
                        chargeCommands.Add(scpiCommand);// Agrega a la lista de comandos de carga
                    }
                    else if (secondColumn.Equals("DISCHARGE", StringComparison.OrdinalIgnoreCase))// Si es un comando de descarga
                    {
                        dischargeCommands.Add(scpiCommand);
                    }
                    else if (secondColumn.Equals("REST", StringComparison.OrdinalIgnoreCase)) // Si es un comando de rest
                    {
                        restCommands.Add(scpiCommand);
                    }

                    Log.Info($"Generated: {scpiCommand}"); //desplegar informacion o comandos
                    count++;
                }
            }
        }

        public void Carga(double Overvoltage, double CCurrent, double Seconds)
        {
            if (abortAllProcesses)
            {
                Log.Error("Process aborted. Exiting Carga method.");
                return; // Sale del método si está activado el aborto de proceso.
            }

            ScpiCommand("*IDN?");
            ScpiCommand("*RST");
            ScpiCommand("SYST:PROB:LIM 1,0");

            foreach (var command in moduleCommands) // Ejecuta cada comando en la lista de comandos de módulo que fue generado previamente.
            {
                ScpiCommand(command);
                Log.Info($"Executed: {command}");// Registra el comando ejecutado
            }

            if (chargeCommandCounter < chargeCommands.Count)// Verifica si hay más comandos de carga para ejecutar
            {
                ScpiCommand(chargeCommands[chargeCommandCounter]); // se ejecuta la carga actual
                Log.Info($"Executed Charge Command: {chargeCommands[chargeCommandCounter]}");  // Incrementa el contador de comandos de carga
                chargeCommandCounter++;
            }
            else
            {
                Log.Info("All charge commands executed.");
                return;
            }

            ScpiCommand("CELL:ENABLE (@1001),1"); // Habilita la celda
            ScpiCommand("CELL:INIT (@1001)");

            Log.Info("Initializing Charge");
            Thread.Sleep(15000); // Espera 15 segundos
            Log.Info("Charge Process Started");

            string csvPath = "Measurements_Charge.csv";  // Ruta del archivo CSV para almacenar las mediciones
            using (StreamWriter writer = new StreamWriter(csvPath))
            {
                writer.WriteLine("Time (s), Voltage (V), Current(A)"); // encabezados de CSV

                int iterations = commandIterationCount[chargeCommands.First()]; // Obtiene el número de iteraciones para el comando actual
                for (int i = 0; i < iterations; i++)
                {
                    if (abortAllProcesses)
                    {
                        Log.Error("Process aborted. Exiting loop in Carga method.");
                        break;
                    }

                    Thread.Sleep(1000);
                    string voltage = ScpiQuery("MEAS:CELL:VOLT? (@1001)"); // Consulta el voltaje
                    Log.Info($"Voltage at second {i + 1}: {voltage}");// Registra el voltaje
                    string current = ScpiQuery("MEAS:CELL:CURRent? (@1001)");

                }
            }

            if (!abortAllProcesses)
            {
                ScpiCommand("SEQ:ABORT");
                Log.Info("SEQ:ABORT");
            }
        }



        // Se utiliza comandos similares que la de la carga y sigue la misma logica.
        public void Descarga(double CVoltage, double CCurrent, double Seconds)
        {
            if (abortAllProcesses)
            {
                Log.Error("Process aborted. Exiting Descarga method.");
                return;
            }

            ScpiCommand("*IDN?");
            ScpiCommand("*RST");
            ScpiCommand("SYST:PROB:LIM 1,0");

            foreach (var command in moduleCommands)
            {
                ScpiCommand(command);
                Log.Info($"Executed: {command}");
            }

            if (dischargeCommandCounter < dischargeCommands.Count)
            {
                ScpiCommand(dischargeCommands[dischargeCommandCounter]);
                Log.Info($"Executed Discharge Command: {dischargeCommands[dischargeCommandCounter]}");
                dischargeCommandCounter++;
            }
            else
            {
                Log.Info("All discharge commands executed.");
                return;
            }

            ScpiCommand("CELL:ENABLE (@1001),1");
            ScpiCommand("CELL:INIT (@1001)");

            Log.Info("Initializing Discharge");
            Thread.Sleep(15000);
            Log.Info("Discharge Process Started");

            string csvPath = "Measurements_Discharge.csv";
            using (StreamWriter writer = new StreamWriter(csvPath))
            {
                writer.WriteLine("Time (s), Voltage (V), Current(A));");

                int iterations = commandIterationCount[dischargeCommands.First()];
                for (int i = 0; i < iterations; i++)
                {
                    if (abortAllProcesses)
                    {
                        Log.Error("Process aborted. Exiting loop in Descarga method.");
                        break;
                    }

                    Thread.Sleep(1000);
                    string voltage = ScpiQuery("MEAS:CELL:VOLT? (@1001)");
                    Log.Info($"Voltage at second {i + 1}: {voltage}");
                    string current = ScpiQuery("MEAS:CELL:CURRent? (@1001)");
                    Log.Info($"Current at second {i + 1}: {current}");



                    // Verifica la temperatura y aborta si es necesario

                }
            }

            if (!abortAllProcesses)
            {
                ScpiCommand("SEQ:ABORT");
                Log.Info("SEQ:ABORT");
            }
        }
    }
}
