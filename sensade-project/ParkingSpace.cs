namespace sensade_project
{
    public class ParkingSpace : IParkingSpaceArea
    {
        private string _status;
        public ParkingSpace(int areaID)
        {
            AreaID = areaID;
            _status = string.Empty;
        }
        public int SpaceID { get; set; }
        public string Status 
        { 
            get
            {
                return _status;
            }
            set
            {
                if (!value.Equals("occupied", StringComparison.CurrentCultureIgnoreCase) && !value.Equals("free", StringComparison.CurrentCultureIgnoreCase))
                {
                    return;
                }

                _status = value;
            }
        }

        public int SpaceNumber { get; set; }

        public int AreaID { get; private set; }
    }
}
