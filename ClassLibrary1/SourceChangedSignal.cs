﻿using System;
using Newtonsoft.Json;

namespace NetBuild.Queue.Engine
{
	public class SourceChangedSignal : ISignal
	{
		[JsonProperty("id")]
		public string ChangeId { get; set; }

		[JsonProperty("path")]
		public string ChangePath { get; set; }

		[JsonProperty("author")]
		public string ChangeAuthor { get; set; }

		[JsonProperty("type")]
		public string ChangeType { get; set; }

		[JsonProperty("comment")]
		public string ChangeComment { get; set; }

		[JsonProperty("date")]
		public DateTime ChangeDate { get; set; }
	}
}
