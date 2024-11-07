using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;

namespace DatabaseController
{
    public class Database
    {
        private static Database Instance;
        private string sqlConnectionString;

        private Database() { }

        public static Database GetInstance(string sqlconnectionString = null)
        {
            if (Instance == null)
            {
                Instance = new Database();
            }

            if (sqlconnectionString != null)
            {
                Instance.sqlConnectionString = sqlconnectionString;

                // Пробуем установить и закрыть соединение сразу для проверки
                try
                {
                    using (SqlConnection connection = new SqlConnection(Instance.sqlConnectionString))
                    {
                        connection.Open(); // Пытаемся открыть соединение
                        connection.Close(); // Закрываем, если удалось подключиться
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка подключения к БД: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    throw; // Пробрасываем исключение, если не удалось подключиться
                }
            }

            return Instance;
        }

        private delegate object Handler(SqlCommand cmd);

        private void ExecuteCommand(Handler handler, string query, out object result, Parameter[] parameters = null)
        {
            result = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            foreach (Parameter param in parameters)
                            {
                                cmd.Parameters.AddWithValue(param.Name, param.Value);
                            }
                        }
                        if (handler != null) result = handler.Invoke(cmd);
                    }
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку
                Console.WriteLine(ex.Message);
            }
        }

        public int Execute(string query, Parameter[] parameters = null)
        {
            Handler handler = cmd => cmd.ExecuteNonQuery();
            ExecuteCommand(handler, query, out object result, parameters);
            return result != null ? (int)result : 0;
        }

        public object GetScalar(string query, Parameter[] parameters = null)
        {
            Handler handler = cmd => cmd.ExecuteScalar();
            ExecuteCommand(handler, query, out object result, parameters);
            return result;
        }

        public object[][] GetRowsData(string query, Parameter[] parameters = null)
        {
            Handler handler = cmd =>
            {
                var list = new List<object[]>();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        object[] values = new object[dr.FieldCount];
                        dr.GetValues(values);
                        list.Add(values);
                    }
                }
                return list.ToArray();
            };
            ExecuteCommand(handler, query, out object result, parameters);
            return (object[][])result;
        }
    }

    public class Parameter
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public Parameter(string name, object value)
        {
            name = name.Replace("@", "");
            Name = $"@{name}";
            Value = value.ToString();
        }
    }
}
