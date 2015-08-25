using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Correct
{
    public class Record
    {       
        Int16 frequency;
        byte gear;
        float actualvalue1,ad1,actualvalue2,ad2 = 0;
        float calibrationParameter = 0;
        float offset = 0;
        float deviation = 0;

        public Record(Int16 frequency,byte gear, float actualvalue1, float ad1, float calibrationParameter, float offset, float deviation)
        {         
            this.frequency = frequency;
            this.gear = gear;
            this.actualvalue1 = actualvalue1;
            this.ad1 = ad1;
            
            this.calibrationParameter = calibrationParameter;
            this.offset = offset;
            this.deviation = deviation;                        
        }

        public Record()
        { 
        }

        public Int16 Frequency 
        { 
            get { return frequency; } 
            set { frequency = value; } 
        }
        public byte Gear
        {
            get { return gear; }
            set { gear = value; }
        }

        public float Deviation
        {
            get { return deviation; }
            set { deviation = value; }
        }
       
       
        public float ActualValue1
        {
            get { return actualvalue1; }
            set { actualvalue1 = value; }
        }
        public float Ad1
        {
            get { return ad1; }
            set { ad1 = value; }
        }
        public float ActualValue2
        {
            get { return actualvalue2; }
            set { actualvalue2 = value; }
        }
        public float Ad2
        {
            get { return ad2; }
            set { ad2 = value; }
        }
        public float CalibrationParameter
        {
            get { return calibrationParameter; }
            set { calibrationParameter = value; }
        }
        public float  Offset
        {
            get { return offset; }
            set { offset = value; }
        }
        //HalfFloat.ToInt16(-100.1258f); 
    }
}
