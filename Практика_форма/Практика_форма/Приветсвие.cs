using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Практика_форма
{
    public partial class Приветсвие : Form
    {
        private int userId;
        private string user_type;
        private Timer timer;

        public Приветсвие(int user_id, string userType, string userFio, byte[] avatarData)
        {
            InitializeComponent();
            userId = user_id;
            user_type = userType;

            // Устанавливаем текст для label1
            label2.Text = $"{userFio}\n{userType}"; // Задаем текст с переводом строки
            label2.Font = new Font("Arial", 12, FontStyle.Bold); // Настройка шрифта
            label2.ForeColor = Color.Snow; // Цвет текста
            label2.TextAlign = ContentAlignment.MiddleCenter; // Центрируем текст
            label2.AutoSize = true; // Автоматическая подстройка под размер текста
            label2.Location = new Point((ClientSize.Width - label2.Width) / 2, pictureBox1.Bottom + 5); // Центрируем по горизонтали и устанавливаем Y-координату
                
            if (avatarData != null && avatarData.Length > 0)
            {
                using (MemoryStream ms = new MemoryStream(avatarData))
                {
                    pictureBox1.Image = Image.FromStream(ms);
                }
            }

            timer = new Timer();
            timer.Interval = 5000;
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();

            Form nextForm = null;

            switch (user_type)
            {
                case "Оператор":
                    nextForm = new Оператор(userId);
                    break;
                case "Мастер":
                    nextForm = new Мастер(userId);
                    break;
                case "Заказчик":
                    nextForm = new Заказчик(userId);
                    break;
            }

            this.Hide();
            nextForm.Show();
        }
    }
}
