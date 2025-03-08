using System;
using System.Collections.Generic;

namespace LearningDomain.Model;

public partial class Certificate : Entity
{
    

    public string Name { get; set; } = null!;

    public string? Info { get; set; }

    public int StudentCoursesId { get; set; }

    public virtual StudentsCourse StudentCourses { get; set; } = null!;
}
