using Alaska.Library.Core.Factories;
using System;
using System.Collections.Generic;

namespace Alaska.Library.Models
{
    public class LogInfo : IEntity
    {
        public DateTime Date { get; set; }
        public string FileName { get; set; }
        public List<LogInfoLine> LogInfoLines { get; set; }
    }
}
