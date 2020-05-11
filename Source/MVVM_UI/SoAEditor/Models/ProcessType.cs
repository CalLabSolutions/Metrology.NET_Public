using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoAEditor.Models
{
    public class ProcessType
    {
        public string Action;
        public string Taxonomy;
        public List<MeasurementParameter> RequiredParameters = new List<MeasurementParameter>();
        public List<MeasurementParameter> OptionalParameters = new List<MeasurementParameter>();
    }
}
