using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebTester.Models
{
	public class Result
	{
		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }

		public List<QuestionResult> Questions { get; set; }

		public Result()
		{
			Questions = new List<QuestionResult>();
			StartDate = DateTime.Now;
		}
	}

	public class QuestionResult
	{
		public int Id_Question { get; set; }

		public DateTime AnswerDate { get; set; }

		public List<int> Id_Answers { get; set; }

		public QuestionResult()
		{
			Id_Answers = new List<int>();
			AnswerDate = DateTime.Now;
		}
	}
}