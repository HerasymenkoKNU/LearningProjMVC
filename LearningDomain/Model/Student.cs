using System;
using System.Collections.Generic;

namespace LearningDomain.Model;

public partial class Student : Entity
{
   

    public string Name { get; set; } = null!;

    public string? Info { get; set; }

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public virtual ICollection<StudentsCourse> StudentsCourses { get; set; } = new List<StudentsCourse>();
}
