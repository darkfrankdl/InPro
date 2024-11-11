using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace sensade_project.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParkingSpaceController : ControllerBase
    {
        private Repository _repo;

        public ParkingSpaceController()
        {
            _repo = new Repository();
        }

        // GET: api/<ParkingSpaceController>/Spaces
        [HttpGet("Spaces")]
        public ActionResult<IEnumerable<ParkingSpace>> Get()
        {
            ActionResult<IEnumerable<ParkingSpace>> spaces = _repo.GetAllSpacesForSpecificArea();
            return Ok(spaces);
        }

        // GET api/<ParkingSpaceController>/5
        [HttpGet("{spaceID}")]
        public ActionResult<ParkingSpace> Get(int spaceID)
        {

            ActionResult<ParkingSpace> space = _repo.SingleParkingSpace(spaceID);
            if (space == null)
            {
                space = NotFound();
            }
            else
            {
                space = Ok(space);
            }
            return space;
        }

        // POST api/<ParkingSpaceController>
        [HttpPost]
        public ActionResult<ParkingSpace> Post([FromBody] ParkingSpace newSpace)
        {
            ActionResult<ParkingSpace> result = null;
            bool success = _repo.CreateParkingSpace(newSpace.Status, newSpace.SpaceNumber, newSpace.AreaID);
            if (!success)
            {
                result = BadRequest("Unable to create parking space");
            }
            else
            {
                result = CreatedAtAction(nameof(Get), new { spaceID = newSpace.SpaceID }, newSpace);
            }

            return result;
        }

        // PUT api/<ParkingSpaceController>/5
        [HttpPut("{spaceID}")]
        public ActionResult Put(int spaceID,[FromBody] ParkingSpace updatedSpace)
        {
            ActionResult result = null;

            if (spaceID != updatedSpace.SpaceID)
            {
                result = BadRequest("Area ID in URL and body do not match");
            }
            else
            {
                bool success = _repo.UpdateSpace(spaceID, updatedSpace.Status, updatedSpace.SpaceNumber, updatedSpace.AreaID);

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

        // DELETE api/<ParkingSpaceController>/5
        [HttpDelete("{spaceID}")]
        public ActionResult<bool> Delete(int spaceID)
        {
            ActionResult result = null;
            bool success = _repo.DeleteSpace(spaceID);
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

        // PUT api/<ParkingSpaceController>/5
        [HttpPatch("{spaceID}")]
        public ActionResult UpdatedSpaceStatus(int spaceID, [FromBody] string updateStatus)
        {
            ParkingSpace spaceToBeUpdated = _repo.SingleParkingSpace(spaceID);

            ActionResult result = null;

            if (spaceToBeUpdated == null)
            {
                result = BadRequest("non-existing space");
            }
            else
            {
                // attempt to update the parking space status
                bool success = _repo.UpdateSpace(spaceToBeUpdated.SpaceID, updateStatus, spaceToBeUpdated.SpaceNumber, spaceToBeUpdated.AreaID);

                if (!success)
                {
                    result = NotFound(); // Return 404 if the update was unsuccessful
                }
                else
                {
                    result = NoContent(); // 204 No Content if the update is successful
                }
            }
            return result;
        }
    }
}
