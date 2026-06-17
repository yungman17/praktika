// ============================================================
//  Рауф — Итоговый проект: "Система управления библиотекой"
//  Файл: ControllerTests.cs — юнит-тесты контроллеров
//  (базовое задание из лекции 6.2)
//  Запуск: dotnet test
// ============================================================
using System;
using System.Linq;
using Xunit;
using LibrarySystem;

namespace LibrarySystem.Tests
{
    // ---------- Тесты BookController ----------
    public class BookControllerTests
    {
        private static (LibraryData, BookController) Setup()
        {
            var data = new LibraryData();
            return (data, new BookController(data));
        }

        [Fact]
        public void Add_ДобавляетКнигу()
        {
            var (data, ctrl) = Setup();
            var book = ctrl.Add("Война и мир", "Толстой", "Роман", 1869, "123");

            Assert.Single(data.Books);
            Assert.Equal(1, book.Id);
            Assert.True(book.IsAvailable);
        }

        [Fact]
        public void Add_БезНазвания_Ошибка()
        {
            var (_, ctrl) = Setup();
            Assert.Throws<ArgumentException>(() => ctrl.Add("", "Автор", "", 2020, ""));
        }

        [Fact]
        public void Add_БезАвтора_Ошибка()
        {
            var (_, ctrl) = Setup();
            Assert.Throws<ArgumentException>(() => ctrl.Add("Название", "", "", 2020, ""));
        }

        [Fact]
        public void Search_НаходитПоНазваниюАвторуИIsbn()
        {
            var (_, ctrl) = Setup();
            ctrl.Add("Чистый код", "Мартин", "IT", 2008, "978-5");
            ctrl.Add("Война и мир", "Толстой", "Роман", 1869, "111");

            Assert.Single(ctrl.Search("чистый"));   // по названию
            Assert.Single(ctrl.Search("толстой"));  // по автору
            Assert.Single(ctrl.Search("978"));      // по ISBN
            Assert.Equal(2, ctrl.Search("").Count); // пустой запрос — все
        }

        [Fact]
        public void Edit_МеняетПоля()
        {
            var (_, ctrl) = Setup();
            var book = ctrl.Add("Старое", "Автор", "Жанр", 2000, "");
            ctrl.Edit(book.Id, "Новое", "Новый автор", "IT", 2024, "999");

            Assert.Equal("Новое", book.Title);
            Assert.Equal(2024, book.Year);
            Assert.Equal("999", book.Isbn);
        }

        [Fact]
        public void Delete_ВыданнуюКнигу_Ошибка()
        {
            var (data, ctrl) = Setup();
            var book = ctrl.Add("Книга", "Автор", "", 2020, "");
            book.IsAvailable = false; // книга выдана

            Assert.Throws<InvalidOperationException>(() => ctrl.Delete(book.Id));
            Assert.Single(data.Books); // книга осталась
        }
    }

    // ---------- Тесты ReaderController ----------
    public class ReaderControllerTests
    {
        private static (LibraryData, ReaderController) Setup()
        {
            var data = new LibraryData();
            return (data, new ReaderController(data));
        }

        [Fact]
        public void Add_РегистрируетЧитателя()
        {
            var (data, ctrl) = Setup();
            var reader = ctrl.Add("Иванов Иван", "+7 900");

            Assert.Single(data.Readers);
            Assert.Equal("Иванов Иван", reader.Name);
        }

        [Fact]
        public void Add_БезИмени_Ошибка()
        {
            var (_, ctrl) = Setup();
            Assert.Throws<ArgumentException>(() => ctrl.Add("  ", "123"));
        }

        [Fact]
        public void Delete_СНевозвращеннойКнигой_Ошибка()
        {
            var data = new LibraryData();
            var readerCtrl = new ReaderController(data);
            var bookCtrl = new BookController(data);
            var loanCtrl = new LoanController(data);

            var reader = readerCtrl.Add("Должник", "");
            var book = bookCtrl.Add("Книга", "Автор", "", 2020, "");
            loanCtrl.Give(book.Id, reader.Id);

            Assert.Throws<InvalidOperationException>(() => readerCtrl.Delete(reader.Id));
        }
    }

    // ---------- Тесты LoanController ----------
    public class LoanControllerTests
    {
        private static (LibraryData data, BookController books, ReaderController readers, LoanController loans) Setup()
        {
            var data = new LibraryData();
            return (data, new BookController(data), new ReaderController(data), new LoanController(data));
        }

        [Fact]
        public void Give_ВыдаетКнигуНа14Дней()
        {
            var (_, books, readers, loans) = Setup();
            var book = books.Add("Книга", "Автор", "", 2020, "");
            var reader = readers.Add("Читатель", "");

            var loan = loans.Give(book.Id, reader.Id);

            Assert.False(book.IsAvailable);
            Assert.Equal(LoanController.LoanDays, (loan.DueDate - loan.LoanDate).Days);
        }

        [Fact]
        public void Give_УжеВыданнуюКнигу_Ошибка()
        {
            var (_, books, readers, loans) = Setup();
            var book = books.Add("Книга", "Автор", "", 2020, "");
            var r1 = readers.Add("Первый", "");
            var r2 = readers.Add("Второй", "");
            loans.Give(book.Id, r1.Id);

            Assert.Throws<InvalidOperationException>(() => loans.Give(book.Id, r2.Id));
        }

        [Fact]
        public void Return_ВозвращаетКнигуБезШтрафа()
        {
            var (_, books, readers, loans) = Setup();
            var book = books.Add("Книга", "Автор", "", 2020, "");
            var reader = readers.Add("Читатель", "");
            var loan = loans.Give(book.Id, reader.Id);

            int fine = loans.Return(loan.Id);

            Assert.Equal(0, fine);
            Assert.True(book.IsAvailable);
            Assert.True(loan.IsReturned);
        }

        [Fact]
        public void Return_Повторно_Ошибка()
        {
            var (_, books, readers, loans) = Setup();
            var book = books.Add("Книга", "Автор", "", 2020, "");
            var reader = readers.Add("Читатель", "");
            var loan = loans.Give(book.Id, reader.Id);
            loans.Return(loan.Id);

            Assert.Throws<InvalidOperationException>(() => loans.Return(loan.Id));
        }

        [Fact]
        public void CalcFine_ПросрочкаНаПятьДней_50Рублей()
        {
            var (data, _, _, loans) = Setup();
            var loan = new Loan
            {
                Id = 1,
                LoanDate = DateTime.Now.AddDays(-19),
                DueDate = DateTime.Now.AddDays(-5) // просрочено на 5 дней
            };
            data.Loans.Add(loan);

            Assert.Equal(5 * LoanController.FinePerDay, loans.CalcFine(loan));
        }

        [Fact]
        public void GetOverdue_НаходитТолькоПросроченные()
        {
            var (data, _, _, loans) = Setup();
            data.Loans.Add(new Loan { Id = 1, DueDate = DateTime.Now.AddDays(-3) });          // просрочена
            data.Loans.Add(new Loan { Id = 2, DueDate = DateTime.Now.AddDays(5) });           // в срок
            data.Loans.Add(new Loan { Id = 3, DueDate = DateTime.Now.AddDays(-3), ReturnDate = DateTime.Now }); // возвращена

            var overdue = loans.GetOverdue();
            Assert.Single(overdue);
            Assert.Equal(1, overdue[0].Id);
        }
    }
}
