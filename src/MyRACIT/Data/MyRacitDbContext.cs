using Microsoft.EntityFrameworkCore;
using MyRACIT.Models.Entities;
namespace MyRACIT.Data
{
    public class MyRacitDbContext : DbContext
    {
        public MyRacitDbContext(DbContextOptions<MyRacitDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Specialty> Specialties { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<StudentProfile> StudentProfiles { get; set; }
        public DbSet<TeacherProfile> TeacherProfiles { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<AssignmentFile> AssignmentFiles { get; set; }
        public DbSet<AssignmentLink> AssignmentLinks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<Submission>()
                .HasOne(s => s.Assignment)
                .WithMany()
                .HasForeignKey(s => s.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<Grade>()
                .HasOne(g => g.Submission)
                .WithMany()
                .HasForeignKey(g => g.SubmissionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AssignmentFile>()
                .HasOne(af => af.Assignment)
                .WithMany()
                .HasForeignKey(af => af.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AssignmentLink>()
                .HasOne(al => al.Assignment)
                .WithMany()
                .HasForeignKey(al => al.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                Email = "admin@myracit.edu.com",
                PasswordHash = "admin123",
                Name = "admin",
                Role = UserRole.Admin
            });
        }

    }
}