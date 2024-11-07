using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Практика_форма
{
    public partial class Мастер : Form
    {
        private int userId;
        public Мастер(int user_id)
        {
            InitializeComponent();
            this.userId = user_id;
            LoadMasterData();
        }
        private void LoadMasterData()
        {
            string query = @"
            SELECT R.requestID, R.requestStatus, R.problemDescription, R.completionDate,
                   H.techType, H.techModel,
                   P.part_id, P.part_name, P.quantity, P.ordered_at,
                   C.message AS Comment
            FROM Requests R
            INNER JOIN HomeTech H ON R.homeTechID = H.homeTechID
            LEFT JOIN PartsOrdered P ON R.repairParts = P.part_id
            LEFT JOIN Comments C ON R.requestID = C.requestID AND R.masterID = C.masterID
            WHERE R.masterID = @userId
            ORDER BY R.requestID, P.part_id";

            using (SqlConnection conn = new SqlConnection("Server=MANGUST17;Database=praktika;Trusted_Connection=True;"))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@userId", userId);

                try
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    dataGridView1.DataSource = dataTable;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void profileBtn_Click(object sender, EventArgs e)
        {
            Профиль profile = new Профиль(userId);
            profile.ShowDialog();
        }

        private void changeBtn_Click(object sender, EventArgs e)
        {
            int requestId;
            if (!int.TryParse(textBox2.Text, out requestId))
            {
                MessageBox.Show("Неверный ID заявки.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int partId;
            if (!int.TryParse(textBox4.Text, out partId))
            {
                MessageBox.Show("Неверный ID запчасти.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string newStatus = comboBox1.SelectedItem?.ToString(); // Новый статус заявки
            string partName = textBox6.Text; // Название запчасти
            int quantity;
            if (!int.TryParse(textBox5.Text, out quantity))
            {
                MessageBox.Show("Неверное количество запчастей.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SqlConnection conn = new SqlConnection("Server=MANGUST17;Database=praktika;Trusted_Connection=True;"))
            {
                try
                {
                    conn.Open();

                    // Обновление таблицы Requests
                    string updateRequestQuery = @"
                UPDATE Requests 
                SET requestStatus = @newStatus, 
                    completionDate = CASE WHEN @newStatus = 'Готова к выдаче' THEN GETDATE() ELSE completionDate END
                WHERE requestID = @requestId AND masterID = @userId";

                    SqlCommand updateRequestCmd = new SqlCommand(updateRequestQuery, conn);
                    updateRequestCmd.Parameters.AddWithValue("@newStatus", newStatus);
                    updateRequestCmd.Parameters.AddWithValue("@requestId", requestId);
                    updateRequestCmd.Parameters.AddWithValue("@userId", userId);

                    int requestRowsAffected = updateRequestCmd.ExecuteNonQuery();

                    // Обновление таблицы PartsOrdered
                    string updatePartsQuery = @"
                UPDATE PartsOrdered 
                SET part_name = @partName, 
                    quantity = @quantity, 
                    ordered_at = GETDATE()
                WHERE part_id = @partId";

                    SqlCommand updatePartsCmd = new SqlCommand(updatePartsQuery, conn);
                    updatePartsCmd.Parameters.AddWithValue("@partName", partName);
                    updatePartsCmd.Parameters.AddWithValue("@quantity", quantity);
                    updatePartsCmd.Parameters.AddWithValue("@partId", partId);

                    int partsRowsAffected = updatePartsCmd.ExecuteNonQuery();

                    // Обновление комментария, если он уже существует
                    string comment = textBox3.Text;
                    if (!string.IsNullOrWhiteSpace(comment))
                    {
                        string updateCommentQuery = @"
                    UPDATE Comments 
                    SET message = @message 
                    WHERE requestID = @requestId AND masterID = @userId";

                        SqlCommand updateCommentCmd = new SqlCommand(updateCommentQuery, conn);
                        updateCommentCmd.Parameters.AddWithValue("@message", comment);
                        updateCommentCmd.Parameters.AddWithValue("@requestId", requestId);
                        updateCommentCmd.Parameters.AddWithValue("@userId", userId);

                        int commentRowsAffected = updateCommentCmd.ExecuteNonQuery();

                        if (commentRowsAffected > 0)
                        {
                            MessageBox.Show("Комментарий обновлен.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }

                    // Сообщение об успешном обновлении данных
                    if (requestRowsAffected > 0 || partsRowsAffected > 0)
                    {
                        MessageBox.Show("Заявка и запчасти обновлены.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadMasterData(); // Обновление данных в DataGridView
                    }
                    else
                    {
                        MessageBox.Show("Не удалось обновить заявку или запчасти. Проверьте введенные данные.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при изменении данных: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }



        private void searchBtn_Click(object sender, EventArgs e)
        {

            string searchTerm = textBox1.Text; // Поле для ввода поиска
            using (SqlConnection conn = new SqlConnection("Server=MANGUST17;Database=praktika;Trusted_Connection=True;"))
                try
            {
                conn.Open();
                string query = @"SELECT r.requestID, r.problemDescription, r.requestStatus,r.homeTechID, h.techType, h.techModel 
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
                MessageBox.Show("Ошибка при поиске данных: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            Авторизация av = new Авторизация();
            av.ShowDialog();
        }
    }
}
