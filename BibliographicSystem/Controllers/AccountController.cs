using System.Web.Mvc;
using System.Web.Security;
using BibliographicSystem.Models;

namespace BibliographicSystem.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        //
        // GET: /Account/

        public ActionResult LogIn() => View();

        [HttpPost]
        public ActionResult LogIn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (Membership.ValidateUser(model.Username, model.Password))
                {
                    FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                    if (Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Неправильный пароль или логин");
            }
            return View(model);
        }

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account");
        }

        public ActionResult SignIn() => View();

        [HttpPost]
        public ActionResult SignIn(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                MembershipCreateStatus createStatus;
                Membership.CreateUser(
                    username: model.Username,
                    password: model.Password, 
                    email: model.Email,
                    passwordQuestion: null, 
                    passwordAnswer: null, 
                    isApproved: true, 
                    providerUserKey: null, 
                    status: out createStatus);

                if (createStatus == MembershipCreateStatus.Success)
                {
                    FormsAuthentication.SetAuthCookie(model.Username, false);
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Ошибка при регистрации");
            }
            return View(model);
        }

        public ActionResult Manage() => View(new AddingToSystem());
    }
}


