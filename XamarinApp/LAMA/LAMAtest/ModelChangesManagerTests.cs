﻿using LAMA;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using LAMA.Extensions;
using LAMA.Communicator;

namespace LAMAtest
{
    [TestClass]
    public class ModelChangesManagerTests
    {
        private LAMA.Communicator.ModelChangesManager InitModelChangesManager()
        {
            RememberedStringDictionary<LAMA.Communicator.Command, LAMA.Communicator.CommandStorage> objectsCache = LAMA.DatabaseHolderStringDictionary<LAMA.Communicator.Command, LAMA.Communicator.CommandStorage>.Instance.rememberedDictionary;
            RememberedStringDictionary<LAMA.Communicator.ModelPropertyChangeInfo, LAMA.Communicator.ModelPropertyChangeInfoStorage> attributesCache = LAMA.DatabaseHolderStringDictionary<LAMA.Communicator.ModelPropertyChangeInfo, LAMA.Communicator.ModelPropertyChangeInfoStorage>.Instance.rememberedDictionary;
            LAMA.Communicator.ModelChangesManager manager = new LAMA.Communicator.ModelChangesManager(null, objectsCache, attributesCache, false, true);
            return manager;
        }
        [TestMethod]
        public void TestItemCreated()
        {
            LAMA.Communicator.ModelChangesManager manager = InitModelChangesManager();
            LAMA.Models.LarpActivity activity = new LAMA.Models.LarpActivity(123, "testActivity", "testDescription", "None", LAMA.Models.LarpActivity.EventType.normal, new LAMA.EventList<long>(),
            60, 1, 120, new Pair<double, double>(10.1, 15.2), LAMA.Models.LarpActivity.Status.readyToLaunch, new LAMA.EventList < Pair<long, int> >(),
            new LAMA.EventList < Pair<string, int> >(), new EventList < Pair<long, string> >());
            string command = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}ItemCreated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{String.Join(SpecialCharacters.attributesSeparator.ToString(), activity.getAttributes())}";

