﻿using NForum.Core;
using NForum.Core.Abstractions.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace NForum.Persistence.EntityFramework.Repositories {

	public class CategoryRepository : RepositoryBase<Category>, ICategoryRepository {

		public CategoryRepository(UnitOfWork uow) : base(uow) { }

		public Category ByName(String name) {
			return this.set.FirstOrDefault(b => b.Name == name);
		}

		public Category ByTopic(Topic topic) {
			return this.set.Include(c => c.Forums).FirstOrDefault(c => c.Forums.Any(f => f.Id == topic.ForumId) == true);
		}

		public Category ByForum(Forum forum) {
			return this.set.FirstOrDefault(c => c.Id == forum.CategoryId);
		}
	}
}