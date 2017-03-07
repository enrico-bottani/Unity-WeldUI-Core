﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityWeld.Binding.Adapters
{
    /// <summary>
    /// Adapter that parses a string as a float.
    /// </summary>
    [Adapter(typeof(string), typeof(float))]
    public class StringToFloatAdapter 
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            return float.Parse((string)valueIn);
        }
    }
}
