using System;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace Практика_форма
{
    public partial class Авторизация : Form
    {
        private string captchaText;
        private int loginAttempts = 0;
        private DateTime lockoutEndTime;
        private Timer lockoutTimer;
        private readonly string loginHistoryFilePath = "login_history.txt";

        public Авторизация()
        {
            InitializeComponent();
            pictureBoxCaptcha.Visible = false;
            textBoxCaptcha.Visible = false;
            lockoutTimer = new Timer { Interval = 1000 };
            lockoutTimer.Tick += LockoutTimer_Tick;
        }


        public bool AuthenticateUser(string login, string password,out int user_id, out string type, out string fio, out byte[] avatar)
        {
            string query = "SELECT userID, type, fio, avatar FROM Users WHERE login = @login AND password_hash = @password";
            user_id = 0;
            type = null;
            fio = null;
            avatar = null;

            using (SqlConnection conn = new SqlConnection("Server=MANGUST17;Database=praktika;Trusted_Connection=True;"))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@login", login);
                cmd.Parameters.AddWithValue("@password", password);

                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read()) 
                    {
                        user_id = Convert.ToInt32(reader["userID"]); ;
                        type = reader["type"].ToString();
                        fio = reader["fio"].ToString();
                        avatar = reader["avatar"] as byte[];
                        return true;
                    }
                }
            }

            return false; 
        }


        private void ShowCaptcha()
        {
            GenerateCaptcha();
            pictureBoxCaptcha.Visible = true;
            textBoxCaptcha.Visible = true;
        }

        private void GenerateCaptcha()
        {
            captchaText = GenerateRandomText(4);
            Bitmap captchaImage = CreateCaptchaImage(captchaText);
            pictureBoxCaptcha.Image = captchaImage;
        }

        private string GenerateRandomText(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private Bitmap CreateCaptchaImage(string captchaText)
        {
            int width = 100;
            int height = 40;
            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                using (Font font = new Font("Arial", 16, FontStyle.Bold))
                {
                    Random random = new Random();
                    for (int i = 0; i < captchaText.Length; i++)
                    {
                        g.DrawString(captchaText[i].ToString(), font,
                            new SolidBrush(Color.FromArgb(random.Next(256), random.Next(256), random.Next(256))),
                            new PointF(10 + i * 20, random.Next(5, 15)));
                    }


                    for (int i = 0; i < 5; i++)
                    {
                        int x1 = random.Next(width);
                        int y1 = random.Next(height);
                        int x2 = random.Next(width);
                        int y2 = random.Next(height);
                        g.DrawLine(new Pen(Color.Gray), x1, y1, x2, y2);
                    }
                }
            }
            return bmp;
        }

        private void LogLoginAttempt(string login, bool isSuccess)
        {
            var status = isSuccess ? "Успешно" : "Ошибка";
            using (StreamWriter writer = new StreamWriter(loginHistoryFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{login},{status}");
            }
        }

        private void LockoutTimer_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now >= lockoutEndTime)
            {
                lockoutTimer.Stop();
                loginAttempts = 0;
                MessageBox.Show("Блокировка снята. Попробуйте снова.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                button1.Enabled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (DateTime.Now < lockoutEndTime)
            {
                MessageBox.Show("Вход заблокирован. Подождите 3 минуты.","Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string login = textBox1.Text;
            string password = textBox2.Text;

            int user_id;
            string type, fio;
            byte[] avatar;

            if (loginAttempts >= 1 && textBoxCaptcha.Text != captchaText)
            {
                MessageBox.Show("Неверная капча. Попробуйте еще раз.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBoxCaptcha.Clear();
                GenerateCaptcha();
                return;
            }

            if (AuthenticateUser(login, password, out user_id, out type, out fio, out avatar))
            {
                LogLoginAttempt(login, true);
                loginAttempts = 0;
                this.Hide();
                Приветсвие greeting = new Приветсвие(user_id, type, fio, avatar);
                greeting.ShowDialog();
            }
            else
            {
                loginAttempts++;
                LogLoginAttempt(login, false);
                MessageBox.Show("Неверный логин или пароль.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (loginAttempts == 1)
                {
                    ShowCaptcha();
                }
                else if (loginAttempts == 3)
                {
                    MessageBox.Show("Вход заблокирован на 3 минуты.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    lockoutEndTime = DateTime.Now.AddMinutes(3);
                    lockoutTimer.Start();
                }
                else if (loginAttempts > 3)
                {
                    MessageBox.Show("Превышено количество попыток. Перезапустите приложение.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    button1.Enabled = false;
                }
            }
        }
    }
}
