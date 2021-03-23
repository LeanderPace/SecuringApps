using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Application.Interfaces;
using ShoppingCart.Application.ViewModels;
using WebApplication1.ActionFilters;

namespace WebApplication1.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductsService _prodService;
        private readonly IWebHostEnvironment _webHost;

        public ProductController(IProductsService prodService, IWebHostEnvironment webHost)
        {
            _prodService = prodService;
            _webHost = webHost;
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

        [HttpPost][ValidateAntiForgeryToken][OwnerAuthorize]
        public IActionResult Create(IFormFile file, ProductViewModel data)
        {
            
            data.Description = HtmlEncoder.Default.Encode(data.Description);

            if (ModelState.IsValid)
            {
                string uniqueFilename;
                if (System.IO.Path.GetExtension(file.FileName) == ".jpg")
                {
                    byte[] whitelist = new byte[] { 255, 216 };

                    if (file != null)
                    {
                        using (var f = file.OpenReadStream())
                        {
                            //int byte1 = f.ReadByte();
                            //int byte2 = f.ReadByte();

                            byte[] buffer = new byte[2]; // to read an x amount of bytes at 1 go
                            f.Read(buffer, 0, 2); // offset - how many bytes you would like the pointer to skip

                            for (int i = 0; i < whitelist.Length; i++)
                            {
                                if (whitelist[i] == buffer[i])
                                {

                                }
                                else
                                {
                                    ModelState.AddModelError("file", "File is not valid and acceptable");
                                    return View();
                                }
                            }
                            // other reading of bytes happening
                            f.Position = 0;

                            // uploading file
                            uniqueFilename = Guid.NewGuid() + Path.GetExtension(file.FileName);
                            data.ImageUrl = uniqueFilename;

                            string absolutePath = _webHost.WebRootPath + @"\pictures\" + uniqueFilename;

                            using (FileStream fsOut = new FileStream(absolutePath, FileMode.CreateNew, FileAccess.Write))
                            {
                                f.CopyTo(fsOut);
                            }
                            f.Close();
                        }
                    }
                }
                if (data.Category.Id < 0 || data.Category.Id > 5)
                {
                    ModelState.AddModelError("Category.Id", "Category is not valid");
                    return View(data);
                }
                data.Owner = HttpContext.User.Identity.Name; // this is the currently logged in user

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

        [Authorize][HttpGet]
        public IActionResult Edit(Guid id)
        {
            var prod = _prodService.GetProduct(id);
            return View(prod);
        }

        [Authorize][OwnerAuthorize][HttpPost][ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, ProductViewModel updatedData)
        {
            //_prodService.EditProduct(udatedData);
            TempData["message"] = "Product was updated successfully";
            return View();
        }
    }
}
