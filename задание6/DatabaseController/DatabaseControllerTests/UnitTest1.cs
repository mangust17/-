using Microsoft.VisualStudio.TestTools.UnitTesting;
using DatabaseController;
using System;

namespace DatabaseControllerTests
{
    [TestClass]
    public class TestDatabase
    {
        private string sqlConnectionString = "Server=MANGUST17;Database=praktika;Trusted_Connection=True;";
        private Database db;

        [TestInitialize]
        public void Setup()
        {
            db = Database.GetInstance(sqlConnectionString);
        }

        [TestMethod]
        public void TestExecute()
        {
            string query = "INSERT INTO Test (login, password) VALUES (@login, @password)";
            Parameter[] parameters = {
                new Parameter("login", "Matvey"),
                new Parameter("password", "Leyko")
            };
            int rows = db.Execute(query, parameters);
            Assert.AreEqual(1, rows, "Должна быть затронута 1 строка.");
        }

        [TestMethod]
        public void TestGetScalar()
        {
            string query = "SELECT COUNT(*) FROM Test";
            object result = db.GetScalar(query);
            Assert.IsNotNull(result, "Результат не должен быть null");
        }

        [TestMethod]
        public void TestGetRowsData()
        {
            string query = "SELECT id, login FROM Test";
            object[][] result = db.GetRowsData(query);
            Assert.IsTrue(result.Length > 0, "Должны быть возвращены строки из таблицы.");
        }

        [TestMethod]
        public void TestUpdateRecord()
        {
            string query = "UPDATE Test SET login = @newLogin WHERE login = @oldLogin";
            Parameter[] parameters = {
            new Parameter("newLogin", "UpdatedMatvey"),
            new Parameter("oldLogin", "Matvey")
        };
            int rows = db.Execute(query, parameters);
            Assert.AreEqual(1, rows, "Должна быть затронута 1 строка.");
        }

        [TestMethod]
        public void TestDeleteRecord()
        {
            string insertQuery = "INSERT INTO Test (login, password) VALUES (@login, @password)";
            Parameter[] parameters = {
            new Parameter("login", "ToDelete"),
            new Parameter("password", "password")
        };
            db.Execute(insertQuery, parameters);

            string deleteQuery = "DELETE FROM Test WHERE login = @login";
            Parameter[] parameterss = { new Parameter("login", "ToDelete") };
            int rows = db.Execute(deleteQuery, parameterss);
            Assert.AreEqual(1, rows, "Должна быть удалена 1 строка.");
        }
    }
}
