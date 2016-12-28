using System.Web.Mvc;
using System.Web.Security;
using BibliographicSystem.Models;

namespace BibliographicSystem.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        public ActionResult SignIn() => View();

        [HttpPost]
        public ActionResult SignIn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                string name = Membership.GetUserNameByEmail(model.Email);
                if (Membership.ValidateUser(name, model.Password))
                {
                    FormsAuthentication.SetAuthCookie(name, model.RememberMe);
                    if (Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Неправильный пароль или почта");
            }
            return View(model);
        }

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("SignIn", "Account");
        }

        public ActionResult SignUp() => View();

        [HttpPost]
        public ActionResult SignUp(RegisterModel model)
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

        public JsonResult CheckUsername(string username)
        {
            var result = Membership.FindUsersByName(username).Count == 0;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CheckEmail(string email)
        {
            var result = Membership.FindUsersByEmail(email).Count == 0;
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}


