using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Практика_форма
{
    public partial class Оператор : Form
    {
        private int userId;

        public Оператор(int user_id)
        {
            InitializeComponent();
            this.userId = user_id;
            LoadMasters();
            LoadAllRequests();
        }

        // Load all requests
        private void LoadAllRequests()
        {
            using (SqlConnection conn = new SqlConnection("Server=MANGUST17;Database=praktika;Trusted_Connection=True;"))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT * FROM Requests";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    dataGridView1.DataSource = table;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
                }
            }
        }

        // Load master names and IDs into comboBox1
        private void LoadMasters()
        {
            comboBox1.Items.Add(new KeyValuePair<string, int>("Мурашов Андрей Юрьевич", 2));
            comboBox1.Items.Add(new KeyValuePair<string, int>("Степанов Андрей Викторович", 3));
            comboBox1.Items.Add(new KeyValuePair<string, int>("Семенова Ясмина Марковна", 6));
            comboBox1.Items.Add(new KeyValuePair<string, int>("Иванов Марк Максимович", 10));
            comboBox1.DisplayMember = "Key";
            comboBox1.ValueMember = "Value";
        }

        // Filter requests by status "Не зарегистрирована"
        private void button1_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection("Server=MANGUST17;Database=praktika;Trusted_Connection=True;"))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT * FROM Requests WHERE requestStatus = 'Не зарегистрирована'";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    dataGridView1.DataSource = table;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при фильтрации данных: " + ex.Message);
                }
            }
        }

        // Delete request by ID
        private void button4_Click(object sender, EventArgs e)
        {
            int requestId;
            if (int.TryParse(textBox2.Text, out requestId))
            {
                using (SqlConnection conn = new SqlConnection("Server=MANGUST17;Database=praktika;Trusted_Connection=True;"))
                {
                    try
                    {
                        conn.Open();
                        string query = "DELETE FROM Requests WHERE requestID = @requestId";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@requestId", requestId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Заявка успешно удалена.");
                            LoadAllRequests();
                        }
                        else
                        {
                            MessageBox.Show("Заявка с указанным ID не найдена.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при удалении заявки: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Введите корректный ID заявки.");
            }
        }

        // Update request status to "Новая заявка"
        private void button2_Click(object sender, EventArgs e)
        {
            int requestId;
            if (int.TryParse(textBox2.Text, out requestId))
            {
                using (SqlConnection conn = new SqlConnection("Server=MANGUST17;Database=praktika;Trusted_Connection=True;"))
                {
                    try
                    {
                        conn.Open();
                        string query = "UPDATE Requests SET requestStatus = 'Новая заявка' WHERE requestID = @requestId";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@requestId", requestId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Статус заявки успешно обновлен.");
                            LoadAllRequests();
                        }
                        else
                        {
                            MessageBox.Show("Заявка с указанным ID не найдена.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при обновлении статуса заявки: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Введите корректный ID заявки.");
            }
        }

        // Assign master to request by ID
        private void button6_Click(object sender, EventArgs e)
        {
            int requestId;
            if (int.TryParse(textBox3.Text, out requestId) && comboBox1.SelectedItem is KeyValuePair<string, int> selectedMaster)
            {
                int masterId = selectedMaster.Value;
                using (SqlConnection conn = new SqlConnection("Server=MANGUST17;Database=praktika;Trusted_Connection=True;"))
                {
                    try
                    {
                        conn.Open();
                        string query = "UPDATE Requests SET masterID = @masterId WHERE requestID = @requestId";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@requestId", requestId);
                        cmd.Parameters.AddWithValue("@masterId", masterId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Мастер успешно назначен.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadAllRequests();
                        }
                        else
                        {
                            MessageBox.Show("Заявка с указанным ID не найдена.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при назначении мастера: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Введите корректный ID заявки и выберите мастера.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void profileBtn_Click(object sender, EventArgs e)
        {
            this.Hide();
            Профиль profile = new Профиль(userId);
            profile.ShowDialog();

        }

        private void searchBtn_Click(object sender, EventArgs e)
        {
            string searchTerm = textBox1.Text; // Поле для ввода поиска
            using (SqlConnection conn = new SqlConnection("Server=MANGUST17;Database=praktika;Trusted_Connection=True;"))
                try
                {
                    conn.Open();
                    string query = @"SELECT *
                                 FROM Requests 
                                 WHERE clientID = @userId AND problemDescription LIKE '%' + @searchTerm + '%'";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@searchTerm", searchTerm);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    dataGridView1.DataSource = table;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при поиске данных: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    conn.Close();
                }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Hide();
            Авторизация av = new Авторизация();
            av.ShowDialog();
        }
    }
}
