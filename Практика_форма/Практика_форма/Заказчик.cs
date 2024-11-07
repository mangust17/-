using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Практика_форма
{
    public partial class Заказчик : Form
    {
        private int userId;
        private string connectionString = "Server=MANGUST17;Database=praktika;Trusted_Connection=True;";

        public Заказчик(int user_id)
        {
            userId = user_id;
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT r.requestID, r.problemDescription, r.requestStatus, r.homeTechID, h.techType, h.techModel 
                                     FROM Requests r
                                     JOIN homeTech h ON r.homeTechID = h.homeTechID
                                     WHERE r.clientID = @userId";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@userId", userId);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    dataGridView1.DataSource = table;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void searchBtn_Click(object sender, EventArgs e)
        {
            string searchTerm = textBox1.Text; // Поле для ввода поиска
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT r.requestID, r.problemDescription, r.requestStatus, r.homeTechID, h.techType, h.techModel 
                                     FROM Requests r
                                     JOIN homeTech h ON r.homeTechID = h.homeTechID
                                     WHERE r.clientID = @userId AND r.problemDescription LIKE '%' + @searchTerm + '%'";

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
            }
        }

        private void Addbtn_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // Вставка данных в Requests и HomeTech
                    string query = @"INSERT INTO Requests (problemDescription, requestStatus, clientID, homeTechID, startDate) 
                                     VALUES (@problemDescription, 'Новая заявка', @userId, @techID, GETDATE()); 
                                     INSERT INTO HomeTech (techType, techModel) 
                                     VALUES (@type, @model)";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@problemDescription", textBox3.Text);
                    cmd.Parameters.AddWithValue("@techID", textBox4.Text);
                    cmd.Parameters.AddWithValue("@type", textBox6.Text);
                    cmd.Parameters.AddWithValue("@model", textBox5.Text);
                    cmd.Parameters.AddWithValue("@userId", userId);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Новая заявка добавлена.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при добавлении заявки: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    LoadData(); // Обновление данных после добавления
                }
            }
        }

        private void changeBtn_Click(object sender, EventArgs e)
        {
            int requestId = Convert.ToInt32(textBox2.Text);
            string newProblemDescription = textBox3.Text;
            int newClientID = userId;
            int newHomeTechID = Convert.ToInt32(textBox4.Text);
            string newTechType = textBox6.Text;
            string newTechModel = textBox5.Text;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Update the Requests table
                    string updateRequestQuery = @"UPDATE Requests 
                                                  SET problemDescription = @problemDescription,
                                                      clientID = @clientId,
                                                      homeTechID = @homeTechId
                                                  WHERE requestId = @requestId 
                                                  AND requestStatus = 'Новая заявка' 
                                                  AND clientID = @userId";

                    SqlCommand updateRequestCmd = new SqlCommand(updateRequestQuery, conn);
                    updateRequestCmd.Parameters.AddWithValue("@problemDescription", newProblemDescription);
                    updateRequestCmd.Parameters.AddWithValue("@clientId", newClientID);
                    updateRequestCmd.Parameters.AddWithValue("@homeTechId", newHomeTechID);
                    updateRequestCmd.Parameters.AddWithValue("@requestId", requestId);
                    updateRequestCmd.Parameters.AddWithValue("@userId", userId);

                    int requestRowsAffected = updateRequestCmd.ExecuteNonQuery();

                    // Update the HomeTech table
                    string updateHomeTechQuery = @"UPDATE HomeTech 
                                                    SET techType = @techType,
                                                        techModel = @techModel
                                                    WHERE homeTechID = @homeTechId";

                    SqlCommand updateHomeTechCmd = new SqlCommand(updateHomeTechQuery, conn);
                    updateHomeTechCmd.Parameters.AddWithValue("@techType", newTechType);
                    updateHomeTechCmd.Parameters.AddWithValue("@techModel", newTechModel);
                    updateHomeTechCmd.Parameters.AddWithValue("@homeTechId", newHomeTechID);

                    int homeTechRowsAffected = updateHomeTechCmd.ExecuteNonQuery();

                    if (requestRowsAffected > 0 && homeTechRowsAffected > 0)
                    {
                        MessageBox.Show("Заявка и данные техники обновлены.","Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Изменение недоступно. Проверьте статус заявки.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при изменении данных: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    LoadData(); // Обновление данных после изменения
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Профиль profile = new Профиль(userId);
            profile.ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            Авторизация av = new Авторизация();
            av.ShowDialog();
        }
    }
}
