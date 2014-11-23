using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;
using WebTester.Data;
using WebTester.Models;

namespace WebTester.Controllers
{
	public class QuizController : Controller
	{
		private readonly TimeSpan QUIZ_MAX_TIME = TimeSpan.FromMinutes(15);
		private readonly ILog m_logger = LogManager.GetLogger("Quiz");

		public static Quiz SessionQuiz
		{
			get
			{
				return (Quiz)System.Web.HttpContext.Current.Session["SessionQuiz"];
			}
			set
			{
				System.Web.HttpContext.Current.Session["SessionQuiz"] = value;
			}
		}

		public static Result SessionResult
		{
			get
			{
				return (Result)System.Web.HttpContext.Current.Session["SessionResult"];
			}
			set
			{
				System.Web.HttpContext.Current.Session["SessionResult"] = value;
			}
		}

		public ActionResult Create(string key)
		{
			if (key != "dbV31P075w94B2f7Z2aj905PB65q596a")
				return Redirect("http://www.example.uri/");

			return View();
		}

		[HttpPost]
		public ActionResult Create(string key, string description)
		{
			using (var bdd = new BDDEntities())
			{
				var token = bdd.Token.Create();
				token.UID = Guid.NewGuid();
				token.Description = description;
				token.IsAvailable = true;
				token.dCreate = DateTime.Now;
				token.dExpire = DateTime.Now.AddDays(1);
				bdd.Token.Add(token);
				bdd.SaveChanges();

				return View(token);
			}
		}

		public ActionResult Index(Guid uid)
		{
			using (var bdd = new BDDEntities())
			{
				var token = bdd.Token.FirstOrDefault(t => t.UID == uid);

				if (token == null || !token.IsAvailable || token.dExpire < DateTime.Now)
					return Redirect("http://www.example.uri/");

				token.IsAvailable = false;
				bdd.SaveChanges();
			}

			SessionQuiz = GetQuizFromXml(Server.MapPath("~/Content/xml/quiz.xml"));
			SessionResult = null;

			SessionQuiz.Randomize();

			ViewBag.Message = String.Format("Ce test comprend {0} questions. Le temps maximum est de {1} minutes. Le chronomètre débutera quand vous aurez cliqué sur \"Commencer le test\"."
				, SessionQuiz.Questions.Count
				, QUIZ_MAX_TIME);

			return View();
		}

		public ActionResult Play()
		{
			if (SessionQuiz == null)
				return RedirectToAction("Index");

			SessionResult = SessionResult ?? new Result();

			var model = SessionQuiz.Questions.FirstOrDefault(q => !SessionResult.Questions.Exists(r => r.Id_Question == q.Id_Question));

			if (model == null)
			{
				SessionResult.EndDate = DateTime.Now;
				return Redirect("Finish");
			}

			return View(model);
		}

		[HttpPost]
		public ActionResult Answer(int Question, int[] Answer)
		{
			if (SessionResult == null)
				return new EmptyResult();

			if (DateTime.Now >= SessionResult.StartDate.Add(QUIZ_MAX_TIME))
				return PartialView("_Finish");

			SessionResult.Questions.Add(new QuestionResult { Id_Question = Question, Id_Answers = Answer != null ? Answer.ToList() : new List<int>() });

			var model = SessionQuiz.Questions.FirstOrDefault(q => !SessionResult.Questions.Exists(r => r.Id_Question == q.Id_Question));

			if (model == null)
			{
				SessionResult.EndDate = DateTime.Now;
				//SendResultByEmail();
				return PartialView("_Finish");
			}

			return PartialView("_Question", model);
		}

		public ActionResult Finish()
		{
			if (SessionQuiz == null || SessionResult == null)
				return RedirectToAction("Index");

			SendResultByEmail();

			return View();
		}

		private void SendResultByEmail()
		{
			try
			{
				var client = new SmtpClient("smtp.viaduc.fr", 587);
				client.EnableSsl = false;
				client.UseDefaultCredentials = false;
				client.Credentials = new NetworkCredential("email@example.uri", "p@ss");

				MailMessage message = new MailMessage();
				message.To.Add("email@example.uri");
				message.From = new MailAddress("email@example.uri");
				message.Subject = "Résultat de test";
				message.Body = RenderViewToString("~/Views/Quiz/Result.cshtml");
				message.IsBodyHtml = true;
				client.Send(message);
			}
			catch (Exception ex)
			{
				m_logger.Error("SendResultByEmail", ex);
			}
		}

		private static Quiz GetQuizFromXml(string xmlPath)
		{
			using (var stream = new StreamReader(xmlPath))
			{
				XmlSerializer serializer = new XmlSerializer(typeof(Quiz), new XmlRootAttribute("quiz"));
				return (Quiz)serializer.Deserialize(stream);
			}
		}

		private string RenderViewToString(string view)
		{
			using (var sw = new StringWriter())
			{
				var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, view);
				var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
				viewResult.View.Render(viewContext, sw);
				viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
				return sw.GetStringBuilder().ToString();
			}
		}
	}
}
