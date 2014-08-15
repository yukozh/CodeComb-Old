using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using CodeComb.Entity;

namespace CodeComb.Web.Controllers
{
    public class CKEditorController : BaseController
    {
        //
        // GET: /CKEditor/
        [HttpPost]
        [Authorize]
        public ActionResult Upload(string CKEditorFuncNum, HttpPostedFileBase upload)
        {
            if (ViewBag.CurrentUser.Role < UserRole.VIP)
            {
                if (upload.ContentLength > 1024 * 512) return new HttpStatusCodeResult(400);
            }
            else if (ViewBag.CurrentUser.Role < UserRole.Root)
            {
                if (upload.ContentLength > 1024 * 2048) return new HttpStatusCodeResult(400);
            }
            ViewBag.FuncName = CKEditorFuncNum;
            byte[] bytes;
            using (MemoryStream mem = new MemoryStream())
            {
                upload.InputStream.CopyTo(mem);
                bytes = mem.ToArray();
            }
            ViewBag.URI = string.Format("data:{0};base64,{1}", upload.ContentType, Convert.ToBase64String(bytes));
            return View();
        }
	}
}