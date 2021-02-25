using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Application.Interfaces;
using ShoppingCart.Application.ViewModels;

namespace WebApplication1.Controllers
{
    public class ProductController : Controller
    {
        public readonly IProductsService _prodService;
        public ProductController(IProductsService prodService)
        {
            _prodService = prodService;
        }
        public IActionResult Index()
        {
            var list = _prodService.GetProducts();
            return View(list);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(ProductViewModel data)
        {
            data.Description = HtmlEncoder.Default.Encode(data.Description);

            if (ModelState.IsValid)
            {
                if (data.Category.Id < 0 || data.Category.Id > 5)
                {
                    ModelState.AddModelError("Category.Id", "Category is not valid");
                    return View(data);
                }
                _prodService.AddProduct(data);

                TempData["message"] = "Product inserted successfully";
                return View();
            }
            else
            {
                ModelState.AddModelError("", "Check your input. Operation Failed");
                return View(data);
            }

        }
    }
}
