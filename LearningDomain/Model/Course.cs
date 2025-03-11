using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LearningDomain.Model;

public partial class Course :Entity
{
    [Required(ErrorMessage ="Поле не повинно бути порожнім")]
    [Display(Name = "Назва курсу")]
    public string Name { get; set; } = null!;
    [Display(Name = "Інформація про курс")]
    public string? Info { get; set; }

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<StudentsCourse> StudentsCourses { get; set; } = new List<StudentsCourse>();

    public virtual ICollection<TeachersCourse> TeachersCourses { get; set; } = new List<TeachersCourse>();

    public virtual ICollection<Test> Tests { get; set; } = new List<Test>();
}
