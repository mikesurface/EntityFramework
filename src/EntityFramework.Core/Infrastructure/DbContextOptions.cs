// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Infrastructure
{
    public abstract class DbContextOptions : IDbContextOptions
    {
        protected DbContextOptions(
            [NotNull] IReadOnlyDictionary<Type, IDbContextOptionsExtension> extensions)
        {
            Check.NotNull(extensions, nameof(extensions));

            _extensions = extensions;
        }

        public virtual IEnumerable<IDbContextOptionsExtension> Extensions => _extensions.Values;

        public virtual TExtension FindExtension<TExtension>()
            where TExtension : class, IDbContextOptionsExtension
        {
            IDbContextOptionsExtension extension;
            return _extensions.TryGetValue(typeof(TExtension), out extension) ? (TExtension)extension : null;
        }

        public abstract DbContextOptions WithExtension<TExtension>([NotNull] TExtension extension)
            where TExtension : class, IDbContextOptionsExtension;

        private readonly IReadOnlyDictionary<Type, IDbContextOptionsExtension> _extensions;
    }
}
