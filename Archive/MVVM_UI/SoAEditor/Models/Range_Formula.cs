﻿using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoAEditor.Models
{
    public class Range_Formula : PropertyChangedBase
    {
        public Range_Formula(string formulaVariable, string formulaValue)
        {
            FormulaVariable = formulaVariable;
            FormulaValue = formulaValue;
        }

        public Range_Formula()
        {
        }

        private string _FormulaVariable;

        public string FormulaVariable
        {
            get { return _FormulaVariable; }
            set { _FormulaVariable = value; NotifyOfPropertyChange(() => FormulaVariable); }
        }


        private string _FormulaValue;

        public string FormulaValue
        {
            get { return _FormulaValue; }
            set { _FormulaValue = value; NotifyOfPropertyChange(() => FormulaValue); }
        }
    }
}
