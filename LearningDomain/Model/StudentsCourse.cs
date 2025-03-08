using System;
using System.Collections.Generic;

namespace LearningDomain.Model;

public partial class StudentsCourse : Entity
{
   

    public int CourseId { get; set; }

    public int StudentId { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual Course Course { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
