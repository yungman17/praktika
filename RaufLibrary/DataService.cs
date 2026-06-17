// ============================================================
//  Рауф — Итоговый проект: "Система управления библиотекой"
//  Файл: DataService.cs — сохранение данных в JSON (лекция 6.1)
// ============================================================
using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace LibrarySystem
{
    public static class DataService
    {
        private static readonly string FilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "library.json");

        private static readonly JsonSerializerOptions Options =
            new JsonSerializerOptions { WriteIndented = true };

        // Загрузка данных из JSON-файла
        public static LibraryData Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    string json = File.ReadAllText(FilePath);
                    return JsonSerializer.Deserialize<LibraryData>(json) ?? new LibraryData();
                }
            }
            catch
            {
                // если файл повреждён — начинаем с пустой базы
            }
            return CreateDemo(); // при первом запуске — демо-данные
        }

        // Сохранение данных в JSON-файл
        public static void Save(LibraryData data)
        {
            File.WriteAllText(FilePath, JsonSerializer.Serialize(data, Options));
        }

        // Демо-данные: 15 книг, 8 читателей, несколько выдач
        public static LibraryData CreateDemo()
        {
            var d = new LibraryData();
            int bookId = 1;
            void AddBook(string title, string author, string genre, int year, string isbn)
                => d.Books.Add(new Book { Id = bookId++, Title = title, Author = author, Genre = genre, Year = year, Isbn = isbn });

            AddBook("Война и мир", "Л. Толстой", "Роман", 1869, "978-5-389-06256-9");
            AddBook("Мастер и Маргарита", "М. Булгаков", "Роман", 1967, "978-5-699-12014-7");
            AddBook("Преступление и наказание", "Ф. Достоевский", "Роман", 1866, "978-5-04-099865-3");
            AddBook("Евгений Онегин", "А. Пушкин", "Поэма", 1833, "978-5-17-090108-1");
            AddBook("Отцы и дети", "И. Тургенев", "Роман", 1862, "978-5-08-005120-4");
            AddBook("Чистый код", "Р. Мартин", "IT", 2008, "978-5-496-00487-9");
            AddBook("Совершенный код", "С. Макконнелл", "IT", 2004, "978-5-91180-948-7");
            AddBook("Грокаем алгоритмы", "А. Бхаргава", "IT", 2016, "978-5-496-02541-6");
            AddBook("1984", "Дж. Оруэлл", "Антиутопия", 1949, "978-5-17-080115-2");
            AddBook("Задача трёх тел", "Лю Цысинь", "Фантастика", 2008, "978-5-04-103579-3");
            AddBook("Гарри Поттер и философский камень", "Дж. Роулинг", "Фэнтези", 1997, "978-5-389-07435-4");
            AddBook("Маленький принц", "А. де Сент-Экзюпери", "Сказка", 1943, "978-5-699-94837-1");
            AddBook("Приключения Шерлока Холмса", "А. Конан Дойл", "Детектив", 1892, "978-5-389-07717-1");
            AddBook("Краткая история времени", "С. Хокинг", "Наука", 1988, "978-5-17-094412-5");
            AddBook("Думай медленно... решай быстро", "Д. Канеман", "Психология", 2011, "978-5-17-080053-7");

            // --- Читатели ---
            int readerId = 1;
            void AddReader(string name, string phone)
                => d.Readers.Add(new Reader { Id = readerId++, Name = name, Phone = phone });

            AddReader("Иванов Иван Иванович", "+7 900 111-22-33");
            AddReader("Петрова Анна Сергеевна", "+7 901 222-33-44");
            AddReader("Смирнов Олег Павлович", "+7 902 333-44-55");
            AddReader("Кузнецова Мария Дмитриевна", "+7 903 444-55-66");
            AddReader("Соколов Артём Игоревич", "+7 904 555-66-77");
            AddReader("Попова Екатерина Андреевна", "+7 905 666-77-88");
            AddReader("Лебедев Дмитрий Николаевич", "+7 906 777-88-99");
            AddReader("Новикова Ольга Викторовна", "+7 907 888-99-00");

            // --- Выдачи ---
            var now = DateTime.Now;
            int loanId = 1;
            void AddLoan(int bId, int rId, int issuedDaysAgo, int returnedDaysAgo = -1)
            {
                var loan = new Loan
                {
                    Id = loanId++,
                    BookId = bId,
                    ReaderId = rId,
                    LoanDate = now.AddDays(-issuedDaysAgo),
                    DueDate = now.AddDays(-issuedDaysAgo + 14)
                };
                if (returnedDaysAgo >= 0)
                    loan.ReturnDate = now.AddDays(-returnedDaysAgo); // книга возвращена
                else
                    d.Books.First(b => b.Id == bId).IsAvailable = false; // книга на руках
                d.Loans.Add(loan);
            }

            // Активные выдачи (книги на руках)
            AddLoan(1, 1, 5);    // Война и мир — на руках, в срок
            AddLoan(6, 2, 20);   // Чистый код — ПРОСРОЧКА
            AddLoan(9, 3, 30);   // 1984 — ПРОСРОЧКА
            AddLoan(11, 4, 3);   // Гарри Поттер — на руках, в срок
            AddLoan(3, 5, 8);    // Преступление и наказание — на руках

            // Возвращённые ранее (для статистики «популярные книги»)
            AddLoan(1, 6, 60, 50);   // Война и мир (2-я выдача)
            AddLoan(6, 7, 45, 35);   // Чистый код (2-я выдача)
            AddLoan(11, 8, 40, 33);  // Гарри Поттер (2-я выдача)

            return d;
        }
    }
}
