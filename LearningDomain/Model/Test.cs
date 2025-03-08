using System;
using System.Collections.Generic;

namespace LearningDomain.Model;

public partial class Test : Entity
{
  

    public string Name { get; set; } = null!;

    public string? Info { get; set; }

    public string? FormUrl { get; set; }

    public int CourseId { get; set; }

    public virtual Course Course { get; set; } = null!;
}
