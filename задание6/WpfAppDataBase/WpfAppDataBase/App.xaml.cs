using System;
using System.Data.SqlClient;
using System.Windows;
// Ссылка на пространство имен библиотеки DatabaseController
using DatabaseController;
namespace WpfApp
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            string connection_str = "Server=MANGUST17;Database=praktika;Trusted_Connection=True;";
            Database.GetInstance(connection_str);
        }
    }
}
