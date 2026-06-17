// ============================================================
//  Рауф — Итоговый проект: "Система управления библиотекой"
//  Файл: Models.cs — модели данных (слой Model из лекции 6.1)
// ============================================================
using System;
using System.Collections.Generic;

namespace LibrarySystem
{
    // Книга
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Isbn { get; set; }
        public string Genre { get; set; }
        public int Year { get; set; }
        public bool IsAvailable { get; set; } = true;
    }

    // Читатель
    public class Reader
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public DateTime RegDate { get; set; } = DateTime.Now;
    }

    // Выдача книги
    public class Loan
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int ReaderId { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        public bool IsReturned => ReturnDate != null;
        public bool IsOverdue => !IsReturned && DateTime.Now > DueDate;
    }

    // Вся база данных библиотеки (сохраняется в JSON)
    public class LibraryData
    {
        public List<Book> Books { get; set; } = new List<Book>();
        public List<Reader> Readers { get; set; } = new List<Reader>();
        public List<Loan> Loans { get; set; } = new List<Loan>();
    }
}
