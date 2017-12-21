﻿using FutureState.Data.KeyBinders;
using FutureState.Data.Keys;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace FutureState.Data.Tests
{
    public class MemoryResidentRepositoryTests
    {
        private InMemoryRepository<TestEntity, int> subject;
        private int allItemsCount;
        private TestEntity insertedItem;
        int idCurrent = 0;

        internal void GivenAnInMemoryDb()
        {
            var entityKeyBinder = new ExpressionKeyBinder<TestEntity, int>(
                e => e.Id,
                (e, k) => e.Id = k);

            var keyGenerator = new KeyGenerator<TestEntity, int>(() => ++idCurrent);

            var entityIdProvider = new EntityIdProvider<TestEntity, int>(
                keyGenerator,
                entityKeyBinder);

            var list = new List<TestEntity>()
            {
                new TestEntity() {Id = 0, Name="Name"}
            };
            
            subject = new InMemoryRepository<TestEntity, int>(
                entityIdProvider, entityKeyBinder, list);
        }

        internal void WhenQueringAllItems()
        {
            allItemsCount = subject.GetAll().Count();
        }

        internal void AndWhenAddingNewItems()
        {
            subject.Insert(new TestEntity() { Name = "Name2" });

            this.insertedItem = subject.Where(m => m.Name == "Name2").FirstOrDefault();
        }

        internal void ThenShouldBeAbleToQueryAllItems()
        {
            Assert.Equal(1, allItemsCount);
        }

        internal void AndThenInsertedItemsShouldHaveIdAssigned()
        {
            Assert.Equal(1, insertedItem.Id);
        }

        [BddfyFact]
        public void CanGetAndSetDataIntoRepository()
        {
            this.BDDfy();
        }

        public class TestEntity
        {
            [Key]
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
