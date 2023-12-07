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
    /// <summary>
    /// This class represents db model in Uniconta
    /// </summary>
    public class ScannerFile : TableDataWithKey
    {
        public override int UserTableId { get { return 81790; } }
        public override int CompanyId { get { return 80730; } }
        
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

        [Display(Name = "Status")]
        public string Status
        {
            get { return this.GetUserFieldString("Status"); }
            set { this.SetUserFieldString("Status", value); NotifyPropertyChanged("Status"); }
        }
    }
}
