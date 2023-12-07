using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uniconta.Common;
using Uniconta.DataModel;

namespace Alaska.Library.Models.Uniconta.Userdefined
{
    /// <summary>
    /// This class represents a db model
    /// </summary>
    public class ScannerData : TableData
    {
        public override int UserTableId { get { return 81791; } }
        public override int CompanyId { get { return 80730; } }
        public override int MasterTableId { get { return 81790; } }

        [Display(Name = "Vare nummer")]
        public string ItemNumber
        {
            get { return this.GetUserFieldString("ItemNumber"); }
            set { this.SetUserFieldString("ItemNumber", value); NotifyPropertyChanged("ItemNumber"); }
        }

        [Display(Name = "Antal")]
        public long Quantity
        {
            get { return this.GetUserFieldInt64("Quantity"); }
            set { this.SetUserFieldInt64("Quantity", value); NotifyPropertyChanged("Quantity"); }
        }

        [Display(Name = "Dato for oprettelse")]
        public DateTime Date
        {
            get { return this.GetUserFieldDateTime("Date"); }
            set { this.SetUserFieldDateTime("Date", value); NotifyPropertyChanged("Date"); }
        }

        [Display(Name = "Status")]
        public string Status
        {
            get { return this.GetUserFieldString("Status"); }
            set { this.SetUserFieldString("Status", value); NotifyPropertyChanged("Status"); }
        }

        [Display(Name = "Valideret")]
        public bool Validated
        {
            get { return this.GetUserFieldBoolean("Validated"); }
            set { this.SetUserFieldBoolean("Validated", value); NotifyPropertyChanged("Validated"); }
        }

        [ForeignKeyAttribute(ForeignKeyTable = typeof(ProductionOrder))]
        [Display(Name = "Produktionsnummer")]
        public string ProductionNumber
        {
            get { return this.GetUserFieldString("ProductionNumber"); }
            set { this.SetUserFieldString("ProductionNumber", value); NotifyPropertyChanged("ProductionNumber"); }
        }

    }
}
