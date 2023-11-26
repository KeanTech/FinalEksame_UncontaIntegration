using Alaska.Library.Core.Factories;

namespace Alaska.Library.Models
{
    public class LogInfoLine : IEntity
    {
        public string Item { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; }
        public bool Validated { get; set; }
        public double OnStock { get; set; }
    }
}
