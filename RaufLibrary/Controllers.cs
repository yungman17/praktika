// ============================================================
//  Рауф — Итоговый проект: "Система управления библиотекой"
//  Файл: Controllers.cs — слой Controller (архитектура из 6.1)
//  BookController, ReaderController, LoanController
// ============================================================
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibrarySystem
{
    // ---------- Контроллер книг: CRUD, поиск ----------
    public class BookController
    {
        private readonly LibraryData data;
        public BookController(LibraryData data) { this.data = data; }

        public List<Book> GetAll() => data.Books;

        // Поиск по названию, автору или ISBN
        public List<Book> Search(string query)
        {
            string q = (query ?? "").Trim().ToLower();
            if (q == "") return data.Books;
            return data.Books.Where(b =>
                (b.Title ?? "").ToLower().Contains(q) ||
                (b.Author ?? "").ToLower().Contains(q) ||
                (b.Isbn ?? "").ToLower().Contains(q)).ToList();
        }

        public Book Add(string title, string author, string genre, int year, string isbn)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Название книги обязательно");
            if (string.IsNullOrWhiteSpace(author))
                throw new ArgumentException("Автор обязателен");

            var book = new Book
            {
                Id = data.Books.Count == 0 ? 1 : data.Books.Max(b => b.Id) + 1,
                Title = title.Trim(),
                Author = author.Trim(),
                Genre = genre?.Trim(),
                Year = year,
                Isbn = isbn?.Trim()
            };
            data.Books.Add(book);
            return book;
        }

        public void Edit(int id, string title, string author, string genre, int year, string isbn)
        {
            var book = data.Books.FirstOrDefault(b => b.Id == id)
                       ?? throw new ArgumentException("Книга не найдена");
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Название книги обязательно");
            book.Title = title.Trim();
            book.Author = author?.Trim();
            book.Genre = genre?.Trim();
            book.Year = year;
            book.Isbn = isbn?.Trim();
        }

        public void Delete(int id)
        {
            var book = data.Books.FirstOrDefault(b => b.Id == id)
                       ?? throw new ArgumentException("Книга не найдена");
            if (!book.IsAvailable)
                throw new InvalidOperationException("Нельзя удалить книгу, которая выдана читателю");
            data.Books.Remove(book);
        }
    }

    // ---------- Контроллер читателей ----------
    public class ReaderController
    {
        private readonly LibraryData data;
        public ReaderController(LibraryData data) { this.data = data; }

        public List<Reader> GetAll() => data.Readers;

        public Reader Add(string name, string phone)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("ФИО читателя обязательно");

            var reader = new Reader
            {
                Id = data.Readers.Count == 0 ? 1 : data.Readers.Max(r => r.Id) + 1,
                Name = name.Trim(),
                Phone = phone?.Trim()
            };
            data.Readers.Add(reader);
            return reader;
        }

        public void Delete(int id)
        {
            var reader = data.Readers.FirstOrDefault(r => r.Id == id)
                         ?? throw new ArgumentException("Читатель не найден");
            if (data.Loans.Any(l => l.ReaderId == id && !l.IsReturned))
                throw new InvalidOperationException("У читателя есть невозвращённые книги");
            data.Readers.Remove(reader);
        }
    }

    // ---------- Контроллер выдачи: выдача, возврат, штрафы ----------
    public class LoanController
    {
        public const int LoanDays = 14;     // срок выдачи
        public const int FinePerDay = 10;   // штраф, руб/день

        private readonly LibraryData data;
        public LoanController(LibraryData data) { this.data = data; }

        public List<Loan> GetAll() => data.Loans;

        public Loan Give(int bookId, int readerId)
        {
            var book = data.Books.FirstOrDefault(b => b.Id == bookId)
                       ?? throw new ArgumentException("Книга не найдена");
            if (!book.IsAvailable)
                throw new InvalidOperationException("Книга уже выдана");
            if (!data.Readers.Any(r => r.Id == readerId))
                throw new ArgumentException("Читатель не найден");

            var loan = new Loan
            {
                Id = data.Loans.Count == 0 ? 1 : data.Loans.Max(l => l.Id) + 1,
                BookId = bookId,
                ReaderId = readerId,
                LoanDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(LoanDays)
            };
            data.Loans.Add(loan);
            book.IsAvailable = false;
            return loan;
        }

        // Возврат книги. Возвращает сумму штрафа (0, если без просрочки)
        public int Return(int loanId)
        {
            var loan = data.Loans.FirstOrDefault(l => l.Id == loanId)
                       ?? throw new ArgumentException("Выдача не найдена");
            if (loan.IsReturned)
                throw new InvalidOperationException("Книга уже возвращена");

            loan.ReturnDate = DateTime.Now;
            var book = data.Books.FirstOrDefault(b => b.Id == loan.BookId);
            if (book != null) book.IsAvailable = true;

            return CalcFine(loan);
        }

        // Расчёт штрафа за просрочку
        public int CalcFine(Loan loan)
        {
            DateTime end = loan.ReturnDate ?? DateTime.Now;
            if (end <= loan.DueDate) return 0;
            return (end - loan.DueDate).Days * FinePerDay;
        }

        // Список должников (просроченные и не возвращённые)
        public List<Loan> GetOverdue() =>
            data.Loans.Where(l => l.IsOverdue).ToList();
    }
}
