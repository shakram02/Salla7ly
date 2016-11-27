using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;

namespace AppLogic
{
    public class TechnicianDatabase
    {
        //Mobile Service sync table used to access data
        private IMobileServiceSyncTable<Technician> _technicaianTable;

        private readonly MobileServiceSQLiteStore _store;

        //Mobile Service Client reference
        public MobileServiceClient Client { get; }

        private const string ApplicationUrl = @"https://salla7ly.azurewebsites.net";

        public TechnicianDatabase()
        {
            Client = new MobileServiceClient(ApplicationUrl);
            _store = new MobileServiceSQLiteStore(LocalDbFilename);
            _store.DefineTable<Technician>();
            Client.SyncContext.InitializeAsync(_store);
            _technicaianTable = Client.GetSyncTable<Technician>();

        }

        /// <summary></summary>
        /// <exception cref="Java.Net.MalformedURLException"></exception>
        /// <exception cref="System.Exception"></exception>
        /// <param name="pullData"></param>
        /// <returns></returns>
        public async Task CommitChanges(bool pullData = false)
        {
            await Client.SyncContext.PushAsync(); // send changes to the mobile service

            if (pullData)
            {
                // Notice, on low android versions, this line produces an error that's related to
                // SQLite, for more details:
                // https://disqus.com/home/discussion/thewindowsazureproductsite/get_started_with_offline_data_in_mobile_services_xamarin_android_mobile_dev_center_33/ https://social.msdn.microsoft.com/Forums/azure/en-US/1b4dc9a8-cc5c-4401-bc99-ad3e43e280e8/offline-sync-on-xamarin-minimum-android-version?forum=azuremobile
                await _technicaianTable.PullAsync("technicianQuery", _technicaianTable.CreateQuery()); // query ID is used for incremental sync
            }
        }

        //#if DEBUG
        //        private Technician tech;
        //        /// <summary>Adds dummy data to the database</summary>
        //        public async Task SeedData(int number, ArrayAdapter adapter)
        //        {
        //            for (int j = 0; j < adapter.Count - 1; j++)
        //            {
        //                for (int i = 0; i < number; i++)
        //                {
        //                    tech = new Technician()
        //                    {
        //                        Name = $"dummy {i}",
        //                        City = $"Dummy city {i}",
        //                        Field = $"{adapter.GetItem(j)}",
        //                        Governorate = $"{i} Alex",
        //                        PhoneNumber = $"01111111111",
        //                    };

        //                    await AddTechnician(tech);
        //                    await CommitChanges();
        //                }
        //            }
        //        }
        //#endif

        public async Task<IEnumerable<Technician>> GetAllTechnicians(int take = 20)
        {
            //return await _technicaianTable.CreateQuery().Take(20).ToListAsync();
            await _technicaianTable.PullAsync("technicianQuery", _technicaianTable.CreateQuery()); // query ID is used for incremental sync
            return await _technicaianTable.ReadAsync();
        }

        public async Task AddTechnician(Technician technician)
        {
            await _technicaianTable.InsertAsync(technician); // insert the new item into the local database
        }

        public async Task StartDb()
        {
            await _technicaianTable.PullAsync("technicianQuery", _technicaianTable.CreateQuery()); // query ID is used for incremental sync
        }

        public async Task<IList<Technician>> Find(string field)
        {
            Expression<Func<Technician, bool>> queryExpression = t => t.Field.StartsWith(field);

            var results = await _technicaianTable.Where(queryExpression).ToListAsync();
            if (results.Count == 0)
            {
                try
                {
                    await _technicaianTable.PullAsync("pullTechByField", _technicaianTable.Where(t => t.Field.StartsWith(field)));
                }
                catch (Exception exc)
                {
                    string err = exc.Message;
                }

                return await _technicaianTable.Where(queryExpression).ToListAsync();
            }

            return results;
        }

        private const string LocalDbFilename = "localstore.db";
    }
}