﻿using NForum.Core;
using NForum.Core.Abstractions.Data;
using NForum.Database.EntityFramework.Repositories;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;

namespace NForum.Database.EntityFramework {

	public class EntityFrameworkDataStore : IDataStore {
		protected readonly IRepository<Dbos.Category> categoryRepository;
		protected readonly IRepository<Dbos.Forum> forumRepository;

		public EntityFrameworkDataStore(IRepository<Dbos.Category> categoryRepository,
										IRepository<Dbos.Forum> forumRepository) {
			this.categoryRepository = categoryRepository;
			this.forumRepository = forumRepository;
		}

		public Category CreateCategory(String name, Int32 sortOrder, String description) {
			if (String.IsNullOrWhiteSpace(name)) {
				throw new ArgumentNullException(nameof(name));
			}
			Dbos.Category model = this.categoryRepository.Create(new Dbos.Category {
				Name = name,
				SortOrder = sortOrder,
				Description = description
			});

			return model.ToModel();
		}

		public Forum CreateForum(String categoryId, String name, Int32 sortOrder, String description) {
			Guid id;
			if (!Guid.TryParse(categoryId, out id)) {
				throw new ArgumentException(nameof(categoryId));
			}

			if (String.IsNullOrWhiteSpace(name)) {
				throw new ArgumentNullException(nameof(name));
			}

			Dbos.Category parent = this.categoryRepository.FindById(id);
			if (parent == null) {
				throw new ArgumentException(nameof(categoryId));
			}

			return this.CreateForum(parent, null, name, sortOrder, description);
		}

		private Forum CreateForum(Dbos.Category category, Dbos.Forum parentForum, String name, Int32 sortOrder, String description) {
			Dbos.Forum newForum = new Dbos.Forum {
				Category = category,
				Name = name,
				SortOrder = sortOrder,
				Description = description,
				Level = 0
			};
			if (parentForum != null) {
				newForum.ParentForum = parentForum;
				newForum.Level = parentForum.Level + 1;
			}

			Dbos.Forum model = this.forumRepository.Create(newForum);
			return model.ToModel();
		}

		public Forum CreateSubForum(String parentForumId, String name, Int32 sortOrder, String description) {
			Guid id;
			if (!Guid.TryParse(parentForumId, out id)) {
				throw new ArgumentException(nameof(parentForumId));
			}

			if (String.IsNullOrWhiteSpace(name)) {
				throw new ArgumentNullException(nameof(name));
			}

			Dbos.Forum parent = this.forumRepository.FindById(id);
			if (parent == null) {
				throw new ArgumentException(nameof(parentForumId));
			}

			return this.CreateForum(parent.Category, parent, name, sortOrder, description);
		}

		public Boolean DeleteCategory(String categoryId) {
			// TODO: All sub-forums? All topics, replies, attachments etc??
			Guid id;
			if (!Guid.TryParse(categoryId, out id)) {
				throw new ArgumentException(nameof(categoryId));
			}
			this.categoryRepository.DeleteById(id);
			// TODO: ?!?!
			return true;
		}

		public bool DeleteForum(String forumId) {
			// TODO: All sub-forums? All topics, replies, attachments etc??
			Guid id;
			if (!Guid.TryParse(forumId, out id)) {
				throw new ArgumentException(nameof(forumId));
			}
			this.forumRepository.DeleteById(id);
			// TODO: ?!?!
			return true;
		}

		public Category FindCategoryById(String categoryId) {
			Guid id;
			if (!Guid.TryParse(categoryId, out id)) {
				throw new ArgumentException(nameof(categoryId));
			}
			Dbos.Category category = this.categoryRepository.FindById(id);
			return category.ToModel();
		}

		public Forum FindForumById(String forumId) {
			Guid id;
			if (!Guid.TryParse(forumId, out id)) {
				throw new ArgumentException(nameof(forumId));
			}
			Dbos.Forum forum = this.forumRepository.FindById(id);
			return forum.ToModel();
		}

		public Category UpdateCategory(String categoryId, String name, Int32 sortOrder, String description) {
			Guid id;
			if (!Guid.TryParse(categoryId, out id)) {
				throw new ArgumentException(nameof(categoryId));
			}
			Dbos.Category category = this.categoryRepository.FindById(id);

			category.Name = name;
			category.SortOrder = sortOrder;
			category.Description = description;

			category = this.categoryRepository.Update(category);
			return category.ToModel();
		}

