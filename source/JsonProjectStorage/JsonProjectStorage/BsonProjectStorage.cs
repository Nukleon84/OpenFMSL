using Caliburn.Micro;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using OpenFMSL.Contracts.Entities;
using OpenFMSL.Contracts.Infrastructure.Messaging;
using OpenFMSL.Contracts.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonProjectStorage
{
    public class BsonProjectStorage : IProjectStorage, IHandle<StoreEntityInRepositoryMessage>, IHandle<OpenRepositoryMessage>
    {
        private readonly IEventAggregator _aggregator;
        public BsonProjectStorage(IEventAggregator aggregator)
        {
            _aggregator = aggregator;
            _aggregator.Subscribe(this);
        }

        public void Handle(StoreEntityInRepositoryMessage message)
        {
            StoreProjectToFile(message.Filename, message.Data);
        }

        void StoreProjectToFile(string filename, Entity root)
        {
            FileStream ms = new FileStream(filename, FileMode.OpenOrCreate);
            using (var writer = new BsonDataWriter(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                serializer.TypeNameHandling = TypeNameHandling.All;
                serializer.ObjectCreationHandling = ObjectCreationHandling.Replace;
                serializer.Serialize(writer, root);
            }
        }

        void RestoreProjectFromFile(string filename)
        {
            Entity deserialized = null;
            try
            {

                FileStream ms = new FileStream(filename, FileMode.Open);
                using (BsonDataReader reader = new BsonDataReader(ms))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                    serializer.TypeNameHandling = TypeNameHandling.All;
                    serializer.ObjectCreationHandling = ObjectCreationHandling.Replace;
                    deserialized = serializer.Deserialize<Entity>(reader);
                }
            }
            catch (Exception e)
            {

                var text = System.IO.File.ReadAllText(filename);
                deserialized = JsonConvert.DeserializeObject<Entity>(text, new JsonSerializerSettings()
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    TypeNameHandling = TypeNameHandling.All,
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                }
                    );
            }

            if (deserialized != null)
            {
                // Root = deserialized;
                // _entityManager.RestoreHierarchy();
                _aggregator.PublishOnUIThread(new RestoreProjectFromRepositoryMessage() { RestoredData = deserialized });
            }
        }
        public void Handle(OpenRepositoryMessage message)
        {
            if (System.IO.File.Exists(message.Filename))
            {
                RestoreProjectFromFile(message.Filename);
            }
            else
            {
                _aggregator.PublishOnUIThread(new LogMessage { TimeStamp = DateTime.Now, Sender = this, Channel = LogChannels.Error, MessageText = "File " + message.Filename + " not found" });
            }
        }
    }

}
