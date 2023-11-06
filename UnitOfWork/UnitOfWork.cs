using Cursus.Data;
using Cursus.Repositories;
using Cursus.Repositories.Interfaces;

namespace Cursus.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MyDbContext _dbContext;
        private IAnswerRepository _AnswerRepository;
        private IAssignmentAnswerRepository _AssignmentAnswerRepository;
        private IAssignmentRepository _AssignmentRepository;
        private ICatalogRepository _CatalogRepository;
        private ICourseCatalogRepository _CourseCatalogRepository;
        private ICourseFeedbackRepository _CourseFeedbackRepository;
        private ICourseReportRepository _CourseReportRepository;
        private ICourseRepository _CourseRepository;
        private IInstructorRepository _InstructorRepository;
        private ILessonRepository _LessonRepository;
        private INotificationRepository _NotificationRepository;
        private IOrderRepository _OrderRepository;
        private IOrderDetailRepository _OrderDetailRepository;
        private IReportRepository _ReportRepository;
        private ISectionRepository _SectionRepository;
        private IUserReportRepository _UserReportRepository;
        private IUserRepository _UserRepository;

        public UnitOfWork(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IAnswerRepository AnswerRepository
        {
            get { return _AnswerRepository = _AnswerRepository ?? new AnswerRepository(_dbContext); }
        }

        // public IAssignmentAnswerRepository AssignmentAnswerRepository
        // {
        //     get
        //     {
        //         return _AssignmentAnswerRepository =
        //             _AssignmentAnswerRepository ?? new AssignmentAnswerRepository(_dbContext);
        //     }
        // }

        public IAssignmentRepository AssignmentRepository
        {
            get { return _AssignmentRepository = _AssignmentRepository ?? new AssignmentRepository(_dbContext); }
        }

        public ICatalogRepository CatalogRepository
        {
            get { return _CatalogRepository = _CatalogRepository ?? new CatalogRepository(_dbContext); }
        }

        public ICourseCatalogRepository CourseCatalogRepository
        {
            get
            {
                return _CourseCatalogRepository = _CourseCatalogRepository ?? new CourseCatalogRepository(_dbContext);
            }
        }

        public ICourseFeedbackRepository CourseFeedbackRepository
        {
            get
            {
                return _CourseFeedbackRepository =
                    _CourseFeedbackRepository ?? new CourseFeedbackRepository(_dbContext);
            }
        }

        public ICourseReportRepository CourseReportRepository
        {
            get { return _CourseReportRepository = _CourseReportRepository ?? new CourseReportRepository(_dbContext); }
        }

        public ICourseRepository CourseRepository
        {
            get { return _CourseRepository = _CourseRepository ?? new CourseRepository(_dbContext); }
        }

        public IInstructorRepository InstructorRepository
        {
            get { return _InstructorRepository = _InstructorRepository ?? new InstructorRepository(_dbContext); }
        }

        public ILessonRepository LessonRepository
        {
            get { return _LessonRepository = _LessonRepository ?? new LessonRepository(_dbContext); }
        }

        public INotificationRepository NotificationRepository
        {
            get { return _NotificationRepository = _NotificationRepository ?? new NotificationRepository(_dbContext); }
        }

        public IOrderRepository OrderRepository
        {
            get { return _OrderRepository = _OrderRepository ?? new OrderRepository(_dbContext); }
        }

        public IOrderDetailRepository OrderDetailRepository
        {
            get { return _OrderDetailRepository = _OrderDetailRepository ?? new OrderDetailRepository(_dbContext); }
        }

        public IReportRepository ReportRepository
        {
            get { return _ReportRepository = _ReportRepository ?? new ReportRepository(_dbContext); }
        }

        public ISectionRepository SectionRepository
        {
            get { return _SectionRepository = _SectionRepository ?? new SectionRepository(_dbContext); }
        }

        public IUserReportRepository UserReportRepository
        {
            get { return _UserReportRepository = _UserReportRepository ?? new UserReportRepository(_dbContext); }
        }

        public IUserRepository UserRepository
        {
            get { return _UserRepository = _UserRepository ?? new UserRepository(_dbContext); }
        }

        public void Commit()
            => _dbContext.SaveChanges();


        public async Task CommitAsync()
            => await _dbContext.SaveChangesAsync();


        public void Rollback()
            => _dbContext.Dispose();


        public async Task RollbackAsync()
            => await _dbContext.DisposeAsync();
    }
}