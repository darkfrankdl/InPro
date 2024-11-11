using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Npgsql;
using NuGet.Frameworks;
using sensade_project;

namespace Tests
{

    public class Test
    {
        // below (connection variable, used for cleanup method) is for easy, quick development of the tests which is what I needed, and this code is not going to github or production
        // but in production should be in appsettings (which then should be ignored by git) or environment varibles in vs or something similar so
        // the connection have some security to it and not hardcoded into the source code. -> security reasons.

        private readonly NpgsqlConnection connection = new NpgsqlConnection("Host=localhost;Port=5432;Username=postgres;Password=mysecretpassword;Database=mydatabase");

        ////////////////////////////////////////////below is setup method for tests, it will make sure the table in the database is created/////////////////
        //public void Setup()
        //{
        //        // arrange
        //        Repository repo = new Repository();
        //        // act
        //        bool sucess = repo.InitializeDatabase();
        //}
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        

        // CLEAN UP METHODS//
        private void Cleanup()
        {
            // Cleanup (delete rows) and reset the ID counter
            DatabaseHelper helper = new DatabaseHelper(connection.ConnectionString);
            CleanUpData(helper);
            ResetIdentity(helper);
        }

        private void CleanUpData(DatabaseHelper helper)
        {
            // Execute DELETE SQL here
            helper.ExecuteNonQuery("DELETE FROM ParkingArea");
        }

        private void ResetIdentity(DatabaseHelper helper)
        {
            // Execute command to reset identity, like DBCC CHECKIDENT in SQL Server
            
            helper.ExecuteNonQuery("SELECT setval((SELECT pg_get_serial_sequence('ParkingArea', 'areaid')), 1, false);");
            helper.ExecuteNonQuery("SELECT setval((SELECT pg_get_serial_sequence('ParkingSpace', 'spaceid')), 1, false);");
        }
        // CLEAN UP METHODS END//
        
        
        
        
        // TESTS BEGIN HERE//
        [Fact]
        public void Test_CreateArea_Expect_True()
        {
            // arrange
            Repository repo = new Repository();
            ParkingArea area = new ParkingArea() { AreaID = 1, City = "Aalborg", ZipCode = 9000, Latitude = 57.0480M, Longitude = 9.9187M };
            // act
            bool success = repo.CreateParkingArea(area.Street, area.City, area.ZipCode, area.Latitude, area.Longitude);
            // assert
            Cleanup();
            Assert.True(success);
        }

        [Fact]
        public void Test_UpdateArea_Expect_True()
        {
            // arrange
            Repository repo = new Repository();
            ParkingArea area = new ParkingArea() { AreaID = 1, City = "Aalborg", ZipCode = 9000, Latitude = 57.0480M, Longitude = 9.9187M };
            ParkingArea updatedarea = new ParkingArea() { AreaID = 1, City = "Aahus", ZipCode = 8000, Latitude = 57.0480M, Longitude = 9.9187M };
            // act
            repo.CreateParkingArea(area.Street, area.City, area.ZipCode, area.Latitude, area.Longitude);
            bool success = repo.UpdateArea(updatedarea.AreaID, updatedarea.Street, updatedarea.City, updatedarea.ZipCode, updatedarea.Latitude, updatedarea.Longitude);
            // assert
            Cleanup();
            Assert.True(success);
        }

        [Fact]
        public void Test_SingleParkingAreaWithSpaces_Expect_NotNull()
        {
            // arrange
            Repository repo = new Repository();
            ParkingArea area = new ParkingArea() { AreaID = 1, City = "Aalborg", ZipCode = 9000, Latitude = 57.0480M, Longitude = 9.9187M };
            repo.CreateParkingArea(area.Street, area.City, area.ZipCode, area.Latitude, area.Longitude);
            repo.CreateParkingSpace("free", 1, area.AreaID);
            // act
            ParkingArea result = repo.SingleParkingAreaWithSpaces(area.AreaID);
            // assert
            Cleanup();
            Assert.NotNull(result);
        }

