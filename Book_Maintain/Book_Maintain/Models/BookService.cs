using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace Book_Maintain.Models
{
	public class BookService
	{
		private string GetDBConnectionString()
		{
			return System.Configuration.ConfigurationManager.ConnectionStrings["DBConn"].ConnectionString.ToString();
		}
		public List<SelectListItem> GetDropdownList(string option)
		{
				string sql = "";
				switch (option)
			{ 
				case "GetClass":
					sql = @"SELECT bc.BOOK_CLASS_ID AS ID,bc.BOOK_CLASS_NAME AS NAME
						FROM BOOK_CLASS AS bc";
					break;
				case "GetKeeper":
					sql = @"SELECT mm.[USER_ID] AS ID,mm.USER_ENAME AS NAME
						FROM MEMBER_M mm;";
					break;
				case "GetStatus":
					sql = @"SELECT bc.CODE_ID AS ID ,bc.CODE_NAME AS NAME
						FROM BOOK_CODE bc	
						WHERE bc.CODE_TYPE = 'BOOK_STATUS'";
					break;
				case "GetUpdateKeeper":
					sql = @"SELECT mm.[USER_ID] AS ID,mm.USER_ENAME+'-'+mm.USER_CNAME AS NAME
						FROM MEMBER_M mm;";
					break;
			}
			List<SelectListItem> selectlist = new List<SelectListItem>();
			DataTable db = new DataTable();
			using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
			{
				conn.Open();
				SqlCommand cmd = new SqlCommand(sql, conn);
				SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
				sqlDataAdapter.Fill(db);
				foreach (DataRow row in db.Rows)
				{
					 selectlist.Add(new SelectListItem { Text = row["NAME"].ToString(), Value = row["ID"].ToString() });
				}

				/* reader會動態綁定資料庫hand在那*/
				//var reader = cmd.ExecuteReader(); 
				//while (reader.Read())
				//{ selectlist.Add(new SelectListItem { Text = reader.GetString(1), Value = reader.GetString(val) }); }
				//reader.Close();
				conn.Close();
			}
			return selectlist;
		}
		public List<Models.BookSearch> SearchBook(Models.BookSearch searchdata)
		{
			string sql = @" SELECT bd.BOOK_ID AS 書ID,bc.BOOK_CLASS_NAME AS 圖書類別,bd.BOOK_NAME AS 書名, CONVERT(varchar, bd.BOOK_BOUGHT_DATE, 111) AS 購書日期, bcd.CODE_NAME AS 借閱狀態,mm.USER_ENAME AS 借閱人
							FROM BOOK_DATA bd
								 INNER JOIN BOOK_CLASS bc
									ON bd.BOOK_CLASS_ID = bc.BOOK_CLASS_ID
								 INNER JOIN BOOK_CODE bcd
									ON bd.BOOK_STATUS = bcd.CODE_ID AND bcd.CODE_TYPE = 'BOOK_STATUS'
								 LEFT OUTER JOIN  MEMBER_M mm
									ON bd.BOOK_KEEPER = mm.[USER_ID]
							WHERE (bd.BOOK_CLASS_ID = @BookClassId OR @BookClassId='') AND
								  (LOWER(bd.BOOK_NAME) LIKE ('%'+LOWER(@BookName)+'%') OR @BookName='') AND
								  (bcd.CODE_ID = @BookStatusId OR @BookStatusId='') AND
								  (mm.[USER_ID] = @BookKeeperId OR @BookKeeperId='')		
							ORDER BY bd.CREATE_DATE DESC";

			DataTable dt = new DataTable();
			using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
			{
				conn.Open();
				SqlCommand cmd = new SqlCommand(sql, conn);
				cmd.Parameters.Add(new SqlParameter("@BookClassId", searchdata.BookClassId == null ? string.Empty : searchdata.BookClassId));
				cmd.Parameters.Add(new SqlParameter("@BookName", searchdata.BookName == null ? string.Empty : searchdata.BookName));
				cmd.Parameters.Add(new SqlParameter("@BookStatusId", searchdata.BookStatusId == null ? string.Empty : searchdata.BookStatusId));
				cmd.Parameters.Add(new SqlParameter("@BookKeeperId", searchdata.BookKeeperId == null ? string.Empty : searchdata.BookKeeperId));
				SqlDataAdapter sqlAdapter = new SqlDataAdapter(cmd);
				sqlAdapter.Fill(dt);
				conn.Close();
			}
			return this.ToModelList(dt);
		}
		public List<Models.BookSearch> ToModelList(DataTable dt)
		{
			List<Models.BookSearch> result = new List<BookSearch>();
			foreach (DataRow row in dt.Rows)
			{
				result.Add(new BookSearch()
				{
					BookID = (int)row["書ID"],
					BookName = row["書名"].ToString(),
					BookClassId = row["圖書類別"].ToString(),
					BookKeeperId = row["借閱人"].ToString(),
					BookStatusId = row["借閱狀態"].ToString(),
					BoughtDate = row["購書日期"].ToString()
				});
			}
			return result;
		}

		public void InsertBook(Models.BookInsert insertdata)
		{
			string sql = @"
						BEGIN TRY
							BEGIN TRANSACTION
								INSERT INTO BOOK_DATA
								 (
									BOOK_NAME,BOOK_CLASS_ID,BOOK_AUTHOR,
									BOOK_BOUGHT_DATE,BOOK_PUBLISHER,BOOK_NOTE,
									BOOK_STATUS,BOOK_KEEPER,BOOK_AMOUNT,CREATE_DATE,
									CREATE_USER,MODIFY_DATE,MODIFY_USER
								 )
								VALUES
								(
									 @BookName,@BookClass, @BookAuthor, 
									 @BoughtDate, @BookPublisher, @Note, 
									 'A','',0,GETDATE(), 
									 '123',GETDATE(), '123'
								)
								Select SCOPE_IDENTITY()
							COMMIT TRANSACTION
						End TRY
						BEGIN CATCH
							SELECT
								ERROR_NUMBER() AS ErrorNumber,
								ERROR_SEVERITY() AS ErrorSeverity,
								ERROR_STATE() as ErrorState,
								ERROR_PROCEDURE() as ErrorProcedure,
								ERROR_LINE() as ErrorLine,
								ERROR_MESSAGE() as ErrorMessage
							ROLLBACK TRANSACTION	
						END CATCH ";

			using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
			{
				conn.Open();
				SqlCommand cmd = new SqlCommand(sql, conn);
				cmd.Parameters.Add(new SqlParameter("@BookName", insertdata.BookName));
				cmd.Parameters.Add(new SqlParameter("@BookClass", insertdata.BookClass));
				cmd.Parameters.Add(new SqlParameter("@BookAuthor", insertdata.BookAuthor));
				cmd.Parameters.Add(new SqlParameter("@BoughtDate", insertdata.BoughtDate));
				cmd.Parameters.Add(new SqlParameter("@BookPublisher", insertdata.BookPublisher));
				cmd.Parameters.Add(new SqlParameter("@Note", insertdata.Note));//ExecuteScalar 執行一個SQL命令返回結果集的第一列的第一行即id
				cmd.ExecuteNonQuery();
				conn.Close();
			}
		}

		public List<Models.BookRecord> GetLendRecord(int BookId)
		{
			string sql = @" SELECT CONVERT(varchar,blr.LEND_DATE, 111) AS 借閱日期,blr.KEEPER_ID AS 借閱人員編號,
							mm.USER_ENAME AS 英文姓名, mm.USER_CNAME AS 中文姓名
							FROM BOOK_LEND_RECORD AS blr
								INNER JOIN MEMBER_M AS mm
									ON blr.KEEPER_ID =mm.USER_ID
							WHERE blr.BOOK_ID=@BookId
							ORDER BY 借閱日期 DESC";

			DataTable dt = new DataTable();
			using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
			{
				conn.Open();
				SqlCommand cmd = new SqlCommand(sql, conn);
				cmd.Parameters.Add(new SqlParameter("@BookId", BookId));
				SqlDataAdapter sqlAdapter = new SqlDataAdapter(cmd);
				sqlAdapter.Fill(dt);
				conn.Close();
			}
			return this.LendToModelList(dt);
		}
		public List<Models.BookRecord> LendToModelList(DataTable dt)
		{
			List<Models.BookRecord> result = new List<BookRecord>();
			foreach (DataRow row in dt.Rows)
			{
				result.Add(new BookRecord()
				{
					LendDate = row["借閱日期"].ToString(),
					BookKeeperId = row["借閱人員編號"].ToString(),
					UserEname = row["英文姓名"].ToString(),
					UserCname = row["中文姓名"].ToString()
				});
			}
			return result;
		}
		//model binding 要和SELECTLIST值一樣才有dropdownlist預設值

		public Models.BookUpdate GetUpdateBook(int id)
		{
			string sql = @"SELECT bd.BOOK_NAME AS 書名,bd.BOOK_AUTHOR AS 作者,bd.BOOK_PUBLISHER AS 出版商,bd.BOOK_NOTE AS 內容簡介,
							CONVERT(varchar, bd.BOOK_BOUGHT_DATE, 111) AS 購書日期,bc.BOOK_CLASS_ID AS 圖書類別,
							bcd.CODE_ID AS 借閱狀態,mm.USER_ID AS 借閱人
						   FROM BOOK_DATA bd
								INNER JOIN  BOOK_CLASS bc
									ON bd.BOOK_CLASS_ID = bc.BOOK_CLASS_ID
								INNER JOIN BOOK_CODE bcd
									ON bd.BOOK_STATUS = bcd.CODE_ID AND bcd.CODE_TYPE = 'BOOK_STATUS'
								LEFT OUTER JOIN  MEMBER_M mm
									ON bd.BOOK_KEEPER = mm.[USER_ID]
							WHERE BOOK_ID = @BOOK_ID";
			DataTable dt = new DataTable();
			using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
			{
				conn.Open();
				SqlCommand cmd = new SqlCommand(sql, conn);
				cmd.Parameters.Add(new SqlParameter("@BOOK_ID", id));
				SqlDataAdapter sqlAdapter = new SqlDataAdapter(cmd);
				sqlAdapter.Fill(dt);
				conn.Close();
			}
			return this.ToModel(dt);
		}
		public Models.BookUpdate ToModel(DataTable dt)
		{
			Models.BookUpdate result = new Models.BookUpdate();
			foreach(DataRow row in dt.Rows )
			{
				result.BookName = row["書名"].ToString();
				result.BookAuthor =row["作者"].ToString();
				result.BookPublisher = row["出版商"].ToString();
				result.Note = row["內容簡介"].ToString();
				result.BoughtDate = row["購書日期"].ToString();
				result.BookClassId = row["圖書類別"].ToString();
				result.BookStatusId = row["借閱狀態"].ToString();
				result.BookKeeperId = row["借閱人"].ToString();
			}


			return result;
		}
		public void UpdateBook(int val, Models.BookUpdate updatedata)
		{
			string sql = @"
							BEGIN TRY
								BEGIN TRANSACTION		
									UPDATE BOOK_DATA 
									SET BOOK_NAME= @BOOK_NAME,
										BOOK_AUTHOR = @BOOK_AUTHOR,
										BOOK_PUBLISHER = @BOOK_PUBLISHER,
										BOOK_NOTE = @BOOK_NOTE,
										BOOK_BOUGHT_DATE = @BOOK_BOUGHT_DATE,
										BOOK_CLASS_ID = @BOOK_CLASS_ID,
										BOOK_STATUS = @BOOK_STATUS_ID,
										BOOK_KEEPER = @BOOK_KEEPER_ID,
										MODIFY_DATE = GETDATE()
									WHERE BOOK_ID = @BOOK_ID
								COMMIT TRANSACTION
							End TRY
							BEGIN CATCH
								SELECT
									ERROR_NUMBER() AS ErrorNumber,
									ERROR_SEVERITY() AS ErrorSeverity,
									ERROR_STATE() as ErrorState,
									ERROR_PROCEDURE() as ErrorProcedure,
									ERROR_LINE() as ErrorLine,
									ERROR_MESSAGE() as ErrorMessage
								ROLLBACK TRANSACTION
							END CATCH ";



			using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
			{
				conn.Open();
				SqlCommand cmd = new SqlCommand(sql, conn);
				cmd.Parameters.Add(new SqlParameter("@BOOK_NAME", updatedata.BookName));
				cmd.Parameters.Add(new SqlParameter("@BOOK_AUTHOR", updatedata.BookAuthor));
				cmd.Parameters.Add(new SqlParameter("@BOOK_PUBLISHER", updatedata.BookPublisher));
				cmd.Parameters.Add(new SqlParameter("@BOOK_NOTE", updatedata.Note));
				cmd.Parameters.Add(new SqlParameter("@BOOK_BOUGHT_DATE", updatedata.BoughtDate));//
				cmd.Parameters.Add(new SqlParameter("@BOOK_CLASS_ID", updatedata.BookClassId));//
				cmd.Parameters.Add(new SqlParameter("@BOOK_STATUS_ID", updatedata.BookStatusId));//
				cmd.Parameters.Add(new SqlParameter("@BOOK_KEEPER_ID", updatedata.BookKeeperId == null ? string.Empty : updatedata.BookKeeperId));//
				cmd.Parameters.Add(new SqlParameter("@BOOK_ID", val));
				cmd.ExecuteNonQuery();
				conn.Close();
			}

		}
		public string DecideDelete(string BookID)
		{
			string sql = @"SELECT BOOK_STATUS 
							FROM BOOK_DATA
							WHERE BOOK_ID=@BookID";
			DataTable dt = new DataTable();
			string StatusId = "";
			using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
			{
				conn.Open();
				SqlCommand cmd = new SqlCommand(sql, conn);
				cmd.Parameters.Add(new SqlParameter("@BookID", BookID));
				SqlDataAdapter sqlAdapter = new SqlDataAdapter(cmd);
				sqlAdapter.Fill(dt);
				foreach (DataRow row in dt.Rows)
				{
					StatusId = row["BOOK_STATUS"].ToString();
				}
				conn.Close();
			}

			if (StatusId == "B" || StatusId == "C")
			{
				return "已借出不可刪除";
			}
			else
			{
				int id = Convert.ToInt32(BookID);
				this.DeleteBook(id);
				return "刪除成功";
			}
		}

		public void DeleteBook(int BookId)
		{
			try
			{
				string sql =
					@"
					 BEGIN TRY
						BEGIN TRANSACTION
							Delete FROM BOOK_DATA Where BOOK_ID=@BookId
						COMMIT TRANSACTION
					 End TRY
					 BEGIN CATCH
						SELECT
							ERROR_NUMBER() AS ErrorNumber,
							ERROR_SEVERITY() AS ErrorSeverity,
							ERROR_STATE() as ErrorState,
							ERROR_PROCEDURE() as ErrorProcedure,
							ERROR_LINE() as ErrorLine,
							ERROR_MESSAGE() as ErrorMessage
						ROLLBACK TRANSACTION
					 END CATCH ";
				using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
				{
					conn.Open();
					SqlCommand cmd = new SqlCommand(sql, conn);
					cmd.Parameters.Add(new SqlParameter("@BookId", BookId));
					cmd.ExecuteNonQuery();
					conn.Close();
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public Models.BookUpdate GetBookDetail(int id)
		{
			string sql = @"SELECT bd.BOOK_ID AS ID,bd.BOOK_NAME AS 書名,bd.BOOK_AUTHOR AS 作者,bd.BOOK_PUBLISHER AS 出版商,bd.BOOK_NOTE AS 內容簡介,
							CONVERT(varchar, bd.BOOK_BOUGHT_DATE, 111) AS 購書日期,bc.BOOK_CLASS_NAME AS 圖書類別,
							bcd.CODE_NAME AS 借閱狀態,mm.USER_ENAME+'-'+mm.USER_CNAME AS 借閱人
						   FROM BOOK_DATA bd
								INNER JOIN  BOOK_CLASS bc
									ON bd.BOOK_CLASS_ID =bc.BOOK_CLASS_ID
								INNER JOIN BOOK_CODE bcd
									ON bd.BOOK_STATUS = bcd.CODE_ID AND bcd.CODE_TYPE = 'BOOK_STATUS'
								LEFT OUTER JOIN  MEMBER_M mm
									ON bd.BOOK_KEEPER = mm.[USER_ID]
							WHERE BOOK_ID = @BOOK_ID";
			DataTable dt = new DataTable();
			using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
			{
				conn.Open();
				SqlCommand cmd = new SqlCommand(sql, conn);
				cmd.Parameters.Add(new SqlParameter("@BOOK_ID", id));
				SqlDataAdapter sqlAdapter = new SqlDataAdapter(cmd);
				sqlAdapter.Fill(dt);
				conn.Close();
			}
			return this.BookDetailModel(dt);
		}
		public Models.BookUpdate BookDetailModel(DataTable dt)
		{
			Models.BookUpdate result = new Models.BookUpdate();
			result.BookID = dt.Rows[0]["ID"].ToString();
			result.BookName = dt.Rows[0]["書名"].ToString();
			result.BookAuthor = dt.Rows[0]["作者"].ToString();
			result.BookPublisher = dt.Rows[0]["出版商"].ToString();
			result.Note = dt.Rows[0]["內容簡介"].ToString();
			result.BoughtDate = dt.Rows[0]["購書日期"].ToString();
			result.BookClassId = dt.Rows[0]["圖書類別"].ToString();
			result.BookStatusId = dt.Rows[0]["借閱狀態"].ToString();
			result.BookKeeperId = dt.Rows[0]["借閱人"].ToString();

			return result;
		}

		public void InsertLendRecord(Models.LendRecordInsert insertdata)
		{
			string sql = @"
						BEGIN TRY
							BEGIN TRANSACTION
								INSERT INTO BOOK_LEND_RECORD
								 (
									BOOK_ID,KEEPER_ID,LEND_DATE,CRE_DATE,CRE_USR,MOD_DATE,MOD_USR
								 )
								VALUES
								(
									 @BookId,@KeeperId, GETDATE(), 
									 GETDATE(), '123', GETDATE(), 
									'123'
								)
							COMMIT TRANSACTION
						End TRY
						BEGIN CATCH
							SELECT
								ERROR_NUMBER() AS ErrorNumber,
								ERROR_SEVERITY() AS ErrorSeverity,
								ERROR_STATE() as ErrorState,
								ERROR_PROCEDURE() as ErrorProcedure,
								ERROR_LINE() as ErrorLine,
								ERROR_MESSAGE() as ErrorMessage
							ROLLBACK TRANSACTION	
						END CATCH ";

			using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
			{
				conn.Open();
				SqlCommand cmd = new SqlCommand(sql, conn);
				cmd.Parameters.Add(new SqlParameter("@BookId", insertdata.BookID));
				cmd.Parameters.Add(new SqlParameter("@KeeperId", insertdata.BookKeeperId));
				//ExecuteScalar 執行一個SQL命令返回結果集的第一列的第一行即id
				cmd.ExecuteNonQuery();
				conn.Close();
			}
		}

	}

}