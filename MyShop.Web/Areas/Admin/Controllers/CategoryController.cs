using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyShop.DataAccess;
using MyShop.DataAccess.Implementation;
using MyShop.Entities.Models;
using MyShop.Entities.Repositories;


namespace MyShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        //// prepare environment to work with database
        ////private readonly ApplicationDbContext _context;
        //public CategoryController(ApplicationDbContext context)
        //{
        //    // inject the context
        //    _context = context;
        //}


        // Unit of work concept
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        // Display Data
        public IActionResult Index()
        {
            // get data from Database and pass it to view 
            //var categories = _context.Categories.ToList();
            var categories = _unitOfWork.Category.GetAll();
            return View(categories);
        }

        // Create Data form
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // save for Cross Side Forgery Attacks
        public IActionResult Create(Category category)
        {
            // Validations (server side)
            if (ModelState.IsValid)
            {
                //_context.Categories.Add(category);
                _unitOfWork.Category.Add(category);
                //_context.SaveChanges();
                _unitOfWork.Complete();

                // for Toastr notification theme
                TempData["Create"] = "Data Has Create Succesfully";

                return RedirectToAction("Index");
            }

            return View(category);
        }

        // Edit Data (display the stored data befor edit)
        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                NotFound();
            }
            // get category from database
            //var categoryToEdit = _context.Categories.Find(id);
            var categoryToEdit = _unitOfWork.Category.GetFirstOrDefault(c => c.Id == id);

            // validate categoryToEdit

            return View(categoryToEdit);
        }

        // Edit Data
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            // Validations (server side)
            if (ModelState.IsValid)
            {
                //_context.Categories.Update(category);
                _unitOfWork.Category.Update(category);
                //_context.SaveChanges();
                _unitOfWork.Complete();

                // for Toastr notification theme
                TempData["Update"] = "Data Has Update Succesfully";

                return RedirectToAction("Index");
            }


            return View(category);
        }

        // Delete
        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                NotFound();
            }
            // get category from database
            //var categoryToEdit = _context.Categories.Find(id);
            var categoryToEdit = _unitOfWork.Category.GetFirstOrDefault(c => c.Id == id);

            return View(categoryToEdit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCategory(int? id)
        {

            // get category from database
            var categoryToDelete = _unitOfWork.Category.GetFirstOrDefault(c => c.Id == id);
            if (categoryToDelete is null)
            {
                NotFound();
            }
            //_context.Categories.Remove(categoryToDelete);
            _unitOfWork.Category.Remove(categoryToDelete);
            //_context.SaveChanges();
            _unitOfWork.Complete();
            // for Toastr notification theme
            TempData["Delete"] = "Data Has Deleted Succesfully";

            return RedirectToAction("Index");
        }
    }
}
