using System.ComponentModel.DataAnnotations;
using Uniconta.ClientTools.DataModel;

namespace Alaska.Library.Models.Uniconta.Inventory
{
    /// <summary>
    /// This class is used the get the userdefined fields 
    /// </summary>
    public class InvItemClientUser : InvItemClient
    {
        [Display(Name = "Kolli pr.lag")]
        public string PalleAntal
        {
            get { return this.GetUserFieldString("PalleAntal"); }
            set { this.SetUserFieldString("PalleAntal", value); NotifyPropertyChanged("PalleAntal"); }
        }

        [Display(Name = "Nettovægt")]
        public string Nettovaegt
        {
            get { return this.GetUserFieldString("Nettovaegt"); }
            set { this.SetUserFieldString("Nettovaegt", value); NotifyPropertyChanged("Nettovaegt"); }
        }

        [Display(Name = "Kolli pr.palle")]
        public string kolliprpalle
        {
            get { return this.GetUserFieldString("kolliprpalle"); }
            set { this.SetUserFieldString("kolliprpalle", value); NotifyPropertyChanged("kolliprpalle"); }
        }

        [Display(Name = "Lag pr. palle")]
        public string Lagprpalle
        {
            get { return this.GetUserFieldString("Lagprpalle"); }
            set { this.SetUserFieldString("Lagprpalle", value); NotifyPropertyChanged("Lagprpalle"); }
        }

        [Display(Name = "Dunsbjergvej Loknr.1")]
        public string Loknr1
        {
            get { return this.GetUserFieldString("Loknr1"); }
            set { this.SetUserFieldString("Loknr1", value); NotifyPropertyChanged("Loknr1"); }
        }

        [Display(Name = "Dunsbjergvej Loknr.2")]
        public string Loknr2
        {
            get { return this.GetUserFieldString("Loknr2"); }
            set { this.SetUserFieldString("Loknr2", value); NotifyPropertyChanged("Loknr2"); }
        }

        [Display(Name = "Dunsbjergvej Loknr.3")]
        public string Loknr3
        {
            get { return this.GetUserFieldString("Loknr3"); }
            set { this.SetUserFieldString("Loknr3", value); NotifyPropertyChanged("Loknr3"); }
        }

        [Display(Name = "Løkkebyvej Loknr.1")]
        public string llokkebyvej1
        {
            get { return this.GetUserFieldString("llokkebyvej1"); }
            set { this.SetUserFieldString("llokkebyvej1", value); NotifyPropertyChanged("llokkebyvej1"); }
        }

        [Display(Name = "Løkkebyvej Loknr.2")]
        public string llokkebyvej2
        {
            get { return this.GetUserFieldString("llokkebyvej2"); }
            set { this.SetUserFieldString("llokkebyvej2", value); NotifyPropertyChanged("llokkebyvej2"); }
        }

        [Display(Name = "Løkkebyvej Loknr.3")]
        public string llokkebyvej3
        {
            get { return this.GetUserFieldString("llokkebyvej3"); }
            set { this.SetUserFieldString("llokkebyvej3", value); NotifyPropertyChanged("llokkebyvej3"); }
        }

        [Display(Name = "ADR")]
        public bool Ard
        {
            get { return this.GetUserFieldBoolean("Ard"); }
            set { this.SetUserFieldBoolean("Ard", value); NotifyPropertyChanged("Ard"); }
        }

        [Display(Name = "IMDG")]
        public bool IMDG
        {
            get { return this.GetUserFieldBoolean("IMDG"); }
            set { this.SetUserFieldBoolean("IMDG", value); NotifyPropertyChanged("IMDG"); }
        }

        [Display(Name = "Navn")]
        public string NavnARDIMDG
        {
            get { return this.GetUserFieldString("NavnARDIMDG"); }
            set { this.SetUserFieldString("NavnARDIMDG", value); NotifyPropertyChanged("NavnARDIMDG"); }
        }

        [Display(Name = "NOS Navn")]
        public string NOSNavn
        {
            get { return this.GetUserFieldString("NOSNavn"); }
            set { this.SetUserFieldString("NOSNavn", value); NotifyPropertyChanged("NOSNavn"); }
        }

        [Display(Name = "Klasse")]
        public string Klasse
        {
            get { return this.GetUserFieldString("Klasse"); }
            set { this.SetUserFieldString("Klasse", value); NotifyPropertyChanged("Klasse"); }
        }

        [Display(Name = "EMB gr")]
        public string EMBgr
        {
            get { return this.GetUserFieldString("EMBgr"); }
            set { this.SetUserFieldString("EMBgr", value); NotifyPropertyChanged("EMBgr"); }
        }

        [Display(Name = "Flashpoint")]
        public string Flashpoint
        {
            get { return this.GetUserFieldString("Flashpoint"); }
            set { this.SetUserFieldString("Flashpoint", value); NotifyPropertyChanged("Flashpoint"); }
        }

        [Display(Name = "LQ")]
        public bool LQ
        {
            get { return this.GetUserFieldBoolean("LQ"); }
            set { this.SetUserFieldBoolean("LQ", value); NotifyPropertyChanged("LQ"); }
        }

        [Display(Name = "EANNUMMER")]
        public string EANNUMMER
        {
            get { return this.GetUserFieldString("EANNUMMER"); }
            set { this.SetUserFieldString("EANNUMMER", value); NotifyPropertyChanged("EANNUMMER"); }
        }

        [Display(Name = "EAN Detail")]
        public string EANRetail
        {
            get { return this.GetUserFieldString("EANRetail"); }
            set { this.SetUserFieldString("EANRetail", value); NotifyPropertyChanged("EANRetail"); }
        }

