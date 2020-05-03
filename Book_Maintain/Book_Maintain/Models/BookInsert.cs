using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Book_Maintain.Models
{
	public class BookInsert
	{
		[Required]
		[DisplayName("書名")]
		public string BookName { get; set; }

		[Required]
		[DisplayName("作者")]
		public string BookAuthor { get; set; }

		[Required]
		[DisplayName("出版商")]
		public string BookPublisher { get; set; }

		[Required]
		[DisplayName("內容簡介")]
		public string Note { get; set; }

		[Required]
		[DisplayName("購書日期")]
		public DateTime BoughtDate { get; set; }

		[Required]
		[DisplayName("圖書類別")]
		public string BookClass { get; set; }
	}
}
