using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Reflection.Emit;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace sensade_project.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParkingAreaController : ControllerBase
    {
        private Repository _repo;

        public ParkingAreaController()
        {
            _repo = new Repository();
        }

        // GET: api/<ParkingAreaController>/Areas
        [HttpGet ("Areas")]
        public ActionResult<IEnumerable<ParkingArea>> Get()
        {
            ActionResult<IEnumerable<ParkingArea>> areas = _repo.GetAllAreasWithoutSpaces();
            return Ok(areas);
        }

        // GET api/<ParkingAreaController>/5
        [HttpGet("{areaID}")]
        public ActionResult<ParkingArea> Get(int areaID)
        {

            ActionResult<ParkingArea> area = _repo.SingleParkingAreaWithSpaces(areaID);
            if(area == null)
            {
                area = NotFound();
            }
            else
            {
                area = Ok(area);
            }
            return area;
        }

        // POST api/<ParkingAreaController>
        [HttpPost]
        public ActionResult<ParkingArea> Post([FromBody] ParkingArea newArea)
        {
            ActionResult<ParkingArea> result = null;
            bool success = _repo.CreateParkingArea(newArea.Street, newArea.City, newArea.ZipCode, newArea.Latitude, newArea.Longitude);
            if(!success)
            {
                result = BadRequest("Unable to create parking area");
            }
            else
            {
                result = CreatedAtAction(nameof(Get), new { areaID = newArea.AreaID }, newArea);
            }

            return result;
        }

        // PUT api/<ParkingAreaController>/5
        [HttpPut("{areaId}")]
        public ActionResult Put(int areaId, [FromBody] ParkingArea updatedArea)
        {
            ActionResult result = null;

            if (areaId != updatedArea.AreaID)
            {
                result = BadRequest("Area ID in URL and body do not match");
            }
            else
            {
                bool success = _repo.UpdateArea(areaId, updatedArea.Street, updatedArea.City, updatedArea.ZipCode, updatedArea.Latitude, updatedArea.Longitude);

                if (!success)
                {
                    result = NotFound();
                }
                else
                {
                    result = NoContent(); // 204 No Content if the update is successful
                }
            }
            return result;
        }

        // DELETE api/<ParkingAreaController>/5
        [HttpDelete("{areaID}")]
        public ActionResult Delete(int areaID)
        {
            ActionResult result = null;
            bool success = _repo.DeleteArea(areaID);
            if (!success)
            {
                result = NotFound();
            }
            else
            {
                result = NoContent();
            }
            return result;
        }

        // GET api/ParkingArea/{areaID}/spaces
        [HttpGet("{areaID}/spaces")]
        public ActionResult<int> GetTotalParkingSpaceForSingleArea(int areaID)
        {
            ActionResult result = null;
            ActionResult<ParkingArea> area = _repo.SingleParkingAreaWithSpaces(areaID);
            if (area == null)
            {
                result = NotFound();
            }
            else
            {
                result = Ok(area.Value.MaxSpace);
            }
            return result;
        }

        // GET api/ParkingArea/{areaID}
        [HttpGet("{areaID}/free-spaces")]
        public ActionResult<int> GetTotalFreeParkingSpaceForSingleArea(int areaID)
        {
            ActionResult result = null;
            ActionResult<ParkingArea> area = _repo.SingleParkingAreaWithSpaces(areaID);
            if (area == null)
            {
                result = NotFound();
            }
            else
            {
                ParkingArea specificArea = area.Value;
                int freeSpace = specificArea.MaxSpace - specificArea.Spaces.Count;
                result = Ok(freeSpace);
            }
            return result;
        }

    }
}
