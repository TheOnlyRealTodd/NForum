﻿using NForum.Core.Abstractions.Data;
using NForum.Core.Abstractions.Events;
using NForum.Core.Abstractions.Providers;
using NForum.Core.Abstractions.Services;
using NForum.Core.Services;
using NForum.Database.EntityFramework;
using NForum.Database.EntityFramework.Repositories;
using System;
using System.Data.Common;

namespace NForum.Tests.Common {

	public static class Initializer {
		private static NForumDbContext dbContext;
		/* DATA */
		public static IDataStore DataStore { get; private set; }
		/* SERVICES */
		public static ICategoryService CategoryService { get; private set; }
		public static IForumService ForumService { get; private set; }
		public static ITopicService TopicService { get; private set; }
		public static IPermissionService PermissionService { get; private set; }
		public static ILoggingService LoggingService { get; private set; }
		public static IUIService UIService { get; private set; }
		/* PROVIDERS */
		public static IUserProvider UserProvider { get; private set; }
		/* OTHERS */
		public static IEventPublisher EventPublisher { get; private set; }

		public static void Initialize() {
			DbConnection connection = Effort.DbConnectionFactory.CreateTransient();
			dbContext = new NForumDbContext(connection);

			DataStore = new NForum.Database.EntityFramework.EntityFrameworkDataStore(
					new GenericRepository<NForum.Database.EntityFramework.Dbos.Category>(dbContext),
					new GenericRepository<NForum.Database.EntityFramework.Dbos.Forum>(dbContext),
					new GenericRepository<NForum.Database.EntityFramework.Dbos.Topic>(dbContext),
					new GenericRepository<NForum.Database.EntityFramework.Dbos.Reply>(dbContext),
					new GenericRepository<NForum.Database.EntityFramework.Dbos.ForumUser>(dbContext)
				);

			GenericRepository<NForum.Database.EntityFramework.Dbos.ForumUser> fuRepo = new GenericRepository<Database.EntityFramework.Dbos.ForumUser>(dbContext);
			NForum.Database.EntityFramework.Dbos.ForumUser fu = fuRepo.Create(new Database.EntityFramework.Dbos.ForumUser {
				Deleted = false,
				EmailAddress = "mr-fake@fake.com",
				ExternalId = "todo",
				Fullname = "mrfake",
				Username = "Mr.Fake User",
				UseFullname = true
			});

			PermissionService = new PermissionService();
			LoggingService = new LoggingService(new FakeLogger(), new FakeLogger());
			UserProvider = new FakeUserProvider(fu);
			EventPublisher = new FakeEventPublisher();

			CategoryService = new CategoryService(DataStore, PermissionService, LoggingService, UserProvider, EventPublisher);
			ForumService = new ForumService(DataStore, PermissionService, LoggingService, UserProvider, EventPublisher);
			TopicService = new TopicService(DataStore, PermissionService, LoggingService, UserProvider, EventPublisher);
			UIService = new UIService(DataStore, PermissionService, LoggingService, UserProvider, new FakeSettings());
		}
	}
}
