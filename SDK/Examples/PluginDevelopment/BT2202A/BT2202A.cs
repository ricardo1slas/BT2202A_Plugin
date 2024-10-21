using OpenTap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace OpenTap.Plugins.Carga
{
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

            // Initialize lists with zero values
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
            Log.Info($"Connected to: {Identifier}");
        }
    }
}