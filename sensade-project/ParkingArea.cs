using System.Runtime.InteropServices;

namespace sensade_project
{
    public class ParkingArea: IParkingSpaceArea
    {
        public ParkingArea([Optional] List<ParkingSpace> spaces)
        {
            if (spaces == null)
            {
                if(spaces.Count < MaxSpace)
                {
                    Spaces = new List<ParkingSpace>();
                }
                
            }
            else
            {
                Spaces = spaces;
            }
        }
        public List<ParkingSpace> Spaces { get; private set; }
        public int AreaID { get; set; }
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int ZipCode { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int MaxSpace { get; set; } = 100;
    }
}
