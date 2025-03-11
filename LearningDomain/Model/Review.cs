using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LearningDomain.Model;

public partial class Review : Entity
{

    [Required(ErrorMessage = "Поле не повинно бути порожнім")]
    [Display(Name = "Відгук")]
    public string Name { get; set; } = null!;
    [Display(Name = "Текст відгуку")]
    public string? Info { get; set; }

    public int CourseId { get; set; }
    [Display(Name = "Назва курсу")]
    public virtual Course Course { get; set; } = null!;
}
