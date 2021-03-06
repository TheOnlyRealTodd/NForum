﻿using System;

namespace NForum.Database.EntityFramework {

	public class DatabaseConfiguration {
		protected readonly String tableNamePrefix = String.Empty;
		protected readonly String tableNamePostfix = String.Empty;

		private DatabaseConfiguration() {
			// TODO: Load from somewhere!
			this.tableNamePrefix = String.Empty;
			this.tableNamePostfix = String.Empty;
		}

		public String GetTableName(Type type) {
			// TODO: Validate postfix/prefix is valid SQL and not an injection attempt!
			return String.Format("{0}{1}{2}", this.tableNamePrefix, type.Name, this.tableNamePostfix);
		}

		private static DatabaseConfiguration dbConf = null;
		private static Object instanceLock = new Object();
		public static DatabaseConfiguration Instance {
			get {
				if (dbConf == null) {
					lock (instanceLock) {
						if (dbConf == null) {
							dbConf = new DatabaseConfiguration();
						}
					}
				}

				return dbConf;
			}
		}
	}
}
