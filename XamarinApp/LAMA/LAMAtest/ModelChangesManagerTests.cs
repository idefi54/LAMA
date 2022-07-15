using LAMA;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LAMAtest
{
    [TestClass]
    public class ModelChangesManagerTests
    {
        private LAMA.Communicator.ModelChangesManager InitModelChangesManager()
        {
            RememberedStringDictionary<LAMA.Communicator.Command, LAMA.Communicator.CommandStorage> objectsCache = LAMA.DatabaseHolderStringDictionary<LAMA.Communicator.Command, LAMA.Communicator.CommandStorage>.Instance.rememberedDictionary;
            RememberedStringDictionary<LAMA.Communicator.TimeValue, LAMA.Communicator.TimeValueStorage> attributesCache = LAMA.DatabaseHolderStringDictionary<LAMA.Communicator.TimeValue, LAMA.Communicator.TimeValueStorage>.Instance.rememberedDictionary;
            LAMA.Communicator.ModelChangesManager manager = new LAMA.Communicator.ModelChangesManager(null, objectsCache, attributesCache);
            return manager;
        }
        [TestMethod]
        public void TestItemCreatedDeleted()
        {
            LAMA.Communicator.ModelChangesManager manager = InitModelChangesManager();
            LAMA.Models.LarpActivity activity = new LAMA.Models.LarpActivity(123, "testActivity", "tesDescription", "None", LAMA.Models.LarpActivity.EventType.normal, new LAMA.EventList<int>(),
            new Time(60), 1, new Time(120), new Pair<double, double>(10.0, 15.0), LAMA.Models.LarpActivity.Status.readyToLaunch, new LAMA.EventList < Pair<int, int> >(),
            new LAMA.EventList < Pair<string, int> >(), new EventList < Pair<int, string> >());
            string command = $"{DateTimeOffset.Now.ToUnixTimeSeconds()};ItemCreated;{activity.GetType().ToString()};{String.Join(",", activity.getAttributes())}";

            LAMA.Models.CP cp = new LAMA.Models.CP(234, "testCP", "testCP234", new LAMA.EventList<string>(), 123456789,
            "facebook", "discord", new Pair<double, double>(13.0, 15.6), "notes");
            string command2 = $"{DateTimeOffset.Now.ToUnixTimeSeconds()};ItemCreated;{cp.GetType().ToString()};{String.Join(",", cp.getAttributes())}";
            
            LAMA.Models.InventoryItem inventoryItem = new LAMA.Models.InventoryItem(345, "testItem", "test inventory item", 2, 3);
            string command3 = $"{DateTimeOffset.Now.ToUnixTimeSeconds()};ItemCreated;{inventoryItem.GetType().ToString()};{String.Join(",", inventoryItem.getAttributes())}";

            manager.ProcessCommand(command, null, true);
            manager.ProcessCommand(command2, null, true);
            manager.ProcessCommand(command3, null, true);

            LAMA.RememberedList<LAMA.Models.LarpActivity, LAMA.Models.LarpActivityStorage> larpActivityList =  LAMA.DatabaseHolder<LAMA.Models.LarpActivity, LAMA.Models.LarpActivityStorage>.Instance.rememberedList;
            LAMA.RememberedList<LAMA.Models.CP, LAMA.Models.CPStorage> cpList = LAMA.DatabaseHolder<LAMA.Models.CP, LAMA.Models.CPStorage>.Instance.rememberedList;
            LAMA.RememberedList<LAMA.Models.InventoryItem, LAMA.Models.InventoryItemStorage> inventoryItemList = LAMA.DatabaseHolder<LAMA.Models.InventoryItem, LAMA.Models.InventoryItemStorage>.Instance.rememberedList;
            LAMA.Models.LarpActivity activityStored = larpActivityList.getByID(123);
            LAMA.Models.CP cpStored = cpList.getByID(234);
            LAMA.Models.InventoryItem itemStored = inventoryItemList.getByID(345);

            Assert.AreEqual(activity.status, activityStored.status, "Activity Status Changed");
            Assert.AreEqual(activity.day, activityStored.day, "Activity Day Changed");
            Assert.AreEqual(activity.duration, activityStored.duration, "Activity Duration Changed");
            Assert.AreEqual(activity.description, activityStored.description, "Activity Description Changed");
            Assert.AreEqual(activity.eventType, activityStored.eventType, "Activity Event Type Changed");
            Assert.AreEqual(activity.ID, activityStored.ID, "Activity ID Changed");
            Assert.AreEqual(activity.name, activityStored.name, "Activity Name Changed");
            Assert.AreEqual(activity.place.first, activityStored.place.first, 0.001, "Activity Position Changed");
            Assert.AreEqual(activity.place.second, activityStored.place.second, 0.001, "Activity Position Changed");
            Assert.AreEqual(activity.preparationNeeded, activityStored.preparationNeeded, "Activity Preparation Changed");
            Assert.AreEqual(activity.start, activityStored.start, "Activity Start Changed");
            Assert.AreEqual(activity.prerequisiteIDs.Count, activityStored.prerequisiteIDs.Count, "Activity Prerequisites Changed");
            Assert.AreEqual(activity.registrationByRole.Count, activityStored.registrationByRole.Count, "Activity Registration By Role Changed");
            Assert.AreEqual(activity.requiredItems.Count, activityStored.requiredItems.Count, "Activity Prerequisites Changed");
            Assert.AreEqual(activity.roles.Count, activityStored.roles.Count, "Activity Roles Changed");

            Assert.AreEqual(cp.discord, cpStored.discord, "CP Discord Changed");
            Assert.AreEqual(cp.facebook, cpStored.facebook, "CP Facebook Changed");
            Assert.AreEqual(cp.ID, cpStored.ID, "CP ID Changed");
            Assert.AreEqual(cp.location.first, cpStored.location.first, 0.001, "CP Location Changed");
            Assert.AreEqual(cp.location.second, cpStored.location.second, 0.001, "CP Location Changed");
            Assert.AreEqual(cp.name, cpStored.name, "CP Name Changed");
            Assert.AreEqual(cp.nick, cpStored.nick, "CP Nick Changed");
            Assert.AreEqual(cp.notes, cpStored.notes, "CP Notes Changed");
            Assert.AreEqual(cp.phone, cpStored.phone, "CP Phone Changed");
            Assert.AreEqual(cp.roles.Count, cpStored.roles.Count, 0.001, "CP Roles Changed");

            Assert.AreEqual(inventoryItem.name, itemStored.name, "inventoryItem Name Changed");
            Assert.AreEqual(inventoryItem.description, itemStored.description, "inventoryItem Description Changed");
            Assert.AreEqual(inventoryItem.free, itemStored.free, "inventoryItem Free Amount Changed");
            Assert.AreEqual(inventoryItem.ID, itemStored.ID, "inventoryItem ID Changed");
            Assert.AreEqual(inventoryItem.taken, itemStored.taken, "inventoryItem Taken Amount Changed");

            string command4 = $"{DateTimeOffset.Now.ToUnixTimeSeconds()};ItemDeleted;{activity.GetType().ToString()};{activity.getID()}";
            string command5 = $"{DateTimeOffset.Now.ToUnixTimeSeconds()};ItemDeleted;{cp.GetType().ToString()};{cp.getID()}";
            string command6 = $"{DateTimeOffset.Now.ToUnixTimeSeconds()};ItemDeleted;{inventoryItem.GetType().ToString()};{inventoryItem.getID()}";

            manager.ProcessCommand(command4, null, true);
            manager.ProcessCommand(command5, null, true);
            manager.ProcessCommand(command6, null, true);

            activityStored = larpActivityList.getByID(activity.getID());
        }
    }
}