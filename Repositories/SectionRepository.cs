using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cursus.Data;
using Cursus.Entities;

namespace Cursus.Repositories.Interfaces
{
    public class SectionRepository : BaseRepository<Section>, ISectionRepository
    {
        public SectionRepository(MyDbContext context) : base(context)
        {
        }
    }
}