		public Forum UpdateForum(String forumId, String name, Int32 sortOrder, String description) {
			Guid id;
			if (!Guid.TryParse(forumId, out id)) {
				throw new ArgumentException(nameof(forumId));
			}
			if (String.IsNullOrWhiteSpace(name)) {
				throw new ArgumentNullException(nameof(name));
			}
			Dbos.Forum forum = this.forumRepository.FindById(id);

			forum.Name = name;
			forum.SortOrder = sortOrder;
			forum.Description = description;

			forum = this.forumRepository.Update(forum);
			return forum.ToModel();
		}

		public IEnumerable<Category> FindCategoriesPlus2Levels() {
			IEnumerable<Dbos.Category> categories = this.categoryRepository.FindAll()
				.Include(c => c.Forums)
				.ToList();
			// TODO: Limit on level!!

			List<Category> output = new List<Category>();
			categories.ToList().ForEach(c => {
				Category cat = c.ToModel();
				PopulatedLevel0(c, cat);
				output.Add(cat);
			});

			return output;
		}

		private static void PopulatedLevel0(Dbos.Category c, Category cat) {
			List<Forum> forums = new List<Forum>();

			c.Forums.Where(f => f.Level == 0).ToList().ForEach(f => {
				Forum forum = f.ToModel();
				forum.Category = cat;

				PopulateLevel1(cat, f, forum);

				forums.Add(forum);
			});

			cat.Forums = forums;
		}

		private static void PopulateLevel1(Category cat, Dbos.Forum f, Forum forum) {
			List<Forum> subForums = new List<Forum>();

			f.SubForums.Where(fc => fc.Level == 1 && fc.ParentForumId == f.Id).ToList().ForEach(fc => {
				Forum child = fc.ToModel();
				child.Category = cat;
				child.ParentForum = forum;
				subForums.Add(child);
			});

			forum.SubForums = subForums;
		}

		public Forum FindForumPlus2Levels(String forumId) {
			Guid id;
			if (!Guid.TryParse(forumId, out id)) {
				throw new ArgumentException(nameof(forumId));
			}
			IEnumerable<Dbos.Forum> forums = this.forumRepository.FindAll()
				.Include(f => f.Category)
				.Where(f => f.CategoryId == f.CategoryId)
				.ToList();

			Dbos.Forum forum = forums.SingleOrDefault(f => f.Id == id);
			if (forum == null) {
				// TODO:
				throw new ApplicationException();
			}

			Category cat = forum.Category.ToModel();
			Forum output = forum.ToModel();
			output.Category = cat;
			Forum dest = output;
			Dbos.Forum parentForum = forum.ParentForum;
			while (parentForum != null) {
				dest.ParentForum = parentForum.ToModel();
				dest.ParentForum.Category = cat;

				dest = dest.ParentForum;
				parentForum = parentForum.ParentForum;
			}

			output.Category = forum.Category.ToModel();
			List<Forum> subForums = new List<Forum>();
			forum.SubForums.ToList().ForEach(f => {
				Forum subForum = f.ToModel();
				subForum.ParentForum = output;
				subForum.Category = cat;

				List<Forum> children = new List<Forum>();
				f.SubForums.ToList().ForEach(fc => {
					Forum child = fc.ToModel();
					child.ParentForum = subForum;
					child.Category = cat;

					children.Add(child);
				});

				subForum.SubForums = children;
				subForums.Add(subForum);
			});
			output.SubForums = subForums;

			return output;
		}

		public Category FindCategoryPlus2Levels(String categoryId) {
			Guid id;
			if (!Guid.TryParse(categoryId, out id)) {
				throw new ArgumentException(nameof(categoryId));
			}

			Dbos.Category category = this.categoryRepository.FindAll()
				.Include(c => c.Forums)
				.SingleOrDefault(c => c.Id == id);

			Category output = category.ToModel();

			PopulatedLevel0(category, output);

			return output;
		}

		public IEnumerable<Category> FindAll() {
			return this.categoryRepository.FindAll()
				.ToList()
				.Select(c => c.ToModel())
				.OrderBy(c => c.SortOrder);
		}
	}
}
