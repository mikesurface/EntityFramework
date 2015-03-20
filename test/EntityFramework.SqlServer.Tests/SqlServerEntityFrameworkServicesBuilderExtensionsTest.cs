// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Relational.Migrations.History;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations.Sql;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.SqlServer.Migrations;
using Microsoft.Data.Entity.SqlServer.Query;
using Microsoft.Data.Entity.SqlServer.Update;
using Microsoft.Data.Entity.Tests;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerEntityFrameworkServicesBuilderExtensionsTest : EntityFrameworkServiceCollectionExtensionsTest
    {
        [Fact]
        public override void Services_wire_up_correctly()
        {
            base.Services_wire_up_correctly();

            // Relational
            VerifySingleton<RelationalObjectArrayValueReaderFactory>();
            VerifySingleton<RelationalTypedValueReaderFactory>();
            VerifySingleton<ModificationCommandComparer>();

            // SQL Server dingletones
            VerifySingleton<ISqlServerModelBuilderFactory>();
            VerifySingleton<ISqlServerValueGeneratorCache>();
            VerifySingleton<SqlServerSequenceValueGeneratorFactory>();
            VerifySingleton<ISqlServerSqlGenerator>();
            VerifySingleton<SqlStatementExecutor>();
            VerifySingleton<SqlServerTypeMapper>();
            VerifySingleton<SqlServerModificationCommandBatchFactory>();
            VerifySingleton<SqlServerCommandBatchPreparer>();
            VerifySingleton<ISqlServerModelSource>();

            // SQL Server scoped
            VerifyScoped<ISqlServerQueryContextFactory>();
            VerifyScoped<ISqlServerValueGeneratorSelector>();
            VerifyScoped<SqlServerBatchExecutor>();
            VerifyScoped<ISqlServerDataStoreServices>();
            VerifyScoped<ISqlServerDataStore>();
            VerifyScoped<ISqlServerConnection>();
            VerifyScoped<ISqlServerModelDiffer>();
            VerifyScoped<ISqlServerDatabaseFactory>();
            VerifyScoped<ISqlServerMigrationSqlGenerator>();
            VerifyScoped<ISqlServerDataStoreCreator>();
            VerifyScoped<ISqlServerHistoryRepository>();

            VerifyCommonDataStoreServices();

            // Migrations
            VerifyScoped<MigrationAssembly>();
            VerifyScoped<IHistoryRepository>();
            VerifyScoped<Migrator>();
            VerifySingleton<MigrationIdGenerator>();
            VerifyScoped<IModelDiffer>();
            VerifyScoped<IMigrationSqlGenerator>();
        }

        protected override IServiceCollection GetServices(IServiceCollection services = null)
        {
            return (services ?? new ServiceCollection())
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection();
        }

        protected override DbContextOptions GetOptions()
        {
            return SqlServerTestHelpers.Instance.CreateOptions();
        }

        protected override DbContext CreateContext(IServiceProvider serviceProvider)
        {
            return SqlServerTestHelpers.Instance.CreateContext(serviceProvider);
        }
    }
}
