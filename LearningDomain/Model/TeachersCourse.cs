using System;
using System.Collections.Generic;

namespace LearningDomain.Model;

public partial class TeachersCourse : Entity
{
  

    public int CourseId { get; set; }

    public int TeacherId { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual Teacher Teacher { get; set; } = null!;
}