            LAMA.Models.CP cp = new LAMA.Models.CP(234, "testCP", "testCP234", new LAMA.EventList<string>(), "123456789",
            "facebook", "discord", "notes");
            string command2 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}ItemCreated{SpecialCharacters.messagePartSeparator}{cp.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{String.Join(SpecialCharacters.attributesSeparator.ToString(), cp.getAttributes())}";
            
            LAMA.Models.InventoryItem inventoryItem = new LAMA.Models.InventoryItem(345, "testItem", "test inventory item", 2, 3);
            string command3 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}ItemCreated{SpecialCharacters.messagePartSeparator}{inventoryItem.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{String.Join(SpecialCharacters.attributesSeparator.ToString(), inventoryItem.getAttributes())}";
            Debug.WriteLine(command);

            manager.ProcessCommand(command, null);
            manager.ProcessCommand(command2, null);
            manager.ProcessCommand(command3, null);

            LAMA.RememberedList<LAMA.Models.LarpActivity, LAMA.Models.LarpActivityStorage> larpActivityList =  LAMA.DatabaseHolder<LAMA.Models.LarpActivity, LAMA.Models.LarpActivityStorage>.Instance.rememberedList;
            LAMA.RememberedList<LAMA.Models.CP, LAMA.Models.CPStorage> cpList = LAMA.DatabaseHolder<LAMA.Models.CP, LAMA.Models.CPStorage>.Instance.rememberedList;
            LAMA.RememberedList<LAMA.Models.InventoryItem, LAMA.Models.InventoryItemStorage> inventoryItemList = LAMA.DatabaseHolder<LAMA.Models.InventoryItem, LAMA.Models.InventoryItemStorage>.Instance.rememberedList;
            LAMA.Models.LarpActivity activityStored = larpActivityList.getByID(123);
            LAMA.Models.CP cpStored = cpList.getByID(234);
            LAMA.Models.InventoryItem itemStored = inventoryItemList.getByID(345);

            Debug.WriteLine(activity);
            Debug.WriteLine("ActivityStored =" + activityStored);

            Assert.AreEqual(activity.status, activityStored.status, "Activity Status Changed");
            Assert.AreEqual(activity.day, activityStored.day, "Activity Day Changed");
            Assert.IsTrue(activity.duration == activityStored.duration, "Activity Duration Changed");
            Assert.AreEqual(activity.description, activityStored.description, "Activity Description Changed");
            Assert.AreEqual(activity.eventType, activityStored.eventType, "Activity Event Type Changed");
            Assert.AreEqual(activity.ID, activityStored.ID, "Activity ID Changed");
            Assert.AreEqual(activity.name, activityStored.name, "Activity Name Changed");
            Assert.AreEqual(activity.place.first, activityStored.place.first, 0.001, "Activity Position Changed");
            Assert.AreEqual(activity.place.second, activityStored.place.second, 0.001, "Activity Position Changed");
            Assert.AreEqual(activity.preparationNeeded, activityStored.preparationNeeded, "Activity Preparation Changed");
            Assert.IsTrue(activity.start==activityStored.start, "Activity Start Changed");
            //Assert.AreEqual(activity.prerequisiteIDs.Count, activityStored.prerequisiteIDs.Count, "Activity Prerequisites Changed");
            //Assert.AreEqual(activity.registrationByRole.Count, activityStored.registrationByRole.Count, "Activity Registration By Role Changed");
            //Assert.AreEqual(activity.requiredItems.Count, activityStored.requiredItems.Count, "Activity Prerequisites Changed");
            //Assert.AreEqual(activity.roles.Count, activityStored.roles.Count, "Activity Roles Changed");

            Assert.AreEqual(cp.discord, cpStored.discord, "CP Discord Changed");
            Assert.AreEqual(cp.facebook, cpStored.facebook, "CP Facebook Changed");
            Assert.AreEqual(cp.ID, cpStored.ID, "CP ID Changed");
            Assert.AreEqual(cp.location.first, cpStored.location.first, 0.001, "CP Location Changed");
            Assert.AreEqual(cp.location.second, cpStored.location.second, 0.001, "CP Location Changed");
            Assert.AreEqual(cp.name, cpStored.name, "CP Name Changed");
            Assert.AreEqual(cp.nick, cpStored.nick, "CP Nick Changed");
            Assert.AreEqual(cp.notes, cpStored.notes, "CP Notes Changed");
            Assert.AreEqual(cp.phone, cpStored.phone, "CP Phone Changed");
            //Assert.AreEqual(cp.roles.Count, cpStored.roles.Count, 0.001, "CP Roles Changed");

            Assert.AreEqual(inventoryItem.name, itemStored.name, "inventoryItem Name Changed");
            Assert.AreEqual(inventoryItem.description, itemStored.description, "inventoryItem Description Changed");
            Assert.AreEqual(inventoryItem.free, itemStored.free, "inventoryItem Free Amount Changed");
            Assert.AreEqual(inventoryItem.ID, itemStored.ID, "inventoryItem ID Changed");
            Assert.AreEqual(inventoryItem.taken, itemStored.taken, "inventoryItem Taken Amount Changed");

            string command4 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}ItemDeleted{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity.getID()}";
            string command5 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}ItemDeleted{SpecialCharacters.messagePartSeparator}{cp.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{cp.getID()}";
            string command6 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}ItemDeleted{SpecialCharacters.messagePartSeparator}{inventoryItem.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{inventoryItem.getID()}";

            manager.ProcessCommand(command4, null);
            manager.ProcessCommand(command5, null);
            manager.ProcessCommand(command6, null);
        }


        [TestMethod]
        public void TestItemDeleted()
        {
            LAMA.Communicator.ModelChangesManager manager = InitModelChangesManager();
            LAMA.Models.LarpActivity activity = new LAMA.Models.LarpActivity(124, "testActivity", "testDescription", "None", LAMA.Models.LarpActivity.EventType.normal, new LAMA.EventList<long>(),
            60, 1, 120, new Pair<double, double>(10.1, 15.2), LAMA.Models.LarpActivity.Status.readyToLaunch, new LAMA.EventList<Pair<long, int>>(),
            new LAMA.EventList<Pair<string, int>>(), new EventList<Pair<long, string>>());
            string command = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}ItemCreated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{String.Join(SpecialCharacters.attributesSeparator.ToString(), activity.getAttributes())}";

            LAMA.Models.CP cp = new LAMA.Models.CP(235, "testCP", "testCP234", new LAMA.EventList<string>(), 123456789.ToString(),
            "facebook", "discord", "notes");
            string command2 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}ItemCreated{SpecialCharacters.messagePartSeparator}{cp.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{String.Join(SpecialCharacters.attributesSeparator.ToString(), cp.getAttributes())}";

            LAMA.Models.InventoryItem inventoryItem = new LAMA.Models.InventoryItem(346, "testItem", "test inventory item", 2, 3);
            string command3 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}ItemCreated{SpecialCharacters.messagePartSeparator}{inventoryItem.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{String.Join(SpecialCharacters.attributesSeparator.ToString(), inventoryItem.getAttributes())}";
            Debug.WriteLine(command);

            manager.ProcessCommand(command, null);
            manager.ProcessCommand(command2, null);
            manager.ProcessCommand(command3, null);

            LAMA.RememberedList<LAMA.Models.LarpActivity, LAMA.Models.LarpActivityStorage> larpActivityList = LAMA.DatabaseHolder<LAMA.Models.LarpActivity, LAMA.Models.LarpActivityStorage>.Instance.rememberedList;
            LAMA.RememberedList<LAMA.Models.CP, LAMA.Models.CPStorage> cpList = LAMA.DatabaseHolder<LAMA.Models.CP, LAMA.Models.CPStorage>.Instance.rememberedList;
            LAMA.RememberedList<LAMA.Models.InventoryItem, LAMA.Models.InventoryItemStorage> inventoryItemList = LAMA.DatabaseHolder<LAMA.Models.InventoryItem, LAMA.Models.InventoryItemStorage>.Instance.rememberedList;

            string command4 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}ItemDeleted{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity.getID()}";
            string command5 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}ItemDeleted{SpecialCharacters.messagePartSeparator}{cp.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{cp.getID()}";
            string command6 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}ItemDeleted{SpecialCharacters.messagePartSeparator}{inventoryItem.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{inventoryItem.getID()}";

            manager.ProcessCommand(command4, null);
            manager.ProcessCommand(command5, null);
            manager.ProcessCommand(command6, null);

            LAMA.Models.LarpActivity activityStored = larpActivityList.getByID(124);
            LAMA.Models.CP cpStored = cpList.getByID(235);
            LAMA.Models.InventoryItem itemStored = inventoryItemList.getByID(346);
            Assert.AreEqual(activityStored, null, "LARP activity wasn't deleted");
            Assert.AreEqual(cpStored, null, "CP wasn't deleted");
            Assert.AreEqual(itemStored, null, "Inventory item wasn't deleted");
        }

        [TestMethod]
        public void TestDataUpdated()
        {
            LAMA.Communicator.ModelChangesManager manager = InitModelChangesManager();
            LAMA.Models.LarpActivity activity = new LAMA.Models.LarpActivity(125, "testActivity", "testDescription", "None", LAMA.Models.LarpActivity.EventType.normal, new LAMA.EventList<long>(),
            60, 1, 120, new Pair<double, double>(10.1, 15.2), LAMA.Models.LarpActivity.Status.readyToLaunch, new LAMA.EventList<Pair<long, int>>(),
            new LAMA.EventList<Pair<string, int>>(), new EventList<Pair<long, string>>());
            string command = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}ItemCreated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{String.Join(SpecialCharacters.attributesSeparator.ToString(), activity.getAttributes())}";

            LAMA.Models.CP cp = new LAMA.Models.CP(236, "testCP", "testCP234", new LAMA.EventList<string>(), 123456789.ToString(),
            "facebook", "discord", "notes");
            string command2 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}ItemCreated{SpecialCharacters.messagePartSeparator}{cp.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{String.Join(SpecialCharacters.attributesSeparator.ToString(), cp.getAttributes())}";

            LAMA.Models.InventoryItem inventoryItem = new LAMA.Models.InventoryItem(347, "testItem", "test inventory item", 2, 3);
            string command3 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}ItemCreated{SpecialCharacters.messagePartSeparator}{inventoryItem.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{String.Join(SpecialCharacters.attributesSeparator.ToString(), inventoryItem.getAttributes())}";

            manager.ProcessCommand(command, null);
            manager.ProcessCommand(command2, null);
            manager.ProcessCommand(command3, null);

            string command4 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity.ID}{SpecialCharacters.messagePartSeparator}{1}{SpecialCharacters.messagePartSeparator}{"testActivityChanged"}";
            string command5 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity.ID}{SpecialCharacters.messagePartSeparator}{2}{SpecialCharacters.messagePartSeparator}{"testDescriptionChanged"}";
            string command6 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity.ID}{SpecialCharacters.messagePartSeparator}{3}{SpecialCharacters.messagePartSeparator}{"Some"}";
            string command7 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity.ID}{SpecialCharacters.messagePartSeparator}{4}{SpecialCharacters.messagePartSeparator}{LAMA.Models.LarpActivity.EventType.preparation}";
            string command8 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity.ID}{SpecialCharacters.messagePartSeparator}{6}{SpecialCharacters.messagePartSeparator}{61}";
            string command9 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity.ID}{SpecialCharacters.messagePartSeparator}{7}{SpecialCharacters.messagePartSeparator}{2}";
            string command10 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity.ID}{SpecialCharacters.messagePartSeparator}{8}{SpecialCharacters.messagePartSeparator}{121}";
            string command11 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity.ID}{SpecialCharacters.messagePartSeparator}{9}{SpecialCharacters.messagePartSeparator}{new Pair<double, double>(10.2, 15.4)}";
            string command12 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity.ID}{SpecialCharacters.messagePartSeparator}{10}{SpecialCharacters.messagePartSeparator}{LAMA.Models.LarpActivity.Status.awaitingPrerequisites}";

            string command13 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{cp.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{cp.ID}{SpecialCharacters.messagePartSeparator}{1}{SpecialCharacters.messagePartSeparator}{"testCPChanged"}";
            string command14 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{cp.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{cp.ID}{SpecialCharacters.messagePartSeparator}{2}{SpecialCharacters.messagePartSeparator}{"testCP234Changed"}";
            string command15 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{cp.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{cp.ID}{SpecialCharacters.messagePartSeparator}{4}{SpecialCharacters.messagePartSeparator}234567891";
            string command16 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{cp.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{cp.ID}{SpecialCharacters.messagePartSeparator}{5}{SpecialCharacters.messagePartSeparator}{"facebookChanged"}";
            string command17 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{cp.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{cp.ID}{SpecialCharacters.messagePartSeparator}{6}{SpecialCharacters.messagePartSeparator}{"discordChanged"}";
            string command18 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{cp.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{cp.ID}{SpecialCharacters.messagePartSeparator}{7}{SpecialCharacters.messagePartSeparator}{new Pair<double, double>(13.02, 15.7)}";
            string command19 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{cp.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{cp.ID}{SpecialCharacters.messagePartSeparator}{8}{SpecialCharacters.messagePartSeparator}{"notesChanged"}";

            string command20 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{inventoryItem.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{inventoryItem.ID}{SpecialCharacters.messagePartSeparator}{1}{SpecialCharacters.messagePartSeparator}{"testItemChanged"}";
            string command21 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{inventoryItem.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{inventoryItem.ID}{SpecialCharacters.messagePartSeparator}{2}{SpecialCharacters.messagePartSeparator}{"test inventory item2"}";
            string command22 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{inventoryItem.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{inventoryItem.ID}{SpecialCharacters.messagePartSeparator}{3}{SpecialCharacters.messagePartSeparator}{3}";
            string command23 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{inventoryItem.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{inventoryItem.ID}{SpecialCharacters.messagePartSeparator}{4}{SpecialCharacters.messagePartSeparator}{4}";

            manager.ProcessCommand(command4, null);
            manager.ProcessCommand(command5, null);
            manager.ProcessCommand(command6, null);
            manager.ProcessCommand(command7, null);
            manager.ProcessCommand(command8, null);
            manager.ProcessCommand(command9, null);
            manager.ProcessCommand(command10, null);
            manager.ProcessCommand(command11, null);
            manager.ProcessCommand(command12, null);
            manager.ProcessCommand(command13, null);
            manager.ProcessCommand(command14, null);
            manager.ProcessCommand(command15, null);
            manager.ProcessCommand(command16, null);
            manager.ProcessCommand(command17, null);
            manager.ProcessCommand(command18, null);
            manager.ProcessCommand(command19, null);
            manager.ProcessCommand(command20, null);
            manager.ProcessCommand(command21, null);
            manager.ProcessCommand(command22, null);
            manager.ProcessCommand(command23, null);

            LAMA.RememberedList<LAMA.Models.LarpActivity, LAMA.Models.LarpActivityStorage> larpActivityList = LAMA.DatabaseHolder<LAMA.Models.LarpActivity, LAMA.Models.LarpActivityStorage>.Instance.rememberedList;
            LAMA.RememberedList<LAMA.Models.CP, LAMA.Models.CPStorage> cpList = LAMA.DatabaseHolder<LAMA.Models.CP, LAMA.Models.CPStorage>.Instance.rememberedList;
            LAMA.RememberedList<LAMA.Models.InventoryItem, LAMA.Models.InventoryItemStorage> inventoryItemList = LAMA.DatabaseHolder<LAMA.Models.InventoryItem, LAMA.Models.InventoryItemStorage>.Instance.rememberedList;

            LAMA.Models.LarpActivity activityStored = larpActivityList.getByID(125);
            LAMA.Models.CP cpStored = cpList.getByID(236);
            LAMA.Models.InventoryItem itemStored = inventoryItemList.getByID(347);
            

            Assert.AreEqual(LAMA.Models.LarpActivity.Status.awaitingPrerequisites, activityStored.status, "Activity Status wasn't updated");
            Assert.AreEqual(2, activityStored.day, "Activity Day wasn't updated");
            Assert.IsTrue(61 == activityStored.duration, "Activity Duration wasn't updated");
            Assert.AreEqual("testDescriptionChanged", activityStored.description, "Activity Description wasn't updated");
            Assert.AreEqual(LAMA.Models.LarpActivity.EventType.preparation, activityStored.eventType, "Activity Event Type wasn't updated");
            Assert.AreEqual("testActivityChanged", activityStored.name, "Activity Name wasn't updated");
            Assert.AreEqual(10.2, activityStored.place.first, 0.001, "Activity Position wasn't updated");
            Assert.AreEqual(15.4, activityStored.place.second, 0.001, "Activity Position wasn't updated");
            Assert.AreEqual("Some", activityStored.preparationNeeded, "Activity Preparation wasn't updated");
            Assert.IsTrue(121 == activityStored.start, "Activity Start wasn't updated");

            Assert.AreEqual("discordChanged", cpStored.discord, "CP Discord wasn't updated");
            Assert.AreEqual("facebookChanged", cpStored.facebook, "CP Facebook wasn't updated");
            Assert.AreEqual(13.02, cpStored.location.first, 0.001, "CP Location wasn't updated");
            Assert.AreEqual(15.7, cpStored.location.second, 0.001, "CP Location wasn't updated");
            Assert.AreEqual("testCPChanged", cpStored.name, "CP Name wasn't updated");
            Assert.AreEqual("testCP234Changed", cpStored.nick, "CP Nick wasn't updated");
            Assert.AreEqual("notesChanged", cpStored.notes, "CP Notes wasn't updated");
            Assert.AreEqual("234567891", cpStored.phone, "CP Phone wasn't updated");

            Assert.AreEqual("testItemChanged", itemStored.name, "inventoryItem Name wasn't updated");
            Assert.AreEqual("test inventory item2", itemStored.description, "inventoryItem Description wasn't updated");
            Assert.AreEqual(4, itemStored.free, "inventoryItem Free Amount wasn't updated");
            Assert.AreEqual(3, itemStored.taken, "inventoryItem Taken Amount wasn't updated");

            string command24 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}ItemDeleted{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity.getID()}";
            string command25 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}ItemDeleted{SpecialCharacters.messagePartSeparator}{cp.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{cp.getID()}";
            string command26 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}ItemDeleted{SpecialCharacters.messagePartSeparator}{inventoryItem.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{inventoryItem.getID()}";

            manager.ProcessCommand(command24, null);
            manager.ProcessCommand(command25, null);
            manager.ProcessCommand(command26, null);
        }

        [TestMethod]
        public void TestRollbackDataUpdated()
        {
            LAMA.Communicator.ModelChangesManager manager = InitModelChangesManager();
            LAMA.Models.LarpActivity activity = new LAMA.Models.LarpActivity(126, "testActivity", "testDescription", "None", LAMA.Models.LarpActivity.EventType.normal, new LAMA.EventList<long>(),
            60, 1, 120, new Pair<double, double>(10.1, 15.2), LAMA.Models.LarpActivity.Status.readyToLaunch, new LAMA.EventList<Pair<long, int>>(),
            new LAMA.EventList<Pair<string, int>>(), new EventList<Pair<long, string>>());
            string command = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}ItemCreated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{String.Join(SpecialCharacters.attributesSeparator.ToString(), activity.getAttributes())}";

            LAMA.Models.CP cp = new LAMA.Models.CP(237, "testCP", "testCP234", new LAMA.EventList<string>(), 123456789.ToString(),
            "facebook", "discord", "notes");
            string command2 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}ItemCreated{SpecialCharacters.messagePartSeparator}{cp.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{String.Join(SpecialCharacters.attributesSeparator.ToString(), cp.getAttributes())}";

            LAMA.Models.InventoryItem inventoryItem = new LAMA.Models.InventoryItem(348, "testItem", "test inventory item", 2, 3);
            string command3 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}ItemCreated{SpecialCharacters.messagePartSeparator}{inventoryItem.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{String.Join(SpecialCharacters.attributesSeparator.ToString(), inventoryItem.getAttributes())}";

            manager.ProcessCommand(command, null);
            manager.ProcessCommand(command2, null);
            manager.ProcessCommand(command3, null);

            string command4 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity.ID}{SpecialCharacters.messagePartSeparator}{1}{SpecialCharacters.messagePartSeparator}{"testActivityChanged"}";
            string command5 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity.ID}{SpecialCharacters.messagePartSeparator}{2}{SpecialCharacters.messagePartSeparator}{"testDescriptionChanged"}";
            string command6 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity.ID}{SpecialCharacters.messagePartSeparator}{3}{SpecialCharacters.messagePartSeparator}{"Some"}";
            string command7 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity.ID}{SpecialCharacters.messagePartSeparator}{4}{SpecialCharacters.messagePartSeparator}{LAMA.Models.LarpActivity.EventType.preparation}";
            string command8 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity.ID}{SpecialCharacters.messagePartSeparator}{6}{SpecialCharacters.messagePartSeparator}{61}";
            string command9 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity.ID}{SpecialCharacters.messagePartSeparator}{7}{SpecialCharacters.messagePartSeparator}{2}";
            string command10 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity.ID}{SpecialCharacters.messagePartSeparator}{8}{SpecialCharacters.messagePartSeparator}{121}";
            string command11 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity.ID}{SpecialCharacters.messagePartSeparator}{9}{SpecialCharacters.messagePartSeparator}{new Pair<double, double>(10.2, 15.4)}";
            string command12 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity.ID}{SpecialCharacters.messagePartSeparator}{10}{SpecialCharacters.messagePartSeparator}{LAMA.Models.LarpActivity.Status.awaitingPrerequisites}";

            string command13 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{cp.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{cp.ID}{SpecialCharacters.messagePartSeparator}{1}{SpecialCharacters.messagePartSeparator}{"testCPChanged"}";
            string command14 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{cp.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{cp.ID}{SpecialCharacters.messagePartSeparator}{2}{SpecialCharacters.messagePartSeparator}{"testCP234Changed"}";
            string command15 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{cp.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{cp.ID}{SpecialCharacters.messagePartSeparator}{4}{SpecialCharacters.messagePartSeparator}234567891";
            string command16 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{cp.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{cp.ID}{SpecialCharacters.messagePartSeparator}{5}{SpecialCharacters.messagePartSeparator}{"facebookChanged"}";
            string command17 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{cp.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{cp.ID}{SpecialCharacters.messagePartSeparator}{6}{SpecialCharacters.messagePartSeparator}{"discordChanged"}";
            string command18 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{cp.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{cp.ID}{SpecialCharacters.messagePartSeparator}{7}{SpecialCharacters.messagePartSeparator}{new Pair<double, double>(13.02, 15.7)}";
            string command19 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{cp.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{cp.ID}{SpecialCharacters.messagePartSeparator}{8}{SpecialCharacters.messagePartSeparator}{"notesChanged"}";

            string command20 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{inventoryItem.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{inventoryItem.ID}{SpecialCharacters.messagePartSeparator}{1}{SpecialCharacters.messagePartSeparator}{"testItemChanged"}";
            string command21 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{inventoryItem.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{inventoryItem.ID}{SpecialCharacters.messagePartSeparator}{2}{SpecialCharacters.messagePartSeparator}{"test inventory item2"}";
            string command22 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{inventoryItem.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{inventoryItem.ID}{SpecialCharacters.messagePartSeparator}{3}{SpecialCharacters.messagePartSeparator}{3}";
            string command23 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{inventoryItem.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{inventoryItem.ID}{SpecialCharacters.messagePartSeparator}{4}{SpecialCharacters.messagePartSeparator}{4}";

            manager.ProcessCommand(command4, null);
            manager.ProcessCommand(command5, null);
            manager.ProcessCommand(command6, null);
            manager.ProcessCommand(command7, null);
            manager.ProcessCommand(command8, null);
            manager.ProcessCommand(command9, null);
            manager.ProcessCommand(command10, null);
            manager.ProcessCommand(command11, null);
            manager.ProcessCommand(command12, null);
            manager.ProcessCommand(command13, null);
            manager.ProcessCommand(command14, null);
            manager.ProcessCommand(command15, null);
            manager.ProcessCommand(command16, null);
            manager.ProcessCommand(command17, null);
            manager.ProcessCommand(command18, null);
            manager.ProcessCommand(command19, null);
            manager.ProcessCommand(command20, null);
            manager.ProcessCommand(command21, null);
            manager.ProcessCommand(command22, null);
            manager.ProcessCommand(command23, null);

            LAMA.RememberedList<LAMA.Models.LarpActivity, LAMA.Models.LarpActivityStorage> larpActivityList = LAMA.DatabaseHolder<LAMA.Models.LarpActivity, LAMA.Models.LarpActivityStorage>.Instance.rememberedList;
            LAMA.RememberedList<LAMA.Models.CP, LAMA.Models.CPStorage> cpList = LAMA.DatabaseHolder<LAMA.Models.CP, LAMA.Models.CPStorage>.Instance.rememberedList;
            LAMA.RememberedList<LAMA.Models.InventoryItem, LAMA.Models.InventoryItemStorage> inventoryItemList = LAMA.DatabaseHolder<LAMA.Models.InventoryItem, LAMA.Models.InventoryItemStorage>.Instance.rememberedList;

            string command24 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}Rollback{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity.ID}{SpecialCharacters.messagePartSeparator}{1}{SpecialCharacters.messagePartSeparator}{"testActivity"}";
            string command25 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}Rollback{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity.ID}{SpecialCharacters.messagePartSeparator}{2}{SpecialCharacters.messagePartSeparator}{"testDescription"}";
            string command26 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}Rollback{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity.ID}{SpecialCharacters.messagePartSeparator}{3}{SpecialCharacters.messagePartSeparator}{"None"}";
            string command27 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}Rollback{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity.ID}{SpecialCharacters.messagePartSeparator}{4}{SpecialCharacters.messagePartSeparator}{LAMA.Models.LarpActivity.EventType.normal}";
            string command28 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}Rollback{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity.ID}{SpecialCharacters.messagePartSeparator}{6}{SpecialCharacters.messagePartSeparator}{60}";
            string command29 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}Rollback{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity.ID}{SpecialCharacters.messagePartSeparator}{7}{SpecialCharacters.messagePartSeparator}{1}";
            string command30 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}Rollback{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity.ID}{SpecialCharacters.messagePartSeparator}{8}{SpecialCharacters.messagePartSeparator}{120}";
            string command31 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}Rollback{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity.ID}{SpecialCharacters.messagePartSeparator}{9}{SpecialCharacters.messagePartSeparator}{new Pair<double, double>(10.1, 15.2)}";
            string command32 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}Rollback{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity.ID}{SpecialCharacters.messagePartSeparator}{10}{SpecialCharacters.messagePartSeparator}{LAMA.Models.LarpActivity.Status.readyToLaunch}";

            string command33 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}Rollback{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{cp.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{cp.ID}{SpecialCharacters.messagePartSeparator}{1}{SpecialCharacters.messagePartSeparator}{"testCP"}";
            string command34 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}Rollback{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{cp.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{cp.ID}{SpecialCharacters.messagePartSeparator}{2}{SpecialCharacters.messagePartSeparator}{"testCP234"}";
            string command35 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}Rollback{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{cp.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{cp.ID}{SpecialCharacters.messagePartSeparator}{4}{SpecialCharacters.messagePartSeparator}123456789";
            string command36 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}Rollback{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{cp.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{cp.ID}{SpecialCharacters.messagePartSeparator}{5}{SpecialCharacters.messagePartSeparator}{"facebook"}";
            string command37 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}Rollback{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{cp.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{cp.ID}{SpecialCharacters.messagePartSeparator}{6}{SpecialCharacters.messagePartSeparator}{"discord"}";
            string command38 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}Rollback{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{cp.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{cp.ID}{SpecialCharacters.messagePartSeparator}{7}{SpecialCharacters.messagePartSeparator}{new Pair<double, double>(0.0, 0.0)}";
            string command39 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}Rollback{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{cp.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{cp.ID}{SpecialCharacters.messagePartSeparator}{8}{SpecialCharacters.messagePartSeparator}{"notes"}";

            string command40 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}Rollback{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{inventoryItem.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{inventoryItem.ID}{SpecialCharacters.messagePartSeparator}{1}{SpecialCharacters.messagePartSeparator}{"testItem"}";
            string command41 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}Rollback{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{inventoryItem.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{inventoryItem.ID}{SpecialCharacters.messagePartSeparator}{2}{SpecialCharacters.messagePartSeparator}{"test inventory item"}";
            string command42 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}Rollback{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{inventoryItem.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{inventoryItem.ID}{SpecialCharacters.messagePartSeparator}{3}{SpecialCharacters.messagePartSeparator}{2}";
            string command43 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}Rollback{SpecialCharacters.messagePartSeparator}DataUpdated{SpecialCharacters.messagePartSeparator}{inventoryItem.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{inventoryItem.ID}{SpecialCharacters.messagePartSeparator}{4}{SpecialCharacters.messagePartSeparator}{3}";

            manager.ProcessCommand(command24, null);
            manager.ProcessCommand(command25, null);
            manager.ProcessCommand(command26, null);
            manager.ProcessCommand(command27, null);
            manager.ProcessCommand(command28, null);
            manager.ProcessCommand(command29, null);
            manager.ProcessCommand(command30, null);
            manager.ProcessCommand(command31, null);
            manager.ProcessCommand(command32, null);
            manager.ProcessCommand(command33, null);
            manager.ProcessCommand(command34, null);
            manager.ProcessCommand(command35, null);
            manager.ProcessCommand(command36, null);
            manager.ProcessCommand(command37, null);
            manager.ProcessCommand(command38, null);
            manager.ProcessCommand(command39, null);
            manager.ProcessCommand(command40, null);
            manager.ProcessCommand(command41, null);
            manager.ProcessCommand(command42, null);
            manager.ProcessCommand(command43, null);

            LAMA.Models.LarpActivity activityStored = larpActivityList.getByID(126);
            LAMA.Models.CP cpStored = cpList.getByID(237);
            LAMA.Models.InventoryItem itemStored = inventoryItemList.getByID(348);

            Assert.AreEqual(activity.status, activityStored.status, "Activity Status not reverted");
            Assert.AreEqual(activity.day, activityStored.day, "Activity Day not reverted");
            Assert.IsTrue(activity.duration == activityStored.duration, "Activity Duration not reverted");
            Assert.AreEqual(activity.description, activityStored.description, "Activity Description not reverted");
            Assert.AreEqual(activity.eventType, activityStored.eventType, "Activity Event Type not reverted");
            Assert.AreEqual(activity.ID, activityStored.ID, "Activity ID not reverted");
            Assert.AreEqual(activity.name, activityStored.name, "Activity Name not reverted");
            Assert.AreEqual(activity.place.first, activityStored.place.first, 0.001, "Activity Position not reverted");
            Assert.AreEqual(activity.place.second, activityStored.place.second, 0.001, "Activity Position not reverted");
            Assert.AreEqual(activity.preparationNeeded, activityStored.preparationNeeded, "Activity Preparation not reverted");
            Assert.IsTrue(activity.start == activityStored.start, "Activity Start not reverted");
            //Assert.AreEqual(activity.prerequisiteIDs.Count, activityStored.prerequisiteIDs.Count, "Activity Prerequisites Changed");
            //Assert.AreEqual(activity.registrationByRole.Count, activityStored.registrationByRole.Count, "Activity Registration By Role Changed");
            //Assert.AreEqual(activity.requiredItems.Count, activityStored.requiredItems.Count, "Activity Prerequisites Changed");
            //Assert.AreEqual(activity.roles.Count, activityStored.roles.Count, "Activity Roles Changed");

            Assert.AreEqual(cp.discord, cpStored.discord, "CP Discord not reverted");
            Assert.AreEqual(cp.facebook, cpStored.facebook, "CP Facebook not reverted");
            Assert.AreEqual(cp.ID, cpStored.ID, "CP ID not reverted");
            Assert.AreEqual(cp.location.first, cpStored.location.first, 0.001, "CP Location not reverted");
            Assert.AreEqual(cp.location.second, cpStored.location.second, 0.001, "CP Location not reverted");
            Assert.AreEqual(cp.name, cpStored.name, "CP Name not reverted");
            Assert.AreEqual(cp.nick, cpStored.nick, "CP Nick not reverted");
            Assert.AreEqual(cp.notes, cpStored.notes, "CP Notes not reverted");
            Assert.AreEqual(cp.phone, cpStored.phone, "CP Phone not reverted");
            //Assert.AreEqual(cp.roles.Count, cpStored.roles.Count, 0.001, "CP Roles Changed");

            Assert.AreEqual(inventoryItem.name, itemStored.name, "inventoryItem Name not reverted");
            Assert.AreEqual(inventoryItem.description, itemStored.description, "inventoryItem Description not reverted");
            Assert.AreEqual(inventoryItem.free, itemStored.free, "inventoryItem Free Amount not reverted");
            Assert.AreEqual(inventoryItem.ID, itemStored.ID, "inventoryItem ID not reverted");
            Assert.AreEqual(inventoryItem.taken, itemStored.taken, "inventoryItem Taken Amount not reverted");

            string command44 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}ItemDeleted{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity.getID()}";
            string command45 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}ItemDeleted{SpecialCharacters.messagePartSeparator}{cp.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{cp.getID()}";
            string command46 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}ItemDeleted{SpecialCharacters.messagePartSeparator}{inventoryItem.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{inventoryItem.getID()}";

            manager.ProcessCommand(command24, null);
            manager.ProcessCommand(command25, null);
            manager.ProcessCommand(command26, null);
        }

        [TestMethod]
        public void TestRollbackItemCreated()
        {
            LAMA.Communicator.ModelChangesManager manager = InitModelChangesManager();
            LAMA.Models.LarpActivity activity = new LAMA.Models.LarpActivity(127, "testActivity", "testDescription", "None", LAMA.Models.LarpActivity.EventType.normal, new LAMA.EventList<long>(),
            60, 1, 120, new Pair<double, double>(10.1, 15.2), LAMA.Models.LarpActivity.Status.readyToLaunch, new LAMA.EventList<Pair<long, int>>(),
            new LAMA.EventList<Pair<string, int>>(), new EventList<Pair<long, string>>());
            string command = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}ItemCreated{SpecialCharacters.messagePartSeparator}{activity.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{String.Join(SpecialCharacters.attributesSeparator.ToString(), activity.getAttributes())}";

            LAMA.Models.CP cp = new LAMA.Models.CP(238, "testCP", "testCP234", new LAMA.EventList<string>(), 123456789.ToString(),
            "facebook", "discord", "notes");
            string command2 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}ItemCreated{SpecialCharacters.messagePartSeparator}{cp.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{String.Join(SpecialCharacters.attributesSeparator.ToString(), cp.getAttributes())}";

            LAMA.Models.InventoryItem inventoryItem = new LAMA.Models.InventoryItem(349, "testItem", "test inventory item", 2, 3);
            string command3 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}ItemCreated{SpecialCharacters.messagePartSeparator}{inventoryItem.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{String.Join(SpecialCharacters.attributesSeparator.ToString(), inventoryItem.getAttributes())}";


            LAMA.Models.LarpActivity activity2 = new LAMA.Models.LarpActivity(127, "testActivity2", "testDescription2", "None2", LAMA.Models.LarpActivity.EventType.preparation, new LAMA.EventList<long>(),
            61, 2, 121, new Pair<double, double>(10.2, 15.3), LAMA.Models.LarpActivity.Status.awaitingPrerequisites, new LAMA.EventList<Pair<long, int>>(),
            new LAMA.EventList<Pair<string, int>>(), new EventList<Pair<long, string>>());
            string command4 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}Rollback{SpecialCharacters.messagePartSeparator}ItemCreated{SpecialCharacters.messagePartSeparator}{activity2.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{String.Join(SpecialCharacters.attributesSeparator.ToString(), activity2.getAttributes())}";

            LAMA.Models.CP cp2 = new LAMA.Models.CP(238, "testCP2", "testCP2342", new LAMA.EventList<string>(), 234567891.ToString(),
            "facebook2", "discord2", "notes2");
            string command5 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}Rollback{SpecialCharacters.messagePartSeparator}ItemCreated{SpecialCharacters.messagePartSeparator}{cp2.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{String.Join(SpecialCharacters.attributesSeparator.ToString(), cp2.getAttributes())}";

            LAMA.Models.InventoryItem inventoryItem2 = new LAMA.Models.InventoryItem(349, "testItem2", "test inventory item2", 3, 4);
            string command6 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}Rollback{SpecialCharacters.messagePartSeparator}ItemCreated{SpecialCharacters.messagePartSeparator}{inventoryItem2.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{String.Join(SpecialCharacters.attributesSeparator.ToString(), inventoryItem2.getAttributes())}";

            manager.ProcessCommand(command, null);
            manager.ProcessCommand(command2, null);
            manager.ProcessCommand(command3, null);

            manager.ProcessCommand(command4, null);
            manager.ProcessCommand(command5, null);
            manager.ProcessCommand(command6, null);


            LAMA.RememberedList<LAMA.Models.LarpActivity, LAMA.Models.LarpActivityStorage> larpActivityList = LAMA.DatabaseHolder<LAMA.Models.LarpActivity, LAMA.Models.LarpActivityStorage>.Instance.rememberedList;
            LAMA.RememberedList<LAMA.Models.CP, LAMA.Models.CPStorage> cpList = LAMA.DatabaseHolder<LAMA.Models.CP, LAMA.Models.CPStorage>.Instance.rememberedList;
            LAMA.RememberedList<LAMA.Models.InventoryItem, LAMA.Models.InventoryItemStorage> inventoryItemList = LAMA.DatabaseHolder<LAMA.Models.InventoryItem, LAMA.Models.InventoryItemStorage>.Instance.rememberedList;



            LAMA.Models.LarpActivity activityStored = larpActivityList.getByID(127);
            LAMA.Models.CP cpStored = cpList.getByID(238);
            LAMA.Models.InventoryItem itemStored = inventoryItemList.getByID(349);

            Assert.AreEqual(activity2.status, activityStored.status, "Activity Status not reverted");
            Assert.AreEqual(activity2.day, activityStored.day, "Activity Day not reverted");
            Assert.IsTrue(activity2.duration == activityStored.duration, "Activity Duration not reverted");
            Assert.AreEqual(activity2.description, activityStored.description, "Activity Description not reverted");
            Assert.AreEqual(activity2.eventType, activityStored.eventType, "Activity Event Type not reverted");
            Assert.AreEqual(activity2.ID, activityStored.ID, "Activity ID not reverted");
            Assert.AreEqual(activity2.name, activityStored.name, "Activity Name not reverted");
            Assert.AreEqual(activity2.place.first, activityStored.place.first, 0.001, "Activity Position not reverted");
            Assert.AreEqual(activity2.place.second, activityStored.place.second, 0.001, "Activity Position not reverted");
            Assert.AreEqual(activity2.preparationNeeded, activityStored.preparationNeeded, "Activity Preparation not reverted");
            Assert.IsTrue(activity2.start == activityStored.start, "Activity Start not reverted");
            //Assert.AreEqual(activity.prerequisiteIDs.Count, activityStored.prerequisiteIDs.Count, "Activity Prerequisites Changed");
            //Assert.AreEqual(activity.registrationByRole.Count, activityStored.registrationByRole.Count, "Activity Registration By Role Changed");
            //Assert.AreEqual(activity.requiredItems.Count, activityStored.requiredItems.Count, "Activity Prerequisites Changed");
            //Assert.AreEqual(activity.roles.Count, activityStored.roles.Count, "Activity Roles Changed");

            Assert.AreEqual(cp2.discord, cpStored.discord, "CP Discord not reverted");
            Assert.AreEqual(cp2.facebook, cpStored.facebook, "CP Facebook not reverted");
            Assert.AreEqual(cp2.ID, cpStored.ID, "CP ID not reverted");
            Assert.AreEqual(cp2.location.first, cpStored.location.first, 0.001, "CP Location not reverted");
            Assert.AreEqual(cp2.location.second, cpStored.location.second, 0.001, "CP Location not reverted");
            Assert.AreEqual(cp2.name, cpStored.name, "CP Name not reverted");
            Assert.AreEqual(cp2.nick, cpStored.nick, "CP Nick not reverted");
            Assert.AreEqual(cp2.notes, cpStored.notes, "CP Notes not reverted");
            Assert.AreEqual(cp2.phone, cpStored.phone, "CP Phone not reverted");
            //Assert.AreEqual(cp.roles.Count, cpStored.roles.Count, 0.001, "CP Roles Changed");

            Assert.AreEqual(inventoryItem2.name, itemStored.name, "inventoryItem Name not reverted");
            Assert.AreEqual(inventoryItem2.description, itemStored.description, "inventoryItem Description not reverted");
            Assert.AreEqual(inventoryItem2.free, itemStored.free, "inventoryItem Free Amount not reverted");
            Assert.AreEqual(inventoryItem2.ID, itemStored.ID, "inventoryItem ID not reverted");
            Assert.AreEqual(inventoryItem2.taken, itemStored.taken, "inventoryItem Taken Amount not reverted");

            string command7 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}ItemDeleted{SpecialCharacters.messagePartSeparator}{activity2.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{activity2.getID()}";
            string command8 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}ItemDeleted{SpecialCharacters.messagePartSeparator}{cp2.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{cp2.getID()}";
            string command9 = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{SpecialCharacters.messagePartSeparator}ItemDeleted{SpecialCharacters.messagePartSeparator}{inventoryItem2.GetType().ToString()}{SpecialCharacters.messagePartSeparator}{inventoryItem2.getID()}";

            manager.ProcessCommand(command7, null);
            manager.ProcessCommand(command8, null);
            manager.ProcessCommand(command9, null);
        }
    }
}