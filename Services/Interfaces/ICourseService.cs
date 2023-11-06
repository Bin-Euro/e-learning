using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cursus.Constants;
using Cursus.DTO;
using Cursus.DTO.Course;
using Cursus.Entities;

namespace Cursus.Services
{
    public interface ICourseService
    {
        public ResultDTO<CourseListDTO> GetCoursesByFilter(
            int offset, int limit,
            double minPrice, double maxPrice, List<Guid> catalogIDs,
            CourseSort courseSort
        );

        public Task<ResultDTO<CourseListDTO>> GetCoursesByInstructor(
            int offset, int limit
        );

        Task<ResultDTO<CreateCourseResDTO>> AddCourse(CreateCourseReqDTO courseRequest);
        Task Update(UpdateCourseDTO updateCourse);

        Task<ResultDTO<string>> DeleteCourse(Guid id);
    }
}