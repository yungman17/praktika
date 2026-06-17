// ============================================================
//  Рауф — Итоговый проект: "Система управления библиотекой"
//  Файл: MainForm.cs — главное окно (тёмная тема, боковое меню)
//  Разделы: Главное меню, Книги, Читатели, Выдача, Отчёты
// ============================================================
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LibrarySystem
{
    public class MainForm : Form
    {
        // ---------- Тёмная палитра ----------
        private static readonly Color BgDark    = Color.FromArgb(24, 26, 32);   // фон контента
        private static readonly Color BgSidebar = Color.FromArgb(18, 19, 24);   // боковое меню
        private static readonly Color BgPanel   = Color.FromArgb(32, 35, 44);   // таблицы / карточки
        private static readonly Color BgInput   = Color.FromArgb(45, 49, 61);   // поля ввода
        private static readonly Color Accent    = Color.FromArgb(99, 156, 255); // акцент (синий)
        private static readonly Color AccentOk  = Color.FromArgb(82, 196, 133); // зелёный
        private static readonly Color AccentNo  = Color.FromArgb(235, 100, 100);// красный
        private static readonly Color FgText    = Color.FromArgb(230, 233, 240);// текст
        private static readonly Color FgMuted   = Color.FromArgb(140, 147, 160);// серый текст

        private LibraryData data;
        private BookController books;
        private ReaderController readers;
        private LoanController loans;

        // Контролы
        private DataGridView gridBooks, gridReaders, gridLoans;
        private TextBox txtSearch;
        private Label lblStats, lblTitle;
        private ListBox lstReport;

        // Карточки на главном экране
        private Label cardBooks, cardFree, cardReaders, cardLoans;

        // Навигация
        private Panel content;
        private readonly Dictionary<string, Panel> pages = new Dictionary<string, Panel>();
        private readonly List<Button> navButtons = new List<Button>();

        public MainForm()
        {
            data = DataService.Load();
            books = new BookController(data);
            readers = new ReaderController(data);
            loans = new LoanController(data);
            BuildUi();
            RefreshAll();
            ShowPage("Главное меню");
        }

        // =================== ИНТЕРФЕЙС ===================
        private void BuildUi()
        {
            Text = "Рауф — Система управления библиотекой";
            ClientSize = new Size(1040, 680);
            MinimumSize = new Size(820, 560);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = BgDark;
            ForeColor = FgText;
            Font = new Font("Segoe UI", 10);
            DoubleBuffered = true;

            var right = new Panel { Dock = DockStyle.Fill, BackColor = BgDark };

            lblTitle = new Label
            {
                Dock = DockStyle.Top,
                Height = 64,
                Font = new Font("Segoe UI Semibold", 17),
                ForeColor = FgText,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(24, 0, 0, 0)
            };

            lblStats = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 32,
                ForeColor = FgMuted,
                BackColor = BgSidebar,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(24, 0, 0, 0)
            };

            content = new Panel { Dock = DockStyle.Fill, BackColor = BgDark, Padding = new Padding(24, 0, 24, 16) };

            pages["Главное меню"] = BuildHomePage();
            pages["Книги"] = BuildBooksPage();
            pages["Читатели"] = BuildReadersPage();
            pages["Выдача книг"] = BuildLoansPage();
            pages["Отчёты"] = BuildReportsPage();
            foreach (var p in pages.Values)
            {
                p.Dock = DockStyle.Fill;
                p.Visible = false;
                content.Controls.Add(p);
            }

            right.Controls.Add(content);
            right.Controls.Add(lblStats);
            right.Controls.Add(lblTitle);

            var sidebar = BuildSidebar();

            Controls.Add(right);
            Controls.Add(sidebar);

            FormClosing += (s, e) => DataService.Save(data);
        }

        // ---------- Боковое меню ----------
        private Panel BuildSidebar()
        {
            var bar = new Panel { Dock = DockStyle.Left, Width = 220, BackColor = BgSidebar };

            var logo = new Label
            {
                Text = "Рауф",
                Dock = DockStyle.Top,
                Height = 80,
                Font = new Font("Segoe UI Semibold", 18),
                ForeColor = Accent,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(24, 0, 0, 0)
            };
            var subtitle = new Label
            {
                Text = "Библиотека",
                Dock = DockStyle.Top,
                Height = 24,
                Font = new Font("Segoe UI", 9),
                ForeColor = FgMuted,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(24, 0, 0, 0)
            };

            var btnExit = NavButton("Выход", null);
            btnExit.Click += (s, e) => Close();
            btnExit.Dock = DockStyle.Bottom;

            // снизу вверх (Dock=Top)
            bar.Controls.Add(NavButton("Отчёты", "Отчёты"));
            bar.Controls.Add(NavButton("Выдача книг", "Выдача книг"));
            bar.Controls.Add(NavButton("Читатели", "Читатели"));
            bar.Controls.Add(NavButton("Книги", "Книги"));
            bar.Controls.Add(NavButton("Главное меню", "Главное меню"));
            bar.Controls.Add(subtitle);
            bar.Controls.Add(logo);
            bar.Controls.Add(btnExit);
            return bar;
        }

        private Button NavButton(string text, string pageKey)
        {
            var b = new Button
            {
                Text = text,
                Dock = DockStyle.Top,
                Height = 50,
                FlatStyle = FlatStyle.Flat,
                BackColor = BgSidebar,
                ForeColor = FgText,
                Font = new Font("Segoe UI", 11),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(24, 0, 0, 0),
                Cursor = Cursors.Hand,
                Tag = pageKey
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 32, 40);
            if (pageKey != null)
            {
                b.Click += (s, e) => ShowPage(pageKey);
                navButtons.Add(b);
            }
            return b;
        }

        private void ShowPage(string key)
        {
            foreach (var p in pages) p.Value.Visible = (p.Key == key);
            lblTitle.Text = key;

            foreach (var b in navButtons)
            {
                bool active = (string)b.Tag == key;
                b.BackColor = active ? Color.FromArgb(38, 42, 54) : BgSidebar;
                b.ForeColor = active ? Accent : FgText;
                b.Font = new Font("Segoe UI", 11, active ? FontStyle.Bold : FontStyle.Regular);
            }
            if (pages.TryGetValue(key, out var page)) page.BringToFront();
            if (key == "Главное меню") RefreshHome();
        }

        // ---------- Страница "Главное меню" ----------
        private Panel BuildHomePage()
        {
            var page = new Panel { BackColor = BgDark };

            var welcome = new Label
            {
                Text = "Добро пожаловать!",
                Dock = DockStyle.Top,
                Height = 48,
                Font = new Font("Segoe UI Semibold", 14),
                ForeColor = FgText
            };
            var hint = new Label
            {
                Text = "Выберите раздел в меню слева или воспользуйтесь быстрыми действиями ниже.",
                Dock = DockStyle.Top,
                Height = 30,
                ForeColor = FgMuted
            };

            var cards = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 130,
                BackColor = BgDark,
                Padding = new Padding(0, 10, 0, 10),
                WrapContents = true
            };
            cards.Controls.Add(StatCard("Всего книг", out cardBooks, Accent));
            cards.Controls.Add(StatCard("В наличии", out cardFree, AccentOk));
            cards.Controls.Add(StatCard("Читателей", out cardReaders, Color.FromArgb(190, 150, 255)));
            cards.Controls.Add(StatCard("Активных выдач", out cardLoans, Color.FromArgb(255, 180, 90)));

            var actionsTitle = new Label
            {
                Text = "Быстрые действия",
                Dock = DockStyle.Top,
                Height = 40,
                Font = new Font("Segoe UI Semibold", 12),
                ForeColor = FgText,
                Padding = new Padding(0, 8, 0, 0)
            };
            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = BgDark,
                WrapContents = false
            };
            var bAddBook = NewButton("Добавить книгу", Accent, (s, e) => { ShowPage("Книги"); AddBook(); });
            bAddBook.Width = 190;
            var bGive = NewButton("Выдать книгу", AccentOk, (s, e) => { ShowPage("Выдача книг"); GiveBook(); });
            bGive.Width = 190;
            actions.Controls.Add(bAddBook);
            actions.Controls.Add(bGive);

            page.Controls.Add(actions);
            page.Controls.Add(actionsTitle);
            page.Controls.Add(cards);
            page.Controls.Add(hint);
            page.Controls.Add(welcome);
            return page;
        }

        private Panel StatCard(string caption, out Label valueLabel, Color color)
        {
            var card = new Panel
            {
                Width = 200,
                Height = 96,
                BackColor = BgPanel,
                Margin = new Padding(0, 0, 16, 0),
                Padding = new Padding(16, 12, 16, 12)
            };
            var value = new Label
            {
                Text = "0",
                Dock = DockStyle.Top,
                Height = 44,
                Font = new Font("Segoe UI Semibold", 24),
                ForeColor = color,
                TextAlign = ContentAlignment.MiddleLeft
            };
            var cap = new Label
            {
                Text = caption,
                Dock = DockStyle.Top,
                Height = 24,
                ForeColor = FgMuted,
                TextAlign = ContentAlignment.MiddleLeft
            };
            card.Controls.Add(cap);
            card.Controls.Add(value);
            valueLabel = value;
            return card;
        }

        private void RefreshHome()
        {
            if (cardBooks == null) return;
            cardBooks.Text = data.Books.Count.ToString();
            cardFree.Text = data.Books.Count(b => b.IsAvailable).ToString();
            cardReaders.Text = data.Readers.Count.ToString();
            cardLoans.Text = data.Loans.Count(l => !l.IsReturned).ToString();
        }

        // ---------- Страница "Книги" ----------
        private Panel BuildBooksPage()
        {
            var page = new Panel { BackColor = BgDark };
            var top = NewTopPanel();

            txtSearch = new TextBox
            {
                Width = 280,
                BackColor = BgInput,
                ForeColor = FgText,
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Поиск: название, автор или ISBN...",
                Margin = new Padding(0, 9, 12, 0)
            };
            txtSearch.TextChanged += (s, e) => RefreshBooks();

            top.Controls.Add(txtSearch);
            top.Controls.Add(NewButton("Добавить", Accent, (s, e) => AddBook()));
            top.Controls.Add(NewButton("Изменить", AccentOk, (s, e) => EditBook()));
            top.Controls.Add(NewButton("Удалить", AccentNo, (s, e) => DeleteBook()));

            gridBooks = NewGrid();
            page.Controls.Add(NewGridHost(gridBooks));
            page.Controls.Add(top);
            return page;
        }

        // ---------- Страница "Читатели" ----------
        private Panel BuildReadersPage()
        {
            var page = new Panel { BackColor = BgDark };
            var top = NewTopPanel();
            top.Controls.Add(NewButton("Добавить", Accent, (s, e) => AddReader()));
            top.Controls.Add(NewButton("Удалить", AccentNo, (s, e) => DeleteReader()));

            gridReaders = NewGrid();
            page.Controls.Add(NewGridHost(gridReaders));
            page.Controls.Add(top);
            return page;
        }

        // ---------- Страница "Выдача книг" ----------
        private Panel BuildLoansPage()
        {
            var page = new Panel { BackColor = BgDark };
            var top = NewTopPanel();
            top.Controls.Add(NewButton("Выдать книгу", Accent, (s, e) => GiveBook()));
            top.Controls.Add(NewButton("Вернуть книгу", AccentOk, (s, e) => ReturnBook()));

            gridLoans = NewGrid();
            page.Controls.Add(NewGridHost(gridLoans));
            page.Controls.Add(top);
            return page;
        }

        // ---------- Страница "Отчёты" ----------
        private Panel BuildReportsPage()
        {
            var page = new Panel { BackColor = BgDark };
            var top = NewTopPanel();
            top.Controls.Add(NewButton("Сформировать отчёт", Accent, (s, e) => BuildReport()));

            lstReport = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = BgPanel,
                ForeColor = FgText,
                BorderStyle = BorderStyle.None,
                IntegralHeight = false,
                Font = new Font("Consolas", 11f)
            };
            var host = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0), BackColor = BgDark };
            host.Controls.Add(lstReport);

            page.Controls.Add(host);
            page.Controls.Add(top);
            return page;
        }

        // =================== ЛОГИКА: КНИГИ ===================
        private void AddBook()
        {
            var fields = new[]
            {
                new Field("Название", ""),
                new Field("Автор", ""),
                new Field("Жанр", ""),
                new Field("Год издания", ""),
                new Field("ISBN", "")
            };
            if (!InputDialog("Новая книга", fields)) return;

            int.TryParse(fields[3].Value, out int year);
            Try(() => books.Add(fields[0].Value, fields[1].Value, fields[2].Value, year, fields[4].Value));
        }

        private void EditBook()
        {
            var book = SelectedBook();
            if (book == null) { Info("Выберите книгу в таблице."); return; }

            var fields = new[]
            {
                new Field("Название", book.Title),
                new Field("Автор", book.Author),
                new Field("Жанр", book.Genre),
                new Field("Год издания", book.Year.ToString()),
                new Field("ISBN", book.Isbn)
            };
            if (!InputDialog("Изменить книгу", fields)) return;

            int.TryParse(fields[3].Value, out int year);
            Try(() => books.Edit(book.Id, fields[0].Value, fields[1].Value, fields[2].Value, year, fields[4].Value));
        }

        private void DeleteBook()
        {
            var book = SelectedBook();
            if (book == null) { Info("Выберите книгу в таблице."); return; }
            if (Confirm($"Удалить книгу «{book.Title}»?"))
                Try(() => books.Delete(book.Id));
        }

        // =================== ЛОГИКА: ЧИТАТЕЛИ ===================
        private void AddReader()
        {
            var fields = new[]
            {
                new Field("ФИО читателя", ""),
                new Field("Телефон", "")
            };
            if (!InputDialog("Новый читатель", fields)) return;

            Try(() => readers.Add(fields[0].Value, fields[1].Value));
        }

        private void DeleteReader()
        {
            if (gridReaders.CurrentRow == null) { Info("Выберите читателя."); return; }
            int id = (int)gridReaders.CurrentRow.Cells["Id"].Value;
            var reader = data.Readers.First(r => r.Id == id);
            if (Confirm($"Удалить читателя «{reader.Name}»?"))
                Try(() => readers.Delete(id));
        }

        // =================== ЛОГИКА: ВЫДАЧА ===================
        private void GiveBook()
        {
            var freeBooks = data.Books.Where(b => b.IsAvailable).ToList();
            if (freeBooks.Count == 0) { Info("Нет доступных книг."); return; }
            if (data.Readers.Count == 0) { Info("Сначала добавьте читателя."); return; }

            var book = Pick("Выберите книгу", freeBooks.Select(b => $"{b.Id}. {b.Title}").ToArray());
            if (book == null) return;
            var reader = Pick("Выберите читателя", data.Readers.Select(r => $"{r.Id}. {r.Name}").ToArray());
            if (reader == null) return;

            int bookId = int.Parse(book.Split('.')[0]);
            int readerId = int.Parse(reader.Split('.')[0]);

            Try(() => loans.Give(bookId, readerId));
        }

        private void ReturnBook()
        {
            if (gridLoans.CurrentRow == null) { Info("Выберите выдачу в таблице."); return; }
            int id = (int)gridLoans.CurrentRow.Cells["Id"].Value;

            Try(() =>
            {
                int fine = loans.Return(id);
                if (fine > 0)
                    Info($"Книга возвращена с просрочкой. Штраф: {fine} руб.");
            });
        }

        // Выполнить действие контроллера с обработкой ошибок (лекция 6.2)
        private void Try(Action action)
        {
            try
            {
                action();
                SaveAndRefresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // =================== ОТЧЁТЫ ===================
        private void BuildReport()
        {
            lstReport.Items.Clear();
            lstReport.Items.Add("======= ОТЧЁТ ПО БИБЛИОТЕКЕ =======");
            lstReport.Items.Add($"Дата: {DateTime.Now:dd.MM.yyyy HH:mm}");
            lstReport.Items.Add("");
            lstReport.Items.Add($"Всего книг:        {data.Books.Count}");
            lstReport.Items.Add($"В наличии:         {data.Books.Count(b => b.IsAvailable)}");
            lstReport.Items.Add($"Выдано:            {data.Books.Count(b => !b.IsAvailable)}");
            lstReport.Items.Add($"Читателей:         {data.Readers.Count}");
            lstReport.Items.Add("");

            var overdue = loans.GetOverdue();
            lstReport.Items.Add($"--- Должники ({overdue.Count}) ---");
            foreach (var l in overdue)
            {
                var b = data.Books.FirstOrDefault(x => x.Id == l.BookId);
                var r = data.Readers.FirstOrDefault(x => x.Id == l.ReaderId);
                lstReport.Items.Add($"  {r?.Name} — «{b?.Title}», срок: {l.DueDate:dd.MM.yyyy}, штраф: {loans.CalcFine(l)} руб.");
            }
            if (overdue.Count == 0) lstReport.Items.Add("  Должников нет");

            lstReport.Items.Add("");
            var popular = data.Loans.GroupBy(l => l.BookId)
                                    .OrderByDescending(g => g.Count())
                                    .Take(3);
            lstReport.Items.Add("--- Популярные книги ---");
            foreach (var g in popular)
            {
                var b = data.Books.FirstOrDefault(x => x.Id == g.Key);
                lstReport.Items.Add($"  «{b?.Title}» — выдач: {g.Count()}");
            }
        }

        // =================== ОБНОВЛЕНИЕ ТАБЛИЦ ===================
        private void SaveAndRefresh()
        {
            DataService.Save(data);
            RefreshAll();
        }

        private void RefreshAll()
        {
            RefreshBooks();
            RefreshReaders();
            RefreshLoans();
            RefreshHome();
            lblStats.Text = $"Книг: {data.Books.Count}      Читателей: {data.Readers.Count}      Активных выдач: {data.Loans.Count(l => !l.IsReturned)}";
        }

        private void RefreshBooks()
        {
            var rows = books.Search(txtSearch?.Text)
                .Select(b => new
                {
                    b.Id,
                    Название = b.Title,
                    Автор = b.Author,
                    ISBN = b.Isbn,
                    Жанр = b.Genre,
                    Год = b.Year,
                    Статус = b.IsAvailable ? "В наличии" : "Выдана"
                }).ToList();
            gridBooks.DataSource = rows;
            TuneColumns(gridBooks);
        }

        private void RefreshReaders()
        {
            gridReaders.DataSource = data.Readers.Select(r => new
            {
                r.Id,
                ФИО = r.Name,
                Телефон = r.Phone,
                Регистрация = r.RegDate.ToString("dd.MM.yyyy")
            }).ToList();
            TuneColumns(gridReaders);
        }

        private void RefreshLoans()
        {
            gridLoans.DataSource = data.Loans.OrderByDescending(l => l.Id).Select(l => new
            {
                l.Id,
                Книга = data.Books.FirstOrDefault(b => b.Id == l.BookId)?.Title,
                Читатель = data.Readers.FirstOrDefault(r => r.Id == l.ReaderId)?.Name,
                Выдана = l.LoanDate.ToString("dd.MM.yyyy"),
                Срок = l.DueDate.ToString("dd.MM.yyyy"),
                Статус = l.IsReturned ? "Возвращена" : (l.IsOverdue ? "ПРОСРОЧКА!" : "На руках")
            }).ToList();
            TuneColumns(gridLoans);
        }

        private void TuneColumns(DataGridView grid)
        {
            if (grid.Columns.Contains("Id"))
            {
                grid.Columns["Id"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                grid.Columns["Id"].Width = 50;
                grid.Columns["Id"].HeaderText = "№";
            }
        }

        // =================== ПОМОЩНИКИ UI ===================
        private FlowLayoutPanel NewTopPanel() => new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 52,
            BackColor = BgDark,
            Padding = new Padding(0, 6, 0, 6),
            WrapContents = false
        };

        private Panel NewGridHost(DataGridView grid)
        {
            var host = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0), BackColor = BgDark };
            host.Controls.Add(grid);
            return host;
        }

        private Button NewButton(string text, Color color, EventHandler onClick)
        {
            var b = new Button
            {
                Text = text,
                Width = 150,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 10),
                Margin = new Padding(0, 0, 10, 0),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = ControlPaint.Light(color, 0.15f);
            if (onClick != null) b.Click += onClick;
            return b;
        }

        private DataGridView NewGrid()
        {
            var g = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None,
                BackgroundColor = BgPanel,
                GridColor = Color.FromArgb(50, 54, 66),
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            };
            g.DefaultCellStyle.BackColor = BgPanel;
            g.DefaultCellStyle.ForeColor = FgText;
            g.DefaultCellStyle.SelectionBackColor = Accent;
            g.DefaultCellStyle.SelectionForeColor = Color.White;
            g.DefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(37, 40, 50);
            g.ColumnHeadersDefaultCellStyle.BackColor = BgInput;
            g.ColumnHeadersDefaultCellStyle.ForeColor = FgMuted;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10);
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            g.ColumnHeadersHeight = 40;
            g.RowTemplate.Height = 32;
            return g;
        }

        private Book SelectedBook()
        {
            if (gridBooks.CurrentRow == null) return null;
            int id = (int)gridBooks.CurrentRow.Cells["Id"].Value;
            return data.Books.FirstOrDefault(b => b.Id == id);
        }

        // =================== ДИАЛОГИ ===================
        private class Field
        {
            public string Label;
            public string Value;
            public Field(string label, string value) { Label = label; Value = value ?? ""; }
        }

        private bool InputDialog(string title, Field[] fields)
        {
            int rowH = 40;
            int pad = 18;
            int formW = 440;
            int formH = pad + fields.Length * rowH + 60 + pad;

            var f = new Form
            {
                Text = title,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MinimizeBox = false, MaximizeBox = false,
                BackColor = BgDark, ForeColor = FgText,
                Font = new Font("Segoe UI", 10),
                ClientSize = new Size(formW, formH),
                Padding = new Padding(pad)
            };

            // Кнопки (снизу)
            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 52
            };
            var ok = NewButton("ОК", Accent, null);
            ok.Width = 120;
            ok.DialogResult = DialogResult.OK;
            var cancel = NewButton("Отмена", BgInput, null);
            cancel.Width = 120;
            cancel.DialogResult = DialogResult.Cancel;
            buttons.Controls.Add(ok);
            buttons.Controls.Add(cancel);

            // Таблица полей (занимает остальное место)
            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = fields.Length
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var boxes = new List<TextBox>();
            foreach (var field in fields)
            {
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, rowH));
                var lbl = new Label
                {
                    Text = field.Label,
                    ForeColor = FgMuted,
                    TextAlign = ContentAlignment.MiddleLeft,
                    AutoSize = false,
                    Dock = DockStyle.Fill
                };
                var box = new TextBox
                {
                    Text = field.Value,
                    BackColor = BgInput,
                    ForeColor = FgText,
                    BorderStyle = BorderStyle.FixedSingle,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0, 6, 0, 6)
                };
                boxes.Add(box);
                table.Controls.Add(lbl);
                table.Controls.Add(box);
            }

            f.Controls.Add(table);
            f.Controls.Add(buttons);
            f.AcceptButton = ok;
            f.CancelButton = cancel;

            bool result = f.ShowDialog(this) == DialogResult.OK;
            if (result)
                for (int i = 0; i < fields.Length; i++)
                    fields[i].Value = boxes[i].Text;
            f.Dispose();
            return result;
        }

        private string Pick(string question, string[] items)
        {
            var f = new Form
            {
                Text = question,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MinimizeBox = false, MaximizeBox = false,
                BackColor = BgDark, ForeColor = FgText,
                Font = new Font("Segoe UI", 10),
                ClientSize = new Size(400, 150),
                Padding = new Padding(16)
            };

            var cb = new ComboBox
            {
                Dock = DockStyle.Top,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = BgInput,
                ForeColor = FgText,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 0, 12)
            };
            cb.Items.AddRange(items);
            if (items.Length > 0) cb.SelectedIndex = 0;

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 48
            };
            var ok = NewButton("ОК", Accent, null);
            ok.Width = 110;
            ok.DialogResult = DialogResult.OK;
            var cancel = NewButton("Отмена", BgInput, null);
            cancel.Width = 110;
            cancel.DialogResult = DialogResult.Cancel;
            buttons.Controls.Add(ok);
            buttons.Controls.Add(cancel);

            f.Controls.Add(cb);
            f.Controls.Add(buttons);
            f.AcceptButton = ok;
            f.CancelButton = cancel;

            string value = f.ShowDialog(this) == DialogResult.OK ? cb.SelectedItem?.ToString() : null;
            f.Dispose();
            return value;
        }

        private bool Confirm(string text) =>
            MessageBox.Show(text, "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

        private void Info(string text) =>
            MessageBox.Show(text, "Библиотека", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
