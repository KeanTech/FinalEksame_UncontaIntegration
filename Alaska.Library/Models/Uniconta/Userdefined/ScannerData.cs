using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uniconta.DataModel;

namespace Alaska.Library.Models.Uniconta.Userdefined
{
    public class ScannerData : TableData
    {
        public override int UserTableId { get { return 81791; } }
        public override int CompanyId { get { return 80730; } }
        public override int MasterTableId { get { return 81790; } }

        public ScannerData Factory(ScannerFile scannerFile) 
        {
            ScannerData data = new ScannerData();
            data.SetMaster(scannerFile);

            return data;
        }

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
        public string Validated
        {
            get { return this.GetUserFieldString("Validated"); }
            set { this.SetUserFieldString("Validated", value); NotifyPropertyChanged("Validated"); }
        }

    }
}
