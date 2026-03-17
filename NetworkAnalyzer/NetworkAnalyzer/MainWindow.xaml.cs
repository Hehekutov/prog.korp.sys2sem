using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace NetworkAnalyzer
{
    public partial class MainWindow : Window
    {
        private HashSet<string> urlHistory = new HashSet<string>();

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
                sb.AppendLine($"IP: {addr.Address} ({GetIpType(addr.Address)})");
            }

            ResultBox.Text = sb.ToString();
        }

        // Анализ URL
        private void Analyze_Click(object sender, RoutedEventArgs e)
        {
            string url = UrlHistoryBox.Text.Trim();
            if (string.IsNullOrEmpty(url))
                return;

            try
            {
                Uri uri = new Uri(url);

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
                    sb.AppendLine($"{ip} ({GetIpType(ip)})");
                }

                ResultBox.Text = sb.ToString();

                // Сохраняем историю URL
                if (!urlHistory.Contains(url))
                {
                    urlHistory.Add(url);
                    UrlHistoryBox.Items.Add(url);
                }
            }
            catch (Exception ex)
            {
                ResultBox.Text = "Ошибка: " + ex.Message;
            }
        }

        // Определение типа IP
        private string GetIpType(IPAddress ip)
        {
            if (IPAddress.IsLoopback(ip))
                return "Loopback";

            byte[] bytes = ip.GetAddressBytes();

            // IPv4 private ranges
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                if (bytes[0] == 10 ||
                    (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
                    (bytes[0] == 192 && bytes[1] == 168))
                    return "Локальный";
                else
                    return "Публичный";
            }

            // IPv6 (упрощенно)
            if (ip.AddressFamily == AddressFamily.InterNetworkV6)
            {
                if (ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal)
                    return "Локальный";
                else
                    return "Публичный";
            }

            return "Неизвестный";
        }
    }
}