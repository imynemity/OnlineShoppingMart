using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OnlineShoppingMart.Models.Data;
using OnlineShoppingMart.Models.ViewModels.Pages;

namespace OnlineShoppingMart.Areas.Admin.Controllers
{
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            List<PageVM> pagesList;

            using (Db db = new Db())
            { pagesList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList(); }
            return View(pagesList);
        }

        [HttpGet]
        public ActionResult AddPage()
        { return View(); }
        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {

            //check model state
            if (!ModelState.IsValid)
            { return View(model); }
            using (Db db = new Db())
            {
                //Declare slug
                string slug;

                // init dto
                PageDTO dto = new PageDTO();
                dto.Title = model.Title;
                //check for end set slug if need be 
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", ".").ToLower();

                }

                else { slug = model.Slug.Replace(" ", ".").ToLower(); }
                //Make sure title and slug are unique
                if (db.Pages.Any(x => x.Title ==model.Title )|| db.Pages.Any (x => x.Slug==slug))
                {
                    ModelState.AddModelError("", "That title or slug exists already");
                    return View(model);

                }

                //dto the rest 
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100;




                //save dto
                db.Pages.Add(dto);
                db.SaveChanges();
                TempData["SM"] = "You have added a new page!";
                return RedirectToAction("AddPage");

                
            }
        }
    }
}