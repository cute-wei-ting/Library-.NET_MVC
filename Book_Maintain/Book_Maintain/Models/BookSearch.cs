using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.Web.Mvc;

namespace Book_Maintain.Models
{
	public class BookSearch
	{
		[DisplayName("書ID")]
		public int BookID { get; set; }

		[DisplayName("書名")]
		public string BookName { get; set; }

		[DisplayName("圖書類別")]
		public string BookClassId { get; set; }
		public List<SelectListItem> BookClass { get; set; }

		[DisplayName("借閱人")]
		public string BookKeeperId{ get; set; }
		public List<SelectListItem> BookKeeper { get; set; }

		[DisplayName("借閱狀態")]
		public string BookStatusId { get; set; }
		public List<SelectListItem> BookStatus { get; set; }


		[DisplayName("購書日期")]
		public string BoughtDate { get; set; }

	}
}