﻿using System;
using System.Web.Mvc;

namespace NForum.Web {

	public class FilterConfig {

		public static void RegisterGlobalFilters(GlobalFilterCollection filters) {
			filters.Add(new HandleErrorAttribute());
		}
	}
}
