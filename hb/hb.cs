//
// hb.cs: wannabe clone of ab (apache benchmark)
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.IO;
using System.Net;
using System.Reflection;
using Mono.GetOptions;

namespace Mono.Bench
{
	class HttpWebRequestBench
	{
		static Settings settings;
		static Uri testUri;
		static string server;
		static void Connect (string uri, bool stats)
		{
			HttpWebRequest req = (HttpWebRequest) WebRequest.Create (uri);
			req.AllowAutoRedirect = false;
			HttpWebResponse wr = (HttpWebResponse) req.GetResponse ();
			Stream s = wr.GetResponseStream ();
			long length = wr.ContentLength;
			if (length >= 0) {
				byte [] buffer = new byte [length];
				s.Read (buffer, 0, (int) length);
				/*int r = s.Read (buffer, 0, (int) length);
				Console.Write (System.Text.Encoding.Default.GetString (buffer, 0, r));*/
				
			} else {
				byte [] buffer = new byte [4096];
				length = 0;
				int read = 0;
				while ((read = s.Read (buffer, 0, 4096)) != 0) {
					//Console.Write (System.Text.Encoding.Default.GetString (buffer, 0, read));
					length += read;
				}
			}
			s.Close ();
			if (stats) {
				Counters.TotalBytesRead += length;
				Counters.TotalBytesRead += wr.Headers.ToString ().Length;
				Counters.TotalBytesRead += wr.Headers.ToString ().Length;
				// 17 -> "HTTP/1.x YYY \r\n".Length
				Counters.TotalBytesRead += 17 + wr.StatusDescription.Length;
			}

			if (server == null)
				server = wr.Headers ["Server"];
		}
		
		static void Report ()
		{
			TimeSpan total = new TimeSpan (Counters.End - Counters.Start);
			Console.WriteLine ("Server software:\t{0}", server);
			Console.WriteLine ("Server hostname:\t{0}", testUri.Host);
			Console.WriteLine ("Server port:\t\t{0}", testUri.Port);
			Console.WriteLine ();
			Console.WriteLine ("Time taken for tests:\t{0}", total);
			Console.WriteLine ("Requests:\t\t{0}", settings.NRequests);
			double rps = settings.NRequests / total.TotalSeconds;
			Console.WriteLine ("Requests/s:\t\t{0}", rps);
			Console.WriteLine ("Bytes received:\t\t{0}", Counters.TotalBytesRead);
			Console.WriteLine ("Bytes/request (avg.):\t{0}", Counters.TotalBytesRead / settings.NRequests);
			Console.WriteLine ("Transfer rate:\t\t{0}", Counters.TotalBytesRead / total.TotalSeconds);
		}

		static int Main (string [] args)
		{
			settings = new Settings (args);
			if (settings.NRequests <= 0)
				settings.NRequests = 1;

			args = settings.RemainingArguments;
			if (args.Length != 1) {
				settings.DoUsage ();
				return 1;
			}

			testUri = new Uri (args [0]);
			/* pre-jit */
			try {
				Connect (args [0], false);
			} catch (WebException e) {
				Console.WriteLine (e.Message);
				if (e.Response != null) {
					StreamReader r = new StreamReader (e.Response.GetResponseStream ());
					Console.WriteLine (r.ReadToEnd ());
				}
				return 0;
			}
			/**/

			Counters.Start = DateTime.Now.Ticks;
			for (int i = 0; i < settings.NRequests; ++i) {
				Connect (args [0], true);
			}

			Counters.End = DateTime.Now.Ticks;
			Report ();
			return 0;
		}
	}

	sealed class Counters
	{
		public static long Start;
		public static long End;
		public static long TotalBytesRead;
	}
	
	sealed class Settings : Options
	{
		[Option ("Be verbose", 'v', "verbose")]
		public bool Verbose = false;
		[Option ("Number of requests", 'n', "requests")]
		public int NRequests = 1;


		public Settings (string [] args)
		{
			this.ParsingMode = OptionsParsingMode.Both;
			ProcessArgs (args);
		}
		
		[Option("Show usage syntax", 'h', "usage")]
		public override WhatToDoNext DoUsage()
		{
			base.DoUsage();
			return WhatToDoNext.AbandonProgram; 
		}

		public override WhatToDoNext DoHelp() // uses parent´s OptionAttribute as is
		{
			base.DoHelp();
			return WhatToDoNext.AbandonProgram; 
		}
	}
}

