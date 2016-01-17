using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using Lextm.SharpSnmpLib;
using RestWebService.SNMPAccessLayer;
using SNMP_Client.AdditionalWindows;

namespace SNMP_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string[] commands =
        {
            "Pokaż tabelę",
            "Pokaż wartość obiektu skalarnego",
            "Zmień wartość obiektu skalarnego"
        };
        private List<string> scalarNames;
        private List<string> scalarChangeNames;
        private List<string> tablesNames;
        private Dictionary<string, string> tables;
        private Dictionary<string, string> scalars;
        private Dictionary<string, string> scalarsChange;

        public MainWindow()
        {
            InitializeComponent();
            initAll();
        }

        private void initAll()
        {
            initDictionaries();
            commandBox.ItemsSource = commands.ToList();
        }

        private void initDictionaries()
        {
            tables = new Dictionary<string, string>() {
                {"ipAddrTable",".1.3.6.1.2.1.4.20" },
                {"ipRouteTable",".1.3.6.1.2.1.4.21" },
                {"ipNetToMediaTable",".1.3.6.1.2.1.4.22" }
            };

            tablesNames = tables.Keys.ToList();

            scalars = new Dictionary<string, string>()
            {
                {"ipForwarding",".1.3.6.1.2.1.4.1.0" },
                {"ipDefaultTTL",".1.3.6.1.2.1.4.2.0"},
                {"ipInReceives",".1.3.6.1.2.1.4.3.0" },
                {"ipInHdrErrors",".1.3.6.1.2.1.4.4.0" },
                {"ipInAddrErrors",".1.3.6.1.2.1.4.5.0" },
                {"ipForwDatagrams",".1.3.6.1.2.1.4.6.0" },
                {"ipInUnknownProtos",".1.3.6.1.2.1.4.7.0" },
                {"ipInDiscards",".1.3.6.1.2.1.4.8.0" },
                {"ipInDelivers",".1.3.6.1.2.1.4.9.0" },
                {"ipOutRequests",".1.3.6.1.2.1.4.10.0" },
                {"ipOutDiscards",".1.3.6.1.2.1.4.11.0" },
                {"ipOutNoRoutes",".1.3.6.1.2.1.4.12.0" },
                {"ipReasmTimeout",".1.3.6.1.2.1.4.13.0" },
                {"ipReasmReqds",".1.3.6.1.2.1.4.14.0" },
                {"ipReasmOKs",".1.3.6.1.2.1.4.15.0" },
                {"ipReasmFails",".1.3.6.1.2.1.4.16.0" },
                {"ipFragOKs",".1.3.6.1.2.1.4.17.0" },
                {"ipFragFails",".1.3.6.1.2.1.4.18.0" },
                {"ipFragCreates",".1.3.6.1.2.1.4.19.0" },
                {"ipRoutingDiscards",".1.3.6.1.2.1.4.23.0" }
            };
            scalarNames = scalars.Keys.ToList();

            scalarsChange = new Dictionary<string, string>()
            {
                {"ipForwarding",".1.3.6.1.2.1.4.1.0" },
                {"ipDefaultTTL",".1.3.6.1.2.1.4.2.0"}
            };
            scalarChangeNames = scalarsChange.Keys.ToList();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            console.Items.Clear();
        }
        private void addItemToConsole(string value)
        {
            console.Items.Add(value);
        }

        private void commandBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selected = commandBox.SelectedIndex;

            switch (selected)
            {
                case 0:
                    selectBox.ItemsSource = tablesNames;
                    break;
                case 1:
                    selectBox.ItemsSource = scalarNames;
                    break;
                case 2:
                    selectBox.ItemsSource = scalarChangeNames;
                    break;
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            int command = commandBox.SelectedIndex;
            string obiekt;
            int obiektIndex = selectBox.SelectedIndex;
            string adres;
            try
            {
                switch (command)
                {
                    case 0:
                        obiekt = selectBox.SelectedItem.ToString();
                        adres = tables[obiekt];
                        TableView tview = new TableView(GenerateGetTableRequest(adres), obiektIndex);
                        tview.ShowDialog();
                        break;
                    case 1:
                        obiekt = selectBox.SelectedItem.ToString();
                        adres = scalars[obiekt];
                        addItemToConsole(obiekt + ": " + GenerateGetRequest(adres));
                        break;
                    case 2:
                        obiekt = selectBox.SelectedItem.ToString();
                        adres = scalarsChange[obiekt];
                        string value = valueBox.Text;
                        addItemToConsole(obiekt + ": " + GeneratePUTRequest(adres, value));
                        break;
                    default:
                        break;

                }
            }
            catch (Exception ex)
            {
                addItemToConsole(ex.Message);
            }
        }

        private string GeneratePUTRequest(string address, string value)
        {
            string Url = "http://localhost/snmpservice/api?";

            byte[] dataByte = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(new ResultData(address,value)));

            HttpWebRequest PUTRequest = (HttpWebRequest)HttpWebRequest.Create(Url);
            PUTRequest.Method = "PUT";
            PUTRequest.ContentLength = dataByte.Length;

            Stream PUTRequestStream = PUTRequest.GetRequestStream();
            PUTRequestStream.Write(dataByte, 0, dataByte.Length);

            HttpWebResponse PUTResponse = (HttpWebResponse)PUTRequest.GetResponse();

            StreamReader PUTResponseStream = new StreamReader(PUTResponse.GetResponseStream(), Encoding.UTF8);
            return PUTResponseStream.ReadToEnd().ToString();
        }

        private String[,] GenerateGetTableRequest(string address)
        {
            try
            {
                string url = "http://localhost/snmpservice/api?table=" + address;

                HttpWebRequest GETRequest = (HttpWebRequest)WebRequest.Create(url);
                GETRequest.Method = "GET";
                HttpWebResponse GETResponse = (HttpWebResponse)GETRequest.GetResponse();
                Stream GETResponseStream = GETResponse.GetResponseStream();
                StreamReader sr = new StreamReader(GETResponseStream);
                return ResultData.ToTableString(JsonConvert.DeserializeObject<ResultData[,]>(sr.ReadToEnd()));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Generates the get request.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        private string GenerateGetRequest(string address)
        {
            StreamReader sr;
            try
            {
                string url = "http://localhost/snmpservice/api?element=" + address;
                HttpWebRequest GETRequest = (HttpWebRequest)WebRequest.Create(url);
                GETRequest.Method = "GET";
                HttpWebResponse GETResponse = (HttpWebResponse)GETRequest.GetResponse();
                Stream GETResponseStream = GETResponse.GetResponseStream();
                sr = new StreamReader(GETResponseStream);
                return ((ResultData)JsonConvert.DeserializeObject<ResultData>(sr.ReadToEnd())).Data;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
  
         private int prepareResult(String result)
        {
            string tmp = result.Split(';')[1];
            string tmp2 = tmp.Split(':')[1];

            return int.Parse(tmp2);
        }
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}