        [Display(Name = "UFI")]
        public string UFI
        {
            get { return this.GetUserFieldString("UFI"); }
            set { this.SetUserFieldString("UFI", value); NotifyPropertyChanged("UFI"); }
        }

        [Display(Name = "UNnr.")]
        public string UNnr
        {
            get { return this.GetUserFieldString("UNnr"); }
            set { this.SetUserFieldString("UNnr", value); NotifyPropertyChanged("UNnr"); }
        }

        [Display(Name = "DB Nummer")]
        public string DBNummer
        {
            get { return this.GetUserFieldString("DBNummer"); }
            set { this.SetUserFieldString("DBNummer", value); NotifyPropertyChanged("DBNummer"); }
        }

        [Display(Name = "Bruttovægt")]
        public string Bruttovaegt
        {
            get { return this.GetUserFieldString("Bruttovaegt"); }
            set { this.SetUserFieldString("Bruttovaegt", value); NotifyPropertyChanged("Bruttovaegt"); }
        }

        [Display(Name = "Rumfang")]
        public string Rumfang
        {
            get { return this.GetUserFieldString("Rumfang"); }
            set { this.SetUserFieldString("Rumfang", value); NotifyPropertyChanged("Rumfang"); }
        }

        [Display(Name = "Produkt dim.        Højden")]
        public string ProduktdimHjoeden
        {
            get { return this.GetUserFieldString("ProduktdimHjoeden"); }
            set { this.SetUserFieldString("ProduktdimHjoeden", value); NotifyPropertyChanged("ProduktdimHjoeden"); }
        }

        [Display(Name = "Kolli dim. ( LxBxH )")]
        public string Kolidim
        {
            get { return this.GetUserFieldString("Kolidim"); }
            set { this.SetUserFieldString("Kolidim", value); NotifyPropertyChanged("Kolidim"); }
        }

        [Display(Name = "Styk Kolli")]
        public string StykKolli
        {
            get { return this.GetUserFieldString("StykKolli"); }
            set { this.SetUserFieldString("StykKolli", value); NotifyPropertyChanged("StykKolli"); }
        }

        [Display(Name = "Varenavn 2")]
        public string Varenavn2
        {
            get { return this.GetUserFieldString("Varenavn2"); }
            set { this.SetUserFieldString("Varenavn2", value); NotifyPropertyChanged("Varenavn2"); }
        }

        [Display(Name = "Varenavn 3")]
        public string Varenavn3
        {
            get { return this.GetUserFieldString("Varenavn3"); }
            set { this.SetUserFieldString("Varenavn3", value); NotifyPropertyChanged("Varenavn3"); }
        }

        [Display(Name = "Lev. varenr")]
        public string Levvarenr
        {
            get { return this.GetUserFieldString("Levvarenr"); }
            set { this.SetUserFieldString("Levvarenr", value); NotifyPropertyChanged("Levvarenr"); }
        }

        [Display(Name = "Toldpos.")]
        public string Toldpos
        {
            get { return this.GetUserFieldString("Toldpos"); }
            set { this.SetUserFieldString("Toldpos", value); NotifyPropertyChanged("Toldpos"); }
        }

        [Display(Name = "Bredden")]
        public string Bredden
        {
            get { return this.GetUserFieldString("Bredden"); }
            set { this.SetUserFieldString("Bredden", value); NotifyPropertyChanged("Bredden"); }
        }

        [Display(Name = "Dybden")]
        public string Dybden
        {
            get { return this.GetUserFieldString("Dybden"); }
            set { this.SetUserFieldString("Dybden", value); NotifyPropertyChanged("Dybden"); }
        }

        [Display(Name = "Højden")]
        public string Hojde
        {
            get { return this.GetUserFieldString("Hojde"); }
            set { this.SetUserFieldString("Hojde", value); NotifyPropertyChanged("Hojde"); }
        }

        [Display(Name = "Højden")]
        public string HjdenKolli
        {
            get { return this.GetUserFieldString("HjdenKolli"); }
            set { this.SetUserFieldString("HjdenKolli", value); NotifyPropertyChanged("HjdenKolli"); }
        }

        [Display(Name = "Bredden")]
        public string Breddenkolli
        {
            get { return this.GetUserFieldString("Breddenkolli"); }
            set { this.SetUserFieldString("Breddenkolli", value); NotifyPropertyChanged("Breddenkolli"); }
        }

        [Display(Name = "Dybden")]
        public string Dybdenkolli
        {
            get { return this.GetUserFieldString("Dybdenkolli"); }
            set { this.SetUserFieldString("Dybdenkolli", value); NotifyPropertyChanged("Dybdenkolli"); }
        }

        [Display(Name = "Palle EAN nr.")]
        public string EANPallet
        {
            get { return this.GetUserFieldString("EANPallet"); }
            set { this.SetUserFieldString("EANPallet", value); NotifyPropertyChanged("EANPallet"); }
        }

        [Display(Name = "Antal pr. palle")]
        public double EANPalletQty
        {
            get { return this.GetUserFieldDouble("EANPalletQty"); }
            set { this.SetUserFieldDouble("EANPalletQty", value); NotifyPropertyChanged("EANPalletQty"); }
        }

        [Display(Name = "EAN DUMMY")]
        public double DummyEAN
        {
            get { return this.GetUserFieldDouble("DummyEAN"); }
            set { this.SetUserFieldDouble("DummyEAN", value); NotifyPropertyChanged("DummyEAN"); }
        }

    }
}
