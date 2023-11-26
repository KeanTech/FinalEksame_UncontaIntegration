using Uniconta.ClientTools.Controls;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid.Native;
using System;
using System.Net;
using DevExpress.Xpf.Grid;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using DevExpress.Utils.Serializing;

namespace UnicontaClient.Pages.CustomPage
{
    public abstract class CorasauDataGridClient : CorasauDataGrid
    {
        public CorasauDataGridClient(IDataControlOriginationElement dataControlOriginationElement)
            : base(dataControlOriginationElement)
        {

        }
        public CorasauDataGridClient()
        {

        }
    }
    public class CorasauDataGridForeignKeyColumnClient : CorasauDataGridForeignKeyColumn
    {
    }

    public class CorasauGridLookupEditorClient : CorasauGridLookupEditor
    {
    }
    public class VariantEditorClient : VariantEditor
    {
    }
    public class CorasauDataGridTemplateColumnClient : CorasauDataGridTemplateColumn
    {
    }
    public class ComboBoxColumnClient : ComboboxColumn
    {
    }
    public class SearchLookUpStyleSettingsClient : SearchLookUpStyleSettings
    {
    }
    public class SumColumn : GridSummaryItem
    {
        private string serializableTag;
        [XtraSerializableProperty]
        public string SerializableTag
        {
            get { return serializableTag; }
            set { serializableTag = value; }
        }
    }

}
