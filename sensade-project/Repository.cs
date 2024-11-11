using Microsoft.AspNetCore.Mvc.Formatters;
using Npgsql;
using System.IO;
using System.Reflection.Emit;

namespace sensade_project
{
    public class Repository
    {
        private readonly string _connection;
        public Repository()
        {
            _connection = "Host=localhost;Port=5432;Username=postgres;Password=mysecretpassword;Database=mydatabase";
        }

        public bool InitializeDatabase()
        {
            bool sucess = false;
            using (NpgsqlConnection connection = new NpgsqlConnection(_connection))
            {
                connection.Open();

                var createParkingAreaTable = @"CREATE TABLE IF NOT EXISTS ParkingArea (
                    AreaID integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    Street VARCHAR(100),
                    City VARCHAR(100),
                    ZipCode integer,
                    Latitude numeric,
                    Longitude numeric
                );";

                var createParkingSpaceTable = @"
                CREATE TABLE IF NOT EXISTS ParkingSpace (
                    SpaceID integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    Status varchar(20),
                    SpaceNumber integer,
                    areaID integer,
                    FOREIGN KEY (areaID) REFERENCES ParkingArea (AreaID) ON DELETE CASCADE ON UPDATE CASCADE
                );";

                using (var command = new NpgsqlCommand(createParkingAreaTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new NpgsqlCommand(createParkingSpaceTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                sucess = true;
            }
            return sucess;
        }

        // AREA METHODS
        public bool CreateParkingArea(string street, string city, int zipCode, decimal latitude, decimal longitude)
        {
            bool result = false;
            DatabaseHelper dbHelper = new DatabaseHelper(_connection);

            // Insert a ParkingArea 
            var query = "INSERT INTO ParkingArea (Street, City, ZipCode, Latitude, Longitude) VALUES (@street, @city, @zipCode, @latitude, @longitude)";
            dbHelper.ExecuteNonQuery(query,
                new NpgsqlParameter("@street", street),
                new NpgsqlParameter("@city", city),
                new NpgsqlParameter("@zipCode", zipCode),
                new NpgsqlParameter("@latitude", latitude),
                new NpgsqlParameter("@longitude", longitude)
            );
            result = true;
            return result;
        }

        public bool UpdateArea(int areaId, string street, string city, int zipCode, decimal latitude, decimal longitude)
        {
            bool result = false;
            const string query = @"
            UPDATE ParkingArea
            SET Street = @street, City = @city, ZipCode = @zipCode, Latitude = @latitude, Longitude = @longitude
            WHERE AreaID = @areaId;";

            DatabaseHelper dbHelper = new DatabaseHelper(_connection);
            dbHelper.ExecuteNonQuery(query,
                new NpgsqlParameter("@areaId", areaId),
                new NpgsqlParameter("@street", street),
                new NpgsqlParameter("@city", city),
                new NpgsqlParameter("@zipCode", zipCode),
                new NpgsqlParameter("@latitude", latitude),
                new NpgsqlParameter("@longitude", longitude)
            );
            result = true; 
            return result;
        }

        public ParkingArea SingleParkingAreaWithSpaces(int areaID)
        {
            DatabaseHelper dbHelper = new DatabaseHelper(_connection);
            // Get Area w/wo Parking spaces
            const string selectQuery = @"SELECT * FROM ParkingArea LEFT JOIN ParkingSpace ON ParkingArea.AreaID = ParkingSpace.AreaID WHERE ParkingArea.AreaID = @areaID";

            var result = dbHelper.ExecuteQuery(selectQuery, new NpgsqlParameter("@areaID", areaID));

            // check if parkingspaces attributes is null (in case a area exist but without any spaces)
            bool IsThisNull_Spaceid = result[0]["spaceid"] == DBNull.Value;
            bool IsThisNull_status = result[0]["status"] == DBNull.Value;
            bool IsThisNull_spacenumber = result[0]["spacenumber"] == DBNull.Value;
            bool IsThisNull_areaid = result[0]["areaid"] == DBNull.Value;
            
            
            ParkingArea area = null;
            List<ParkingSpace> spaces = new List<ParkingSpace>();
            if (!IsThisNull_Spaceid && !IsThisNull_status && !IsThisNull_spacenumber && !IsThisNull_areaid)
            {
                for (int i = 0; i < result.Count; i++)
                {
                    ParkingSpace space = new ParkingSpace((int)result[i]["areaid"]);
                    space.SpaceID = (int)result[i]["spaceid"];
                    space.Status = (string)result[i]["status"];
                    space.SpaceNumber = (int)result[i]["spacenumber"];
                    spaces.Add(space);
                }
            }

            // making conversion from object type from the result variable
            area = new ParkingArea(spaces);
            area.AreaID = result[0]["areaid"] == DBNull.Value ? areaID: (int)result[0]["areaid"];
            area.Street = (string)result[0]["street"];
            area.City = (string)result[0]["city"];
            area.ZipCode = (int)result[0]["zipcode"];
            area.Latitude = (decimal)result[0]["latitude"];
            area.Longitude = (decimal)result[0]["longitude"];

            return area;
        }

        public List<ParkingArea> GetAllAreasWithoutSpaces()
        {
            const string query = "SELECT * FROM ParkingArea;";
            DatabaseHelper dbHelper = new DatabaseHelper(_connection);
            var result = dbHelper.ExecuteQuery(query);

            List<ParkingArea> areas = new List<ParkingArea>();
            for (int i = 0; i < result.Count; i++)
            {
                ParkingArea area = new ParkingArea();
                area.AreaID = (int)result[i]["areaid"];
                area.Street = (string)result[i]["street"];
                area.City = (string)result[i]["city"];
                area.ZipCode = (int)result[i]["zipcode"];
                area.Latitude = (decimal)result[i]["latitude"];
                area.Longitude = (decimal)result[i]["longitude"];

                areas.Add(area);
            }
            return areas;
        }

        public bool DeleteArea(int areaId)
        {
            bool toReturn = false;
            const string query = "DELETE FROM ParkingArea WHERE AreaID = @areaId;";
            DatabaseHelper dbHelper = new DatabaseHelper(_connection);
            var result = dbHelper.ExecuteNonQuery(query, new NpgsqlParameter ("AreaID", areaId));
            if (result == 1)
            {
                toReturn = true;
            }
            return toReturn;
        
        }

        // SPACE METHODS
        public bool CreateParkingSpace(string status, int spacenumber, int areaID)
        {
            bool toReturn = false;
            int result = -1;
            DatabaseHelper dbHelper = new DatabaseHelper(_connection);

            // Insert a Parkingspace 
            var insertParkingSpaceQuery = "INSERT INTO ParkingSpace (Status, SpaceNumber, areaID) VALUES (@Status, @SpaceNumber, @areaID)";
            result = dbHelper.ExecuteNonQuery(insertParkingSpaceQuery,
                new NpgsqlParameter("@Status", status),
                new NpgsqlParameter("@SpaceNumber", spacenumber),
                new NpgsqlParameter("areaID", areaID)
            );

            if (result == 1)
            {
                toReturn = true;
            }

            return toReturn;
        }

        public ParkingSpace SingleParkingSpace(int spaceID)
        {
            DatabaseHelper dbHelper = new DatabaseHelper(_connection);
            var selectQuery = @"SELECT SpaceID,Status,SpaceNumber,areaID from ParkingSpace where spaceID = @spaceID";

            var result = dbHelper.ExecuteQuery(selectQuery, new NpgsqlParameter("@spaceID", spaceID));

            ParkingSpace space = null;
            if (result.Count() == 1)
            {
                space = new ParkingSpace((int)result[0]["areaid"]);
                space.SpaceID = (int)result[0]["spaceid"];
                space.Status = (string)result[0]["status"];
                space.SpaceNumber = (int)result[0]["spacenumber"];
            }
            return space;
        }

        public bool UpdateSpace(int spaceId, string status, int spaceNumber, int areaId)
        {
            bool toReturn = false;
            const string query = @"
            UPDATE ParkingSpace
            SET Status = @status, SpaceNumber = @spaceNumber, AreaID = @areaId
            WHERE SpaceID = @spaceId;";

            DatabaseHelper dbHelper = new DatabaseHelper(_connection);
            var result = dbHelper.ExecuteNonQuery(query,
                new NpgsqlParameter("@spaceId", spaceId),
                new NpgsqlParameter("@status", status),
                new NpgsqlParameter("@spaceNumber", spaceNumber),
                new NpgsqlParameter("@areaID", areaId)
            );

            if (result == 1)
            {
                toReturn = true;
            }
            return toReturn;
        }

        public List<ParkingSpace> GetAllSpacesForSpecificArea()
        {
            const string query = @"SELECT subquery1.AreaID, subquery1.SpaceID, subquery1.Status, subquery1.SpaceNumber FROM 
                                    (SELECT AreaID, SpaceID, Status, SpaceNumber FROM ParkingSpace WHERE AreaID = @areaId) AS subquery1 JOIN
                                    (SELECT AreaID FROM ParkingArea WHERE AreaID = @areaId) AS subquery2 ON subquery1.AreaID = subquery2.AreaID;";
            DatabaseHelper dbHelper = new DatabaseHelper(_connection);
            var result = dbHelper.ExecuteQuery(query);

            var spaces = new List<ParkingSpace>();
            for (int i = 0; i < result.Count; i++)
            {
                ParkingSpace space = new ParkingSpace((int)result[i]["areaid"]);
                space.SpaceID = (int)result[i]["spaceid"];
                space.Status = (string)result[i]["status"];
                space.SpaceNumber = (int)result[i]["spacenumber"];
                spaces.Add(space);
            }

            return spaces;
        }

        public bool DeleteSpace(int spaceId)
        {
            bool toReturn = false;
            const string query = "DELETE FROM ParkingSpace WHERE SpaceID = @spaceId;";
            DatabaseHelper dbHelper = new DatabaseHelper(_connection);
            var result = dbHelper.ExecuteNonQuery(query, new NpgsqlParameter("spaceId", spaceId));
            if(result == 1)
            { 
                toReturn = true; 
            }
            return toReturn;
        }
    }
}
