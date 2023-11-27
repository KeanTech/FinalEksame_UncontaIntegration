using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uniconta.DataModel;

namespace Alaska.Library.Models.Uniconta.Userdefined
{
    public class ScannerFile : TableDataWithKey
    {
        public override int UserTableId { get { return 81790; } }
        public override int CompanyId { get { return 80730; } }

        public static ScannerFile Factory(Company company, string name, string filePath, string production, string status) 
        {
            ScannerFile scannerFile = new ScannerFile();
            scannerFile.SetMaster(company);
            scannerFile.KeyName = name;
            scannerFile.FilePath = filePath;
            scannerFile.Created = DateTime.Now;
            scannerFile.Production = production;
            scannerFile.Status = status;
            return scannerFile;
        }

        [Display(Name = "Filsti")]
        public string FilePath
        {
            get { return this.GetUserFieldString("FilePath"); }
            set { this.SetUserFieldString("FilePath", value); NotifyPropertyChanged("FilePath"); }
        }

        [Display(Name = "Oprettet")]
        public DateTime Created
        {
            get { return this.GetUserFieldDateTime("Created"); }
            set { this.SetUserFieldDateTime("Created", value); NotifyPropertyChanged("Created"); }
        }

        [Display(Name = "@Production")]
        public string Production
        {
            get { return this.GetUserFieldString("Production"); }
            set { this.SetUserFieldString("Production", value); NotifyPropertyChanged("Production"); }
        }

        [Display(Name = "Status")]
        public string Status
        {
            get { return this.GetUserFieldString("Status"); }
            set { this.SetUserFieldString("Status", value); NotifyPropertyChanged("Status"); }
        }
    }
}
