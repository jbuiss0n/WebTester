using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace WebTester.Models
{
	public class Quiz
	{
		[XmlArray(ElementName = "questions")]
		[XmlArrayItem(ElementName = "question")]
		public List<QuizQuestion> Questions { get; set; }

		public Quiz()
		{
			Questions = new List<QuizQuestion>();
		}

		public void Randomize()
		{
			Questions = Questions.OrderBy(q => Guid.NewGuid()).ToList();
			Questions.ForEach(q => q.Randomize());
		}
	}

	public class QuizQuestion
	{
		[XmlAttribute(AttributeName = "id")]
		public int Id_Question { get; set; }

		[XmlElement(ElementName = "description")]
		public string Description { get; set; }

		[XmlArray(ElementName = "answers")]
		[XmlArrayItem(ElementName = "answer")]
		public List<QuizAnswer> Answers { get; set; }

		public QuizQuestion()
		{
			Answers = new List<QuizAnswer>();
		}

		public void Randomize()
		{
			Answers = Answers.OrderBy(a => Guid.NewGuid()).ToList();
		}
	}

	public class QuizAnswer
	{
		[XmlAttribute(AttributeName = "id")]
		public int Id_Answer { get; set; }

		[XmlAttribute(AttributeName = "iscorrect")]
		public bool IsCorrect { get; set; }

		[XmlText]
		public string Description { get; set; }
	}
}