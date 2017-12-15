﻿using System.Data;
using Dapper.Extensions.Linq.Core.Enums;

namespace Dapper.Extensions.Linq.Core.Mapper
{
    /// <summary>
    ///     Maps an entity property to its corresponding column in the database.
    /// </summary>
    public interface IPropertyMap : FluentMap.Mapping.IPropertyMap
    {
        bool IsReadOnly { get; }
        KeyType KeyType { get; }
        DbType Type { get; }
    }
}