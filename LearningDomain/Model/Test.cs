using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LearningDomain.Model;

public partial class Test : Entity
{

    [Required(ErrorMessage = "Поле не повинно бути порожнім")]
    [Display(Name = "Назва тесту")]
    public string Name { get; set; } = null!;
    [Display(Name = "Інформація про тест")]
    public string? Info { get; set; }
    [Display(Name = "Посилання на тест")]
    public string? FormUrl { get; set; }
    
    public int CourseId { get; set; }
    [Display(Name = "Назва курсу")]
    public virtual Course Course { get; set; } = null!;
}
