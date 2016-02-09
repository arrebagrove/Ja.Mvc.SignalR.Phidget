using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ja.Mvc.SignalR.Phidget.Controllers
{
    public class PhidgetController : Controller
    {
        // GET: Phidget
        public ActionResult PhidgetClient()
        {
            return View();
        }

        // GET: Phidget/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Phidget/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Phidget/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Phidget/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Phidget/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Phidget/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Phidget/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
