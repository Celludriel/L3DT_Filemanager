using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3dtFileManager
{
    public class Epsilon
    {
        private double _value;
        
        public Epsilon(double value) 
        { 
            _value = value; 
        }
        
        internal bool IsEqual(double a, double b) 
        { 
            return (a == b) || (Math.Abs(a - b) < _value); 
        }
        
        internal bool IsNotEqual(double a, double b) 
        { 
            return (a != b) && !(Math.Abs(a - b) < _value); 
        }
    }
}
