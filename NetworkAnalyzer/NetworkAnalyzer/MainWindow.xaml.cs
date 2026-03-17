using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows;

namespace NetworkAnalyzer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadInterfaces();
        }

        // Загрузка интерфейсов
        private void LoadInterfaces()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var ni in interfaces)
            {
                InterfacesList.Items.Add(ni.Name);
            }
        }

        // При выборе интерфейса
        private void InterfacesList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            var ni = interfaces[InterfacesList.SelectedIndex];

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Имя: " + ni.Name);
            sb.AppendLine("Тип: " + ni.NetworkInterfaceType);
            sb.AppendLine("Скорость: " + ni.Speed);
            sb.AppendLine("Статус: " + ni.OperationalStatus);
            sb.AppendLine("MAC: " + ni.GetPhysicalAddress());

            var ipProps = ni.GetIPProperties();

            foreach (var addr in ipProps.UnicastAddresses)
            {
                sb.AppendLine("IP: " + addr.Address);
            }

            ResultBox.Text = sb.ToString();
        }

        // Анализ URL
        private void Analyze_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Uri uri = new Uri(UrlBox.Text);

                StringBuilder sb = new StringBuilder();

                sb.AppendLine("Схема: " + uri.Scheme);
                sb.AppendLine("Хост: " + uri.Host);
                sb.AppendLine("Порт: " + uri.Port);
                sb.AppendLine("Путь: " + uri.AbsolutePath);
                sb.AppendLine("Параметры: " + uri.Query);
                sb.AppendLine("Фрагмент: " + uri.Fragment);

                // Ping
                Ping ping = new Ping();
                var reply = ping.Send(uri.Host);

                sb.AppendLine("Ping: " + reply.Status);

                // DNS
                var host = Dns.GetHostEntry(uri.Host);
                sb.AppendLine("DNS IP:");
                foreach (var ip in host.AddressList)
                {
                    sb.AppendLine(ip.ToString());
                }

                ResultBox.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                ResultBox.Text = "Ошибка: " + ex.Message;
            }
        }
    }
}