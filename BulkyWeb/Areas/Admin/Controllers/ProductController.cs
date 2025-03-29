using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using BulkyBook.Models.ViewModels;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork db, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = db;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return View(objProductList);
        }
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                CategoryList = _unitOfWork.Category.GetAll().Select(c =>
                 new SelectListItem
                 {
                     Text = c.Name,
                     Value = c.Id.ToString()
                 }),
                Product = new Product()
            };
            if (id == null || id == 0)
            {
               //create
               return View(productVM);
            }
            else
            {
                //update
                productVM.Product = _unitOfWork.Product.Get(p=> p.Id == id);
                return View(productVM);
            }
            //ViewBag.CategoryList = CategoryList;
            //ViewData["CategoryList"]  = CategoryList; viewbag property and viewdata key, both added to the same dictionary
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM ,IFormFile? file)
        {

            if (ModelState.IsValid)
            {
                var wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file!= null)
                {
                    string fileName= Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        var oldImagePath =
                                       Path.Combine(_webHostEnvironment.WebRootPath,
                                       productVM.Product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileSteam = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileSteam);
                    }
                    productVM.Product.ImageUrl = @"\images\product\" + fileName;

                }

                if (productVM.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(productVM.Product);
                }
                else
                {
                    _unitOfWork.Product.Update(productVM.Product);
                }
                _unitOfWork.Save();
                TempData["success"] = "Product Created Successfully";
                return RedirectToAction("Index");
            }
            else
            {
                productVM.CategoryList = _unitOfWork.Category.GetAll().Select(c =>
                     new SelectListItem
                     {
                         Text = c.Name,
                         Value = c.Id.ToString()
                     });
                return View(productVM);
            }
        }

        //public IActionResult Delete(int? id)
        //{
        //    if (id == null || id == 0)
        //    {
        //        return NotFound();
        //    }
        //    Product productFromDb = _unitOfWork.Product.Get(c => c.Id == id); // or firstordefault or where and fod

        //    if (productFromDb == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(productFromDb);
        //}
        //[HttpPost, ActionName("Delete")]
        //public IActionResult DeletePOST(int? id)
        //{
        //    Product? obj = _unitOfWork.Product.Get(c => c.Id == id);
        //    if (obj == null)
        //    {
        //        return NotFound();
        //    }
        //    _unitOfWork.Product.Remove(obj);
        //    _unitOfWork.Save();
        //    TempData["success"] = "Product Deleted Successfully";
        //    return RedirectToAction("Index");
        //}
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll ()
        {
            List<Product> objCategoyList = _unitOfWork.Product.GetAll(includeProperties:"Category").ToList();
            return Json(new {data = objCategoyList });
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _unitOfWork.Product.Get(u => u.Id == id);
            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            string productPath = @"images\products\product-" + id;
            string finalPath = Path.Combine(_webHostEnvironment.WebRootPath, productPath);

            if (Directory.Exists(finalPath))
            {
                string[] filePaths = Directory.GetFiles(finalPath);
                foreach (string filePath in filePaths)
                {
                    System.IO.File.Delete(filePath);
                }

                Directory.Delete(finalPath);
            }


            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion
    }
}
