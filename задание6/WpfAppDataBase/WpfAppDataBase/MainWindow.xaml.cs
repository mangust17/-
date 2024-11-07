using System;
using System.Data.SqlClient;
using System.Windows;
// Подключить пространство имен библиотеки DatabaseController.
using DatabaseController;
namespace WpfApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Database db;
        public MainWindow()
        {
            string connection_str = "Server=MANGUST17;Database=praktika;Trusted_Connection=True;";
            InitializeComponent();
            db = Database.GetInstance(connection_str); // Получаем уже инициализированный экземпляр
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string login = login_tb.Text;
            string password = password_tb.Text;

            string query = "SELECT COUNT(*) FROM Test WHERE login = @login AND password = @password";
            Parameter[] par =
            {
        new Parameter("@login", login),
        new Parameter("@password", password),
    };

        object result = db.GetScalar(query, par);
            if ((int)result > 0)
            {
                MessageBox.Show("Авторизация прошла успешно");
            }
            else
            {
                MessageBox.Show("Неверные логин или пароль");
            }
        }
    }
}