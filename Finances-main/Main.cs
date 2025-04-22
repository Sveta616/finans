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

namespace Finances
{

    public partial class Main : Form
    {
        private SqlConnection connection;
        private int currentUserId;
        private Panel currentPanel;

        // Цвета для интерфейса
        private Color primaryColor = Color.FromArgb(0, 123, 255);
        private Color secondaryColor = Color.FromArgb(108, 117, 125);
        private Color successColor = Color.FromArgb(40, 167, 69);
        private Color dangerColor = Color.FromArgb(220, 53, 69);

        public Main(int userId)
        {
            InitializeComponent();
            currentUserId = userId;
            InitializeDatabaseConnection();
            InitializeMainForm();
        }

        private void InitializeDatabaseConnection()
        {
            string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=FinanceTracker;Integrated Security=True";
            connection = new SqlConnection(connectionString);

            try
            {
                connection.Open();
                CreateDefaultAccounts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database connection error: {ex.Message}");
                Application.Exit();
            }
        }

        private void CreateDefaultAccounts()
        {
            // Создаем стандартные счета, если их нет
            string[] defaultAccounts = { "Основной", "Накопительный", "Инвестиционный" };

            foreach (string accountName in defaultAccounts)
            {
                string checkQuery = "SELECT COUNT(*) FROM Accounts WHERE UserId = @UserId AND Name = @Name";
                SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
                checkCommand.Parameters.AddWithValue("@UserId", currentUserId);
                checkCommand.Parameters.AddWithValue("@Name", accountName);

                int count = Convert.ToInt32(checkCommand.ExecuteScalar());

                if (count == 0)
                {
                    string insertQuery = "INSERT INTO Accounts (UserId, Name) VALUES (@UserId, @Name)";
                    SqlCommand insertCommand = new SqlCommand(insertQuery, connection);
                    insertCommand.Parameters.AddWithValue("@UserId", currentUserId);
                    insertCommand.Parameters.AddWithValue("@Name", accountName);
                    insertCommand.ExecuteNonQuery();
                }
            }
        }

        private void InitializeMainForm()
        {
            // Настройка основной формы
            this.Text = "Финансовый трекер";
            this.Size = new Size(900, 600);
            this.MinimumSize = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Создаем главное меню
            CreateMainMenu();

            // Создаем панель для отображения контента
            currentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                AutoScroll = true
            };
            this.Controls.Add(currentPanel);

            // Показываем главную панель
            ShowDashboard();
        }

        private void CreateMainMenu()
        {
            Panel menuPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 200,
                BackColor = Color.FromArgb(33, 37, 41)
            };
            this.Controls.Add(menuPanel);

            // Заголовок меню
            Label menuTitle = new Label
            {
                Text = "Меню",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 60,
                TextAlign = ContentAlignment.MiddleCenter
            };
            menuPanel.Controls.Add(menuTitle);

            // Кнопки меню
            string[] menuItems = { "Обзор", "Доходы", "Расходы", "Переводы", "Категории", "Автоплатежи", "Автопополнения", "Отчеты", "Выход" };

