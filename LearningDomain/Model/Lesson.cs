using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LearningDomain.Model;

public partial class Lesson : Entity
{

    [Required(ErrorMessage = "Поле не повинно бути порожнім")]
    [Display(Name = "Назва уроку")]
    public string Name { get; set; } = null!;
    [Display(Name = "Інформація про урок")]
    public string? Info { get; set; }
    [Display(Name = "Посилання на відео")]
    public string? VideoUrl { get; set; }
    [Display(Name = "Посилання на урок")]
    public string? DocxUrl { get; set; }

    public int CourseId { get; set; }
    [Display(Name = "Назва курсу")]
    public virtual Course Course { get; set; } = null!;
}
