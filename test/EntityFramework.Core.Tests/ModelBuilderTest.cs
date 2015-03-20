// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Builders;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class ModelBuilderTest
    {
        [Fact]
        public void Can_get_entity_builder_for_clr_type()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            var entityBuilder = modelBuilder.Entity<Customer>();

            Assert.NotNull(entityBuilder);
            Assert.Equal(typeof(Customer).FullName, model.GetEntityType(typeof(Customer)).Name);
        }

        [Fact]
        public void Can_get_entity_builder_for_clr_type_non_generic()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            var entityBuilder = modelBuilder.Entity(typeof(Customer));

            Assert.NotNull(entityBuilder);
            Assert.Equal(typeof(Customer).FullName, model.GetEntityType(typeof(Customer)).Name);
        }

        [Fact]
        public void Can_get_entity_builder_for_entity_type_name()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            var entityBuilder = modelBuilder.Entity(typeof(Customer).FullName);

            Assert.NotNull(entityBuilder);
            Assert.NotNull(model.TryGetEntityType(typeof(Customer).FullName));
        }

        [Fact]
        public void Cannot_get_entity_builder_for_ignored_clr_type()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder.Ignore<Customer>();

            Assert.Equal(Strings.EntityIgnoredExplicitly(typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    modelBuilder.Entity(typeof(Customer).FullName)).Message);
        }

        [Fact]
        public void Cannot_get_entity_builder_for_ignored_clr_type_non_generic()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder.Ignore(typeof(Customer));

            Assert.Equal(Strings.EntityIgnoredExplicitly(typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    modelBuilder.Entity<Customer>()).Message);
        }

        [Fact]
        public void Cannot_get_entity_builder_for_ignored_entity_type_name()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder.Ignore(typeof(Customer).FullName);

            Assert.Equal(Strings.EntityIgnoredExplicitly(typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    modelBuilder.Entity(typeof(Customer))).Message);
        }

        [Fact]
        public void Can_set_entity_key_from_clr_property()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder.Entity<Customer>().Key(b => b.Id);

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(1, entity.GetPrimaryKey().Properties.Count);
            Assert.Equal(Customer.IdProperty.Name, entity.GetPrimaryKey().Properties.First().Name);
        }

        [Fact]
        public void Can_set_entity_key_from_CLR_property_non_generic()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder.Entity(typeof(Customer), b => b.Key(Customer.IdProperty.Name));

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(1, entity.GetPrimaryKey().Properties.Count);
            Assert.Equal(Customer.IdProperty.Name, entity.GetPrimaryKey().Properties.First().Name);
        }

        [Fact]
        public void Can_set_entity_key_from_property_name()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder.Entity<Customer>(b => { b.Key(Customer.IdProperty.Name); });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(1, entity.GetPrimaryKey().Properties.Count);
            Assert.Equal(Customer.IdProperty.Name, entity.GetPrimaryKey().Properties.First().Name);
        }

        [Fact]
        public void Can_set_entity_key_from_property_name_when_no_clr_property()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder.Entity<Customer>(b =>
                {
                    b.Property<int>(Customer.IdProperty.Name + 1);
                    b.Key(Customer.IdProperty.Name);
                });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(1, entity.GetPrimaryKey().Properties.Count);
            Assert.Equal(Customer.IdProperty.Name, entity.GetPrimaryKey().Properties.First().Name);
        }

        [Fact]
        public void Can_set_entity_key_from_property_name_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder.Entity(typeof(Customer).FullName, b =>
                {
                    b.Property<int>(Customer.IdProperty.Name);
                    b.Key(Customer.IdProperty.Name);
                });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(1, entity.GetPrimaryKey().Properties.Count);
            Assert.Equal(Customer.IdProperty.Name, entity.GetPrimaryKey().Properties.First().Name);
        }

        [Fact]
        public void Setting_entity_key_from_property_name_when_no_property_throws()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            Assert.Equal(Strings.PropertyNotFound(Customer.IdProperty.Name, typeof(Customer).FullName),
                Assert.Throws<ModelItemNotFoundException>(() =>
                    modelBuilder.Entity(typeof(Customer).FullName, b => b.Key(Customer.IdProperty.Name))).Message);
        }

        [Fact]
        public void Setting_entity_key_from_clr_property_when_property_ignored_throws()
        {
            var modelBuilder = CreateModelBuilder();

            Assert.Equal(Strings.PropertyIgnoredExplicitly(Customer.IdProperty.Name, typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    modelBuilder.Entity<Customer>(b =>
                        {
                            b.Ignore(Customer.IdProperty.Name);
                            b.Key(e => e.Id);
                        })).Message);
        }

        [Fact]
        public void Can_set_composite_entity_key_from_clr_properties()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Key(e => new { e.Id, e.Name });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entity.GetPrimaryKey().Properties.Count);
            Assert.Equal(Customer.IdProperty.Name, entity.GetPrimaryKey().Properties.First().Name);
            Assert.Equal(Customer.NameProperty.Name, entity.GetPrimaryKey().Properties.Last().Name);
        }

        [Fact]
        public void Can_set_composite_entity_key_from_property_names()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Ignore<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            modelBuilder.Entity<Customer>(b => { b.Key(Customer.IdProperty.Name, Customer.NameProperty.Name); });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entity.GetPrimaryKey().Properties.Count);
            Assert.Equal(Customer.IdProperty.Name, entity.GetPrimaryKey().Properties.First().Name);
            Assert.Equal(Customer.NameProperty.Name, entity.GetPrimaryKey().Properties.Last().Name);
        }

        [Fact]
        public void Can_set_composite_entity_key_from_property_names_when_mixed_properties()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Ignore<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            modelBuilder.Entity<Customer>(b =>
                {
                    b.Property<string>(Customer.NameProperty.Name + "Shadow");
                    b.Key(Customer.IdProperty.Name, Customer.NameProperty.Name + "Shadow");
                });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entity.GetPrimaryKey().Properties.Count);
            Assert.Equal(Customer.IdProperty.Name, entity.GetPrimaryKey().Properties.First().Name);
            Assert.Equal(Customer.NameProperty.Name + "Shadow", entity.GetPrimaryKey().Properties.Last().Name);
        }

        [Fact]
        public void Can_set_composite_entity_key_from_property_names_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder.Entity(typeof(Customer).FullName, ps =>
                {
                    ps.Property<int>(Customer.IdProperty.Name);
                    ps.Property<string>(Customer.NameProperty.Name);
                    ps.Key(Customer.IdProperty.Name, Customer.NameProperty.Name);
                });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entity.GetPrimaryKey().Properties.Count);
            Assert.Equal(Customer.IdProperty.Name, entity.GetPrimaryKey().Properties.First().Name);
            Assert.Equal(Customer.NameProperty.Name, entity.GetPrimaryKey().Properties.Last().Name);
        }

        [Fact]
        public void Can_set_entity_key_with_annotations()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder
                .Entity<Customer>(
                    b => b.Key(e => new { e.Id, e.Name })
                        .Annotation("A1", "V1")
                        .Annotation("A2", "V2"));

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entity.GetPrimaryKey().Properties.Count);
            Assert.Equal(new[] { Customer.IdProperty.Name, Customer.NameProperty.Name }, entity.GetPrimaryKey().Properties.Select(p => p.Name));
            Assert.Equal("V1", entity.GetPrimaryKey()["A1"]);
            Assert.Equal("V2", entity.GetPrimaryKey()["A2"]);
        }

        [Fact]
        public void Can_set_entity_key_with_annotations_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder.Entity(typeof(Customer).FullName, b =>
                {
                    b.Property<int>(Customer.IdProperty.Name);
                    b.Property<string>(Customer.NameProperty.Name);
                    b.Key(Customer.IdProperty.Name, Customer.NameProperty.Name)
                        .Annotation("A1", "V1")
                        .Annotation("A2", "V2");
                });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entity.GetPrimaryKey().Properties.Count);
            Assert.Equal(new[] { Customer.IdProperty.Name, Customer.NameProperty.Name }, entity.GetPrimaryKey().Properties.Select(p => p.Name));
            Assert.Equal("V1", entity.GetPrimaryKey()["A1"]);
            Assert.Equal("V2", entity.GetPrimaryKey()["A2"]);
        }

        [Fact]
        public void Can_upgrade_candidate_key_to_primary_key()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();

            var entity = model.GetEntityType(typeof(Customer));
            var key = entity.AddKey(entity.GetOrAddProperty(Customer.NameProperty));

            modelBuilder.Entity<Customer>().Key(b => b.Name);

            Assert.Same(key, entity.Keys.Single());
            Assert.Equal(Customer.NameProperty.Name, entity.GetPrimaryKey().Properties.Single().Name);
        }

        [Fact]
        public void Can_set_entity_annotation()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Annotation("foo", "bar");

            Assert.Equal("bar", model.GetEntityType(typeof(Customer))["foo"]);
        }

        [Fact]
        public void Can_set_entity_annotation_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder
                .Entity(typeof(Customer).FullName)
                .Annotation("foo", "bar");

            Assert.Equal("bar", model.GetEntityType(typeof(Customer))["foo"]);
        }

        [Fact]
        public void Can_set_property_annotation()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Property(c => c.Name).Annotation("foo", "bar");

            Assert.Equal("bar", model.GetEntityType(typeof(Customer)).GetProperty(Customer.NameProperty.Name)["foo"]);
        }

        [Fact]
        public void Can_set_property_annotation_when_no_clr_property()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Property<string>(Customer.NameProperty.Name).Annotation("foo", "bar");

            Assert.Equal("bar", model.GetEntityType(typeof(Customer)).GetProperty(Customer.NameProperty.Name)["foo"]);
        }

        [Fact]
        public void Can_set_property_annotation_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder
                .Entity(typeof(Customer).FullName)
                .Property<string>(Customer.NameProperty.Name).Annotation("foo", "bar");

            Assert.Equal("bar", model.GetEntityType(typeof(Customer)).GetProperty(Customer.NameProperty.Name)["foo"]);
        }

        [Fact]
        public void Can_add_multiple_properties()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder.Entity<Customer>(b =>
                {
                    b.Property(e => e.Id);
                    b.Property(e => e.Name);
                    b.Property(e => e.AlternateKey);
                });

            Assert.Equal(3, model.GetEntityType(typeof(Customer)).PropertyCount);
        }

        [Fact]
        public void Can_add_multiple_properties_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder.Entity(typeof(Customer).FullName, b =>
                {
                    b.Property<int>(Customer.IdProperty.Name);
                    b.Property<string>(Customer.NameProperty.Name).Annotation("foo", "bar");
                });

            Assert.Equal(2, model.GetEntityType(typeof(Customer)).PropertyCount);
        }

        [Fact]
        public void Properties_are_required_by_default_only_if_CLR_type_is_nullable()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
                {
                    b.Property(e => e.Up);
                    b.Property(e => e.Down);
                    b.Property<int>("Charm");
                    b.Property<string>("Strange");
                    b.Property(typeof(int), "Top");
                    b.Property(typeof(string), "Bottom");
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Quarks));

            Assert.False(entityType.GetProperty("Up").IsNullable);
            Assert.True(entityType.GetProperty("Down").IsNullable);
            Assert.False(entityType.GetProperty("Charm").IsNullable);
            Assert.True(entityType.GetProperty("Strange").IsNullable);
            Assert.False(entityType.GetProperty("Top").IsNullable);
            Assert.True(entityType.GetProperty("Bottom").IsNullable);
        }

        [Fact]
        public void Properties_can_be_ignored()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            var entityType = (IEntityType)modelBuilder.Entity<Quarks>().Metadata;

            Assert.Equal(7, entityType.PropertyCount);

            modelBuilder.Entity<Quarks>(b =>
                {
                    b.Ignore(e => e.Up);
                    b.Ignore(e => e.Down);
                    b.Ignore("Charm");
                    b.Ignore("Strange");
                    b.Ignore("Top");
                    b.Ignore("Bottom");
                    b.Ignore("Shadow");
                });

            Assert.Equal(Customer.IdProperty.Name, entityType.Properties.Single().Name);
        }

        [Fact]
        public void Ignoring_a_property_that_is_part_of_explicit_entity_key_throws()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            Assert.Equal(Strings.PropertyAddedExplicitly(Customer.IdProperty.Name, typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    modelBuilder.Entity<Customer>(b =>
                        {
                            b.Key(e => e.Id);
                            b.Ignore(e => e.Id);
                        })).Message);
        }

        [Fact]
        public void Ignoring_shadow_properties_when_they_have_been_added_throws()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            Assert.Equal(Strings.PropertyAddedExplicitly("Shadow", typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    modelBuilder.Entity<Customer>(b =>
                        {
                            b.Property<string>("Shadow");
                            b.Ignore("Shadow");
                        })).Message);
        }

        [Fact]
        public void Adding_shadow_properties_when_they_have_been_ignored_throws()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            Assert.Equal(Strings.PropertyIgnoredExplicitly("Shadow", typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    modelBuilder.Entity<Customer>(b =>
                        {
                            b.Ignore("Shadow");
                            b.Property<string>("Shadow");
                        })).Message);
        }

        [Fact]
        public void Ignoring_a_navigation_property_removes_discovered_entity_types()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>(b =>
                {
                    b.Ignore(c => c.Details);
                    b.Ignore(c => c.Orders);
                });

            Assert.Equal(1, model.EntityTypes.Count);
        }

        [Fact]
        public void Ignoring_a_navigation_property_removes_discovered_relationship()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>(b =>
                {
                    b.Ignore(c => c.Details);
                    b.Ignore(c => c.Orders);
                });
            modelBuilder.Entity<CustomerDetails>(b => { b.Ignore(c => c.Customer); });

            Assert.Equal(2, model.EntityTypes.Count);
            Assert.Equal(0, model.EntityTypes[0].ForeignKeys.Count);
            Assert.Equal(0, model.EntityTypes[1].ForeignKeys.Count);
        }

        [Fact]
        public void Properties_can_be_made_required()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
                {
                    b.Property(e => e.Up).Required();
                    b.Property(e => e.Down).Required();
                    b.Property<int>("Charm").Required();
                    b.Property<string>("Strange").Required();
                    b.Property(typeof(int), "Top").Required();
                    b.Property(typeof(string), "Bottom").Required();
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Quarks));

            Assert.False(entityType.GetProperty("Up").IsNullable);
            Assert.False(entityType.GetProperty("Down").IsNullable);
            Assert.False(entityType.GetProperty("Charm").IsNullable);
            Assert.False(entityType.GetProperty("Strange").IsNullable);
            Assert.False(entityType.GetProperty("Top").IsNullable);
            Assert.False(entityType.GetProperty("Bottom").IsNullable);
        }

        [Fact]
        public void Properties_can_be_made_optional()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
                {
                    b.Property(e => e.Down).Required(false);
                    b.Property<string>("Strange").Required(false);
                    b.Property(typeof(string), "Bottom").Required(false);
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Quarks));

            Assert.True(entityType.GetProperty("Down").IsNullable);
            Assert.True(entityType.GetProperty("Strange").IsNullable);
            Assert.True(entityType.GetProperty("Bottom").IsNullable);
        }

        [Fact]
        public void Non_nullable_properties_cannot_be_made_optional()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
                {
                    Assert.Equal(
                        Strings.CannotBeNullable("Up", "Quarks", "Int32"),
                        Assert.Throws<InvalidOperationException>(() => b.Property(e => e.Up).Required(false)).Message);

                    Assert.Equal(
                        Strings.CannotBeNullable("Charm", "Quarks", "Int32"),
                        Assert.Throws<InvalidOperationException>(() => b.Property<int>("Charm").Required(false)).Message);

                    Assert.Equal(
                        Strings.CannotBeNullable("Top", "Quarks", "Int32"),
                        Assert.Throws<InvalidOperationException>(() => b.Property(typeof(int), "Top").Required(false)).Message);
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Quarks));

            Assert.False(entityType.GetProperty("Up").IsNullable);
            Assert.False(entityType.GetProperty("Charm").IsNullable);
            Assert.False(entityType.GetProperty("Top").IsNullable);
        }

        [Fact]
        public void Properties_specified_by_string_are_shadow_properties_unless_already_known_to_be_CLR_properties()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
                {
                    b.Property<int>("Charm");
                    b.Property(typeof(int), "Top");
                    b.Property<string>("Gluon");
                    b.Property(typeof(string), "Photon");
                });

            var entityType = model.GetEntityType(typeof(Quarks));

            Assert.False(entityType.GetProperty("Up").IsShadowProperty);
            Assert.False(entityType.GetProperty("Charm").IsShadowProperty);
            Assert.False(entityType.GetProperty("Top").IsShadowProperty);
            Assert.True(entityType.GetProperty("Gluon").IsShadowProperty);
            Assert.True(entityType.GetProperty("Photon").IsShadowProperty);

            Assert.Equal(-1, entityType.GetProperty("Up").ShadowIndex);
            Assert.Equal(-1, entityType.GetProperty("Charm").ShadowIndex);
            Assert.Equal(-1, entityType.GetProperty("Top").ShadowIndex);
            Assert.Equal(0, entityType.GetProperty("Gluon").ShadowIndex);
            Assert.Equal(1, entityType.GetProperty("Photon").ShadowIndex);
        }

        [Fact]
        public void Properties_can_be_made_shadow_properties_or_vice_versa()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
                {
                    b.Property(e => e.Up).Shadow();
                    b.Property<int>("Charm").Shadow();
                    b.Property(typeof(int), "Top").Shadow();
                    b.Property<string>("Gluon").Shadow();
                    b.Property(typeof(string), "Photon").Shadow();
                });

            var entityType = model.GetEntityType(typeof(Quarks));

            Assert.True(entityType.GetProperty("Up").IsShadowProperty);
            Assert.True(entityType.GetProperty("Charm").IsShadowProperty);
            Assert.True(entityType.GetProperty("Top").IsShadowProperty);
            Assert.True(entityType.GetProperty("Gluon").IsShadowProperty);
            Assert.True(entityType.GetProperty("Photon").IsShadowProperty);

            Assert.Equal(4, entityType.GetProperty("Up").ShadowIndex);
            Assert.Equal(0, entityType.GetProperty("Charm").ShadowIndex);
            Assert.Equal(3, entityType.GetProperty("Top").ShadowIndex);
            Assert.Equal(1, entityType.GetProperty("Gluon").ShadowIndex);
            Assert.Equal(2, entityType.GetProperty("Photon").ShadowIndex);
        }

        [Fact]
        public void Properties_can_be_made_concurency_tokens()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
                {
                    b.Property(e => e.Up).ConcurrencyToken();
                    b.Property(e => e.Down).ConcurrencyToken(false);
                    b.Property<int>("Charm").ConcurrencyToken(true);
                    b.Property<string>("Strange").ConcurrencyToken(false);
                    b.Property(typeof(int), "Top").ConcurrencyToken();
                    b.Property(typeof(string), "Bottom").ConcurrencyToken(false);
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Quarks));

            Assert.False(entityType.GetProperty(Customer.IdProperty.Name).IsConcurrencyToken);
            Assert.True(entityType.GetProperty("Up").IsConcurrencyToken);
            Assert.False(entityType.GetProperty("Down").IsConcurrencyToken);
            Assert.True(entityType.GetProperty("Charm").IsConcurrencyToken);
            Assert.False(entityType.GetProperty("Strange").IsConcurrencyToken);
            Assert.True(entityType.GetProperty("Top").IsConcurrencyToken);
            Assert.False(entityType.GetProperty("Bottom").IsConcurrencyToken);

            Assert.Equal(-1, entityType.GetProperty(Customer.IdProperty.Name).OriginalValueIndex);
            Assert.Equal(2, entityType.GetProperty("Up").OriginalValueIndex);
            Assert.Equal(-1, entityType.GetProperty("Down").OriginalValueIndex);
            Assert.Equal(0, entityType.GetProperty("Charm").OriginalValueIndex);
            Assert.Equal(-1, entityType.GetProperty("Strange").OriginalValueIndex);
            Assert.Equal(1, entityType.GetProperty("Top").OriginalValueIndex);
            Assert.Equal(-1, entityType.GetProperty("Bottom").OriginalValueIndex);
        }

        [Fact]
        public void Properties_can_be_set_to_generate_values_on_Add()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
                {
                    b.Property(e => e.Id).GenerateValueOnAdd(false);
                    b.Property(e => e.Up).GenerateValueOnAdd();
                    b.Property(e => e.Down).GenerateValueOnAdd(true);
                    b.Property<int>("Charm").GenerateValueOnAdd();
                    b.Property<string>("Strange").GenerateValueOnAdd(false);
                    b.Property(typeof(int), "Top").GenerateValueOnAdd();
                    b.Property(typeof(string), "Bottom").GenerateValueOnAdd(false);
                });

            var entityType = model.GetEntityType(typeof(Quarks));

            Assert.Equal(false, entityType.GetProperty(Customer.IdProperty.Name).GenerateValueOnAdd);
            Assert.Equal(true, entityType.GetProperty("Up").GenerateValueOnAdd);
            Assert.Equal(true, entityType.GetProperty("Down").GenerateValueOnAdd);
            Assert.Equal(true, entityType.GetProperty("Charm").GenerateValueOnAdd);
            Assert.Equal(false, entityType.GetProperty("Strange").GenerateValueOnAdd);
            Assert.Equal(true, entityType.GetProperty("Top").GenerateValueOnAdd);
            Assert.Equal(false, entityType.GetProperty("Bottom").GenerateValueOnAdd);
        }

        [Fact]
        public void Properties_can_be_set_to_be_store_computed()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
                {
                    b.Property(e => e.Up).StoreComputed();
                    b.Property(e => e.Down).StoreComputed(false);
                    b.Property<int>("Charm").StoreComputed();
                    b.Property<string>("Strange").StoreComputed(false);
                    b.Property(typeof(int), "Top").StoreComputed();
                    b.Property(typeof(string), "Bottom").StoreComputed(false);
                });

            var entityType = model.GetEntityType(typeof(Quarks));

            Assert.Null(entityType.GetProperty(Customer.IdProperty.Name).IsStoreComputed);
            Assert.Equal(true, entityType.GetProperty("Up").IsStoreComputed);
            Assert.Equal(false, entityType.GetProperty("Down").IsStoreComputed);
            Assert.Equal(true, entityType.GetProperty("Charm").IsStoreComputed);
            Assert.Equal(false, entityType.GetProperty("Strange").IsStoreComputed);
            Assert.Equal(true, entityType.GetProperty("Top").IsStoreComputed);
            Assert.Equal(false, entityType.GetProperty("Bottom").IsStoreComputed);
        }

        [Fact]
        public void Properties_can_be_set_to_use_store_default_values()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
                {
                    b.Property(e => e.Up).UseStoreDefault();
                    b.Property(e => e.Down).UseStoreDefault(false);
                    b.Property<int>("Charm").UseStoreDefault();
                    b.Property<string>("Strange").UseStoreDefault(false);
                    b.Property(typeof(int), "Top").UseStoreDefault();
                    b.Property(typeof(string), "Bottom").UseStoreDefault(false);
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Quarks));

            Assert.False(entityType.GetProperty(Customer.IdProperty.Name).UseStoreDefault);
            Assert.True(entityType.GetProperty("Up").UseStoreDefault);
            Assert.False(entityType.GetProperty("Down").UseStoreDefault);
            Assert.True(entityType.GetProperty("Charm").UseStoreDefault);
            Assert.False(entityType.GetProperty("Strange").UseStoreDefault);
            Assert.True(entityType.GetProperty("Top").UseStoreDefault);
            Assert.False(entityType.GetProperty("Bottom").UseStoreDefault);
        }

        [Fact]
        public void Can_set_max_length_for_properties()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
                {
                    b.Property(e => e.Up).MaxLength(0);
                    b.Property(e => e.Down).MaxLength(100);
                    b.Property<int>("Charm").MaxLength(0);
                    b.Property<string>("Strange").MaxLength(100);
                    b.Property(typeof(int), "Top").MaxLength(0);
                    b.Property(typeof(string), "Bottom").MaxLength(100);
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Quarks));

            Assert.Equal(0, entityType.GetProperty(Customer.IdProperty.Name).MaxLength);
            Assert.Equal(0, entityType.GetProperty("Up").MaxLength);
            Assert.Equal(100, entityType.GetProperty("Down").MaxLength);
            Assert.Equal(0, entityType.GetProperty("Charm").MaxLength);
            Assert.Equal(100, entityType.GetProperty("Strange").MaxLength);
            Assert.Equal(0, entityType.GetProperty("Top").MaxLength);
            Assert.Equal(100, entityType.GetProperty("Bottom").MaxLength);
        }

        [Fact]
        public void PropertyBuilder_methods_can_be_chained()
        {
            CreateModelBuilder()
                .Entity<Quarks>()
                .Property(e => e.Up)
                .Required()
                .Annotation("A", "V")
                .ConcurrencyToken()
                .Shadow()
                .StoreComputed()
                .GenerateValueOnAdd()
                .UseStoreDefault()
                .MaxLength(100)
                .Required();
        }

        [Fact]
        public void Can_add_index()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Index(ix => ix.Name);

            var entityType = model.GetEntityType(typeof(Customer));

            Assert.Equal(1, entityType.Indexes.Count());
        }

        [Fact]
        public void Can_add_index_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder.Entity(typeof(Customer).FullName, b =>
                {
                    b.Property<string>(Customer.NameProperty.Name);
                    b.Index(Customer.NameProperty.Name);
                });

            var entityType = model.GetEntityType(typeof(Customer));

            Assert.Equal(1, entityType.Indexes.Count());
        }

        [Fact]
        public void Can_add_multiple_indexes()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder.Entity<Customer>(b =>
                {
                    b.Index(ix => ix.Id).IsUnique();
                    b.Index(ix => ix.Name).Annotation("A1", "V1");
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entityType.Indexes.Count());
            Assert.True(entityType.Indexes.First().IsUnique);
            Assert.False(entityType.Indexes.Last().IsUnique);
            Assert.Equal("V1", entityType.Indexes.Last()["A1"]);
        }

        [Fact]
        public void Can_add_multiple_indexes_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            modelBuilder.Entity(typeof(Customer).FullName, b =>
                {
                    b.Property<int>(Customer.IdProperty.Name);
                    b.Property<string>(Customer.NameProperty.Name);
                    b.Index(Customer.IdProperty.Name).IsUnique();
                    b.Index(Customer.NameProperty.Name).Annotation("A1", "V1");
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entityType.Indexes.Count());
            Assert.True(entityType.Indexes.First().IsUnique);
            Assert.False(entityType.Indexes.Last().IsUnique);
            Assert.Equal("V1", entityType.Indexes.Last()["A1"]);
        }

        [Fact]
        public void OneToMany_finds_existing_navs_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder
                .Entity<Order>().HasOne(o => o.Customer).WithMany(c => c.Orders)
                .ForeignKey(c => c.CustomerId);
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var navToPrincipal = dependentType.GetNavigation("Customer");
            var navToDependent = principalType.GetNavigation("Orders");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal(navToPrincipal.Name, dependentType.Navigations.Single().Name);
            Assert.Same(navToDependent, principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_finds_existing_nav_to_principal_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder
                .Entity<Order>().HasOne(c => c.Customer).WithMany()
                .ForeignKey(c => c.CustomerId);
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var navigation = dependentType.GetNavigation("Customer");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer);

            Assert.Equal(1, dependentType.ForeignKeys.Count());
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk.ReferencedKey, principalType.Navigations.Single().ForeignKey.ReferencedKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_finds_existing_nav_to_dependent_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>().Metadata.GetOrAddForeignKey(
                model.GetEntityType(typeof(Order)).GetProperty("CustomerId"),
                model.GetEntityType(typeof(Customer)).GetPrimaryKey());
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var navigation = principalType.GetOrAddNavigation("Orders", fk, pointsToPrincipal: false);

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Same(navigation, principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_uses_existing_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .ForeignKey(c => c.CustomerId);
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_new_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_nav_and_new_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne();

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_from_other_end_nav_and_new_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            // Passing null as the first arg is not super-compelling, but it is consistent
            modelBuilder.Entity<Customer>().HasMany<Order>().WithOne(e => e.Customer);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_relationship_with_no_navigations()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Customer>().HasMany<Order>().WithOne();

            var newFk = dependentType.ForeignKeys.Single(foreignKey => foreignKey != fk);

            Assert.Equal(fk.ReferencedProperties, newFk.ReferencedProperties);
            Assert.Empty(dependentType.Navigations.Where(nav => nav.ForeignKey == newFk));
            Assert.Empty(principalType.Navigations.Where(nav => nav.ForeignKey == newFk));
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { "AnotherCustomerId", fk.Properties.Single().Name, newFk.Properties.Single().Name, dependentKey.Properties.Single().Name },
                dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_uses_specified_FK_even_if_found_by_convention()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .ForeignKey(e => e.CustomerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_uses_existing_FK_not_found_by_convention()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder
                .Entity<Pickle>().HasOne(e => e.BigMak).WithMany()
                .ForeignKey(c => c.BurgerId);
            modelBuilder.Ignore<Bun>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));
            var fk = dependentType.ForeignKeys.Single(foreignKey => foreignKey.Properties.Single().Name == "BurgerId");
            dependentType.RemoveNavigation(fk.GetNavigationToPrincipal());

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
                .ForeignKey(e => e.BurgerId);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_creates_FK_specified()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
                .ForeignKey(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_nav_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<BigMak>().HasMany(e => e.Pickles).WithOne()
                .ForeignKey(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_from_other_end_nav_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<BigMak>().HasMany<Pickle>().WithOne(e => e.BigMak)
                .ForeignKey(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_relationship_with_no_navigations_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");
            var fk = dependentType.ForeignKeys.SingleOrDefault();

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<BigMak>().HasMany<Pickle>().WithOne()
                .ForeignKey(e => e.BurgerId);

            var newFk = dependentType.ForeignKeys.Single(foreignKey => foreignKey != fk);
            Assert.Same(fkProperty, newFk.Properties.Single());

            Assert.Empty(dependentType.Navigations.Where(nav => nav.ForeignKey != fk));
            Assert.Empty(principalType.Navigations.Where(nav => nav.ForeignKey != fk));
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fk.Properties.Single().Name, fkProperty.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_creates_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak);

            var fk = dependentType.ForeignKeys.Single();
            var fkProperty = fk.Properties.Single();

            Assert.Equal("BigMakId", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int?), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fk.Properties.Single().Name, "BurgerId", dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_nav_with_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<BigMak>().HasMany(e => e.Pickles).WithOne();

            var fk = dependentType.ForeignKeys.Single();
            var fkProperty = fk.Properties.Single();

            Assert.Equal("BigMakId", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int?), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fk.Properties.Single().Name, "BurgerId", dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_from_other_end_nav_with_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<BigMak>().HasMany<Pickle>().WithOne(e => e.BigMak);

            var fk = dependentType.ForeignKeys.Single();
            var fkProperty = fk.Properties.Single();

            Assert.Equal("BigMakId", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int?), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fk.Properties.Single().Name, "BurgerId", dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_relationship_with_no_navigations_with_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();
            var existingFk = dependentType.ForeignKeys.Single();

            modelBuilder.Entity<BigMak>().HasMany<Pickle>().WithOne();

            var foreignKey = dependentType.ForeignKeys.Single(fk => fk != existingFk);
            var fkProperty = foreignKey.Properties.Single();

            Assert.Equal("BigMakId1", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int?), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Empty(dependentType.Navigations.Where(nav => nav.ForeignKey != existingFk));
            Assert.Empty(principalType.Navigations.Where(nav => nav.ForeignKey != existingFk));
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { existingFk.Properties.Single().Name, fkProperty.Name, "BurgerId", dependentKey.Properties.Single().Name },
                dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_matches_shadow_FK_property_by_convention()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>().Property<int>("BigMakId");
            modelBuilder.Ignore<Bun>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            var fkProperty = dependentType.GetProperty("BigMakId");

            modelBuilder.Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fkProperty.Name, "BurgerId", dependentKey.Properties.Single().Name },
                dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_overrides_existing_FK_when_uniqueness_does_not_match()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            var fk = dependentType.AddForeignKey(dependentType.GetOrAddProperty("BurgerId", typeof(int)), principalKey);
            fk.IsUnique = true;

            modelBuilder
                .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
                .ForeignKey(e => e.BurgerId);

            Assert.Equal(1, dependentType.ForeignKeys.Count);
            var newFk = (IForeignKey)dependentType.ForeignKeys.Single(k => k != fk);

            Assert.False(newFk.IsUnique);

            Assert.NotSame(fk, newFk);
            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(newFk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(newFk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { newFk.Properties.Single().Name, dependentKey.Properties.Single().Name },
                dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_use_explicitly_specified_PK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty(Customer.IdProperty.Name);

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .ReferencedKey(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_use_non_PK_principal()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .ReferencedKey(e => e.AlternateKey);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Equal("AlternateKey", fk.ReferencedProperties.Single().Name);

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Equal(1, dependentType.Keys.Count);
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());

            expectedPrincipalProperties.Add(fk.ReferencedProperties.Single());
            AssertEqual(expectedPrincipalProperties, principalType.Properties);
            expectedDependentProperties.Add(fk.Properties.Single());
            AssertEqual(expectedDependentProperties, dependentType.Properties);
        }

        [Fact]
        public void OneToMany_can_have_both_convention_properties_specified()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty(Customer.IdProperty.Name);

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .ForeignKey(e => e.CustomerId)
                .ReferencedKey(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_have_both_convention_properties_specified_in_any_order()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty(Customer.IdProperty.Name);

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .ReferencedKey(e => e.Id)
                .ForeignKey(e => e.CustomerId);

            var foreignKey = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, foreignKey.Properties.Single());
            Assert.Same(principalProperty, foreignKey.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(foreignKey, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(foreignKey, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { "AnotherCustomerId", fkProperty.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_have_FK_by_convention_specified_with_explicit_principal_key()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .ForeignKey(e => e.AnotherCustomerId)
                .ReferencedKey(e => e.AlternateKey);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Equal("AnotherCustomerId", fk.Properties.Single().Name);
            Assert.Equal("AlternateKey", fk.ReferencedProperties.Single().Name);

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Equal(1, dependentType.Keys.Count);
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());

            expectedPrincipalProperties.Add(fk.ReferencedProperties.Single());
            AssertEqual(expectedPrincipalProperties, principalType.Properties);
            expectedDependentProperties.Add(fk.Properties.Single());
            AssertEqual(expectedDependentProperties, dependentType.Properties);
        }

        [Fact]
        public void OneToMany_can_have_FK_by_convention_specified_with_explicit_principal_key_in_any_order()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .ReferencedKey(e => e.AlternateKey)
                .ForeignKey(e => e.AnotherCustomerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Equal("AnotherCustomerId", fk.Properties.Single().Name);
            Assert.Equal("AlternateKey", fk.ReferencedProperties.Single().Name);

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Equal(1, dependentType.Keys.Count);
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());

            expectedPrincipalProperties.Add(fk.ReferencedProperties.Single());
            AssertEqual(expectedPrincipalProperties, principalType.Properties);
            expectedDependentProperties.Add(fk.Properties.Single());
            AssertEqual(expectedDependentProperties, dependentType.Properties);
        }

        [Fact]
        public void OneToMany_can_have_principal_key_by_convention_specified_with_explicit_PK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
                .ForeignKey(e => e.BurgerId)
                .ReferencedKey(e => e.AlternateKey);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_have_principal_key_by_convention_specified_with_explicit_PK_in_any_order()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
                .ReferencedKey(e => e.AlternateKey)
                .ForeignKey(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_have_principal_key_by_convention_replaced_with_primary_key()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder
                .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
                .ForeignKey(e => e.BurgerId);
            modelBuilder.Ignore<Bun>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetOrAddProperty("BurgerId", typeof(int));
            var principalProperty = principalType.GetOrAddProperty("AlternateKey", typeof(int));

            var dependentKey = dependentType.Keys.SingleOrDefault();

            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            modelBuilder.Entity<BigMak>().Key(e => e.AlternateKey);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            AssertEqual(expectedPrincipalProperties, principalType.Properties);
            AssertEqual(expectedDependentProperties, dependentType.Properties);
            Assert.Empty(principalType.ForeignKeys);

            var principalKey = principalType.Keys.Single();
            Assert.Same(principalProperty, principalKey.Properties.Single());
            Assert.Same(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.SingleOrDefault());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.TryGetPrimaryKey());
        }

        [Fact]
        public void OneToMany_principal_key_by_convention_is_not_replaced_with_new_incompatible_primary_key()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder
                .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
                .ForeignKey(e => new { e.BurgerId, e.Id });
            modelBuilder.Ignore<Bun>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var nonPrimaryPrincipalKey = principalType.Keys.Single(k => !k.IsPrimaryKey());
            var principalProperty = principalType.GetOrAddProperty("AlternateKey", typeof(int));

            var dependentKey = dependentType.Keys.SingleOrDefault();

            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            modelBuilder.Entity<BigMak>().Key(e => e.AlternateKey);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Equal(2, fk.Properties.Count);
            Assert.Same(nonPrimaryPrincipalKey, fk.ReferencedKey);

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            AssertEqual(expectedPrincipalProperties, principalType.Properties);
            AssertEqual(expectedDependentProperties, dependentType.Properties);
            Assert.Empty(principalType.ForeignKeys);

            var primaryPrincipalKey = principalType.GetPrimaryKey();
            Assert.Same(principalProperty, primaryPrincipalKey.Properties.Single());
            Assert.Equal(2, principalType.Keys.Count);
            Assert.True(principalType.Keys.Contains(nonPrimaryPrincipalKey));

            Assert.Same(dependentKey, dependentType.Keys.SingleOrDefault());
            Assert.Same(dependentKey, dependentType.TryGetPrimaryKey());
        }

        [Fact]
        public void OneToMany_explicit_principal_key_is_not_replaced_with_new_primary_key()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder
                .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
                .ReferencedKey(e => new { e.Id });
            modelBuilder.Ignore<Bun>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var nonPrimaryPrincipalKey = principalType.Keys.Single();
            var principalProperty = principalType.GetOrAddProperty("AlternateKey", typeof(int));

            var dependentKey = dependentType.Keys.SingleOrDefault();

            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            modelBuilder.Entity<BigMak>().Key(e => e.AlternateKey);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(nonPrimaryPrincipalKey, fk.ReferencedKey);

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            AssertEqual(expectedPrincipalProperties, principalType.Properties);
            AssertEqual(expectedDependentProperties, dependentType.Properties);
            Assert.Empty(principalType.ForeignKeys);

            var primaryPrincipalKey = principalType.GetPrimaryKey();
            Assert.Same(principalProperty, primaryPrincipalKey.Properties.Single());
            Assert.Equal(2, principalType.Keys.Count);
            Assert.True(principalType.Keys.Contains(nonPrimaryPrincipalKey));

            Assert.Same(dependentKey, dependentType.Keys.SingleOrDefault());
            Assert.Same(dependentKey, dependentType.TryGetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_finds_existing_navs_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>().Metadata.GetOrAddForeignKey(
                model.GetEntityType(typeof(Order)).GetProperty("CustomerId"),
                model.GetEntityType(typeof(Customer)).GetPrimaryKey());
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var navToPrincipal = dependentType.GetOrAddNavigation("Customer", fk, pointsToPrincipal: true);
            var navToDependent = principalType.GetOrAddNavigation("Orders", fk, pointsToPrincipal: false);

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Same(navToPrincipal, dependentType.Navigations.Single());
            Assert.Same(navToDependent, principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_finds_existing_nav_to_principal_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder
                .Entity<Order>().HasOne(o => o.Customer).WithMany()
                .ForeignKey(c => c.CustomerId);
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var navigation = dependentType.GetNavigation("Customer");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders);

            Assert.Equal(1, dependentType.ForeignKeys.Count());
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk.ReferencedKey, principalType.Navigations.Single().ForeignKey.ReferencedKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_finds_existing_nav_to_dependent_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>().Metadata.GetOrAddForeignKey(
                model.GetEntityType(typeof(Order)).GetProperty("CustomerId"),
                model.GetEntityType(typeof(Customer)).GetPrimaryKey());
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();
            var navigation = fk.GetNavigationToDependent();

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Same(navigation, principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_does_not_use_existing_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>().HasOne<Customer>().WithMany().ForeignKey(e => e.CustomerId);
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders);
            var newFk = dependentType.ForeignKeys.Single(foreignKey => foreignKey != fk);

            Assert.NotSame(fk, newFk);
            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(newFk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(newFk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { "AnotherCustomerId", fk.Properties.Single().Name, newFk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_creates_new_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_nav_and_new_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Order>().HasOne(e => e.Customer).WithMany();

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_from_other_end_nav_and_new_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Order>().HasOne<Customer>().WithMany(e => e.Orders);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_relationship_with_no_navigations()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Order>().HasOne<Customer>().WithMany();

            var newFk = dependentType.ForeignKeys.Single(foreignKey => foreignKey != fk);

            Assert.Empty(dependentType.Navigations.Where(nav => nav.ForeignKey != fk));
            Assert.Empty(principalType.Navigations.Where(nav => nav.ForeignKey != fk));
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { "AnotherCustomerId", fk.Properties.Single().Name, newFk.Properties.Single().Name, dependentKey.Properties.Single().Name },
                dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_uses_specified_FK_even_if_found_by_convention()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .ForeignKey(e => e.CustomerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_creates_both_navs_with_existing_FK_not_found_by_convention()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            var fk = dependentType.AddForeignKey(dependentType.GetOrAddProperty("BurgerId", typeof(int)), principalKey);
            fk.IsUnique = false;

            modelBuilder
                .Entity<Pickle>().HasOne(e => e.BigMak).WithMany(e => e.Pickles)
                .ForeignKey(e => e.BurgerId);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_creates_FK_specified()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Pickle>().HasOne(e => e.BigMak).WithMany(e => e.Pickles)
                .ForeignKey(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fkProperty.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_nav_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Pickle>().HasOne(e => e.BigMak).WithMany(null)
                .ForeignKey(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fkProperty.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_from_other_end_nav_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Pickle>().HasOne<BigMak>().WithMany(e => e.Pickles)
                .ForeignKey(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fkProperty.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_relationship_with_no_navigations_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");
            var fk = dependentType.ForeignKeys.SingleOrDefault();

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Pickle>().HasOne<BigMak>().WithMany()
                .ForeignKey(e => e.BurgerId);

            var newFk = dependentType.ForeignKeys.Single(foreignKey => foreignKey != fk);
            Assert.Same(fkProperty, newFk.Properties.Single());

            Assert.Empty(dependentType.Navigations.Where(nav => nav.ForeignKey != fk));
            Assert.Empty(principalType.Navigations.Where(nav => nav.ForeignKey != fk));
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fk.Properties.Single().Name, fkProperty.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_creates_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Pickle>().HasOne(e => e.BigMak).WithMany(e => e.Pickles);

            var fk = dependentType.ForeignKeys.Single();
            var fkProperty = fk.Properties.Single();

            Assert.Equal("BigMakId", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int?), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fkProperty.Name, "BurgerId", dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_nav_with_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Pickle>().HasOne(e => e.BigMak).WithMany();

            var fk = dependentType.ForeignKeys.Single();
            var fkProperty = fk.Properties.Single();

            Assert.Equal("BigMakId", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int?), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fkProperty.Name, "BurgerId", dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_from_other_end_nav_with_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Pickle>().HasOne<BigMak>().WithMany(e => e.Pickles);

            var fk = dependentType.ForeignKeys.Single();
            var fkProperty = fk.Properties.Single();

            Assert.Equal("BigMakId", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int?), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fkProperty.Name, "BurgerId", dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_relationship_with_no_navigations_with_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            var fk = dependentType.ForeignKeys.SingleOrDefault();

            modelBuilder.Entity<Pickle>().HasOne<BigMak>().WithMany();

            var newFk = dependentType.ForeignKeys.Single(foreignKey => foreignKey != fk);
            var fkProperty = newFk.Properties.Single();

            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int?), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Empty(dependentType.Navigations.Where(nav => nav.ForeignKey != fk));
            Assert.Empty(principalType.Navigations.Where(nav => nav.ForeignKey != fk));
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fk.Properties.Single().Name, fkProperty.Name, "BurgerId", dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_matches_shadow_FK_by_convention()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>().Property<int>("BigMakId");
            modelBuilder.Ignore<Bun>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            var fkProperty = dependentType.GetProperty("BigMakId");

            modelBuilder.Entity<Pickle>().HasOne(e => e.BigMak).WithMany(e => e.Pickles);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fkProperty.Name, "BurgerId", dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_overrides_existing_FK_if_uniqueness_does_not_match()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder
                .Entity<Pickle>().HasOne(e => e.BigMak).WithOne()
                .ForeignKey<Pickle>(c => c.BurgerId);
            modelBuilder.Ignore<Bun>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));
            var fk = dependentType.ForeignKeys.Single(foreignKey => foreignKey.Properties.Single().Name == "BurgerId");
            Assert.True(((IForeignKey)fk).IsUnique);
            dependentType.RemoveNavigation(fk.GetNavigationToPrincipal());

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Pickle>().HasOne(e => e.BigMak).WithMany(e => e.Pickles)
                .ForeignKey(e => e.BurgerId);

            Assert.Equal(1, dependentType.ForeignKeys.Count);
            var newFk = (IForeignKey)dependentType.ForeignKeys.Single();

            Assert.False(newFk.IsUnique);
            Assert.NotSame(fk, newFk);

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(newFk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(newFk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { newFk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_use_explicitly_specified_PK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty(Customer.IdProperty.Name);

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .ReferencedKey(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { "AnotherCustomerId", fkProperty.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_use_non_PK_principal()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var principalProperty = principalType.GetProperty("AlternateKey");

            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .ReferencedKey(e => e.AlternateKey);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);

            Assert.Empty(principalType.ForeignKeys);
            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Equal(1, dependentType.Keys.Count);
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());

            expectedPrincipalProperties.Add(fk.ReferencedProperties.Single());
            AssertEqual(expectedPrincipalProperties, principalType.Properties);
            expectedDependentProperties.Add(fk.Properties.Single());
            AssertEqual(expectedDependentProperties, dependentType.Properties);
        }

        [Fact]
        public void ManyToOne_can_have_both_convention_properties_specified()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty(Customer.IdProperty.Name);

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .ForeignKey(e => e.CustomerId)
                .ReferencedKey(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { "AnotherCustomerId", fkProperty.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_have_both_convention_properties_specified_in_any_order()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty(Customer.IdProperty.Name);

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .ReferencedKey(e => e.Id)
                .ForeignKey(e => e.CustomerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { "AnotherCustomerId", fkProperty.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_have_FK_by_convention_specified_with_explicit_principal_key()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .ForeignKey(e => e.AnotherCustomerId)
                .ReferencedKey(e => e.AlternateKey);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Equal("AnotherCustomerId", fk.Properties.Single().Name);
            Assert.Equal("AlternateKey", fk.ReferencedProperties.Single().Name);

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Equal(1, dependentType.Keys.Count);
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());

            expectedPrincipalProperties.Add(fk.ReferencedProperties.Single());
            AssertEqual(expectedPrincipalProperties, principalType.Properties);
            expectedDependentProperties.Add(fk.Properties.Single());
            AssertEqual(expectedDependentProperties, dependentType.Properties);
        }

        [Fact]
        public void ManyToOne_can_have_FK_by_convention_specified_with_explicit_principal_key_in_any_order()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<OrderDetails>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .ReferencedKey(e => e.AlternateKey)
                .ForeignKey(e => e.AnotherCustomerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Equal("AnotherCustomerId", fk.Properties.Single().Name);
            Assert.Equal("AlternateKey", fk.ReferencedProperties.Single().Name);

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Equal(1, dependentType.Keys.Count);
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());

            expectedPrincipalProperties.Add(fk.ReferencedProperties.Single());
            AssertEqual(expectedPrincipalProperties, principalType.Properties);
            expectedDependentProperties.Add(fk.Properties.Single());
            AssertEqual(expectedDependentProperties, dependentType.Properties);
        }

        [Fact]
        public void ManyToOne_can_have_principal_key_by_convention_specified_with_explicit_PK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Pickle>().HasOne(e => e.BigMak).WithMany(e => e.Pickles)
                .ForeignKey(e => e.BurgerId)
                .ReferencedKey(e => e.AlternateKey);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fkProperty.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_have_principal_key_by_convention_specified_with_explicit_PK_in_any_order()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();
            modelBuilder.Ignore<Bun>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Pickle>().HasOne(e => e.BigMak).WithMany(e => e.Pickles)
                .ReferencedKey(e => e.AlternateKey)
                .ForeignKey(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fkProperty.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_finds_existing_navs_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder
                .Entity<CustomerDetails>().HasOne(d => d.Customer).WithOne(c => c.Details)
                .ForeignKey<CustomerDetails>(c => c.Id);
            modelBuilder.Ignore<Order>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var navToPrincipal = dependentType.GetNavigation("Customer");
            var navToDependent = principalType.GetNavigation("Details");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Customer>().HasOne(e => e.Details).WithOne(e => e.Customer);

            Assert.Equal(1, dependentType.ForeignKeys.Count());
            Assert.Same(navToPrincipal, dependentType.Navigations.Single());
            Assert.Same(navToDependent, principalType.Navigations.Single());
            Assert.Same(fk.ReferencedKey, principalType.Navigations.Single().ForeignKey.ReferencedKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_replaces_existing_nav_to_principal()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<CustomerDetails>().HasOne(c => c.Customer).WithOne();
            modelBuilder.Ignore<Order>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Customer>().HasOne(e => e.Details).WithOne(e => e.Customer);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fk.GetNavigationToPrincipal(), dependentType.Navigations.Single());
            Assert.Same(fk.GetNavigationToDependent(), principalType.Navigations.Single());
            AssertEqual(expectedPrincipalProperties, principalType.Properties);
            AssertEqual(expectedDependentProperties, dependentType.Properties);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_finds_existing_nav_to_dependent_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>().HasOne(c => c.Details).WithOne()
                .ForeignKey<CustomerDetails>(c => c.Id);
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var navigation = principalType.GetNavigation("Details");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Customer>().HasOne(e => e.Details).WithOne(e => e.Customer);

            Assert.Equal(1, dependentType.ForeignKeys.Count);
            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Same(fk.ReferencedKey, principalType.Navigations.Single().ForeignKey.ReferencedKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_shadow_FK_if_existing_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>()
                .HasOne<CustomerDetails>()
                .WithOne()
                .ForeignKey<CustomerDetails>(e => e.Id);
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            modelBuilder.Entity<CustomerDetails>().HasOne(e => e.Customer).WithOne(e => e.Details)
                .ReferencedKey<Customer>(e => e.Id);
            var newFk = dependentType.ForeignKeys.Single(foreignKey => foreignKey != fk);

            Assert.NotSame(fk.Properties.Single(), newFk.Properties.Single());
            Assert.Same(newFk.GetNavigationToPrincipal(), dependentType.Navigations.Single());
            Assert.Same(newFk.GetNavigationToDependent(), principalType.Navigations.Single());
            AssertEqual(expectedPrincipalProperties, principalType.Properties);
            expectedDependentProperties.Add(newFk.Properties.Single());
            AssertEqual(expectedDependentProperties, dependentType.Properties);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_new_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Entity<Customer>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            modelBuilder.Entity<CustomerDetails>().HasOne(e => e.Customer).WithOne(e => e.Details);

            var fk = dependentType.ForeignKeys.Single();

            Assert.Same(fk.GetNavigationToPrincipal(), dependentType.Navigations.Single());
            Assert.Same(fk.GetNavigationToDependent(), principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(expectedPrincipalProperties, principalType.Properties);
            Assert.Equal(expectedDependentProperties, dependentType.Properties);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_removes_existing_FK_when_not_specified()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();
            modelBuilder
                .Entity<Order>()
                .HasOne(e => e.Details)
                .WithOne()
                .ForeignKey<OrderDetails>(c => c.Id);
            modelBuilder.Ignore<Customer>();

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));
            var existingFk = dependentType.ForeignKeys.Single();

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            modelBuilder.Entity<OrderDetails>().HasOne(e => e.Order).WithOne(e => e.Details);

            var fk = dependentType.ForeignKeys.Single();
            Assert.NotSame(existingFk, fk);

            Assert.Same(fk.GetNavigationToPrincipal(), dependentType.Navigations.Single());
            Assert.Same(fk.GetNavigationToDependent(), principalType.Navigations.Single());
            Assert.Equal(expectedPrincipalProperties, principalType.Properties);
            Assert.Equal(expectedDependentProperties, dependentType.Properties);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_creates_new_FK_when_not_specified()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<OrderDetails>();
            modelBuilder.Entity<Order>();
            modelBuilder.Ignore<Customer>();

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var fkProperty = dependentType.GetProperty("OrderId");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<OrderDetails>().HasOne(e => e.Order).WithOne(e => e.Details);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AnotherCustomerId", "CustomerId", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { dependentKey.Properties.Single().Name, fkProperty.Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_create_unidirectional_nav_and_new_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.GetEntityType(typeof(Customer));
            var principalType = model.GetEntityType(typeof(CustomerDetails));
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Customer>().HasOne(e => e.Details).WithOne();

            var fk = dependentType.Navigations.Single().ForeignKey;
            if (fk.EntityType == dependentType)
            {
                Assert.Empty(principalType.ForeignKeys);
            }
            else
            {
                Assert.Empty(dependentType.ForeignKeys);
            }

            Assert.Empty(principalType.Navigations);
            Assert.Equal(expectedPrincipalProperties, principalType.Properties);
            Assert.Equal(expectedDependentProperties, dependentType.Properties);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_create_unidirectional_from_other_end_nav_and_new_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            foreach (var navigation in principalType.Navigations.ToList())
            {
                var inverse = navigation.TryGetInverse();
                inverse?.EntityType.RemoveNavigation(inverse);
                principalType.RemoveNavigation(navigation);

                var foreignKey = navigation.ForeignKey;
                foreignKey.EntityType.RemoveForeignKey(foreignKey);
            }

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            modelBuilder.Entity<CustomerDetails>().HasOne<Customer>().WithOne(e => e.Details);

            var fk = principalType.Navigations.Single().ForeignKey;

            Assert.Empty(dependentType.Navigations);
            AssertEqual(expectedPrincipalProperties, principalType.Properties);
            expectedDependentProperties.Add(fk.Properties.Single());
            AssertEqual(expectedDependentProperties, dependentType.Properties);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_create_relationship_with_no_navigations()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            modelBuilder.Entity<CustomerDetails>().HasOne<Customer>().WithOne();

            var fk = dependentType.ForeignKeys.Single(foreignKey => foreignKey.GetNavigationToDependent() == null);

            Assert.Empty(dependentType.Navigations.Where(nav => nav.ForeignKey == fk));
            Assert.Equal(expectedPrincipalProperties, principalType.Properties);
            expectedDependentProperties.Add(fk.Properties.Single());
            AssertEqual(expectedDependentProperties, dependentType.Properties);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_uses_specified_FK_even_if_found_by_convention()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();
            modelBuilder.Ignore<Customer>();

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var fkProperty = dependentType.GetProperty("OrderId");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Details).WithOne(e => e.Order)
                .ForeignKey<OrderDetails>(e => e.OrderId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AnotherCustomerId", "CustomerId", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { dependentKey.Properties.Single().Name, fkProperty.Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_uses_specified_FK_even_if_PK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty(Customer.IdProperty.Name);

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Customer>().HasOne(e => e.Details).WithOne(e => e.Customer)
                .ForeignKey<CustomerDetails>(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name, "Name" }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_uses_existing_FK_not_found_by_convention()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Bun>().Metadata.AddForeignKey(
                model.GetEntityType(typeof(Bun)).GetProperty("BurgerId"),
                model.GetEntityType(typeof(BigMak)).GetPrimaryKey());
            modelBuilder.Ignore<Pickle>();

            var dependentType = model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));
            var fk = dependentType.ForeignKeys.Single(foreignKey => foreignKey.Properties.All(p => p.Name == "BurgerId"));
            fk.IsUnique = true;

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<BigMak>().HasOne(e => e.Bun).WithOne(e => e.BigMak)
                .ForeignKey<Bun>(e => e.BurgerId);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Bun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_creates_FK_specified()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Bun>();
            modelBuilder.Ignore<Pickle>();

            var dependentType = model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<BigMak>().HasOne(e => e.Bun).WithOne(e => e.BigMak)
                .ForeignKey<Bun>(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Bun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_create_unidirectional_nav_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Bun>();
            modelBuilder.Ignore<Pickle>();

            var dependentType = model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<BigMak>().HasOne(e => e.Bun).WithOne()
                .ForeignKey<Bun>(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Bun", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_create_unidirectional_from_other_end_nav_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Bun>();
            modelBuilder.Ignore<Pickle>();

            var dependentType = model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<BigMak>().HasOne<Bun>().WithOne(e => e.BigMak)
                .ForeignKey<Bun>(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_create_relationship_with_no_navigations_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Bun>();
            modelBuilder.Ignore<Pickle>();

            var dependentType = model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            modelBuilder
                .Entity<BigMak>().HasOne<Bun>().WithOne()
                .ForeignKey<Bun>(e => e.BurgerId);

            var newFk = dependentType.ForeignKeys.Single(foreignKey => foreignKey.Properties.All(p => p.Name == "BurgerId"));

            Assert.Empty(dependentType.Navigations.Where(nav => nav.ForeignKey == newFk));
            Assert.Empty(principalType.Navigations.Where(nav => nav.ForeignKey == newFk));
            Assert.Equal(expectedPrincipalProperties, principalType.Properties);
            Assert.Equal(expectedDependentProperties, dependentType.Properties);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_overrides_existing_FK_when_uniqueness_does_not_match()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Bun>().Metadata.AddForeignKey(
                model.GetEntityType(typeof(Bun)).GetProperty("BurgerId"),
                model.GetEntityType(typeof(BigMak)).GetPrimaryKey());
            modelBuilder.Ignore<Pickle>();

            var dependentType = (IEntityType)model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));
            var fk = dependentType.ForeignKeys.Single(foreignKey => foreignKey.Properties.All(p => p.Name == "BurgerId"));

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<BigMak>().HasOne(e => e.Bun).WithOne(e => e.BigMak)
                .ForeignKey<Bun>(e => e.BurgerId);

            Assert.Equal(1, dependentType.ForeignKeys.Count);
            var newFk = dependentType.ForeignKeys.Single(k => k != fk);

            Assert.False(fk.IsUnique);
            Assert.True(newFk.IsUnique);

            Assert.Equal(fk.Properties.ToList(), newFk.Properties.ToList());
            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Bun", principalType.Navigations.Single().Name);
            Assert.Same(newFk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(newFk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { newFk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_removes_existing_FK_when_specified()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();
            modelBuilder
                .Entity<OrderDetails>().HasOne<Order>().WithOne()
                .ForeignKey<OrderDetails>(c => c.Id);
            modelBuilder.Ignore<Customer>();

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));
            var existingFk = dependentType.ForeignKeys.Single(fk => fk.Properties.All(p => p.Name == "Id"));

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<OrderDetails>().HasOne(e => e.Order).WithOne(e => e.Details)
                .ForeignKey<OrderDetails>(e => e.Id);

            var newFk = dependentType.ForeignKeys.Single();
            Assert.Equal(existingFk.Properties, newFk.Properties);
            Assert.Equal(existingFk.ReferencedProperties, newFk.ReferencedProperties);
            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(newFk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(newFk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AnotherCustomerId", "CustomerId", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { existingFk.Properties.Single().Name, "OrderId" }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_FK_when_specified_on_dependent()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();
            modelBuilder.Ignore<Customer>();

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var fkProperty = dependentType.GetProperty("OrderId");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            modelBuilder
                .Entity<OrderDetails>().HasOne(e => e.Order).WithOne(e => e.Details)
                .ForeignKey<OrderDetails>(e => e.OrderId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Same(fk.GetNavigationToPrincipal(), dependentType.Navigations.Single());
            Assert.Same(fk.GetNavigationToDependent(), principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(expectedPrincipalProperties, principalType.Properties);
            Assert.Equal(expectedDependentProperties, dependentType.Properties);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_FK_when_specified_on_principal()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();
            modelBuilder.Ignore<Customer>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(OrderDetails));

            var fkProperty = dependentType.GetProperty("OrderId");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            modelBuilder
                .Entity<OrderDetails>().HasOne(e => e.Order).WithOne(e => e.Details)
                .ForeignKey<Order>(e => e.OrderId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Same(fk.GetNavigationToPrincipal(), dependentType.Navigations.Single());
            Assert.Same(fk.GetNavigationToDependent(), principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(expectedPrincipalProperties, principalType.Properties);
            Assert.Equal(expectedDependentProperties, dependentType.Properties);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void Unidirectional_OneToOne_creates_FK_when_specified_on_principal()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.GetEntityType(typeof(Customer));
            var principalType = model.GetEntityType(typeof(CustomerDetails));

            var fkProperty = dependentType.GetProperty(Customer.IdProperty.Name);

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();
            var expectedPrincipalProperties = principalType.Properties.Where(p => !p.IsShadowProperty).ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            modelBuilder
                .Entity<CustomerDetails>().HasOne(e => e.Customer).WithOne()
                .ForeignKey<Customer>(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Same(fk.GetNavigationToDependent(), principalType.Navigations.Single());
            Assert.Equal(expectedPrincipalProperties, principalType.Properties);
            Assert.Equal(expectedDependentProperties, dependentType.Properties);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void Unidirectional_OneToOne_creates_FK_when_specified_on_dependent()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty(Customer.IdProperty.Name);

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.Where(p => !p.IsShadowProperty).ToList();

            modelBuilder
                .Entity<CustomerDetails>().HasOne(e => e.Customer).WithOne()
                .ForeignKey<CustomerDetails>(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Same(fk.GetNavigationToPrincipal(), dependentType.Navigations.Single());
            Assert.Empty(principalType.Navigations);
            Assert.Equal(expectedPrincipalProperties, principalType.Properties);
            Assert.Equal(expectedDependentProperties, dependentType.Properties);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void Unidirectional_from_other_end_OneToOne_creates_FK_when_specified_on_principal()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.GetEntityType(typeof(Customer));
            var principalType = model.GetEntityType(typeof(CustomerDetails));

            var fkProperty = dependentType.GetProperty(Customer.IdProperty.Name);

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();
            var expectedPrincipalProperties = principalType.Properties.Where(p => !p.IsShadowProperty).ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            modelBuilder
                .Entity<CustomerDetails>().HasOne<Customer>().WithOne(e => e.Details)
                .ForeignKey<Customer>(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Same(fk.GetNavigationToPrincipal(), dependentType.Navigations.Single());
            Assert.Empty(principalType.Navigations);
            Assert.Equal(expectedPrincipalProperties, principalType.Properties);
            Assert.Equal(expectedDependentProperties, dependentType.Properties);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void Unidirectional_from_other_end_OneToOne_creates_FK_when_specified_on_dependent()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty(Customer.IdProperty.Name);

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.Where(p => !p.IsShadowProperty).ToList();

            modelBuilder
                .Entity<CustomerDetails>().HasOne<Customer>().WithOne(e => e.Details)
                .ForeignKey<CustomerDetails>(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Same(fk.GetNavigationToDependent(), principalType.Navigations.Single());
            Assert.Equal(expectedPrincipalProperties, principalType.Properties);
            Assert.Equal(expectedDependentProperties, dependentType.Properties);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void No_navigation_OneToOne_creates_FK_when_specified_on_dependent()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty(Customer.IdProperty.Name);

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            var principalFk = principalType.ForeignKeys.SingleOrDefault();
            var existingFk = dependentType.ForeignKeys.SingleOrDefault();
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            modelBuilder
                .Entity<CustomerDetails>().HasOne<Customer>().WithOne()
                .ForeignKey<CustomerDetails>(e => e.Id);

            var newForeignKey = dependentType.ForeignKeys.Single(fk => fk != existingFk);
            Assert.Same(fkProperty, newForeignKey.Properties.Single());

            Assert.Empty(dependentType.Navigations.Where(nav => nav.ForeignKey == newForeignKey));
            Assert.Empty(principalType.Navigations.Where(nav => nav.ForeignKey == newForeignKey));
            Assert.Equal(expectedPrincipalProperties, principalType.Properties);
            Assert.Equal(expectedDependentProperties, dependentType.Properties);
            Assert.Same(principalFk, principalType.ForeignKeys.SingleOrDefault());
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void No_navigation_OneToOne_creates_FK_when_specified_on_principal()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.GetEntityType(typeof(Customer));
            var principalType = model.GetEntityType(typeof(CustomerDetails));

            var fkProperty = dependentType.GetProperty(Customer.IdProperty.Name);

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            var principalFk = principalType.ForeignKeys.SingleOrDefault();
            var existingFk = dependentType.ForeignKeys.SingleOrDefault();
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            modelBuilder
                .Entity<CustomerDetails>().HasOne<Customer>().WithOne()
                .ForeignKey<Customer>(e => e.Id);

            var newForeignKey = dependentType.ForeignKeys.Single(fk => fk != existingFk);
            Assert.Same(fkProperty, newForeignKey.Properties.Single());

            Assert.Empty(dependentType.Navigations.Where(nav => nav.ForeignKey == newForeignKey));
            Assert.Empty(principalType.Navigations.Where(nav => nav.ForeignKey == newForeignKey));
            Assert.Equal(expectedPrincipalProperties, principalType.Properties);
            Assert.Equal(expectedDependentProperties, dependentType.Properties);
            Assert.Same(principalFk, principalType.ForeignKeys.SingleOrDefault());
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_use_PK_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty(Customer.IdProperty.Name);

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<CustomerDetails>().HasOne(e => e.Customer).WithOne(e => e.Details)
                .ForeignKey<CustomerDetails>(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name, "Name" }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_have_PK_explicitly_specified()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var principalProperty = principalType.GetProperty(Customer.IdProperty.Name);

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            modelBuilder
                .Entity<Customer>().HasOne(e => e.Details).WithOne(e => e.Customer)
                .ReferencedKey<Customer>(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Same(fk.GetNavigationToPrincipal(), dependentType.Navigations.Single());
            Assert.Same(fk.GetNavigationToDependent(), principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(expectedPrincipalProperties, principalType.Properties);
            Assert.Equal(expectedDependentProperties, dependentType.Properties);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_use_alternate_principal_key()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));
            var principalProperty = principalType.GetProperty("AlternateKey");
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.Where(p => !p.IsShadowProperty).ToList();
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Customer>().HasOne(e => e.Details).WithOne(e => e.Customer)
                .ReferencedKey<Customer>(e => e.AlternateKey);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Same(fk.GetNavigationToPrincipal(), dependentType.Navigations.Single());
            Assert.Same(fk.GetNavigationToDependent(), principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Equal(1, dependentType.Keys.Count);
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());

            expectedPrincipalProperties.Add(fk.ReferencedProperties.Single());
            AssertEqual(expectedPrincipalProperties, principalType.Properties);
            expectedDependentProperties.Add(fk.Properties.Single());
            AssertEqual(expectedDependentProperties, dependentType.Properties);
        }

        [Fact]
        public void OneToOne_can_have_both_keys_specified_explicitly()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();
            modelBuilder.Ignore<Customer>();

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var fkProperty = dependentType.GetProperty("OrderId");
            var principalProperty = principalType.GetProperty("OrderId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Details).WithOne(e => e.Order)
                .ForeignKey<OrderDetails>(e => e.OrderId)
                .ReferencedKey<Order>(e => e.OrderId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_have_both_keys_specified_explicitly_in_any_order()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();
            modelBuilder.Ignore<Customer>();

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var fkProperty = dependentType.GetProperty("OrderId");
            var principalProperty = principalType.GetProperty("OrderId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Details).WithOne(e => e.Order)
                .ReferencedKey<Order>(e => e.OrderId)
                .ForeignKey<OrderDetails>(e => e.OrderId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_have_both_alternate_keys_specified_explicitly()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Bun>();
            modelBuilder.Ignore<Pickle>();

            var dependentType = model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<BigMak>().HasOne(e => e.Bun).WithOne(e => e.BigMak)
                .ForeignKey<Bun>(e => e.BurgerId)
                .ReferencedKey<BigMak>(e => e.AlternateKey);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Bun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { principalProperty.Name, principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fkProperty.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_have_both_alternate_keys_specified_explicitly_in_any_order()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Bun>();
            modelBuilder.Ignore<Pickle>();

            var dependentType = model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<BigMak>().HasOne(e => e.Bun).WithOne(e => e.BigMak)
                .ReferencedKey<BigMak>(e => e.AlternateKey)
                .ForeignKey<Bun>(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Bun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fkProperty.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_does_not_use_existing_FK_when_principal_key_specified()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>()
                .HasOne<Order>().WithOne()
                .ForeignKey<OrderDetails>(e => e.Id);
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));
            var existingFk = dependentType.ForeignKeys.Single(fk => fk.Properties.All(p => p.Name == "Id"));

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            modelBuilder
                .Entity<OrderDetails>().HasOne(e => e.Order).WithOne(e => e.Details)
                .ReferencedKey<Order>(e => e.OrderId);

            var newFk = dependentType.ForeignKeys.Single(fk => fk != existingFk);
            Assert.NotEqual(existingFk.Properties, newFk.Properties);
            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(newFk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(newFk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(expectedPrincipalProperties, principalType.Properties);
            Assert.Equal(expectedDependentProperties, dependentType.Properties);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_principal_key_when_specified_on_dependent()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var keyProperty = principalType.GetProperty("OrderId");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            modelBuilder
                .Entity<OrderDetails>().HasOne(e => e.Order).WithOne(e => e.Details)
                .ReferencedKey<Order>(e => e.OrderId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(keyProperty, fk.ReferencedProperties.Single());

            Assert.Same(fk.GetNavigationToPrincipal(), dependentType.Navigations.Single());
            Assert.Same(fk.GetNavigationToDependent(), principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(expectedPrincipalProperties, principalType.Properties);
            Assert.Equal(expectedDependentProperties, dependentType.Properties);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_principal_key_when_specified_on_principal()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(OrderDetails));

            var keyProperty = principalType.GetProperty("OrderId");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            modelBuilder
                .Entity<OrderDetails>().HasOne(e => e.Order).WithOne(e => e.Details)
                .ReferencedKey<OrderDetails>(e => e.OrderId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(keyProperty, fk.ReferencedProperties.Single());

            Assert.Same(fk.GetNavigationToPrincipal(), dependentType.Navigations.Single());
            Assert.Same(fk.GetNavigationToDependent(), principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(expectedPrincipalProperties, principalType.Properties);
            expectedDependentProperties.Add(fk.Properties.Single());
            AssertEqual(expectedDependentProperties, dependentType.Properties);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(fk.ReferencedKey, principalType.Keys.Single(k => k != principalKey));
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_FK_when_principal_and_foreign_key_specified_on_dependent()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var fkProperty = dependentType.GetProperty("OrderId");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            modelBuilder
                .Entity<OrderDetails>().HasOne(e => e.Order).WithOne(e => e.Details)
                .ForeignKey<OrderDetails>(e => e.OrderId)
                .ReferencedKey<Order>(e => e.OrderId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Same(fk.GetNavigationToPrincipal(), dependentType.Navigations.Single());
            Assert.Same(fk.GetNavigationToDependent(), principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(expectedPrincipalProperties, principalType.Properties);
            Assert.Equal(expectedDependentProperties, dependentType.Properties);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_FK_when_principal_and_foreign_key_specified_on_dependent_in_reverse_order()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var fkProperty = dependentType.GetProperty("OrderId");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            modelBuilder
                .Entity<OrderDetails>().HasOne(e => e.Order).WithOne(e => e.Details)
                .ReferencedKey<Order>(e => e.OrderId)
                .ForeignKey<OrderDetails>(e => e.OrderId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Same(fk.GetNavigationToPrincipal(), dependentType.Navigations.Single());
            Assert.Same(fk.GetNavigationToDependent(), principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(expectedPrincipalProperties, principalType.Properties);
            Assert.Equal(expectedDependentProperties, dependentType.Properties);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_FK_when_principal_and_foreign_key_specified_on_principal()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(OrderDetails));

            var fkProperty = dependentType.GetProperty("OrderId");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            modelBuilder
                .Entity<OrderDetails>().HasOne(e => e.Order).WithOne(e => e.Details)
                .ForeignKey<Order>(e => e.OrderId)
                .ReferencedKey<OrderDetails>(e => e.OrderId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Same(fk.GetNavigationToPrincipal(), dependentType.Navigations.Single());
            Assert.Same(fk.GetNavigationToDependent(), principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(expectedPrincipalProperties, principalType.Properties);
            Assert.Equal(expectedDependentProperties, dependentType.Properties);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(fk.ReferencedKey, principalType.Keys.Single(k => k != principalKey));
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_principal_and_dependent_cannot_be_flipped_twice()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>()
                .HasOne(e => e.Order).WithOne(e => e.Details)
                .ReferencedKey<OrderDetails>(e => e.Id);
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<CustomerDetails>();

            Assert.Equal(Strings.RelationshipCannotBeInverted,
                Assert.Throws<InvalidOperationException>(() => modelBuilder
                    .Entity<OrderDetails>().HasOne(e => e.Order).WithOne(e => e.Details)
                    .ForeignKey<OrderDetails>(e => e.OrderId)
                    .ReferencedKey<OrderDetails>(e => e.OrderId)).Message);
        }

        [Fact]
        public void OneToOne_principal_and_dependent_cannot_be_flipped_twice_in_reverse_order()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>()
                .HasOne(e => e.Order).WithOne(e => e.Details)
                .ReferencedKey<OrderDetails>(e => e.Id);
            modelBuilder.Ignore<Customer>();
            modelBuilder.Ignore<CustomerDetails>();

            Assert.Equal(Strings.RelationshipCannotBeInverted,
                Assert.Throws<InvalidOperationException>(() => modelBuilder
                    .Entity<OrderDetails>().HasOne(e => e.Order).WithOne(e => e.Details)
                    .ReferencedKey<OrderDetails>(e => e.OrderId)
                    .ForeignKey<OrderDetails>(e => e.OrderId)).Message);
        }

        [Fact]
        public void Unidirectional_OneToOne_creates_principal_key_when_specified_on_principal()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            modelBuilder
                .Entity<Customer>().HasOne(e => e.Details).WithOne()
                .ReferencedKey<Customer>(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(principalKey.Properties.Single(), fk.ReferencedProperties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Same(fk.GetNavigationToDependent(), principalType.Navigations.Single());
            Assert.Equal(expectedPrincipalProperties, principalType.Properties);
            Assert.Equal(expectedDependentProperties, dependentType.Properties);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void Unidirectional_OneToOne_creates_principal_key_when_specified_on_dependent()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            modelBuilder
                .Entity<CustomerDetails>().HasOne(e => e.Customer).WithOne()
                .ReferencedKey<Customer>(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(principalKey.Properties.Single(), fk.ReferencedProperties.Single());

            Assert.Same(fk.GetNavigationToPrincipal(), dependentType.Navigations.Single());
            Assert.Empty(principalType.Navigations);
            Assert.Equal(expectedPrincipalProperties, principalType.Properties);
            Assert.Equal(expectedDependentProperties, dependentType.Properties);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void Unidirectional_from_other_end_OneToOne_creates_principal_key_when_specified_on_principal()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            modelBuilder
                .Entity<Customer>().HasOne<CustomerDetails>().WithOne(e => e.Customer)
                .ReferencedKey<Customer>(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(principalKey.Properties.Single(), fk.ReferencedProperties.Single());

            Assert.Same(fk.GetNavigationToPrincipal(), dependentType.Navigations.Single());
            Assert.Empty(principalType.Navigations);
            Assert.Equal(expectedPrincipalProperties, principalType.Properties);
            Assert.Equal(expectedDependentProperties, dependentType.Properties);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void Unidirectional_from_other_end_OneToOne_creates_principal_key_when_specified_on_dependent()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            modelBuilder
                .Entity<CustomerDetails>().HasOne<Customer>().WithOne(e => e.Details)
                .ReferencedKey<Customer>(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(principalKey.Properties.Single(), fk.ReferencedProperties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Same(fk.GetNavigationToDependent(), principalType.Navigations.Single());
            Assert.Equal(expectedPrincipalProperties, principalType.Properties);
            Assert.Equal(expectedDependentProperties, dependentType.Properties);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void No_navigation_OneToOne_creates_principal_key_when_specified_on_principal()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var existingFk = dependentType.ForeignKeys.SingleOrDefault();
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            modelBuilder
                .Entity<Customer>().HasOne<CustomerDetails>().WithOne()
                .ReferencedKey<Customer>(e => e.Id);

            var fk = dependentType.ForeignKeys.Single(foreignKey => foreignKey != existingFk);
            Assert.Same(principalKey.Properties.Single(), fk.ReferencedProperties.Single());

            Assert.Empty(dependentType.Navigations.Where(nav => nav.ForeignKey == fk));
            Assert.Empty(principalType.Navigations.Where(nav => nav.ForeignKey == fk));
            Assert.Equal(expectedPrincipalProperties, principalType.Properties);
            expectedDependentProperties.Add(fk.Properties.Single());
            AssertEqual(expectedDependentProperties, dependentType.Properties);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void No_navigation_OneToOne_creates_principal_key_when_specified_on_dependent()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder.Ignore<Order>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var existingFk = dependentType.ForeignKeys.SingleOrDefault();
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            modelBuilder
                .Entity<CustomerDetails>().HasOne<Customer>().WithOne()
                .ReferencedKey<Customer>(e => e.Id);

            var fk = dependentType.ForeignKeys.Single(foreignKey => foreignKey != existingFk);
            Assert.Same(principalKey.Properties.Single(), fk.ReferencedProperties.Single());

            Assert.Empty(dependentType.Navigations.Where(nav => nav.ForeignKey == fk));
            Assert.Empty(principalType.Navigations.Where(nav => nav.ForeignKey == fk));
            Assert.Equal(expectedPrincipalProperties, principalType.Properties);
            expectedDependentProperties.Add(fk.Properties.Single());
            AssertEqual(expectedDependentProperties, dependentType.Properties);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        private class BigMak
        {
            public int Id { get; set; }
            public int AlternateKey { get; set; }

            public IEnumerable<Pickle> Pickles { get; set; }

            public Bun Bun { get; set; }
        }

        private class Pickle
        {
            public int Id { get; set; }

            public int BurgerId { get; set; }
            public BigMak BigMak { get; set; }
        }

        private class Bun
        {
            public int Id { get; set; }

            public int BurgerId { get; set; }
            public BigMak BigMak { get; set; }
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_uses_existing_composite_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder
                .Entity<Tomato>().HasOne(e => e.Whoopper).WithMany()
                .ForeignKey(c => new { c.BurgerId1, c.BurgerId2 });
            modelBuilder.Ignore<ToastedBun>();
            modelBuilder.Ignore<Moostard>();

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));
            var fk = dependentType.ForeignKeys.Single();

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Whoopper>().HasMany(e => e.Tomatoes).WithOne(e => e.Whoopper)
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            Assert.Equal(1, dependentType.ForeignKeys.Count());
            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk.ReferencedKey, principalType.Navigations.Single().ForeignKey.ReferencedKey);
            Assert.Equal(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fk.Properties[0].Name, fk.Properties[1].Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_creates_composite_FK_specified()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Whoopper>(b => b.Key(c => new { c.Id1, c.Id2 }));
            modelBuilder.Entity<Tomato>();
            modelBuilder.Ignore<ToastedBun>();
            modelBuilder.Ignore<Moostard>();

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalKey = principalType.GetPrimaryKey();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Whoopper>().HasMany(e => e.Tomatoes).WithOne(e => e.Whoopper)
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fk.Properties[0].Name, fk.Properties[1].Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_use_alternate_composite_key()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Whoopper>(b => b.Key(c => new { c.Id1, c.Id2 }));
            modelBuilder.Entity<Tomato>();
            modelBuilder.Ignore<ToastedBun>();
            modelBuilder.Ignore<Moostard>();

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");
            var principalProperty1 = principalType.GetProperty("AlternateKey1");
            var principalProperty2 = principalType.GetProperty("AlternateKey2");

            var principalKey = principalType.GetPrimaryKey();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Whoopper>().HasMany(e => e.Tomatoes).WithOne(e => e.Whoopper)
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 })
                .ReferencedKey(e => new { e.AlternateKey1, e.AlternateKey2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Same(principalProperty1, fk.ReferencedProperties[0]);
            Assert.Same(principalProperty2, fk.ReferencedProperties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fk.Properties[0].Name, fk.Properties[1].Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_use_alternate_composite_key_in_any_order()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Whoopper>(b => b.Key(c => new { c.Id1, c.Id2 }));
            modelBuilder.Entity<Tomato>();
            modelBuilder.Ignore<ToastedBun>();
            modelBuilder.Ignore<Moostard>();

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");
            var principalProperty1 = principalType.GetProperty("AlternateKey1");
            var principalProperty2 = principalType.GetProperty("AlternateKey2");

            var principalKey = principalType.GetPrimaryKey();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Whoopper>().HasMany(e => e.Tomatoes).WithOne(e => e.Whoopper)
                .ReferencedKey(e => new { e.AlternateKey1, e.AlternateKey2 })
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Same(principalProperty1, fk.ReferencedProperties[0]);
            Assert.Same(principalProperty2, fk.ReferencedProperties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fk.Properties[0].Name, fk.Properties[1].Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_nav_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Tomato>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });
            modelBuilder.Ignore<ToastedBun>();
            modelBuilder.Ignore<Moostard>();

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalKey = principalType.GetPrimaryKey();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Whoopper>().HasMany(e => e.Tomatoes).WithOne()
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fk.Properties[0].Name, fk.Properties[1].Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_from_other_end_nav_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Tomato>();
            modelBuilder.Ignore<ToastedBun>();
            modelBuilder.Ignore<Moostard>();

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalKey = principalType.GetPrimaryKey();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Whoopper>().HasMany<Tomato>().WithOne(e => e.Whoopper)
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fk.Properties[0].Name, fk.Properties[1].Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_relationship_with_no_navigations_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Whoopper>().HasMany(w => w.Tomatoes).WithOne(t => t.Whoopper);
            modelBuilder.Entity<Tomato>();
            modelBuilder.Ignore<Moostard>();
            modelBuilder.Ignore<ToastedBun>();

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fk = dependentType.ForeignKeys.SingleOrDefault();
            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalKey = principalType.GetPrimaryKey();
            var dependentKey = dependentType.GetPrimaryKey();

            modelBuilder
                .Entity<Whoopper>().HasMany<Tomato>().WithOne()
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            var newFk = dependentType.ForeignKeys.Single(foreignKey => foreignKey != fk);
            Assert.Same(fkProperty1, newFk.Properties[0]);
            Assert.Same(fkProperty2, newFk.Properties[1]);

            Assert.Empty(dependentType.Navigations.Where(nav => nav.ForeignKey == newFk));
            Assert.Empty(principalType.Navigations.Where(nav => nav.ForeignKey == newFk));
            Assert.Equal(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name },
                principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fkProperty1.Name, fkProperty2.Name, dependentKey.Properties.Single().Name, fk.Properties[0].Name, fk.Properties[1].Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_finds_existing_composite_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder
                .Entity<Tomato>().HasOne(e => e.Whoopper).WithMany()
                .ForeignKey(c => new { c.BurgerId1, c.BurgerId2 });
            modelBuilder.Ignore<ToastedBun>();
            modelBuilder.Ignore<Moostard>();

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));
            var fk = dependentType.ForeignKeys.Single(foreignKey => foreignKey.Properties.First().Name == "BurgerId1");
            dependentType.RemoveNavigation(fk.GetNavigationToPrincipal());

            var principalKey = principalType.GetPrimaryKey();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Tomato>().HasOne(e => e.Whoopper).WithMany(e => e.Tomatoes)
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fk.Properties[0].Name, fk.Properties[1].Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_creates_composite_FK_specified()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Tomato>();
            modelBuilder.Ignore<ToastedBun>();
            modelBuilder.Ignore<Moostard>();

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalKey = principalType.GetPrimaryKey();
            var dependentKey = dependentType.GetPrimaryKey();

            modelBuilder
                .Entity<Tomato>().HasOne(e => e.Whoopper).WithMany(e => e.Tomatoes)
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fk.Properties[0].Name, fk.Properties[1].Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_use_alternate_composite_key()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Whoopper>(b => b.Key(c => new { c.Id1, c.Id2 }));
            modelBuilder.Entity<Tomato>();
            modelBuilder.Ignore<ToastedBun>();
            modelBuilder.Ignore<Moostard>();

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");
            var principalProperty1 = principalType.GetProperty("AlternateKey1");
            var principalProperty2 = principalType.GetProperty("AlternateKey2");

            var principalKey = principalType.GetPrimaryKey();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Tomato>().HasOne(e => e.Whoopper).WithMany(e => e.Tomatoes)
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 })
                .ReferencedKey(e => new { e.AlternateKey1, e.AlternateKey2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Same(principalProperty1, fk.ReferencedProperties[0]);
            Assert.Same(principalProperty2, fk.ReferencedProperties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fk.Properties[0].Name, fk.Properties[1].Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_use_alternate_composite_key_in_any_order()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Whoopper>(b => b.Key(c => new { c.Id1, c.Id2 }));
            modelBuilder.Entity<Tomato>();
            modelBuilder.Ignore<ToastedBun>();
            modelBuilder.Ignore<Moostard>();

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");
            var principalProperty1 = principalType.GetProperty("AlternateKey1");
            var principalProperty2 = principalType.GetProperty("AlternateKey2");

            var principalKey = principalType.GetPrimaryKey();
            var dependentKey = dependentType.GetPrimaryKey();

            modelBuilder
                .Entity<Tomato>().HasOne(e => e.Whoopper).WithMany(e => e.Tomatoes)
                .ReferencedKey(e => new { e.AlternateKey1, e.AlternateKey2 })
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Same(principalProperty1, fk.ReferencedProperties[0]);
            Assert.Same(principalProperty2, fk.ReferencedProperties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fk.Properties[0].Name, fk.Properties[1].Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_nav_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Tomato>();
            modelBuilder.Ignore<ToastedBun>();
            modelBuilder.Ignore<Moostard>();

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalKey = principalType.GetPrimaryKey();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Tomato>().HasOne(e => e.Whoopper).WithMany()
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fk.Properties[0].Name, fk.Properties[1].Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_from_other_end_nav_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Tomato>();
            modelBuilder.Ignore<ToastedBun>();
            modelBuilder.Ignore<Moostard>();

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalKey = principalType.GetPrimaryKey();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Tomato>().HasOne<Whoopper>().WithMany(e => e.Tomatoes)
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fk.Properties[0].Name, fk.Properties[1].Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_relationship_with_no_navigations_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Whoopper>().HasMany(w => w.Tomatoes).WithOne(t => t.Whoopper);
            modelBuilder.Entity<Tomato>();
            modelBuilder.Ignore<ToastedBun>();
            modelBuilder.Ignore<Moostard>();

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetOrAddProperty("BurgerId1", typeof(int));
            var fkProperty2 = dependentType.GetOrAddProperty("BurgerId2", typeof(int));
            var navigationForeignKey = dependentType.ForeignKeys.SingleOrDefault();

            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            var principalKey = principalType.GetPrimaryKey();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Tomato>().HasOne<Whoopper>().WithMany()
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            var newFk = dependentType.ForeignKeys.Single(foreignKey => foreignKey != navigationForeignKey);
            Assert.Same(fkProperty1, newFk.Properties[0]);
            Assert.Same(fkProperty2, newFk.Properties[1]);

            Assert.Empty(dependentType.Navigations.Where(nav => nav.ForeignKey != navigationForeignKey));
            Assert.Empty(principalType.Navigations.Where(nav => nav.ForeignKey != navigationForeignKey));
            AssertEqual(expectedPrincipalProperties, principalType.Properties);
            AssertEqual(expectedDependentProperties, dependentType.Properties);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Equal(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_uses_existing_composite_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            var dependentType = model.GetEntityType(typeof(ToastedBun));
            modelBuilder.Entity<ToastedBun>().Metadata.AddForeignKey(
                new[] { dependentType.GetProperty("BurgerId1"), dependentType.GetProperty("BurgerId2") },
                model.GetEntityType(typeof(Whoopper)).GetPrimaryKey());
            modelBuilder.Ignore<Tomato>();
            modelBuilder.Ignore<Moostard>();

            var principalType = model.GetEntityType(typeof(Whoopper));
            var fk = dependentType.ForeignKeys.Single();
            fk.IsUnique = true;

            var principalKey = principalType.GetPrimaryKey();
            var dependentKey = dependentType.GetPrimaryKey();

            modelBuilder
                .Entity<Whoopper>().HasOne(e => e.ToastedBun).WithOne(e => e.Whoopper)
                .ForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 });

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("ToastedBun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fk.Properties[0].Name, fk.Properties[1].Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_creates_composite_FK_specified()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<ToastedBun>();
            modelBuilder.Ignore<Tomato>();
            modelBuilder.Ignore<Moostard>();

            var dependentType = model.GetEntityType(typeof(ToastedBun));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalKey = principalType.GetPrimaryKey();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Whoopper>().HasOne(e => e.ToastedBun).WithOne(e => e.Whoopper)
                .ForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("ToastedBun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fkProperty1.Name, fkProperty2.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_use_alternate_composite_key()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Whoopper>(b => b.Key(c => new { c.Id1, c.Id2 }));
            modelBuilder.Entity<ToastedBun>();
            modelBuilder.Ignore<Tomato>();
            modelBuilder.Ignore<Moostard>();

            var dependentType = model.GetEntityType(typeof(ToastedBun));
            var principalType = model.GetEntityType(typeof(Whoopper));
            var principalProperty1 = principalType.GetProperty("AlternateKey1");
            var principalProperty2 = principalType.GetProperty("AlternateKey2");

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Whoopper>().HasOne(e => e.ToastedBun).WithOne(e => e.Whoopper)
                .ForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 })
                .ReferencedKey<Whoopper>(e => new { e.AlternateKey1, e.AlternateKey2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Same(principalProperty1, fk.ReferencedProperties[0]);
            Assert.Same(principalProperty2, fk.ReferencedProperties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("ToastedBun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fkProperty1.Name, fkProperty2.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_use_alternate_composite_key_in_any_order()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Whoopper>(b => b.Key(c => new { c.Id1, c.Id2 }));
            modelBuilder.Entity<ToastedBun>();
            modelBuilder.Ignore<Tomato>();
            modelBuilder.Ignore<Moostard>();

            var dependentType = model.GetEntityType(typeof(ToastedBun));
            var principalType = model.GetEntityType(typeof(Whoopper));
            var principalProperty1 = principalType.GetProperty("AlternateKey1");
            var principalProperty2 = principalType.GetProperty("AlternateKey2");

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Whoopper>().HasOne(e => e.ToastedBun).WithOne(e => e.Whoopper)
                .ReferencedKey<Whoopper>(e => new { e.AlternateKey1, e.AlternateKey2 })
                .ForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Same(principalProperty1, fk.ReferencedProperties[0]);
            Assert.Same(principalProperty2, fk.ReferencedProperties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("ToastedBun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fkProperty1.Name, fkProperty2.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_uses_composite_PK_for_FK_by_convention()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Moostard>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Ignore<Tomato>();
            modelBuilder.Ignore<ToastedBun>();

            var dependentType = model.GetEntityType(typeof(Moostard));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("Id1");
            var fkProperty2 = dependentType.GetProperty("Id2");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.GetPrimaryKey();

            modelBuilder
                .Entity<Moostard>().HasOne(e => e.Whoopper).WithOne(e => e.Moostard)
                .ForeignKey<Moostard>(e => new { e.Id1, e.Id2 });

            var fk = dependentType.ForeignKeys.Single();

            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Moostard", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { dependentKey.Properties[0].Name, dependentKey.Properties[1].Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_principal_and_dependent_can_be_flipped_and_composite_PK_is_still_used_by_convention()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Moostard>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Ignore<Tomato>();
            modelBuilder.Ignore<ToastedBun>();

            var dependentType = model.GetEntityType(typeof(Moostard));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("Id1");
            var fkProperty2 = dependentType.GetProperty("Id2");

            var principalKey = principalType.GetPrimaryKey();
            var dependentKey = dependentType.GetPrimaryKey();

            modelBuilder
                .Entity<Moostard>().HasOne(e => e.Whoopper).WithOne(e => e.Moostard)
                .ForeignKey<Moostard>(e => new { e.Id1, e.Id2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Moostard", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { dependentKey.Properties[0].Name, dependentKey.Properties[1].Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_principal_and_dependent_can_be_flipped_using_principal_and_composite_PK_is_still_used_by_convention()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Moostard>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Ignore<Tomato>();
            modelBuilder.Ignore<ToastedBun>();

            var dependentType = model.GetEntityType(typeof(Moostard));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("Id1");
            var fkProperty2 = dependentType.GetProperty("Id2");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.GetPrimaryKey();

            modelBuilder
                .Entity<Moostard>().HasOne(e => e.Whoopper).WithOne(e => e.Moostard)
                .ReferencedKey<Whoopper>(e => new { e.Id1, e.Id2 })
                .Required();

            var fk = dependentType.ForeignKeys.Single();

            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Moostard", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { dependentKey.Properties[0].Name, dependentKey.Properties[1].Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_create_unidirectional_nav_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<ToastedBun>();
            modelBuilder.Ignore<Tomato>();
            modelBuilder.Ignore<Moostard>();

            var dependentType = model.GetEntityType(typeof(ToastedBun));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Whoopper>().HasOne(e => e.ToastedBun).WithOne()
                .ForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("ToastedBun", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fkProperty1.Name, fkProperty2.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_create_unidirectional_from_other_end_nav_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<ToastedBun>();
            modelBuilder.Ignore<Tomato>();
            modelBuilder.Ignore<Moostard>();

            var dependentType = model.GetEntityType(typeof(ToastedBun));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Whoopper>().HasOne<ToastedBun>().WithOne(e => e.Whoopper)
                .ForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fkProperty1.Name, fkProperty2.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_create_relationship_with_no_navigations_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<ToastedBun>();
            modelBuilder.Ignore<Tomato>();
            modelBuilder.Ignore<Moostard>();

            var dependentType = model.GetEntityType(typeof(ToastedBun));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Whoopper>().HasOne<ToastedBun>().WithOne()
                .ForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Empty(dependentType.Navigations.Where(nav => nav.ForeignKey == fk));
            Assert.Empty(principalType.Navigations.Where(nav => nav.ForeignKey == fk));
            Assert.Equal(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name, principalType.ForeignKeys.Single().Properties.Single().Name },
                principalType.Properties.Select(p => p.Name));
            Assert.Equal(new[] { fkProperty1.Name, fkProperty2.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_throws_if_specified_FK_types_do_not_match()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>().Property<Guid>("GuidProperty");
            modelBuilder.Ignore<Order>();

            Assert.Equal(Strings.ForeignKeyTypeMismatch("{'GuidProperty'}", typeof(CustomerDetails).FullName, typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    modelBuilder
                        .Entity<Customer>().HasOne(c => c.Details).WithOne(d => d.Customer)
                        .ReferencedKey(typeof(Customer), "Id")
                        .ForeignKey(typeof(CustomerDetails), "GuidProperty")).Message);
        }

        [Fact]
        public void OneToOne_throws_if_specified_PK_types_do_not_match()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>().Property<Guid>("GuidProperty");
            modelBuilder.Ignore<Order>();

            Assert.Equal(Strings.ForeignKeyTypeMismatch("{'GuidProperty'}", typeof(CustomerDetails).FullName, typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    modelBuilder
                        .Entity<Customer>().HasOne(c => c.Details).WithOne(d => d.Customer)
                        .ForeignKey(typeof(CustomerDetails), "GuidProperty")
                        .ReferencedKey(typeof(Customer), "Id")).Message);
        }

        [Fact]
        public void OneToOne_throws_if_specified_FK_count_does_not_match()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>().Property<Guid>("GuidProperty");
            modelBuilder.Ignore<Order>();

            Assert.Equal(Strings.ForeignKeyCountMismatch("{'Id', 'GuidProperty'}", typeof(CustomerDetails).FullName, "{'Id'}", typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    modelBuilder
                        .Entity<Customer>().HasOne(c => c.Details).WithOne(d => d.Customer)
                        .ReferencedKey(typeof(Customer), "Id")
                        .ForeignKey(typeof(CustomerDetails), "Id", "GuidProperty")).Message);
        }

        [Fact]
        public void OneToOne_throws_if_specified_PK_count_does_not_match()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>().Property<Guid>("GuidProperty");
            modelBuilder.Ignore<Order>();

            Assert.Equal(Strings.ForeignKeyCountMismatch("{'Id', 'GuidProperty'}", typeof(CustomerDetails).FullName, "{'Id'}", typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    modelBuilder
                        .Entity<Customer>().HasOne(c => c.Details).WithOne(d => d.Customer)
                        .ForeignKey(typeof(CustomerDetails), "Id", "GuidProperty")
                        .ReferencedKey(typeof(Customer), "Id")).Message);
        }

        [Fact]
        public void Can_convert_to_non_convention_builder()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            Assert.Same(model, new BasicModelBuilder(modelBuilder.Model).Model);
        }

        private class Whoopper
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }
            public int AlternateKey1 { get; set; }
            public int AlternateKey2 { get; set; }

            public IEnumerable<Tomato> Tomatoes { get; set; }

            public ToastedBun ToastedBun { get; set; }

            public Moostard Moostard { get; set; }
        }

        private class Tomato
        {
            public int Id { get; set; }

            public int BurgerId1 { get; set; }
            public int BurgerId2 { get; set; }
            public Whoopper Whoopper { get; set; }
        }

        private class ToastedBun
        {
            public int Id { get; set; }

            public int BurgerId1 { get; set; }
            public int BurgerId2 { get; set; }
            public Whoopper Whoopper { get; set; }
        }

        private class Moostard
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }

            public Whoopper Whoopper { get; set; }
        }

        private class Customer
        {
            public static readonly PropertyInfo IdProperty = typeof(Customer).GetProperty("Id");
            public static readonly PropertyInfo NameProperty = typeof(Customer).GetProperty("Name");

            public int Id { get; set; }
            public Guid AlternateKey { get; set; }
            public string Name { get; set; }

            public IEnumerable<Order> Orders { get; set; }

            public CustomerDetails Details { get; set; }
        }

        private class CustomerDetails
        {
            public int Id { get; set; }

            public Customer Customer { get; set; }
        }

        private class Order
        {
            public int OrderId { get; set; }

            public int? CustomerId { get; set; }
            public Guid AnotherCustomerId { get; set; }
            public Customer Customer { get; set; }

            public OrderDetails Details { get; set; }
        }

        private class OrderDetails
        {
            public int Id { get; set; }

            public int OrderId { get; set; }
            public Order Order { get; set; }
        }

        // INotify interfaces not really implemented; just marking the classes to test metadata construction
        private class Quarks : INotifyPropertyChanging, INotifyPropertyChanged
        {
            public int Id { get; set; }

            public int Up { get; set; }
            public string Down { get; set; }
            private int Charm { get; set; }
            private string Strange { get; set; }
            private int Top { get; set; }
            private string Bottom { get; set; }

#pragma warning disable 67
            public event PropertyChangingEventHandler PropertyChanging;
            public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67
        }

        [Fact]
        public void One_to_many_relationships_with_nullable_keys_are_optional_by_default()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity<Hob>().HasMany(e => e.Nobs).WithOne(e => e.Hob)
                .ForeignKey(e => new { e.HobId1, e.HobId2 });

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));

            Assert.True(entityType.GetProperty("HobId1").IsNullable);
            Assert.True(entityType.GetProperty("HobId1").IsNullable);
            Assert.False(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void One_to_many_relationships_with_non_nullable_keys_are_required_by_default()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity<Nob>().HasMany(e => e.Hobs).WithOne(e => e.Nob)
                .ForeignKey(e => new { e.NobId1, e.NobId2 });

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));

            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.True(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void Many_to_one_relationships_with_nullable_keys_are_optional_by_default()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity<Nob>().HasOne(e => e.Hob).WithMany(e => e.Nobs)
                .ForeignKey(e => new { e.HobId1, e.HobId2 });

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));

            Assert.True(entityType.GetProperty("HobId1").IsNullable);
            Assert.True(entityType.GetProperty("HobId1").IsNullable);
            Assert.False(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void Many_to_one_relationships_with_non_nullable_keys_are_required_by_default()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity<Hob>().HasOne(e => e.Nob).WithMany(e => e.Hobs)
                .ForeignKey(e => new { e.NobId1, e.NobId2 });

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));

            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.True(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void One_to_one_relationships_with_nullable_keys_are_optional_by_default()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity<Hob>().HasOne(e => e.Nob).WithOne(e => e.Hob)
                .ForeignKey<Nob>(e => new { e.HobId1, e.HobId2 });

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));

            Assert.True(entityType.GetProperty("HobId1").IsNullable);
            Assert.True(entityType.GetProperty("HobId1").IsNullable);
            Assert.False(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void One_to_one_relationships_with_non_nullable_keys_are_required_by_default()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity<Nob>().HasOne(e => e.Hob).WithOne(e => e.Nob)
                .ForeignKey<Hob>(e => new { e.NobId1, e.NobId2 });

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));

            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.True(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void One_to_many_relationships_with_nullable_keys_can_be_made_required()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity<Hob>().HasMany(e => e.Nobs).WithOne(e => e.Hob)
                .ForeignKey(e => new { e.HobId1, e.HobId2 })
                .Required();

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));

            Assert.False(entityType.GetProperty("HobId1").IsNullable);
            Assert.False(entityType.GetProperty("HobId1").IsNullable);
            Assert.True(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void One_to_many_relationships_with_non_nullable_keys_cannot_be_made_optional()
        {
            var modelBuilder = HobNobBuilder();

            Assert.Equal(
                Strings.CannotBeNullable("NobId1", "Hob", "Int32"),
                Assert.Throws<InvalidOperationException>(() => modelBuilder
                    .Entity<Nob>().HasMany(e => e.Hobs).WithOne(e => e.Nob)
                    .ForeignKey(e => new { e.NobId1, e.NobId2 })
                    .Required(false)).Message);

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));

            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.True(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void Many_to_one_relationships_with_nullable_keys_can_be_made_required()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity<Nob>().HasOne(e => e.Hob).WithMany(e => e.Nobs)
                .ForeignKey(e => new { e.HobId1, e.HobId2 })
                .Required();

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));

            Assert.False(entityType.GetProperty("HobId1").IsNullable);
            Assert.False(entityType.GetProperty("HobId1").IsNullable);
            Assert.True(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void Many_to_one_relationships_with_non_nullable_keys_cannot_be_made_optional()
        {
            var modelBuilder = HobNobBuilder();

            Assert.Equal(
                Strings.CannotBeNullable("NobId1", "Hob", "Int32"),
                Assert.Throws<InvalidOperationException>(() => modelBuilder
                    .Entity<Hob>().HasOne(e => e.Nob).WithMany(e => e.Hobs)
                    .ForeignKey(e => new { e.NobId1, e.NobId2 })
                    .Required(false)).Message);

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));

            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.True(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void One_to_one_relationships_with_nullable_keys_can_be_made_required()
        {
            var modelBuilder = HobNobBuilder();
            var principalType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));
            var dependentType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            modelBuilder
                .Entity<Hob>().HasOne(e => e.Nob).WithOne(e => e.Hob)
                .Required()
                .ForeignKey<Nob>(e => new { e.HobId1, e.HobId2 });

            Assert.False(dependentType.GetProperty("HobId1").IsNullable);
            Assert.False(dependentType.GetProperty("HobId2").IsNullable);
            Assert.True(dependentType.ForeignKeys.Single().IsRequired);

            AssertEqual(expectedPrincipalProperties, principalType.Properties);
            AssertEqual(expectedDependentProperties, dependentType.Properties);
        }

        [Fact]
        public void One_to_one_relationships_with_non_nullable_keys_cannot_be_made_optional()
        {
            var modelBuilder = HobNobBuilder();

            Assert.Equal(
                Strings.CannotBeNullable("NobId1", "Hob", "Int32"),
                Assert.Throws<InvalidOperationException>(() => modelBuilder
                    .Entity<Nob>().HasOne(e => e.Hob).WithOne(e => e.Nob)
                    .ForeignKey<Hob>(e => new { e.NobId1, e.NobId2 })
                    .Required(false)).Message);

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));

            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.True(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void One_to_one_relationships_that_are_optional_cannot_be_assigned_non_nullable_keys()
        {
            var modelBuilder = HobNobBuilder();

            Assert.Equal(
                Strings.CannotBeNullable("NobId1", "Hob", "Int32"),
                Assert.Throws<InvalidOperationException>(() => modelBuilder
                    .Entity<Nob>().HasOne(e => e.Hob).WithOne(e => e.Nob)
                    .Required(false)
                    .ForeignKey<Hob>(e => new { e.NobId1, e.NobId2 })).Message);

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));

            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.True(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void One_to_one_relationships_with_unspecified_keys_can_be_made_optional()
        {
            var modelBuilder = HobNobBuilder();
            var principalType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));
            var dependentType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            modelBuilder
                .Entity<Hob>().HasOne(e => e.Nob).WithOne(e => e.Hob)
                .Required(false)
                .ReferencedKey<Nob>(e => new { e.Id1, e.Id2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.False(fk.IsRequired);

            AssertEqual(expectedPrincipalProperties, principalType.Properties);
            expectedDependentProperties.AddRange(fk.Properties);
            AssertEqual(expectedDependentProperties, dependentType.Properties);
        }

        [Fact]
        public void One_to_one_relationships_with_unspecified_keys_can_be_made_required()
        {
            var modelBuilder = HobNobBuilder();
            var principalType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));
            var dependentType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));
            var expectedPrincipalProperties = principalType.Properties.ToList();
            var expectedDependentProperties = dependentType.Properties.ToList();

            modelBuilder
                .Entity<Hob>().HasOne(e => e.Nob).WithOne(e => e.Hob)
                .Required()
                .ReferencedKey<Nob>(e => new { e.Id1, e.Id2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.True(fk.IsRequired);

            AssertEqual(expectedPrincipalProperties, principalType.Properties);
            expectedDependentProperties.AddRange(fk.Properties);
            AssertEqual(expectedDependentProperties, dependentType.Properties);
        }

        [Fact]
        public void One_to_one_relationships_can_be_defined_before_the_PK_from_principal()
        {
            var modelBuilder = CreateModelBuilder(new Model());

            modelBuilder
                .Entity<Hob>(eb =>
                    {
                        eb.HasOne(e => e.Nob).WithOne(e => e.Hob)
                            .ForeignKey<Nob>(e => new { e.HobId1, e.HobId2 })
                            .ReferencedKey<Hob>(e => new { e.Id1, e.Id2 });
                        eb.Key(e => new { e.Id1, e.Id2 });
                    });

            modelBuilder.Entity<Nob>().Key(e => new { e.Id1, e.Id2 });

            var dependentEntityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));
            var fk = dependentEntityType.ForeignKeys.Single();
            AssertEqual(fk.ReferencedProperties, dependentEntityType.Keys.Single().Properties);
            Assert.False(fk.IsRequired);

            var principalEntityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));
            AssertEqual(fk.ReferencedProperties, principalEntityType.Keys.Single().Properties);
        }

        [Fact]
        public void One_to_one_relationships_can_be_defined_before_the_PK_from_dependent()
        {
            var modelBuilder = CreateModelBuilder(new Model());

            modelBuilder
                .Entity<Hob>(eb =>
                    {
                        eb.HasOne(e => e.Nob).WithOne(e => e.Hob)
                            .ForeignKey<Hob>(e => new { e.NobId1, e.NobId2 })
                            .ReferencedKey<Nob>(e => new { e.Id1, e.Id2 });
                        eb.Key(e => new { e.Id1, e.Id2 });
                    });

            modelBuilder.Entity<Nob>().Key(e => new { e.Id1, e.Id2 });

            var dependentEntityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));
            var fk = dependentEntityType.ForeignKeys.Single();
            AssertEqual(fk.ReferencedProperties, dependentEntityType.Keys.Single().Properties);
            Assert.True(fk.IsRequired);

            var principalEntityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));
            AssertEqual(fk.ReferencedProperties, principalEntityType.Keys.Single().Properties);
        }

        private class Hob
        {
            public string Id1 { get; set; }
            public string Id2 { get; set; }

            public int NobId1 { get; set; }
            public int NobId2 { get; set; }

            public Nob Nob { get; set; }
            public ICollection<Nob> Nobs { get; set; }
        }

        private class Nob
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }

            public string HobId1 { get; set; }
            public string HobId2 { get; set; }

            public Hob Hob { get; set; }
            public ICollection<Hob> Hobs { get; set; }
        }

        private ModelBuilder HobNobBuilder()
        {
            var builder = CreateModelBuilder(new Model());

            builder.Entity<Hob>().Key(e => new { e.Id1, e.Id2 });
            builder.Entity<Nob>().Key(e => new { e.Id1, e.Id2 });

            return builder;
        }

        [Fact]
        public void Generic_OneToMany_is_preserved_when_chaining_from_Annotation()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<OrderDetails>();

            AssertIsGenericOneToMany(modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .Annotation("X", "Y"));

            var entityType = modelBuilder.Model.GetEntityType(typeof(Order));
            Assert.Equal("Y", entityType.ForeignKeys.Single()["X"]);
        }

        [Fact]
        public void Generic_OneToMany_is_preserved_when_chaining_from_ForeignKey()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<OrderDetails>();

            AssertIsGenericOneToMany(modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .ForeignKey("AnotherCustomerId"));

            var entityType = modelBuilder.Model.GetEntityType(typeof(Order));
            Assert.Equal("AnotherCustomerId", entityType.ForeignKeys.Single().Properties.Single().Name);
        }

        [Fact]
        public void Generic_OneToMany_is_preserved_when_chaining_from_ReferencedKey()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<OrderDetails>();

            AssertIsGenericOneToMany(modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .ReferencedKey("AlternateKey"));

            var entityType = modelBuilder.Model.GetEntityType(typeof(Order));
            Assert.Equal("AlternateKey", entityType.ForeignKeys.Single().ReferencedProperties.Single().Name);
        }

        [Fact]
        public void Generic_OneToMany_is_preserved_when_chaining_from_Required()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<CustomerDetails>();
            modelBuilder.Ignore<OrderDetails>();

            AssertIsGenericOneToMany(modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .Required(false));

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Order));
            Assert.False(entityType.ForeignKeys.Single().IsRequired);
        }

        private static void AssertIsGenericOneToMany(OneToManyBuilder<Customer, Order> _)
        {
        }

        [Fact]
        public void Generic_ManyToOne_is_preserved_when_chaining_from_Annotation()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<OrderDetails>();

            AssertIsGenericManyToOne(modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .Annotation("X", "Y"));

            var entityType = modelBuilder.Model.GetEntityType(typeof(Order));
            Assert.Equal("Y", entityType.ForeignKeys.Single()["X"]);
        }

        [Fact]
        public void Generic_ManyToOne_is_preserved_when_chaining_from_ForeignKey()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<OrderDetails>();

            AssertIsGenericManyToOne(modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .ForeignKey("AnotherCustomerId"));

            var entityType = modelBuilder.Model.GetEntityType(typeof(Order));
            Assert.Equal("AnotherCustomerId", entityType.ForeignKeys.Single().Properties.Single().Name);
        }

        [Fact]
        public void Generic_ManyToOne_is_preserved_when_chaining_from_ReferencedKey()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<OrderDetails>();

            AssertIsGenericManyToOne(modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .ReferencedKey("AlternateKey"));

            var entityType = modelBuilder.Model.GetEntityType(typeof(Order));
            Assert.Equal("AlternateKey", entityType.ForeignKeys.Single().ReferencedProperties.Single().Name);
        }

        [Fact]
        public void Generic_ManyToOne_is_preserved_when_chaining_from_Required()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<OrderDetails>();

            AssertIsGenericManyToOne(modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .Required(false));

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Order));
            Assert.False(entityType.ForeignKeys.Single().IsRequired);
        }

        private static void AssertIsGenericManyToOne(ManyToOneBuilder<Order, Customer> _)
        {
        }

        [Fact]
        public void OneToOne_self_referencing_principal_and_dependent_can_be_flipped()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder
                .Entity<SelfRef>().HasOne(e => e.SelfRef1).WithOne(e => e.SelfRef2);

            var entityType = modelBuilder.Model.GetEntityType(typeof(SelfRef));
            var fk = entityType.ForeignKeys.Single();

            var navigationToPrincipal = fk.GetNavigationToPrincipal();
            var navigationToDependent = fk.GetNavigationToDependent();

            modelBuilder
                .Entity<SelfRef>().HasOne(e => e.SelfRef1).WithOne(e => e.SelfRef2);

            Assert.Same(fk, entityType.ForeignKeys.Single());
            Assert.Equal(navigationToDependent.Name, fk.GetNavigationToDependent().Name);
            Assert.Equal(navigationToPrincipal.Name, fk.GetNavigationToPrincipal().Name);
            Assert.True(((IForeignKey)fk).IsRequired);

            modelBuilder
                .Entity<SelfRef>().HasOne(e => e.SelfRef2).WithOne(e => e.SelfRef1);

            var newFk = entityType.ForeignKeys.Single();

            Assert.Equal(fk.Properties, newFk.Properties);
            Assert.Equal(fk.ReferencedKey, newFk.ReferencedKey);
            Assert.Equal(navigationToPrincipal.Name, newFk.GetNavigationToDependent().Name);
            Assert.Equal(navigationToDependent.Name, newFk.GetNavigationToPrincipal().Name);
            Assert.True(((IForeignKey)newFk).IsRequired);
        }

        [Fact]
        public void OneToOne_self_referencing_can_create_unidirectional_nav_to_principal()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<SelfRef>(eb =>
                {
                    eb.Key(e => e.Id);
                    eb.Property(e => e.SelfRefId);
                });

            var entityType = modelBuilder.Model.GetEntityType(typeof(SelfRef));
            var expectedProperties = entityType.Properties.ToList();

            modelBuilder.Entity<SelfRef>().HasOne(e => e.SelfRef1).WithOne();

            var fk = entityType.ForeignKeys.Single();
            var navigationToPrincipal = fk.GetNavigationToPrincipal();
            var navigationToDependent = fk.GetNavigationToDependent();

            Assert.NotEqual(fk.Properties, entityType.GetPrimaryKey().Properties);
            Assert.Equal(fk.ReferencedKey, entityType.GetPrimaryKey());
            Assert.Equal(null, navigationToDependent?.Name);
            Assert.Equal("SelfRef1", navigationToPrincipal?.Name);
            Assert.Same(navigationToPrincipal, entityType.Navigations.Single());
            AssertEqual(expectedProperties, entityType.Properties);
            Assert.True(((IForeignKey)fk).IsRequired);
        }

        [Fact]
        public void OneToOne_self_referencing_can_create_unidirectional_nav_to_dependent()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<SelfRef>(eb =>
                {
                    eb.Key(e => e.Id);
                    eb.Property(e => e.SelfRefId);
                });

            var entityType = modelBuilder.Model.GetEntityType(typeof(SelfRef));
            var expectedProperties = entityType.Properties.ToList();

            modelBuilder.Entity<SelfRef>().HasOne<SelfRef>().WithOne(e => e.SelfRef1);

            var fk = entityType.ForeignKeys.Single();
            var navigationToPrincipal = fk.GetNavigationToPrincipal();
            var navigationToDependent = fk.GetNavigationToDependent();

            Assert.NotEqual(fk.Properties, entityType.GetPrimaryKey().Properties);
            Assert.Equal(fk.ReferencedKey, entityType.GetPrimaryKey());
            Assert.Equal("SelfRef1", navigationToDependent?.Name);
            Assert.Equal(null, navigationToPrincipal?.Name);
            Assert.Same(navigationToDependent, entityType.Navigations.Single());
            AssertEqual(expectedProperties, entityType.Properties);
            Assert.True(((IForeignKey)fk).IsRequired);
        }

        [Fact]
        public void OneToOne_self_referencing_throws_on_duplicate_navigation()
        {
            var modelBuilder = CreateModelBuilder();

            Assert.Equal(Strings.NavigationToSelfDuplicate("SelfRef1"),
                Assert.Throws<InvalidOperationException>(() =>
                    modelBuilder.Entity<SelfRef>().HasOne(e => e.SelfRef1).WithOne(e => e.SelfRef1)).Message);
        }

        private class SelfRef
        {
            public int Id { get; set; }
            public SelfRef SelfRef1 { get; set; }
            public SelfRef SelfRef2 { get; set; }
            public int SelfRefId { get; set; }
        }

        private void AssertEqual(IReadOnlyList<Property> expectedProperties, IEnumerable<Property> actualProperties)
        {
            var expectedPropertyNamesSet = new SortedSet<string>(expectedProperties.Select(p => p.Name), StringComparer.Ordinal);
            Assert.Equal(expectedPropertyNamesSet, actualProperties.Select(p => p.Name));
        }

        private void AssertEqual(IReadOnlyList<IProperty> expectedProperties, IEnumerable<IProperty> actualProperties)
        {
            var expectedPropertyNamesSet = new SortedSet<string>(expectedProperties.Select(p => p.Name), StringComparer.Ordinal);
            Assert.Equal(expectedPropertyNamesSet, actualProperties.Select(p => p.Name));
        }

        protected virtual ModelBuilder CreateModelBuilder()
        {
            return CreateModelBuilder(new Model());
        }

        protected virtual ModelBuilder CreateModelBuilder(Model model)
        {
            return TestHelpers.Instance.CreateConventionBuilder(model);
        }
    }
}
