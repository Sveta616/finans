using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Finances
{
    public partial class LoginForm : Form
    {
        private SqlConnection connection;

        public LoginForm()
        {
            InitializeComponent();
            InitializeDatabaseConnection();
            InitializeLoginForm();
        }

        private void InitializeDatabaseConnection()
        {
            string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=FinanceTracker;Integrated Security=True";
            connection = new SqlConnection(connectionString);

            try
            {
                connection.Open();
                CreateTablesIfNotExist();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database connection error: {ex.Message}");
                Application.Exit();
            }
        }

        private void CreateTablesIfNotExist()
        {
            // SQL для создания таблиц (аналогично предыдущему примеру)
            // ...
        }

        private void InitializeLoginForm()
        {
            
                this.Text = "Финансовый трекер - Вход";
                this.Size = new Size(400, 500); // Increased height for better spacing
                this.StartPosition = FormStartPosition.CenterScreen;
                this.FormBorderStyle = FormBorderStyle.FixedSingle;
                this.MaximizeBox = false;
                this.BackColor = Color.FromArgb(240, 242, 245); // Light gray background

                // Main content panel
                Panel contentPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(40),
                    BackColor = Color.White, // White background for the content area
                    BorderStyle = BorderStyle.FixedSingle
                };
                this.Controls.Add(contentPanel);

                // Title label
                Label titleLabel = new Label
                {
                    Text = "Вход в систему",
                    Font = new Font("Segoe UI", 18, FontStyle.Bold),
                    ForeColor = Color.FromArgb(52, 73, 94), // Dark blue text
                    AutoSize = true,
                    Location = new Point(100, 20)
                };
                contentPanel.Controls.Add(titleLabel);

                // Username field
                int yPos = 80;
                Label usernameLabel = new Label
                {
                    Text = "Имя пользователя:",
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.FromArgb(128, 128, 128), // Gray text
                    Location = new Point(20, yPos),
                    AutoSize = true
                };
                contentPanel.Controls.Add(usernameLabel);

                TextBox usernameTextBox = new TextBox
                {
                    Name = "usernameTextBox",
                    Location = new Point(20, yPos + 25),
                    Size = new Size(300, 40),
                    Font = new Font("Segoe UI", 12),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White,
                    ForeColor = Color.Black
                };
                contentPanel.Controls.Add(usernameTextBox);

                // Password field
                yPos += 80;
                Label passwordLabel = new Label
                {
                    Text = "Пароль:",
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.FromArgb(128, 128, 128), // Gray text
                    Location = new Point(20, yPos),
                    AutoSize = true
                };
                contentPanel.Controls.Add(passwordLabel);

                TextBox passwordTextBox = new TextBox
                {
                    Name = "passwordTextBox",
                    Location = new Point(20, yPos + 25),
                    Size = new Size(300, 40),
                    Font = new Font("Segoe UI", 12),
                    PasswordChar = '*',
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White,
                    ForeColor = Color.Black
                };
                contentPanel.Controls.Add(passwordTextBox);

                // Login button
                yPos += 80;
                Button loginButton = new Button
                {
                    Text = "Войти",
                    BackColor = Color.FromArgb(52, 152, 219), // Blue button
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Size = new Size(300, 40),
                    Location = new Point(20, yPos),
                    Font = new Font("Segoe UI", 12, FontStyle.Bold)
                };
                loginButton.FlatAppearance.BorderSize = 0; // Remove border
                loginButton.Click += LoginButton_Click;
                contentPanel.Controls.Add(loginButton);


                // Кнопка регистрации
                Button registerButton = new Button
            {
                Text = "Регистрация",
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                Size = new Size(120, 40),
                Location = new Point(200, yPos),
                FlatStyle = FlatStyle.Flat
            };
            registerButton.FlatAppearance.BorderSize = 0;
            registerButton.Click += RegisterButton_Click;
            contentPanel.Controls.Add(registerButton);
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            TextBox usernameTextBox = (TextBox)this.Controls.Find("usernameTextBox", true)[0];
            TextBox passwordTextBox = (TextBox)this.Controls.Find("passwordTextBox", true)[0];

            string username = usernameTextBox.Text.Trim();
            string password = passwordTextBox.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите имя пользователя и пароль");
                return;
            }

            string query = "SELECT Id, Password, Salt FROM Users WHERE Username = @Username";
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username);

            try
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int userId = reader.GetInt32(0);
                        string storedHash = reader.GetString(1);
                        string salt = reader.GetString(2);

                        string inputHash = HashPassword(password, salt);

                        if (inputHash == storedHash)
                        {
                            // Успешный вход
                            this.Hide();
                            Main mainForm = new Main(userId);
                            mainForm.ShowDialog();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Неверный пароль");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Пользователь не найден");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при входе: {ex.Message}");
            }
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
            TextBox usernameTextBox = (TextBox)this.Controls.Find("usernameTextBox", true)[0];
            TextBox passwordTextBox = (TextBox)this.Controls.Find("passwordTextBox", true)[0];

            string username = usernameTextBox.Text.Trim();
            string password = passwordTextBox.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите имя пользователя и пароль");
                return;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать не менее 6 символов");
                return;
            }

            // Проверка, что пользователь не существует
            string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
            SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
            checkCommand.Parameters.AddWithValue("@Username", username);

            int userCount = Convert.ToInt32(checkCommand.ExecuteScalar());

            if (userCount > 0)
            {
                MessageBox.Show("Пользователь с таким именем уже существует");
                return;
            }

            // Генерация соли и хеширование пароля
            string salt = GenerateSalt();
            string passwordHash = HashPassword(password, salt);

            // Создание пользователя
            string insertQuery = "INSERT INTO Users (Username, Password, Salt) VALUES (@Username, @Password, @Salt)";
            SqlCommand insertCommand = new SqlCommand(insertQuery, connection);
            insertCommand.Parameters.AddWithValue("@Username", username);
            insertCommand.Parameters.AddWithValue("@Password", passwordHash);
            insertCommand.Parameters.AddWithValue("@Salt", salt);

            try
            {
                insertCommand.ExecuteNonQuery();
                MessageBox.Show("Регистрация успешна. Теперь вы можете войти.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}");
            }
        }

        private string GenerateSalt()
        {
            byte[] saltBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        private string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] saltedPassword = Encoding.UTF8.GetBytes(password + salt);
                byte[] hashBytes = sha256.ComputeHash(saltedPassword);
                return Convert.ToBase64String(hashBytes);
            }
        }

        private void LoginForm_Load (object sender, EventArgs e)
        {

        }
    }
}
