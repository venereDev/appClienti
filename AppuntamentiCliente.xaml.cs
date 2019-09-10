using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Venere.Forms.Entity;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Venere.Forms
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppuntamentiCliente : ContentPage, INotifyPropertyChanged
    {
        public AppuntamentiCliente(Cliente clienteSelez, List<Appuntamento> listaappuntamentiCliente, List<Operatore> listaoperatori)
        {
            InitializeComponent();
            this.BindingContext = this;

            labelcountAppCliente.Text = "(" + clienteSelez.Nome + " " + clienteSelez.Cognome + ")" + " Appuntamenti fino al " + DateTime.Now.AddYears(+1).ToString("dd/MM/yyyy") + " : " + listaappuntamentiCliente.Count;

            try
            {
                //ORDINE BY ora inizio dell'appuntamento(appuntamenti da oggi fino all'anno prossimo)
                listaappuntamentiCliente = listaappuntamentiCliente.OrderBy(x => x.OrarioInizio.ToString()).ToList();
                //Join tra appuntamenti e lista operatori per estrabolare inOperatore adiacenti
                var joinAppuntamentiOperatori = listaappuntamentiCliente.Join(listaoperatori, la => la.IDDipendente, lc => lc.IdOperatore, (la, lc) => new { la, lc });

                var sorted = from appuntamento in joinAppuntamentiOperatori
                             orderby appuntamento.la.Data
                             group appuntamento by appuntamento.la.DataSort into datagroup
                             select new TrattAdapter<string, dynamic>(datagroup.Key, datagroup);

                //CREAZIONE NUOVA COLLEZIONE DI GRUPPI
                ObservableCollection<TrattAdapter<string, dynamic>> appuntamentigrouped = new ObservableCollection<TrattAdapter<string, dynamic>>(sorted);

                ListViewAppuntamentiCliente.ItemsSource = appuntamentigrouped;
                ListViewAppuntamentiCliente.GroupDisplayBinding = new Binding("Key");
                ListViewAppuntamentiCliente.IsGroupingEnabled = true;

                ListViewAppuntamentiCliente.GroupHeaderTemplate = new DataTemplate(typeof(HeaderCellAppuntamentiCliente));

                switch (Device.RuntimePlatform)
                {
                    case Device.iOS:
                        Padding = new Thickness(5, 20, 5, 5);
                        break;
                }
            }
            catch (Exception ex)
            {
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected override void OnPropertyChanged([CallerMemberName] string benvenutiCentro = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(benvenutiCentro));
        }

        //Disabilita click su riga
        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var list = (ListView)sender;
            list.SelectedItem = null;
        }

        //presonalizzazione celle lista
        public class HeaderCellAppuntamentiCliente : ViewCell
        {
            public HeaderCellAppuntamentiCliente()
            {
                this.Height = 30;

                var title = new Label
                {
                    // Font = Font.SystemFontOfSize(NamedSize.Large, FontAttributes.Bold),
                    TextColor = Color.White,
                    FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                    VerticalOptions = LayoutOptions.Center,
                    Margin = 2
                };

                title.SetBinding(Label.TextProperty, " " + "Key");

                View = new StackLayout
                {
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    HeightRequest = 25,

                    BackgroundColor = Color.FromRgb(159, 24, 151), //colore background header cell
                    Orientation = StackOrientation.Horizontal,
                    Children = { title }
                };
            }
        }
    }
}