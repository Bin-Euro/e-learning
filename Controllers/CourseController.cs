using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cursus.Constants;
using Cursus.DTO.Course;
using Cursus.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cursus.Controllers
{
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetCoursesByFilter(int offset, int limit, double minPrice, double maxPrice, List<Guid> catalogIDs, CourseSort courseSort)
        {
            var result = _courseService.GetCoursesByFilter(offset, limit, minPrice, maxPrice, catalogIDs, courseSort);
            return result._isSuccess ? Ok(result) : StatusCode(500, result);
        }

        [Authorize(Roles = "Instructor")]
        [HttpGet("get-by-instructor")]
        public async Task<IActionResult> GetCoursesByInstructor(int offset, int limit)
        {
            var result = await _courseService.GetCoursesByInstructor(offset, limit);
            return StatusCode(result._statusCode, result);
        }
        
        [HttpPost("create")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Post([FromBody] CreateCourseReqDTO courseRequest)
        {
            if (courseRequest == null)
            {
                return NoContent();
            }
            var createCourse = await _courseService.AddCourse(courseRequest);
            if (!createCourse._isSuccess)
            {
                return NotFound(createCourse);
            }
            return Ok(createCourse);
        }

        [HttpPut]
        [Route("update")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Update([FromBody] UpdateCourseDTO course)
        {
            await _courseService.Update(course);
            return Ok();
        }

        [HttpDelete]
        [Route("delete")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> DeleteCourse(Guid id)
        {
            var result = await _courseService.DeleteCourse(id);
            return result._isSuccess ? Ok(result) : StatusCode(500, result);
        }
    }
}