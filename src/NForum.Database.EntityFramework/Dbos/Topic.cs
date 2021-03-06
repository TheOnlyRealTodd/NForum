﻿using System;
using System.Collections.Generic;

namespace NForum.Database.EntityFramework.Dbos {

	public class Topic {
		public Guid Id { get; set; }
		public NForum.Core.TopicState State { get; set; }
		public NForum.Core.TopicType Type { get; set; }
		public String CustomData { get; set; }

		public Guid MessageId { get; set; }
		public virtual Message Message { get; set; }

		public Guid ForumId { get; set; }
		public virtual Forum Forum { get; set; }

		public Guid? LatestReplyId { get; set; }
		public virtual Reply LatestReply { get; set; } 

		public virtual ICollection<Reply> Replies { get; set; }
	}
}
