using System.Threading.Tasks;
using LearningDomain.Model;
using LearningInfrastructure.Models; // пространство имен для ViewModel
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearningInfrastructure.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly LearningMvcContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            LearningMvcContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        #region Регистрация

        [HttpGet]
        public IActionResult RegisterTeacher()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterTeacher(RegisterTeacherViewModel model)
        {
          
                // Создаём пользователя в Identity
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Добавляем роль "Teacher" (убедитесь, что роль создана)
                    await _userManager.AddToRoleAsync(user, "Teacher");

                    // Создаем запись в таблице Teachers
                    var teacher = new Teacher
                    {
                        Name = model.Name,
                        Email = model.Email,
                        // Обычно пароль не сохраняют в открытом виде, но оставляем, если требуется по условию
                        Password = model.Password,
                        IdentityId = user.Id
                    };
                    _context.Teachers.Add(teacher);
                    await _context.SaveChangesAsync();

                    // Логиним пользователя
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Courses");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            
            return View(model);
        }

        [HttpGet]
        public IActionResult RegisterStudent()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterStudent(RegisterStudentViewModel model)
        {
            
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Student");

                    var student = new Student
                    {
                        Name = model.Name,
                        Email = model.Email,
                        Password = model.Password,
                        IdentityId = user.Id
                    };
                    _context.Students.Add(student);
                    await _context.SaveChangesAsync();

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Courses");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            
            return View(model);
        }

        #endregion

        #region Авторизация

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            // Если returnUrl содержит "Courses", можно добавить сообщение
            if (!string.IsNullOrEmpty(returnUrl) && returnUrl.Contains("Courses"))
            {
                ViewBag.ErrorMessage = "Доступ до курсів можливий тільки після входу в систему.";
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // Пытаемся войти с использованием Email и Password
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Courses");
                }
                if (result.IsLockedOut)
                {
                    return View("Lockout");
                }
                else
                {
                    ModelState.AddModelError("", "Неправильна пошта або пароль.");
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction("Index", "Courses");
        }

        #endregion
    }
}
