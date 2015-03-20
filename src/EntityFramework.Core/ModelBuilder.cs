// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Builders;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API surface for configuring a <see cref="Model" /> that defines the shape of your
    ///         entities and how they map to the data store.
    ///     </para>
    ///     <para>
    ///         You can use <see cref="ModelBuilder" /> to construct a model for a context by overriding
    ///         <see cref="DbContext.OnModelCreating(ModelBuilder)" /> or creating a <see cref="Model" />
    ///         externally
    ///         and setting is on a <see cref="DbContextOptions" /> instance that is passed to the context
    ///         constructor.
    ///     </para>
    /// </summary>
    public class ModelBuilder : IModelBuilder<ModelBuilder>
    {
        private readonly InternalModelBuilder _builder;

        // TODO: Configure property facets, foreign keys & navigation properties
        // Issue #213

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModelBuilder" /> class with an empty model.
        ///     The builder will not use any conventions and the entire model must be explicitly configured.
        ///     To specify conventions to be used, use a constructor that accepts a <see cref="ConventionSet" />.
        /// </summary>
        public ModelBuilder()
            : this(new Model())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModelBuilder" /> class that will
        ///     configure an existing model. The builder will not use any conventions and the
        ///     entire model must be explicitly configured. To specify conventions to be used,
        ///     use a constructor that accepts a <see cref="ConventionSet" />.
        /// </summary>
        /// <param name="model"> The model to be configured. </param>
        public ModelBuilder([NotNull] Model model)
            : this(model, new ConventionSet())
        {
            Check.NotNull(model, nameof(model));
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModelBuilder" /> class that will
        ///     configure an existing model and apply a set of conventions.
        /// </summary>
        /// <param name="model"> The model to be configured. </param>
        /// <param name="conventions"> The conventions to be applied to the model. </param>
        public ModelBuilder([NotNull] Model model, [NotNull] ConventionSet conventions)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(conventions, nameof(conventions));

            _builder = new InternalModelBuilder(model, conventions);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModelBuilder" /> class that will
        ///     configure an existing model. The builder will not use any conventions and the
        ///     entire model must be explicitly configured. To specify conventions to be used,
        ///     use a constructor that accepts a <see cref="ConventionSet" />.
        /// </summary>
        /// <param name="internalBuilder"> The internal builder being used to configure the model. </param>
        protected internal ModelBuilder([NotNull] InternalModelBuilder internalBuilder)
        {
            Check.NotNull(internalBuilder, nameof(internalBuilder));

            _builder = internalBuilder;
        }

        /// <summary>
        ///     The model being configured.
        /// </summary>
        public virtual Model Metadata => Builder.Metadata;

        /// <summary>
        ///     The model being configured.
        /// </summary>
        public virtual Model Model => Metadata;

        /// <summary>
        ///     Adds or updates an annotation on the model. If an annotation with the key specified in
        ///     <paramref name="annotation" /> already exists it's value will be updated.
        /// </summary>
        /// <param name="annotation"> The key of the annotation to be added or updated. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual ModelBuilder Annotation(string annotation, string value)
        {
            Check.NotEmpty(annotation, nameof(annotation));
            Check.NotEmpty(value, nameof(value));

            _builder.Annotation(annotation, value, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     The internal builder being used to configure this model.
        /// </summary>
        protected virtual InternalModelBuilder Builder => _builder;

        /// <summary>
        ///     Returns an object that can be used to configure a given entity type in the model.
        ///     If the entity type is not already part of the model, it will be added to the model.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type to be configured. </typeparam>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual EntityBuilder<TEntity> Entity<TEntity>() where TEntity : class
        {
            return new EntityBuilder<TEntity>(Builder.Entity(typeof(TEntity), ConfigurationSource.Explicit));
        }

        /// <summary>
        ///     Returns an object that can be used to configure a given entity type in the model.
        ///     If the entity type is not already part of the model, it will be added to the model.
        /// </summary>
        /// <param name="entityType"> The entity type to be configured. </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual EntityBuilder Entity([NotNull] Type entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return new EntityBuilder(Builder.Entity(entityType, ConfigurationSource.Explicit));
        }

        // TODO Remove this constructor as part of #748
        public virtual EntityBuilder Entity([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            return new EntityBuilder(Builder.Entity(name, ConfigurationSource.Explicit));
        }

        /// <summary>
        ///     <para>
        ///         Performs configuration of a given entity type in the model. If the entity type is not already part
        ///         of the model, it will be added to the model.
        ///     </para>
        ///     <para>
        ///         This overload allows configuration of the entity type to be done in line in the method call rather
        ///         than being chained after a call to <see cref="Entity{TEntity}()" />. This allows additional
        ///         configuration at the model level to be chained after configuration for the entity type.
        ///     </para>
        /// </summary>
        /// <typeparam name="TEntity"> The entity type to be configured. </typeparam>
        /// <param name="entityBuilder"> An action that performs configuration of the entity type. </param>
        /// <returns>
        ///     The same builder instance so that additional configuration calls can be chained.
        /// </returns>
        public virtual ModelBuilder Entity<TEntity>([NotNull] Action<EntityBuilder<TEntity>> entityBuilder) where TEntity : class
        {
            Check.NotNull(entityBuilder, nameof(entityBuilder));

            entityBuilder(Entity<TEntity>());

            return this;
        }

        /// <summary>
        ///     <para>
        ///         Performs configuration of a given entity type in the model. If the entity type is not already part
        ///         of the model, it will be added to the model.
        ///     </para>
        ///     <para>
        ///         This overload allows configuration of the entity type to be done in line in the method call rather
        ///         than being chained after a call to <see cref="Entity{TEntity}()" />. This allows additional
        ///         configuration at the model level to be chained after configuration for the entity type.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type to be configured. </param>
        /// <param name="entityBuilder"> An action that performs configuration of the entity type. </param>
        /// <returns>
        ///     The same builder instance so that additional configuration calls can be chained.
        /// </returns>
        public virtual ModelBuilder Entity([NotNull] Type entityType, [NotNull] Action<EntityBuilder> entityBuilder)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(entityBuilder, nameof(entityBuilder));

            entityBuilder(Entity(entityType));

            return this;
        }

        // TODO Remove this constructor as part of #748
        public virtual ModelBuilder Entity([NotNull] string name, [NotNull] Action<EntityBuilder> entityBuilder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(entityBuilder, nameof(entityBuilder));

            entityBuilder(Entity(name));

            return this;
        }

        /// <summary>
        ///     Excludes the given entity type from the model. This method is typically used to remove types from
        ///     the model that were added by convention.
        /// </summary>
        /// <typeparam name="TEntity"> The  entity type to be removed from the model. </typeparam>
        public virtual void Ignore<TEntity>() where TEntity : class
        {
            Ignore(typeof(TEntity));
        }

        /// <summary>
        ///     Excludes the given entity type from the model. This method is typically used to remove types from
        ///     the model that were added by convention.
        /// </summary>
        /// <param name="entityType"> The entity type to be removed from the model. </param>
        public virtual void Ignore([NotNull] Type entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            Builder.Ignore(entityType, ConfigurationSource.Explicit);
        }

        // TODO Remove this constructor as part of #748
        public virtual void Ignore([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            Builder.Ignore(name, ConfigurationSource.Explicit);
        }
    }
}
