// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational
{
    public abstract class RelationalOptionsExtension : IDbContextOptionsExtension
    {
        private string _connectionString;
        private DbConnection _connection;
        private int? _commandTimeout;
        private int? _maxBatchSize;

        protected RelationalOptionsExtension()
        {
        }

        protected RelationalOptionsExtension([NotNull] RelationalOptionsExtension copyFrom)
        {
            Check.NotNull(copyFrom, nameof(copyFrom));

            _connectionString = copyFrom._connectionString;
            _connection = copyFrom._connection;
            _commandTimeout = copyFrom._commandTimeout;
            _maxBatchSize = copyFrom._maxBatchSize;
        }

        public virtual string ConnectionString
        {
            get { return _connectionString; }

            [param: NotNull]
            set
            {
                Check.NotEmpty(value, nameof(value));

                _connectionString = value;
            }
        }

        public virtual DbConnection Connection
        {
            get { return _connection; }
            [param: NotNull]
            set
            {
                Check.NotNull(value, nameof(value));

                _connection = value;
            }
        }

        public virtual int? CommandTimeout
        {
            get { return _commandTimeout; }
            [param: CanBeNull]
            set
            {
                if (value.HasValue
                    && value <= 0)
                {
                    throw new InvalidOperationException(Strings.InvalidCommandTimeout);
                }

                _commandTimeout = value;
            }
        }

        public virtual int? MaxBatchSize
        {
            get { return _maxBatchSize; }
            [param: CanBeNull]
            set
            {
                if (value.HasValue
                    && value <= 0)
                {
                    throw new InvalidOperationException(Strings.InvalidMaxBatchSize);
                }

                _maxBatchSize = value;
            }
        }

        public virtual string MigrationsAssembly { get; [param: CanBeNull] set; }

        public static RelationalOptionsExtension Extract([NotNull] IDbContextOptions options)
        {
            Check.NotNull(options, nameof(options));

            var storeConfigs = options.Extensions
                .OfType<RelationalOptionsExtension>()
                .ToArray();

            if (storeConfigs.Length == 0)
            {
                throw new InvalidOperationException(Strings.NoDataStoreConfigured);
            }

            if (storeConfigs.Length > 1)
            {
                throw new InvalidOperationException(Strings.MultipleDataStoresConfigured);
            }

            return storeConfigs[0];
        }

        public abstract void ApplyServices(EntityFrameworkServicesBuilder builder);
    }
}
