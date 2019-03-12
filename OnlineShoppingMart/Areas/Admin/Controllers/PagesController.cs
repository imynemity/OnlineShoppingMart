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
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            //declare pageVm
            PageVM model;
            using (Db db = new Db())
            {
                PageDTO dto = db.Pages.Find(id);
                if (dto==null)
                {
                    return Content("The page does not exist");
                }
                model = new PageVM(dto);
                 
                
            }

                return View(model);
        }


        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            //check model state
            if (!ModelState.IsValid)
            { return View(model); }
            using (Db db = new Db())
            {

                int id = model.Id;
                string slug ="home";
                PageDTO dto = db.Pages.Find(id);
                dto.Title = model.Title;

                if (model.Slug!="home")
                {

                    if (string.IsNullOrWhiteSpace(model.Slug))
                        {

                        slug = model.Title.Replace(" ", ".").ToLower();

                    }
                    else { slug = model.Slug.Replace(" ", ".").ToLower(); }


                }

                if (db.Pages.Where(x => x.Id!= id).Any(x => x.Title==model.Title)||
                    db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug) )
                {

                    ModelState.AddModelError("", "That Title or Slug already exists");
                    return View(model);
                }
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                db.SaveChanges();


            }

            TempData["SM"] = "You have edited the page";







                return RedirectToAction("EditPage");





        }


        public ActionResult PageDetails(int id)




        {
            PageVM model;

            using (Db db = new Db()) {
                PageDTO dto = db.Pages.Find(id);
                if (dto==null)
                {

                    return Content("The page does not exists!");
                }



                model = new PageVM(dto);


            }


            return View(model);
        }



        public ActionResult DeletePage(int id)
        {
            using (Db db = new Db())
            {

                PageDTO dto = db.Pages.Find(id);
                db.Pages.Remove(dto);
                db.SaveChanges();


            }

            return RedirectToAction("Index");

        }


        [HttpPost]
        public ActionResult ReorderPages(int [] id)
        {
            using (Db db = new Db())
            {

                int count = 1;
                PageDTO dto;
                foreach (var pageID in id)
                {
                    dto = db.Pages.Find(pageID);
                    dto.Sorting = count;
                    db.SaveChanges();
                    count++;
                }




            }

                return View();
        }




    }
}