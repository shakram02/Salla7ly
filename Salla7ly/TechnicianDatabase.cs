using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;


namespace Salla7ly
{
    public class TechnicianDatabase
    {

        //Mobile Service sync table used to access data
        private readonly IMobileServiceSyncTable<Technician> _technicaianTable;

        private readonly MobileServiceSQLiteStore _store;

        //Mobile Service Client reference
        private readonly MobileServiceClient _client;
        private bool _isOnline;
        public TechnicianDatabase(MobileServiceClient client, bool isOnline)
        {
            _client = client;
            _isOnline = isOnline;

            // new code to initialize the SQLite store
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), LocalDbFilename);

            //// Start locally clean each time
#if (DEBUG)
            // TODO remove
            //if (File.Exists(path))
            //    File.Delete(path);
#endif

            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
            }

            _store = new MobileServiceSQLiteStore(path);
            _store.DefineTable<Technician>();
            _technicaianTable = _client.GetSyncTable<Technician>();
        }

        public async Task Initialize()
        {
            // Uses the default conflict handler, which fails on conflict To use a different conflict
            // handler, pass a parameter to InitializeAsync. For more details, see http://go.microsoft.com/fwlink/?LinkId=521416
            await _client.SyncContext.InitializeAsync(_store);

            if (!_isOnline) return;
            await _technicaianTable.PullAsync("technicianQuery", _technicaianTable.CreateQuery()); // query ID is used for incremental sync
        }

        /// <summary></summary>
        /// <exception cref="Java.Net.MalformedURLException"></exception>
        /// <exception cref="System.Exception"></exception>
        /// <param name="pullData"></param>
        /// <returns></returns>
        public async Task CommitChanges(bool pullData = false)
        {
            await _client.SyncContext.PushAsync(); // send changes to the mobile service

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
            return await _technicaianTable.CreateQuery().Take(20).ToListAsync();
        }

        public async Task AddTechnician(Technician technician)
        {
            await _technicaianTable.InsertAsync(technician); // insert the new item into the local database
        }

        public async Task<IList<Technician>> Find(Expression<Func<Technician, bool>> expression)
        {
            return await _technicaianTable.Where(expression).ToListAsync();
        }
        public async Task<IList<Technician>> Find(string field)
        {
            Expression<Func<Technician, bool>> queryExpression = t => t.Field.StartsWith(field);

            var results = await _technicaianTable.Where(queryExpression).ToListAsync();
            if (results.Count == 0)
            {
                try
                {
                    if (_isOnline)
                    { await _technicaianTable.PullAsync("pullTechByField", _technicaianTable.Where(t => t.Field.StartsWith(field))); }
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