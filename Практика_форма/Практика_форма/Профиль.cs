using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Практика_форма
{
    public partial class Профиль : Form
    {
        private int userId;

        public Профиль(int user_id)
        {
            InitializeComponent();
            userId = user_id;
            LoadUserProfile();
        }

        private void LoadUserProfile()
        {
            string query = "SELECT type, fio, avatar FROM Users WHERE userID = @userId";

            using (SqlConnection conn = new SqlConnection("Server=MANGUST17;Database=praktika;Trusted_Connection=True;"))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@userId", userId);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        lblType.Text = reader["type"].ToString();
                        lblFio.Text = reader["fio"].ToString();

                        // Load avatar if available
                        if (reader["avatar"] != DBNull.Value)
                        {
                            byte[] avatarData = (byte[])reader["avatar"];
                            using (MemoryStream ms = new MemoryStream(avatarData))
                            {
                                pictureBox1.Image = Image.FromStream(ms);
                            }
                        }
                    }
                }
            }
        }

        // Save the updated avatar back to the database
        private void button2_Click(object sender, EventArgs e)
        {
            string query = "UPDATE Users SET avatar = @avatar WHERE userID = @userId";

            using (SqlConnection conn = new SqlConnection("Server=MANGUST17;Database=praktika;Trusted_Connection=True;"))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@userId", userId);

                // Convert the avatar image to a byte array if it exists
                if (pictureBox1.Image != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        pictureBox1.Image.Save(ms, pictureBox1.Image.RawFormat);
                        byte[] avatarData = ms.ToArray();
                        cmd.Parameters.AddWithValue("@avatar", avatarData);
                    }
                }
                else
                {
                    cmd.Parameters.AddWithValue("@avatar", DBNull.Value);
                }

                conn.Open();
                cmd.ExecuteNonQuery();
                MessageBox.Show("Profile updated successfully.");
            }
        }

        // Open a file dialog to select a new avatar image
        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    pictureBox1.Image = Image.FromFile(openFileDialog.FileName);
                }
            }
        }
    }
}
