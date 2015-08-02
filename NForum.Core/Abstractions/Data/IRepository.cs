﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NForum.Core.Abstractions.Data {

	public interface IRepository<TEntity> where TEntity : class {
		TEntity Create(TEntity newEntity);
		TEntity Read(Expression<Func<TEntity, Boolean>> expression);
		IEnumerable<TEntity> ReadAll();
		TEntity Update(TEntity entity);
		void Delete(TEntity entity);
		//void Delete(Int32 id);
	}
}