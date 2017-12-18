﻿using System;
using Autofac;
using FutureState.Data;
using FutureState.Data.Keys;

namespace FutureState.Autofac.Data
{
    /// <summary>
    ///     Extension methods to resolve data access objects such as linq readers and repositories.
    /// </summary>
    public static class IComponentContextEx
    {
        public static Func<ISession, ILinqReader<TEntity, TKey>> GetReader<TEntity, TKey>(this IComponentContext ctx,
            IEntityIdProvider<TEntity, TKey> idGenerator)
            where TEntity : class, new()
        {
            return session => ctx.Resolve<ILinqReader<TEntity, TKey>>(
                new TypedParameter(typeof(ISession), session),
                new TypedParameter(typeof(IEntityIdProvider<TEntity>), idGenerator));
        }

        public static Func<ISession, ILinqReader<TEntity, TKey>> GetReader<TEntity, TKey>(this IComponentContext ctx)
            where TEntity : class, new()
        {
            return GetReader(ctx, new NoOpEntityIdProvider<TEntity, TKey>());
        }

        public static Func<ISession, IRepositoryLinq<TEntity, TKey>> GetRepository<TEntity, TKey>(
            this IComponentContext ctx, IEntityIdProvider<TEntity, TKey> idGenerator)
            where TEntity : class, new()
        {
            return session => ctx.Resolve<IRepositoryLinq<TEntity, TKey>>(
                new TypedParameter(typeof(ISession), session),
                new TypedParameter(typeof(IEntityIdProvider<TEntity>), idGenerator));
        }

        public static Func<ISession, IRepositoryLinq<TEntity, TKey>> GetRepository<TEntity, TKey>(
            this IComponentContext ctx)
            where TEntity : class, new()
        {
            return GetRepository(ctx, new NoOpEntityIdProvider<TEntity, TKey>());
        }

        public static Func<ISession, IRepositoryLinq<TEntity, long>> GetRepositoryLong<TEntity>(
            this IComponentContext ctx)
            where TEntity : class, new()
        {
            return GetRepository<TEntity, long>(ctx);
        }

        public static Func<ISession, IRepositoryLinq<TEntity, short>> GetRepositoryShort<TEntity>(
            this IComponentContext ctx)
            where TEntity : class, new()
        {
            return GetRepository<TEntity, short>(ctx);
        }

        public static Func<ISession, IRepositoryLinq<TEntity, string>> GetRepositoryString<TEntity>(
            this IComponentContext ctx) where TEntity : class, new()
        {
            return GetRepository<TEntity, string>(ctx);
        }

        public static TServiceType ResolveNamedOrDefault<TServiceType>(this IComponentContext container, string name)
            where TServiceType : class
        {
            if (string.IsNullOrWhiteSpace(name))
                container.Resolve<TServiceType>();

            var type = container.ResolveOptionalNamed<TServiceType>(name);

            if (type == default(TServiceType))
                return container.Resolve<TServiceType>();

            return type;
        }
    }
}