        [Fact]
        public void Test_GetAllAreasWithoutSpaces_Expect_onlyTwoElements()
        {
            // arrange
            Repository repo = new Repository();
            ParkingArea area = new ParkingArea() { AreaID = 1, City = "Aalborg", ZipCode = 9000, Latitude = 57.0480M, Longitude = 9.9187M };
            ParkingArea area2 = new ParkingArea() { AreaID = 2, City = "Aahus", ZipCode = 8000, Latitude = 57.0480M, Longitude = 9.9187M };
            repo.CreateParkingArea(area.Street, area.City, area.ZipCode, area.Latitude, area.Longitude);
            repo.CreateParkingArea(area2.Street, area2.City, area2.ZipCode, area2.Latitude, area2.Longitude);
            // act
            List<ParkingArea> result = repo.GetAllAreasWithoutSpaces();
            // assert
            Cleanup();
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void Test_DeleteArea_Expect_True()
        {
            // arrange
            Repository repo = new Repository();
            ParkingArea area = new ParkingArea() { AreaID = 1, City = "Aalborg", ZipCode = 9000, Latitude = 57.0480M, Longitude = 9.9187M };
            repo.CreateParkingArea(area.Street, area.City, area.ZipCode, area.Latitude, area.Longitude);
            // act
            bool result = repo.DeleteArea(area.AreaID);
            // assert
            Cleanup();
            Assert.True(result);
        }

        [Fact]
        public void Test_CreateParkingSpace_Expect_True()
        {
            // arrange
            Repository repo = new Repository();
            ParkingArea area = new ParkingArea() { AreaID = 1, City = "Aalborg", ZipCode = 9000, Latitude = 57.0480M, Longitude = 9.9187M };
            //ParkingSpace space = new ParkingSpace(area.AreaID) { SpaceID = 1, SpaceNumber = 1, Status = "free"};
            ParkingSpace space = new ParkingSpace(area.AreaID);
            space.SpaceID = 1;
            space.SpaceNumber = 1;
            space.Status = "free";
            repo.CreateParkingArea(area.Street, area.City, area.ZipCode, area.Latitude, area.Longitude);
            // act
            bool result = repo.CreateParkingSpace(space.Status, space.SpaceNumber, space.AreaID);
            // assert
            Cleanup();
            Assert.True(result);
        }

        [Fact]
        public void Test_SingleParkingSpace_Expect_NotNull()
        {
            // arrange
            Repository repo = new Repository();
            ParkingArea area = new ParkingArea() { AreaID = 1, City = "Aalborg", ZipCode = 9000, Latitude = 57.0480M, Longitude = 9.9187M };
            ParkingSpace space = new ParkingSpace(area.AreaID) { SpaceID = 1, SpaceNumber = 1, Status = "free" };
            repo.CreateParkingArea(area.Street, area.City, area.ZipCode, area.Latitude, area.Longitude);
            repo.CreateParkingSpace(space.Status, space.SpaceNumber, space.AreaID);
            // act
            ParkingSpace result = repo.SingleParkingSpace(space.SpaceID);
            // assert
            Cleanup();
            Assert.NotNull(result);
        }

        [Fact]
        public void Test_UpdateSpace_Expect_True()
        {
            // arrange
            Repository repo = new Repository();
            ParkingArea area = new ParkingArea() { AreaID = 1, City = "Aalborg", ZipCode = 9000, Latitude = 57.0480M, Longitude = 9.9187M };
            ParkingSpace space = new ParkingSpace(area.AreaID) { SpaceID = 1, SpaceNumber = 1, Status = "free" };
            ParkingSpace updatedSpace = new ParkingSpace(area.AreaID) { SpaceID = 1, SpaceNumber = 2, Status = "occupied" };
            repo.CreateParkingArea(area.Street, area.City, area.ZipCode, area.Latitude, area.Longitude);
            repo.CreateParkingSpace(space.Status, space.SpaceNumber, space.AreaID);
            // act
            bool result = repo.UpdateSpace(updatedSpace.SpaceID, updatedSpace.Status, updatedSpace.SpaceNumber, updatedSpace.AreaID);
            // assert
            Cleanup();
            Assert.True(result);
        }

        [Fact]
        public void Test_GetAllSpacesForSpecificArea_Expect_TwoElements()
        {
            // arrange
            Repository repo = new Repository();
            ParkingArea area = new ParkingArea() { AreaID = 1, City = "Aalborg", ZipCode = 9000, Latitude = 57.0480M, Longitude = 9.9187M };
            ParkingSpace space = new ParkingSpace(area.AreaID) { SpaceID = 1, SpaceNumber = 1, Status = "free" };
            ParkingSpace space2 = new ParkingSpace(area.AreaID) { SpaceID = 2, SpaceNumber = 2, Status = "occupied" };
            repo.CreateParkingArea(area.Street, area.City, area.ZipCode, area.Latitude, area.Longitude);
            repo.CreateParkingSpace(space.Status, space.SpaceNumber, space.AreaID);
            repo.CreateParkingSpace(space2.Status, space2.SpaceNumber, space2.AreaID);
            // act
            List<ParkingSpace> result = repo.GetAllSpacesForSpecificArea();
            // assert
            Cleanup();
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void Test_DeleteSpace_Expect_True()
        {
            // arrange
            Repository repo = new Repository();
            ParkingArea area = new ParkingArea() { AreaID = 1, City = "Aalborg", ZipCode = 9000, Latitude = 57.0480M, Longitude = 9.9187M };
            ParkingSpace space = new ParkingSpace(area.AreaID) { SpaceID = 1, SpaceNumber = 1, Status = "free" };
            repo.CreateParkingArea(area.Street, area.City, area.ZipCode, area.Latitude, area.Longitude);
            repo.CreateParkingSpace(space.Status, space.SpaceNumber, space.AreaID);
            // act
            bool result = repo.DeleteSpace(space.SpaceID);
            // assert
            Cleanup();
            Assert.True(result);
        }
    }
}
