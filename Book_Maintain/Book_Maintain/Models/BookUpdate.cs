﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace Book_Maintain.Models
{
	public class BookUpdate
	{
		
		[DisplayName("ID")]
		public string BookID { get; set; }

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
		public string BoughtDate { get; set; }

		[Required]
		[DisplayName("圖書類別")]
		public string BookClassId { get; set; }

		[Required]
		[DisplayName("借閱狀態")]
		public string BookStatusId { get; set; }

		
		[DisplayName("借閱人")]
		public string BookKeeperId { get; set; }

		public string IniStatus { get; set; }
	}
}