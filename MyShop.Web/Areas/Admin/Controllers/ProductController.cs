using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyShop.DataAccess;
using MyShop.DataAccess.Implementation;
using MyShop.Entities.Models;
using MyShop.Entities.Repositories;
using MyShop.Entities.ViewModels;
using NuGet.Packaging.Signing;


namespace MyShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        // prepare environment to work with database
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }


        // Display Products
        public IActionResult Index()
        {
            // get data from Database and pass it to view 
            return View();
        }

        // retrive data in json format
        public IActionResult GetData()
        {
            // get data from Database and pass it to products.js 
            var products = _unitOfWork.Product.GetAll(IncludeWord: "category");
            return Json(new { data = products });
        }

        // Create Data form
        [HttpGet]
        public IActionResult Create()
        {
            ProductVM productsVM = new ProductVM
            {
                Product = new Product(),
                CategoryList = _unitOfWork.Category.GetAll().Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                })
            };
            return View(productsVM);
        }


        // IFormFile must be the same name of input tag in view
        [HttpPost]
        [ValidateAntiForgeryToken] // save for Cross Side Forgery Attacks
        public IActionResult Create(ProductVM productVM, IFormFile file) 
        {
            // Validations (server side)
            if (ModelState.IsValid)
            {
                // handeling the uploaded file
                string RootPath = _webHostEnvironment.WebRootPath;
                if(file != null)
                {
                    var FileName = Guid.NewGuid().ToString();
                    var Upload = Path.Combine(RootPath, @"Images\Products");
                    var Extension = Path.GetExtension(file.FileName);

                    using (var fileStream = new FileStream(Path.Combine(Upload, FileName + Extension), FileMode.Create))
                    {
                        // add file to images folder path
                        file.CopyTo(fileStream);
                    }
                    // value in database
                    productVM.Product.Img = @"Images\Products\" + FileName + Extension;
                }

                _unitOfWork.Product.Add(productVM.Product);
                _unitOfWork.Complete();

                // for Toastr notification theme
                TempData["Create"] = "Product has Created Succesfully";

                return RedirectToAction("Index");
            }

            return View(productVM);
        }

        // Edit Data (display the stored data befor edit)
        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                NotFound();
            }
            // get product from database
            var productToEdit = _unitOfWork.Product.GetFirstOrDefault(c => c.Id == id);

            // validate productToEdit
            if (productToEdit == null) 
            {
                return NotFound();
            }
            ProductVM productsVM = new ProductVM
            {
                Product = productToEdit,
                CategoryList = _unitOfWork.Category.GetAll().Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                })
            }; 
            return View(productsVM);
        }

        // Edit Data
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ProductVM productVM, IFormFile? file)
        {
            // Validations (server side)
            if (ModelState.IsValid)
            {
                // handeling the uploaded file
                string RootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    var FileName = Guid.NewGuid().ToString();
                    var Upload = Path.Combine(RootPath, @"Images\Products");
                    var Extension = Path.GetExtension(file.FileName);

                    // delete old Image to replace with new one if there
                    if (productVM.Product.Img != null)
                    {
                        var oldImg = Path.Combine(RootPath, productVM.Product.Img.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImg))
                        {
                            System.IO.File.Delete(oldImg);
                        }

                    }

                    using (var fileStream = new FileStream(Path.Combine(Upload, FileName + Extension), FileMode.Create))
                    {
                        // add file to images folder path
                        file.CopyTo(fileStream);
                    }
                    // value in database
                    productVM.Product.Img = @"Images\Products\" + FileName + Extension;
                }

                _unitOfWork.Product.Update(productVM.Product);
                _unitOfWork.Complete();

                // for Toastr notification theme
                TempData["Update"] = "Data Has Update Succesfully";

                return RedirectToAction("Index");
            }
            return View(productVM.Product);
        }

        // Delete
        
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            // get product from database
            var productToDelete = _unitOfWork.Product.GetFirstOrDefault(c => c.Id == id);
            if (productToDelete is null)
            {
                return Json(new {success = false, message = "Product Not found"});
            }
            // remove record from database
            _unitOfWork.Product.Remove(productToDelete);

            // delete old Image to replace with new one if there
            var oldImg = Path.Combine(_webHostEnvironment.WebRootPath, productToDelete.Img.TrimStart('\\'));
            if (System.IO.File.Exists(oldImg))
            {
                System.IO.File.Delete(oldImg);
            }

            _unitOfWork.Complete();

            // for Sweet Alert
            return Json(new { success = true, message = "Product has been deleted" });
        }
    }
}
