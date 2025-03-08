using System;
using System.Collections.Generic;

namespace LearningDomain.Model;

public partial class Teacher : Entity
{
   

    public string Name { get; set; } = null!;

    public string? Info { get; set; }

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public virtual ICollection<TeachersCourse> TeachersCourses { get; set; } = new List<TeachersCourse>();
}
