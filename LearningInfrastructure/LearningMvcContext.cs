using System;
using System.Collections.Generic;
using LearningDomain.Model;
using Microsoft.EntityFrameworkCore;

namespace LearningInfrastructure;

public partial class LearningMvcContext : DbContext
{
    public LearningMvcContext()
    {
    }

    public LearningMvcContext(DbContextOptions<LearningMvcContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Certificate> Certificates { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Lesson> Lessons { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentsCourse> StudentsCourses { get; set; }

    public virtual DbSet<Teacher> Teachers { get; set; }

    public virtual DbSet<TeachersCourse> TeachersCourses { get; set; }

    public virtual DbSet<Test> Tests { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=User\\SQLEXPRESS; Database=LearningMVC; Trusted_Connection=True; TrustServerCertificate=True; ");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.Property(e => e.Info).HasColumnType("ntext");
            entity.Property(e => e.Name).HasMaxLength(255);

            entity.HasOne(d => d.StudentCourses).WithMany(p => p.Certificates)
                .HasForeignKey(d => d.StudentCoursesId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Certificates_StudentsCourses");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.Property(e => e.Info).HasColumnType("ntext");
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.Property(e => e.DocxUrl).HasMaxLength(255);
            entity.Property(e => e.Info).HasColumnType("ntext");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.VideoUrl).HasMaxLength(255);

            entity.HasOne(d => d.Course).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Lessons_Courses");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.Property(e => e.Info).HasColumnType("ntext");
            entity.Property(e => e.Name).HasMaxLength(255);

            entity.HasOne(d => d.Course).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Reviews_Courses");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Info).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(255);
        });

        modelBuilder.Entity<StudentsCourse>(entity =>
        {
            entity.Property(e => e.Status).HasMaxLength(255);

            entity.HasOne(d => d.Course).WithMany(p => p.StudentsCourses)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentsCourses_Students");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentsCourses)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentsCourses_Students1");
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Info).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(255);
        });

        modelBuilder.Entity<TeachersCourse>(entity =>
        {
            entity.HasOne(d => d.Course).WithMany(p => p.TeachersCourses)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TeachersCourses_Courses");

            entity.HasOne(d => d.Teacher).WithMany(p => p.TeachersCourses)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TeachersCourses_Teachers1");
        });

        modelBuilder.Entity<Test>(entity =>
        {
            entity.Property(e => e.FormUrl).HasMaxLength(255);
            entity.Property(e => e.Info).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(d => d.Course).WithMany(p => p.Tests)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Tests_Courses");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
