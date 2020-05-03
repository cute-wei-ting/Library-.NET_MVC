using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace Book_Maintain.Controllers
{
	public class BookController : Controller
	{
		Models.BookService bookservice = new Models.BookService();

		public ActionResult Index()
		{
			Models.BookSearch booksearch = new Models.BookSearch(); //table顯示全部
			Models.BookSearch DropDownList =  ShowIndexPrepare(booksearch);
			return View(DropDownList);
		}

		/*---------------------SEARCH---------------------*/
		[HttpPost()]
		public ActionResult Index(Models.BookSearch searchdata)//table顯示要搜尋資料
		{
			Models.BookSearch DropDownList = ShowIndexPrepare(searchdata);
			return View(DropDownList);
		}

		/*---------------------INSERT---------------------*/

		public ActionResult InsertBook()
		{
			ViewBag.BookClass = bookservice.GetDropdownList("GetClass");
			return View();
		}
		[HttpPost()]
		public ActionResult InsertBook(Models.BookInsert insertdata)
		{
			ViewBag.BookClass = bookservice.GetDropdownList("GetClass");
			if (ModelState.IsValid)
			{
				bookservice.InsertBook(insertdata);
				return RedirectToAction("Index"); 
			}
			return View();
		}
		public ActionResult LendRecord(int id)
		{
			ViewBag.Lenddisplay= bookservice.GetLendRecord(id);
			return View();
		}
		public ActionResult UpdateBook(int id)
		{
			ViewBag.BookClass = bookservice.GetDropdownList("GetClass");
			ViewBag.BookKeeper = bookservice.GetDropdownList("GetUpdateKeeper");
			ViewBag.BookStatus = bookservice.GetDropdownList("GetStatus"); //顯示DropDownList

			var update  = bookservice.GetUpdateBook(id);
			update.BookID = id.ToString();
			update.IniStatus = update.BookStatusId;
			//IniBookStatus = update.BookStatusId;//拿到一開始狀態,判斷是否增加借閱紀錄
			//nowID = id;
			return View(update);
		}
		[HttpPost()]
		public ActionResult UpdateBook(Models.BookUpdate updatedata)
		{
			ViewBag.BookClass = bookservice.GetDropdownList("GetClass");
			ViewBag.BookKeeper = bookservice.GetDropdownList("GetUpdateKeeper");
			ViewBag.BookStatus = bookservice.GetDropdownList("GetStatus"); //顯示DropDownList
			bool relation = ((updatedata.BookStatusId == "B" || updatedata.BookStatusId == "C")&&( updatedata.BookKeeperId == "" || updatedata.BookKeeperId == null));
			if (ModelState.IsValid && !relation)
			{
				string LaterBookStatus = updatedata.BookStatusId;
				string IniBookStatus = updatedata.IniStatus;
				if ((IniBookStatus == "A" || IniBookStatus == "U") && (LaterBookStatus == "B" || LaterBookStatus == "C"))
				{
					Models.LendRecordInsert lendRecordInsert = new Models.LendRecordInsert();
					lendRecordInsert.BookKeeperId = updatedata.BookKeeperId;
					lendRecordInsert.BookID = Convert.ToInt32(updatedata.BookID);
					bookservice.InsertLendRecord(lendRecordInsert);
				}
				bookservice.UpdateBook(Convert.ToInt32(updatedata.BookID), updatedata);
				return RedirectToAction("Index");
			}
			return View();
		}

		[HttpPost()]
		public JsonResult UpdateBookDel(string id)
		{
			return DeleteBook(id);
		}

		[HttpPost()]
		public JsonResult DeleteBook(string BookId)
		{
			return Json(bookservice.DecideDelete(BookId));
		}

		public ActionResult BookDetail(int id)
		{
			ViewBag.bookdetaildisplay= bookservice.GetBookDetail(id);
			return View();
		}

		private Models.BookSearch ShowIndexPrepare(Models.BookSearch booksearch)
		{
			Models.BookSearch indexDL = new Models.BookSearch();
			indexDL.BookClass= bookservice.GetDropdownList("GetClass");
			indexDL.BookKeeper = bookservice.GetDropdownList("GetKeeper");
			indexDL.BookStatus = bookservice.GetDropdownList("GetStatus");
			ViewBag.display = bookservice.SearchBook(booksearch);

			return indexDL;
		}


	}
}