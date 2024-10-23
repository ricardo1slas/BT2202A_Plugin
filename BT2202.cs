using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using OpenTap;

namespace BT2202a
{
    [Display("BT2202")]
    public class BT2202 : Instrument
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion
        public BT2202()
        {
            Name = "BT2202";
            // ToDo: Set default values for properties / settings.
        }

        public override void Open()
        {
            base.Open();
            // TODO:  Open the connection to the instrument here

            //if (!IdnString.Contains("Instrument ID"))
            //{
            //    Log.Error("This instrument driver does not support the connected instrument.");
            //    throw new ArgumentException("Wrong instrument type.");
            // }
        }

        public override void Close()
        {
            // TODO:  Shut down the connection to the instrument here.
            base.Close();
        }
    }
}
