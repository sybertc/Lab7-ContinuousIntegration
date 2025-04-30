using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Lab5_WebAppDevelopment.Models;

namespace Lab5_WebAppDevelopment.Services
{
    public class LibraryService : ILibraryService
    {
        private readonly string _dataFolder;
        private readonly string _booksCsvPath;
        private readonly string _usersCsvPath;

        public List<Book> Books { get; private set; }
        public List<User> Users { get; private set; }
        public Dictionary<User, List<Book>> BorrowedBooks { get; }

        /// <summary>
        /// Default constructor — points at the "Data" folder under the current directory.
        /// </summary>
        public LibraryService()
            : this(Path.Combine(Directory.GetCurrentDirectory(), "Data"))
        {
        }

        /// <summary>
        /// Testable constructor — you can pass in a custom folder (e.g. a temp folder).
        /// </summary>
        public LibraryService(string dataFolder)
        {
            _dataFolder = dataFolder;
            _booksCsvPath = Path.Combine(_dataFolder, "Books.csv");
            _usersCsvPath = Path.Combine(_dataFolder, "Users.csv");

            Books = new List<Book>();
            Users = new List<User>();
            BorrowedBooks = new Dictionary<User, List<Book>>();

            LoadDataFromCsv();
        }

        // ─── CSV Parsing Helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Splits a CSV line on commas not inside quotes.
        /// </summary>
        private string[] ParseCsvLine(string line)
        {
            var regex = new Regex(@",(?=(?:[^""]*""[^""]*"")*[^""]*$)");
            return regex.Split(line);
        }

        /// <summary>
        /// Escapes a field for CSV output: wraps in quotes if needed and doubles any quotes inside.
        /// </summary>
        private string EscapeCsvField(string field)
        {
            if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
            {
                // double any quotes
                var escaped = field.Replace("\"", "\"\"");
                return $"\"{escaped}\"";
            }
            return field;
        }

        // ─── Load ───────────────────────────────────────────────────────────────────

        private void LoadDataFromCsv()
        {
            // -- Books
            if (File.Exists(_booksCsvPath))
            {
                foreach (var line in File.ReadLines(_booksCsvPath))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var fields = ParseCsvLine(line);
                    if (fields.Length < 4) continue;
                    if (!int.TryParse(fields[0].Trim(), out var id)) continue;

                    var title = fields[1].Trim().Trim('"');
                    var author = fields[2].Trim().Trim('"');
                    var isbn = fields[3].Trim().Trim('"');

                    Books.Add(new Book { Id = id, Title = title, Author = author, ISBN = isbn });
                }
            }

            // -- Users
            if (File.Exists(_usersCsvPath))
            {
                foreach (var line in File.ReadLines(_usersCsvPath))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var fields = ParseCsvLine(line);
                    if (fields.Length < 3) continue;
                    if (!int.TryParse(fields[0].Trim(), out var id)) continue;

                    var name = fields[1].Trim().Trim('"');
                    var email = fields[2].Trim().Trim('"');

                    Users.Add(new User { Id = id, Name = name, Email = email });
                }
            }
        }

        // ─── Save Helpers ───────────────────────────────────────────────────────────

        private void SaveBooksToCsv()
        {
            Directory.CreateDirectory(_dataFolder);
            var lines = Books.Select(b =>
                $"{b.Id}," +
                $"{EscapeCsvField(b.Title)}," +
                $"{EscapeCsvField(b.Author)}," +
                $"{EscapeCsvField(b.ISBN)}");
            File.WriteAllLines(_booksCsvPath, lines);
        }

        private void SaveUsersToCsv()
        {
            Directory.CreateDirectory(_dataFolder);
            var lines = Users.Select(u =>
                $"{u.Id}," +
                $"{EscapeCsvField(u.Name)}," +
                $"{EscapeCsvField(u.Email)}");
            File.WriteAllLines(_usersCsvPath, lines);
        }

        // ─── Book CRUD ─────────────────────────────────────────────────────────────

        public void AddBook(Book book)
        {
            book.Id = Books.Any() ? Books.Max(b => b.Id) + 1 : 1;
            Books.Add(book);
            SaveBooksToCsv();
        }

        public void EditBook(Book updatedBook)
        {
            var existing = Books.FirstOrDefault(b => b.Id == updatedBook.Id);
            if (existing != null)
            {
                existing.Title = updatedBook.Title;
                existing.Author = updatedBook.Author;
                existing.ISBN = updatedBook.ISBN;
                SaveBooksToCsv();
            }
        }

        public void DeleteBook(int bookId)
        {
            var book = Books.FirstOrDefault(b => b.Id == bookId);
            if (book != null)
            {
                Books.Remove(book);
                SaveBooksToCsv();
            }
        }

        // ─── User CRUD ─────────────────────────────────────────────────────────────

        public void AddUser(User user)
        {
            user.Id = Users.Any() ? Users.Max(u => u.Id) + 1 : 1;
            Users.Add(user);
            SaveUsersToCsv();
        }

        public void EditUser(User updatedUser)
        {
            var existing = Users.FirstOrDefault(u => u.Id == updatedUser.Id);
            if (existing != null)
            {
                existing.Name = updatedUser.Name;
                existing.Email = updatedUser.Email;
                SaveUsersToCsv();
            }
        }

        public void DeleteUser(int userId)
        {
            var user = Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                Users.Remove(user);
                SaveUsersToCsv();
            }
        }

        // ─── Borrow / Return ───────────────────────────────────────────────────────

        public bool BorrowBook(int bookId, int userId)
        {
            var book = Books.FirstOrDefault(b => b.Id == bookId);
            var user = Users.FirstOrDefault(u => u.Id == userId);
            if (book != null && user != null)
            {
                if (!BorrowedBooks.ContainsKey(user))
                    BorrowedBooks[user] = new List<Book>();
                BorrowedBooks[user].Add(book);
                Books.Remove(book);
                SaveBooksToCsv();
                return true;
            }
            return false;
        }

        public bool ReturnBook(int userId, int borrowedBookIndex)
        {
            var user = Users.FirstOrDefault(u => u.Id == userId);
            if (user != null &&
                BorrowedBooks.ContainsKey(user) &&
                borrowedBookIndex >= 0 &&
                borrowedBookIndex < BorrowedBooks[user].Count)
            {
                var book = BorrowedBooks[user][borrowedBookIndex];
                BorrowedBooks[user].RemoveAt(borrowedBookIndex);
                Books.Add(book);
                SaveBooksToCsv();
                return true;
            }
            return false;
        }
    }
}