            for (int i = 0; i < menuItems.Length; i++)
            {
                Button menuButton = new Button
                {
                    Text = menuItems[i],
                    Tag = menuItems[i],
                    Dock = DockStyle.Top,
                    Height = 50,
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(20, 0, 0, 0),
                    Font = new Font("Segoe UI", 10)
                };

                menuButton.FlatAppearance.BorderSize = 0;
                menuButton.MouseEnter += (s, e) => { menuButton.BackColor = Color.FromArgb(52, 58, 64); };
                menuButton.MouseLeave += (s, e) => { menuButton.BackColor = Color.Transparent; };

                menuButton.Click += MenuButton_Click;

                menuPanel.Controls.Add(menuButton);
                menuButton.BringToFront();
            }
        }

        private void MenuButton_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            string menuItem = button.Tag.ToString();

            currentPanel.Controls.Clear();

            switch (menuItem)
            {
                case "Обзор":
                    ShowDashboard();
                    break;
                case "Доходы":
                    ShowTransactionForm("Income");
                    break;
                case "Расходы":
                    ShowTransactionForm("Expense");
                    break;
                case "Переводы":
                    ShowTransferForm();
                    break;
                case "Категории":
                    ShowCategoriesForm();
                    break;
                case "Автоплатежи":
                    ShowAutoPaymentsForm();
                    break;
                case "Отчеты":
                    ShowReportsForm();
                    break;
                case "Автопополнения":
                    ShowAutoDepositsForm(); // Аналог ShowAutoPaymentsForm(), но для пополнений
                    break;
                case "Выход":
                    this.Close();
                    break;
            }
        }

        private void ShowAutoDepositsForm()
        {
            // Заголовок
            Label titleLabel = new Label
            {
                Text = "Автопополнения",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };
            currentPanel.Controls.Add(titleLabel);

            // Таблица с автопополнениями
            DataGridView autoDepositsGrid = new DataGridView
            {
                Location = new Point(20, 70),
                Size = new Size(currentPanel.Width - 250, 300),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                ReadOnly = true
            };
            currentPanel.Controls.Add(autoDepositsGrid);

            // Загрузка автопополнений
            LoadAutoDeposits(autoDepositsGrid);

            // Панель для добавления нового автопополнения
            Panel addPanel = new Panel
            {
                Location = new Point(currentPanel.Width - 210, 70),
                Size = new Size(190, 350),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10)
            };
            currentPanel.Controls.Add(addPanel);

            Label addLabel = new Label
            {
                Text = "Новое автопополнение",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10)
            };
            addPanel.Controls.Add(addLabel);

            // Название
            Label nameLabel = new Label { Text = "Название:", Location = new Point(10, 40), AutoSize = true };
            addPanel.Controls.Add(nameLabel);

            TextBox nameTextBox = new TextBox { Location = new Point(10, 60), Width = 170 };
            addPanel.Controls.Add(nameTextBox);

            // Счет получателя
            Label accountLabel = new Label { Text = "На счет:", Location = new Point(10, 90), AutoSize = true };
            addPanel.Controls.Add(accountLabel);

            ComboBox toAccountCombo = new ComboBox
            {
                Location = new Point(10, 110),
                Width = 170,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = "Name",
                ValueMember = "Id",
                DataSource = GetUserAccounts()
            };
            addPanel.Controls.Add(toAccountCombo);

            // Источник пополнения
            Label sourceLabel = new Label { Text = "Источник:", Location = new Point(10, 140), AutoSize = true };
            addPanel.Controls.Add(sourceLabel);

            ComboBox sourceCombo = new ComboBox
            {
                Location = new Point(10, 160),
                Width = 170,
                Items = { "Зарплата", "Дивиденды", "Проценты", "Возврат долга", "Другое" },
                SelectedIndex = 0
            };
            addPanel.Controls.Add(sourceCombo);

            // Сумма
            Label amountLabel = new Label { Text = "Сумма:", Location = new Point(10, 190), AutoSize = true };
            addPanel.Controls.Add(amountLabel);

            TextBox amountTextBox = new TextBox { Location = new Point(10, 210), Width = 170, Text = "0" };
            addPanel.Controls.Add(amountTextBox);

            // Дата следующего пополнения
            Label dateLabel = new Label { Text = "Дата:", Location = new Point(10, 240), AutoSize = true };
            addPanel.Controls.Add(dateLabel);

            DateTimePicker datePicker = new DateTimePicker
            {
                Location = new Point(10, 260),
                Width = 170,
                Value = DateTime.Now.AddDays(1)
            };
            addPanel.Controls.Add(datePicker);

            // Периодичность
            Label frequencyLabel = new Label { Text = "Периодичность:", Location = new Point(10, 290), AutoSize = true };
            addPanel.Controls.Add(frequencyLabel);

            ComboBox frequencyCombo = new ComboBox
            {
                Location = new Point(10, 310),
                Width = 170,
                Items = { "Ежедневно", "Еженедельно", "Ежемесячно", "Ежеквартально", "Ежегодно" },
                SelectedIndex = 2
            };
            addPanel.Controls.Add(frequencyCombo);

            // Кнопка добавления
            Button addButton = new Button
            {
                Text = "Добавить",
                BackColor = successColor,
                ForeColor = Color.White,
                Size = new Size(170, 30),
                Location = new Point(10, 340),
                FlatStyle = FlatStyle.Flat
            };
            addButton.FlatAppearance.BorderSize = 0;
            addButton.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(nameTextBox.Text))
                {
                    MessageBox.Show("Введите название автопополнения");
                    return;
                }

                if (!decimal.TryParse(amountTextBox.Text, out decimal amount) || amount <= 0)
                {
                    MessageBox.Show("Введите корректную сумму");
                    return;
                }

                if (toAccountCombo.SelectedValue == null)
                {
                    MessageBox.Show("Выберите счет");
                    return;
                }

                int toAccountId = (int)toAccountCombo.SelectedValue;
                string source = sourceCombo.SelectedItem.ToString();

                string frequency;
                switch (frequencyCombo.SelectedItem.ToString())
                {
                    case "Ежедневно":
                        frequency = "Daily";
                        break;
                    case "Еженедельно":
                        frequency = "Weekly";
                        break;
                    case "Ежеквартально":
                        frequency = "Quarterly";
                        break;
                    case "Ежегодно":
                        frequency = "Yearly";
                        break;
                    default:
                        frequency = "Monthly";
                        break;
                }

                string query = @"INSERT INTO AutoDeposits 
                       (UserId, Name, Amount, ToAccountId, Source, NextDate, Frequency)
                       VALUES (@UserId, @Name, @Amount, @ToAccountId, @Source, @NextDate, @Frequency)";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", currentUserId);
                command.Parameters.AddWithValue("@Name", nameTextBox.Text.Trim());
                command.Parameters.AddWithValue("@Amount", amount);
                command.Parameters.AddWithValue("@ToAccountId", toAccountId);
                command.Parameters.AddWithValue("@Source", source);
                command.Parameters.AddWithValue("@NextDate", datePicker.Value);
                command.Parameters.AddWithValue("@Frequency", frequency);

                try
                {
                    command.ExecuteNonQuery();
                    nameTextBox.Text = "";
                    amountTextBox.Text = "0";
                    LoadAutoDeposits(autoDepositsGrid);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении автопополнения: {ex.Message}");
                }
            };
            addPanel.Controls.Add(addButton);

            // Кнопка обработки автопополнений
            Button processButton = new Button
            {
                Text = "Обработать автопополнения",
                BackColor = successColor,
                ForeColor = Color.White,
                Size = new Size(200, 40),
                Location = new Point(20, 390),
                FlatStyle = FlatStyle.Flat
            };
            processButton.FlatAppearance.BorderSize = 0;
            processButton.Click += (s, e) => ProcessAutoDeposits(autoDepositsGrid);
            currentPanel.Controls.Add(processButton);
        }

        private void ProcessAutoDeposits(DataGridView grid)
        {
            string query = @"SELECT ad.Id, ad.Name, ad.Amount, ad.ToAccountId, ad.Source, ad.Frequency
                   FROM AutoDeposits ad
                   WHERE ad.UserId = @UserId AND ad.NextDate <= @Today";

            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", currentUserId);
            command.Parameters.AddWithValue("@Today", DateTime.Now.Date);

            int processedCount = 0;

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    decimal amount = reader.GetDecimal(2);
                    int toAccountId = reader.GetInt32(3);
                    string source = reader.GetString(4);
                    string frequency = reader.GetString(5);

                    // Создаем транзакцию пополнения
                    string transactionQuery = @"INSERT INTO Transactions 
                                      (UserId, AccountId, Type, Amount, Date, Description)
                                      VALUES (@UserId, @AccountId, 'Income', @Amount, @Date, @Description)";

                    SqlCommand transactionCommand = new SqlCommand(transactionQuery, connection);
                    transactionCommand.Parameters.AddWithValue("@UserId", currentUserId);
                    transactionCommand.Parameters.AddWithValue("@AccountId", toAccountId);
                    transactionCommand.Parameters.AddWithValue("@Amount", amount);
                    transactionCommand.Parameters.AddWithValue("@Date", DateTime.Now);
                    transactionCommand.Parameters.AddWithValue("@Description", $"Автопополнение: {name} ({source})");

                    // Обновляем баланс счета
                    string updateAccountQuery = "UPDATE Accounts SET Balance = Balance + @Amount WHERE Id = @AccountId";
                    SqlCommand updateCommand = new SqlCommand(updateAccountQuery, connection);
                    updateCommand.Parameters.AddWithValue("@Amount", amount);
                    updateCommand.Parameters.AddWithValue("@AccountId", toAccountId);

                    // Обновляем дату следующего пополнения
                    string updateAutoDepositQuery = "UPDATE AutoDeposits SET NextDate = @NextDate, LastProcessed = @LastProcessed WHERE Id = @Id";
                    DateTime nextDate = CalculateNextPaymentDate(frequency);

                    SqlCommand updateAutoDepositCommand = new SqlCommand(updateAutoDepositQuery, connection);
                    updateAutoDepositCommand.Parameters.AddWithValue("@NextDate", nextDate);
                    updateAutoDepositCommand.Parameters.AddWithValue("@LastProcessed", DateTime.Now);
                    updateAutoDepositCommand.Parameters.AddWithValue("@Id", id);

                    // Начинаем транзакцию
                    SqlTransaction transaction = connection.BeginTransaction();

                    try
                    {
                        transactionCommand.Transaction = transaction;
                        updateCommand.Transaction = transaction;
                        updateAutoDepositCommand.Transaction = transaction;

                        transactionCommand.ExecuteNonQuery();
                        updateCommand.ExecuteNonQuery();
                        updateAutoDepositCommand.ExecuteNonQuery();

                        transaction.Commit();
                        processedCount++;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            MessageBox.Show($"Обработано автопополнений: {processedCount}");
            LoadAutoDeposits(grid);
        }

        private void LoadAutoDeposits(DataGridView grid)
        {
            string query = @"SELECT ad.Id, ad.Name, ad.Amount, 
                   a.Name AS ToAccount, ad.Source,
                   ad.NextDate, 
                   CASE ad.Frequency
                       WHEN 'Daily' THEN 'Ежедневно'
                       WHEN 'Weekly' THEN 'Еженедельно'
                       WHEN 'Monthly' THEN 'Ежемесячно'
                       WHEN 'Quarterly' THEN 'Ежеквартально'
                       WHEN 'Yearly' THEN 'Ежегодно'
                   END AS Frequency
                   FROM AutoDeposits ad
                   JOIN Accounts a ON ad.ToAccountId = a.Id
                   WHERE ad.UserId = @UserId
                   ORDER BY ad.NextDate";

            SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
            adapter.SelectCommand.Parameters.AddWithValue("@UserId", currentUserId);

            DataTable table = new DataTable();
            adapter.Fill(table);

            grid.DataSource = table;

            // Настройка столбцов
            if (grid.Columns.Contains("Amount"))
            {
                grid.Columns["Amount"].DefaultCellStyle.Format = "C";
                grid.Columns["Amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            if (grid.Columns.Contains("NextDate"))
            {
                grid.Columns["NextDate"].DefaultCellStyle.Format = "dd.MM.yyyy";
            }
        }

        private void ProcessAutoDeposits_Click(object sender, EventArgs e)
        {
            string query = @"SELECT ad.Id, ad.Name, ad.Amount, ad.ToAccountId, ad.Source, ad.Frequency
                   FROM AutoDeposits ad
                   WHERE ad.UserId = @UserId AND ad.NextDate <= @Today";

            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", currentUserId);
            command.Parameters.AddWithValue("@Today", DateTime.Now.Date);

            int processedCount = 0;

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    decimal amount = reader.GetDecimal(2);
                    int toAccountId = reader.GetInt32(3);
                    string source = reader.GetString(4);
                    string frequency = reader.GetString(5);

                    // Создаем транзакцию пополнения
                    string transactionQuery = @"INSERT INTO Transactions 
                                      (UserId, AccountId, Type, Amount, Date, Description)
                                      VALUES (@UserId, @AccountId, 'Income', @Amount, @Date, @Description)";

                    SqlCommand transactionCommand = new SqlCommand(transactionQuery, connection);
                    transactionCommand.Parameters.AddWithValue("@UserId", currentUserId);
                    transactionCommand.Parameters.AddWithValue("@AccountId", toAccountId);
                    transactionCommand.Parameters.AddWithValue("@Amount", amount);
                    transactionCommand.Parameters.AddWithValue("@Date", DateTime.Now);
                    transactionCommand.Parameters.AddWithValue("@Description", $"Автопополнение: {name} ({source})");

                    // Обновляем баланс счета
                    string updateAccountQuery = "UPDATE Accounts SET Balance = Balance + @Amount WHERE Id = @AccountId";
                    SqlCommand updateCommand = new SqlCommand(updateAccountQuery, connection);
                    updateCommand.Parameters.AddWithValue("@Amount", amount);
                    updateCommand.Parameters.AddWithValue("@AccountId", toAccountId);

                    // Обновляем дату следующего пополнения
                    string updateAutoDepositQuery = "UPDATE AutoDeposits SET NextDate = @NextDate, LastProcessed = @LastProcessed WHERE Id = @Id";
                    DateTime nextDate = CalculateNextPaymentDate(frequency);

                    SqlCommand updateAutoDepositCommand = new SqlCommand(updateAutoDepositQuery, connection);
                    updateAutoDepositCommand.Parameters.AddWithValue("@NextDate", nextDate);
                    updateAutoDepositCommand.Parameters.AddWithValue("@LastProcessed", DateTime.Now);
                    updateAutoDepositCommand.Parameters.AddWithValue("@Id", id);

                    // Начинаем транзакцию
                    SqlTransaction transaction = connection.BeginTransaction();

                    try
                    {
                        transactionCommand.Transaction = transaction;
                        updateCommand.Transaction = transaction;
                        updateAutoDepositCommand.Transaction = transaction;

                        transactionCommand.ExecuteNonQuery();
                        updateCommand.ExecuteNonQuery();
                        updateAutoDepositCommand.ExecuteNonQuery();

                        transaction.Commit();
                        processedCount++;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            MessageBox.Show($"Обработано автопополнений: {processedCount}");

            // Обновляем таблицу
            DataGridView grid = currentPanel.Controls.OfType<DataGridView>().First();
            LoadAutoDeposits(grid);
        }

        private void ShowDashboard()
        {
            // Заголовок
            Label titleLabel = new Label
            {
                Text = "Обзор финансов",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };
            currentPanel.Controls.Add(titleLabel);

            // Общий баланс
            decimal totalBalance = GetTotalBalance();
            Label balanceLabel = new Label
            {
                Text = $"Общий баланс: {totalBalance:C}",
                Font = new Font("Segoe UI", 14),
                AutoSize = true,
                Location = new Point(20, 60)
            };
            currentPanel.Controls.Add(balanceLabel);

            // Панель с быстрыми действиями
            Panel quickActionsPanel = new Panel
            {
                BorderStyle = BorderStyle.FixedSingle,
                Size = new Size(currentPanel.Width - 40, 100),
                Location = new Point(20, 100),
                Padding = new Padding(10)
            };
            currentPanel.Controls.Add(quickActionsPanel);

            string[] quickActions = { "Добавить доход", "Добавить расход", "Сделать перевод" };
            Color[] actionColors = { successColor, dangerColor, primaryColor };

            for (int i = 0; i < quickActions.Length; i++)
            {
                Button actionButton = new Button
                {
                    Text = quickActions[i],
                    BackColor = actionColors[i],
                    ForeColor = Color.White,
                    Size = new Size(150, 70),
                    Location = new Point(20 + i * 170, 10),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10),
                    Tag = quickActions[i]
                };

                actionButton.FlatAppearance.BorderSize = 0;
                actionButton.Click += QuickActionButton_Click;

                quickActionsPanel.Controls.Add(actionButton);
            }

            // Последние транзакции
            Label recentLabel = new Label
            {
                Text = "Последние транзакции",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 220)
            };
            currentPanel.Controls.Add(recentLabel);

            DataGridView transactionsGrid = new DataGridView
            {
                Location = new Point(20, 250),
                Size = new Size(currentPanel.Width - 60, 200),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false
            };
            currentPanel.Controls.Add(transactionsGrid);

            // Загрузка последних транзакций
            LoadRecentTransactions(transactionsGrid);
        }

        private decimal GetTotalBalance()
        {
            string query = "SELECT SUM(Balance) FROM Accounts WHERE UserId = @UserId";
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", currentUserId);

            object result = command.ExecuteScalar();
            return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }

        private void LoadRecentTransactions(DataGridView grid)
        {
            string query = @"SELECT TOP 10 t.Date, a.Name AS Account, t.Type, t.Amount, c.Name AS Category, t.Description
                            FROM Transactions t
                            JOIN Accounts a ON t.AccountId = a.Id
                            LEFT JOIN Categories c ON t.CategoryId = c.Id
                            WHERE t.UserId = @UserId
                            ORDER BY t.Date DESC";

            SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
            adapter.SelectCommand.Parameters.AddWithValue("@UserId", currentUserId);

            DataTable table = new DataTable();
            adapter.Fill(table);

            grid.DataSource = table;

            // Форматирование столбцов
            if (grid.Columns.Contains("Amount"))
            {
                grid.Columns["Amount"].DefaultCellStyle.Format = "C";
                grid.Columns["Amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            if (grid.Columns.Contains("Date"))
            {
                grid.Columns["Date"].DefaultCellStyle.Format = "dd.MM.yyyy";
            }
        }

        private void QuickActionButton_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            string action = button.Tag.ToString();

            currentPanel.Controls.Clear();

            switch (action)
            {
                case "Добавить доход":
                    ShowTransactionForm("Income");
                    break;
                case "Добавить расход":
                    ShowTransactionForm("Expense");
                    break;
                case "Сделать перевод":
                    ShowTransferForm();
                    break;
            }
        }

        private void ShowTransactionForm(string transactionType)
        {
            // Заголовок
            string title = transactionType == "Income" ? "Добавление дохода" : "Добавление расхода";
            Label titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };
            currentPanel.Controls.Add(titleLabel);

            // Форма для ввода данных
            int yPos = 70;
            int labelWidth = 150;
            int controlWidth = 300;

            // Счет
            AddLabelAndComboBox("Счет:", "account", GetUserAccounts(), 20, ref yPos, labelWidth, controlWidth);

            // Сумма
            AddLabelAndTextBox("Сумма:", "amount", "0", 20, ref yPos, labelWidth, controlWidth);

            // Дата
            AddLabelAndDateTimePicker("Дата:", "date", DateTime.Now, 20, ref yPos, labelWidth, controlWidth);

            // Категория (только для расходов)
            if (transactionType == "Expense")
            {
                AddLabelAndComboBox("Категория:", "category", GetUserCategories("Expense"), 20, ref yPos, labelWidth, controlWidth);
            }
            else
            {
                AddLabelAndComboBox("Категория:", "category", GetUserCategories("Income"), 20, ref yPos, labelWidth, controlWidth);
            }

            // Описание
            AddLabelAndTextBox("Описание:", "description", "", 20, ref yPos, labelWidth, controlWidth);

            // Кнопка сохранения
            Button saveButton = new Button
            {
                Text = "Сохранить",
                BackColor = transactionType == "Income" ? successColor : dangerColor,
                ForeColor = Color.White,
                Size = new Size(150, 40),
                Location = new Point(20 + labelWidth, yPos + 20),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Tag = transactionType
            };
            saveButton.FlatAppearance.BorderSize = 0;
            saveButton.Click += SaveTransaction_Click;
            currentPanel.Controls.Add(saveButton);

            // Кнопка отмены
            Button cancelButton = new Button
            {
                Text = "Отмена",
                BackColor = secondaryColor,
                ForeColor = Color.White,
                Size = new Size(150, 40),
                Location = new Point(190 + labelWidth, yPos + 20),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
            cancelButton.FlatAppearance.BorderSize = 0;
            cancelButton.Click += (s, e) => ShowDashboard();
            currentPanel.Controls.Add(cancelButton);
        }

        private void AddLabelAndTextBox(string labelText, string name, string defaultValue, int x, ref int y, int labelWidth, int controlWidth)
        {
            Label label = new Label
            {
                Text = labelText,
                Location = new Point(x, y),
                Size = new Size(labelWidth, 30),
                TextAlign = ContentAlignment.MiddleLeft
            };
            currentPanel.Controls.Add(label);

            TextBox textBox = new TextBox
            {
                Name = name,
                Text = defaultValue,
                Location = new Point(x + labelWidth + 10, y),
                Size = new Size(controlWidth, 30)
            };
            currentPanel.Controls.Add(textBox);

            y += 40;
        }

        private void AddLabelAndComboBox(string labelText, string name, DataTable dataSource, int x, ref int y, int labelWidth, int controlWidth)
        {
            Label label = new Label
            {
                Text = labelText,
                Location = new Point(x, y),
                Size = new Size(labelWidth, 30),
                TextAlign = ContentAlignment.MiddleLeft
            };
            currentPanel.Controls.Add(label);

            ComboBox comboBox = new ComboBox
            {
                Name = name,
                Location = new Point(x + labelWidth + 10, y),
                Size = new Size(controlWidth, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = "Name",
                ValueMember = "Id",
                DataSource = dataSource
            };
            currentPanel.Controls.Add(comboBox);

            y += 40;
        }

        private void AddLabelAndDateTimePicker(string labelText, string name, DateTime defaultValue, int x, ref int y, int labelWidth, int controlWidth)
        {
            Label label = new Label
            {
                Text = labelText,
                Location = new Point(x, y),
                Size = new Size(labelWidth, 30),
                TextAlign = ContentAlignment.MiddleLeft
            };
            currentPanel.Controls.Add(label);

            DateTimePicker dateTimePicker = new DateTimePicker
            {
                Name = name,
                Value = defaultValue,
                Location = new Point(x + labelWidth + 10, y),
                Size = new Size(controlWidth, 30),
                Format = DateTimePickerFormat.Short
            };
            currentPanel.Controls.Add(dateTimePicker);

            y += 40;
        }

        private DataTable GetUserAccounts()
        {
            DataTable table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Name", typeof(string));

            string query = "SELECT Id, Name FROM Accounts WHERE UserId = @UserId";
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", currentUserId);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    table.Rows.Add(reader["Id"], reader["Name"]);
                }
            }

            return table;
        }

        private DataTable GetUserCategories(string type)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Name", typeof(string));

            string query = "SELECT Id, Name FROM Categories WHERE UserId = @UserId AND Type = @Type";
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", currentUserId);
            command.Parameters.AddWithValue("@Type", type);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    table.Rows.Add(reader["Id"], reader["Name"]);
                }
            }

            // Добавляем пустую категорию
            table.Rows.Add(-1, "-- Без категории --");

            return table;
        }

        private void SaveTransaction_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            string transactionType = button.Tag.ToString();

            // Получаем значения из формы
            int accountId = GetSelectedComboBoxValue("account");
            decimal amount = decimal.Parse(GetControlValue<TextBox>("amount").Text);
            DateTime date = GetControlValue<DateTimePicker>("date").Value;
            int categoryId = GetSelectedComboBoxValue("category");
            string description = GetControlValue<TextBox>("description").Text;

            // Проверки
            if (amount <= 0)
            {
                MessageBox.Show("Сумма должна быть больше нуля");
                return;
            }

            if (categoryId == -1) categoryId = 0; // Устанавливаем NULL в БД

            // Сохраняем транзакцию
            string query = @"INSERT INTO Transactions (UserId, AccountId, Type, CategoryId, Amount, Date, Description)
                           VALUES (@UserId, @AccountId, @Type, @CategoryId, @Amount, @Date, @Description)";

            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", currentUserId);
            command.Parameters.AddWithValue("@AccountId", accountId);
            command.Parameters.AddWithValue("@Type", transactionType);
            command.Parameters.AddWithValue("@CategoryId", categoryId == 0 ? DBNull.Value : (object)categoryId);
            command.Parameters.AddWithValue("@Amount", amount);
            command.Parameters.AddWithValue("@Date", date);
            command.Parameters.AddWithValue("@Description", description);

            try
            {
                // Обновляем баланс счета
                string updateAccountQuery = @"UPDATE Accounts SET Balance = Balance " +
                                          (transactionType == "Income" ? "+" : "-") +
                                          " @Amount WHERE Id = @AccountId";

                SqlCommand updateCommand = new SqlCommand(updateAccountQuery, connection);
                updateCommand.Parameters.AddWithValue("@Amount", amount);
                updateCommand.Parameters.AddWithValue("@AccountId", accountId);

                // Начинаем транзакцию
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    command.Transaction = transaction;
                    updateCommand.Transaction = transaction;

                    command.ExecuteNonQuery();
                    updateCommand.ExecuteNonQuery();

                    transaction.Commit();
                    MessageBox.Show("Транзакция сохранена");
                    ShowDashboard();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
        }

        private T GetControlValue<T>(string name) where T : Control
        {
            return (T)currentPanel.Controls.Find(name, true)[0];
        }

        private int GetSelectedComboBoxValue(string name)
        {
            ComboBox comboBox = GetControlValue<ComboBox>(name);
            if (comboBox.SelectedValue != null && comboBox.SelectedValue is int)
            {
                return (int)comboBox.SelectedValue;
            }
            return -1;
        }

        private void ShowTransferForm()
        {
            // Заголовок
            Label titleLabel = new Label
            {
                Text = "Перевод между счетами",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };
            currentPanel.Controls.Add(titleLabel);

            // Форма для ввода данных
            int yPos = 70;
            int labelWidth = 150;
            int controlWidth = 300;

            // Счет отправителя
            AddLabelAndComboBox("Со счета:", "fromAccount", GetUserAccounts(), 20, ref yPos, labelWidth, controlWidth);

            // Счет получателя
            AddLabelAndComboBox("На счет:", "toAccount", GetUserAccounts(), 20, ref yPos, labelWidth, controlWidth);

            // Сумма
            AddLabelAndTextBox("Сумма:", "amount", "0", 20, ref yPos, labelWidth, controlWidth);

            // Дата
            AddLabelAndDateTimePicker("Дата:", "date", DateTime.Now, 20, ref yPos, labelWidth, controlWidth);

            // Описание
            AddLabelAndTextBox("Описание:", "description", "", 20, ref yPos, labelWidth, controlWidth);

            // Кнопка сохранения
            Button saveButton = new Button
            {
                Text = "Выполнить перевод",
                BackColor = primaryColor,
                ForeColor = Color.White,
                Size = new Size(200, 40),
                Location = new Point(20 + labelWidth, yPos + 20),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
            saveButton.FlatAppearance.BorderSize = 0;
            saveButton.Click += SaveTransfer_Click;
            currentPanel.Controls.Add(saveButton);

            // Кнопка отмены
            Button cancelButton = new Button
            {
                Text = "Отмена",
                BackColor = secondaryColor,
                ForeColor = Color.White,
                Size = new Size(150, 40),
                Location = new Point(240 + labelWidth, yPos + 20),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
            cancelButton.FlatAppearance.BorderSize = 0;
            cancelButton.Click += (s, e) => ShowDashboard();
            currentPanel.Controls.Add(cancelButton);
        }

        private void SaveTransfer_Click(object sender, EventArgs e)
        {
            // Получаем значения из формы
            int fromAccountId = GetSelectedComboBoxValue("fromAccount");
            int toAccountId = GetSelectedComboBoxValue("toAccount");
            decimal amount = decimal.Parse(GetControlValue<TextBox>("amount").Text);
            DateTime date = GetControlValue<DateTimePicker>("date").Value;
            string description = GetControlValue<TextBox>("description").Text;

            // Проверки
            if (amount <= 0)
            {
                MessageBox.Show("Сумма должна быть больше нуля");
                return;
            }

            if (fromAccountId == toAccountId)
            {
                MessageBox.Show("Нельзя перевести на тот же счет");
                return;
            }

            // Проверяем достаточно ли средств на счете
            string balanceQuery = "SELECT Balance FROM Accounts WHERE Id = @AccountId";
            SqlCommand balanceCommand = new SqlCommand(balanceQuery, connection);
            balanceCommand.Parameters.AddWithValue("@AccountId", fromAccountId);

            decimal currentBalance = Convert.ToDecimal(balanceCommand.ExecuteScalar());

            if (currentBalance < amount)
            {
                MessageBox.Show("Недостаточно средств на счете");
                return;
            }

            // Сохраняем перевод (две транзакции)
            string expenseQuery = @"INSERT INTO Transactions 
                                   (UserId, AccountId, Type, Amount, Date, Description, TargetAccountId)
                                   VALUES (@UserId, @FromAccountId, 'Transfer', @Amount, @Date, @Description, @ToAccountId)";

            string incomeQuery = @"INSERT INTO Transactions 
                                  (UserId, AccountId, Type, Amount, Date, Description, TargetAccountId)
                                  VALUES (@UserId, @ToAccountId, 'Transfer', @Amount, @Date, @Description, @FromAccountId)";

            string updateFromQuery = "UPDATE Accounts SET Balance = Balance - @Amount WHERE Id = @AccountId";
            string updateToQuery = "UPDATE Accounts SET Balance = Balance + @Amount WHERE Id = @AccountId";

            SqlCommand expenseCommand = new SqlCommand(expenseQuery, connection);
            SqlCommand incomeCommand = new SqlCommand(incomeQuery, connection);
            SqlCommand updateFromCommand = new SqlCommand(updateFromQuery, connection);
            SqlCommand updateToCommand = new SqlCommand(updateToQuery, connection);

            // Параметры для команд
            expenseCommand.Parameters.AddWithValue("@UserId", currentUserId);
            expenseCommand.Parameters.AddWithValue("@FromAccountId", fromAccountId);
            expenseCommand.Parameters.AddWithValue("@Amount", amount);
            expenseCommand.Parameters.AddWithValue("@Date", date);
            expenseCommand.Parameters.AddWithValue("@Description", description);
            expenseCommand.Parameters.AddWithValue("@ToAccountId", toAccountId);

            incomeCommand.Parameters.AddWithValue("@UserId", currentUserId);
            incomeCommand.Parameters.AddWithValue("@ToAccountId", toAccountId);
            incomeCommand.Parameters.AddWithValue("@Amount", amount);
            incomeCommand.Parameters.AddWithValue("@Date", date);
            incomeCommand.Parameters.AddWithValue("@Description", description);
            incomeCommand.Parameters.AddWithValue("@FromAccountId", fromAccountId);

            updateFromCommand.Parameters.AddWithValue("@Amount", amount);
            updateFromCommand.Parameters.AddWithValue("@AccountId", fromAccountId);

            updateToCommand.Parameters.AddWithValue("@Amount", amount);
            updateToCommand.Parameters.AddWithValue("@AccountId", toAccountId);

            // Начинаем транзакцию
            SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                expenseCommand.Transaction = transaction;
                incomeCommand.Transaction = transaction;
                updateFromCommand.Transaction = transaction;
                updateToCommand.Transaction = transaction;

                expenseCommand.ExecuteNonQuery();
                incomeCommand.ExecuteNonQuery();
                updateFromCommand.ExecuteNonQuery();
                updateToCommand.ExecuteNonQuery();

                transaction.Commit();
                MessageBox.Show("Перевод выполнен успешно");
                ShowDashboard();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                MessageBox.Show($"Ошибка при выполнении перевода: {ex.Message}");
            }
        }

        private void ShowCategoriesForm()
        {
            // Заголовок
            Label titleLabel = new Label
            {
                Text = "Управление категориями",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };
            currentPanel.Controls.Add(titleLabel);

            // Таблица с категориями
            DataGridView categoriesGrid = new DataGridView
            {
                Location = new Point(20, 70),
                Size = new Size(currentPanel.Width - 250, 300),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                ReadOnly = true
            };
            currentPanel.Controls.Add(categoriesGrid);

            // Загрузка категорий
            LoadCategories(categoriesGrid);

            // Панель для добавления новой категории
            Panel addPanel = new Panel
            {
                Location = new Point(currentPanel.Width - 210, 70),
                Size = new Size(190, 200),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10)
            };
            currentPanel.Controls.Add(addPanel);

            Label addLabel = new Label
            {
                Text = "Добавить категорию",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10)
            };
            addPanel.Controls.Add(addLabel);

            // Тип категории
            ComboBox typeComboBox = new ComboBox
            {
                Items = { "Доход", "Расход" },
                SelectedIndex = 0,
                Location = new Point(10, 40),
                Width = 170
            };
            addPanel.Controls.Add(typeComboBox);

            // Название категории
            TextBox nameTextBox = new TextBox
            {
                Location = new Point(10, 80),
                Width = 170
            };
            addPanel.Controls.Add(nameTextBox);

            // Кнопка добавления
            Button addButton = new Button
            {
                Text = "Добавить",
                BackColor = successColor,
                ForeColor = Color.White,
                Size = new Size(170, 30),
                Location = new Point(10, 120),
                FlatStyle = FlatStyle.Flat
            };
            addButton.FlatAppearance.BorderSize = 0;
            addButton.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(nameTextBox.Text))
                {
                    MessageBox.Show("Введите название категории");
                    return;
                }

                string type = typeComboBox.SelectedItem.ToString() == "Доход" ? "Income" : "Expense";

                string query = "INSERT INTO Categories (UserId, Name, Type) VALUES (@UserId, @Name, @Type)";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", currentUserId);
                command.Parameters.AddWithValue("@Name", nameTextBox.Text.Trim());
                command.Parameters.AddWithValue("@Type", type);

                try
                {
                    command.ExecuteNonQuery();
                    nameTextBox.Text = "";
                    LoadCategories(categoriesGrid);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении категории: {ex.Message}");
                }
            };
            addPanel.Controls.Add(addButton);

            // Кнопка удаления
            Button deleteButton = new Button
            {
                Text = "Удалить выбранное",
                BackColor = dangerColor,
                ForeColor = Color.White,
                Size = new Size(170, 30),
                Location = new Point(10, 160),
                FlatStyle = FlatStyle.Flat
            };
            deleteButton.FlatAppearance.BorderSize = 0;
            deleteButton.Click += (s, e) =>
            {
                if (categoriesGrid.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Выберите категорию для удаления");
                    return;
                }

                int categoryId = (int)categoriesGrid.SelectedRows[0].Cells["Id"].Value;

                // Проверяем, есть ли транзакции с этой категорией
                string checkQuery = "SELECT COUNT(*) FROM Transactions WHERE CategoryId = @CategoryId";
                SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
                checkCommand.Parameters.AddWithValue("@CategoryId", categoryId);

                int count = Convert.ToInt32(checkCommand.ExecuteScalar());

                if (count > 0)
                {
                    MessageBox.Show("Нельзя удалить категорию, так как с ней связаны транзакции");
                    return;
                }

                string deleteQuery = "DELETE FROM Categories WHERE Id = @Id";
                SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection);
                deleteCommand.Parameters.AddWithValue("@Id", categoryId);

                try
                {
                    deleteCommand.ExecuteNonQuery();
                    LoadCategories(categoriesGrid);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении категории: {ex.Message}");
                }
            };
            addPanel.Controls.Add(deleteButton);
        }

        private void LoadCategories(DataGridView grid)
        {
            string query = @"SELECT c.Id, c.Name, 
                            CASE WHEN c.Type = 'Income' THEN 'Доход' ELSE 'Расход' END AS Type,
                            COUNT(t.Id) AS TransactionsCount
                            FROM Categories c
                            LEFT JOIN Transactions t ON c.Id = t.CategoryId
                            WHERE c.UserId = @UserId
                            GROUP BY c.Id, c.Name, c.Type";

            SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
            adapter.SelectCommand.Parameters.AddWithValue("@UserId", currentUserId);

            DataTable table = new DataTable();
            adapter.Fill(table);

            grid.DataSource = table;

            // Настройка столбцов
            if (grid.Columns.Contains("Id"))
            {
                grid.Columns["Id"].Visible = false;
            }
        }

        private void ShowAutoPaymentsForm()
        {
            // Заголовок
            Label titleLabel = new Label
            {
                Text = "Автоплатежи",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };
            currentPanel.Controls.Add(titleLabel);

            // Таблица с автоплатежами
            DataGridView autoPaymentsGrid = new DataGridView
            {
                Location = new Point(20, 70),
                Size = new Size(currentPanel.Width - 250, 300),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                ReadOnly = true
            };
            currentPanel.Controls.Add(autoPaymentsGrid);

            // Загрузка автоплатежей
            LoadAutoPayments(autoPaymentsGrid);

            // Панель для добавления нового автоплатежа
            Panel addPanel = new Panel
            {
                Location = new Point(currentPanel.Width - 210, 70),
                Size = new Size(190, 400),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10)
            };
            currentPanel.Controls.Add(addPanel);

            Label addLabel = new Label
            {
                Text = "Новый автоплатеж",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10)
            };
            addPanel.Controls.Add(addLabel);

            // Название
            addPanel.Controls.Add(new Label { Text = "Название:", Location = new Point(10, 40), AutoSize = true });
            TextBox nameTextBox = new TextBox { Location = new Point(10, 60), Width = 170 };
            addPanel.Controls.Add(nameTextBox);

            // Счет отправителя
            addPanel.Controls.Add(new Label { Text = "Со счета:", Location = new Point(10, 90), AutoSize = true });
            ComboBox fromAccountCombo = new ComboBox
            {
                Location = new Point(10, 110),
                Width = 170,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = "Name",
                ValueMember = "Id",
                DataSource = GetUserAccounts()
            };
            addPanel.Controls.Add(fromAccountCombo);

            // Счет получателя
            addPanel.Controls.Add(new Label { Text = "На счет:", Location = new Point(10, 140), AutoSize = true });
            ComboBox toAccountCombo = new ComboBox
            {
                Location = new Point(10, 160),
                Width = 170,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = "Name",
                ValueMember = "Id",
                DataSource = GetUserAccounts()
            };
            addPanel.Controls.Add(toAccountCombo);

            // Сумма
            addPanel.Controls.Add(new Label { Text = "Сумма:", Location = new Point(10, 190), AutoSize = true });
            TextBox amountTextBox = new TextBox { Location = new Point(10, 210), Width = 170, Text = "0" };
            addPanel.Controls.Add(amountTextBox);

            // Дата следующего платежа
            addPanel.Controls.Add(new Label { Text = "Дата:", Location = new Point(10, 240), AutoSize = true });
            DateTimePicker datePicker = new DateTimePicker
            {
                Location = new Point(10, 260),
                Width = 170,
                Value = DateTime.Now.AddDays(1)
            };
            addPanel.Controls.Add(datePicker);

            // Периодичность
            addPanel.Controls.Add(new Label { Text = "Периодичность:", Location = new Point(10, 290), AutoSize = true });
            ComboBox frequencyCombo = new ComboBox
            {
                Location = new Point(10, 310),
                Width = 170,
                Items = { "Ежедневно", "Еженедельно", "Ежемесячно", "Ежеквартально", "Ежегодно" },
                SelectedIndex = 2
            };
            addPanel.Controls.Add(frequencyCombo);

            // Кнопка добавления
            Button addButton = new Button
            {
                Text = "Добавить",
                BackColor = successColor,
                ForeColor = Color.White,
                Size = new Size(170, 30),
                Location = new Point(10, 350),
                FlatStyle = FlatStyle.Flat
            };
            addButton.FlatAppearance.BorderSize = 0;
            addButton.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(nameTextBox.Text))
                {
                    MessageBox.Show("Введите название автоплатежа");
                    return;
                }

                if (!decimal.TryParse(amountTextBox.Text, out decimal amount) || amount <= 0)
                {
                    MessageBox.Show("Введите корректную сумму");
                    return;
                }

                if (fromAccountCombo.SelectedValue == null || toAccountCombo.SelectedValue == null)
                {
                    MessageBox.Show("Выберите счета");
                    return;
                }

                int fromAccountId = (int)fromAccountCombo.SelectedValue;
                int toAccountId = (int)toAccountCombo.SelectedValue;

                if (fromAccountId == toAccountId)
                {
                    MessageBox.Show("Нельзя перевести на тот же счет");
                    return;
                }


                string query = @"INSERT INTO AutoPayments 
                               (UserId, Name, Amount, FromAccountId, ToAccountId, NextDate, Frequency)
                               VALUES (@UserId, @Name, @Amount, @FromAccountId, @ToAccountId, @NextDate, @Frequency)";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", currentUserId);
                command.Parameters.AddWithValue("@Name", nameTextBox.Text.Trim());
                command.Parameters.AddWithValue("@Amount", amount);
                command.Parameters.AddWithValue("@FromAccountId", fromAccountId);
                command.Parameters.AddWithValue("@ToAccountId", toAccountId);
                command.Parameters.AddWithValue("@NextDate", datePicker.Value);


                try
                {
                    command.ExecuteNonQuery();
                    nameTextBox.Text = "";
                    amountTextBox.Text = "0";
                    LoadAutoPayments(autoPaymentsGrid);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении автоплатежа: {ex.Message}");
                }
            };
            addPanel.Controls.Add(addButton);

            // Кнопка обработки автоплатежей
            Button processButton = new Button
            {
                Text = "Обработать автоплатежи",
                BackColor = primaryColor,
                ForeColor = Color.White,
                Size = new Size(200, 40),
                Location = new Point(20, 390),
                FlatStyle = FlatStyle.Flat
            };
            processButton.FlatAppearance.BorderSize = 0;
            processButton.Click += ProcessAutoPayments_Click;
            currentPanel.Controls.Add(processButton);
        }

        private void LoadAutoPayments(DataGridView grid)
        {
            string query = @"SELECT ap.Id, ap.Name, ap.Amount, 
                           fa.Name AS FromAccount, ta.Name AS ToAccount,
                           ap.NextDate, 
                           CASE ap.Frequency
                               WHEN 'Daily' THEN 'Ежедневно'
                               WHEN 'Weekly' THEN 'Еженедельно'
                               WHEN 'Monthly' THEN 'Ежемесячно'
                               WHEN 'Quarterly' THEN 'Ежеквартально'
                               WHEN 'Yearly' THEN 'Ежегодно'
                           END AS Frequency
                           FROM AutoPayments ap
                           JOIN Accounts fa ON ap.FromAccountId = fa.Id
                           JOIN Accounts ta ON ap.ToAccountId = ta.Id
                           WHERE ap.UserId = @UserId
                           ORDER BY ap.NextDate";

            SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
            adapter.SelectCommand.Parameters.AddWithValue("@UserId", currentUserId);

            DataTable table = new DataTable();
            adapter.Fill(table);

            grid.DataSource = table;

            // Настройка столбцов
            if (grid.Columns.Contains("Amount"))
            {
                grid.Columns["Amount"].DefaultCellStyle.Format = "C";
                grid.Columns["Amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            if (grid.Columns.Contains("NextDate"))
            {
                grid.Columns["NextDate"].DefaultCellStyle.Format = "dd.MM.yyyy";
            }
        }

        private void ProcessAutoPayments_Click(object sender, EventArgs e)
        {
            string query = @"SELECT ap.Id, ap.Name, ap.Amount, ap.FromAccountId, ap.ToAccountId, ap.Frequency
                           FROM AutoPayments ap
                           WHERE ap.UserId = @UserId AND ap.NextDate <= @Today";

            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", currentUserId);
            command.Parameters.AddWithValue("@Today", DateTime.Now.Date);

            int processedCount = 0;

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    decimal amount = reader.GetDecimal(2);
                    int fromAccountId = reader.GetInt32(3);
                    int toAccountId = reader.GetInt32(4);
                    string frequency = reader.GetString(5);

                    // Выполняем перевод
                    ProcessSingleAutoPayment(id, name, amount, fromAccountId, toAccountId, frequency);
                    processedCount++;
                }
            }

            MessageBox.Show($"Обработано автоплатежей: {processedCount}");

            // Обновляем таблицу
            DataGridView grid = currentPanel.Controls.OfType<DataGridView>().First();
            LoadAutoPayments(grid);
        }

        private void ProcessSingleAutoPayment(int id, string name, decimal amount, int fromAccountId, int toAccountId, string frequency)
        {
            // Проверяем достаточно ли средств на счете
            string balanceQuery = "SELECT Balance FROM Accounts WHERE Id = @AccountId";
            SqlCommand balanceCommand = new SqlCommand(balanceQuery, connection);
            balanceCommand.Parameters.AddWithValue("@AccountId", fromAccountId);

            decimal currentBalance = Convert.ToDecimal(balanceCommand.ExecuteScalar());

            if (currentBalance < amount)
            {
                // Пропускаем этот платеж, но обновляем дату
                UpdateNextAutoPaymentDate(id, frequency);
                return;
            }

            // Создаем запись о транзакции (списание)
            string expenseQuery = @"INSERT INTO Transactions 
                                  (UserId, AccountId, Type, Amount, Date, Description, TargetAccountId)
                                  VALUES (@UserId, @FromAccountId, 'Transfer', @Amount, @Date, @Description, @ToAccountId)";

            SqlCommand expenseCommand = new SqlCommand(expenseQuery, connection);
            expenseCommand.Parameters.AddWithValue("@UserId", currentUserId);
            expenseCommand.Parameters.AddWithValue("@FromAccountId", fromAccountId);
            expenseCommand.Parameters.AddWithValue("@Amount", amount);
            expenseCommand.Parameters.AddWithValue("@Date", DateTime.Now);
            expenseCommand.Parameters.AddWithValue("@Description", $"Автоплатеж: {name}");
            expenseCommand.Parameters.AddWithValue("@ToAccountId", toAccountId);

            // Создаем запись о транзакции (зачисление)
            string incomeQuery = @"INSERT INTO Transactions 
                                 (UserId, AccountId, Type, Amount, Date, Description, TargetAccountId)
                                 VALUES (@UserId, @ToAccountId, 'Transfer', @Amount, @Date, @Description, @FromAccountId)";

            SqlCommand incomeCommand = new SqlCommand(incomeQuery, connection);
            incomeCommand.Parameters.AddWithValue("@UserId", currentUserId);
            incomeCommand.Parameters.AddWithValue("@ToAccountId", toAccountId);
            incomeCommand.Parameters.AddWithValue("@Amount", amount);
            incomeCommand.Parameters.AddWithValue("@Date", DateTime.Now);
            incomeCommand.Parameters.AddWithValue("@Description", $"Автоплатеж: {name}");
            incomeCommand.Parameters.AddWithValue("@FromAccountId", fromAccountId);

            // Обновляем балансы счетов
            string updateFromQuery = "UPDATE Accounts SET Balance = Balance - @Amount WHERE Id = @AccountId";
            string updateToQuery = "UPDATE Accounts SET Balance = Balance + @Amount WHERE Id = @AccountId";

            SqlCommand updateFromCommand = new SqlCommand(updateFromQuery, connection);
            updateFromCommand.Parameters.AddWithValue("@Amount", amount);
            updateFromCommand.Parameters.AddWithValue("@AccountId", fromAccountId);

            SqlCommand updateToCommand = new SqlCommand(updateToQuery, connection);
            updateToCommand.Parameters.AddWithValue("@Amount", amount);
            updateToCommand.Parameters.AddWithValue("@AccountId", toAccountId);

            // Обновляем дату следующего платежа
            string updateAutoPaymentQuery = "UPDATE AutoPayments SET NextDate = @NextDate, LastProcessed = @LastProcessed WHERE Id = @Id";
            DateTime nextDate = CalculateNextPaymentDate(frequency);

            SqlCommand updateAutoPaymentCommand = new SqlCommand(updateAutoPaymentQuery, connection);
            updateAutoPaymentCommand.Parameters.AddWithValue("@NextDate", nextDate);
            updateAutoPaymentCommand.Parameters.AddWithValue("@LastProcessed", DateTime.Now);
            updateAutoPaymentCommand.Parameters.AddWithValue("@Id", id);

            // Начинаем транзакцию
            SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                expenseCommand.Transaction = transaction;
                incomeCommand.Transaction = transaction;
                updateFromCommand.Transaction = transaction;
                updateToCommand.Transaction = transaction;
                updateAutoPaymentCommand.Transaction = transaction;

                expenseCommand.ExecuteNonQuery();
                incomeCommand.ExecuteNonQuery();
                updateFromCommand.ExecuteNonQuery();
                updateToCommand.ExecuteNonQuery();
                updateAutoPaymentCommand.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private DateTime CalculateNextPaymentDate(string frequency)
        {
            DateTime nextDate = DateTime.Now.Date;

            switch (frequency)
            {
                case "Daily":
                    nextDate = nextDate.AddDays(1);
                    break;
                case "Weekly":
                    nextDate = nextDate.AddDays(7);
                    break;
                case "Monthly":
                    nextDate = nextDate.AddMonths(1);
                    break;
                case "Quarterly":
                    nextDate = nextDate.AddMonths(3);
                    break;
                case "Yearly":
                    nextDate = nextDate.AddYears(1);
                    break;
            }

            return nextDate;
        }

        private void UpdateNextAutoPaymentDate(int id, string frequency)
        {
            DateTime nextDate = CalculateNextPaymentDate(frequency);

            string updateQuery = "UPDATE AutoPayments SET NextDate = @NextDate WHERE Id = @Id";
            SqlCommand updateCommand = new SqlCommand(updateQuery, connection);
            updateCommand.Parameters.AddWithValue("@NextDate", nextDate);
            updateCommand.Parameters.AddWithValue("@Id", id);
            updateCommand.ExecuteNonQuery();
        }

        private void ShowReportsForm()
        {
            // Заголовок
            Label titleLabel = new Label
            {
                Text = "Отчеты",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };
            currentPanel.Controls.Add(titleLabel);

            // Параметры отчета
            int yPos = 70;
            int labelWidth = 150;
            int controlWidth = 200;

            // Период
            AddLabelAndComboBox("Период:", "period", GetReportPeriods(), 20, ref yPos, labelWidth, controlWidth);

            // Тип отчета
            AddLabelAndComboBox("Тип отчета:", "reportType", GetReportTypes(), 20, ref yPos, labelWidth, controlWidth);

            // Кнопка генерации
            Button generateButton = new Button
            {
                Text = "Сгенерировать отчет",
                BackColor = primaryColor,
                ForeColor = Color.White,
                Size = new Size(200, 40),
                Location = new Point(20 + labelWidth + 10, yPos),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
            generateButton.FlatAppearance.BorderSize = 0;
            generateButton.Click += GenerateReport_Click;
            currentPanel.Controls.Add(generateButton);

            yPos += 60;

            // Результаты отчета
            Label resultsLabel = new Label
            {
                Text = "Результаты:",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            currentPanel.Controls.Add(resultsLabel);

            yPos += 40;

            // Таблица с данными
            DataGridView reportGrid = new DataGridView
            {
                Location = new Point(20, yPos),
                Size = new Size(currentPanel.Width - 60, 200),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                ReadOnly = true
            };
            currentPanel.Controls.Add(reportGrid);

            yPos += 220;

            // График (заглушка)
            Label chartLabel = new Label
            {
                Text = "График распределения:",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            currentPanel.Controls.Add(chartLabel);

            yPos += 30;

            PictureBox chartPlaceholder = new PictureBox
            {
                Location = new Point(20, yPos),
                Size = new Size(400, 200),
                BackColor = Color.LightGray,
                BorderStyle = BorderStyle.FixedSingle
            };
            currentPanel.Controls.Add(chartPlaceholder);

            // Подпись к графику
            Label chartText = new Label
            {
                Text = "Здесь будет график",
                AutoSize = true,
                Location = new Point(180, yPos + 90),
                Font = new Font("Segoe UI", 12)
            };
            currentPanel.Controls.Add(chartText);
        }

        private DataTable GetReportPeriods()
        {
            DataTable table = new DataTable();
            table.Columns.Add("Id", typeof(string));
            table.Columns.Add("Name", typeof(string));

            table.Rows.Add("current_month", "Текущий месяц");
            table.Rows.Add("last_month", "Прошлый месяц");
            table.Rows.Add("current_quarter", "Текущий квартал");
            table.Rows.Add("last_quarter", "Прошлый квартал");
            table.Rows.Add("current_year", "Текущий год");
            table.Rows.Add("last_year", "Прошлый год");
            table.Rows.Add("all_time", "Все время");

            return table;
        }

        private DataTable GetReportTypes()
        {
            DataTable table = new DataTable();
            table.Columns.Add("Id", typeof(string));
            table.Columns.Add("Name", typeof(string));

            table.Rows.Add("income", "Доходы");
            table.Rows.Add("expense", "Расходы");
            table.Rows.Add("both", "Доходы и расходы");
            table.Rows.Add("by_category", "По категориям");
            table.Rows.Add("by_account", "По счетам");

            return table;
        }

        private void GenerateReport_Click(object sender, EventArgs e)
        {
            ComboBox periodCombo = GetControlValue<ComboBox>("period");
            ComboBox typeCombo = GetControlValue<ComboBox>("reportType");

            string periodId = periodCombo.SelectedValue.ToString();
            string reportType = typeCombo.SelectedValue.ToString();

            DataGridView grid = currentPanel.Controls.OfType<DataGridView>().First();

            // Определяем даты для периода
            DateTime startDate, endDate;

            switch (periodId)
            {
                case "current_month":
                    startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    endDate = startDate.AddMonths(1).AddDays(-1);
                    break;
                case "last_month":
                    startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1);
                    endDate = startDate.AddMonths(1).AddDays(-1);
                    break;
                case "current_quarter":
                    int quarter = (DateTime.Now.Month - 1) / 3 + 1;
                    startDate = new DateTime(DateTime.Now.Year, (quarter - 1) * 3 + 1, 1);
                    endDate = startDate.AddMonths(3).AddDays(-1);
                    break;
                case "last_quarter":
                    quarter = (DateTime.Now.Month - 1) / 3;
                    if (quarter == 0)
                    {
                        quarter = 4;
                        startDate = new DateTime(DateTime.Now.Year - 1, 10, 1);
                    }
                    else
                    {
                        startDate = new DateTime(DateTime.Now.Year, (quarter - 1) * 3 + 1, 1);
                    }
                    endDate = startDate.AddMonths(3).AddDays(-1);
                    break;
                case "current_year":
                    startDate = new DateTime(DateTime.Now.Year, 1, 1);
                    endDate = startDate.AddYears(1).AddDays(-1);
                    break;
                case "last_year":
                    startDate = new DateTime(DateTime.Now.Year - 1, 1, 1);
                    endDate = startDate.AddYears(1).AddDays(-1);
                    break;
                default: // all_time
                    startDate = DateTime.MinValue;
                    endDate = DateTime.MaxValue;
                    break;
            }

            // Формируем запрос в зависимости от типа отчета
            string query;

            switch (reportType)
            {
                case "income":
                    query = @"SELECT t.Date, a.Name AS Account, t.Amount, t.Description
                             FROM Transactions t
                             JOIN Accounts a ON t.AccountId = a.Id
                             WHERE t.UserId = @UserId AND t.Type = 'Income' 
                             AND t.Date BETWEEN @StartDate AND @EndDate
                             ORDER BY t.Date DESC";
                    break;
                case "expense":
                    query = @"SELECT t.Date, a.Name AS Account, t.Amount, c.Name AS Category, t.Description
                            FROM Transactions t
                            JOIN Accounts a ON t.AccountId = a.Id
                            LEFT JOIN Categories c ON t.CategoryId = c.Id
                            WHERE t.UserId = @UserId AND t.Type = 'Expense' 
                            AND t.Date BETWEEN @StartDate AND @EndDate
                            ORDER BY t.Date DESC";
                    break;
                case "both":
                    query = @"SELECT t.Date, t.Type, a.Name AS Account, t.Amount, c.Name AS Category, t.Description
                            FROM Transactions t
                            JOIN Accounts a ON t.AccountId = a.Id
                            LEFT JOIN Categories c ON t.CategoryId = c.Id
                            WHERE t.UserId = @UserId 
                            AND t.Date BETWEEN @StartDate AND @EndDate
                            ORDER BY t.Date DESC";
                    break;
                case "by_category":
                    query = @"SELECT 
                            CASE WHEN c.Name IS NULL THEN 'Без категории' ELSE c.Name END AS Category,
                            SUM(t.Amount) AS TotalAmount,
                            COUNT(t.Id) AS TransactionsCount
                            FROM Transactions t
                            LEFT JOIN Categories c ON t.CategoryId = c.Id
                            WHERE t.UserId = @UserId AND t.Type = 'Expense'
                            AND t.Date BETWEEN @StartDate AND @EndDate
                            GROUP BY c.Name
                            ORDER BY TotalAmount DESC";
                    break;
                case "by_account":
                    query = @"SELECT 
                            a.Name AS Account,
                            SUM(CASE WHEN t.Type = 'Income' THEN t.Amount ELSE 0 END) AS TotalIncome,
                            SUM(CASE WHEN t.Type = 'Expense' THEN t.Amount ELSE 0 END) AS TotalExpense,
                            SUM(CASE WHEN t.Type = 'Income' THEN t.Amount ELSE -t.Amount END) AS Balance
                            FROM Transactions t
                            JOIN Accounts a ON t.AccountId = a.Id
                            WHERE t.UserId = @UserId
                            AND t.Date BETWEEN @StartDate AND @EndDate
                            GROUP BY a.Name
                            ORDER BY a.Name";
                    break;
                default:
                    query = "";
                    break;
            }

            if (string.IsNullOrEmpty(query))
            {
                MessageBox.Show("Неизвестный тип отчета");
                return;
            }

            SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
            adapter.SelectCommand.Parameters.AddWithValue("@UserId", currentUserId);
            adapter.SelectCommand.Parameters.AddWithValue("@StartDate", startDate);
            adapter.SelectCommand.Parameters.AddWithValue("@EndDate", endDate);

            DataTable table = new DataTable();
            adapter.Fill(table);

            grid.DataSource = table;

            // Форматирование столбцов
            if (grid.Columns.Contains("Amount"))
            {
                grid.Columns["Amount"].DefaultCellStyle.Format = "C";
                grid.Columns["Amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            if (grid.Columns.Contains("TotalAmount"))
            {
                grid.Columns["TotalAmount"].DefaultCellStyle.Format = "C";
                grid.Columns["TotalAmount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            if (grid.Columns.Contains("TotalIncome"))
            {
                grid.Columns["TotalIncome"].DefaultCellStyle.Format = "C";
                grid.Columns["TotalIncome"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            if (grid.Columns.Contains("TotalExpense"))
            {
                grid.Columns["TotalExpense"].DefaultCellStyle.Format = "C";
                grid.Columns["TotalExpense"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            if (grid.Columns.Contains("Balance"))
            {
                grid.Columns["Balance"].DefaultCellStyle.Format = "C";
                grid.Columns["Balance"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            if (grid.Columns.Contains("Date"))
            {
                grid.Columns["Date"].DefaultCellStyle.Format = "dd.MM.yyyy";
            }
        }

        private void Main_Load (object sender, EventArgs e)
        {

        }
    }
}
