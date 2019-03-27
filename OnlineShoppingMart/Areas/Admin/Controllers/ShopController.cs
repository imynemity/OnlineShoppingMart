using OnlineShoppingMart.Models.Data;
using OnlineShoppingMart.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using PagedList;

namespace OnlineShoppingMart.Areas.Admin.Controllers
{
    public class ShopController : Controller
    {
        // GET: Admin/Shop
        public ActionResult Categories()
        {
            List<CategoryVM> categoryVMList;
            using (Db db = new Db())
            {
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
            }
            return View(categoryVMList);
        }



        /* [HttpGet]
         public ActionResult Addcat()
         { return View(); }
         [HttpPost]
         public ActionResult Addcat(CategoryVM model)
         {

             using (Db db = new Db())
             {


                 CategoryDTO dto = new CategoryDTO();
                 dto.Name = model.Name;
                 db.Categories.Add(dto);
                 db.SaveChanges();

             }
             TempData["SM"] = "You have added a new category!";
             return RedirectToAction("Categories");

         }

     */


        [HttpPost]
        public string AddNewCategory(string catName)
        {

            string id;
            using (Db db = new Db())
            {

                if (db.Categories.Any(x => x.Name == catName))
                {

                    return "titletaken";
                }

                CategoryDTO dto = new CategoryDTO();
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", ".").ToLower();
                dto.Sorting = 100;
                db.Categories.Add(dto);
                db.SaveChanges();
                id = dto.Id.ToString();
            }
            return id;
        }


        [HttpPost]
        public ActionResult ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {

                int count = 1;
                CategoryDTO dto;
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;
                    db.SaveChanges();
                    count++;
                }




            }

            return View();
        }



        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {

                CategoryDTO dto = db.Categories.Find(id);
                db.Categories.Remove(dto);
                db.SaveChanges();


            }

            return RedirectToAction("Categories");

        }

        [HttpGet]
        public ActionResult AddProduct()
        {
            ProductVM model = new ProductVM();
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }



            return View(model);

        }
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            if (!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }


          /* using (Db db = new Db())
            {
                if (db.Products.Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "That Product name is taken");
                    return View(model);
                }
            }*/
            int id;
            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();
                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;
                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDTO.Name;
                db.Products.Add(product);
                db.SaveChanges();
                id = product.Id;


            }
            TempData["SM"] = "You have added a product!";

            #region Upload Image
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            if (!Directory.Exists(pathString1))
            {
                Directory.CreateDirectory(pathString1);
            }

            if (!Directory.Exists(pathString2))
            {
                Directory.CreateDirectory(pathString2);
            }
            if (!Directory.Exists(pathString3))
            {
                Directory.CreateDirectory(pathString3);
            }
            if (!Directory.Exists(pathString4))
            {
                Directory.CreateDirectory(pathString4);
            }

            if (!Directory.Exists(pathString5))
            {
                Directory.CreateDirectory(pathString5);
            }

            if (file != null && file.ContentLength > 0)
            {
                string ext = file.ContentType.ToLower();
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjeg" &&
                    ext != "image/jpg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png"
                    )
                {

                    using (Db db = new Db())
                    {

                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "That Image was not uploaded-wrong extension");
                        return View(model);

                    }
                }
                string imageName = file.FileName;
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }
                var path = string.Format("{0}\\{1}", pathString2, imageName);
                var path2 = string.Format("{0}\\{1}", pathString3, imageName);
                file.SaveAs(path);
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);

            }
            #endregion
            return RedirectToAction("AddProduct");
        }


        public ActionResult Products (int? page,int? catId)
        {
            List<ProductVM> listOfProductVM;
            var pageNumber = page ?? 1;
            using (Db db = new Db())
            {
                listOfProductVM = db.Products.ToArray().Where(x=> catId==null || catId==0|| x.CategoryId==catId).Select(x=>new ProductVM(x)).ToList();
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                ViewBag.SelectedCat = catId.ToString();
               




            }


            var onePageOfPRoducts = listOfProductVM.ToPagedList(pageNumber, 3);
            ViewBag.OnePageOfProducts = onePageOfPRoducts;
            return View(listOfProductVM);
        }
        [HttpGet]
        public ActionResult EditProduct(int id)
        {

            ProductVM model;
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                if(dto==null)
                {
                    return Content("That product does not exist!");
                }
                model = new ProductVM(dto);
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs")).Select(fn => Path.GetFileName(fn));

            }











                return View(model);
        }
        [HttpPost]
        public ActionResult EditProduct(ProductVM model,HttpPostedFileBase file)
        {
            int id = model.Id;
            using (Db db = new Db())
            {

                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");


            }


            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs")).Select(fn => Path.GetFileName(fn));
            if (!ModelState.IsValid)
            {
                
                    return View(model);
                
            }

            using (Db db = new Db())
            {
                if (db.Products.Where(x=>x.Id!=id).Any(x=>x.Name==model.Name))
                {
                    ModelState.AddModelError("", "That Product name is taken!");
                    return View(model);
                }


            }


            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Price = model.Price;
                dto.Description = model.Description;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;
                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDTO.Name;
                db.SaveChanges();
            }
            TempData["SM"] = "You have edited the product";


            #region Upload Image
          
            if (file != null && file.ContentLength > 0)
            {
                string ext = file.ContentType.ToLower();
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjeg" &&
                    ext != "image/jpg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png"
                    )
                {

                    using (Db db = new Db())
                    {

                       
                        ModelState.AddModelError("", "That Image was not uploaded-wrong extension");
                        return View(model);

                    }
                }
                var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

               

                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);
                foreach (FileInfo file2 in di1.GetFiles())
                    file2.Delete();
                foreach (FileInfo file3 in di2.GetFiles())
                    file3.Delete();
                string imageName = file.FileName;
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }

                var path = string.Format("{0}\\{1}", pathString1, imageName);
                var path2 = string.Format("{0}\\{1}", pathString2, imageName);
                file.SaveAs(path);
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }
            #endregion


            return RedirectToAction("EditProduct");

        }
        public ActionResult DeleteProduct(int id)
        {

            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);
                db.SaveChanges();


            }

            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            var pathString = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            if (Directory.Exists(pathString))
                Directory.Delete(pathString,true);

           return RedirectToAction("Products");


        }
    }

    }
